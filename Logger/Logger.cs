using System;
using log4net;
using System.Reflection;
using Newtonsoft.Json;
using log4net.Config;

namespace Cap.Helpers.Logger
{
    public interface ILogger
    {
        void Debug(string message, object? data = null);
        void Info(string message, object? data = null);
        void Error(string message, Exception? exception = null);
    }

    public class Logger : ILogger
    {
        private readonly ILog _logger;
        public Logger()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4netconfig.config"));
            this._logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        }

        public void Debug(string message, object? data = null)
        {
            this._logger?.Debug(message + JsonConvert.SerializeObject(data ?? ""));
        }

        public void Info(string message, object? data = null)
        {
            this._logger.Info(message + JsonConvert.SerializeObject(data ?? ""));
        }

        public void Error(string message, Exception? exception = null)
        {
            this._logger?.Error(message + JsonConvert.SerializeObject(exception));
        }
    }
}

