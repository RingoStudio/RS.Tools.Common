using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Formatting = Newtonsoft.Json.Formatting;

namespace RS.Tools.Common.Utils
{
    public class JSONHelper
    {
        const string File_Path_Spliter_Mark = "<??>";

        #region BASICS
        public static dynamic JArraySlice(dynamic arr, int start, int count)
        {
            if (arr is null || arr is not JArray) return new JArray();
            var total = JSONHelper.GetCount(arr);
            if (start < 0 || start >= total || count > total - start) return arr;
            var ret = new JArray();
            int index = 1;
            for (int i = start; i < total; i++)
            {
                if (count > 0 && count <= index) break;
                ret.Add(arr[i].DeepCopy());
                index++;
            }
            return ret;
        }
        public static JArray JArrayAddLevel(dynamic src)
        {
            var ret = new JArray();
            var arr = src as JArray ?? new JArray();
            ret.Add(arr);
            return ret;
        }
        /// <summary>
        /// 将LIST对象转换为JSON字符串
        /// </summary>
        /// <param name="In_Obj"></param>
        /// <returns></returns>
        public static string Object2JString(List<Dictionary<object, object>> In_Obj)
        {
            return AnyObject2JString(In_Obj);
        }

        /// <summary>
        /// 将DICTIONAY对象转换为JSON字符串
        /// </summary>
        /// <param name="In_Obj"></param>
        /// <returns></returns>
        public static string Object2JString(Dictionary<object, object> In_Obj)
        {
            return AnyObject2JString(In_Obj);
        }

        public static int AnyObject2Int(object In_Obj)
        {
            var str = AnyObject2JString(In_Obj);
            if (StringHelper.IsNumeric(str))
                return Convert.ToInt32(str);
            else
                return -1;
        }
        public static int GetCount(dynamic src)
        {
            if (src is null || src is string) return 0;
            if (src is JValue || src is string) return 1;
            Type type = src.GetType();
            try
            {
                return type.GetProperty("Count").GetValue(src, null);
            }
            catch (Exception)
            {
                return 0;
            }
        }
        /// <summary>
        /// 将对象转换为JSON字符串
        /// </summary>
        /// <param name="In_Obj"></param>
        /// <returns></returns>
        public static string AnyObject2JString(dynamic In_Obj)
        {
            try
            {

                if (In_Obj == null) return "";
                if (In_Obj is JValue)
                {
                    string str = ParseString(In_Obj);
                    return str;
                }
                else
                {
                    string str = JsonConvert.SerializeObject(In_Obj, Formatting.Indented);
                    return str;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteException(ex, "JSONHelper.AnyObject2JString");
                try
                {
                    return In_Obj.ToString();
                }
                catch (Exception)
                {
                    return "";
                }
            }

        }

        /// <summary>
        /// 将LIST对象转换为JArray对象
        /// </summary>
        /// <param name="In_Obj"></param>
        /// <returns></returns>
        public static JArray List2JArray(List<Dictionary<string, string>> In_Obj)
        {

            return JsonConvert.DeserializeObject<JArray>(JsonConvert.SerializeObject(In_Obj, Formatting.Indented));
        }
        public JArray List2JArray(List<int> In_Obj)
        {

            return JsonConvert.DeserializeObject<JArray>(JsonConvert.SerializeObject(In_Obj, Formatting.Indented));
        }
        /// <summary>
        /// 将LIST对象转换为JProperty对象
        /// </summary>
        /// <param name="PropName">JProperty的名称</param>
        /// <param name="In_Obj">JProperty的内容值</param>
        /// <returns></returns>
        public static JProperty List2JProperty(string PropName, List<Dictionary<string, string>> In_Obj)
        {
            return Object2JProperty(PropName, List2JArray(In_Obj));
        }

        private static JProperty Object2JProperty(string PropName, object In_Obj)
        {
            return new JProperty(PropName, In_Obj);
        }


        /// <summary>
        /// 将字符串转换为JSON对象
        /// </summary>
        /// <param name="In_Str"></param>
        /// <returns></returns>
        public static JObject String2JObject(string In_Str)
        {
            return (JObject)JsonConvert.DeserializeObject(In_Str);
        }
        public static JToken EmptyJToken() => (JToken)JsonConvert.DeserializeObject("{}");
        public static JToken String2JToken(string In_Str) => (JToken)new JsonSerializer().Deserialize(new JsonTextReader(new StringReader(In_Str)));
        public static JToken Dic2JToken(Dictionary<string, object> dic) => JsonConvert.DeserializeObject<JToken>(JsonConvert.SerializeObject(dic, Formatting.Indented));
        public static JToken Dic2JToken(Dictionary<string, string> dic) => JsonConvert.DeserializeObject<JToken>(JsonConvert.SerializeObject(dic, Formatting.Indented));

        public static JToken List2JToken(List<object> list) => JsonConvert.DeserializeObject<JToken>(JsonConvert.SerializeObject(list, Formatting.Indented));

        /// <summary>
        /// 将字符串转换为JARRAY对象
        /// </summary>
        /// <param name="In_Str"></param>
        /// <returns></returns>
        public static JArray String2JArray(string In_Str)
        {
            return (JArray)JsonConvert.DeserializeObject(In_Str);
        }
        #endregion

        #region CONVERTOR
        public static dynamic QueryCommon(dynamic source, string id, string path = null)
        {
            if (source is null) return null;
            var m = source[id];
            if (m == null || string.IsNullOrEmpty(path)) return m;
            m = m[path];
            if (m is JValue || m is JObject || m is JArray) return m.DeepClone();
            return m;
        }
        static public void AbsorbDbase(ref dynamic m)
        {
            if (m == null || m is not JObject) return;
            if (m.dbase == null || m.dbase is not JObject) return;
            foreach (var item in m.dbase)
            {
                m[item.Name] = item.Value;
            }
            m.Remove("dbase");
        }

        public static void CombineJO(ref dynamic target, dynamic source, bool cloneSource = true)
        {
            if (source is not JObject || GetCount(source) == 0) return;

            if (target is null) target = new JObject();
            else if (target is not JObject) return;

            foreach (var item in cloneSource ? source.DeepClone() : source)
            {
                target[item.Name] = item.Value;
            }
        }

        public static dynamic Mixed(dynamic src1, dynamic src2, bool notReplaceIndex = false, int startIndex = 1)
        {
            dynamic ret = new JObject();
            if (src1 is not null && src1 is JObject)
            {
                foreach (var item in src1)
                {
                    ret[item.Name] = item.Value;
                }
            }
            if (src2 is not null && src2 is JObject)
            {
                if (!notReplaceIndex)
                {

                    foreach (var item in src2)
                    {
                        ret[item.Name] = item.Value;
                    }
                }
                else
                {
                    var indexList = new List<int>();
                    foreach (var item in src2)
                    {
                        string key = item.Name;
                        if (StringHelper.IsNumeric(key))
                        {
                            int index = startIndex;
                            while (indexList.Contains(index))
                            {
                                index++;
                            }
                            ret[index.ToString()] = item.Value;
                            indexList.Add(index);
                        }
                        else
                        {
                            ret[item.Name] = item.Value;
                        }
                    }
                }
            }

            return ret;
        }

        static dynamic ParseXML(dynamic raw)
        {
            try
            {
                var xml = JSONHelper.ParseString(raw);
                var doc = new XmlDocument();
                doc.LoadXml(xml);
                return JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeXmlNode(doc));
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteException(ex, "ParseXML");
                return null;
            }
        }

