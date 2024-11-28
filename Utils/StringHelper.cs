using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RS.Tools.Common.Utils
{
    public class StringHelper
    {
        #region LOC
        public static string FixContent(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";

            value = value.Trim();

            value = value.Replace("douhaoh", ",");
            value = value.Replace("douhaof", "，");
            value = value.Replace("jvhaoh", ".");
            value = value.Replace("juhaoh", ".");
            value = value.Replace("jvhaof", "。");
            value = value.Replace("juhaof", "。");
            value = value.Replace("fenhaoh", ";");
            value = value.Replace("fenhaof", "；");
            value = value.Replace("&mh", ":");
            value = value.Replace("maohaoh", ":");
            value = value.Replace("maohaof", "：");
            value = value.Replace("tanhaoh", "!");
            value = value.Replace("tanhaof", "！");
            value = value.Replace("wenhaoh", "?");
            value = value.Replace("wenhaof", "？");
            value = value.Replace("baifenhao", "%");
            value = value.Replace("at", "@");

            for (int i = 0; i <= 9; i++)
            {
                value = value.Replace($"rep{i}rep", $"{{{i}}}");
            }
            return value;
        }
        public static string MixContent(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";

            value = value.Trim();

            value = value.Replace(",", "douhaoh");
            value = value.Replace("，", "douhaof");
            value = value.Replace(".", "jvhaoh");
            value = value.Replace("。", "jvhaof");
            value = value.Replace(";", "fenhaoh");
            value = value.Replace("；", "fenhaof");
            value = value.Replace("&mh", "maohaoh");
            value = value.Replace(":", "maohaoh");
            value = value.Replace("：", "maohaof");
            value = value.Replace("!", "tanhaoh");
            value = value.Replace("！", "tanhaof");
            value = value.Replace("?", "wenhaoh");
            value = value.Replace("？", "wenhaof");
            value = value.Replace("%", "baifenhao");
            value = value.Replace("@", "at");

            for (int i = 0; i <= 9; i++)
            {
                value = value.Replace($"{{{i}}}", $"rep{i}rep");
            }

            return value;
        }
        #endregion
        #region FORMATTING
        #region GAMES

        /// <summary>
        /// 炸开字符串[xxx]aaa[-]
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static List<string> ExplodeSegs(string str)
        {
            var ret = new List<string>();
            if (string.IsNullOrEmpty(str) || !str.Contains("[-]"))
            {
                ret.Add(str ?? "");
                return ret;
            }
            bool isStart = false;
            bool isEnd = false;
            foreach (var item in str.Split("[-]"))
            {
                if (string.IsNullOrEmpty(item)) continue;
                if (!(item.Contains("[") && item.Contains("]")))
                {
                    ret.Add(item);
                }
                isStart = item.StartsWith("[");
                isEnd = item.EndsWith("]");
                if (isEnd && isStart)
                {
                    //  [seg]

                    ret.Add(item);

                }
                else if (isEnd)
                {
                    //xxx[seg] 
                    item.Replace("]", "[");
                    var arr = item.Split("[");
                    ret.Add(arr[0]);
                    ret.Add($"[{item[1]}]");

                }
                else if (isStart)
                {
                    //[seg]xxx
                    ret.Add(item);
                }

                else
                {
                    // xxx[seg]xxx
                    item.Replace("]", "[");
                    var arr = item.Split("[");
                    ret.Add(arr[0]);
                    ret.Add($"[{item[1]}]{item[2]}");
                }
            }
            return ret;
        }
        /// <summary>
        /// 转换含有%d和%s的替换字符串
        /// </summary>
        /// <param name="src"></param>
        /// <param name="vals"></param>
        /// <returns></returns>
        public static string LUA_Formatting(string src, params object[] vals)
        {
            if (vals == null || vals.Length == 0) return src;
            foreach (var item in vals)
            {
                if (item is int || item is long || item is decimal || item is double)
                {
                    src = ReplaceFirst(src, "%d", item.ToString());
                }
                else
                {
                    src = ReplaceFirst(src, "%s", item.ToString());
                }
            }
            return src;
        }
        public static string ReplaceFirst(string source, string match, object replacement)
        {
            char[] sArr = source.ToCharArray();
            char[] mArr = match.ToCharArray();
            char[] rArr = replacement.ToString().ToCharArray();
            int idx = StringIndexOf(sArr, mArr);
            if (idx == -1)
            {
                return source;
            }
            else
            {
                return new string(sArr.Take(idx).Concat(rArr).Concat(sArr.Skip(idx + mArr.Length)).ToArray());
            }
        }
        private static int StringIndexOf(char[] source, char[] match)
        {
            int idx = -1;
            for (int i = 0; i <= source.Length - match.Length; i++)
            {
                if (source[i] == match[0])
                {
                    bool isMatch = true;
                    for (int j = 0; j < match.Length; j++)
                    {
                        if (source[i + j] != match[j])
                        {
                            isMatch = false;
                            break;
                        }
                    }
                    if (isMatch)
                    {
                        idx = i;
                        break;
                    }
                }
            }
            return idx;
        }

        #endregion

        #endregion
        #region STRING CONVERT

        public static string BytesToStringDesc(byte[] bytes, string spliter = "-")
        {
            if (bytes == null) return "";
            var list = new List<string>();
            foreach (byte b in bytes)
                list.Add(b.ToString());
            return string.Join(spliter, list.ToArray());
        }
        public static string Chr(byte asc)
        {
            return ((char)asc).ToString();
        }
        public static string ChrW(int CharCode)
        {
            if ((CharCode < -32768) || (CharCode > 0xffff))
            {
                return "";
            }
            return Convert.ToChar((int)(CharCode & 0xffff)).ToString();
        }
        public static byte Asc(string str)
        {
            var arr = System.Text.Encoding.ASCII.GetBytes(str);
            return arr[0];
        }
        public static byte Asc(char str)
        {
            var mid = Convert.ToInt16(str);
            if (mid < 256) return Convert.ToByte(mid);
            return 0;
        }
        public static string GetStrFromBytes(byte[] src, bool decode = false, bool unicode = false)
        {
            if (src == null) return "";
            if (src.Length == 0) return "";
            string result = "";
            int i = 0;
            while (i <= src.Length - 1)
            {
                if (src[i] > 223)
                {
                    result += System.Text.Encoding.UTF8.GetString(src.Skip(i).Take(3).ToArray());
                    i += 3;
                }
                else if (src[i] > 127)
                {
                    result += System.Text.Encoding.UTF8.GetString(src.Skip(i).Take(2).ToArray());
                    i += 2;
                }
                else
                {
                    result += System.Text.Encoding.ASCII.GetString(src.Skip(i).Take(1).ToArray());
                    i += 1;
                }
            };

            return result;
        }
        #endregion

        #region NUMBERS
        private static string _numberChars = "0123456789";
        public static bool IsNumeric(char value) => _numberChars.Contains(value);
        public static bool IsNumeric(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            if (Regex.IsMatch(value, @"^[+-]?0[xX][0-9a-fA-F]+$")) return true;
            return Regex.IsMatch(value, @"^[+-]?\d*[.]?\d*$");
        }
        public static bool IsInt(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            return Regex.IsMatch(value, @"^[+-]?\d*$");
        }
        public static bool IsUnsign(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            return Regex.IsMatch(value, @"^\d*[.]?\d*$");
        }
        public static bool IsRID(string rid)
        {
            if (string.IsNullOrEmpty(rid)) return false;
            return Regex.IsMatch(rid, @"^[a-zA-Z0-9]{14}$")|| Regex.IsMatch(rid, @"^[a-zA-Z0-9]{12}$");
        }
        public static bool isTel(string strInput)
        {
            if (string.IsNullOrEmpty(strInput)) return false;
            return Regex.IsMatch(strInput, @"\d{3}-\d{8}|\d{4}-\d{7}");
        }
        public static bool isQCAccount(string account)
        {

            if (string.IsNullOrEmpty(account)) return false;
            return Regex.IsMatch(account, @"^\\+{0,1}[0-9]+$");
        }
        public static bool isQCPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return false;
            return Regex.IsMatch(password, @"^[0-9A-Za-z_/+!@#:\\{\\}\\|\\%;\\^\\&\\*-]{6,20}$");
        }
        public static bool IsBase64(string cipher)
        {
            if (string.IsNullOrEmpty(cipher)) return false;
            return Regex.IsMatch(cipher, @"^[A-Za-z0-9+/=]+$");
        }

        public static string GetMixedPhoneNumber(string phoneNumber)
        {
            return $"{phoneNumber[..3]}****{phoneNumber[^4..]}";
        }


        /// <summary>
        /// 文本超过指定长度后加省略号
        /// </summary>
        /// <param name="text"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string GetEllipsisText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return "";
            if (text.Length <= maxLength) return text;
            return $"{text[..maxLength]}...";
        }
        #endregion
    }
}
