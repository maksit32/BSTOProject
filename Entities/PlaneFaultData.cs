using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


//Entity for EF Core (ORM)
namespace Entities
{
    [Serializable]
    public class PlaneFaultData
    {
        public Guid Id { get; set; }
        public int FaultCode { get; set; }
        public string FaultMessage { get; set; }
        public string PlaneIdentificator { get; set; }
        public string FromPlace { get; set; }
        public string ToPlace { get; set; }
        public DateTime RecordUTCDate { get; set; }

        protected PlaneFaultData() { }

        public PlaneFaultData(int faultCode, string faultMessage, string planeIdentificator, string fromPlace, string toPlace, DateTime recordUTCDate)
        {
            if (faultCode < 1 || faultCode > 3) throw new ArgumentOutOfRangeException("Fault code must be between 1 and 3");
            if (string.IsNullOrWhiteSpace(faultMessage))
            {
                throw new ArgumentException($"\"{nameof(faultMessage)}\" не может быть пустым или содержать только пробел.", nameof(faultMessage));
            }

            if (string.IsNullOrWhiteSpace(fromPlace))
            {
                throw new ArgumentException($"\"{nameof(fromPlace)}\" не может быть пустым или содержать только пробел.", nameof(fromPlace));
            }

            if (string.IsNullOrWhiteSpace(planeIdentificator))
            {
                throw new ArgumentException($"\"{nameof(planeIdentificator)}\" не может быть пустым или содержать только пробел.", nameof(planeIdentificator));
            }

            if (string.IsNullOrWhiteSpace(toPlace))
            {
                throw new ArgumentException($"\"{nameof(toPlace)}\" не может быть пустым или содержать только пробел.", nameof(toPlace));
            }

            Id = Guid.NewGuid();
            FaultCode = faultCode;
            FaultMessage = faultMessage;
            FromPlace = fromPlace;
            ToPlace = toPlace;
            RecordUTCDate = recordUTCDate;
        }
        public override string ToString()
        {
            return $"Код: {FaultCode}	Отказ: {FaultMessage}	Идентификатор ВС: {PlaneIdentificator}	Время отказа: {RecordUTCDate}";
        }
    }
}
