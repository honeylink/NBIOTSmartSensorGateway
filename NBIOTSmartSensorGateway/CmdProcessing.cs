using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace NBIOTSmartSensorGateway
{
    public class CmdProcessing
    {
        public bool isHttp = false;//是否HTTP传输,默认为HTTPS协议传输（电信IOT平台API调用需要使用HTTPS进行访问）
        string iot_appId;//IOT平台应用ID
        string iot_appKey;//IOT平台应用秘钥
        string iot_appIp;//IOT平台IP
        int iot_appPort;//IOP平台端口
        string iot_p12Certfile;//证书名
        string iot_CertfilePwd;//证书私钥

        private static Object thisLock = new Object();

        /// <summary>
        /// 初始化信息
        /// </summary>
        /// <param name="platformIp">IOT平台IP地址</param>
        /// <param name="platformPort">IOT平台端口</param>
        /// <param name="appId">IOT平台应用ID</param>
        /// <param name="appKey">IOT平台应用秘钥</param>
        /// <param name="certFileName">证书名（p12）</param>
        /// <param name="certPwd">证书私钥</param>
        public CmdProcessing(string platformIp, int platformPort, string appId, string appKey, string certFileName, string certPwd)
        {
            this.iot_appIp = platformIp;
            this.iot_appPort = platformPort;
            this.iot_appId = appId;
            this.iot_appKey = appKey;
            this.iot_p12Certfile = certFileName;
            this.iot_CertfilePwd = certPwd;
        }

        /// <summary>
        /// 获取Token
        /// </summary>
        public TokenResult getToken()
        {
            TokenResult result = null;

            string apiPath = "/iocm/app/sec/v1.1.0/login";
            string body = "appId=" + this.iot_appId + "&secret=" + this.iot_appKey;
            string method = "POST";
            string contenttype = "application/x-www-form-urlencoded";
            WebHeaderCollection headers = new WebHeaderCollection();
            try
            {
                ApiResult apiResult = new ApiResult();
                apiResult = PostIotApi(apiPath, body, headers, method, contenttype, this.iot_p12Certfile, this.iot_CertfilePwd);
                //获取到api返回的结果集
                if (apiResult != null)
                {
                    TokenResult token = JsonConvert.DeserializeObject<TokenResult>(apiResult.result);
                    result = token;
                }
            }
            catch (Exception ex)
            {
                result = null;
            }
            return result;
        }

        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="token">token码</param>
        /// <param name="deviceId">设备ID</param>
        /// <param name="callbackurl">回调地址</param>
        /// <param name="expireTime">命令过期时间</param>
        /// <param name="serviceId">服务ID（profile文件中设置）</param>
        /// <param name="commandId">命令ID（profile文件中设置）</param>
        /// <param name="lsParas">命令参数</param>
        /// <returns></returns>
        public string sendCommand(string token, string deviceId, string callbackurl, int expireTime, string serviceId, string commandId, List<CommandPara> listParas)
        {
            lock (thisLock)
            {
                string result = null;
                //下发命令地址,省略IOT平台IP地址
                string apiPath = string.Format("/iocm/app/cmd/v1.4.0/deviceCommands?appId={0}", this.iot_appId);

                //下发命令对象赋值
                SendCommandRequest scr = new SendCommandRequest();
                scr.deviceId = deviceId;
                scr.callbackUrl = callbackurl;
                scr.expireTime = expireTime;
                scr.command = new Command();
                scr.command.method = commandId;
                scr.command.serviceId = serviceId;
                scr.command.paras = "#commandparas#";
                string body = JsonConvert.SerializeObject(scr);
                string parabody = "{";
                //遍历参数集合给command.paras赋值（json格式字符串）
                foreach (CommandPara item in listParas)
                {
                    if (item.isNum) { parabody += string.Format("\"{0}\":{1},", item.paraName, item.paraValue); }
                    else
                    {
                        parabody += string.Format("\"{0}\":\"{1}\",", item.paraName, item.paraValue);
                    }

                }
                parabody = parabody.TrimEnd(',') + "}";

                body = body.Replace("\"#commandparas#\"", parabody);

                //请求头赋值
                string method = "POST";
                string contenttype = "application/json";
                WebHeaderCollection headers = new WebHeaderCollection();
                headers.Add("app_key", this.iot_appId);//电信平台中的app_key、app_Id都指的是IOT平台应用ID,app_secret指的是IOT平台应用秘钥
                headers.Add("Authorization", "Bearer " + token);

                try
                {
                    ApiResult apiResult = new ApiResult();
                    //调用API执行方法
                    apiResult = PostIotApi(apiPath, body, headers, method, contenttype, this.iot_p12Certfile, this.iot_CertfilePwd);
                    if (apiResult != null)//调用成功,获取到数据
                    {
                        //获取返回的状态码
                        result = apiResult.statusCode.ToString();
                    }
                }
                catch (Exception ex)
                {
                    result = null;
                }
                return result;
            }
        }

        /// <summary>
        /// 进行IOT平台API调用
        /// </summary>
        /// <param name="apiPath">API方法名路径</param>
        /// <param name="bodyPostData">请求Body体</param>
        /// <param name="headers">请求头</param>
        /// <param name="method">请求方法</param>
        /// <param name="contenttype">请求类型</param>
        /// <param name="certFile">证书名</param>
        /// <param name="cerPwd">证书私钥</param>
        /// <returns></returns>
        private ApiResult PostIotApi(string apiPath, string bodyPostData, WebHeaderCollection headers, string method, string contenttype, string certFile, string cerPwd)
        {
            ApiResult apiResult = new ApiResult();
            try
            {
                string url = string.Format("https://{0}:{1}{2}", this.iot_appIp, this.iot_appPort, apiPath);
                if (isHttp)//HTTP传输
                {
                    url = string.Format("http://{0}:{1}{2}", this.iot_appIp, this.iot_appPort, apiPath);
                }
                //证书验证
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);
                //默认支持所有安全协议
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                X509Certificate2 cerCaiShang = new X509Certificate2(certFile, cerPwd);

                System.GC.Collect();
                System.Net.ServicePointManager.DefaultConnectionLimit = 200;
                HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create(url);

                if (!isHttp)//非HTTP协议传输需要验证证书
                {
                    httpRequest.ClientCertificates.Add(cerCaiShang);
                }

                //设置请求头参数
                httpRequest.Method = method;
                httpRequest.ContentType = contenttype;
                httpRequest.Referer = null;
                httpRequest.AllowAutoRedirect = true;
                httpRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.2; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
                httpRequest.Accept = "*/*";
                httpRequest.KeepAlive = false;

                for (int i = 0; i < headers.Count; i++)
                {
                    for (int j = 0; j < headers.GetValues(i).Length; j++)
                    {
                        httpRequest.Headers.Add(headers.Keys[i], headers.GetValues(i)[j]);
                    }
                }

                if (isHttp) { httpRequest.ServicePoint.Expect100Continue = false; }

                if (method != "GET")//非GET请求（电信IOT平台API调用基本上都是POST请求）
                {
                    //将body体写入流
                    Stream requestStem = httpRequest.GetRequestStream();
                    StreamWriter sw = new StreamWriter(requestStem);
                    sw.Write(bodyPostData);
                    sw.Close();
                }

                HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                //获取流
                Stream receiveStream = httpResponse.GetResponseStream();

                string result = string.Empty;

                //读取返回的流
                using (StreamReader sr = new StreamReader(receiveStream))
                {
                    result = sr.ReadToEnd();
                }

                //API返回结果赋值
                apiResult.result = result;//结果（一般返回的结果都为json格式）
                apiResult.statusCode = (int)httpResponse.StatusCode;//状态码

                //关闭流
                if (httpResponse != null)
                {
                    httpResponse.Close();
                }
                if (httpRequest != null)
                {
                    httpRequest.Abort();
                }

                return apiResult;
            }
            catch (Exception ex)
            {
                apiResult = null;
                return apiResult;
            }
        }

        /// <summary>
        /// 证书验证（默认通过）
        /// </summary>
        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            return true;
        }
    }
}
