using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// using Ionic.Zlib;
namespace RS.Tools.Common.Utils
{
    public class ZlibHelper
    {
        public static string CompressStr(string Text)
        {
            if (string.IsNullOrEmpty(Text))
                return "";
            byte[] Result = ZlibStream.CompressString(Text);
            return Convert.ToBase64String(Result);
        }

        //public static byte[] Compress(byte[] data)
        //{
        //    if (data == null || data.Length <= 0) return data;
        //    using var raw = new MemoryStream(data); // 这里举例用的是内存中的数据；需要对文本进行压缩的话，使用 FileStream 即可
        //    using var output = new MemoryStream();
        //    using var deflateStream = new ZLibStream(output, CompressionMode.Compress, true);
        //    raw.CopyTo(deflateStream); // 用 CopyTo 将需要压缩的数据一次性输入；也可以使用Write进行部分输入
        //    deflateStream.Close();  // 在Close中，会先后执行 Finish 和 Flush 操作。
        //    byte[] result = output.ToArray();
        //    return result;
        //}
        public static byte[] Compress(byte[] Bytes)
        {
            try
            {
                if (Bytes == null)
                    return null;
                byte[] Result = ZlibStream.CompressBuffer(Bytes);
                return Result;
            }
            catch (Exception)
            {
                return null;
            }

        }


        public static string Decompress(string Text)
        {
            if (string.IsNullOrEmpty(Text)) return "";
            byte[] ByteArr = Convert.FromBase64String(Text);
            return ZlibStream.UncompressString(ByteArr);
        }
        public static byte[] Decompress(byte[] Bytes)
        {
            if (Bytes == null) return null;
            if (Bytes.Length == 0)
                return Bytes;
            byte[] ByteArr = Bytes;
            try
            {
                return ZlibStream.UncompressBuffer(ByteArr);
            }
            catch (Exception)
            {
                return ByteArr;
            }
        }
        //public static byte[] Decompress(byte[] data)
        //{
        //    if (data == null || data.Length <= 0) return data;
        //    using var raw = new MemoryStream(data); // 这里举例用的是内存中的数据；需要对文本进行压缩的话，使用 FileStream 即可
        //    using var output = new MemoryStream();
        //    using var deflateStream = new ZLibStream(output, CompressionMode.Decompress,true);
        //    raw.CopyTo(deflateStream); // 用 CopyTo 将需要压缩的数据一次性输入；也可以使用Write进行部分输入
        //    deflateStream.Close();  // 在Close中，会先后执行 Finish 和 Flush 操作。
        //    byte[] result = output.ToArray();
        //    return result;
        //}
    }
}
