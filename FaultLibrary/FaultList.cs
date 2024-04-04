using System.ComponentModel.DataAnnotations;


namespace FaultsLibrary
{
	//Отказ
	public class Fault
	{
		public Guid Id { get; set; }

		//1 - gr, 2 - yell, 3 - red
		[Required, MaxLength(1)]
		public int FaultCode
		{
			get { return this.FaultCode; }
			set
			{
				if (value < 1 || value > 3) throw new ArgumentOutOfRangeException("Fault code must be between 1 and 3");
				this.FaultCode = value;
			}
		}

		public required string FaultMessage { get; set; }
	}
	//список отказов (проверяются на сервере, отсылаются ВС)
	public static class FaultList
	{
		public static readonly List<Fault> faultsList = new List<Fault>
		{
			new Fault { Id = Guid.NewGuid(), FaultCode = 3, FaultMessage = "Отказ двигателя 1"},
			new Fault { Id = Guid.NewGuid(), FaultCode = 3, FaultMessage = "Отказ двигателя 2"},
			new Fault { Id = Guid.NewGuid(), FaultCode = 3, FaultMessage = "Отказ двигателя 3"},
			new Fault { Id = Guid.NewGuid(), FaultCode = 3, FaultMessage = "Отказ двигателя 4"},
			new Fault { Id = Guid.NewGuid(), FaultCode = 3, FaultMessage = "Отказ ВСУ"},
			new Fault { Id = Guid.NewGuid(), FaultCode = 3, FaultMessage = "Пожар двигателя 1"},
			new Fault { Id = Guid.NewGuid(), FaultCode = 3, FaultMessage = "Пожар двигателя 2"},
			new Fault { Id = Guid.NewGuid(), FaultCode = 3, FaultMessage = "Пожар двигателя 3"},
			new Fault { Id = Guid.NewGuid(), FaultCode = 3, FaultMessage = "Пожар двигателя 4"},
			new Fault { Id = Guid.NewGuid(), FaultCode = 3, FaultMessage = "Пожар ВСУ"},
			new Fault { Id = Guid.NewGuid(), FaultCode = 3, FaultMessage = "Потеря радиосвязи"},
			new Fault { Id = Guid.NewGuid(), FaultCode = 3, FaultMessage = "Отказ бортовой МЕТЕО РЛС"},


			new Fault { Id = Guid.NewGuid(), FaultCode = 2, FaultMessage = "Отказ интерцепторов"},
			new Fault { Id = Guid.NewGuid(), FaultCode = 2, FaultMessage = "Отказ левого комплекта автопилота"},
			new Fault { Id = Guid.NewGuid(), FaultCode = 2, FaultMessage = "Отказ правого комплекта автопилота"},
			new Fault { Id = Guid.NewGuid(), FaultCode = 2, FaultMessage = "Обнаружен лед"},



			new Fault { Id = Guid.NewGuid(), FaultCode = 1, FaultMessage = "Отказ освещения"},
			new Fault { Id = Guid.NewGuid(), FaultCode = 1, FaultMessage = "Отказ нагревательных элементов на кухне"},
			new Fault { Id = Guid.NewGuid(), FaultCode = 1, FaultMessage = "Неисправлена подсветка ECAM панели"},
			new Fault { Id = Guid.NewGuid(), FaultCode = 1, FaultMessage = "Неисправна подсветка MCDU"}
		};
	}
}
