using BSTOServer.Databases;
using LogLibrary.Classes;
using LogLibrary.Interfaces;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Entities;



using static JSONLibrary.JSONLibrary;
using static TCPLibrary.TCPLibrary;


//сервер для приема сообщений от воздушного судна(ВС)
//передача в СУБД (PgSQL)
//передача в органы ТО (клиент)




namespace BSTOServer
{
	public class Server
	{
		#region [FilePaths]
		private static readonly string logFilePath = Environment.CurrentDirectory + "\\..\\..\\..\\Logs\\errorLog.txt";
		private static readonly string appSettingsPath = Environment.CurrentDirectory + "\\..\\..\\..\\Settings\\appsettings.json";
		#endregion


		#region [Objects]
		private static object locker = new();
		//логгирование ошибок
		private static ILogger logger = new TxtLogger(logFilePath);


		//для приложения службы ТОиР
		private static readonly IPAddress ip = IPAddress.Parse("192.168.0.13");
		private static readonly int portToir = 9999;
		private static TcpClient toirClient;
		//для получения данных от БСТО ВС
		private static readonly int portPlane = 9998;
		private static TcpClient planeClient;
		#endregion



		public static async Task Main()
		{
			try
			{
				//удаление старого лога при перезапуске сервера
				logger.RemoveLogFile();
				//ожидаем подключения БСТО ВС
				planeClient = await WaitIncomingConnectionAsync(new TcpListener(ip, portPlane));
				ServeAirplaneBstoAsync(planeClient);
				//ожидаем поключение клиента (ТОиР)
				toirClient = await WaitIncomingConnectionAsync(new TcpListener(ip, portToir));
				ServeEngineeringClientAsync(toirClient);
				await Console.Out.WriteLineAsync("Для выключения сервера нажмите на любую клавишу");
				Console.ReadLine();
			}
			catch (Exception ex)
			{
				await logger.LogToFileAsync(locker, ex.Message);
			}
		}
		#region [EngineeringCommunication]
		private static async Task ServeEngineeringClientAsync(TcpClient client)
		{
			await Console.Out.WriteLineAsync($"Подключение со службой ТОиР установлено. Время:  {DateTime.UtcNow}");
			await SendMessageAsync(client, "ConnectedToServer");
			try
			{
				while (client.Connected)
				{
					//развертка базы (Scoped lifetime)
					var config = new ConfigurationBuilder()
					.AddJsonFile(appSettingsPath, optional: false, reloadOnChange: true)
					.Build();
					using PostgreSqlDb postgreeDb = new PostgreSqlDb(connectionString: config.GetSection("PostgreeSqlConnectionString").Value ?? throw new NullReferenceException());

					string receivedMessage = await ReceiveMessageAsync(client);

					switch (receivedMessage)
					{
						case string message when receivedMessage.Contains("AddJSON|"):
							{
								if (client.Connected)
								{
									message = receivedMessage.Replace("AddJSON|", "");
									PlaneFaultData newFaultData = DeserializeClassObject(message);
									await postgreeDb.AddPlaneFault(newFaultData);
									await SendMessageAsync(client, "AddSuccess");
								}
							}
							break;
						case string message when receivedMessage.Contains("ChangeJSON|"):
							{
								if (client.Connected)
								{
									message = receivedMessage.Replace("UpdateJSON|", "");
									PlaneFaultData updFaultData = DeserializeClassObject(message);
									var res = await postgreeDb.UpdatePlaneFaultAsync(updFaultData);
									if (res) await SendMessageAsync(client, "Updated");
									else await SendMessageAsync(client, "NotUpdated");
								}
							}
							break;
						case string message when receivedMessage.Contains("ReadById|"):
							{
								if (client.Connected)
								{
									message = receivedMessage.Replace("ReadById|", "");
									PlaneFaultData fault = await postgreeDb.ReadPlaneFaultByIdAsync(Guid.Parse(message));
									if (fault is null) break;
									//сериализация данных
									string serializedStrObj = SerializeClassObject(fault);
									await SendMessageAsync (client, "Readed|" + serializedStrObj);
								}
							}
							break;
						case string message when receivedMessage.Contains("RemById|"):
							{
								if (client.Connected)
								{
									message = receivedMessage.Replace("RemById|", "");
									var res = await postgreeDb.RemoveByGuidPlaneFaultAsync(Guid.Parse(message));
									if(res) await SendMessageAsync(client, "Removed");
									else await SendMessageAsync(client, "NotRemoved");
								}
							}
							break;
						case string message when receivedMessage.Contains("Disconnect"):
							{
								if (client.Connected)
								{
									Console.ForegroundColor = ConsoleColor.Green;
									var addr = client.Client.RemoteEndPoint;
									await SendMessageAsync(client, "DisconnectSuccess");
									client.Close(); //auto dispose()
									await Console.Out.WriteLineAsync($"Клиент службы ТОиР отключен от сервера. Адрес: {addr}");
									return;
								}
							}
							break;
						default:
							{
								if (client.Connected)
								{
									Console.ForegroundColor = ConsoleColor.Red;
									await Console.Out.WriteLineAsync("Получена неизвестная команда");
									await logger.LogToFileAsync(locker, $"Получена неизвестная команда от клиента ТОиР по адресу: {client.Client.RemoteEndPoint}");
									await SendMessageAsync(client, "ErrorCommand");
								}
							}
							break;
					}
				}
			}
			catch (Exception ex)
			{
				await logger.LogToFileAsync(locker, ex.Message);
			}
		}
		#endregion

