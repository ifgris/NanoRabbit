using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanoRabbit.Consumer
{
    public interface IRabbitConsumer
    {
        public void Receive();
    }
}
