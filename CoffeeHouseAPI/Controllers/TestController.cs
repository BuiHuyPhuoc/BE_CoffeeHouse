using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeHouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : TCHControllerBase
    {
        [HttpGet]
        [Route("Test")]
        public IActionResult Test() {
            return Ok();
        }

        [HttpGet]
        [Route("GetPort")]
        public IActionResult GetPort()
        {
            string port = this.GetUrlPort();
            return Ok(port);
        }
    }


}
