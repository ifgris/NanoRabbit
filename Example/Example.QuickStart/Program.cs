using NanoRabbit.Helper;

var rabbitHelper = new RabbitHelper("localhost", virtualHost: "foo", userName: "admin", password: "admin");

rabbitHelper.Publish<string>("data.topic", "foo.key", "Hello from NanoRabbit");
