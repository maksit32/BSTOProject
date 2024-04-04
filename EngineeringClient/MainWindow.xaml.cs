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




//Приложение службы ТОиР
namespace EngineeringClient
{
	public partial class MainWindow : Window
	{
		private readonly IPAddress ip = IPAddress.Parse("192.168.0.13");
		private readonly int port = 9999;
		private TcpClient server = new TcpClient();
		private object locker = new();
		private static readonly string logPath = Environment.CurrentDirectory + "\\..\\..\\..\\Logs\\errorLog.txt";
		private readonly ILogger logger = new TxtLogger(logPath);
		//коллекция с автообновлением (для работы с таблицами и полученными данными с pgSql)
		private ObservableCollection<PlaneFaultData> faultCollection = new ObservableCollection<PlaneFaultData>();

		public MainWindow()
		{
			InitializeComponent();
		}
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			//удаление старого лога
			logger.RemoveLogFile();

		}
		private async void ConnectButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if(!server.Connected)
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