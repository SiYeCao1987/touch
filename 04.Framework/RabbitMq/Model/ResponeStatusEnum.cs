using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMq.Model
{
    /// <summary>
    /// 消息响应枚举
    /// </summary>
    public enum ResponeStatusEnum
    {
        /// <summary>
        /// 成功
        /// </summary>
        SUCCESS = 1,
        /// <summary>
        /// 失败
        /// </summary>
        FAILED = 2,
        /// <summary>
        /// 异常
        /// </summary>
        ERROR = 3
    }
}
