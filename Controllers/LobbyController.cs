using Microsoft.AspNetCore.Mvc;
using umps.Hubs;

namespace umps.Controllers
{
    [ApiController]
    public class LobbyController : ControllerBase
    {
        [HttpGet]
        [Route("api/[controller]/GetPlayers")]
        public IActionResult GetPlayers()
        {
            return Ok(ControlHub.ConnectedClients);
        }
    }
}