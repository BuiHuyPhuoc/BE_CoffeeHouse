using AutoMapper;
using CoffeeHouseAPI.DTOs;
using CoffeeHouseAPI.DTOs.Product;
using CoffeeHouseLib.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace CoffeeHouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : TCHControllerBase
    {
        readonly DbcoffeeHouseContext _context;
        readonly IMapper _maper;

        public TestController(IMapper maper, DbcoffeeHouseContext context)
        {
            _context = context;
            _maper = maper;
        }

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

        [HttpPost("UploadCsv")]
        public async Task<IActionResult> UploadCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File không hợp lệ.");
            }

            var allowedExtensions = new[] { ".csv" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest("Chỉ chấp nhận file CSV.");
            }

            try
            {
                using var reader = new StreamReader(file.OpenReadStream());
                var csvData = new List<Dictionary<string, string>>();

                string? headerLine = await reader.ReadLineAsync();
                if (headerLine == null)
                {
                    return BadRequest("File CSV không có dữ liệu.");
                }

                var headers = headerLine.Split(',');

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (line != null)
                    {
                        var values = line.Split(',');

                        var rowDict = new Dictionary<string, string>();
                        for (int i = 0; i < headers.Length; i++)
                        {
                            rowDict[headers[i]] = values.ElementAtOrDefault(i) ?? "";
                        }

                        csvData.Add(rowDict);
                    }
                }

                return Ok(csvData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("GetPro")]
        public IActionResult GetPro()
        {
            var products = _context.Products.Include(x => x.Category).ToList();

            return Ok(_maper.Map<List<ProductResponseDTO>>(products));
        }
    }


}
