using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanoRabbit.Helper
{
    public interface IRabbitHelper
    {
        public void Publish<T>(string exchangeName, string routingKey, T message);
        public void DeclareQueue(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false, IDictionary<string, object>? arguments = null);
        public void AddConsumer(string queueName, Action<string> onMessageReceived, int consumers = 1);
    }
}
