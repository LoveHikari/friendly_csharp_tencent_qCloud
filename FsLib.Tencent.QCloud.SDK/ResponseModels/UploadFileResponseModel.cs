namespace FsLib.Tencent.QCloud.SDK.ResponseModels
{
    public class UploadFileResponseModel
    {
        public string Message { get; set; }
        /// <summary>
        /// 通过 CDN 访问该文件的资源链接（访问速度更快）
        /// </summary>
        public string AccessUrl { get; set; }
        /// <summary>
        /// 该文件在 COS 中的相对路径名，可作为其 ID 标识。格式/APPID/BucketName/ObjectName。推荐业务端存储 resource_path，然后根据业务需求灵活拼接资源 url（通过 CDN 访问 COS 资源的 url 和直接访问 COS 资源的 url 不同）。
        /// </summary>
        public string ResourcePath { get; set; }
        /// <summary>
        /// （不通过 CDN）直接访问 COS 的资源链接
        /// </summary>
        public string SourceUrl { get; set; }
        /// <summary>
        /// 操作文件的 url 。业务端可以将该 url 作为请求地址来进一步操作文件，对应 API ：文件属性、更新文件、删除文件、移动文件中的请求地址。
        /// </summary>
        public string Url { get; set; }
    }
}