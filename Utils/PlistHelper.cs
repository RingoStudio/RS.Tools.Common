using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Tools.Common.Utils
{
    public class PlistHelper
    {
        #region CONST
        private const string PLIST_DIC_START = "<dict>";
        private const string PLIST_DIC_END = "</dict>";
        private const string PLIST_KEY_START = "<key>";
        private const string PLIST_KEY_END = "</key>";
        private const string PLIST_STR_START = "<string>";
        private const string PLIST_STR_END = "</string>";
        private const string PLIST_INT_START = "<integer>";
        private const string PLIST_INT_END = "</integer>";
        private const string PLIST_BOOL_TRUE = "<true/>";
        private const string PLIST_BOOL_FALSE = "<false/>";
        private const string PLIST_ARR_START = "<array>";
        private const string PLIST_ARR_END = "</array>";
        private const string PLIST_ARR_NULL = "<array/>";
        private const string PLIST_DATA_START = "<data>";
        private const string PLIST_DATA_END = "</data>";
        private const string PLIST_DATE_START = "<date>";
        private const string PLIST_DATE_END = "</date>";
        private const string PLIST_REAL_START = "<real>";
        private const string PLIST_REAL_END = "</real>";
        #endregion
        public static dynamic PlistToJToken(string path, bool isDirectPath)
        {
            try
            {
                var src = IOHelper.GetStringFromFile(path, isDirectPath);
                return PlistToJson(src);
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteException(ex, "PLIST_HELPER");
                return null;
            }

        }
        public static dynamic PlistToJson(string src)
        {
            if (string.IsNullOrEmpty(src)) return null;
            var offset = 0;
            return ParseInternal(PreTreatment(src), ref offset, true);
        }
        private static string PreTreatment(string src)
        {
            src = src.Replace("\t", "").Replace("\r", "\n").Replace(" ", "");
            bool flag = false;
            var result = new List<string>();
            foreach (var item in src.Split("\n"))
            {
                if (item.StartsWith("<plist"))
                {
                    flag = true;
                }
                else if (item.StartsWith("</plist>"))
                {
                    flag = false;
                }
                else if (flag)
                {
                    result.Add(item);
                }
            }
            return string.Join("", result);
        }
        private static dynamic ParseInternal(string src, ref int offset, bool isVal = false)
        {
            int offset_internal = 0;
            int result_offset = offset;
            int bak_offset = offset;
            var read_result = Read(src, ref result_offset);
            //offset_internal = 0;
            try
            {
                switch (read_result.Item1)
                {
                    case PlistTagType.ARRAY:
                        var result_list = new List<dynamic>();
                        do
                        {
                            var sub_result = ParseInternal(read_result.Item2 as string, ref offset_internal);
                            if (sub_result == null) break;
                            result_list.Add(sub_result);
                            if (offset_internal >= result_offset) break;
                        } while (true);
                        offset = result_offset;
                        return JArray.FromObject(result_list);
                    case PlistTagType.DIC:
                        dynamic result_dic = new JObject();
                        do
                        {
                            var result_dic_key = ParseInternal(read_result.Item2 as string, ref offset_internal) as string;
                            if (result_dic_key == null) break;
                            if (result_dic_key == "login_main_r2_f/1_zhuanshen_1_6.png")
                                Console.WriteLine(result_dic_key);
                            var result_dic_val = ParseInternal(read_result.Item2 as string, ref offset_internal, true);
                            if (result_dic_key == null) break;
                            if (result_dic.ContainsKey(result_dic_key))
                                Console.WriteLine(result_dic_key);
                            result_dic[result_dic_key] = result_dic_val;
                            if (offset_internal >= result_offset) break;
                        } while (true);
                        offset = result_offset;
                        return result_dic;
                    case PlistTagType.REAL:
                    case PlistTagType.DATA:
                    case PlistTagType.INT:
                    case PlistTagType.DATE:
                    case PlistTagType.ARR_NULL:
                        offset = result_offset;
                        return read_result.Item2;
                    case PlistTagType.STR:
                        offset = result_offset;
                        return read_result.Item2 as string;
                    case PlistTagType.KEY:
                        if (isVal)
                        {
                            return null;
                        }
                        offset = result_offset;
                        if (!string.IsNullOrEmpty(read_result.Item2 as string))
                        {
                            switch (read_result.Item2 as string)
                            {
                                case "spriteOffset":
                                    return "offset";
                                //case "spriteSize":
                                case "spriteSourceSize":
                                    return "sourceSize";
                                case "textureRect":
                                    return "frame";
                                case "textureRotated":
                                    return "rotated";
                            }
                        }
                        //return read_result.Item2 as JValue;
                        return read_result.Item2 as string;
                    case PlistTagType.TRUE:
                        offset = result_offset;
                        return true;
                    case PlistTagType.FALSE:
                        offset = result_offset;
                        return false;


                    default:
                        offset = result_offset;
                        return null;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteException(ex, "Plist_Helper");
                return null;
            }
        }

        private static (PlistTagType, object, int) Read(string src, ref int offset)
        {
            Console.WriteLine(offset);
            int close = 0;
            object result = null;
            int result_start = 0;
            PlistTagType tag = PlistTagType.NULL;
            do
            {
                if (offset >= src.Length)
                    return (tag, result, offset);
                if (src.Substring(offset, 1) != "<")
                {
                    offset++;
                    continue;
                }

                if (src.Length - offset >= PLIST_DIC_START.Length && src.Substring(offset, PLIST_DIC_START.Length) == PLIST_DIC_START)
                {
                    if (tag == PlistTagType.NULL)
                    {
                        tag = PlistTagType.DIC;
                        close++;
                        result_start = offset + PLIST_DIC_START.Length;
                    }
                    else if (tag == PlistTagType.DIC)
                        close++;
                    offset += PLIST_DIC_START.Length;
                }
                else if (src.Length - offset >= PLIST_ARR_START.Length && src.Substring(offset, PLIST_ARR_START.Length) == PLIST_ARR_START)
                {
                    if (tag == PlistTagType.NULL)
                    {
                        tag = PlistTagType.ARRAY;
                        close++;
                        result_start = offset + PLIST_ARR_START.Length;
                    }
                    else if (tag == PlistTagType.ARRAY)
                        close++;
                    offset += PLIST_ARR_START.Length;
                }
                else if (src.Length - offset >= PLIST_DATA_START.Length && src.Substring(offset, PLIST_DATA_START.Length) == PLIST_DATA_START)
                {
                    if (tag == PlistTagType.NULL)
                    {
                        tag = PlistTagType.DATA;
                        close++;
                        result_start = offset + PLIST_DATA_START.Length;
                    }
                    else if (tag == PlistTagType.DATA)
                        close++;
                    offset += PLIST_DATA_START.Length;
                }
                else if (src.Length - offset >= PLIST_DATE_START.Length && src.Substring(offset, PLIST_DATE_START.Length) == PLIST_DATE_START)
                {
                    if (tag == PlistTagType.NULL)
                    {
                        tag = PlistTagType.DATE;
                        close++;
                        result_start = offset + PLIST_DATE_START.Length;
                    }
                    else if (tag == PlistTagType.DATE)
                        close++;
                    offset += PLIST_DATE_START.Length;
                }
                else if (src.Length - offset >= PLIST_REAL_START.Length && src.Substring(offset, PLIST_REAL_START.Length) == PLIST_REAL_START)
                {
                    if (tag == PlistTagType.NULL)
                    {
                        tag = PlistTagType.REAL;
                        close++;
                        result_start = offset + PLIST_REAL_START.Length;
                    }
                    else if (tag == PlistTagType.REAL)
                        close++;
                    offset += PLIST_REAL_START.Length;
                }
                else if (src.Length - offset >= PLIST_INT_START.Length && src.Substring(offset, PLIST_INT_START.Length) == PLIST_INT_START)
                {
                    if (tag == PlistTagType.NULL)
                    {
                        tag = PlistTagType.INT;
                        close++;
                        result_start = offset + PLIST_INT_START.Length;
                    }
                    else if (tag == PlistTagType.INT)
                        close++;
                    offset += PLIST_INT_START.Length;
                }
                else if (src.Length - offset >= PLIST_STR_START.Length && src.Substring(offset, PLIST_STR_START.Length) == PLIST_STR_START)
                {
                    if (tag == PlistTagType.NULL)
                    {
                        tag = PlistTagType.STR;
                        close++;
                        result_start = offset + PLIST_STR_START.Length;
                    }
                    else if (tag == PlistTagType.STR)
                        close++;
                    offset += PLIST_STR_START.Length;
                }
                else if (src.Length - offset >= PLIST_KEY_START.Length && src.Substring(offset, PLIST_KEY_START.Length) == PLIST_KEY_START)
                {
                    if (tag == PlistTagType.NULL)
                    {
                        tag = PlistTagType.KEY;
                        close++;
                        result_start = offset + PLIST_KEY_START.Length;
                    }
                    else if (tag == PlistTagType.KEY)
                        close++;
                    offset += PLIST_KEY_START.Length;
                }
                else if (src.Length - offset >= PLIST_BOOL_TRUE.Length && src.Substring(offset, PLIST_BOOL_TRUE.Length) == PLIST_BOOL_TRUE)
                {
                    if (tag == PlistTagType.NULL)
                    {
                        tag = PlistTagType.TRUE;
                        offset += PLIST_BOOL_TRUE.Length;
                        return (tag, true, offset);
                    }
                    offset += PLIST_BOOL_TRUE.Length;
                }
                else if (src.Length - offset >= PLIST_BOOL_FALSE.Length && src.Substring(offset, PLIST_BOOL_FALSE.Length) == PLIST_BOOL_FALSE)
                {
                    if (tag == PlistTagType.NULL)
                    {
                        tag = PlistTagType.FALSE;
                        offset += PLIST_BOOL_FALSE.Length;
                        return (tag, false, offset);
                    }
                    offset += PLIST_BOOL_FALSE.Length;
                }
                else if (src.Length - offset >= PLIST_ARR_NULL.Length && src.Substring(offset, PLIST_ARR_NULL.Length) == PLIST_ARR_NULL)
                {
                    if (tag == PlistTagType.NULL)
                    {
                        tag = PlistTagType.ARR_NULL;
                        offset += PLIST_ARR_NULL.Length;
                        return (tag, new JArray(), offset);
                    }
                    offset += PLIST_ARR_NULL.Length;
                }
                else if (src.Length - offset >= PLIST_DIC_END.Length && src.Substring(offset, PLIST_DIC_END.Length) == PLIST_DIC_END)
                {
                    if (tag == PlistTagType.DIC || tag == PlistTagType.NULL)
                    {
                        close--;
                        if (close == 0)
                        {
                            offset += PLIST_DIC_END.Length;
                            return (tag, src.Substring(result_start, offset - result_start - PLIST_DIC_END.Length), result_start);
                        }
                    }
                    offset += PLIST_DIC_END.Length;
                }
                else if (src.Length - offset >= PLIST_ARR_END.Length && src.Substring(offset, PLIST_ARR_END.Length) == PLIST_ARR_END)
                {
                    if (tag == PlistTagType.ARRAY || tag == PlistTagType.NULL)
                    {
                        close--;
                        if (close == 0)
                        {
                            offset += PLIST_ARR_END.Length;
                            return (tag, src.Substring(result_start, offset - result_start - PLIST_ARR_END.Length), result_start);
                        }
                    }
                    offset += PLIST_ARR_END.Length;
                }
                else if (src.Length - offset >= PLIST_DATA_END.Length && src.Substring(offset, PLIST_DATA_END.Length) == PLIST_DATA_END)
                {
                    if (tag == PlistTagType.DATA || tag == PlistTagType.NULL)
                    {
                        close--;
                        if (close == 0)
                        {
                            offset += PLIST_DATA_END.Length;
                            return (tag, src.Substring(result_start, offset - result_start - PLIST_DATA_END.Length), result_start);
                        }
                    }
                    offset += PLIST_DATA_END.Length;
                }
                else if (src.Length - offset >= PLIST_DATE_END.Length && src.Substring(offset, PLIST_DATE_END.Length) == PLIST_DATE_END)
                {
                    if (tag == PlistTagType.DATE || tag == PlistTagType.NULL)
                    {
                        close--;
                        if (close == 0)
                        {
                            offset += PLIST_DATE_END.Length;
                            return (tag, src.Substring(result_start, offset - result_start - PLIST_DATE_END.Length), result_start);
                        }
                    }
                    offset += PLIST_DATE_END.Length;
                }
                else if (src.Length - offset >= PLIST_REAL_END.Length && src.Substring(offset, PLIST_REAL_END.Length) == PLIST_REAL_END)
                {
                    if (tag == PlistTagType.REAL || tag == PlistTagType.NULL)
                    {
                        close--;
                        if (close == 0)
                        {
                            offset += PLIST_REAL_END.Length;
                            return (tag, src.Substring(result_start, offset - result_start - PLIST_REAL_END.Length), result_start);
                        }
                    }
                    offset += PLIST_REAL_END.Length;
                }
                else if (src.Length - offset >= PLIST_INT_END.Length && src.Substring(offset, PLIST_INT_END.Length) == PLIST_INT_END)
                {
                    if (tag == PlistTagType.INT || tag == PlistTagType.NULL)
                    {
                        close--;
                        if (close == 0)
                        {
                            offset += PLIST_INT_END.Length;
                            return (tag, src.Substring(result_start, offset - result_start - PLIST_INT_END.Length), result_start);
                        }
                    }
                    offset += PLIST_INT_END.Length;
                }
                else if (src.Length - offset >= PLIST_STR_END.Length && src.Substring(offset, PLIST_STR_END.Length) == PLIST_STR_END)
                {
                    if (tag == PlistTagType.STR || tag == PlistTagType.NULL)
                    {
                        close--;
                        if (close == 0)
                        {
                            offset += PLIST_STR_END.Length;
                            return (tag, src.Substring(result_start, offset - result_start - PLIST_STR_END.Length).Replace("{", "(").Replace("}", ")"), result_start);
                        }
                    }
                    offset += PLIST_STR_END.Length;
                }
                else if (src.Length - offset >= PLIST_KEY_END.Length && src.Substring(offset, PLIST_KEY_END.Length) == PLIST_KEY_END)
                {
                    if (tag == PlistTagType.KEY || tag == PlistTagType.NULL)
                    {
                        close--;
                        if (close == 0)
                        {
                            offset += PLIST_KEY_END.Length;
                            return (tag, src.Substring(result_start, offset - result_start - PLIST_KEY_END.Length), result_start);
                        }
                    }
                    offset += PLIST_KEY_END.Length;
                }
                else
                    offset++;
            } while (true);
        }
        private enum PlistTagType
        {
            NULL = 0,
            ARRAY = 1,
            DATA = 2,
            DATE = 3,
            DIC = 4,
            REAL = 5,
            INT = 6,
            STR = 7,
            TRUE = 8,
            FALSE = 9,
            KEY = 11,
            ARR_NULL = 12,
            PLIST = 10,
        }
    }
}
