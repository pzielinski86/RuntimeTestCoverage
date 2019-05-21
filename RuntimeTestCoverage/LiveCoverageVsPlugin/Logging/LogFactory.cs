using log4net.Config;
using System.IO;
using System.Reflection;

namespace LiveCoverageVsPlugin.Logging
{
    public class LogFactory
    {
        private static volatile bool _isLoaded;
        private static readonly object _Sync = new object();

        public static log4net.ILog GetLogger(MethodBase currentMethod)
        {
            InitLogs();

            return log4net.LogManager.GetLogger(currentMethod.DeclaringType);

        }
        private static void InitLogs()
        {
            if (!_isLoaded)
            {
                lock (_Sync)
                {
                    if (!_isLoaded)
                    {
                        string extensionFolder = Path.GetDirectoryName(typeof(LogFactory).Assembly.Location);
                        string configLocation = Path.Combine(extensionFolder, "log4net.config");
                        log4net.GlobalContext.Properties["ExtensionFolder"] = extensionFolder;
                        XmlConfigurator.Configure(new FileInfo(configLocation));
                        _isLoaded = true;
                    }
                }
            }
        }
    }
}