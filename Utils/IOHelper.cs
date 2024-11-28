using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RS.Tools.Common.Enums;
using System.Collections.Concurrent;
using System.Drawing;
using System.Threading;

namespace RS.Tools.Common.Utils
{
    public class IOHelper
    {
        private const string TAG = "IOHelper";
        private static ConcurrentDictionary<string, object> _readFileSyncObjects = new ConcurrentDictionary<string, object>();
        private static ReaderWriterLockSlim _writeLocker = new ReaderWriterLockSlim();

        private static Object GetReadFileSyncObject(string path)
        {
            if (!_readFileSyncObjects.ContainsKey(path)) _readFileSyncObjects.TryAdd(path, new Object());
            return _readFileSyncObjects[path];
        }
        public static long GetFileSize(string fullName)
        {
            if (File.Exists(fullName)) return new FileInfo(fullName).Length;
            return 0;
        }

        public static string GetFileSizeDesc(string fullName)
        {
            string m_strSize = "";
            long FactSize = GetFileSize(fullName);
            if (FactSize < 1024.00) m_strSize = FactSize.ToString("F2") + " Byte";
            else if (FactSize >= 1024.00 && FactSize < 1048576) m_strSize = (FactSize / 1024.00).ToString("F2") + " K";
            else if (FactSize >= 1048576 && FactSize < 1073741824) m_strSize = (FactSize / 1024.00 / 1024.00).ToString("F2") + " M";
            else if (FactSize >= 1073741824) m_strSize = (FactSize / 1024.00 / 1024.00 / 1024.00).ToString("F2") + " G";
            return m_strSize;
        }

