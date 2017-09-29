namespace FsLib.Tencent.QCloud.SDK.ResponseModels
{
    /// <summary>
    /// 获取文件信息响应模型
    /// </summary>
    public class GetFileStatResponseModel
    {
        /// <summary>
        /// COS 服务调用方自定义属性
        /// </summary>
        public string BizAttr { get; set; }
        /// <summary>
        /// 文件大小
        /// </summary>
        public int FileSize { get; set; }
        /// <summary>
        /// 文件 SHA-1 校验码
        /// </summary>
        public string Sha { get; set; }
        /// <summary>
        /// 创建时间，10 位 Unix 时间戳（UNIX 时间是从协调世界时 1970 年 1 月 1 日 0 时 0 分 0 秒起的总秒数）
        /// </summary>
        public long Ctime { get; set; }
        /// <summary>
        /// 修改时间，10 位 Unix 时间戳（UNIX 时间是从协调世界时 1970 年 1 月 1 日 0 时 0 分 0 秒起的总秒数）
        /// </summary>
        public long Mtime { get; set; }
        /// <summary>
        /// 通过 CDN 访问该文件的资源链接
        /// </summary>
        public string AccessUrl { get; set; }
        /// <summary>
        /// （不通过 CDN ）直接访问 COS 的资源链接
        /// </summary>
        public string SourceUrl { get; set; }
        /// <summary>
        /// Object 的权限，默认与 Bucket 权限一致，此时不会返回该字段。如果设置了独立权限，则会返回该字段。有效值：eInvalid 空权限，此时系统会默认调取 Bucket 权限 eWRPrivate 私有读写 eWPrivateRPublic 公有读私有写
        /// </summary>
        public string Authority { get; set; }
    }
}