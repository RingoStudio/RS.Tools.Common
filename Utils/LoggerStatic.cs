using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Tools.Common.Utils
{
    public class LoggerStatic
    {
        private readonly string _logFileName = "";

        private const string DebugMessageTemplate = "Time: {0} DEBUG: {1}";
        private const string WarningTemplate = "Time: {0} WARNING: {1};{3}StackTrace:{3}{2}{3}";
        private const string InfoTemplate = "Time: {0} Name: {1} INFO: {2}";
        private const string MessageTemplate = "{4}Time: {0};{5}{4}Type: {1};{5}{4}Message: {2};{5}{4}StackTrace:{5}{3}{5}";
        private const string InnerTemplate = "{1}InnerException: {2}{2}{0}{2}";
        private const int WriteAttempts = 5;
        private static Dictionary<string, object> _syncObjects = new Dictionary<string, object>();
        private static readonly object _consoleWriterLock = new object();

        /// <summary>
        /// 写入同步锁
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static object GetSyncObject(string fileName)
        {
            lock (_syncObjects)
            {
                if (!_syncObjects.ContainsKey(fileName)) _syncObjects.TryAdd(fileName, new object());
                return _syncObjects[fileName];
            }
        }

        private static string FilePath(string fileName) => $"LOG\\{fileName}_{DateTime.Now.ToString("yyyy-MM-dd-HH")}.log";

        /// <summary>
        /// 记录日志信息
        /// </summary>
        /// <param name="file"></param>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <param name="param"></param>
        public static void WriteInfo(string file, string type, string message, bool ShowConsole = true)
        {
            if (ShowConsole)
            {
                lock (_consoleWriterLock)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss")}][INFO][{type}]{message}");
                }
            }
            Write(file, string.Format(InfoTemplate, DateTime.Now, type, message));
        }

        [Conditional("DEBUG")]
        public static void WriteDebug(string file, string message, bool ShowConsole = true)
        {
            if (ShowConsole)
            {
                lock (_consoleWriterLock)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(string.Format("[{0}][", DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss")));
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("DEBUG");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(string.Format("][{0}]", file));
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(message);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            Write(file, string.Format(DebugMessageTemplate, DateTime.Now, message));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteWarning(string file, string message, string extra = "", bool ShowConsole = true)
        {
            if (ShowConsole)
            {
                lock (_consoleWriterLock)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(string.Format("[{0}][", DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss")));
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("WARNINIG");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(string.Format("][{0}]", file));
                    if (!string.IsNullOrEmpty(extra)) Console.Write(string.Format("[{0}]", extra));
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(string.Format("{0}", message));
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            StackTrace stackTrace = new StackTrace(1, true);
            if (!string.IsNullOrEmpty(extra)) message = $"[{extra}] {message}";
            Write(file, string.Format(WarningTemplate, DateTime.Now, message, stackTrace, Environment.NewLine));
        }

        public static void WriteException(string file, Exception e, string extra = "", bool ShowConsole = true)
        {
            string message = CreateLogMessage(e, 0, extra);
            if (ShowConsole)
            {
                lock (_consoleWriterLock)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(string.Format("[{0}][", DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss")));
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("ERROR");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(string.Format("][{0}]", file));
                    //Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(message);
                    //Console.ForegroundColor = ConsoleColor.White;
                }
            }
            Write(file, message);
        }

        private static void Write(string file, string message, params object[] args)
        {
            lock (GetSyncObject(file))
            {
                int attempts = 0;
                while (true)
                {
                    FileStream logFile = null;
                    StreamWriter logWriter = null;
                    try
                    {
                        var path = FilePath(file);
                        var directory = IOHelper.GetFileRoot(path);
                        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                        logFile = new FileStream(path, FileMode.Append, FileAccess.Write);
                        logWriter = new StreamWriter(logFile);
                        if (message.Contains("{0}")) message = string.Format(message, args);
                        logWriter.WriteLine(message);
                    }
                    catch (IOException ex)
                    {
                        attempts++;
                        if (attempts >= WriteAttempts)
                            return;
                        continue;
                    }
                    finally
                    {
                        if (logWriter != null)
                            logWriter.Dispose();

                        if (logFile != null)
                            logFile.Dispose();
                    }

                    break;
                }
            }
        }

        private static string CreateLogMessage(Exception e, int level, string extra)
        {

            var tabs = new StringBuilder();
            for (int i = 0; i < level; i++)
                tabs.Append("  ");

            var stackTrace = new StringBuilder(e.StackTrace);
            stackTrace.Replace("  ", "  " + tabs);

            var builder = new StringBuilder(200);
            builder.AppendFormat(MessageTemplate, DateTime.Now, e.GetType(), e.Message, stackTrace, tabs, Environment.NewLine);

            if (e.InnerException != null)
                builder.AppendFormat(InnerTemplate, CreateLogMessage(e.InnerException, level + 1, ""), tabs, Environment.NewLine);

            if (!string.IsNullOrEmpty(extra)) return "@" + extra + " " + builder.ToString();
            else return builder.ToString();
        }
    }
}
