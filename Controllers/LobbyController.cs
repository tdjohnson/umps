using Microsoft.AspNetCore.Mvc;
using umps.Hubs;
using System.Collections.Concurrent;

namespace umps.Controllers
{

    [ApiController]
    public class LobbyController : ControllerBase
    {
        private static ConcurrentDictionary<string, string> playerNames = new ConcurrentDictionary<string, string>();

        [HttpGet]
        [Route("api/[controller]/GetPlayers")]
        public IActionResult GetPlayers()
        {
            return Ok(ControlHub.ConnectedClients);
        }

         [HttpPost]
        [Route("api/[controller]/SetPlayerName")]
        public IActionResult SetPlayerName([FromQuery] string id, [FromQuery] string name)
        {
            playerNames[id] = name;
            return Ok();
        }

        [HttpGet]
        [Route("api/[controller]/GetPlayerName")]
        public IActionResult GetPlayerName([FromQuery] string id)
        {
            if (playerNames.TryGetValue(id, out var name))
            {
                return Ok(name);
            }
            return NotFound("Player not found");
        }
    }
}