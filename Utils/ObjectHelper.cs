using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RS.Tools.Common.Utils
{
    public static class ObjectHelper
    {
        public static T CreateInstance<T>()
        {
            return (T)CreateInstance(typeof(T));
        }

        public static object CreateInstance(this Type type)
        {
            return type.GetConstructors(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault((ConstructorInfo con) => con.GetParameters().Length == 0)?.Invoke(null) ?? FormatterServices.GetUninitializedObject(type);
        }

        public static T Convert<T>(this string input)
        {
            try
            {
                return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(input);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public static object Convert(this string input, Type type)
        {
            try
            {
                return TypeDescriptor.GetConverter(type).ConvertFromString(input);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
