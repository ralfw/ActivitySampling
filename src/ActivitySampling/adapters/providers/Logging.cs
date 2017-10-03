using System;
using System.IO;

namespace ActivitySampling.adapters.providers
{
    class Logging
    {
        public static Logging Instance { get; private set; }

        public static void Initialize(string applicationDataFolderPath) {
            Instance = new Logging(applicationDataFolderPath);
        }


        private readonly string logFilePath;

        private Logging(string applicationDataFolderPath) {
            this.logFilePath = Path.Combine(applicationDataFolderPath, "app.log");
        }

        public void Append(string message) {
            var entry = $"{DateTime.Now:s} - {message}";
            File.AppendAllLines(this.logFilePath, new[] { entry });
        }
    }
}
