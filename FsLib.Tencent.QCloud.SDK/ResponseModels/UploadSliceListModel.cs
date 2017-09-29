namespace FsLib.Tencent.QCloud.SDK.ResponseModels
{
    public class UploadSliceListModel
    {
        /// <summary>
        /// 文件大小
        /// </summary>
        public int FileSize { get; set; }
        /// <summary>
        /// 完全上传的文件分片信息
        /// </summary>
        public string ListParts { get; set; }
        /// <summary>
        /// 唯一标识此文件传输过程的 id，命中秒传则不携带
        /// </summary>
        public string Session { get; set; }
        /// <summary>
        /// 分片大小，单位为 Byte。有效值：524288 (512 KB), 1048576 (1 MB), 2097152 (2 MB), 3145728 (3 MB)
        /// </summary>
        public int SliceSize { get; set; }
    }
}