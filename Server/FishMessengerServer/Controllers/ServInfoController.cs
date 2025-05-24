using FishMessengerServer.priv;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace FishMessengerServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ServInfoController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        string message = string.Empty;
        StreamReader testReader;
        string user = string.Empty;
        bool accept = false;

        public ServInfoController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetServInfo")]
        public IEnumerable<servInfo> Get()
        {
            return Enumerable.Range(1, 1).Select(index => new servInfo
            {
                authuser = user,
                authaccept = accept
            })
            .ToArray();
        }

        [HttpPost(Name = "PostServInfo")]
        public async Task Post()
        {
            testReader = new StreamReader(HttpContext.Request.Body);
            message = await testReader.ReadToEndAsync();
            Console.WriteLine("User attemped to connect. Information sent: " + message);
        }
    }
}
