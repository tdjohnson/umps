using Microsoft.AspNetCore.Mvc;
using umps.Models;

namespace umps.Controllers
{
    [ApiController]
    public class ServerController : ControllerBase
    {
        [HttpGet]
        [Route("api/[controller]/GetName")]
        public IActionResult GetName()
        {
            return Ok("MyServerName");
        }

        [HttpGet]
        [Route("api/[controller]/GetServers")]
        public IActionResult GetServer()
        {
            var servers = new List<Server>
            {
                new Server { defaultUrl = "https://umps.tdj23.com/controlhub", baseUrl = "https://umps.tdj23.com" },
                new Server { defaultUrl = "https://umps2.tdj23.com/controlhub", baseUrl = "https://umps2.tdj23.com" }
            };
            return Ok(servers);
        }
    }
}