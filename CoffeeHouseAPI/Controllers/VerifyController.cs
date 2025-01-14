using CoffeeHouseLib.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeHouseAPI.Controllers
{
    [Route("[controller]")]
    public class VerifyController : TCHControllerBase
    {
        readonly DbcoffeeHouseContext _context;

        public VerifyController(DbcoffeeHouseContext context)
        {
            _context = context;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        public async Task<IActionResult> Verify([FromQuery] string verify)
        {
            var account = await _context.Accounts.Where(x => x.VerifyToken == verify).FirstOrDefaultAsync();
            if (account == null)
            {
                return new ContentResult
                {
                    Content = "<p> <strong>Verify failed</strong> </p>",
                    ContentType = "text/html",
                    StatusCode = 400
                };
            }
            account.VerifyTime = DateTime.Now;
            await this.SaveChanges(_context);
            return new ContentResult
            {
                Content = "<p> <strong>Verify success</strong> </p>",
                ContentType = "text/html",
                StatusCode = 200
            };
        }
    }
}
