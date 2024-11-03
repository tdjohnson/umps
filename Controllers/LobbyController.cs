using Microsoft.AspNetCore.Mvc;

namespace umps.Controllers
{
    [ApiController]
    public class LobbyController : ControllerBase
    {
        [HttpGet]
        [Route("api/[controller]/GetPlayers")]
        public IActionResult GetPlayers()
        {
            return Ok("ListOfPlayers");
        }
    }
}