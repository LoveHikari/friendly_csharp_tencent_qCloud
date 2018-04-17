﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace FsLib.Tencent.QCloud.SDK.Common
{
    /// <summary>
    /// 获得http信息
    /// </summary>
    public class HttpHelper
    {
        HttpWebRequest request;
        #region http请求

        /// <summary>
        /// http POST 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="postData">请求主体</param>
        /// <param name="chareset">编码，默认utf-8</param>
        /// <param name="headerItem"></param>
        /// <param name="cookie">cookie容器</param>
        /// <returns>响应的页面, 响应的cookie</returns>
        public string PostHttpWebRequest(string url, Dictionary<string, object> postData, string chareset = "utf-8", Hashtable headerItem = null, string cookie = "")
        {
            return HttpRequest(url, "POST", postData, chareset, headerItem, null, null, cookie).retHtml;
        }

        /// <summary>
        /// http GET 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="chareset">编码，默认utf-8</param>
        /// <param name="headerItem">请求头</param>
        /// <param name="cookie">cookie容器</param>
        /// <returns>响应的页面</returns>
        public string GetHttpWebRequest(string url, string chareset = "utf-8", Hashtable headerItem = null, string cookie = "")
        {
            return HttpRequest(url, "GET", null, chareset, headerItem, null, null, cookie).retHtml;
        }
        /// <summary>
        /// http GET 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="chareset">编码，默认utf-8</param>
        /// <param name="headerItem">请求头</param>
        /// <param name="cookie">cookie容器</param>
        /// <returns>响应的页面</returns>
        public Stream GetHttpWebRequest2(string url, string chareset = "utf-8", Hashtable headerItem = null, string cookie = "")
        {
            return HttpRequestStream(url, "GET", null, chareset, headerItem, null, null, cookie).responseStream;
        }


        #endregion
        private string GetFormData(Dictionary<string, object> param, Hashtable headerItem)
        {
            StringBuilder sb = new StringBuilder();
            if (param != null && headerItem != null)
            {
                if (headerItem["Content-Type"].ToString().Equals("multipart/form-data"))
                {
                    string boundary = DateTime.Now.Ticks.ToString("x");//元素分割标记
                    headerItem["Content-Type"] = $"multipart/form-data; boundary=---------------------------{boundary}";

                    foreach (KeyValuePair<string, object> pair in param)
                    {
                        sb.AppendLine("-----------------------------" + boundary);
                        sb.AppendLine($"Content-Disposition: form-data; name=\"{pair.Key}\"");
                        sb.AppendLine();
                        sb.AppendLine(pair.Value.ToString());
                    }
                    sb.AppendLine("-----------------------------" + boundary + "--");

                }
                else if (headerItem["Content-Type"].ToString().Contains("json"))
                {
                    sb.Append(JsonConvert.SerializeObject(param));
                }
                else
                {
                    if (param.Count > 0)
                    {
                        sb.Append("?");
                        foreach (KeyValuePair<string, object> pair in param)
                        {
                            sb.Append(pair.Key + "=" + pair.Value + "&");
                        }
                        sb.Remove(sb.Length - 1, 1);
                    }

                }
            }


            return sb.ToString();
        }
        /// <summary> 
        /// 上传文件 
        /// </summary> 
        /// <param name="url">提交的地址</param>
        /// <param name="param">请求body</param>
        /// <param name="localStream">文本流</param> 
        /// <param name="fileName">文件名称 </param>
        /// <param name="offset"></param>
        /// <param name="sliceSize">切片大小（字节）</param>
        /// <param name="headerItem">header</param>
        /// <param name="fileKey"></param> 
        /// <param name="cookie">cookie数据</param> 
        /// <returns>响应源码</returns> 
        public string HttpUploadFile(string url, Dictionary<string, object> param, Stream localStream, string fileName, long offset = -1, int sliceSize=0, Hashtable headerItem = null,string fileKey= "filecontent", string cookie = "")
        {
            // 这个可以是改变的，也可以是下面这个固定的字符串 
            string boundary = DateTime.Now.Ticks.ToString("x");//元素分割标记

            // 创建request对象
            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
            webrequest.Method = "POST";
            webrequest.CookieContainer = GetCookieContainer(url, cookie);
            SetHeaderValue(webrequest, headerItem);
            webrequest.ContentType = $"multipart/form-data; boundary=---------------------------{boundary}";

            // 构造发送数据 
            StringBuilder sb = new StringBuilder();
            if (param != null)
            {
                foreach (KeyValuePair<string, object> pair in param)
                {
                    sb.AppendLine("-----------------------------" + boundary);
                    sb.AppendLine($"Content-Disposition: form-data; name=\"{pair.Key}\"");
                    sb.AppendLine();
                    sb.AppendLine(pair.Value.ToString());
                }
            }

            sb.AppendLine("-----------------------------" + boundary);
            sb.AppendLine($"Content-Disposition: form-data; name=\"{fileKey}\"; filename=\"{fileName}\"");
            sb.AppendLine("Content-Type: application/octet-stream");
            sb.AppendLine();

            string postHeader = sb.ToString();
            byte[] postHeaderBytes = Encoding.GetEncoding("utf-8").GetBytes(postHeader);

            //构造尾部数据 
            byte[] boundaryBytes = Encoding.GetEncoding("utf-8").GetBytes("\r\n-----------------------------" + boundary + "--\r\n");

            long length = postHeaderBytes.Length + localStream.Length + boundaryBytes.Length;
            webrequest.ContentLength = length;

            Stream requestStream = webrequest.GetRequestStream();

            // 输入头部数据 
            requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);

            // 输入文件流数据 
            if (offset == -1)
            {
                var buffer = new byte[1024];
                int bytesRead;
                localStream.Seek(0, SeekOrigin.Begin);
                while ((bytesRead = localStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    requestStream.Write(buffer, 0, bytesRead);
                }
            }
            else
            {
                var buffer = new Byte[sliceSize];
                int bytesRead;
                localStream.Seek(offset, SeekOrigin.Begin);
                bytesRead = localStream.Read(buffer, 0, buffer.Length);
                requestStream.Write(buffer, 0, bytesRead);
            }
            localStream.Close();


            // 输入尾部数据 
            requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);

            Stream s;
            try
            {
                WebResponse responce = webrequest.GetResponse();
                s = responce.GetResponseStream();
            }
            catch (WebException wex)
            {
                s = wex.Response.GetResponseStream();
            }


            StreamReader sr = new StreamReader(s);

            // 返回数据流(源码) 
            return sr.ReadToEnd();
        }

        #region 私有方法
        /// <summary>
        /// 设置请求头
        /// </summary>
        /// <param name="request">HttpWebRequest对象</param>
        /// <param name="headerItem">header的属性对象</param>
        /// <example>调用说明：SetHeaderValue(request.Headers, headerItem);</example>
        private void SetHeaderValue(HttpWebRequest request, Hashtable headerItem)
        {
            request.ContentType = "text/html";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.101 Safari/537.36";
            request.Accept = "*/*";
            if (headerItem != null)
            {
                if (headerItem.ContainsKey("Content-Type"))
                {
                    request.ContentType = headerItem["Content-Type"].ToString();
                    headerItem.Remove("Content-Type");
                }
                if (headerItem.ContainsKey("User-Agent"))
                {
                    request.UserAgent = headerItem["User-Agent"].ToString();
                    headerItem.Remove("User-Agent");
                }
                if (headerItem.ContainsKey("Accept"))
                {
                    request.Accept = headerItem["Accept"].ToString();
                    headerItem.Remove("Accept");
                }

                var property = typeof(WebHeaderCollection).GetProperty("InnerCollection",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (property != null)
                {
                    var collection = property.GetValue(request.Headers, null) as System.Collections.Specialized.NameValueCollection;
                    foreach (var pair in headerItem.Keys)
                    {
                        if (collection != null) collection[pair.ToString()] = headerItem[pair].ToString();
                    }
                }
            }

        }
        /// <summary>
        /// 获得cookie容器
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="cookie">cookie字符串</param>
        private CookieContainer GetCookieContainer(string url, string cookie)
        {
            CookieContainer cookieContainer = new CookieContainer();
            if (cookie == null)
            {
                return cookieContainer;
            }
            string[] ss = cookie.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            CookieCollection cookieCollection = new CookieCollection();
            foreach (string s in ss)
            {
                string name = s.Trim().Split('=')[0];
                string value = s.Trim().Split('=')[1];
                cookieCollection.Add(new Cookie(name, value));
            }

            cookieContainer.Add(new Uri(url), cookieCollection);
            return cookieContainer;
        }
        /// <summary>
        /// gzip解压
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <remarks>https://wenku.baidu.com/view/dd321f1a59eef8c75fbfb352.html</remarks>
        private byte[] Decompress(Stream stream)
        {
            MemoryStream ms = new MemoryStream();

            GZipStream g = new GZipStream(stream, CompressionMode.Decompress);

            byte[] buf = new byte[1024 * 1024];
            int count;
            do
            {
                count = g.Read(buf, 0, buf.Length);
                ms.Write(buf, 0, count);
            } while (count > 0);

            return ms.ToArray();
        }

        /// <summary>
        /// http 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="qequest">请求方法，GET、POST</param>
        /// <param name="dataStr">请求主体</param>
        /// <param name="chareset">编码，默认utf-8</param>
        /// <param name="headerItem"></param>
        /// <param name="proxy">http代理设置</param>
        /// <param name="cookies">cookie容器</param>
        /// <param name="cookie">cookie</param>
        /// <returns>响应的页面, 响应的cookie</returns>
        /// <c>！注意：有时候请求会重定向，但我们就需要从重定向url获取东西，像QQ登录成功后获取sid，但上面的会自动根据重定向地址跳转。我们可以用:
        ///     request.AllowAutoRedirect = false;设置重定向禁用，你就可以从headers的Location属性中获取重定向地址</c>
        private (string retHtml, string cookies) HttpRequest(string url, string qequest, Dictionary<string, object> param, string chareset, Hashtable headerItem, WebProxy proxy, CookieContainer cookies, string cookie)
        {
            //HttpWebRequest request;
            //if (qequest == "GET")
            //    request = (HttpWebRequest)WebRequest.Create(url + (dataStr == "" ? "" : "?") + dataStr);
            //else
            //    request = (HttpWebRequest)WebRequest.Create(url);
            //request.Method = qequest;
            //if (qequest == "POST")
            //    request.ContentLength = Encoding.GetEncoding(chareset).GetByteCount(dataStr);
            //SetHeaderValue(request, headerItem);
            //request.Proxy = proxy;  //将其默认代理设置为空
            ////CookieContainer cookie = new CookieContainer();

            //request.CookieContainer = cookies ?? GetCookieContainer(url, cookie);
            //request.Timeout = 90000;
            //if (qequest == "POST")
            //{
            //    Stream myRequestStream = request.GetRequestStream();
            //    myRequestStream.Write(Encoding.GetEncoding(chareset).GetBytes(dataStr), 0, Encoding.GetEncoding(chareset).GetByteCount(dataStr));
            //    myRequestStream.Close();
            //}
            //request.ServicePoint.Expect100Continue = false;  //这个在Post的时候，一定要加上，如果服务器返回错误，他还会继续再去请求，不会使用之前的错误数据，做返回数据

            //HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            //response.Cookies = request.CookieContainer?.GetCookies(response.ResponseUri);
            //cookie = request.CookieContainer?.GetCookieHeader(response.ResponseUri);

            var v = HttpRequestStream(url, qequest, param, chareset, headerItem, proxy, cookies, cookie);
            using (Stream myResponseStream = v.responseStream)
            {
                if (v.request.Headers[HttpRequestHeader.AcceptEncoding]?.IndexOf("gzip", StringComparison.CurrentCultureIgnoreCase) > -1)
                {
                    string retString = System.Text.Encoding.GetEncoding(chareset).GetString(Decompress(myResponseStream));
                    //v.request.Abort();
                    return (retString, cookie);
                }
                else
                {
                    using (StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding(chareset)))
                    {
                        string retString = myStreamReader.ReadToEnd();
                        //v.request.Abort();
                        return (retString, cookie);
                    }
                }

            }


        }

        /// <summary>
        /// http 请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="qequest">请求方法，GET、POST</param>
        /// <param name="dataStr">请求主体</param>
        /// <param name="chareset">编码，默认utf-8</param>
        /// <param name="headerItem"></param>
        /// <param name="proxy">http代理设置</param>
        /// <param name="cookies">cookie容器</param>
        /// <param name="cookie">cookie</param>
        /// <returns>响应的页面, 响应的cookie</returns>
        /// <c>！注意：有时候请求会重定向，但我们就需要从重定向url获取东西，像QQ登录成功后获取sid，但上面的会自动根据重定向地址跳转。我们可以用:
        ///     request.AllowAutoRedirect = false;设置重定向禁用，你就可以从headers的Location属性中获取重定向地址</c>
        private (HttpWebRequest request, Stream responseStream, string cookies) HttpRequestStream(string url, string qequest, Dictionary<string, object> param, string chareset, Hashtable headerItem, WebProxy proxy, CookieContainer cookies, string cookie)
        {
            request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = qequest;

            string dataStr = GetFormData(param, headerItem);
            if (qequest == "POST")
                request.ContentLength = Encoding.GetEncoding(chareset).GetByteCount(dataStr);
            SetHeaderValue(request, headerItem);
            request.Proxy = proxy;  //将其默认代理设置为空
            //CookieContainer cookie = new CookieContainer();

            request.CookieContainer = cookies ?? GetCookieContainer(url, cookie);
            request.Timeout = 90000;
            if (qequest == "POST")
            {
                Stream myRequestStream = request.GetRequestStream();
                myRequestStream.Write(Encoding.GetEncoding(chareset).GetBytes(dataStr), 0, Encoding.GetEncoding(chareset).GetByteCount(dataStr));
                myRequestStream.Close();
            }
            request.ServicePoint.Expect100Continue = false;  //这个在Post的时候，一定要加上，如果服务器返回错误，他还会继续再去请求，不会使用之前的错误数据，做返回数据

            HttpWebResponse response;
            Stream myResponseStream;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                response.Cookies = request.CookieContainer?.GetCookies(response.ResponseUri);
                cookie = request.CookieContainer?.GetCookieHeader(response.ResponseUri);
                myResponseStream = response.GetResponseStream();
            }
            catch (WebException wex)
            {
                myResponseStream = wex.Response.GetResponseStream();
            }





            return (request, myResponseStream, cookie);
        }
        #endregion
    }

}