        static public Dictionary<string, int[]> ParseParams(JToken Src, string Key)
        {
            if (Src == null || Src.Count() <= 0) return null;
            if (string.IsNullOrEmpty(Key)) return null;
            if (Src[Key] == null) return null;

            return ParseParams(Src[Key]);
        }

        static public Dictionary<string, int[]> ParseParams(JToken Src)
        {
            if (Src == null || Src.Count() <= 0) return null;

            var result = new Dictionary<string, int[]>();
            foreach (var item in Src)
            {
                var key = item.Path.Split(".").Last();
                result.Add(key, ParseJtokenToIntArr(Src, key));
            }
            return result;
        }

        static public string ParseDescStr(JToken Src, string Key)
        {
            var desc = ParseJTokenToStr(Src, Key);
            if (string.IsNullOrEmpty(desc)) return "";
            if (!desc.Contains("@")) return desc;
            return desc.Split("(\"").Last().Split("\")").First();
        }
        static public string[] ParseJtokenToStrArr(JToken Src, string Key, string[] DefaultVal = null)
        {
            if (Src == null)
                return DefaultVal;
            if (Src.Count() == 0)
                return DefaultVal;
            if (string.IsNullOrEmpty(Key))
                return DefaultVal;
            if (Src[Key] == null)
                return DefaultVal;
            if (Src[Key].ToString() == "{}")
                return DefaultVal;
            if (string.IsNullOrEmpty(Src[Key].ToString()))
                return DefaultVal;
            return ParseJtokenToStrArr((JArray)Src[Key]);
        }

