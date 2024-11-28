using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Concurrent;
using RS.Tools.Common.Utils.RC4;

namespace RS.Tools.Common.Utils
{
    public class CryptoHelper
    {
        private const string TAG = "CryptoHelper";
        private static object _initLocker = new object();
        #region MAPPING
        private static byte[] charMaps = new byte[]
        {
        0x61, 0x67, 0x62, 0x78, 0x63, 0x77, 0x64, 0x76, 0x65,
        0x66, 0x66, 0x75, 0x67, 0x74, 0x68, 0x7A, 0x69, 0x73,
        0x6A, 0x68, 0x6B, 0x65, 0x6C, 0x69, 0x6D, 0x79, 0x6E,
        0x72, 0x6F, 0x64, 0x70, 0x63, 0x71, 0x71, 0x72, 0x62,
        0x73, 0x6A, 0x74, 0x70, 0x75, 0x6B, 0x76, 0x61, 0x77,
        0x6C, 0x78, 0x6D, 0x79, 0x6F, 0x7A, 0x6E, 0x41, 0x4F,
        0x42, 0x4D, 0x43, 0x45, 0x44, 0x4E, 0x45, 0x50, 0x46,
        0x44, 0x47, 0x51, 0x48, 0x4B, 0x49, 0x43, 0x4A, 0x4C,
        0x4B, 0x53, 0x4C, 0x55, 0x4D, 0x4A, 0x4E, 0x42, 0x4F,
        0x52, 0x50, 0x54, 0x51, 0x41, 0x52, 0x49, 0x53, 0x47,
        0x54, 0x46, 0x55, 0x5A, 0x56, 0x58, 0x57, 0x57, 0x58,
        0x56, 0x59, 0x48, 0x5A, 0x59, 0x30, 0x33, 0x31, 0x39,
        0x32, 0x35, 0x33, 0x32, 0x34, 0x34, 0x35, 0x30, 0x36,
        0x37, 0x37, 0x31, 0x38, 0x36, 0x39, 0x38, 0x7B, 0x28,
        0x7D, 0x29, 0x28, 0x3A, 0x29, 0x7B, 0x2E, 0x7D, 0x3A,
        0x20, 0x20, 0x5F, 0x5F, 0x2C, 0x2C, 0xA, 0xA, 0x2E
        };

        private static byte[] encodeToPlain = new byte[256];
        private static byte[] plainToEncoded = new byte[256];
        private static bool charMapInited = false;

        private static void InitMaps()
        {
            lock (_initLocker)
            {
                if (charMapInited) return;
                BuildEncryptMap();
                charMapInited = true;
            }
        }
        private static void BuildEncryptMap()
        {

            int idx = 0;
            while (true)
            {
                idx += 2;
                if (idx == 146) break;
                encodeToPlain[charMaps[idx - 1]] = charMaps[idx - 2];
                plainToEncoded[charMaps[idx - 2]] = charMaps[idx - 1];
            }
            charMapInited = true;
        }


