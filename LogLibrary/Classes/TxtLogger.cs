using LogLibrary.Interfaces;



namespace LogLibrary.Classes
{
	public class TxtLogger : ILogger
	{
		private readonly string filePath;

		public TxtLogger(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException($"\"{nameof(filePath)}\" не может быть пустым или содержать только пробел.", nameof(filePath));
			}

			this.filePath = filePath;
		}

		public async Task LogToFileAsync(object lockObject, string logMessage)
		{
			if (string.IsNullOrWhiteSpace(logMessage))
			{
				throw new ArgumentException($"\"{nameof(logMessage)}\" не может быть пустым или содержать только пробел.", nameof(logMessage));
			}

			try
			{
				await Task.Run(() =>
				{
					// Создаем экземпляр StreamWriter с указанием пути к файлу
					lock (lockObject)
					{
						using StreamWriter writer = new StreamWriter(filePath, true);
						writer.WriteLine();
						writer.WriteLine(logMessage);
					}
				});
			}
			catch (Exception ex)
			{
				await Console.Out.WriteLineAsync("Возникла ошибка при записи лога: " + ex.Message);
			}
		}

		public void RemoveLogFile()
		{
			File.Delete(filePath);
		}
	}
}
