using Microsoft.Extensions.DependencyInjection;
using NanoRabbit.Connection;
using NanoRabbit.DependencyInjection;

namespace AddRabbitPoolTest;

[TestClass]
public class RabbitPoolExtensionsTests
{
    [TestMethod]
    public void AddRabbitPool_ShouldRegisterServicesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var globalConfig = new GlobalConfig();
        Action<GlobalConfig> setupGlobalConfig = config => config.EnableLogging = true;
        Action<List<ConnectOptions>> setupAction = options =>
        {
            // options.Add(new ConnectOptions("FooConnection",
            //     config => { config.ConnectConfig = new ConnectConfig(connectConfig => { }); }));
            // options.Add(new ConnectOptions("BarConnection",
            //     config => { config.ConnectConfig = new ConnectConfig(connectConfig => { }); }));
        };

        // Act
        var result = services.AddRabbitPool(setupGlobalConfig, setupAction);

        // Assert
        Assert.IsNotNull(RabbitPoolExtensions.GlobalConfig);
        Assert.AreEqual(globalConfig.EnableLogging, RabbitPoolExtensions.GlobalConfig.EnableLogging);
        Assert.AreEqual(1, result.Count);
        Assert.IsInstanceOfType(result[0], typeof(ServiceDescriptor));
    }
}