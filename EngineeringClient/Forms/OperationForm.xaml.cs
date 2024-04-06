using Entities;
using LogLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


using static JSONLibrary.JSONLibrary;
using static TCPLibrary.TCPLibrary;

namespace EngineeringClient.Forms
{
	public partial class OperationForm : Window
	{
		private PlaneFaultData? planeFault;
		private TcpClient server;
		private readonly string operationText;
		private readonly ILogger logger;
		private object locker;
		public OperationForm(TcpClient server, string operationText, PlaneFaultData? fault, ILogger logger, object locker)
		{
			InitializeComponent();
			planeFault = fault;
			this.server = server;
			this.OperationLabel.Content = operationText;
			Window.Title = operationText;
			this.operationText = operationText;
			this.OperationLabel.VerticalContentAlignment = VerticalAlignment.Center;
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.locker = locker ?? throw new ArgumentNullException(nameof(locker));
		}
		//в зависимости от того, какая операция вызвала функцию, будет вызвана та или иная отправка
		private async void SendButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				switch (operationText)
				{
					case string message when operationText.Contains("Добавление"):
						{
							//проверка на введенные данные
							if (this.ToPlaceTextBox.Text.Length == 4 && this.FromPlaceTextBox.Text.Length == 4 && this.PlaneIdentificatorTextBox.Text.Length == 6 && this.ErrorCodeTextBox.Text.Length == 1 && this.FaultMessageTextBox.Text.Length > 0)
							{
								//создание нового объекта
								PlaneFaultData fault = new PlaneFaultData(int.Parse(this.ErrorCodeTextBox.Text), FaultMessageTextBox.Text, PlaneIdentificatorTextBox.Text, FromPlaceTextBox.Text, ToPlaceTextBox.Text, DateTime.UtcNow);
								string serializedObj = SerializeClassObject(fault);

								if (server.Connected)
								{
									await SendMessageAsync(server, $"AddJSON|{serializedObj}");
									this.Close();
								}
							}
						}
						break;
					case string message when operationText.Contains("Изменение"):
						{
							//проверка на введенные данные
							if (this.ToPlaceTextBox.Text.Length == 4 && this.FromPlaceTextBox.Text.Length == 4 && this.PlaneIdentificatorTextBox.Text.Length == 6 && this.ErrorCodeTextBox.Text.Length == 1 && this.FaultMessageTextBox.Text.Length > 0)
							{
								if (planeFault is null) break;
								//изменение старого объекта(id сохранен)
								PlaneFaultData fault = new PlaneFaultData(planeFault.Id, int.Parse(this.ErrorCodeTextBox.Text), FaultMessageTextBox.Text, PlaneIdentificatorTextBox.Text, FromPlaceTextBox.Text, ToPlaceTextBox.Text, planeFault.RecordUTCDate);
								string serializedObj = SerializeClassObject(fault);

								if (server.Connected)
								{
									await SendMessageAsync(server, $"ChangeJSON|{serializedObj}");
									this.Close();
								}
							}
						}
						break;
				}
			}
			catch (Exception ex)
			{
				await logger.LogToFileAsync(locker, ex.Message);
				MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void ErrorCodeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			//только цифры из диапазона
			char inp = e.Text[0];
			if (inp < '1' || inp > '3')
				e.Handled = true;

			if (this.ErrorCodeTextBox.Text.Length == 1)
			{
				e.Handled = true;
			}
		}

		private void FromPlaceTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (this.FromPlaceTextBox.Text.Length >= 4)
				e.Handled = true;
		}
		private void FromPlaceTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			char inp = e.Text[0];
			if (inp < 'A' || inp > 'Z')
				e.Handled = true;
		}
		private void ToPlaceTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (this.ToPlaceTextBox.Text.Length >= 4)
				e.Handled = true;
		}
		private void ToPlaceTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			char inp = e.Text[0];
			if (inp < 'A' || inp > 'Z')
				e.Handled = true;
		}

		private void FaultMessageTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			//ошибка только в виде текста! 
			char ch = e.Text[0];
			if (!(ch >= 'а' && ch <= 'я' || ch >= 'A' && ch <= 'Я'))
				e.Handled = true;
		}

		private void PlaneIdentificatorTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			//с 1 по 3 символ
			if (this.PlaneIdentificatorTextBox.Text.Length >= 0 && this.PlaneIdentificatorTextBox.Text.Length < 3)
			{
				char ch = e.Text[0];
				if (ch < 'A' || ch > 'Z')
					e.Handled = true;
			}
			if (this.PlaneIdentificatorTextBox.Text.Length >= 3 && this.PlaneIdentificatorTextBox.Text.Length < 6)
			{
				if (!Char.IsDigit(e.Text, 0))
					e.Handled = true;
			}
			if (this.PlaneIdentificatorTextBox.Text.Length > 5)
				e.Handled = true;
		}
	}
}
