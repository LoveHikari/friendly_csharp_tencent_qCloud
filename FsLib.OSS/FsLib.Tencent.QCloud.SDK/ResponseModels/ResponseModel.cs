namespace FsLib.Tencent.QCloud.SDK.ResponseModels
{
    public class ResponseModel
    {
        /// <summary>
        /// 服务端返回码，如果没有发生任何错误取值为0；如果发生错误该参数指称具体的错误码。
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 服务端提示内容，如果发生错误该字段将详细描述发生错误的情况。
        /// </summary>
        public string Message { get; set; }
    }
}