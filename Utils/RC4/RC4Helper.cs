using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Tools.Common.Utils.RC4
{
    internal class RC4Helper
    {
        public static byte[] MyRC4(byte[] data, string key)
        {
            var KSA = (string _key) =>
            {
                var keyLen = key.Length;
                Dictionary<byte, byte> schedule = new();
                Dictionary<byte, byte> keyByte = new();
                for (byte i = 0; i <= 255; i++)
                {
                    schedule[i] = i;
                    if (i == 255) break;
                }

                for (byte i = 0; i < keyLen; i++)
                {
                    keyByte[i] = (byte)key[i];
                }

                byte j = 0;
                for (byte i = 0; j <= 255; i++)
                {
                    j = (byte)((j + schedule[i] + keyByte[(byte)(i % keyLen)]) % 256);
                    (schedule[i], schedule[j]) = (schedule[j], schedule[i]);
                    if (i == 255) break;
                }

                return schedule;
            };

            var PRGA = (Dictionary<byte, byte> schedule, int textLen) =>
            {
                byte i = 0, j = 0;
                Dictionary<byte, byte> k = new();
                for (byte n = 0; n < textLen; n++)
                {
                    i = (byte)((i + 1) % 256);
                    j = (byte)(j + schedule[i] % 256);
                    (schedule[i], schedule[j]) = (schedule[j], schedule[i]);
                    k[n] = schedule[(byte)((schedule[i] + schedule[j]) % 256)];
                }

                return k;
            };

            var output = (Dictionary<byte, byte> schedule, byte[] text) =>
            {
                var len = text.Length;
                var res = new Dictionary<byte, byte>();
                for (byte i = 0; i < len; i++)
                {
                    var c = text[i];
                    res[i] = (byte)(schedule[i] ^ c);
                }

                return res.Values.ToArray();
            };

            var s = KSA(key);
            var k = PRGA(s, data.Length);
            return output(k, data);
        }

        // rc4 解密具体实现
        public static byte[] Decrypt(byte[] data, string key)
        {
            var s = new byte[256];
            var k = new byte[256];
            var result = new byte[data.Length];
            for (var i = 0; i < 256; i++)
            {
                s[i] = (byte)i;
                k[i] = (byte)key[i % key.Length];
            }
            var j = 0;
            for (var i = 0; i < 256; i++)
            {
                j = (j + s[i] + k[i]) % 256;
                var temp = s[i];
                s[i] = s[j];
                s[j] = temp;
            }
            var i1 = 0;
            j = 0;
            for (var m = 0; m < data.Length; m++)
            {
                i1 = (i1 + 1) % 256;
                j = (j + s[i1]) % 256;
                var temp = s[i1];
                s[i1] = s[j];
                s[j] = temp;
                var t = (s[i1] + s[j]) % 256;
                result[m] = (byte)(data[m] ^ s[t]);
            }
            return result;
        }
    }
}
