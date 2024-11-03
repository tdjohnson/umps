using Microsoft.AspNetCore.Mvc;

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
            return Ok("ListOfServers");
        }
    }
}