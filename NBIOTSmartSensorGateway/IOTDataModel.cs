using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBIOTSmartSensorGateway
{

    /// <summary>
    /// IOT平台获取Token码实体对象
    /// </summary>
    public class TokenResult
    {
        /// <summary>
        /// 
        /// </summary>
        public string accessToken { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string tokenType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string refreshToken { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int expiresIn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string scope { get; set; }
    }

    /// <summary>
    /// IOT平台API调用返回结果实体对象
    /// </summary>
    public class ApiResult
    {
        public int statusCode;
        public string result;
        public string errcode;
        public string memo;
    }

    /// <summary>
    /// IOT平台下发命令中命令参数的实体类中的参数实体类（即Command.paras）
    /// </summary>
    public class CommandPara
    {
        public string paraName { get; set; }
        public string paraValue { get; set; }
        public bool isNum { get; set; }
    }

    /// <summary>
    /// IOT平台下发命令中命令参数的实体类（即SendCommandRequest.command）
    /// </summary>
    public class Command
    {
        /// <summary>
        /// 
        /// </summary>
        public string serviceId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string method { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string paras { get; set; }
    }

    /// <summary>
    /// IOT平台下发命令实体对象
    /// </summary>
    public class SendCommandRequest
    {
        /// <summary>
        /// 
        /// </summary>
        public string deviceId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Command command { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string callbackUrl { get; set; }

        public int expireTime { get; set; }
    }

    public enum ReturnState
    {
        /// <summary>
        /// 发送命令成功
        /// </summary>
        SendCmdSuccess = 201,
    }
}
