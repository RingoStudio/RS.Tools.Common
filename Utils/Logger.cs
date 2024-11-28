using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.IO;


namespace RS.Tools.Common.Utils
{
    public class Logger
    {
        private static Logger _instance = new Logger("common");
        public static Logger Instance => _instance;

        private readonly string _logFileName = "common";

        public Logger(string fileName)
        {
            _logFileName = fileName;
        }

        public void WriteInfo(string type, string message, params object[] param)
        {
            var raw = string.Format(message, param);
            LoggerStatic.WriteInfo(_logFileName, type, raw);
        }

        [Conditional("DEBUG")]
        public void WriteDebug(string message) => LoggerStatic.WriteDebug(_logFileName, message);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void WriteWarning(string message) => LoggerStatic.WriteWarning(_logFileName, message);
        public void WriteException(Exception e, string track, bool showConsole = true) => LoggerStatic.WriteException(_logFileName, e, track, showConsole);


    }
}
