using FishMessengerServer.priv;
using Microsoft.AspNetCore.Mvc;

namespace FishMessengerServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IsAcceptController : ControllerBase
    {
        string user = string.Empty;
        bool accept = false;
        StreamReader testReader;
        public void Pass(string user, bool acpt)
        {
            this.user = user;
            accept = acpt;
        }

        [HttpGet(Name = "GetAcceptInfo")]
        public IEnumerable<isAccepted> Get()
        {
            return Enumerable.Range(1, 1).Select(index => new isAccepted
            {
                user = user,
                accepted = accept,
            })
            .ToArray();
        }
    }
}
