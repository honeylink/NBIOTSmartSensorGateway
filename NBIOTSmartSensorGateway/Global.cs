using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBIOTSmartSensorGateway
{
    public class Global
    {
        /// <summary>
        /// 监听的IP地址（调试可使用HTTP地址,正式使用请使用HTTPS地址）
        /// </summary>
        public static string listeningAddress = ConfigurationManager.AppSettings["ListeningAddress"].ToString();

        /// <summary>
        /// IOT平台应用ID
        /// </summary>
        public static string appId = ConfigurationManager.AppSettings["AppId"].ToString();

        /// <summary>
        /// IOT平台应用秘钥
        /// </summary>
        public static string appKey = ConfigurationManager.AppSettings["AppKey"].ToString();

        /// <summary>
        /// IOT平台IP地址
        /// </summary>
        public static string appIP = ConfigurationManager.AppSettings["AppIP"].ToString();

        /// <summary>
        /// IOT平台端口
        /// </summary>
        public static int appPort = int.Parse(ConfigurationManager.AppSettings["AppPort"].ToString());

        /// <summary>
        /// 证书
        /// </summary>
        public static string p12Certfile = ConfigurationManager.AppSettings["p12Certfile"].ToString();

        /// <summary>
        /// 证书私钥
        /// </summary>
        public static string p12CertfilePwd = ConfigurationManager.AppSettings["p12CertfilePwd"].ToString();

        /// <summary>
        /// 回调地址（下发命令后IOT平台会返回成功或失败等信息到此地址,调试可使用HTTP地址,正式使用请使用HTTPS地址）
        /// </summary>
        public static string callBackUrl = ConfigurationManager.AppSettings["CallBackUrl"].ToString();

        /// <summary>
        /// 命令过期时间
        /// </summary>
        public static int expireTime = int.Parse(ConfigurationManager.AppSettings["ExpireTime"].ToString());

        /// <summary>
        /// 服务ID（profile文件中设置）
        /// </summary>
        public static string serviceId = ConfigurationManager.AppSettings["ServiceId"].ToString();

        /// <summary>
        /// 命令ID（profile文件中设置）
        /// </summary>
        public static string commandId = ConfigurationManager.AppSettings["CommandId"].ToString();

    }
}
