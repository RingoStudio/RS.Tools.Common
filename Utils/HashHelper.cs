using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace RS.Tools.Common.Utils
{
    public class HashHelper
    {
        public static string MD5(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "";
            using (var hashmd5 = new MD5CryptoServiceProvider())
            {
                var bytes = hashmd5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(raw));
                return Convert.ToString(bytes).Replace("-", "");
            }
        }
    }
}
