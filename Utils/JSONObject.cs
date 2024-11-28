using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RS.Tools.Common.Utils
{
    public class JSONObject
    {
        //private JToken m_raw;
        private Dictionary<string, JToken> dic = new Dictionary<string, JToken> { };

        public JSONObject() { }
        public JSONObject(string raw)
        {
            if (!string.IsNullOrEmpty(raw))
            {
                try
                {
                    ConvertJToken(JSONHelper.String2JToken(raw));
                }
                catch (Exception)
                {
                    return;
                }
            }
        }
        public JSONObject(JToken raw)
        {
            ConvertJToken(raw);
        }

        private void ConvertJToken(JToken src)
        {
            dic.Clear();
            if (src == null) return;

            string key;
            foreach (var item in src)
            {
                key = item.Path.Split(".").Last();
                dic.Add(key, JSONHelper.ParseJTokenToJToken(src, key));
            }
        }

        #region IN
        public void Reset(JToken src = null) => ConvertJToken(src);
        public void put(JToken src) => Absorb(src);
        public void put(string key, JToken val)
        {
            if (!dic.ContainsKey(key))
                dic.Add(key, val);
            else
                dic[key] = val;
        }
        public void put(string key, object val)
        {
            var jo = JsonConvert.DeserializeObject<JToken>(JsonConvert.SerializeObject(val, Formatting.Indented));
            if (!dic.ContainsKey(key))
                dic.Add(key, jo);
            else
                dic[key] = jo;
        }
        #endregion

        #region OUT

        public bool IsNull() => dic.Count == 0;
        public JToken GetJtoken() => JsonConvert.DeserializeObject<JToken>(JsonConvert.SerializeObject(dic, Formatting.Indented));
        public bool has(string key) => dic.ContainsKey(key);
        public override string ToString()
        {
            try
            {
                if (dic.Count == 0) return "";
                var jo = JsonConvert.DeserializeObject<JToken>(JsonConvert.SerializeObject(dic, Formatting.Indented));
                return JSONHelper.AnyObject2JString(jo);
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteException(ex, "JSONObject.ToString");
                return "";
            }
        }
        public JSONObject QueryNew(string path, JToken defaultVal = null) => new JSONObject(Query(path, defaultVal));
        public string[] GetKeys() => dic.Keys.ToArray();
        #endregion

        #region QUERY
        public void Delete(string path)
        {
            if (dic.ContainsKey(path))
                dic.Remove(path);
        }
        public JToken Query(string Path, JToken defaultVal = null)
        {
            if (dic.Count == 0)
                return defaultVal;
            if (string.IsNullOrEmpty(Path))
                return defaultVal;

            Path = Path.Replace("/", "\\");
            var paths = Path.Split("\\").ToList();
            var firstPath = paths.First();
            paths.RemoveAt(0);

            if (!dic.ContainsKey(firstPath)) return defaultVal;

            JToken result = dic[firstPath];

            foreach (var path in paths)
            {

                result = JSONHelper.ParseJTokenToJToken(result, path);
                if (result == null) return defaultVal;
            }
            return result;
        }
        /// <summary>
        /// 从一个包中按路径获取具体值（字符串）
        /// </summary>
        /// <param name="Path">路径</param>
        /// <param name="DefaultVal">结果为空时的取值</param>
        /// <returns></returns>
        public string QueryEx(string Path, string DefaultVal = "")
        {
            var result = Query(Path, DefaultVal);
            if (result == null) return DefaultVal;
            return JSONHelper.AnyObject2JString(result);
        }
        public bool QueryExBool(string path, bool DefaultVal = false)
        {
            try
            {
                var result = Query(path, DefaultVal);
                if (result == null) return DefaultVal;
                if (result.Count() == 0) return DefaultVal;
                if (!(result is JValue)) return DefaultVal;
                var str = JSONHelper.AnyObject2JString(result);
                if (StringHelper.IsNumeric(str))
                {
                    if (Convert.ToInt32(str) > 0)
                        return true;
                    else
                        return false;
                }
                else
                {
                    if (str.ToLower() == "true")
                        return true;
                    else
                        return false;
                }
            }
            catch (Exception)
            {
                Logger.Instance.WriteInfo("JSONObject", "QueryExBool abnormal result!");
                return DefaultVal;
            }
        }
        public void Absorb(JSONObject content)
        {
            if (content.IsNull()) return;
            Absorb(content.GetJtoken());
        }
        public void Absorb(JToken content)
        {
            if (content == null) return;
            if (content.Count() == 0) return;

            var key = "";

            foreach (var item in content)
            {
                key = item.Path.Split('.').Last();
                if (!dic.ContainsKey(key))
                    dic.Add(key, JSONHelper.ParseJTokenToJToken(content, key));
                else
                    dic[key] = JSONHelper.ParseJTokenToJToken(content, key);
            }
        }
        #endregion
    }
}
