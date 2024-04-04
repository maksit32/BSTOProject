using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace LogLibrary.Interfaces
{
    public interface ILogger
    {
        Task LogToFileAsync(object lockObject, string logMessage);
        void RemoveLogFile();
	}
}
