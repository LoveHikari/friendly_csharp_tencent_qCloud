using FsLib.Tencent.QCloud.SDK.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiDu.Common;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var v = FileHelper.FileToStream(@"D:\Program\Visual Studio\VA_X_Setup2258_0.exe");
            CosCloud cloud = new CosCloud(1256486294, "AKIDufCVaQQNUN0Xqv7LhogLlxvfAUaRGawK", "IMErFOJ0WyCC0ax86yc5KL7evwjGfp7A");
            string bucketName = "myyfxbucket123";
            cloud.UploadFile(bucketName, "/download/VA_X_Setup2258_0.exe", v);
        }
    }
}
