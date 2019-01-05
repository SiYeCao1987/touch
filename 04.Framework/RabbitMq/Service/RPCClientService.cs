using CommomUtils.Helper;
using RabbitMq.Dto;
using RabbitMq.Model;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMq.Service
{
    /// <summary>
    /// 客户端调用服务
    /// </summary>
    public class RPCClientService
    {
        private RabbitMqConfig _rabbitConfig { get; set; }
        private IConnection _connection;
        private IModel _channel;
        private ConnectionFactory _factory;
        private EventingBasicConsumer _consumer;
        private MQRespone _mqRespone;

        public RPCClientService(RabbitMqConfig rabbitMqConfig)
        {
            _rabbitConfig = rabbitMqConfig;
            _factory = new ConnectionFactory();
            _factory.UserName = _rabbitConfig.UserName;
            _factory.Password = _rabbitConfig.Password;
            _factory.VirtualHost = _rabbitConfig.VirtualHost;
            _connection = _factory.CreateConnection(_rabbitConfig.AmqpLists);
            _channel = _connection.CreateModel();
            _consumer = new EventingBasicConsumer(_channel);
            _mqRespone = new MQRespone() { SerialNumber = Guid.NewGuid().ToString() };
        }

        /// <summary>
        /// 生产消息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task<string> SendMsg(string message)
        {
            var tcs = new TaskCompletionSource<string>();
            try
            {
                if (string.IsNullOrEmpty(message))
                {
                    _mqRespone.Body = new Body { message = "消息不能为空" };
                    _mqRespone.ResponeStatusEnum = ResponeStatusEnum.FAILED;
                    tcs.TrySetResult(_mqRespone.ToJson());
                    return tcs.Task;
                }
                //推送消息转为二进制字节
                byte[] bytes = Encoding.UTF8.GetBytes(message);
                //声明交换机
                _channel.ExchangeDeclare(_rabbitConfig.ExchangeName, _rabbitConfig.ExchangeType.ToString(), _rabbitConfig.DurableMessage, false, null);
                //推送设置
                IBasicProperties properties = _channel.CreateBasicProperties();
                //支持可持久化数据
                properties.DeliveryMode = Convert.ToByte(_rabbitConfig.DurableMessage ? 2 : 1);
                //关联消息id
                var correlationId = Guid.NewGuid().ToString();
                //声明一个临时队列
                var replyQueue = _channel.QueueDeclare().QueueName;
                //指定回调队列
                properties.ReplyTo = replyQueue;
                //指定回调消息id
                properties.CorrelationId = correlationId;
                //消费者
                var callbackConsumer = new EventingBasicConsumer(_channel);
                //回调消息
                callbackConsumer.Received += (model, ea) =>
                {
                    //仅当消息回调的ID与发送的ID一致时，说明远程调用结果正确返回。
                    if (ea.BasicProperties.CorrelationId == correlationId)
                    {
                        _mqRespone.Body = new Body { message = Encoding.UTF8.GetString(ea.Body) };
                        _mqRespone.ResponeStatusEnum = ResponeStatusEnum.SUCCESS;
                        tcs.TrySetResult(_mqRespone.ToJson());
                    }
                };
                //消息推送
                _channel.BasicPublish(_rabbitConfig.ExchangeName, _rabbitConfig.RoutingKey, properties, bytes);
                //订阅回调消息
                _channel.BasicConsume(replyQueue, true, callbackConsumer);
                //返回结果
                return tcs.Task;
            }
            catch (Exception ex)
            {
                _mqRespone.Body = new Body { message = ex.Message };
                _mqRespone.ResponeStatusEnum = ResponeStatusEnum.ERROR;
                tcs.TrySetResult(_mqRespone.ToJson());
                return tcs.Task;
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            _connection.Close();
        }
    }
}
