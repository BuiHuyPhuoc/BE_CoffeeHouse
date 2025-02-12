using CoffeeHouseAPI.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CoffeeHouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : TCHControllerBase
    {
        [HttpGet]
        [Route("Test")]
        public IActionResult Test()
        {
            return Ok();
        }

        [HttpGet]
        [Route("GetPort")]
        public IActionResult GetPort()
        {
            string port = this.GetUrlPort();
            return Ok(port);
        }

        [HttpPost]
        [Route("DemoAddRange")]
        public IActionResult DemoAddRange([FromBody] string request)
        {
            StringListSingleton.Instance.AddString(request);

            if (StringListSingleton.Instance.Strings.Count < 5)
            {
                return BadRequest("Not Enough: " + JsonSerializer.Serialize(StringListSingleton.Instance.Strings));
            }
            else
            {
                StringListSingleton.Instance.RemoveAll();
                return Ok(JsonSerializer.Serialize(StringListSingleton.Instance.Strings));
            }
        }
    }


}
