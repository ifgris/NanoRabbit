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
pool.RegisterConnection("Connection2", new ConnectOptions
{
    HostName = "localhost",
    Port = 5672,
    UserName = "admin",
    Password = "admin",
    VirtualHost = "HOST"
});

while (true)
{
    pool.Send("Connection1", "BASIC.TOPIC", "BASIC.KEY", Encoding.UTF8.GetBytes("Hello from conn 1"));
    Task.Delay(1000);    
    
    pool.Send("Connection2", "BASIC.TOPIC", "BASIC.KEY", Encoding.UTF8.GetBytes("Hello from conn 2"));
    Task.Delay(1000);
}

//while (true)
//{
//    // 接收消息
//    pool.Receive("Connection1", "BASIC_QUEUE", body =>
//    {
//        Console.WriteLine(Encoding.UTF8.GetString(body));
//    });
//    Task.Delay(1000);
//}