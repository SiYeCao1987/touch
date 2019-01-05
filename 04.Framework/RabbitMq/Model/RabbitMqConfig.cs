using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMq.Model
{
    /// <summary>
    /// RabbitMq的配置信息
    /// </summary>
    public class RabbitMqConfig
    {
        #region 服务器配置
        /// <summary>
        /// 服务器id地址集合
        /// </summary>
        public List<AmqpTcpEndpoint> AmqpLists { get; set; }
        /// <summary>
        /// 登录用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 登录密码
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 虚拟主机名称
        /// </summary>
        public string VirtualHost { get; set; }
        #endregion

        #region 队列
        /// <summary>
        /// 队列名称
        /// </summary>
        public string QueueName { get; set; }
        /// <summary>
        /// 是否持久化该队列
        /// </summary>
        public bool DurableQueue { get; set; }
        #endregion

        #region 交换机
        /// <summary>
        /// 路由名称
        /// </summary>
        public string ExchangeName { get; set; }
        /// <summary>
        /// 路由的类型枚举
        /// </summary>
        public ExchangeTypeEnum ExchangeType { get; set; }
        /// <summary>
        /// 路由的关键字
        /// </summary>
        public string RoutingKey { get; set; }
        #endregion

        #region 消息
        /// <summary>
        /// 是否持久化队列中的消息
        /// </summary>
        public bool DurableMessage { get; set; }
        #endregion
    }
}
