using System.Collections.Generic;

namespace FsLib.Tencent.QCloud.SDK.ResponseModels
{
    /// <summary>
    /// 列出目录响应模型
    /// </summary>
    public class GetFolderListResponseModel
    {
        public string Context { get; set; }
        /// <summary>
        /// 是否有内容可以继续往后翻页
        /// </summary>
        public bool Listover { get; set; }
        /// <summary>
        /// 文件和文件夹列表，若当前目录下不存在文件或文件夹，则该返回值可能为空
        /// </summary>
        public List<FolderInfoModel> InfoList { get; set; }
    }

    public class FolderInfoModel
    {
        /// <summary>
        /// 文件名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 文件大小，当类型为文件时返回
        /// </summary>
        public int FileSize { get; set; }
        /// <summary>
        /// 文件已传输大小，当类型为文件时返回
        /// </summary>
        public int FileLen { get; set; }
        /// <summary>
        /// 文件 sha，当类型为文件时返回
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
        /// 生成的资源可访问的 cdn url，当类型为文件时返回
        /// </summary>
        public string AccessUrl { get; set; }
        /// <summary>
        /// 如果没有对文件单独设置该属性，则可能不会返回该字段。枚举值：
        /// eInvalid（表示继承 bucket 的读写权限）；
        /// eWRPrivate（私有读写）；
        /// eWPrivateRPublic（公有读私有写）。
        /// 说明：文件可以和 bucket 拥有不同的权限类型，已经设置过权限的文件如果想要撤销，将会直接被赋值为 eInvalid，即继承 bucket 的权限
        /// </summary>
        public string Authority { get; set; }
        /// <summary>
        /// 生成的资源可访问的源站 url，当类型为文件时返回
        /// </summary>
        public string SourceUrl { get; set; }
    }
}