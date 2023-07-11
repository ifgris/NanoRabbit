// 发送消息
using NanoRabbit.NanoRabbit;
using System.Text;

var pool = new RabbitPool();
pool.RegisterConnection("Connection1", new ConnectOptions
{
    HostName = "localhost",
    Port = 5672,
    UserName = "admin",
    Password = "admin",
    VirtualHost = "DATA"
});

//while (true)
//{
//    pool.Send("Connection1", "BASIC.TOPIC", "BASIC.KEY", Encoding.UTF8.GetBytes("Hello, RabbitMQ!"));
//    //Task.Delay(200);
//}

while (true)
{
    // 接收消息
    pool.Receive("Connection1", "BASIC_QUEUE", body =>
    {
        Console.WriteLine(Encoding.UTF8.GetString(body));
    });
    Task.Delay(1000);
}