        static public string[] ParseJtokenToStrArr(JArray Src, string[] DefaultVal = null)
        {
            if (Src == null)
                return DefaultVal;
            if (Src.Count == 0)
                return DefaultVal;

            if (Src.ToString().Contains("|"))
            {
                return Src.ToString().Split("|");
            }

            List<string> Result = new List<string>();
            foreach (JToken Item_loopVariable in Src)
            {
                if (Item_loopVariable == null)
                    continue;
                Result.Add(AnyObject2JString(Item_loopVariable));
            }
            if (Result.Count > 0)
            {
                return Result.ToArray();
            }
            else
            {
                return DefaultVal;
            }
        }
        static public int[] ParseJtokenToIntArr(JToken Src, string Key, int[] DefaultVal = null)
        {
            if (Src == null)
                return DefaultVal;
            if (Src.Count() == 0)
                return DefaultVal;
            if (string.IsNullOrEmpty(Key))
                return DefaultVal;
            if (Src[Key] == null)
                return DefaultVal;
            if (string.IsNullOrEmpty(Src[Key].ToString()))
                return DefaultVal;
            if (Src[Key].ToString() == "{}")
                return DefaultVal;
            return ParseJtokenToIntArr((JArray)Src[Key]);
        }
        static public long[] ParseJtokenToLngArr(JToken Src, string Key, long[] DefaultVal = null)
        {
            if (Src == null)
                return DefaultVal;
            if (Src.Count() == 0)
                return DefaultVal;
            if (string.IsNullOrEmpty(Key))
                return DefaultVal;
            if (Src[Key] == null)
                return DefaultVal;
            if (string.IsNullOrEmpty(Src[Key].ToString()))
                return DefaultVal;
            if (Src[Key].ToString() == "{}")
                return DefaultVal;
            return ParseJtokenToLngArr((JArray)Src[Key]);
        }
        static public byte[] ParseJtokenToByteArr(JToken Src, string Key, byte[] DefaultVal = null)
        {
            if (Src == null)
                return DefaultVal;
            if (Src.Count() == 0)
                return DefaultVal;
            if (string.IsNullOrEmpty(Key))
                return DefaultVal;
            if (Src[Key] == null)
                return DefaultVal;
            if (string.IsNullOrEmpty(Src[Key].ToString()))
                return DefaultVal;
            if (Src[Key].ToString() == "{}")
                return DefaultVal;
            return ParseJtokenToByteArr((JArray)Src[Key]);
        }
        static public float[] ParseJtokenToSngArr(JToken Src, string Key, float[] DefaultVal = null)
        {
            if (Src == null)
                return DefaultVal;
            if (Src.Count() == 0)
                return DefaultVal;
            if (string.IsNullOrEmpty(Key))
                return DefaultVal;
            if (Src[Key] == null)
                return DefaultVal;
            if (string.IsNullOrEmpty(Src[Key].ToString()))
                return DefaultVal;
            if (Src[Key].ToString() == "{}")
                return DefaultVal;
            return ParseJtokenToSngArr((JArray)Src[Key]);
        }
        static public byte[] ParseByteArr(dynamic src)
        {
            if (src is null || src is not JArray || src.Count <= 0) return new byte[] { };
            var ret = new List<byte>();
            foreach (var item in src)
            {
                if (!StringHelper.IsNumeric(item.ToString())) continue;
                ret.Add(ParseByte(item));
            }
            return ret.ToArray();
        }
        static public byte[] ParseJtokenToByteArr(JArray Src)
        {

            if (Src == null)
                return null;
            if (Src.Count == 0)
                return null;

            if (Src.ToString().Contains("|"))
            {
                List<byte> Result1 = new List<byte>();
                foreach (JToken Item_loopVariable in Src.ToString().Split("|"))
                {
                    if (Item_loopVariable == null)
                        continue;
                    Result1.Add(Convert.ToByte(Item_loopVariable));
                }
                if (Result1.Count > 0)
                {
                    return Result1.ToArray();
                }
                else
                {
                    return null;
                }
            }

            List<byte> Result = new List<byte>();
            foreach (JToken Item_loopVariable in Src)
            {
                if (Item_loopVariable == null)
                    continue;
                Result.Add(Convert.ToByte(Item_loopVariable.ToString()));
            }
            if (Result.Count() > 0)
            {
                return Result.ToArray();
            }
            else
            {
                return null;
            }
        }
        static public float[] ParseJtokenToSngArr(JArray Src)
        {

            if (Src == null)
                return null;
            if (Src.Count() == 0)
                return null;

            if (Src.ToString().Contains("|"))
            {
                List<float> Result1 = new List<float>();
                foreach (JToken Item_loopVariable in Src.ToString().Split("|"))
                {
                    if (Item_loopVariable == null)
                        continue;
                    Result1.Add(Convert.ToSingle(Item_loopVariable));
                }
                if (Result1.Count > 0)
                {
                    return Result1.ToArray();
                }
                else
                {
                    return null;
                }
            }

            List<float> Result = new List<float>();
            foreach (JToken Item_loopVariable in Src)
            {
                if (Item_loopVariable == null)
                    continue;
                Result.Add(Convert.ToSingle(Item_loopVariable.ToString()));
            }
            if (Result.Count() > 0)
            {
                return Result.ToArray();
            }
            else
            {
                return null;
            }
        }
        static public int[] ParseJtokenToIntArr(JArray Src)
        {

            if (Src == null)
                return null;
            if (Src.Count() == 0)
                return null;

            if (Src.ToString().Contains("|"))
            {
                List<int> Result1 = new List<int>();
                foreach (JToken Item_loopVariable in Src.ToString().Split("|"))
                {
                    if (Item_loopVariable == null)
                        continue;
                    Result1.Add(Convert.ToInt32(Item_loopVariable));
                }
                if (Result1.Count() > 0)
                {
                    return Result1.ToArray();
                }
                else
                {
                    return null;
                }
            }

            List<int> Result = new List<int>();
            foreach (JToken Item_loopVariable in Src)
            {
                if (Item_loopVariable == null)
                    continue;
                Result.Add(Convert.ToInt32(Item_loopVariable.ToString()));
            }
            if (Result.Count() > 0)
            {
                return Result.ToArray();
            }
            else
            {
                return null;
            }
        }
        static public long[] ParseJtokenToLngArr(JArray Src)
        {

            if (Src == null)
                return null;
            if (Src.Count() == 0)
                return null;

            if (Src.ToString().Contains("|"))
            {
                List<long> Result1 = new List<long>();
                foreach (JToken Item_loopVariable in Src.ToString().Split("|"))
                {
                    if (!StringHelper.IsNumeric(Item_loopVariable.ToString()))
                    {
                        Result1.Add(-1);
                    }
                    else
                    {
                        Result1.Add(Convert.ToInt64(Item_loopVariable.ToString()));
                    }
                }
                if (Result1.Count() > 0)
                {
                    return Result1.ToArray();
                }
                else
                {
                    return null;
                }
            }

            List<long> Result = new List<long>();
            foreach (JToken Item_loopVariable in Src)
            {
                if (!StringHelper.IsNumeric(Item_loopVariable.ToString()))
                {
                    Result.Add(-1);
                }
                else
                {
                    Result.Add(Convert.ToInt64(Item_loopVariable.ToString()));
                }
            }
            if (Result.Count() > 0)
            {
                return Result.ToArray();
            }
            else
            {
                return null;
            }
        }

