using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Tools.Common.extension
{
    public static class IEnumrableExtension
    {
        public static List<T> DeepCopy<T>(this List<T> list)
        {
            var ret = new List<T>();
            if (list == null || list.Count == 0) return ret;
            foreach (var item in list) ret.Add(item);
            return ret;
        }

        public static Dictionary<TKey, TValue> DeepCopy<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            var ret = new Dictionary<TKey, TValue>();
            if (dictionary == null || dictionary.Count == 0) return ret;
            foreach (var item in dictionary) ret.Add(item.Key, item.Value);
            return ret;
        }

        public static List<T> Remove<T>(this List<T> list, params T[] items)
        {
            if (list == null || list.Count == 0) return list;
            foreach (var item in items) list.Remove(item);
            return list;
        }

        public static List<string> Keys(this JObject src)
        {
            var ret = new List<string>();
            foreach (var kv in src) ret.Add(kv.Key);
            return ret;
        }
    }
}
