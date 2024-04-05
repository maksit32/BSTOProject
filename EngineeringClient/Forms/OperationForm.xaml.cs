using Entities;
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


namespace EngineeringClient.Forms
{
	public partial class OperationForm : Window
	{
		private PlaneFaultData? planeFault;
		private TcpClient server;
		public OperationForm(TcpClient server, string operationText, PlaneFaultData? fault)
		{
			InitializeComponent();
			planeFault = fault;
			this.server = server;
			this.OperationLabel.Content = operationText;
			Window.Title = operationText;
		}

		private void SendButton_Click(object sender, RoutedEventArgs e)
		{

		}
	}
}
