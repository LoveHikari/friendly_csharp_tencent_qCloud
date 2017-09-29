using System;
using System.IO;
using System.Security.Cryptography;

namespace FsLib.Tencent.QCloud.SDK.Common
{
    internal class SHA1
    {
        internal static string GetFileSHA1(string filePath)
        {
            var strResult = "";
            var strHashData = "";
            byte[] arrbytHashValue;
            FileStream oFileStream = null;
            SHA1CryptoServiceProvider osha1 = new SHA1CryptoServiceProvider();
            try
            {
                oFileStream = new FileStream(filePath.Replace("\"", ""), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                arrbytHashValue = osha1.ComputeHash(oFileStream); //计算指定Stream 对象的哈希值
                oFileStream.Close();
                //由以连字符分隔的十六进制对构成的String，其中每一对表示value 中对应的元素；例如“F-2C-4A”
                strHashData = BitConverter.ToString(arrbytHashValue);
                //替换-
                strHashData = strHashData.Replace("-", "");
                strResult = strHashData.ToLower();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return strResult;
        }

        internal static string GetFileSHA1(Stream stream)
        {
            var strResult = "";
            var strHashData = "";
            byte[] arrbytHashValue;
            MemoryStream oFileStream = new MemoryStream();
            stream.CopyTo(oFileStream);
            oFileStream.Seek(0, SeekOrigin.Begin);
            SHA1CryptoServiceProvider osha1 = new SHA1CryptoServiceProvider();
            try
            {
                arrbytHashValue = osha1.ComputeHash(oFileStream); //计算指定Stream 对象的哈希值
                oFileStream.Close();
                //由以连字符分隔的十六进制对构成的String，其中每一对表示value 中对应的元素；例如“F-2C-4A”
                strHashData = BitConverter.ToString(arrbytHashValue);
                //替换-
                strHashData = strHashData.Replace("-", "");
                strResult = strHashData.ToLower();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return strResult;
        }
    }
}