        public static string DeMapping(string FileName, bool IsFullName = false, bool needFix = false, byte[] fixKey = null)
        {
            if (string.IsNullOrEmpty(FileName)) return "";
            string path = IsFullName ? AppDomain.CurrentDomain.BaseDirectory + FileName : FileName;
            if (!File.Exists(path)) return "";
            InitMaps();

            try
            {
                var bytes = IOHelper.GetBytesFromFile(path, true);
                if (bytes == null || bytes.Length == 0) return "";
                int skips = CheckMappingPrefix(bytes);
                //#if DEBUG
                //                needFix = false;
                //#endif
                if (needFix && skips > 0)
                {
                    if (skips > 0) bytes = bytes.Skip(skips).ToArray();
                    bytes = FixMapping(bytes, fixKey);
                }
        //        var xx = System.Text.Encoding.UTF8.GetString(bytes).Replace(StringHelper.ChrW(65279), "");
                var target = new List<byte>();
                foreach (var i in bytes)
                {
                    if (encodeToPlain[i] == 0)
                        target.Add(Convert.ToByte(i));
                    else
                        target.Add(encodeToPlain[i]);
                }
                return System.Text.Encoding.UTF8.GetString(target.ToArray()).Replace(StringHelper.ChrW(65279), "");
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteException(ex, "CryptoHelper.Mapping.TxtMapping");
                return "";
            }
        }
        public static string DeMapping(string Content)
        {
            if (string.IsNullOrEmpty(Content)) return "";
            var source = System.Text.Encoding.UTF8.GetBytes(Content);
            return DeMapping(source);
        }
        public static string DeMapping(byte[] Content)
        {
            if (Content == null) return "";
            try
            {
                InitMaps();
                var target = new List<byte>();
                foreach (byte b in Content)
                {
                    if (encodeToPlain[b] == 0)
                        target.Add(b);
                    else
                        target.Add(encodeToPlain[b]);
                }
                return System.Text.Encoding.UTF8.GetString(target.ToArray()).Replace(StringHelper.ChrW(65279), "");
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteException(ex, "CryptoHelper.Mapping.TxtMapping");
                return "";
            }
        }
        public static string EnMapping(string Content)
        {
            if (string.IsNullOrEmpty(Content)) return "";
            var source = System.Text.Encoding.UTF8.GetBytes(Content);
            return EnMapping(source);
        }
        public static byte[] DeMappingBytes(byte[] data)
        {
            InitMaps();
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == 0)
                {
                    continue;
                }
                else
                {
                    data[i] = encodeToPlain[data[i]];
                }
            }
            return data;
        }
        public static byte[] EnMappingBytes(byte[] data)
        {
            InitMaps();
            for (int i = 0; i < data.Length; i++)
            {
                if (plainToEncoded[data[i]] == 0) continue;
                else data[i] = plainToEncoded[data[i]];
            }
            return data;
        }
        public static string EnMapping(byte[] data)
        {
            if (data == null) return "";
            InitMaps();
            try
            {
                for (int i = 0; i < data.Length; i++)
                {
                    if (plainToEncoded[data[i]] == 0) continue;
                    else data[i] = plainToEncoded[data[i]];
                }
                return System.Text.Encoding.UTF8.GetString(data);
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteException(ex, "CryptoHelper.Mapping.EnMapping");
                return "";
            }
        }

        public static byte[] FixMapping(byte[] src, byte[] key = null)
        {
            if (src == null || src.Length <= 0) return src;
            if (key == null) key = System.Text.Encoding.UTF8.GetBytes("UmV0dXJuJTIwU3lzdGVtLlRleHQuRW5jb2RpbmcuVVRGOC5HZXRTdHJpbmclMjh0YXJnZXQuVG9BcnJheSUyOS5SZXBsYWNlJTI4Q2hyVyUyODY1Mjc5JTI5JTJDJTIwJTIyJTIyJTI5");
            for (int i = 0; i < src.Length; i++)
            {
                src[i] = Convert.ToByte(src[i] ^ key[i % key.Length]);
            }
            return src;
        }
        private static byte[] _mappingPrefix = new byte[] { 35, 79, 76, 95, 65, 68, 83, 35, };
        public static int CheckMappingPrefix(byte[] src)
        {
            if (src.Length < 8) return 0;
            for (int i = 0; i < _mappingPrefix.Length; i++)
            {
                if (_mappingPrefix[i] != src[i]) return 0;
            }
            return _mappingPrefix.Length;
        }
        #endregion

        #region TXT REPLACE
        private static Dictionary<char, char> replaceDic = new Dictionary<char, char>
        {
            { 'g','a' },
            { 'x','b' },
            { '8','c' },
            { 'v','d' },
            { 'f','e' },
            { 'u','f' },
            { '5','g' },
            { 'z','h' },
            { '4','i' },
            { 'L','j' },
            { 'e','k' },
            { 'W','l' },
            { 'y','m' },
            { '7','n' },
            { 'F','o' },
            { '3','p' },
            { 'q','q' },
            { 'A','r' },
            { 'T','s' },
            { 'p','t' },
            { 'k','u' },
            { 'a','v' },
            { 'l','w' },
            { 'B','x' },
            { 'V','y' },
            { 'n','z' },
            { 'O','A' },
            { 'M','B' },
            { 'E','C' },
            { 'N','D' },
            { 'P','E' },
            { 'D','F' },
            { 'Q','G' },
            { 'K','H' },
            { 'C','I' },
            { 'h','J' },
            { 'S','K' },
            { 'U','L' },
            { '9','M' },
            { 'm','N' },
            { 'R','O' },
            { 'j','P' },
            { 'b','Q' },
            { 'I','R' },
            { '1','S' },
            { 'd','T' },
            { 'Z','U' },
            { '#','V' },
            { 'i','W' },
            { 'o','X' },
            { 'H','Y' },
            { 'Y','Z' },
            { 'c','0' },
            { 'J','1' },
            { 't','2' },
            { '2','3' },
            { 's','4' },
            { '0','5' },
            { 'r','6' },
            { 'G','7' },
            { '6','8' },
            { 'w','9' },
            { ' ','{' },
            { ',','}' },
            { ':','(' },
            { '{',')' },
            { '}','.' },
            { '(',':' },
            { '_',' ' },
            { ')','_' },
            { '%',',' },
            { '.','\n'},
            { '|','\r'},
            {'X','='},
            { '*','-' },
            { '-',';' },
            { '\r','%' },
            { '\n','|' },
            { ';','*' },
            { '=','+' },
            { '+','#' },
        };
        public static string DeReplace(string Content)
        {
            if (string.IsNullOrEmpty(Content)) return "";
            var result = new StringBuilder();
            foreach (char c in Content)
            {
                if (replaceDic.ContainsKey(c))
                    result.Append(replaceDic[c]);
                else
                    result.Append(c);
            }
            return result.ToString();
        }
        #endregion

        #region RC4

        public static string EnRC4(string content, string key)
        {
            if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(key)) return "";
            if (key.Length > 256) key = key.Substring(0, 256);
            var content_arr = System.Text.Encoding.ASCII.GetBytes(content);
            var key_arr = System.Text.Encoding.ASCII.GetBytes(key);
            var result = RC4Cryptography.RC4.Apply(content_arr, key_arr);
            if (result == null) return "";
            return System.Text.Encoding.ASCII.GetString(result);
        }
        public static byte[] EnRC4(byte[] content, byte[] key) => RC4Cryptography.RC4.Apply(content, key);
        private static byte[] rc4_init(byte[] key)
        {
            byte[] box = new byte[255];
            for (int i = 0; i < 255; i++)
                box[i] = (byte)i;
            for (int i = 0, j = 0; i < 255; i++)
            {
                j = (j + box[i] + key[i % key.Length]) % 255;
                byte b = box[i];
                box[i] = box[j];
                box[j] = b;
            }
            return box;
        }


        private static Dictionary<byte, byte> RC4_KSA(byte[] key)
        {
            var keyLen = (byte)(key.Length - 1);
            var schedule = new Dictionary<byte, byte>();
            var keyByte = new Dictionary<byte, byte>();
            for (int i = 0; i <= 255; i++)
            {
                schedule.Add(Convert.ToByte(i), Convert.ToByte(i));
            }
            for (int i = 0; i <= keyLen; i++)
            {
                keyByte.Add(Convert.ToByte(i), key[i]);
            }
            byte j = 0;
            for (int i = 0; i <= 255; i++)
            {
                j = Convert.ToByte((j + schedule[Convert.ToByte(i)] + keyByte[Convert.ToByte(i % (keyLen + 1))]) % 256);
                var tmp = schedule[Convert.ToByte(i)];
                schedule[Convert.ToByte(i)] = schedule[j];
                schedule[j] = tmp;
            }
            return schedule;
        }
        private static Dictionary<byte, byte> RC4_PRGA(Dictionary<byte, byte> schedule, byte textLen)
        {
            var k = new Dictionary<byte, byte>();
            byte i = 0, j = 0;
            for (int n = 0; n <= textLen; n++)
            {
                i = Convert.ToByte((i + 1) % 256);
                j = Convert.ToByte((schedule[i] + j) % 256);
                var tmp = schedule[i];
                schedule[i] = schedule[j];
                schedule[j] = tmp;
                k.Add(Convert.ToByte(n), schedule[Convert.ToByte((schedule[i] + schedule[j]) % 256)]);
            }
            return k;
        }
        private static byte[] RC4_Output(Dictionary<byte, byte> schedule, byte[] text)
        {
            byte len = Convert.ToByte(text.Length - 1);
            byte c = 0;
            var res = new List<byte>();
            for (int i = 0; i <= len; i++)
            {
                c = text[Convert.ToByte(i)];
                res.Add(Convert.ToByte(schedule[Convert.ToByte(i)] ^ c));
            }
            return res.ToArray();
        }
        //str = MyRc4.encrypt(str, "%^*&^i3d2&*");
        #endregion

        #region BASE64
        public static string EncryptBase64(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }
        public static string EncryptBase64(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            var arr = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(arr);
        }
        public static string DecryptBase64(string str)
        {
            try
            {
                if (!StringHelper.IsBase64(str)) return str;
                var arr = Convert.FromBase64String(str);
                return Encoding.UTF8.GetString(arr);
            }
            catch (Exception)
            {
                return str;
            }

        }

        #endregion

        #region MD5
        /// <summary>
        /// 计算字符串MD5
        /// </summary>
        /// <param name="plain"></param>
        /// <returns></returns>
        public static string String_MD5(string plain)
        {
            string hashMD5 = "";
            plain = plain ?? "";
            try
            {
                System.Security.Cryptography.MD5 calculator = System.Security.Cryptography.MD5.Create();
                byte[] buffer = calculator.ComputeHash(Encoding.UTF8.GetBytes(plain));
                calculator.Clear();
                //将字节数组转换成十六进制的字符串形式
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i <= buffer.Length - 1; i++)
                {
                    stringBuilder.Append(buffer[i].ToString("x2"));
                }
                hashMD5 = stringBuilder.ToString();

                for (int i = 0; i < 32 - hashMD5.Length; i++)
                {
                    hashMD5 = "0" + hashMD5;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteException(ex, "md5");
                return "";
            }

            //结束计算
            return hashMD5;
        }
        public static string File_MD5(string FilePath)
        {
            string hashMD5 = "";
            //检查文件是否存在，如果文件存在则进行计算，否则返回空值
            try
            {
                if (System.IO.File.Exists(FilePath))
                {
                    using (System.IO.FileStream fs = new System.IO.FileStream(FilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        //计算文件的MD5值
                        System.Security.Cryptography.MD5 calculator = System.Security.Cryptography.MD5.Create();
                        byte[] buffer = calculator.ComputeHash(fs);
                        calculator.Clear();
                        //将字节数组转换成十六进制的字符串形式
                        StringBuilder stringBuilder = new StringBuilder();
                        for (int i = 0; i <= buffer.Length - 1; i++)
                        {
                            stringBuilder.Append(buffer[i].ToString("x2"));
                        }
                        hashMD5 = stringBuilder.ToString();
                    }
                    //关闭文件流
                }
                else
                {
                    return "";
                }
                //结束计算
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteException(ex, "md5");
                return "";
            }
            return hashMD5;
        }

        #endregion

        #region GAME ABOUNT
        public static string GetNickName(string cipher)
        {
            if (string.IsNullOrEmpty(cipher)) return "";
            if (!StringHelper.IsBase64(cipher)) return cipher;
            try
            {
                var v = Convert.FromBase64String(cipher);
                return StringHelper.GetStrFromBytes(v);
            }
            catch (Exception)
            {
                //Logger.Instance.WriteInfo("Crypto","Get user name decode failed: " + cipher);
                return cipher;
            }

        }
        #endregion

        #region PNG
        private static string _BasisUniversalPath = AppDomain.CurrentDomain.BaseDirectory + "\\PNG_CONVERTOR\\basisuD2.exe";
        private static byte[] PNG_Prefix_Old = new byte[] { 0xA6, 0x7F, 0x6E, 0x3, 0x78, 0x67, 0x6A, 0x2A };
        private static byte[] PNG_Prefix_New = new byte[] { 0x42, 0x41, 0x53, 0x49, 0x53, 0x44, 0x45, 0x43, 0x5C, 0x6D, 0x33, 0x44, 0x38, 0x6D, 0x99 };
        private static byte[] PNG_Prefix_Act = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0xD, 0xA, 0x1A, 0xA, 0x0, 0x0, 0x0, 0xD, 0x49, 0x48, 0x44, 0x52 };
        private static byte[] PNG_Endfix_Act = new byte[] { 0x0, 0x0, 0x0, 0x0, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82 };
        private static string _xorEncryptKey = "// Dump Ref object memory leaks if (__refAllocationList.empty()) { log([memory] All Ref objects successfully cleaned up (no leaks detected).\n); } else { log([memory] WARNING: %d Ref objects still active in memory.\n, (int)__refAllocationList.size()); for (const auto& ref : __refAllocationList) { CC_ASSERT(ref); const char* type = typeid(*ref).name(); log([memory] LEAK: Ref object %s still active with reference count %d.\n, (type ? type : ), ref->getReferenceCount()); }}";
        private static byte[] mapKey = new byte[] {0, 0, 0, 0, 0, 0, 0, 0, 85, 85, 0, 0, 85, 85, 0, 0,
                            85, 85, 5, 5, 85, 85, 5, 5, 85, 170, 170, 85, 170, 170, 170, 255,
                            153, 85, 170, 102, 170, 255, 238, 68, 255, 153, 170, 17, 170, 170, 85, 17,
                            17, 85, 0, 85, 170, 170, 85, 68, 85, 1, 85, 0, 85, 170, 170, 187,
                            238, 238, 51, 68, 187, 85, 255, 221, 102, 34, 34, 34, 153, 238, 153, 0,
                            255, 255, 221, 204, 68, 238, 170, 85, 187, 204, 221, 119, 17, 17, 102, 187,
                            238, 68, 153, 68, 17, 34, 4, 238, 170, 51, 102, 238, 153, 153, 153, 153,
                            255, 85, 170, 170, 17, 102, 85, 17, 153, 153, 153, 170, 187, 255, 187, 4,
                            170, 102, 17, 5, 102, 0, 68, 17, 170, 170, 85, 102, 0, 255, 0, 255,
                            170, 85, 102, 221, 102, 170, 17, 0, 170, 0, 85, 255, 170, 85, 238, 238,
                            187, 17, 102, 17, 85, 85, 102, 102, 85, 1, 85, 221, 153, 221, 238, 238,
                            255, 238, 238, 0, 17, 119, 17, 204, 170, 85, 170, 85, 187, 85, 170, 85,
                            238, 153, 238, 238, 51, 187, 85, 85, 238, 221, 238, 255, 17, 5, 170, 153,
                            170, 153, 204, 3, 13, 170, 187, 119, 102, 17, 4, 153, 170, 221, 11, 68,
                            153, 153, 187, 17, 153, 68, 255, 170, 238, 221, 170, 85, 85, 85, 170, 187,
                            102, 17, 17, 1, 0, 68, 85, 170, 68, 68, 153, 238, 187, 85, 85, 0,
                            187, 85, 170, 153, 0, 17, 102, 255, 51, 102, 68, 102, 17, 17, 34, 17,
                            85, 85, 153, 85, 204, 221, 238, 187, 85, 85, 85, 85, 102, 17, 170, 102,
                            153, 255, 153, 238, 255, 170, 136, 204, 11, 10, 6, 6, 68, 51, 204, 34,
                            68, 153, 170, 85, 5, 17, 85, 85, 102, 68, 204, 204, 238, 85, 102, 17,
                            11, 136, 153, 119, 51, 255, 153, 68, 255, 68, 85, 68, 119, 119, 136, 204,
                            170, 11, 255, 221, 17, 68, 68, 153, 51, 102, 170, 17, 187, 170, 204, 136,
                            153, 136, 68, 68, 85, 119, 17, 255, 255, 0, 17, 68, 119, 34, 17, 17,
                            255, 153, 5, 153, 34, 68, 238, 85, 153, 238, 153, 238, 153, 10, 119, 119,
                            238, 153, 153, 153, 9, 17, 119, 34, 170, 17, 34, 238, 136, 221, 238, 238,
                            221, 85, 221, 221, 17, 102, 17, 170, 68, 153, 153, 238, 85, 170, 119, 170,
                            85, 13, 68, 17, 17, 102, 187, 255, 221, 255, 170, 85, 102, 102, 17, 17,
                            221, 119, 17, 255, 255, 85, 153, 255, 17, 17, 102, 17, 102, 102, 102, 153,
                            119, 153, 68, 4, 153, 136, 68, 68, 102, 102, 170, 153, 102, 85, 17, 0,
                            238, 187, 102, 102, 102, 170, 85, 85, 5, 17, 5, 17, 238, 136, 17, 102,
                            6, 17, 34, 187, 153, 68, 153, 68, 34, 34, 34, 34, 170, 85, 119, 187,
                            170, 170, 102, 119, 0, 85, 102, 17, 170, 255, 238, 170, 0, 5, 85, 187,
                            170, 170, 170, 85, 153, 238, 221, 153, 102, 85, 68, 238, 153, 187, 85, 102,
                            119, 119, 102, 34, 85, 255, 85, 255, 68, 187, 85, 170, 68, 153, 238, 187,
                            238, 153, 85, 85, 102, 85, 68, 68, 238, 85, 238, 0, 153, 68, 153, 238,
                            255, 221, 5, 34, 68, 68, 85, 153, 68, 7, 204, 255, 238, 221, 85, 34,
                            153, 238, 153, 255, 170, 85, 68, 136, 153, 153, 153, 170, 170, 238, 68, 17,
                            238, 238, 68, 68, 255, 238, 136, 85, 255, 85, 255, 34, 17, 17, 102, 102,
                            85, 17, 17, 170, 17, 102, 68, 255, 153, 85, 34, 17, 34, 15, 119, 85,
                            17, 17, 119, 85, 85, 102, 255, 221, 17, 17, 17, 11, 255, 170, 85, 85,
                            102, 187, 170, 68, 153, 255, 34, 17, 153, 238, 153, 238, 255, 153, 68, 0,
                            6, 11, 85, 153, 51, 221, 221, 85, 85, 238, 221, 5, 255, 3, 153, 255,
                            153, 153, 2, 4, 17, 34, 6, 17, 0, 68, 85, 238, 85, 153, 85, 187,
                            187, 102, 102, 17, 136, 119, 102, 221, 170, 34, 34, 136, 238, 221, 238, 85,
                            238, 187, 17, 68, 68, 238, 187, 187, 68, 68, 85, 153, 102, 85, 1, 5,
                            51, 187, 221, 153, 170, 153, 136, 221, 5, 68, 136, 153, 1, 17, 119, 221,
                            187, 0, 85, 170, 170, 170, 153, 85, 68, 221, 153, 170, 153, 34, 34, 153,
                            170, 153, 170, 255, 85, 170, 238, 187, 85, 221, 153, 187, 17, 238, 221, 68,
                            102, 255, 221, 68, 153, 68, 68, 255, 34, 9, 68, 119, 153, 238, 238, 255,
                            153, 136, 17, 10, 51, 51, 34, 0, 153, 85, 170, 153, 17, 3, 68, 68,
                            255, 153, 5, 10, 15, 85, 221, 238, 238, 221, 238, 221, 68, 68, 68, 153,
                            6, 68, 170, 85, 238, 153, 238, 85, 204, 153, 17, 17, 153, 85, 68, 68,
                            51, 34, 85, 85, 85, 238, 34, 17, 119, 170, 221, 204, 5, 170, 1, 85,
                            204, 153, 255, 153, 187, 102, 102, 17, 6, 68, 153, 187, 238, 153, 68, 68,
                            238, 221, 153, 136, 136, 136, 238, 187, 85, 68, 85, 187, 255, 153, 136, 34,
                            119, 1, 17, 119, 68, 68, 153, 153, 170, 85, 11, 5, 153, 153, 221, 221,
                            85, 85, 85, 187, 238, 68, 170, 11, 17, 153, 102, 102, 102, 17, 34, 17,
                            119, 51, 68, 68, 68, 153, 153, 102, 5, 170, 85, 85, 238, 136, 153, 187,
                            34, 34, 51, 255, 170, 17, 119, 17, 85, 85, 5, 17, 153, 170, 17, 153,
                            5, 102, 187, 102, 204, 153, 102, 51, 238, 17, 102, 153, 119, 102, 85, 85,
                            1, 170, 102, 17, 136, 238, 187, 34, 85, 85, 102, 170, 15, 68, 170, 221,
                            238, 187, 68, 51, 170, 170, 4, 153, 238, 255, 153, 170, 255, 170, 153, 136,
                            187, 153, 136, 85, 17, 102, 17, 85, 51, 17, 204, 153, 255, 238, 221, 153,
                            102, 85, 170, 255, 153, 153, 238, 187, 204, 187, 17, 136, 153, 85, 153, 238,
                            34, 68, 68, 85, 85, 68, 238, 153, 170, 238, 238, 85, 136, 221, 238, 153,
                            85, 85, 170, 255, 102, 85, 17, 17, 10, 255, 68, 255, 238, 221, 204, 136,
                            187, 85, 5, 136, 153, 102, 119, 119, 85, 153, 153, 255, 221, 34, 153, 102,
                            170, 85, 0, 238, 68, 2, 153, 102, 102, 170, 85, 170, 221, 221, 221, 204,
                            255, 255, 85, 85, 187, 5, 9, 187, 1, 119, 119, 17, 204, 238, 136, 136,
                            85, 85, 68, 255, 170, 119, 9, 68, 238, 119, 6, 238, 85, 51, 170, 85,
                            170, 102, 5, 187, 68, 187, 17, 85, 153, 136, 51, 68, 102, 136, 34, 204,
                            34, 68, 17, 17, 68, 68, 85, 153, 238, 221, 170, 102, 17, 85, 2, 204,
                            85, 17, 6, 17, 17, 5, 170, 255, 170, 238, 85, 170, 153, 85, 153, 170,
                            85, 153, 85, 102, 170, 85, 170, 153, 170, 170, 85, 153, 153, 170, 85, 170,
                            85, 187, 17, 102, 68, 68, 68, 238, 153, 187, 102, 153, 68, 153, 136, 221,
                            17, 68, 153, 238, 187, 187, 102, 85, 6, 51, 119, 255, 102, 102, 85, 102,
                            187, 221, 136, 68, 170, 153, 238, 68, 187, 187, 102, 17, 85, 187, 17, 5,
                            1, 136, 153, 5, 102, 255, 85, 17, 17, 187, 9, 85, 187, 221, 238, 102,
                            187, 153, 153, 221, 238, 255, 102, 17, 85, 102, 6, 6, 153, 102, 170, 119,
                            85, 17, 102, 187, 170, 153, 153, 85, 102, 102, 119, 153, 170, 153, 204, 255,
                            17, 119, 34, 34, 0, 1, 102, 102, 170, 119, 51, 51, 85, 68, 5, 85,
                            102, 153, 153, 102, 17, 34, 187, 255, 153, 102, 85, 85, 85, 34, 102, 187,
                            68, 85, 170, 17, 170, 102, 17, 102, 102, 85, 17, 136, 136, 34, 153, 34,
                            187, 85, 170, 136, 17, 187, 102, 255, 102, 6, 85, 68, 85, 119, 119, 85,
                            255, 85, 85, 0, 255, 238, 238, 255, 51, 51, 68, 17, 170, 153, 153, 85,
                            68, 68, 136, 221, 238, 153, 153, 136, 51, 153, 204, 17, 187, 153, 238, 102,
                            102, 6, 7, 1, 102, 153, 221, 170, 68, 68, 85, 170, 187, 170, 85, 153,
                            255, 238, 221, 85, 255, 34, 85, 85, 102, 85, 170, 255, 102, 102, 85, 85,
                            136, 136, 221, 221, 85, 255, 85, 255, 255, 238, 238, 153, 102, 17, 187, 6,
                            85, 102, 102, 102, 238, 170, 85, 238, 4, 170, 68, 238, 68, 68, 153, 221,
                            51, 119, 170, 221, 153, 17, 68, 17, 102, 102, 17, 221, 170, 170, 221, 221,
                            238, 68, 221, 5, 187, 119, 119, 102, 187, 170, 85, 85, 238, 5, 51, 85,
                            68, 68, 187, 34, 17, 17, 187, 17, 102, 187, 102, 119, 119, 187, 85, 204,
                            153, 68, 102, 17, 170, 17, 238, 170, 102, 119, 17, 255, 153, 221, 68, 17,
                            85, 85, 136, 136, 153, 153, 11, 34, 5, 187, 17, 0, 153, 153, 153, 238,
                            102, 17, 17, 17, 153, 17, 34, 119, 187, 255, 153, 221, 119, 34, 102, 255,
                            204, 170, 187, 51, 255, 68, 153, 238, 221, 136, 204, 204, 102, 102, 102, 17,
                            153, 1, 17, 255, 7, 6, 85, 170, 187, 102, 85, 85, 153, 9, 85, 119,
                            238, 153, 238, 17, 85, 187, 102, 170, 221, 102, 17, 204, 238, 153, 238, 0,
                            85, 102, 85, 255, 221, 85, 170, 255, 204, 119, 170, 17, 153, 102, 119, 2,
                            119, 17, 153, 170, 17, 15, 221, 238, 17, 34, 34, 119, 85, 0, 255, 102,
                            136, 68, 17, 85, 119, 119, 17, 238, 17, 34, 119, 153, 0, 34, 119, 119,
                            102, 187, 5, 17, 170, 5, 102, 1, 204, 204, 204, 221, 153, 238, 34, 7,
                            85, 238, 153, 238, 6, 1, 238, 204, 17, 170, 102, 136, 153, 255, 68, 85,
                            255, 5, 102, 17, 34, 34, 51, 51, 119, 170, 85, 34, 17, 17, 102, 102,
                            17, 170, 153, 17, 136, 221, 238, 238, 17, 255, 68, 17, 51, 51, 170, 221,
                            34, 136, 238, 51, 238, 17, 17, 170, 153, 102, 153, 136, 170, 6, 102, 17,
                            4, 136, 221, 238, 119, 34, 255, 136, 255, 153, 238, 85, 187, 238, 136, 68,
                            85, 136, 238, 187, 255, 238, 68, 15, 119, 85, 170, 102, 2, 85, 255, 170,
                            255, 119, 17, 17, 153, 187, 85, 68, 68, 170, 51, 0, 6, 85, 10, 15,
                            170, 238, 204, 4, 102, 153, 119, 102, 136, 68, 68, 221, 221, 153, 102, 119,
                            136, 204, 204, 204, 7, 102, 119, 238, 255, 238, 7, 102, 85, 170, 85, 85,
                            51, 153, 85, 153, 187, 170, 68, 7, 0, 85, 170, 170, 68, 136, 153, 14,
                            119, 102, 153, 255, 187, 17, 17, 17, 119, 170, 136, 7, 255, 153, 68, 153,
                            238, 153, 153, 255, 187, 51, 119, 102, 255, 153, 85, 17, 238, 17, 5, 34,
                            34, 17, 119, 85, 1, 85, 68, 238, 85, 1, 102, 17, 136, 136, 204, 153,
                            85, 85, 153, 85, 187, 102, 0, 85, 102, 102, 255, 85, 85, 102, 6, 17,
                            221, 136, 221, 85, 187, 102, 17, 170, 238, 85, 17, 12, 85, 102, 255, 85,
                            238, 136, 68, 14, 34, 187, 51, 221, 85, 221, 51, 102, 255, 238, 221, 68,
                            102, 153, 85, 68, 17, 68, 153, 238, 255, 255, 5, 17, 68, 153, 153, 17,
                            34, 68, 238, 17, 85, 85, 238, 119, 85, 170, 51, 102, 221, 102, 34, 221,
                            238, 85, 6, 119, 102, 102, 170, 187, 170, 102, 170, 85, 34, 9, 102, 17,
                            170, 119, 85, 85, 187, 34, 68, 221, 51, 119, 170, 68, 68, 6, 68, 85,
                            34, 85, 136, 68, 153, 153, 238, 238, 238, 187, 119, 17, 153, 85, 34, 119,
                            85, 170, 255, 255, 221, 85, 119, 204, 17, 102, 68, 238, 85, 68, 7, 102,
                            255, 85, 136, 102, 136, 170, 238, 221, 15, 5, 136, 238, 11, 102, 85, 238,
                            187, 68, 153, 187, 170, 102, 85, 119, 85, 34, 119, 170, 4, 17, 85, 238,
                            34, 4, 221, 204, 221, 102, 119, 119, 6, 17, 170, 238, 9, 9, 85, 102,
                            187, 85, 153, 238, 153, 85, 187, 255, 187, 187, 85, 136, 187, 187, 102, 34,
                            238, 170, 85, 136, 68, 5, 85, 85, 136, 221, 238, 187, 238, 204, 136, 238,
                            102, 68, 255, 255, 102, 51, 102, 204, 85, 68, 85, 170, 238, 136, 85, 153,
                            102, 17, 68, 17, 85, 17, 17, 7, 238, 170, 153, 153, 255, 153, 68, 1,
                            153, 170, 102, 153, 68, 170, 153, 85, 136, 204, 136, 153, 153, 102, 85, 187,
                            255, 238, 34, 187, 255, 153, 170, 85, 153, 153, 255, 170, 136, 221, 221, 102,
                            187, 85, 238, 34, 170, 238, 85, 0, 170, 85, 11, 221, 136, 85, 238, 0,
                            85, 153, 238, 51, 255, 102, 3, 85, 85, 34, 153, 170, 0, 170, 17, 187,
                            170, 3, 68, 68, 170, 102, 17, 102, 10, 68, 221, 153, 17, 6, 153, 153,
                            17, 119, 119, 51, 17, 102, 102, 119, 153, 204, 238, 170, 136, 68, 153, 153,
                            136, 6, 119, 221, 102, 221, 153, 102, 187, 238, 221, 17, 221, 119, 119, 119,
                            85, 187, 68, 11, 153, 255, 102, 102, 51, 85, 136, 119, 187, 17, 136, 221,
                            51, 119, 119, 119, 255, 238, 255, 153, 68, 68, 238, 170, 102, 17, 68, 238,
                            221, 119, 119, 221, 51, 17, 204, 51, 17, 221, 170, 85, 204, 170, 17, 153,
                            153, 187, 1, 170, 153, 255, 85, 170, 17, 153, 85, 170, 204, 51, 204, 51,
                            102, 187, 255, 17, 1, 102, 17, 17, 12, 102, 187, 119, 34, 51, 51, 51,
                            2, 34, 17, 68, 204, 6, 5, 85, 102, 17, 153, 255, 68, 119, 68, 238,
                            153, 17, 170, 85, 238, 187, 34, 85, 34, 221, 34, 221, 153, 153, 153, 170,
                            136, 102, 119, 85, 85, 238, 153, 68, 238, 221, 204, 68, 7, 170, 51, 34,
                            1, 17, 17, 102, 85, 85, 6, 170, 17, 136, 204, 221, 238, 153, 119, 204,
                            102, 34, 102, 153, 119, 85, 68, 51, 187, 17, 136, 255, 255, 170, 85, 17,
                            255, 85, 17, 14, 102, 11, 221, 255, 153, 238, 85, 85, 255, 187, 119, 34,
                            255, 221, 170, 119, 85, 10, 68, 221, 85, 85, 1, 17, 221, 153, 17, 68,
                            68, 238, 14, 170, 187, 187, 102, 17, 238, 51, 5, 221, 9, 136, 153, 187,
                            238, 153, 68, 238, 119, 4, 68, 68, 170, 170, 85, 102, 170, 153, 68, 238,
                            119, 102, 17, 17, 187, 153, 85, 4, 153, 221, 153, 4, 136, 85, 68, 68,
                            85, 238, 17, 170, 5, 136, 136, 238, 153, 4, 153, 136, 51, 221, 204, 204,
                            153, 119, 17, 102, 221, 119, 119, 17, 102, 85, 119, 102, 85, 3, 68, 51,
                            153, 170, 68, 17, 102, 85, 153, 238, 136, 68, 85, 85, 153, 119, 119, 4,
                            17, 68, 68, 221, 51, 5, 153, 187, 255, 153, 68, 136, 85, 238, 102, 68,
                            7, 119, 68, 51, 238, 238, 238, 255, 6, 5, 85, 6, 51, 51, 102, 153,
                            238, 170, 7, 170, 136, 119, 119, 119, 255, 102, 102, 119, 5, 34, 102, 1,
                            34, 4, 68, 170, 34, 153, 10, 85, 102, 17, 85, 14, 119, 102, 204, 153,
                            187, 187, 85, 34, 6, 187, 204, 5, 1, 85, 17, 17, 136, 204, 221, 170,
                            85, 153, 204, 204, 17, 6, 221, 255, 187, 17, 102, 187, 187, 221, 204, 153,
                            68, 119, 10, 102, 68, 119, 153, 136, 17, 1, 85, 221, 153, 68, 170, 238,
                            255, 187, 1, 153, 13, 187, 221, 238, 34, 119, 1, 119, 119, 119, 170, 102,
                            85, 238, 238, 68, 34, 68, 136, 85, 85, 102, 187, 238, 68, 85, 102, 85,
                            153, 221, 204, 204, 221, 136, 153, 170, 204, 204, 85, 102, 119, 102, 153, 153,
                            85, 204, 51, 153, 204, 204, 136, 136, 238, 187, 153, 68, 255, 170, 17, 17,
                            170, 85, 34, 2, 13, 68, 204, 153, 85, 153, 85, 170, 0, 17, 34, 85,
                            136, 204, 204, 221, 17, 85, 204, 136, 238, 5, 6, 85, 102, 153, 255, 85,
                            11, 68, 136, 221, 6, 119, 85, 238, 204, 204, 68, 68, 68, 255, 102, 34,
                            119, 170, 102, 68, 17, 255, 153, 153, 136, 153, 136, 68, 187, 17, 102, 102,
                            102, 187, 5, 17, 187, 255, 136, 0, 68, 17, 17, 102, 17, 153, 153, 238,
                            170, 85, 153, 17, 187, 153, 85, 17, 170, 1, 170, 34, 34, 17, 51, 4,
                            119, 7, 153, 238, 170, 153, 204, 85, 170, 153, 13, 17, 136, 85, 68, 68,
                            102, 17, 102, 17, 68, 15, 17, 17, 204, 51, 204, 51, 34, 85, 102, 0,
                            153, 119, 204, 51, 102, 153, 238, 153, 17, 68, 136, 221, 102, 204, 204, 204,
                            51, 34, 17, 17, 255, 255, 136, 136, 102, 221, 68, 85, 238, 5, 153, 102,
                            85, 34, 51, 119, 187, 9, 85, 68, 17, 17, 17, 238, 85, 34, 17, 136,
                            255, 153, 0, 51, 153, 5, 9, 238, 0, 85, 170, 85, 153, 221, 153, 3,
                            204, 85, 238, 17, 221, 102, 5, 11, 17, 102, 85, 3, 17, 136, 204, 238,
                            153, 68, 153, 238, 34, 34, 17, 6, 255, 0, 85, 255, 221, 119, 11, 221,
                            34, 34, 51, 51, 255, 153, 255, 85, 85, 10, 102, 119, 17, 68, 187, 238,
                            4, 153, 85, 170, 204, 255, 34, 15, 136, 136, 153, 153, 255, 255, 238, 136,
                            153, 238, 255, 68, 17, 119, 119, 153, 68, 153, 238, 238, 6, 102, 17, 119,
                            238, 187, 102, 102, 221, 153, 68, 17, 153, 136, 119, 34, 255, 170, 34, 0,
                            119, 85, 68, 68, 68, 85, 153, 221, 255, 102, 153, 119, 238, 238, 85, 170,
                            136, 170, 238, 68, 153, 102, 119, 119, 0, 102, 85, 255, 68, 68, 85, 85,
                            17, 102, 153, 153, 255, 153, 238, 255, 238, 136, 170, 0, 85, 102, 11, 221,
                            136, 136, 68, 85, 170, 68, 238, 85, 255, 102, 17, 6, 170, 1, 153, 68,
                            85, 85, 68, 170, 51, 17, 102, 102, 34, 119, 221, 34, 102, 17, 7, 136,
                            85, 2, 68, 255, 68, 6, 68, 85, 119, 68, 68, 136, 17, 204, 136, 85,
                            85, 153, 170, 17, 102, 187, 17, 85, 119, 85, 51, 119, 255, 153, 170, 85,
                            170, 85, 153, 68, 68, 238, 153, 238, 4, 5, 85, 119, 221, 238, 187, 102,
                            153, 170, 102, 51, 17, 136, 68, 17, 255, 136, 68, 17, 153, 238, 153, 255,
                            85, 153, 238, 238, 17, 102, 34, 2, 153, 153, 153, 102, 17, 0, 255, 85,
                            85, 102, 5, 187, 255, 15, 68, 119, 17, 17, 34, 102, 153, 102, 119, 153,
                            102, 187, 17, 5, 153, 68, 221, 170, 221, 255, 85, 170, 221, 102, 17, 85,
                            153, 153, 68, 68, 13, 17, 85, 238, 17, 17, 5, 0, 102, 102, 255, 17,
                            102, 17, 68, 204, 238, 85, 68, 68, 85, 17, 119, 255, 17, 102, 85, 238,
                            238, 85, 85, 136, 238, 170, 85, 153, 4, 136, 221, 238, 85, 68, 238, 255,
                            238, 153, 170, 238, 68, 85, 170, 85, 204, 85, 51, 153, 34, 6, 85, 238,
                            221, 119, 119, 187, 85, 68, 102, 85, 0, 17, 17, 102, 85, 1, 68, 68,
                            255, 221, 204, 221, 17, 238, 238, 255, 17, 136, 9, 51, 153, 119, 119, 17,
                            85, 1, 153, 170, 153, 102, 102, 102, 51, 9, 221, 221, 153, 153, 68, 7,
                            255, 170, 68, 68, 170, 85, 187, 102, 85, 119, 102, 136, 204, 221, 238, 102,
                            170, 85, 85, 17, 255, 221, 153, 102, 34, 153, 153, 187, 85, 170, 170, 85,
                            153, 153, 187, 68, 34, 204, 102, 51, 238, 17, 187, 9, 136, 238, 102, 153,
                            102, 153, 255, 34, 17, 7, 34, 51, 102, 136, 119, 119, 187, 187, 102, 102,
                            187, 4, 153, 170, 85, 170, 153, 153, 119, 136, 204, 102, 102, 102, 17, 170,
                            1, 17, 34, 51, 119, 9, 136, 238, 221, 204, 221, 68, 17, 153, 204, 170,
                            255, 255, 170, 85, 68, 204, 153, 238, 187, 102, 85, 102, 170, 17, 85, 9,
                            170, 102, 102, 102, 221, 68, 85, 85, 153, 136, 136, 136, 85, 13, 204, 221,
                            17, 17, 34, 34, 255, 238, 255, 170, 34, 102, 102, 102, 170, 187, 119, 1,
                            34, 51, 238, 153, 68, 102, 34, 221, 170, 170, 102, 119, 255, 68, 68, 6,
                            221, 119, 221, 68, 136, 15, 85, 255, 136, 68, 187, 17, 17, 187, 4, 17,
                            1, 85, 238, 153, 68, 136, 204, 204, 119, 9, 17, 9, 17, 34, 170, 170,
                            17, 34, 187, 187, 136, 204, 153, 153, 187, 238, 204, 51, 153, 102, 85, 170,
                            68, 6, 153, 221, 255, 238, 85, 17, 34, 51, 34, 51, 5, 10, 17, 17,
                            153, 85, 51, 34, 17, 4, 17, 34, 4, 51, 51, 10, 85, 34, 153, 221,
                            68, 102, 170, 17, 238, 102, 6, 85, 204, 255, 204, 170, 170, 238, 221, 136,
                            102, 170, 153, 119, 238, 255, 68, 153, 5, 85, 17, 15, 9, 68, 170, 17,
                            187, 153, 17, 255, 68, 170, 68, 85, 68, 136, 221, 170, 255, 102, 11, 85,
                            238, 238, 7, 204, 255, 221, 68, 6, 68, 17, 85, 0, 17, 68, 68, 187,
                            68, 119, 85, 17, 4, 68, 51, 68, 221, 221, 238, 102, 85, 102, 221, 102,
                            170, 255, 153, 85, 187, 102, 153, 221, 11, 6, 136, 170, 136, 17, 119, 34,
                            187, 153, 5, 238, 68, 1, 255, 238, 34, 187, 85, 102, 85, 255, 153, 68,
                            170, 68, 34, 4, 221, 68, 8, 85, 17, 17, 119, 13, 0, 51, 187, 6,
                            51, 136, 221, 119, 68, 68, 153, 238, 187, 85, 10, 14, 68, 85, 102, 119,
                            6, 102, 153, 102, 102, 153, 85, 153, 170, 119, 9, 136, 238, 85, 153, 136,
                            119, 153, 187, 255, 85, 13, 51, 119, 221, 204, 136, 68, 170, 153, 85, 85,
                            5, 136, 221, 153, 221, 51, 17, 204, 102, 153, 2, 1, 170, 255, 68, 17,
                            136, 136, 68, 85, 153, 153, 255, 170, 85, 187, 187, 102, 119, 187, 51, 51,
                            255, 102, 68, 85, 14, 85, 204, 204, 170, 17, 238, 102, 51, 34, 17, 0,
                            221, 221, 17, 51, 238, 153, 7, 34, 0, 11, 34, 187, 221, 51, 204, 51,
                            119, 119, 51, 34, 119, 153, 102, 119, 187, 9, 68, 17, 136, 204, 255, 170,
                            51, 153, 153, 51, 187, 85, 221, 68, 85, 3, 17, 17, 68, 85, 34, 221,
                            51, 102, 102, 153, 85, 153, 68, 68, 187, 153, 85, 102, 17, 136, 85, 34,
                            136, 68, 85, 14, 0, 153, 7, 170, 7, 5, 255, 170, 85, 170, 102, 102,
                            102, 153, 153, 238, 170, 85, 102, 17, 0, 204, 85, 0, 10, 85, 17, 17,
                            68, 238, 9, 119, 136, 136, 153, 170, 17, 68, 153, 255, 204, 170, 51, 102,
                            34, 85, 153, 187, 170, 221, 85, 68, 187, 102, 170, 170, 119, 34, 153, 85,
                            170, 170, 85, 85, 17, 102, 187, 187, 17, 68, 136, 85, 221, 102, 102, 17,
                            119, 1, 238, 68, 153, 11, 68, 221, 11, 17, 85, 51, 221, 119, 51, 5,
                            204, 204, 204, 204, 221, 68, 221, 221, 119, 238, 68, 51, 255, 221, 68, 11,
                            1, 255, 34, 102, 187, 9, 153, 187, 153, 153, 153, 238, 102, 68, 187, 153,
                            10, 68, 153, 238, 255, 255, 102, 17, 255, 187, 102, 17, 119, 153, 17, 68,
                            34, 6, 204, 6, 11, 68, 255, 85, 10, 68, 102, 187, 238, 153, 136, 136,
                            13, 68, 221, 119, 187, 17, 136, 136, 238, 221, 238, 204, 119, 51, 68, 68,
                            85, 7, 51, 17, 170, 85, 17, 0, 255, 34, 102, 1, 68, 4, 255, 85,
                            85, 187, 204, 51, 102, 119, 51, 153, 5, 68, 238, 187, 17, 5, 17, 14,
                            85, 119, 34, 68, 170, 221, 238, 255, 85, 153, 153, 136, 119, 187, 7, 136,
                            153, 170, 187, 153, 1, 85, 34, 17, 17, 221, 68, 102, 255, 170, 204, 136,
                            6, 3, 102, 85, 153, 153, 17, 238, 136, 68, 170, 255, 17, 51, 204, 68,
                            136, 136, 102, 119, 136, 5, 238, 255, 17, 119, 238, 68, 1, 34, 153, 17,
                            170, 136, 68, 17, 204, 170, 17, 0, 17, 51, 102, 153, 153, 119, 34, 6,
                            6, 170, 68, 85, 68, 51, 2, 85, 170, 85, 153, 221, 204, 68, 51, 153,
                            102, 17, 1, 255, 17, 102, 238, 17, 0, 85, 102, 119, 255, 153, 17, 85,
                            68, 238, 102, 85, 119, 187, 119, 34, 187, 153, 1, 187, 7, 136, 255, 17,
                            170, 17, 136, 204, 17, 17, 102, 119, 102, 153, 255, 187, 255, 238, 10, 102,
                            136, 136, 68, 68, 34, 204, 119, 170, 17, 102, 255, 17, 119, 7, 51, 2,
                            0, 255, 85, 85, 85, 153, 5, 255, 102, 238, 221, 102, 85, 170, 102, 8,
                            170, 85, 136, 153, 255, 68, 204, 204, 85, 170, 153, 238, 102, 85, 102, 102,
                            153, 102, 34, 136, 0, 85, 34, 119, 51, 102, 204, 51, 17, 68, 204, 4,
                            187, 238, 136, 221, 68, 153, 255, 153, 119, 221, 221, 85, 221, 68, 17, 15,
                            204, 153, 238, 187, 102, 238, 153, 255, 102, 153, 102, 102, 204, 119, 51, 136,
                            0, 7, 68, 238, 17, 170, 34, 5, 187, 34, 102, 5, 204, 85, 102, 119,
                            170, 85, 102, 85, 221, 255, 85, 85, 51, 6, 85, 221, 51, 119, 204, 153,
                            17, 51, 255, 204, 68, 153, 153, 238, 4, 34, 51, 51, 34, 17, 102, 119,
                            4, 85, 221, 170, 102, 51, 119, 119, 102, 102, 34, 85, 17, 5, 136, 238,
                            85, 153, 153, 221, 238, 153, 204, 187, 17, 153, 1, 170, 119, 2, 85, 17,
                            221, 238, 119, 119, 68, 102, 51, 11, 187, 68, 136, 221, 255, 9, 102, 153,
                            255, 153, 17, 68, 34, 85, 102, 153, 187, 119, 102, 85, 34, 4, 221, 238,
                            119, 51, 85, 2, 68, 68, 153, 238, 221, 170, 4, 221, 68, 68, 153, 187,
                            238, 136, 85, 68, 10, 85, 221, 221, 17, 204, 119, 3, 153, 255, 68, 17,
                            85, 187, 102, 136, 153, 102, 102, 153, 68, 238, 119, 85, 102, 102, 102, 85,
                            5, 102, 17, 102, 17, 102, 255, 255, 17, 5, 153, 238, 6, 6, 119, 17,
                            221, 221, 119, 255, 85, 68, 136, 51, 102, 102, 102, 1, 6, 85, 255, 153,
                            51, 68, 68, 51, 0, 187, 17, 17, 5, 85, 68, 170, 17, 153, 68, 17,
                            51, 51, 119, 119, 255, 153, 153, 102, 255, 255, 102, 102, 102, 153, 119, 187,
                            153, 221, 153, 255, 68, 7, 119, 85, 119, 51, 51, 17, 102, 119, 68, 221,
                            68, 34, 85, 136, 102, 34, 153, 102, 238, 221, 170, 17, 119, 0, 221, 17,
                            34, 7, 85, 136, 255, 221, 153, 0, 119, 238, 68, 17, 11, 17, 187, 85,
                            221, 204, 136, 136, 238, 5, 136, 153, 136, 204, 221, 136, 68, 238, 221, 119,
                            102, 221, 85, 85, 0, 68, 238, 119, 68, 170, 119, 17, 153, 238, 85, 2,
                            17, 17, 136, 51, 221, 153, 153, 136, 187, 11, 102, 17, 102, 221, 221, 34,
                            153, 102, 170, 85, 85, 17, 187, 85, 85, 102, 68, 136, 0, 102, 255, 85,
                            85, 238, 85, 68, 187, 17, 136, 255, 187, 170, 1, 187, 221, 34, 17, 1,
                            136, 153, 119, 119, 3, 119, 34, 102, 255, 255, 17, 17, 119, 34, 102, 4,
                            153, 221, 6, 255, 187, 4, 102, 17, 153, 170, 85, 85, 153, 68, 204, 51,
                            17, 238, 0, 119, 119, 51, 51, 102, 187, 102, 6, 17, 153, 102, 102, 119,
                            68, 6, 102, 221, 153, 153, 102, 170, 255, 153, 238, 153, 170, 17, 119, 85,
                            68, 238, 255, 85, 255, 85, 17, 17, 68, 238, 221, 170, 238, 204, 153, 17,
                            136, 153, 17, 119, 4, 17, 204, 187, 221, 238, 153, 153, 255, 68, 68, 153,
                            153, 204, 238, 102, 255, 255, 34, 5, 102, 255, 85, 102, 170, 68, 153, 0,
                            221, 221, 170, 221, 51, 238, 136, 136, 102, 153, 85};

        public static byte[] PNGLoad(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !System.IO.File.Exists(fileName)) return null;
            try
            {
                var data = System.IO.File.ReadAllBytes(fileName);
                if (PNGCheckIsAct(data)) return data;
                else if (PNGCheckIsNew(data)) return PNGLoadNew(data);
                else if (PNGCheckIsOld(data)) return PNGLoadOld(data);
                return null;
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteException(ex, "PNG_HELPER");
                return null;
            }
        }

        private static bool PNGCheckIsAct(byte[] data)
        {
            if (data is null || data.Length < 32) return false;
            for (int i = 0; i < PNG_Prefix_Act.Length; i++)
            {
                if (data[i] != PNG_Prefix_Act[i]) return false;
            }
            return true;
        }
        private static bool PNGCheckIsNew(byte[] data)
        {
            if (data is null || data.Length < 32) return false;
            for (int i = 0; i < PNG_Prefix_New.Length; i++)
            {
                if (data[i] != PNG_Prefix_New[i]) return false;
            }
            return true;
        }
        private static bool PNGCheckIsOld(byte[] data)
        {
            if (data is null || data.Length < 32) return false;
            for (int i = 0; i < PNG_Prefix_Old.Length; i++)
            {
                if (data[i] != PNG_Prefix_Old[i]) return false;
            }
            return true;
        }

        public static byte[] PNGLoadOld(byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (i == 200) break;
                data[i] ^= StringHelper.Asc(_xorEncryptKey[i]);
            }
            return data;
        }

        public static byte[] PNGLoadNew(byte[] data)
        {
            var startPos = Convert.ToInt32(data[15]);
            for (int i = 0; i <= 15; i++)
            {
                data[i] = PNG_Prefix_Act[i];
            }
            for (int i = 16; i < data.Length; i++)
            {
                if (startPos > (mapKey.Length - 1)) break;
                data[i] ^= mapKey[startPos];
                startPos += 1;
            }
            startPos = data.Length;
            data = data.Concat(PNG_Endfix_Act).ToArray();
            return data;
        }

        #endregion


    }
}
