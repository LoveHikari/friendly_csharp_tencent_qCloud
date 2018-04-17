using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using FsLib.Tencent.QCloud.SDK.Common;
using FsLib.Tencent.QCloud.SDK.ResponseModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FsLib.Tencent.QCloud.SDK.Api
{
    public class CosCloud
    {
        private const string COSAPI_CGI_URL = "http://sh.file.myqcloud.com/files/v2/";

        private const string REGION = "sh";
        //文件大于8M时采用分片上传,小于等于8M时采用单文件上传
        private const int SLICE_UPLOAD_FILE_SIZE = 8 * 1024 * 1024;

        private readonly int _appId;
        private readonly string _secretId;
        private readonly string _secretKey;
        private readonly HttpHelper _httpRequest;
        /// <summary>
        /// CosCloud 构造方法
        /// </summary>
        /// <param name="appId">授权appid</param>
        /// <param name="secretId">授权secret id</param>
        /// <param name="secretKey">授权secret key</param>
        /// <param name="timeOut">网络超时,默认60秒</param>
        public CosCloud(int appId, string secretId, string secretKey)
        {
            this._appId = appId;
            this._secretId = secretId;
            this._secretKey = secretKey;
            this._httpRequest = new HttpHelper();
        }

        #region 文件接口

        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param name="bucketName">bucket名称</param>
        /// <param name="remotePath">远程文件路径</param>
        /// <returns></returns>
        public GetFileStatResponseModel GetFileStat(string bucketName, string remotePath)
        {
            var url = GenerateURL(bucketName, remotePath);
            url = url + "?op=stat";
            var sign = Sign.Signature(_appId, _secretId, _secretKey, GetExpiredTime(), bucketName);
            Hashtable header = new Hashtable();
            header.Add("Authorization", sign);
            string json = _httpRequest.GetHttpWebRequest(url, "utf-8", header);
            JObject jo = JObject.Parse(json);
            if (jo["message"].ToString() == "SUCCESS")
            {
                GetFileStatResponseModel model = new GetFileStatResponseModel()
                {
                    BizAttr = jo["data"]["biz_attr"].ToString(),
                    FileSize = int.Parse(jo["data"]["filesize"].ToString()),
                    Sha = jo["data"]["sha"].ToString(),
                    Ctime = long.Parse(jo["data"]["ctime"].ToString()),
                    Mtime = long.Parse(jo["data"]["mtime"].ToString()),
                    AccessUrl = jo["data"]["access_url"].ToString(),
                    SourceUrl = jo["data"]["source_url"].ToString(),
                    Authority = jo["data"]["authority"].ToString()
                };
                return model;
            }
            return null;
        }
        /// <summary>
        /// 更新文件信息
        /// </summary>
        /// <param name="bucketName"> bucket名称</param>
        /// <param name="remotePath">远程文件路径</param>
        /// <param name="parameterDic">参数Dictionary</param>
        /// 包含如下可选参数：
        /// forbid:0允许访问 0x01进制访问 0x02进制写访问
        /// biz_attr:文件属性
        /// authority: eInvalid（继承bucket的权限）、eWRPrivate(私有读写)、eWPrivateRPublic(公有读私有写)
        /// 以下参数会打包到custom_headers对象中,携带到cos系统
        /// Cache-Control:
        /// Content-Type:
        /// Content-Disposition:
        /// Content-Language:
        /// x-cos-meta-: 以"x-cos-meta-"为前缀的参数
        /// <returns></returns>
        public ResponseModel UpdateFile(string bucketName, string remotePath, Dictionary<string, string> parameterDic = null)
        {
            var url = GenerateURL(bucketName, remotePath);
            var data = new Dictionary<string, object>();
            data.Add("op", "update");

            //接口中的flag统一cgi设置
            //将biz_attr设置到data中
            AddParameter(CosParameters.PARA_BIZ_ATTR, data, parameterDic);
            //将authority设置到data中
            AddAuthority(data, parameterDic);
            //将forbid设置到data中，这个不用设置flag
            AddParameter(CosParameters.PARA_FORBID, data, parameterDic);

            ////将customer_headers设置到data["custom_headers"]中
            AddCustomerHeaders(data, parameterDic);

            var sign = Sign.SignatureOnce(_appId, _secretId, _secretKey, (remotePath.StartsWith("/") ? "" : "/") + remotePath, bucketName);
            Hashtable header = new Hashtable();
            header.Add("Authorization", sign);
            header.Add("Content-Type", "application/json");
            string json = _httpRequest.PostHttpWebRequest(url, data, "utf-8", header);

            return JsonConvert.DeserializeObject<ResponseModel>(json);
        }
        /// <summary>
        /// 复制文件
        /// </summary>
        /// <param name="bucketName">bucket名称</param>
        /// <param name="remotePath">远程文件路径</param>
        /// <param name="destFileid">目标路径（不带路径则为当前路径下，带路径则会复制到携带指定的路径下）</param>
        /// <returns></returns>
        public ResponseModel CopyFile(string bucketName, string remotePath, string destFileid)
        {
            var url = GenerateURL(bucketName, remotePath);
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("op", "copy");
            data.Add("dest_fileid", destFileid);
            var sign = Sign.Signature(_appId, _secretId, _secretKey, GetExpiredTime(), bucketName);
            Hashtable header = new Hashtable();
            header.Add("Authorization", sign);

            header.Add("Content-Type", "multipart/form-data");
            string json = _httpRequest.PostHttpWebRequest(url, data, "utf-8", header);
            return JsonConvert.DeserializeObject<ResponseModel>(json);
        }
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="bucketName">bucket名称</param>
        /// <param name="remotePath">远程文件路径</param>
        /// <param name="localDir">本地文件目录</param>
        public void DownloadFile(string bucketName, string remotePath, string localDir)
        {
            if (!System.IO.Directory.Exists(localDir))
            {
                System.IO.Directory.CreateDirectory(localDir);
            }
            string path = System.IO.Path.Combine(localDir, System.IO.Path.GetFileName(remotePath));
            var url = $"http://{bucketName}-{_appId}.cos{REGION}.myqcloud.com{remotePath}";
            var sign = Sign.Signature(_appId, _secretId, _secretKey, GetExpiredTime(), bucketName);
            Hashtable header = new Hashtable();
            header.Add("Authorization", sign);

            using (Stream stream = _httpRequest.GetHttpWebRequest2(url, "utf-8", header))
            {
                //创建本地文件写入流
                using (Stream filestream = new FileStream(path, FileMode.Create))
                {
                    byte[] bArr = new byte[1024];
                    int size = stream.Read(bArr, 0, (int)bArr.Length);
                    while (size > 0)
                    {
                        filestream.Write(bArr, 0, size);
                        size = stream.Read(bArr, 0, (int)bArr.Length);
                    }
                }
            }

        }
        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param name="bucketName">bucket名称</param>
        /// <param name="remotePath">远程文件路径</param>
        /// <param name="destFileid">目标路径（不带路径则为当前路径下，带路径则会复制到携带指定的路径下）</param>
        /// <returns></returns>
        public ResponseModel MoveFile(string bucketName, string remotePath, string destFileid)
        {
            var url = GenerateURL(bucketName, remotePath);
            url = url + $"?to_over_write=0&dest_fileid={destFileid}&op=move";
            var sign = Sign.SignatureOnce(_appId, _secretId, _secretKey, (remotePath.StartsWith("/") ? "" : "/") + remotePath, bucketName);
            Hashtable header = new Hashtable();
            header.Add("Authorization", sign);
            string json = _httpRequest.PostHttpWebRequest(url, null, "utf-8", header);
            return JsonConvert.DeserializeObject<ResponseModel>(json);
        }
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="bucketName">bucket名称</param>
        /// <param name="remotePath">远程文件路径</param>
        /// <returns></returns>
        public ResponseModel DeleteFile(string bucketName, string remotePath)
        {
            var url = GenerateURL(bucketName, remotePath);
            var data = new Dictionary<string, object>();
            data.Add("op", "delete");
            var sign = Sign.SignatureOnce(_appId, _secretId, _secretKey, (remotePath.StartsWith("/") ? "" : "/") + remotePath, bucketName);
            var header = new Hashtable();
            header.Add("Authorization", sign);
            header.Add("Content-Type", "application/json");

            string json = _httpRequest.PostHttpWebRequest(url, data, "utf-8", header);
            return JsonConvert.DeserializeObject<ResponseModel>(json);
        }

        #endregion

        #region 目录接口

        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="bucketName">bucket名称</param>
        /// <param name="remoteDir">远程目录</param>
        /// <param name="bizAttr">目录属性</param>
        /// <returns></returns>
        public CreateFolderResponseModel CreateFolder(string bucketName, string remoteDir, string bizAttr = "")
        {
            remoteDir = HttpUtils.StandardizationRemotePath(remoteDir);

            var url = GenerateURL(bucketName, remoteDir);
            var data = new Dictionary<string, object>();
            data.Add("op", "create");
            data.Add(CosParameters.PARA_BIZ_ATTR, bizAttr);

            var sign = Sign.Signature(_appId, _secretId, _secretKey, GetExpiredTime(), bucketName);
            var header = new Hashtable();
            header.Add("Authorization", sign);
            header.Add("Content-Type", "application/json");
            string json = _httpRequest.PostHttpWebRequest(url, data, "utf-8", header);
            JObject jo = JObject.Parse(json);
            if (jo["message"].ToString() == "SUCCESS")
            {
                CreateFolderResponseModel model = new CreateFolderResponseModel()
                {
                    Ctime = long.Parse(jo["data"]["ctime"].ToString())
                };
                return model;
            }
            return null;
        }
        /// <summary>
        /// 查询目录属性
        /// </summary>
        /// <param name="bucketName">bucket名称</param>
        /// <param name="remoteDir">远程目录</param>
        /// <returns></returns>
        public GetFolderStatResponseModel GetFolderStat(string bucketName, string remoteDir)
        {
            remoteDir = HttpUtils.StandardizationRemotePath(remoteDir);

            GetFileStatResponseModel json = GetFileStat(bucketName, remoteDir);
            if (json != null)
            {
                GetFolderStatResponseModel model = new GetFolderStatResponseModel()
                {
                    BizAttr = json.BizAttr,
                    Ctime = json.Ctime,
                    Mtime = json.Mtime
                };
                return model;
            }
            return null;
        }

        /// <summary>
        /// 目录列表查询所有目录和文件,前缀搜索
        /// </summary>
        /// <param name="bucketName">bucket名称</param>
        /// <param name="remoteDir">远程目录</param>
        /// <param name="context">透传字段,用于翻页,前端不需理解,需要往后翻页则透传回来</param>
        /// <param name="prefix">读取文件/文件夹前缀</param>
        /// <param name="num">拉取的总数,最大1000,如果不带,则默认num=1000</param>
        /// <param name="listFlag">大于0返回全部数据，否则返回部分数据</param>
        /// <returns></returns>
        public GetFolderListResponseModel GetFolderList(string bucketName, string remoteDir, string context = null, string prefix = "", int num = 1000, int listFlag = 1)
        {
            remoteDir = HttpUtils.StandardizationRemotePath(remoteDir);

            var url = GenerateURL(bucketName, remoteDir, prefix);
            url = url + $"?op=list&num={num}&listFlag={listFlag}";
            if (context != null)
            {
                url += "&context" + context;
            }

            var sign = Sign.Signature(_appId, _secretId, _secretKey, GetExpiredTime(), bucketName);
            var header = new Hashtable();
            header.Add("Authorization", sign);

            string json = _httpRequest.GetHttpWebRequest(url, "utf-8", header);
            JObject jo = JObject.Parse(json);
            if (jo["message"].ToString() == "SUCCESS")
            {
                GetFolderListResponseModel model = new GetFolderListResponseModel()
                {
                    Context = jo["data"]["context"].ToString(),
                    Listover = bool.Parse(jo["data"]["listover"].ToString())
                };
                List<FolderInfoModel> infoList = new List<FolderInfoModel>();
                JToken jt = jo["data"]["infos"];
                foreach (JToken token in jt)
                {
                    infoList.Add(new FolderInfoModel()
                    {
                        Name = token["name"].ToString(),
                        FileSize = token["filesize"] == null ? 0 : int.Parse(token["filesize"].ToString()),
                        FileLen = token["filelen"] == null ? 0 : int.Parse(token["filelen"].ToString()),
                        Sha = token["sha"]?.ToString(),
                        Ctime = long.Parse(token["ctime"].ToString()),
                        Mtime = long.Parse(token["mtime"].ToString()),
                        AccessUrl = token["access_url"]?.ToString(),
                        Authority = token["authority"]?.ToString(),
                        SourceUrl = token["source_url"]?.ToString()
                    });
                }
                model.InfoList = infoList;

                return model;
            }
            return null;
        }
        /// <summary>
        /// 删除目录
        /// </summary>
        /// <param name="bucketName">bucket名称</param>
        /// <param name="remoteDir">远程目录</param>
        /// <returns></returns>
        public ResponseModel DeleteFolder(string bucketName, string remoteDir)
        {
            remoteDir = HttpUtils.StandardizationRemotePath(remoteDir);

            return DeleteFile(bucketName, remoteDir);
        }
        /// <summary>
        /// 更新目录信息
        /// </summary>
        /// <param name="bucketName"> bucket名称</param>
        /// <param name="remoteDir">远程目录</param>
        /// <param name="bizAttr">目录属性</param>
        /// <returns></returns>
        public ResponseModel UpdateFolder(string bucketName, string remoteDir, string bizAttr = "")
        {
            remoteDir = HttpUtils.StandardizationRemotePath(remoteDir);
            Dictionary<string, string> parameterDic = null;
            if (!string.IsNullOrWhiteSpace(bizAttr))
            {
                parameterDic = new Dictionary<string, string>();
                parameterDic.Add(CosParameters.PARA_BIZ_ATTR, bizAttr);
            }
            return UpdateFile(bucketName, remoteDir, parameterDic);
        }
        #endregion

        /// <summary>
        /// 文件上传
        /// 说明: 根据文件大小判断使用单文件上传还是分片上传,当文件大于8M时,内部会进行分片上传,可以携带分片大小sliceSize
        /// 其中分片上传使用SliceUploadInit SliceUploadData SliceUploadFinihs
        /// </summary>
        /// <param name="bucketName">bucket名称</param>
        /// <param name="remotePath">远程文件路径</param>
        /// <param name="localStream">本地文件流</param>
        /// <param name="bizAttr">文件属性</param>
        /// <param name="insertOnly">0:同名文件覆盖, 1:同名文件不覆盖,默认1</param>
        /// <param name="sliceSize">分片大小，可选取值为:64*1024 512*1024，1*1024*1024，2*1024*1024，3*1024*1024</param>
        /// <returns></returns>
        public UploadFileResponseModel UploadFile(string bucketName, string remotePath, Stream localStream, string bizAttr = "", int insertOnly = 1, int sliceSize = SLICE_SIZE.SLIZE_SIZE_1M)
        {
            var fileSize = localStream.Length;
            if (fileSize <= SLICE_UPLOAD_FILE_SIZE)  //当文件大于8M时,进行分片上传
            {
                return Upload(bucketName, remotePath, localStream, bizAttr, insertOnly);
            }
            else
            {
                //分片上传
                //sliceSize = GetSliceSize(sliceSize);
                return SliceUploadFile(bucketName, remotePath, localStream, bizAttr, insertOnly);
            }
        }


        /// <summary>
        /// 查询上传分片
        /// </summary>
        /// <param name="bucketName">bucket名称</param>
        /// <param name="remotePath">目标路径</param>
        /// <returns></returns>
        public UploadSliceListModel UploadSliceList(string bucketName, string remotePath)
        {
            var url = GenerateURL(bucketName, remotePath);
            var data = new Dictionary<string, object>();

            data.Add("op", "upload_slice_list");

            var header = new Hashtable();
            var sign = Sign.Signature(_appId, _secretId, _secretKey, GetExpiredTime(), bucketName);
            header.Add("Authorization", sign);
            header.Add("Content-Type", "multipart/form-data");

            string json = _httpRequest.PostHttpWebRequest(url, data, "utf-8", header);
            JObject jo = JObject.Parse(json);
            if (jo["message"].ToString() == "SUCCESS")
            {
                UploadSliceListModel model = new UploadSliceListModel()
                {
                    FileSize = int.Parse(jo["data"]["filesize"].ToString()),
                    ListParts = jo["data"]["listparts"].ToString(),
                    Session = jo["data"]["session"].ToString(),
                    SliceSize = int.Parse(jo["data"]["slice_size"].ToString())
                };
                return model;
            }
            return null;
        }

        /// <summary>
        /// 单个文件上传
        /// </summary>
        /// <param name="bucketName">bucket名称</param>
        /// <param name="remotePath">远程文件路径</param>
        /// <param name="localStream">本地文件流</param>
        /// <param name="bizAttr">COS 服务调用方自定义属性</param>
        /// <param name="insertOnly">同名文件覆盖选项，有效值：0 覆盖（删除已有的重名文件，存储新上传的文件）1 不覆盖（若已存在重名文件，则不做覆盖，返回“上传失败”）。默认为 1 不覆盖。</param>
        /// <returns></returns>
        public UploadFileResponseModel Upload(string bucketName, string remotePath, Stream localStream, string bizAttr = "", int insertOnly = 1)
        {
            var url = GenerateURL(bucketName, remotePath);
            var sha1 = SHA1.GetFileSHA1(localStream);
            var data = new Dictionary<string, object>();
            data.Add("op", "upload");
            data.Add("sha", sha1);
            data.Add("biz_attr", bizAttr);
            data.Add("insertOnly", insertOnly);

            var sign = Sign.Signature(_appId, _secretId, _secretKey, GetExpiredTime(), bucketName);
            var header = new Hashtable();
            header.Add("Authorization", sign);
            string json = _httpRequest.HttpUploadFile(url, data, localStream, System.IO.Path.GetFileName(remotePath), -1, 0, header);
            JObject jo = JObject.Parse(json);
            if (jo["message"].ToString() == "SUCCESS")
            {
                UploadFileResponseModel model = new UploadFileResponseModel()
                {
                    AccessUrl = jo["data"]["access_url"].ToString(),
                    ResourcePath = jo["data"]["resource_path"].ToString(),
                    SourceUrl = jo["data"]["source_url"].ToString(),
                    Url = jo["data"]["url"].ToString()
                };
                return model;
            }
            return null;
        }

        /// <summary>
        /// 初始化分片上传 init,如果上一次分片上传未完成，会返回{"code":-4019,"message":"_ERROR_FILE_NOT_FINISH_UPLOAD"}
        /// </summary>
        /// <param name="bucketName">bucket名称</param>
        /// <param name="remotePath">远程文件路径</param>
        /// <param name="localStream">本地文件流</param>
        /// <param name="fileSha">文件sha1</param>
        /// <param name="bizAttr"></param>
        /// <param name="insertOnly">是否覆盖同名文件</param>
        /// <returns></returns>
        private string SliceUploadInit(string bucketName, string remotePath, Stream localStream, string fileSha, string bizAttr = "", int insertOnly = 1)
        {
            int sliceSize = SLICE_SIZE.SLIZE_SIZE_1M;
            var url = GenerateURL(bucketName, remotePath);

            var fileSize = localStream.Length;

            var data = new Dictionary<string, object>();

            data.Add("op", "upload_slice_init");
            data.Add("filesize", fileSize);
            data.Add("sha", fileSha);
            data.Add("biz_attr", bizAttr);
            data.Add("slice_size", sliceSize);
            data.Add("insertOnly", insertOnly);
            string uploadParts = ComputeUploadParts(localStream, sliceSize);
            data.Add("uploadparts", uploadParts);


            var header = new Hashtable();
            var sign = Sign.Signature(_appId, _secretId, _secretKey, GetExpiredTime(), bucketName);
            header.Add("Authorization", sign);
            header.Add("Content-Type", "multipart/form-data");

            return _httpRequest.PostHttpWebRequest(url, data, "utf-8", header);
        }

        /// <summary>
        /// 逐个上传分片
        /// </summary>
        /// <param name="bucketName">bucket名称</param>
        /// <param name="remotePath">远程文件路径</param>
        /// <param name="localStream">本地文件流</param>
        /// <param name="fileSha">文件的sha1</param>
        /// <param name="session">init请求返回的session</param>
        /// <param name="offset">分片的偏移量</param>
        /// <param name="sliceSize">切片大小（字节）,默认为1M</param>
        /// <param name="sign">签名</param>
        /// <returns></returns>
        private string SliceUploadData(string bucketName, string remotePath, Stream localStream, string fileSha, string session, long offset, int sliceSize, string sign)
        {
            var url = GenerateURL(bucketName, remotePath);
            var data = new Dictionary<string, object>();

            data.Add("op", "upload_slice_data");
            data.Add("session", session);
            data.Add("offset", offset);
            data.Add("sha", fileSha);

            var header = new Hashtable();
            header.Add("Authorization", sign);

            return _httpRequest.HttpUploadFile(url, data, localStream, System.IO.Path.GetFileName(remotePath), offset, sliceSize, header);
        }

        /// <summary>
        /// 结束上传分片 finish
        /// </summary>
        /// <param name="bucketName">bucket名</param>
        /// <param name="remotePath">目标路径</param>
        /// <param name="localStream">本地文件流</param>
        /// <param name="fileSha">文件的sha1</param>
        /// <param name="session">init请求返回的session</param>
        /// <returns></returns>
        private UploadFileResponseModel SliceUploadFinish(string bucketName, string remotePath, Stream localStream, string fileSha, string session)
        {
            var url = GenerateURL(bucketName, remotePath);
            var fileSize = localStream.Length;
            var data = new Dictionary<string, object>();

            data.Add("op", "upload_slice_finish");
            data.Add("session", session);
            data.Add("fileSize", fileSize);
            data.Add("sha", fileSha);

            var header = new Hashtable();
            var sign = Sign.Signature(_appId, _secretId, _secretKey, GetExpiredTime(), bucketName);
            header.Add("Authorization", sign);
            header.Add("Content-Type", "multipart/form-data");
            string json = _httpRequest.PostHttpWebRequest(url, data, "utf-8", header);

            JObject jo = JObject.Parse(json);
            if (jo["message"].ToString() == "SUCCESS")
            {
                UploadFileResponseModel model = new UploadFileResponseModel()
                {
                    AccessUrl = jo["data"]["access_url"].ToString(),
                    ResourcePath = jo["data"]["resource_path"].ToString(),
                    SourceUrl = jo["data"]["source_url"].ToString(),
                    Url = jo["data"]["url"].ToString()
                };
                return model;
            }
            return null;
        }

        /// <summary>
        /// 计算上传部件
        /// </summary>
        /// <param name="localStream"></param>
        /// <param name="sliceSize"></param>
        /// <returns></returns>
        private string ComputeUploadParts(Stream localStream, int sliceSize)
        {
            try
            {
                byte[] buffer = new byte[sliceSize + 1];
                long offset = 0;    //文件的偏移
                long totalLen = 0;  //总共读取的字节数
                int readLen = 0;

                CosSha1Pure sha = new CosSha1Pure();
                StringBuilder jsonStr = new StringBuilder();

                jsonStr.Append("[");

                for (int i = 0; offset < localStream.Length; ++i, offset += readLen)
                {
                    localStream.Seek(offset, SeekOrigin.Begin);
                    readLen = localStream.Read(buffer, 0, sliceSize);
                    totalLen += readLen;
                    string dataSha;

                    sha.HashCore(buffer, 0, readLen);
                    if ((readLen < sliceSize) || (readLen == sliceSize && totalLen == localStream.Length))
                    {
                        //最后一片
                        dataSha = sha.FinalHex();
                    }
                    else
                    {
                        //中间的分片
                        dataSha = sha.GetDigest();
                    }

                    if (i != 0)
                    {
                        jsonStr.Append(",{\"offset\":" + offset + "," +
                                       "\"datalen\":" + readLen + "," +
                                       "\"datasha\":\"" + dataSha + "\"}");
                    }
                    else
                    {
                        jsonStr.Append("{\"offset\":" + offset + "," +
                                       "\"datalen\":" + readLen + "," +
                                       "\"datasha\":\"" + dataSha + "\"}");
                    }
                }


                jsonStr.Append("]");
                return jsonStr.ToString();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 分片上传
        /// </summary>
        /// <param name="bucketName">bucket名称</param>
        /// <param name="remotePath">远程文件路径</param>
        /// <param name="localStream">本地文件流</param>
        /// <param name="bizAttr">biz属性</param>
        /// <param name="insertOnly">是否覆盖同名文件</param>
        /// <returns></returns>
        public UploadFileResponseModel SliceUploadFile(string bucketName, string remotePath, Stream localStream, string bizAttr, int insertOnly = 1)
        {
            var fileSha = SHA1.GetFileSHA1(localStream);
            var fileSize = localStream.Length;
            var result = SliceUploadInit(bucketName, remotePath, localStream, fileSha, bizAttr, insertOnly);
            var obj = (JObject)JsonConvert.DeserializeObject(result);
            if ((int)obj["code"] != 0)
            {
                return new UploadFileResponseModel() { Message = result };
            }

            var data = obj["data"];
            if (data["access_url"] != null)
            {
                var accessUrl = data["access_url"];
                //Console.WriteLine("命中秒传" + accessUrl);
                return new UploadFileResponseModel() { Message = result };
            }

            int retryCount = 0;
            var session = data["session"].ToString();
            int sliceSize = (int)data["slice_size"];

            var sign = Sign.Signature(_appId, _secretId, _secretKey, GetExpiredTime(), bucketName);

            //总共重试三次
            for (long offset = 0; offset < fileSize; offset += sliceSize)
            {
                result = SliceUploadData(bucketName, remotePath, localStream, fileSha, session, offset, sliceSize, sign);
                obj = (JObject)JsonConvert.DeserializeObject(result);
                if ((int)obj["code"] != 0)
                {
                    if (retryCount < 10)
                    {
                        ++retryCount;
                        offset -= sliceSize;
                        //Console.WriteLine("重试...");
                        continue;
                    }
                    else
                    {
                        //Console.WriteLine("upload fail");
                        return new UploadFileResponseModel() { Message = result };
                    }
                }
            }

            return SliceUploadFinish(bucketName, remotePath, localStream, fileSha, session);
        }

        #region 私有方法
        /// <summary>
        /// 构造URL
        /// </summary>
        /// <returns></returns>
        private string GenerateURL(string bucketName, string remotePath)
        {
            string url = COSAPI_CGI_URL + this._appId + "/" + bucketName + HttpUtils.EncodeRemotePath(remotePath);
            return url;
        }
        /// <summary>
        /// 构造URL
        /// </summary>
        /// <returns></returns>
        private string GenerateURL(string bucketName, string remotePath, string prefix)
        {
            string url = COSAPI_CGI_URL + this._appId + "/" + bucketName + HttpUtils.EncodeRemotePath(remotePath) + HttpUtility.UrlEncode(prefix);
            return url;
        }

        /// <summary>
        /// 增加参数到data中
        /// </summary>
        /// <param name="paraName">键</param>
        /// <param name="data"></param>
        /// <param name="paras"></param>
        private void AddParameter(string paraName, Dictionary<string, object> data, Dictionary<string, string> paras)
        {
            if (data != null && paras != null && paraName != null)
            {
                if (paras.ContainsKey(paraName))
                {
                    string val = paras[paraName];
                    data.Add(paraName, val);
                }
            }

        }
        /// <summary>
        /// 增加参数authority到data中
        /// </summary>
        /// <returns></returns>
        private void AddAuthority(Dictionary<string, object> data, Dictionary<string, string> paras)
        {
            if (data != null && paras != null)
            {
                if (paras.ContainsKey(CosParameters.PARA_AUTHORITY))
                {
                    string val = paras[CosParameters.PARA_AUTHORITY];
                    if (val.Equals("eInvalid") || val.Equals("eWRPrivate") || val.Equals("eWPrivateRPublic"))
                    {
                        data.Add(CosParameters.PARA_AUTHORITY, val);
                    }
                }
            }
        }
        /// <summary>
        /// 增加用户自定义参数到data中
        /// </summary>
        /// <returns></returns>	
        private void AddCustomerHeaders(Dictionary<string, object> data, Dictionary<string, string> paras)
        {
            var customerHeaders = new Dictionary<string, object>();

            if (paras != null)
            {
                foreach (var dic in paras)
                {
                    if (IsCustomerHeader(dic.Key))
                    {
                        if (!customerHeaders.ContainsKey(dic.Key))
                        {
                            customerHeaders.Add(dic.Key, dic.Value);
                        }
                    }
                }
            }

            data.Add(CosParameters.PARA_CUSTOM_HEADERS, customerHeaders);
        }
        /// <summary>
        /// 判断是否为用户自定义参数
        /// </summary>
        /// <returns></returns>
        private bool IsCustomerHeader(string key)
        {
            if (key.Equals(CosParameters.PARA_CACHE_CONTROL)
                      || key.Equals(CosParameters.PARA_CONTENT_TYPE)
                      || key.Equals(CosParameters.PARA_CONTENT_DISPOSITION)
                      || key.Equals(CosParameters.PARA_CONTENT_LANGUAGE)
                      || key.StartsWith(CosParameters.PARA_X_COS_META_PREFIX))
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// 超时时间(当前系统时间+300秒)
        /// </summary>
        /// <returns></returns>	
        private long GetExpiredTime()
        {
            return DateTime.Now.ToUnixTime() / 1000 + 180;
        }
        /// <summary>
        /// 用户传入的slice_size进行规范,[64k,3m],大于1m必须是1m的整数倍
        /// </summary>
        /// <returns></returns>	
        private int GetSliceSize(int sliceSize)
        {
            int size;

            if (sliceSize < SLICE_SIZE.SLIZE_SIZE_64K)
            {
                size = SLICE_SIZE.SLIZE_SIZE_64K;
            }
            else if (sliceSize < SLICE_SIZE.SLIZE_SIZE_1M)
            {
                //size = SLICE_SIZE.SLIZE_SIZE_512K;
                size = sliceSize;
            }
            else if (sliceSize < SLICE_SIZE.SLIZE_SIZE_2M)
            {
                size = SLICE_SIZE.SLIZE_SIZE_1M;
            }
            else if (sliceSize < SLICE_SIZE.SLIZE_SIZE_3M)
            {
                size = SLICE_SIZE.SLIZE_SIZE_2M;
            }
            else
            {
                size = SLICE_SIZE.SLIZE_SIZE_3M;
            }

            return size;
        }
        #endregion

    }


}
