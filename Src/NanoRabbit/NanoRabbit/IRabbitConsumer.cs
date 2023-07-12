using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanoRabbit.NanoRabbit
{
    public interface IRabbitConsumer
    {
        public void Receive();
    }
}
