using Microsoft.AspNetCore.Mvc;
using NanoRabbit;

namespace Example.WebApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IRabbitHelper _rabbitHelper;
        private readonly ILogger<TestController> _logger;

        public TestController(IRabbitHelper rabbitHelper, ILogger<TestController> logger)
        {
            _rabbitHelper = rabbitHelper;
            _logger = logger;
        }

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet(Name = "PublishMessage")]
        public void PublishMessage()
        {
            var model = new WeatherForecast
            {
                Date = DateTime.Now,
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            };

            _rabbitHelper.Publish<WeatherForecast>("FooProducer", model);
        }
    }
}
