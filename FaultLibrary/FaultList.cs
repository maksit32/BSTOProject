using FaultsLibrary;
using System.ComponentModel.DataAnnotations;


namespace FaultsLibrary
{
	//Отказ
	public class Fault
	{
		public Guid Id { get; set; }
		public string FaultMessage { get; set; }

		//1 - gr, 2 - yell, 3 - red
		[Required, MaxLength(1)]
		public int FaultCode { get; set; }
		public Fault(int faultCode, string faultMessage)
		{
			if (faultCode < 1 || faultCode > 3) throw new ArgumentOutOfRangeException("Fault code must be between 1 and 3");
			if (string.IsNullOrWhiteSpace(faultMessage))
			{
				throw new ArgumentException($"\"{nameof(faultMessage)}\" не может быть пустым или содержать только пробел.", nameof(faultMessage));
			}

			Id = Guid.NewGuid();
			FaultCode = faultCode;
			FaultMessage = faultMessage;
		}

	}
	//список отказов (проверяются на сервере, отсылаются ВС)
	public static class FaultList
	{
		public static readonly List<Fault> faultsList = new List<Fault>()
		{
			new Fault(3, "Отказ двигателя 1"),
			new Fault(3, "Отказ двигателя 2"),
			new Fault(3, "Отказ двигателя 3"),
			new Fault(3, "Отказ двигателя 4"),
			new Fault(3, "Отказ ВСУ"),
			new Fault(3, "Пожар двигателя 1"),
			new Fault(3, "Пожар двигателя 2"),
			new Fault(3, "Пожар двигателя 3"),
			new Fault(3, "Пожар двигателя 4"),
			new Fault(3, "Пожар ВСУ"),
			new Fault(3, "Потеря радиосвязи"),
			new Fault(3, "Отказ бортовой МЕТЕО РЛС"),

			new Fault(2, "Отказ интерцепторов"),
			new Fault(2, "Отказ левого комплекта автопилота"),
			new Fault(2, "Отказ правого комплекта автопилота"),
			new Fault(2, "Обнаружен лед"),

			new Fault(1, "Отказ освещения"),
			new Fault(1, "Отказ нагревательных элементов на кухне"),
			new Fault(1, "Неисправлена подсветка ECAM панели"),
			new Fault(1, "Неисправна подсветка MCDU"),
		};
	}
}


//new Fault { Id = Guid.NewGuid(), FaultCode = 3, FaultMessage = "Отказ двигателя 1" },
//			new Fault { Id = Guid.NewGuid(), FaultCode = 3, FaultMessage = "Отказ двигателя 2" },
//			new Fault { Id = Guid.NewGuid(), FaultCode = 3, FaultMessage = "Отказ двигателя 3" },
//			new Fault { Id = Guid.NewGuid(), FaultCode = 3, FaultMessage = "Отказ двигателя 4" },
//			new Fault { Id = Guid.NewGuid(), FaultCode = 3, FaultMessage = "Отказ ВСУ" },
//			new Fault { Id = Guid.NewGuid(), FaultCode = 3, FaultMessage = "Пожар двигателя 1" },
//			new Fault { Id = Guid.NewGuid(), FaultCode = 3, FaultMessage = "Пожар двигателя 2" },
//			new Fault { Id = Guid.NewGuid(), FaultCode = 3, FaultMessage = "Пожар двигателя 3" },
//			new Fault { Id = Guid.NewGuid(), FaultCode = 3, FaultMessage = "Пожар двигателя 4" },
//			new Fault { Id = Guid.NewGuid(), FaultCode = 3, FaultMessage = "Пожар ВСУ" },
//			new Fault { Id = Guid.NewGuid(), FaultCode = 3, FaultMessage = "Потеря радиосвязи" },
//			new Fault { Id = Guid.NewGuid(), FaultCode = 3, FaultMessage = "Отказ бортовой МЕТЕО РЛС" },


//			new Fault { Id = Guid.NewGuid(), FaultCode = 2, FaultMessage = "Отказ интерцепторов" },
//			new Fault { Id = Guid.NewGuid(), FaultCode = 2, FaultMessage = "Отказ левого комплекта автопилота" },
//			new Fault { Id = Guid.NewGuid(), FaultCode = 2, FaultMessage = "Отказ правого комплекта автопилота" },
//			new Fault { Id = Guid.NewGuid(), FaultCode = 2, FaultMessage = "Обнаружен лед" },



//			new Fault { Id = Guid.NewGuid(), FaultCode = 1, FaultMessage = "Отказ освещения" },
//			new Fault { Id = Guid.NewGuid(), FaultCode = 1, FaultMessage = "Отказ нагревательных элементов на кухне" },
//			new Fault { Id = Guid.NewGuid(), FaultCode = 1, FaultMessage = "Неисправлена подсветка ECAM панели" },
//			new Fault { Id = Guid.NewGuid(), FaultCode = 1, FaultMessage = "Неисправна подсветка MCDU" }