        static public string[][] ParseJtokenTo2D2StrArr(JToken Src, string Key)
        {
            if (Src == null || Src.Count() == 0) return null;
            if (string.IsNullOrEmpty(Key)) return null;
            if (Src[Key] == null) return null;
            return ParseJtokenTo2D2StrArrCore(Src[Key]);
        }
        static public string[][] ParseJtokenTo2D2StrArrCore(JToken Src)
        {
            if (Src == null)
                return null;
            if (Src.Count() == 0)
                return null;

            List<List<string>> OutList = new List<List<string>>();
            List<string> InList = default(List<string>);

            try
            {
                foreach (var item1 in Src)
                {
                    if (item1.GetType().ToString().Split(".").Last().ToLower() == "jarray")
                    {
                        InList = new List<string>();
                        foreach (JToken Item2 in item1)
                        {
                            if (string.IsNullOrEmpty(Item2.ToString()))
                                continue;
                            InList.Add(Item2.ToString());
                        }
                        OutList.Add(InList);
                    }
                    else if (item1.GetType().ToString().Split(".").Last().ToLower() == "jproperty")
                    {
                        string attrib = item1.Path.Split(".").Last();
                        var subjo = JSONHelper.ParseJTokenToJToken(item1, attrib);
                        if (subjo.GetType().ToString().Split(".").Last().ToLower() == "jarray")
                        {
                            foreach (var val in subjo)
                            {
                                OutList.Add(new List<string> { attrib, val.ToString() });
                            }
                        }
                        else
                        {
                            foreach (var item3 in subjo)
                            {
                                var key = item3.Path.Split(".").Last();
                                var val = JSONHelper.ParseJTokenToStr(subjo, key);
                                OutList.Add(new List<string> { key, val });
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                //Log.Write(ex, "s_o::j_2_2d_str_arr_key");
                return null;
            }

            string[][] result = new string[OutList.Count][];

            for (int i = 0; i <= OutList.Count() - 1; i++)
            {
                result[i] = OutList[i].ToArray();

            }
            return result;
        }
        static public string[,] ParseJtokenTo2DStrArr(JToken Src, string Key, int PerLength = -1)
        {
            if (Src == null)
                return null;
            if (Src.Count() == 0)
                return null;
            if (string.IsNullOrEmpty(Key))
                return null;
            if (Src.Count() == 0)
                return null;
            if (Src[Key] == null)
                return null;
            if (Src[Key].Count() == 0)
                return null;
            if (!Src[Key].First.HasValues)
            {
                return null;
            }
            try
            {
                return ParseJtokenTo2DStrArrCore(Src[Key], PerLength);
            }
            catch (Exception ex)
            {
                //Log.Write(ex, "s_o::j_2_2d_str_arr_key_" + Key);
                return null;
                //MsgBox(Src[Key].GetType.ToString)
            }
        }

        static public string[,] ParseJtokenTo2DStrArrCore(JToken Src, int PerLength = -1)
        {
            if (Src == null)
                return null;
            if (Src.Count() == 0)
                return null;

            int Len1 = 0;
            int Len2 = 0;
            List<List<string>> OutList = new List<List<string>>();
            List<string> InList = default(List<string>);

            int MaxInLength = 0;

            try
            {
                foreach (JArray Item1 in Src)
                {
                    InList = new List<string>();
                    foreach (JToken Item2 in Item1)
                    {
                        if (string.IsNullOrEmpty(Item2.ToString()))
                            continue;
                        InList.Add(Item2.ToString());
                    }
                    MaxInLength = Math.Max(MaxInLength, InList.Count);
                    OutList.Add(InList);
                }
            }
            catch (Exception ex)
            {
                //Log.Write(ex, "s_o::j_2_2d_str_arr_key");
                return null;
            }

            if (PerLength > 0)
                MaxInLength = PerLength;

            string[,] Result = new string[OutList.Count, MaxInLength];
            for (int i = 0; i <= OutList.Count() - 1; i++)
            {
                for (int j = 0; j <= MaxInLength - 1; j++)
                {
                    if (j <= (OutList[i].Count() - 1))
                    {
                        Result[i, j] = OutList[i][j];
                    }
                    else
                    {
                        Result[i, j] = "";
                    }
                }
            }
            return Result;
        }

        static public int[][] ParseJtokenTo2D2IntArr(JToken Src, string Key)
        {
            if (Src == null || Src.Count() <= 0) return null;
            if (string.IsNullOrEmpty(Key)) return null;
            if (Src[Key] == null) return null;
            return ParseJtokenTo2D2IntArr(Src[Key] as JArray);
        }
        static public int[][] ParseJtokenTo2D2IntArr(JArray Src)
        {
            if (Src == null || Src.Count() == 0) return null;

            int Len1 = 0;
            int Len2 = 0;
            List<List<int>> OutList = new List<List<int>>();
            List<int> InList = default(List<int>);

            try
            {
                foreach (JArray Item1 in Src)
                {
                    InList = new List<int>();
                    foreach (JToken Item2 in Item1)
                    {
                        if (string.IsNullOrEmpty(Item2.ToString()))
                            continue;
                        InList.Add(Convert.ToInt32(Item2.ToString()));
                    }
                    OutList.Add(InList);
                }
            }
            catch (Exception ex)
            {
                //Log.Write(ex, "s_o::j_2_2d_int_arr");
                return null;
            }

            int[][] result = new int[OutList.Count][];

            for (int i = 0; i <= OutList.Count() - 1; i++)
            {
                result[i] = OutList[i].ToArray();
            }
            return result;
        }

        static public int[,] ParseJtokenTo2DIntArr(JToken Src, string Key)
        {
            if (Src == null)
                return null;
            if (Src.Count() == 0)
                return null;
            if (string.IsNullOrEmpty(Key))
                return null;
            if (Src[Key] == null)
                return null;
            if (Src[Key].ToString() == "{}")
                return null;
            if (string.IsNullOrEmpty(Src[Key].ToString()))
                return null;
            return ParseJtokenTo2DIntArr((JArray)Src[Key]);
        }
        static public int[,] ParseJtokenTo2DIntArr(JArray Src)
        {
            if (Src == null)
                return null;
            if (Src.Count() == 0)
                return null;

            int Len1 = 0;
            int Len2 = 0;
            List<List<int>> OutList = new List<List<int>>();
            List<int> InList = default(List<int>);

            int MaxInLength = 0;
            try
            {
                foreach (JArray Item1 in Src)
                {
                    InList = new List<int>();
                    foreach (JToken Item2 in Item1)
                    {
                        if (string.IsNullOrEmpty(Item2.ToString()))
                            continue;
                        InList.Add(Convert.ToInt32(Item2.ToString()));
                    }
                    MaxInLength = Math.Max(MaxInLength, InList.Count);
                    OutList.Add(InList);
                }
            }
            catch (Exception ex)
            {
                //Log.Write(ex, "s_o::j_2_2d_int_arr");
                return null;
            }

            int[,] Result = new int[OutList.Count, MaxInLength];
            for (int i = 0; i <= OutList.Count() - 1; i++)
            {
                for (int j = 0; j <= MaxInLength - 1; j++)
                {
                    if (j <= (OutList[i].Count() - 1))
                    {
                        Result[i, j] = OutList[i][j];
                    }
                    else
                    {
                        Result[i, j] = 0;
                    }
                }
            }
            return Result;
        }

        public static JToken ParseJTokenToJToken(JToken Src, string Key)
        {
            try
            {
                if (string.IsNullOrEmpty(Key))
                    return null;
                if (Src == null || Src.Count() == 0)
                    return null;
                if (Src.Count() == 0)
                    return null;
                if (Src[Key] == null)
                    return null;
                return Src[Key];
            }
            catch (Exception ex)
            {
                //Log.Write(ex, "s_o::j_2_jt_key_" + Key);
                return null;
            }
        }
        static public int ParseJTokenToInt(JToken Src, string Key, int DefaultVal = 0)
        {
            try
            {
                if (string.IsNullOrEmpty(Key))
                    return DefaultVal;
                if (Src == null)
                    return DefaultVal;
                if (Src.Count() == 0)
                    return DefaultVal;
                if (Src[Key] == null)
                    return DefaultVal;
                if (Src[Key].ToString() == "*")
                    return -1;
                if (!StringHelper.IsNumeric(Src[Key].ToString()))
                    return DefaultVal;
                return Convert.ToInt32(Src[Key]);
            }
            catch (Exception ex)
            {
                //Log.Write(ex, "s_o::j_2_int_key_" + Key);
                return DefaultVal;
            }
        }
        static public float ParseJTokenToSng(JToken Src, string Key, int DefaultVal = 0)
        {
            try
            {
                if (string.IsNullOrEmpty(Key))
                    return DefaultVal;
                if (Src == null)
                    return DefaultVal;
                if (Src.Count() == 0)
                    return DefaultVal;
                if (Src[Key] == null)
                    return DefaultVal;
                if (Src[Key].ToString() == "*")
                    return -1;
                if (!StringHelper.IsNumeric(Src[Key].ToString()))
                    return DefaultVal;
                return Convert.ToSingle(Src[Key]);
            }
            catch (Exception ex)
            {
                //Log.Write(ex, "s_o::j_2_sng_key_" + Key);
                return DefaultVal;
            }
        }
        static public long ParseJTokenToLng(JToken Src, string Key, long DefaultVal = 0)
        {
            try
            {
                if (string.IsNullOrEmpty(Key))
                    return DefaultVal;
                if (Src == null)
                    return DefaultVal;
                if (Src.Count() == 0)
                    return DefaultVal;
                if (Src[Key] == null)
                    return DefaultVal;
                if (Src[Key].ToString() == "*")
                    return -1;
                if (!StringHelper.IsNumeric(Src[Key].ToString()))
                    return DefaultVal;
                return Convert.ToInt64(Src[Key].ToString());
            }
            catch (Exception ex)
            {
                //Log.Write(ex, "s_o::j_2_lng_key_" + Key);
                return DefaultVal;
            }
        }
        static public Dictionary<int, int> ParseJtokenToDicIntInt(JToken Src, string Key)
        {
            Src = ParseJTokenToJToken(Src, Key);
            return ParseJtokenToDicIntInt(Src);
        }
        static public Dictionary<int, int> ParseJtokenToDicIntInt(JToken Src)
        {
            var result = new Dictionary<int, int>();
            if (Src == null) return result;
            if (Src.Count() == 0) return result;
            string key, value;
            foreach (var item in Src)
            {
                key = item.Path.Split(".").Last();
                value = ParseJTokenToStr(Src, key);
                if (StringHelper.IsNumeric(key) && StringHelper.IsNumeric(value))
                {
                    result.Add(Convert.ToInt32(key), Convert.ToInt32(value));
                }
            }
            return result;
        }
        static public Dictionary<int, string> ParseJtokenToDicIntStr(JToken Src, string Key)
        {
            var result = new Dictionary<int, string>();
            Src = ParseJTokenToJToken(Src, Key);
            if (Src == null) return result;
            if (Src.Count() == 0) return result;
            string key, value;
            foreach (var item in Src)
            {
                key = item.Path.Split(".").Last();
                value = ParseJTokenToStr(Src, key);
                if (StringHelper.IsNumeric(key))
                {
                    result.Add(Convert.ToInt32(key), value);
                }
            }
            return result;
        }
        static public Dictionary<string, int> ParseDicStrInt(dynamic src)
        {
            var result = new Dictionary<string, int>();
            if (src == null || src is not JObject) return result;
            string key, val;
            foreach (var item in src)
            {
                key = item.Name;
                if (item.Value is not JValue) continue;
                val = ParseString(item.Value);
                if (!StringHelper.IsNumeric(val)) continue;
                result.Add(key, Convert.ToInt32(val));
            }
            return result;
        }
        static public Dictionary<int, int> ParseDicIntInt(dynamic src)
        {
            var result = new Dictionary<int, int>();
            if (src == null || src is not JObject) return result;
            string key, val;
            foreach (var item in src)
            {
                key = item.Name;
                if (!StringHelper.IsNumeric(key)) continue;
                if (item.Value is not JValue) continue;
                val = ParseString(item.Value);
                if (!StringHelper.IsNumeric(val)) continue;
                result.Add(Convert.ToInt32(key), Convert.ToInt32(val));
            }
            return result;
        }

        static public Dictionary<string, long> ParseDicStrLong(dynamic src)
        {
            var result = new Dictionary<string, long>();
            if (src == null || src is not JObject) return result;
            string key, val;
            foreach (var item in src)
            {
                key = item.Name;
                if (item.Value is not JValue) continue;
                val = ParseString(item.Value);
                if (!StringHelper.IsNumeric(val)) continue;
                result.Add(key, Convert.ToInt64(val));
            }
            return result;
        }
        static public Dictionary<string, string> ParseDicStrStr(dynamic src)
        {
            var result = new Dictionary<string, string>();
            if (src == null || src is not JObject) return result;
            string key, val;
            foreach (var item in src)
            {
                key = item.Name;
                if (item.Value is not JValue) continue;
                val = ParseString(item.Value);
                result.Add(key, ParseString(val));
            }
            return result;
        }
        static public Dictionary<string, int> ParseJtokenToDicStrInt(JToken Src, string Key)
        {
            var result = new Dictionary<string, int>();
            Src = ParseJTokenToJToken(Src, Key);
            if (Src == null) return result;
            if (Src.Count() == 0) return result;
            string key, value;
            foreach (var item in Src)
            {
                key = item.Path.Split(".").Last();
                value = ParseJTokenToStr(Src, key);
                if (StringHelper.IsNumeric(value))
                {
                    result.Add(key, Convert.ToInt32(value));
                }
            }
            return result;
        }
        static public Dictionary<string, int> ParseJtokenToDicStrInt(JToken Src)
        {
            var result = new Dictionary<string, int>();
            if (Src == null) return result;
            if (Src.Count() == 0) return result;
            string key, value;
            foreach (var item in Src)
            {
                key = item.Path.Split(".").Last();
                value = ParseJTokenToStr(Src, key);
                if (StringHelper.IsNumeric(value))
                {
                    result.Add(key, Convert.ToInt32(value));
                }
            }
            return result;
        }
        static public Dictionary<string, string> ParseJtokenToDicStrStr(JToken Src)
        {
            var result = new Dictionary<string, string>();
            if (Src == null) return result;
            if (Src.Count() == 0) return result;
            string key, value;
            foreach (var item in Src)
            {
                key = item.ToObject<JProperty>().Name;
                value = AnyObject2JString(item.ToObject<JProperty>().Value);
                result.Add(key, value);
            }
            return result;
        }
        static public Dictionary<string, long> ParseJtokenToDicStrLng(JToken Src, string Key)
        {
            Src = ParseJTokenToJToken(Src, Key);
            return ParseJtokenToDicStrLng(Src);
        }
        static public Dictionary<string, long> ParseJtokenToDicStrLng(JToken Src)
        {
            var result = new Dictionary<string, long>();
            if (Src == null) return result;
            if (Src.Count() == 0) return result;
            string key, value;
            foreach (var item in Src)
            {
                key = item.Path.Split(".").Last();
                value = ParseJTokenToStr(Src, key);
                if (StringHelper.IsNumeric(value))
                {
                    result.Add(key, Convert.ToInt64(value));
                }
            }
            return result;
        }
        static public Dictionary<int, int[]> ParseJtokenToDicIntIntArr(JToken Src, string Key)
        {
            var result = new Dictionary<int, int[]>();
            Src = ParseJTokenToJToken(Src, Key);
            if (Src == null) return result;
            if (Src.Count() == 0) return result;
            string key;
            int[] value;
            foreach (var item in Src)
            {
                key = item.Path.Split(".").Last();
                value = ParseJtokenToIntArr(Src, key);
                if (!StringHelper.IsNumeric(key)) continue;
                if (value == null) continue;
                result.Add(Convert.ToInt32(key), value);
            }
            return result;
        }
        static public Dictionary<int, List<int>> ParseJtokenToDicIntIntList(JToken Src, string Key)
        {
            var result = new Dictionary<int, List<int>>();
            Src = ParseJTokenToJToken(Src, Key);
            if (Src == null) return result;
            if (Src.Count() == 0) return result;
            string key;
            int[] value;
            foreach (var item in Src)
            {
                key = item.Path.Split(".").Last();
                value = ParseJtokenToIntArr(Src, key);
                if (!StringHelper.IsNumeric(key)) continue;
                if (value == null) continue;
                result.Add(Convert.ToInt32(key), value.ToList());
            }
            return result;
        }

        static public Dictionary<int, List<string>> ParseJtokenToDicIntStrList(JToken Src, string Key)
        {
            var result = new Dictionary<int, List<string>>();
            Src = ParseJTokenToJToken(Src, Key);
            if (Src == null) return result;
            if (Src.Count() == 0) return result;
            string key;
            string[] value;
            foreach (var item in Src)
            {
                key = item.Path.Split(".").Last();
                value = ParseJtokenToStrArr(Src, key);
                if (!StringHelper.IsNumeric(key)) continue;
                if (value == null) continue;
                result.Add(Convert.ToInt32(key), value.ToList());
            }
            return result;
        }

        static public string ParseJTokenToStr(JToken Src, string Key, string DefaultVal = "")
        {
            try
            {
                if (string.IsNullOrEmpty(Key))
                    return DefaultVal;
                if (Src == null)
                    return DefaultVal;
                if (Src.Count() == 0)
                    return DefaultVal;
                if (Src[Key] == null)
                    return DefaultVal;
                if (string.IsNullOrEmpty(Src[Key].ToString()))
                    return DefaultVal;
                return Src[Key].ToString().Replace(File_Path_Spliter_Mark, "\\");
            }
            catch (Exception ex)
            {
                try
                {
                    return AnyObject2JString(Src[Key]);
                }
                catch (Exception ex2)
                {
                    //Log.Write(ex2, "s_o::j_2_str_key_" + Key);
                    return DefaultVal;
                }
            }
        }
        static public int GetMaxKey(dynamic src)
        {
            List<string> list = GetKeys(src);
            if (list.Count == 0) return -1;
            return list.Select((a) => Convert.ToInt32(a)).Max();
        }
        static public List<string> GetKeys(dynamic src)
        {
            var ret = new List<string>();
            if (src == null || src is not JObject || GetCount(src) == 0) return ret;
            foreach (var item in src) ret.Add(item.Name);
            return ret;
        }
        static public List<int> GetKeysInt(dynamic src)
        {
            var ret = new List<int>();
            if (src == null || src is not JObject || GetCount(src) == 0) return ret;
            foreach (var item in src)
            {
                var key = ParseString(item.Name);
                if (StringHelper.IsNumeric(key)) ret.Add(Convert.ToInt32(key));
            }
            return ret;
        }
        static public List<long> GetKeysLong(dynamic src)
        {
            var ret = new List<long>();
            if (src == null || src is not JObject || GetCount(src) == 0) return ret;
            foreach (var item in src)
            {
                var key = ParseString(item.Name);
                if (StringHelper.IsNumeric(key)) ret.Add(Convert.ToInt64(key));
            }
            return ret;
        }
        public static List<dynamic> GetValuesRaw(dynamic src)
        {
            var ret = new List<dynamic>();
            if (src == null || src is not JObject || GetCount(src) == 0) return ret;
            foreach (var item in src) ret.Add(item.Value);
            return ret;
        }
        static public List<string> GetValues(dynamic src)
        {
            var ret = new List<string>();
            if (src == null || src is not JObject || GetCount(src) == 0) return ret;
            foreach (var item in src)
            {
                if (item.Value is JValue) ret.Add(item.Value.ToString());
            }
            return ret;
        }
        static public List<int> GetValuesInt(dynamic src)
        {
            var ret = new List<int>();
            if (src == null || src is not JObject || GetCount(src) == 0) return ret;
            foreach (var item in src)
            {
                var val = ParseString(item.Value);
                if (StringHelper.IsNumeric(val)) ret.Add(Convert.ToInt32(val));
            }
            return ret;
        }

        static public bool ParseBool(dynamic src, bool defaults = false)
        {
            if (src == null || src is JArray || src is JObject) return defaults;
            var val = src.ToString();
            if (StringHelper.IsNumeric(val)) return Convert.ToInt32(val) > 0;
            else if (val.ToLower() == "true") return true;
            return false;
        }
        static public List<string> ParseStringList(dynamic arr)
        {
            if (arr == null || arr is not JArray) return new List<string>();
            var ret = new List<string>();
            foreach (var item in arr)
            {
                ret.Add(ParseString(item));
            }
            return ret;
        }
        static public int ParseInt(dynamic src, int defaults = 0)
        {
            if (src == null) return defaults;
            if (src is JArray || src is JObject) return defaults;
            string val = src.ToString();
            if (val.Contains(".")) val = val.Split(".").First();
            if (StringHelper.IsNumeric(val)) return Convert.ToInt32(val);
            return defaults;
        }
        static public ulong ParseULong(dynamic src, ulong defaults = 0)
        {
            if (src == null) return defaults;
            if (src is JArray || src is JObject) return defaults;
            string val = src.ToString();
            if (val.Contains(".")) val = val.Split(".").First();
            if (StringHelper.IsNumeric(val)) return Convert.ToUInt64(val);
            return defaults;
        }
        static public uint ParseUInt(dynamic src, uint defaults = 0)
        {
            if (src == null) return defaults;
            if (src is JArray || src is JObject) return defaults;
            string val = src.ToString();
            if (val.Contains(".")) val = val.Split(".").First();
            if (StringHelper.IsNumeric(val)) return Convert.ToUInt32(val);
            return defaults;
        }
        static public byte ParseByte(dynamic src, byte defaults = 0)
        {
            if (src == null) return defaults;
            if (src is JArray || src is JObject) return defaults;
            var val = src.ToString();
            if (StringHelper.IsNumeric(val)) return Convert.ToByte(val);
            return defaults;
        }
        static public long ParseLong(dynamic src, long defaults = 0)
        {
            if (src == null || src is JArray || src is JObject) return defaults;
            var val = src.ToString();
            if (StringHelper.IsNumeric(val)) return Convert.ToInt64(val);
            return defaults;
        }
        static public double ParseDouble(dynamic src, double defaults = 0)
        {
            if (src == null || src is JArray || src is JObject) return defaults;
            var val = src.ToString();
            if (StringHelper.IsNumeric(val)) return Convert.ToDouble(val);
            return defaults;
        }
        static public decimal ParseDecimal(dynamic src, decimal defaults = 0)
        {
            if (src == null || src is JArray || src is JObject) return defaults;
            var val = src.ToString();
            if (StringHelper.IsNumeric(val)) return Convert.ToDecimal(val);
            return defaults;
        }
        static public List<int> ParseIntList(dynamic src)
        {
            var ret = new List<int>();
            if (src == null || src is not JArray || GetCount(src) == 0) return ret;
            foreach (var item in src)
            {
                if (!StringHelper.IsNumeric(item.ToString())) continue;
                ret.Add(ParseInt(item));
            }
            return ret;
        }
        static public List<string> ParseStringList(dynamic src, bool ignoreEmpty = true)
        {
            var ret = new List<string>();
            if (src == null || src is not JArray || GetCount(src) == 0) return ret;
            foreach (var item in src)
            {
                var one = ParseString(item);
                if (ignoreEmpty && string.IsNullOrEmpty(one)) continue;
                ret.Add(one);
            }
            return ret;
        }
        /// <summary>
        /// t2中含有一个t1中的项目为true
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        static public bool Intersection<T>(IEnumerable<T> t1, IEnumerable<T> t2)
        {
            if (t1 is null || t2 is null) return false;
            var l1 = t1.ToList();
            var l2 = t2.ToList();
            foreach (var item in t1)
            {
                if (t2.Contains(item)) return true;
            }
            return false;
        }
        static public string PraseJoinString(dynamic src, string connector = "，")
        {
            List<string> list = ParseStringList(src);
            if (list.Count == 0) return "";
            return string.Join(connector, list);
        }
        static public string ParseString(dynamic src, string defaults = "")
        {
            if (src is null || src is JArray || src is JObject) return defaults;
            return src.ToString();
        }
        static public bool ParseJTokenToBool(JToken Src, string Key, bool DefalultVal = false)
        {
            try
            {
                if (string.IsNullOrEmpty(Key))
                    return DefalultVal;
                if (Src == null)
                    return DefalultVal;
                if (Src.Count() == 0)
                    return DefalultVal;
                if (Src[Key] == null)
                    return DefalultVal;
                var val = Src[Key].ToString();
                if (string.IsNullOrEmpty(val))
                    return DefalultVal;
                if (StringHelper.IsNumeric(val))
                    return Convert.ToInt32(val) > 0;
                else
                    return val.ToLower() == "true";
            }
            catch (Exception)
            {
                //Logger.Instance.Write(ex, "s_o::j_2_boo_key_" + Key);
                return DefalultVal;
            }
        }
        public static void JArrayRemove(ref dynamic arr, dynamic val)
        {
            foreach (var item in arr)
            {
                if (item.ToString() == val.ToString()) item.Remove();
            }
        }
        public static void JArrayAppend(ref dynamic arr, dynamic arr2)
        {
            if (arr2 == null || arr2 is not JArray || GetCount(arr2) == 0) return;
            arr = arr ?? new JArray();
            foreach (var item in arr2)
            {
                arr.Add(item);
            }
        }
        public static dynamic JArrayReverse(dynamic arr)
        {
            if (arr is null || arr is not JArray || GetCount(arr) == 0) return new JArray();
            var arr2 = new JArray();
            foreach (var item in arr)
            {
                arr2.Insert(0, item);
            }
            return arr2;
        }
        public static void JObjectAdd(ref dynamic jo, dynamic jo2)
        {
            if (jo2 == null || GetCount(jo2) == 0) return;
            jo = jo ?? new JObject();
            foreach (var item in jo2)
            {
                jo[item.Name] = item.Value;
            }
        }
        public static bool JArrayContains(dynamic arr, dynamic val)
        {
            if (arr == null || arr is not JArray || GetCount(arr) == 0) return false;
            var str = "";
            if (val is bool)
            {
                foreach (var item in arr)
                {
                    if (item is not JValue) continue;
                    str = item.ToString().ToLower();
                    if (val == (str == "true" || str == "1")) return true;
                }
            }
            //else if (val is int)
            //{
            //    foreach (var item in arr)
            //    {
            //        if (Convert.ToInt32(item) == val) return true;
            //    }
            //}
            else
            {
                foreach (var item in arr)
                {
                    if (item is not JValue) continue;
                    if (item.ToString() == val.ToString()) return true;
                }
            }

            return false;
        }

        #endregion
    }
}
