using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NBIOTSmartSensorGateway
{
    public partial class NBIOT_Demo : Form
    {
        //初始化信息
        private bool isHttp = false;
        private string appId = Global.appId;
        private string appKey = Global.appKey;
        private string appIP = Global.appIP;
        private int appPort = Global.appPort;
        private string serviceId = Global.serviceId;
        private string p12Certfile = Global.p12Certfile;
        private string p12CertfilePwd = Global.p12CertfilePwd;
        private string commandId = Global.commandId;
        private string callBackUrl = Global.callBackUrl;
        private int expireTime = Global.expireTime;

        CmdProcessing cmdProcess = null;

        public NBIOT_Demo()
        {
            InitializeComponent();
        }

        private void NBIOT_Demo_Load(object sender, EventArgs e)
        {
            ReceiveUplinkData();//上行
            SendCommandToIOT();//下行
        }

        #region 上行
        /// <summary>
        /// 接收电信平台推送的数据
        /// </summary>
        private void ReceiveUplinkData()
        {
            HttpListener httpListener;
            httpListener = new HttpListener();
            httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            httpListener.Prefixes.Add(Global.listeningAddress);//添加监听地址

            httpListener.Start();//开始监听

            new Thread(new ThreadStart(delegate
            {
                try
                {
                    //端口监听执行方法
                    ListeningProcessing(httpListener);
                }
                catch (Exception ex)
                {
                    this.Dispose();
                }
            })).Start();
        }

        /// <summary>
        /// 监听端口收到的数据进行处理
        /// </summary>
        /// <param name="httpListener">监听器</param>
        private static void ListeningProcessing(HttpListener httpListener)
        {
            NBIOT_Demo nb_IotMain = new NBIOT_Demo();
            while (true)
            {
                HttpListenerContext context = httpListener.GetContext();
                HttpListenerRequest hRequest = context.Request;
                HttpListenerResponse hResponse = context.Response;
                Stream stream = null;
                string result = "";
                byte[] res = Encoding.UTF8.GetBytes("200 OK");
                try
                {
                    //接收数据
                    stream = hRequest.InputStream;
                    StreamReader sr = new StreamReader(stream, Encoding.UTF8);

                    result = sr.ReadToEnd();

                    //在此处进行数据处理（数据格式转换、数据库存储等）
                    //数据转换：根据编解码插件中所定义的字段及电信上行数据返回字段进行转换
                    //电信平台返回的数据格式为json格式,注意json格式转换

                    //向电信平台返回指令
                    hResponse.OutputStream.Write(res, 0, res.Length);
                    hResponse.Close();
                }
                catch (Exception ex)
                {
                    hResponse.Close();
                }
            }
            httpListener.Close();
        }

        #endregion

        #region 下发
        /// <summary>
        /// 向电信IOT平台发送命令
        /// </summary>
        private void SendCommandToIOT()
        {
            new Thread(new ThreadStart(delegate
            {
                try
                {
                    while (true)
                    {
                        SendCommandProcessing();
                        Thread.Sleep(60000);
                    }
                }
                catch (Exception ex)
                {
                    this.Dispose();
                }
            })).Start();
        }

        /// <summary>
        /// 下发命令处理
        /// </summary>
        private void SendCommandProcessing()
        {
            //初始化信息
            cmdProcess = new CmdProcessing(appIP, appPort, appId, appKey, p12Certfile, p12CertfilePwd);

            try
            {
                TokenResult token = new TokenResult();
                //调用获取Token方法
                token = cmdProcess.getToken();
                if (token != null)
                {
                    string tokenCode = token.accessToken; //获取token码

                    //下发命令操作
                    //IOT平台应用中的设备ID（此处需要获取设备ID）
                    string deviceId = "";
                    //参数实体类赋值（需要下发的编解码插件中定义的字段值）
                    List<CommandPara> listParas = new List<CommandPara>();
                    //发送命令
                    string cmdResult = cmdProcess.sendCommand(tokenCode, deviceId, callBackUrl, expireTime, serviceId, commandId, listParas);
                    if (cmdResult == null)
                    {
                        //下发命令失败
                    }
                    else
                    {
                        if (cmdResult == ((int)ReturnState.SendCmdSuccess).ToString())
                        {
                            //下发命令成功
                            //可在此执行数据处理等操作
                        }
                        else
                        {
                            //下发命令失败
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        #endregion
    }
}
