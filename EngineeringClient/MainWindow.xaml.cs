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
using Newtonsoft.Json;
using EngineeringClient.Forms;


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

		//для защиты от data race (ObservableCollection)
		private object locker2 = new();
		//коллекция с автообновлением (для работы с таблицами и полученными данными с pgSql)
		private ObservableCollection<PlaneFaultData> faultCollection = new ObservableCollection<PlaneFaultData>();


		public MainWindow()
		{
			InitializeComponent();
			FaultsDataGrid.IsReadOnly = true;
		}
		private void Window_Loaded(object sender, RoutedEventArgs e)
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
		private void AboutAuthorMenuItem_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show($"Программу разработал студент группы РС 5{Environment.NewLine}Корнильев Максим Михайлович.", "Об авторе", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void AboutProgramMenuItem_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show($"Программа позволяет облегчить взаимодействие между службами ТОиР и авиакомпаниями.", "О программе", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private async void ConnectionMenuItem_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (!server.Connected)
				{
					await server.ConnectAsync(ip, port);
					ServeServer(server);
					await SendMessageAsync(server, "ReadAll");
				}
			}
			catch (Exception ex)
			{
				await logger.LogToFileAsync(locker, ex.Message);
				MessageBox.Show(ex.Message);
			}
		}

		//auto Window_Closing() will be here
		private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
		private void ExcelCreationMenuItem_Click(object sender, RoutedEventArgs e)
		{

		}
		private void AddDataMenuItem_Click(object sender, RoutedEventArgs e)
		{
			if (server.Connected)
			{
				OperationForm form = new OperationForm(server, "Добавление происшествия", null);
				form.Show();
			}
		}

		private void ChangeDataMenuItem_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (server.Connected)
				{
					PlaneFaultData fault = (PlaneFaultData)FaultsDataGrid.SelectedItem;
					if (fault is not null)
					{
						if (MessageBox.Show($"Вы уверены, что хотите изменить это событие?{Environment.NewLine}Id: {fault.Id}", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
						{
							OperationForm form = new OperationForm(server, $"Изменение данных происшествия   id: {fault.Id}", fault);
							form.Show();
						}
						else
							MessageBox.Show("Изменение отменено!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Information);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private async void DeleteByIdMenuitem_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (server.Connected)
				{
					PlaneFaultData fault = (PlaneFaultData)FaultsDataGrid.SelectedItem;
					//если да
					if (MessageBox.Show($"Вы уверены, что хотите удалить это событие?{Environment.NewLine}Id: {fault.Id}", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
					{
						await SendMessageAsync(server, $"RemById|{fault.Id}");
					}
					else
						MessageBox.Show("Удаление отменено!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Information);
				}
			}
			catch (Exception) { }
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
						case string message when receivedMessage.Contains("ReadedAll|"):
							{
								message = receivedMessage.Replace("ReadedAll|", "");
								var lst = JsonConvert.DeserializeObject<ObservableCollection<PlaneFaultData>>(message);
								if (lst is null) throw new ArgumentNullException(nameof(lst));
								lock (locker2)
								{
									faultCollection = lst;
									this.FaultsDataGrid.ItemsSource = faultCollection;
								}
							}
							break;
						case string message when receivedMessage == "AddSuccess":
							{
								await SendMessageAsync(server, "ReadAll");
							}
							break;
						case string message when receivedMessage == "Updated":
							{
								await SendMessageAsync(server, "ReadAll");
							}
							break;
						case string message when receivedMessage == "NotUpdated":
							{
								MessageBox.Show("Ошибка при изменении. Возможно такого id не существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
							}
							break;
						case string message when receivedMessage == "Removed":
							{
								await SendMessageAsync(server, "ReadAll");
							}
							break;
						case string message when receivedMessage == "NotRemoved":
							{
								MessageBox.Show($"Неудачное удаление!{Environment.NewLine}Возможно такого id не существует!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning);
							}
							break;
						case string message when receivedMessage == "Disconnect":
							{
								MessageBox.Show("Сервер разорвал соединение!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning);
								//закрываем соединение
								server.Close();
							}
							break;
						default:
							{
								MessageBox.Show($"Получена неизвестная команда от сервера по адресу: {server.Client.RemoteEndPoint}", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning);
								await logger.LogToFileAsync(locker, $"Получена неизвестная команда от сервера по адресу: {server.Client.RemoteEndPoint}");
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

//ReadAll - обновление таблицы