        /// <summary>
        /// 拷贝文件
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static bool CopyFile(string from, string to)
        {
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to)) return false;
            if (!File.Exists(from)) return false;
            var folder = GetFileRoot(to);
            if (string.IsNullOrEmpty(folder)) return false;
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            try
            {
                File.Copy(from, to, true);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteException(ex, TAG);
                return false;
            }
        }
        /// <summary>
        /// 从文件中获取全部字符串
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isFullPath"></param>
        /// <returns></returns>
        public static string GetStringFromFile(string path, bool isFullPath = false)
        {
            if (string.IsNullOrEmpty(path)) return "";
            path = isFullPath ? path : AppDomain.CurrentDomain.BaseDirectory + path;
            if (!File.Exists(path)) return "";

            lock (GetReadFileSyncObject(path))
            {
                try
                {
                    return File.ReadAllText(path);
                }
                catch (Exception ex)
                {
                    Logger.Instance.WriteException(ex, TAG);
                    return null;
                }
            }

        }
        /// <summary>
        /// 从文件读取比特数组
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isFullPath"></param>
        /// <returns></returns>
        public static byte[] GetBytesFromFile(string path, bool isFullPath = false)
        {
            if (string.IsNullOrEmpty(path)) return null;
            string Path = isFullPath ? path : AppDomain.CurrentDomain.BaseDirectory + path;
            if (!File.Exists(Path)) return null;

            lock (GetReadFileSyncObject(path))
            {
                try
                {
                    return File.ReadAllBytes(path);
                }
                catch (Exception ex)
                {
                    Logger.Instance.WriteException(ex, TAG);
                    return null;
                }
            }
        }

        public static void OpenFolder(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return;
            try
            {
                System.Diagnostics.Process.Start(path);
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteException(ex, "OpenFolder");
            }
        }

        public static void OpenFolderAndSelect(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;
            try
            {
                System.Diagnostics.Process.Start("Explorer", "/select," + path);
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteException(ex, "OpenFolder");
            }
        }

        ///// <summary>
        ///// 从文件获取比特数组
        ///// </summary>
        ///// <param name="path"></param>
        ///// <returns></returns>
        //public static byte[] GetBytes(string path)
        //{
        //    byte[] data = null;
        //    long count = 0;
        //    using (var oriFS = new FileStream(path, FileMode.Open))
        //    {
        //        count = oriFS.Length;
        //        if (count == 0) return null;
        //        data = new byte[count];
        //        oriFS.Read(data, 0, Convert.ToInt32(count));
        //    }
        //    return data;
        //}
        /// <summary>
        /// 文件是否存在
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        public static bool IsFileExist(string FilePath)
        {
            if (string.IsNullOrEmpty(FilePath))
                return false;
            string Path = AppDomain.CurrentDomain.BaseDirectory + FilePath;
            return File.Exists(Path);
        }
        /// <summary>
        /// 保存指定类型的JTOKEN到明文文本文件
        /// </summary>
        /// <param name="Src"></param>
        /// <param name="FullPath"></param>
        /// <param name="DirectPath"></param>
        /// <returns></returns>
        public static string SaveJO(dynamic Src, string FullPath, bool DirectPath = false)
        {
            if (Src == null || string.IsNullOrEmpty(FullPath))
                return "";
            WriteFile(
                JSONHelper.AnyObject2JString(Src).Replace("\\", "").Replace("\"" + "[", "[").Replace("]" + "\"", "]"),
                FullPath, DirectPath
                );
            return FullPath;
        }
        /// <summary>
        /// 从明文文本文件读取JTOKEN
        /// </summary>
        /// <param name="FullPath"></param>
        /// <param name="DirectPath"></param>
        /// <returns></returns>
        public static JToken GetJO(string FullPath, bool DirectPath = false)
        {
            if (string.IsNullOrEmpty(FullPath))
                return null;
            //if (!DirectPath)
            //    FullPath = string.Format("{0}.res", FullPath);
            string Res_Str;
            Res_Str = GetStringFromFile(FullPath, DirectPath);
            if (string.IsNullOrEmpty(Res_Str))
                return null;
            try
            {
                JToken Res_JO = JSONHelper.String2JToken(Res_Str);
                return Res_JO;
            }
            catch (Exception ex)
            {
                //Log.Error("IO HELPER", Log.ErrorLogBuilder(ex));
                return null;
            }
        }
        /// <summary>
        /// 获取指定的SEND.TXT
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static string GetSendFile(ChannelType channel, byte[] key)
        {
            var path = String.Join("", AppDomain.CurrentDomain.BaseDirectory, @"res\" + DataSourceFolderName(channel), "send.res");
            if (!File.Exists(path)) return "";
            return CryptoHelper.DeMapping(path, false, true, key);
        }
        /// <summary>
        /// 获取指定的COMMUNICATE.TXT
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static string GetCommFile(ChannelType channel, byte[] key)
        {
            var path = String.Join("", AppDomain.CurrentDomain.BaseDirectory, @"res\" + DataSourceFolderName(channel), "communicate.res");
            if (!File.Exists(path)) return "";
            return CryptoHelper.DeMapping(path, false, true, key);
        }
        /// <summary>
        /// 保存JTOKEN到加密的文件
        /// </summary>
        /// <param name="type"></param>
        /// <param name="src"></param>
        /// <param name="name"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static string SaveCSV(CSVType type, dynamic src, string name = "", ChannelType channel = ChannelType.UNDEFINED)
        {
            if (src == null)
            {
                Logger.Instance.WriteInfo(TAG, "Save CSV failed! Empty data!");
                return "";
            }

            var path = GetCSVPathByType(type, name, channel);
            if (string.IsNullOrEmpty(path.path))
            {
                Logger.Instance.WriteInfo(TAG, "Save CSV failed! Can not find the path!");
                return "";
            }

            try
            {
                return SaveCSV(path.path, src);
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteException(ex, TAG);
                return "";
            }
        }
        /// <summary>
        /// 保存JTOKEN到加密的文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string SaveCSV(string path, dynamic src)
        {
            try
            {
                WriteFile(CryptoHelper.EnMapping(JSONHelper.AnyObject2JString(src)), path);
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteException(ex, TAG);
                return "";
            }
            return path;
        }

        public static string GetCSVString(string path)
        {
            try
            {
                return CryptoHelper.DeMapping(GetStringFromFile(path, true));
            }
            catch (Exception)
            {
                return "";
            }
        }

        public (long time, string nickName) AnalysePackage(string path)
        {
            if (!File.Exists(path)) return (-1, null);
            var str = GetCSVString(path);
            if (string.IsNullOrEmpty(str)) return (-1, null);
            try
            {
                long time = -1;
                string name = null;
                var ja = JArray.Parse(str);
                foreach (dynamic jo in ja)
                {
                    if (jo is not JObject) return (-1, null);
                    foreach (var item in jo)
                    {
                        if (item.Name == "MSG_SERVER_TIME")
                        {
                            time = JSONHelper.ParseLong(item.Value.server_time);
                            break;
                        }
                        else if (item.Name == "MSG_ME_UPDATED")
                        {
                            name = JSONHelper.ParseString(item.Value.dbase.name);
                            name = CryptoHelper.GetNickName(name);
                            break;
                        }
                    }
                    if (time > 0 && !string.IsNullOrEmpty(name)) break;
                }
                return (time, name);
            }
            catch (Exception)
            {
                return (-1, null);
            }
        }

        public static (string path, bool needFix) GetCSVPathByType(CSVType type, string name = "", ChannelType channel = ChannelType.UNDEFINED)
        {
            switch (type)
            {
                case CSVType.ClubKPI:
                    return ($@"USER\CLUB\kpi_{name}.res", false);
                case CSVType.ClubMembers:
                    return ($@"USER\CLUB\members_{name}.res", false);
                case CSVType.Packages:
                    return ($@"USER\PKGS\{name}.res", false);
                case CSVType.UserProfile:
                    if (channel == ChannelType.UNDEFINED) return ("", false);
                    return ($@"USER\CACHE\c_{DataSourceFolderName(channel)}.res", false);
                case CSVType.Events:
                    return ($@"USER\CLUB\events.res", false);
                case CSVType.Settings:
                    return ($@"USER\SETTINGS\setting.res", false);
                case CSVType.FileList:
                    return ($@"USER\CACHE\{name}.res", false);
                case CSVType.DefaultValues:
                    if (channel == ChannelType.UNDEFINED) return ("", false);
                    return ($@"RES\{DataSourceFolderName(channel)}defaults.res", true);
                case CSVType.CSV:
                    if (channel == ChannelType.UNDEFINED) return ("", true);
                    return ($@"RES\{DataSourceFolderName(channel).ToUpper()}{name}.res", true);
                case CSVType.RobotData:
                    return ($@"BOT\{name}.res", false);
                case CSVType.SSEUData:
                    if (channel == ChannelType.UNDEFINED) return ("", false);
                    return ($@"SSEU\SRC\{DataSourceFolderName(channel)}{name}.res", false);
                case CSVType.SSEUDataCommon:
                    return ($@"SSEU\SRC\common\{name}.res", false);
                case CSVType.DeamonData:
                    return ($@"USER\DATA\deamon.res", false);
                case CSVType.JJJDeviceInfoIos:
                    return ($@"BOT\device_info_ios.res", false);
                case CSVType.JJJDeviceInfoAndroid:
                    return ($@"BOT\device_info_android.res", false);
                case CSVType.Mayday:
                    return ($@"BOT\mayday.res", false);
                default: return ("", false);
            }
        }


        /// <summary>
        /// 从加密的文件中读取JTOKEN
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static dynamic GetCSV(CSVType type, string name = "", ChannelType channel = ChannelType.UNDEFINED, byte[] key = null)
        {
            string res = "";
            var path = GetCSVPathByType(type, name, channel);
            if (string.IsNullOrEmpty(path.path) || !File.Exists(path.path)) return null;
            try
            {
                res = CryptoHelper.DeMapping(path.path, false, path.needFix, key);
                if (string.IsNullOrEmpty(res)) return null;
                // switch (type)
                //{
                //    case CSVType.CSV:
                //    case CSVType.Events:
                //        return JArray.Parse(res);
                //    default: return JObject.Parse(res);
                //}
                return JToken.Parse(res);
            }
            catch (Exception ex)
            {
                if (type == CSVType.CSV)
                {
                    throw ex;
                }
                else
                {
                    Logger.Instance.WriteException(ex, "IOHELPER.GetCSV");
                    return null;
                }

            }
        }
        /// <summary>
        /// 获得渠道对应的资源文件夹名称
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static string DataSourceFolderName(ChannelType channel)
        {
            switch (channel)
            {
                case ChannelType.IOS:
                case ChannelType.ANDROID:
                case ChannelType.CHANNEL:
                    return @$"formal\";
                case ChannelType.GUANGZI:
                    return @$"guangzi\";
                case ChannelType.TAIWAN:
                    return @$"taiwan\";
                default: return "";

            }
        }
        //public static bool WriteFile(byte[] content, string path, bool directPath = false)
        //{
        //    //var outputStr = Encoding.UTF8.GetString(content);
        //    //return WriteFile(outputStr, path, directPath);
        //    if (string.IsNullOrEmpty(path)) return false;
        //    path = directPath ? path : AppDomain.CurrentDomain.BaseDirectory + path;
        //    var folder = GetFileRoot(path);
        //    if (!Directory.Exists(path)) Directory.CreateDirectory(folder);
        //    try
        //    {
        //        using (FileStream streamWriter = File.Create(path))
        //        {
        //            streamWriter.Write(content);
        //            streamWriter.Close();
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Instance.Write(ex, TAG);
        //    }
        //    return false;
        //}

        public static bool WriteFile(string OutputStr, string OutputPath, bool DirectPath = false)
        {
            if (string.IsNullOrEmpty(OutputStr) || string.IsNullOrEmpty(OutputPath))
                return false;
            string Path = DirectPath ? OutputPath : System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, OutputPath);
            string folder = GetFileRoot(Path);
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            try
            {
                _writeLocker.EnterWriteLock();
                using (StreamWriter sw = new StreamWriter(Path, false, System.Text.Encoding.UTF8))
                {
                    sw.WriteLine(OutputStr);
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteException(ex, "IOHELPER.save");
                return false;
            }
            finally { _writeLocker.ExitWriteLock(); }
        }

        #region PATH STRING
        public static string GetFileName(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            return path.Split('\\').Last();
        }
        public static string GetFileRoot(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            var arr = path.Split('\\');
            if (arr.Length < 2) return path;
            var new_path = new List<string> { };
            for (int i = 0; i < (arr.Length - 1); i++)
            {
                new_path.Add(arr[i]);
            }
            return string.Join("\\", new_path.ToArray());
        }
        public static string GetExtension(string path)
        {
            path = GetFileName(path);
            return path.Split('.').Last();
        }
        public static string GetFileNameWithoutExtension(string path)
        {
            path = GetFileName(path);
            var arr = path.Split('.');
            if (arr.Length < 2) return path;
            var new_path = new List<string> { };
            for (int i = 0; i < (arr.Length - 1); i++)
            {
                new_path.Add(arr[i]);
            }
            return string.Join(".", new_path.ToArray());
        }
        #endregion
        #region ENUMS

        #endregion
    }
}
