using FaultsLibrary;
using LogLibrary.Classes;
using LogLibrary.Interfaces;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


using static FaultsLibrary.FaultList;
using static TCPLibrary.TCPLibrary;


//модель БСТО
namespace AirplaneBSTOModel
{
	public partial class MainWindow : Window
	{
		private readonly IPAddress ip = IPAddress.Parse("192.168.0.13");
		private readonly int port = 9998;
		private TcpClient server = new TcpClient();
		private object locker = new();
		private static readonly string logPath = Environment.CurrentDirectory + "\\..\\..\\..\\Logs\\errorLog.txt";
		private readonly ILogger logger = new TxtLogger(logPath);

		public MainWindow()
		{
			InitializeComponent();
			this.IdentificatorTextBox.TextAlignment = TextAlignment.Center;
		}


		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				//удаляем старый лог
				logger.RemoveLogFile();

				var identificator = await RandomizeIdentificator();
				this.IdentificatorTextBox.Text = identificator;
				await server.ConnectAsync(ip, port);
			}
			catch (Exception ex)
			{
				await logger.LogToFileAsync(locker, ex.Message);
				MessageBox.Show(ex.Message);
			}
		}

		private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				if (server.Connected) await SendMessageAsync(server, "Disconnect");
			}
			catch (Exception ex)
			{
				await logger.LogToFileAsync(locker, ex.Message);
				MessageBox.Show(ex.Message);
			}
		}
		private async Task<string> RandomizeIdentificator()
		{
			string identificator = "";
			string symbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			string numbers = "0123456789";

			Random r = new Random();
			await Task.Run(() =>
			{
				//SYL
				for (int i = 0; i < 3; i++)
				{
					int rand = r.Next(symbols.Length);
					identificator += symbols[rand];
				}
				//198
				for (int i = 0; i < 3; i++)
				{
					int rand = r.Next(numbers.Length);
					identificator += numbers[rand];
				}
			});
			//SYL198
			return identificator;
		}

		//генерация отказа, передача на сервер
		private async void GenerateFaultButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Random r = new Random();
				Fault fault = faultsList[r.Next(faultsList.Count)];

				this.ErrorCodeLabel.Content = "Код ошибки: " + fault.FaultCode.ToString();
				this.FaultLabel.Content = "Отказ: " + fault.FaultMessage;

				if (server.Connected)
					await SendMessageAsync(server, $"TrM|{fault.FaultCode}|{fault.FaultMessage}|{IdentificatorTextBox.Text}|{FromTextBox.Text}|{ToTextBox.Text}");
			}
			catch (Exception ex)
			{
				await logger.LogToFileAsync(locker, ex.Message);
				MessageBox.Show(ex.Message);
			}
		}
		//замена идентификатора воздушного судна
		private async void RandomizePlaneButton_Click(object sender, RoutedEventArgs e)
		{
			this.IdentificatorTextBox.Text = await RandomizeIdentificator();
		}

		private async void ConnectButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (!server.Connected)
				{
					await server.ConnectAsync(ip, port);
				}
			}
			catch (Exception ex)
			{
				await logger.LogToFileAsync(locker, ex.Message);
				MessageBox.Show(ex.Message);
			}
		}
	}
}