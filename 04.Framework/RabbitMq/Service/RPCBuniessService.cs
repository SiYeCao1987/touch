using CommomUtils.Helper;
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
    /// 业务处理服务
    /// </summary>
    public class RPCBuniessService
    {
        private RabbitMqConfig _rabbitConfig { get; set; }
        //private IConnection _connection;
        //private IModel _channel;
        //private ConnectionFactory _factory;

        public RPCBuniessService(RabbitMqConfig rabbitMqConfig)
        {
            _rabbitConfig = rabbitMqConfig;
        }

        /// <summary>
        /// 消费消息
        /// </summary>
        /// <param name="method"></param>
        public void Receive(Func<string, bool> boolMethod)
        {
            try
            {
                var _factory = new ConnectionFactory();
                _factory.UserName = _rabbitConfig.UserName;
                _factory.Password = _rabbitConfig.Password;
                _factory.VirtualHost = _rabbitConfig.VirtualHost;
                using (var _connection = _factory.CreateConnection(_rabbitConfig.AmqpLists))
                {
                    using (var _channel = _connection.CreateModel())
                    {
                        //声明交换机
                        _channel.ExchangeDeclare(_rabbitConfig.ExchangeName, _rabbitConfig.ExchangeType.ToString(), _rabbitConfig.DurableMessage, false, null);
                        //声明队列
                        _channel.QueueDeclare(_rabbitConfig.QueueName, _rabbitConfig.DurableQueue, false, false, null);
                        //将队列和交换机绑定
                        _channel.QueueBind(_rabbitConfig.QueueName, _rabbitConfig.ExchangeName, _rabbitConfig.RoutingKey, arguments: null);
                        //一次接收一个消息，但是没有应答，则客户端不会收到下一个消息
                        _channel.BasicQos(0, 1, false);
                        //定义接收消息的消费者逻辑
                        EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);
                        consumer.Received += (model, ea) =>
                        {
                            byte[] body = ea.Body;
                            string bodyMsg = Encoding.UTF8.GetString(body);
                            //处理消息逻辑
                            bool result = boolMethod(bodyMsg);
                            //生成回调消息
                            var properties = ea.BasicProperties;
                            var replyProerties = _channel.CreateBasicProperties();
                            replyProerties.CorrelationId = properties.CorrelationId;
                            _channel.BasicPublish("", properties.ReplyTo, replyProerties, Encoding.UTF8.GetBytes(result.ToString()));
                            //确认处理消息
                            _channel.BasicAck(ea.DeliveryTag, false);
                        };
                        //消费消息
                        _channel.BasicConsume(_rabbitConfig.QueueName, false, consumer);
                        //阻塞函数，获取队列中的消息
                        System.Threading.Thread.Sleep(-1);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 消费消息
        /// </summary>
        /// <param name="method"></param>
        public void ReceiveAndCallBackObject(Func<string, object> objectMethod)
        {
            try
            {
                var _factory = new ConnectionFactory();
                _factory.UserName = _rabbitConfig.UserName;
                _factory.Password = _rabbitConfig.Password;
                _factory.VirtualHost = _rabbitConfig.VirtualHost;
                using (var _connection = _factory.CreateConnection(_rabbitConfig.AmqpLists))
                {
                    using (var _channel = _connection.CreateModel())
                    {
                        //声明交换机
                        _channel.ExchangeDeclare(_rabbitConfig.ExchangeName, _rabbitConfig.ExchangeType.ToString(), _rabbitConfig.DurableMessage, false, null);
                        //声明队列
                        _channel.QueueDeclare(_rabbitConfig.QueueName, _rabbitConfig.DurableQueue, false, false, null);
                        //将队列和交换机绑定
                        _channel.QueueBind(_rabbitConfig.QueueName, _rabbitConfig.ExchangeName, _rabbitConfig.RoutingKey, arguments: null);
                        //一次接收一个消息，但是没有应答，则客户端不会收到下一个消息
                        _channel.BasicQos(0, 1, false);
                        //定义接收消息的消费者逻辑
                        EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);
                        consumer.Received += (model, ea) =>
                        {
                            byte[] body = ea.Body;
                            string bodyMsg = Encoding.UTF8.GetString(body);
                            //处理消息逻辑
                            object o = objectMethod(bodyMsg);
                            //生成回调消息
                            var properties = ea.BasicProperties;
                            var replyProerties = _channel.CreateBasicProperties();
                            replyProerties.CorrelationId = properties.CorrelationId;
                            _channel.BasicPublish("", properties.ReplyTo, replyProerties, Encoding.UTF8.GetBytes(o.ToJson()));
                            //确认处理消息
                            _channel.BasicAck(ea.DeliveryTag, false);
                        };
                        //消费消息
                        _channel.BasicConsume(_rabbitConfig.QueueName, false, consumer);
                        //阻塞函数，获取队列中的消息
                        System.Threading.Thread.Sleep(-1);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
