using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Tools.Common.Utils
{
    public class WechatFileHelper
    {
        private static Dictionary<string, byte[]> _imagePrefixs = new Dictionary<string, byte[]>
        {
            { "jpeg", new byte[]{ 0xff, 0xd8, 0xff } },
            { "png", new byte[]{ 0x89, 0x50, 0x4e, 0x47 } },
            { "gif", new byte[]{ 0x47, 0x49, 0x46, 0x38 } },
            { "tif", new byte[]{ 0x49, 0x49, 0x2a, 0x00 } },
            { "bmp", new byte[]{ 0x42, 0x4d } },
        };

        //public static string? EncodeWechatImageDat(string sourcePath, string targetDirectory, string? targetFileName = null)
        //{

        //}

        /// <summary>
        /// 将微信图片DAT转换为图片文件
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="targetDirectory"></param>
        /// <param name="targetFileName"></param>
        /// <returns></returns>
        public static string? DecodeWechatImageDat(string sourcePath, string targetDirectory, string? targetFileName = null)
        {
            try
            {
                if (!System.IO.File.Exists(sourcePath)) return null;
                if (string.IsNullOrEmpty(targetFileName)) targetFileName = IOHelper.GetFileNameWithoutExtension(sourcePath);
                var targetPath = System.IO.Path.Combine(targetDirectory, targetFileName);

                var data = System.IO.File.ReadAllBytes(sourcePath);

                var key = GetImageDatExtension(data);
                if (key.key is null || key.extension is null) return null;

                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = (byte)(data[i] ^ key.key);
                }

                targetPath = $"{targetPath}.{key.extension}";

                System.IO.File.WriteAllBytes(targetPath, data);

                return targetPath;
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteException(ex, "WechatFileHelper.DecodeWechatImageDat");
                return null;
            }
        }

        private static (string? extension, byte? key) GetImageDatExtension(byte[] data)
        {
            if (data is null || data.Length < 4) return (null, null);
            string extension = null;
            byte? key;
            foreach (var item in _imagePrefixs)
            {
                key = (byte)(item.Value[0] ^ data[0]);
                extension = item.Key;
                for (int i = 1; i < item.Value.Length; i++)
                {
                    if ((item.Value[i] ^ data[i]) != key)
                    {
                        key = null;
                        extension = null;
                        break;
                    }
                }

                if (key is not null && extension is not null) return (extension, key);
            }

            return (null, null);
        }
    }
}
