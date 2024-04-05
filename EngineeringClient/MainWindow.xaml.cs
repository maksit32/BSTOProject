using System.Net.Sockets;
using System.Net;
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
using LogLibrary.Classes;
using LogLibrary.Interfaces;
using System.Collections.ObjectModel;
using Entities;


using static TCPLibrary.TCPLibrary;


//Приложение службы ТОиР
namespace EngineeringClient
{
	public partial class MainWindow : Window
	{
		#region [Connections]
		private readonly IPAddress ip = IPAddress.Parse("192.168.0.13");
		private readonly int port = 9999;
		private TcpClient server = new TcpClient();
		#endregion

		#region [Loggers]
		private object locker = new();
		private static readonly string logPath = Environment.CurrentDirectory + "\\..\\..\\..\\Logs\\errorLog.txt";
		private readonly ILogger logger = new TxtLogger(logPath);
		#endregion

		//коллекция с автообновлением (для работы с таблицами и полученными данными с pgSql)
		private ObservableCollection<PlaneFaultData> faultCollection = new ObservableCollection<PlaneFaultData>();


		public MainWindow()
		{
			InitializeComponent();
		}
		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			//удаление старого лога
			logger.RemoveLogFile();
		}

		private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				if (server.Connected)
				{
					await SendMessageAsync(server, "Disconnect");
				}
			}
			catch (Exception ex)
			{
				await logger.LogToFileAsync(locker, ex.Message);
				MessageBox.Show(ex.Message);
			}
		}

		//отправляемые команды на сервер
		#region [UIMethods]
		private async void ConnectButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (!server.Connected)
				{
					await server.ConnectAsync(ip, port);
					ServeServer(server);
				}
			}
			catch (Exception ex)
			{
				await logger.LogToFileAsync(locker, ex.Message);
				MessageBox.Show(ex.Message);
			}
		}

		#endregion

		//команды от сервера
		#region [ServeServer]
		public async Task ServeServer(TcpClient server)
		{
			if (server is null)
			{
				throw new ArgumentNullException(nameof(server));
			}

			while (server.Connected)
			{
				try
				{
					string receivedMessage = await ReceiveMessageAsync(server);

					switch (receivedMessage)
					{
						case string message when receivedMessage == "Disconnect":
							{
								MessageBox.Show("Сервер разорвал соединение!");
								//закрываем соединение
								server.Close();
							}
							break;
						default:
							{

							}
							break;
					}
				}
				catch (Exception ex)
				{
					await logger.LogToFileAsync(locker, ex.Message);
					MessageBox.Show(ex.Message);
				}
			}
		}
		#endregion
	}
}