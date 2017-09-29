namespace FsLib.Tencent.QCloud.SDK.ResponseModels
{
    /// <summary>
    /// 查询目录属性响应模型
    /// </summary>
    public class GetFolderStatResponseModel
    {
        /// <summary>
        /// COS 服务调用方自定义属性，可通过 查询目录属性 获取该属性值
        /// </summary>
        public string BizAttr { get; set; }
        /// <summary>
        /// 创建时间，10 位 Unix 时间戳（UNIX时间是从协调世界时 1970 年 1 月 1 日 0 时 0 分 0 秒起的总秒数）
        /// </summary>
        public long Ctime { get; set; }
        /// <summary>
        /// 修改时间，10 位 Unix 时间戳（UNIX时间是从协调世界时 1970 年 1 月 1 日 0 时 0 分 0 秒起的总秒数）
        /// </summary>
        public long Mtime { get; set; }
    }
}