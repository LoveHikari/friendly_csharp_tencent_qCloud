using System;
using System.Collections.Generic;
using System.IO;
using FsLib.Tencent.QCloud.SDK.Api;
using FsLib.Tencent.QCloud.SDK.Common;

namespace ConsoleApp1
{
    class Program
    {
        const int APP_ID = 1253213493;
        const string SECRET_ID = "AKID9B1hJke5VpxtgJHh9QWQ7ezBC6sXC9tS";
        const string SECRET_KEY = "Wqz51lKhrpiPvHRPBk7IH4kEbmlZBbrv";

        static void Main(string[] args)
        {

                var result = "";

                const string bucketName = "mybucket";
                const string localPath = @"D:\Program\Visual Studio\Image\599cda83dbc93.jpg";
                const string remotePath = "/data/1.png";
                const string folder = "/upload/photo/";

                //创建cos对象
                var cos = new CosCloud(APP_ID, SECRET_ID, SECRET_KEY);


                //         //创建文件夹
                //         result = cos.CreateFolder(bucketName, folder);
                //         Console.WriteLine("创建文件目录："+result);

                //       	//目录更新
                //       	var updateParasDic = new Dictionary<string, string>();                
                //updateParasDic.Add(CosParameters.PARA_BIZ_ATTR,"new attribute");
                //result = cos.UpdateFolder(bucketName, folder, updateParasDic);
                //Console.WriteLine("目录更新:"+result);


                //            //获取文件夹属性
                //            result = cos.GetFolderStat(bucketName, folder);
                //Console.WriteLine("查询文件夹属性:"+result);


                //上传文件（不论文件是否分片，均使用本接口）
                //var uploadParasDic = new Dictionary<string, string>();
                //uploadParasDic.Add(CosParameters.PARA_BIZ_ATTR, "");
                //uploadParasDic.Add(CosParameters.PARA_INSERT_ONLY, "0");
                ////uploadParasDic.Add(CosParameters.PARA_SLICE_SIZE,SLICE_SIZE.SLIZE_SIZE_3M.ToString());
                //FileStream stream = new FileStream(localPath,FileMode.Open);
                //result = cos.UploadFile(bucketName, remotePath, stream, uploadParasDic);
                //Console.WriteLine("上传文件:" + result);


                //           //获取文件属性
                //           result = cos.GetFileStat(bucketName, remotePath);
                //           Console.WriteLine("获取文件属性:" + result);

                //获取文件列表
                //var foldListParasDic = new Dictionary<string, string>();
                //foldListParasDic.Add(CosParameters.PARA_NUM, "100");
                //result = cos.GetFolderList(bucketName, folder, foldListParasDic);
                //Console.WriteLine("获取文件列表:" + result);


                // //设置可选参数
                // var optionParasDic = new Dictionary<string, string>();
                // optionParasDic.Add(CosParameters.PARA_BIZ_ATTR,"new attribute");                
                // optionParasDic.Add(CosParameters.PARA_AUTHORITY,AUTHORITY.AUTHORITY_PRIVATEPUBLIC);
                // optionParasDic.Add(CosParameters.PARA_CACHE_CONTROL,"no");
                // optionParasDic.Add(CosParameters.PARA_CONTENT_TYPE,"application/text");
                // optionParasDic.Add(CosParameters.PARA_CONTENT_DISPOSITION,"inline filename=\"QC-7677.pdf\"");
                // optionParasDic.Add(CosParameters.PARA_CONTENT_LANGUAGE,"en");
                // optionParasDic.Add("x-cos-meta-test","test");

                // //更新文件
                // result = cos.UpdateFile(bucketName, remotePath, optionParasDic);
                // Console.WriteLine("更新文件属性" + result);
                FileStream fileStream = new FileStream(@"C:\Users\Administrator\Desktop\产品图片\图标.png", FileMode.Open);
                // //获取文件属性
                var v = cos.Upload(bucketName, "/data/3.png", fileStream);
                Console.WriteLine("获取文件属性:" + v);


                // //删除文件
                // result = cos.DeleteFile(bucketName, remotePath);
                // Console.WriteLine("删除文件:" + result);


            ////目录列表
            //var folderlistParasDic = new Dictionary<string, string>();
            //folderlistParasDic.Add(CosParameters.PARA_NUM, "100");
            //folderlistParasDic.Add(CosParameters.PARA_ORDER, "0");
            //folderlistParasDic.Add(CosParameters.PARA_PATTERN, FolderPattern.PATTERN_BOTH);
            //result = cos.GetFolderList(bucketName, folder, folderlistParasDic);
            //Console.WriteLine("查询目录列表:" + result);


            ////删除文件夹
            //result = cos.DeleteFolder(bucketName, folder);
            //Console.WriteLine("删除文件夹:" + result);



            Console.ReadKey();
        }
    }
}