		#region [BSTOCommunication]
		//получение данных от БСТО, обработка со списком ошибок, формирование данных для отправки в БД
		private static async Task ServeAirplaneBstoAsync(TcpClient client)
		{
			await Console.Out.WriteLineAsync($"Подключение с БСТО ВС установлено. Время:  {DateTime.UtcNow}");
			try
			{
				while (client.Connected)
				{
					bool correctData = false;
					//развертка базы (Scoped lifetime)
					var config = new ConfigurationBuilder()
					.AddJsonFile(appSettingsPath, optional: false, reloadOnChange: true)
					.Build();
					using PostgreSqlDb postgreeDb = new PostgreSqlDb(connectionString: config.GetSection("PostgreeSqlConnectionString").Value ?? throw new NullReferenceException());


					string receivedMessage = await ReceiveMessageAsync(client);

					switch (receivedMessage)
					{
						//обработка полученного значения
						case string message when receivedMessage.Contains("TrM|"):
							{
								if (client.Connected)
								{
									message = receivedMessage.Replace("TrM|", "");

									//0 - error code
									//1 - fault message
									//2 - plane identificator
									//3 - from (place)
									//4 - to (place)
									var dataArr = message.Split("|");

									//сверка правильности кода ошибки и текста ошибки
									foreach (var fault in FaultsLibrary.FaultList.faultsList)
									{
										if (fault.FaultCode == int.Parse(dataArr[0]) && fault.FaultMessage == dataArr[1])
										{
											correctData = true;
											break;
										}
									}

									//если данные корректны, то запись в БД
									if (correctData)
									{
										await postgreeDb.AddPlaneFault(new PlaneFaultData(int.Parse(dataArr[0]), dataArr[1], dataArr[2], dataArr[3], dataArr[4], DateTime.UtcNow));
									}
								}
							}
							break;
						case string message when receivedMessage.Contains("Disconnect"):
							{
								client.Close();
							}
							break;
						default:
							{
								if (client.Connected)
								{
									Console.ForegroundColor = ConsoleColor.Red;
									await Console.Out.WriteLineAsync("Получена неизвестная данные");
									await logger.LogToFileAsync(locker, $"Получена неизвестные данные от БСТО");
								}
							}
							break;
					}
				}
			}
			catch (Exception ex)
			{
				await logger.LogToFileAsync(locker, ex.Message);
			}
			#endregion
		}
	}
}
