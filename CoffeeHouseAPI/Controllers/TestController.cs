using AutoMapper;
using CoffeeHouseAPI.DTOs;
using CoffeeHouseAPI.DTOs.Product;
using CoffeeHouseLib.Models;
using CsvHelper.Configuration;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Abstractions;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Buffers.Text;
using Azure.Core;
using CoffeeHouseAPI.DTOs.APIPayload;
using System.Net;
using Google.Apis.Storage.v1.Data;
using System.IO;

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

        [HttpPost]
        [Route(nameof(CsvBase64ToJson))]
        public IActionResult CsvBase64ToJson([FromBody] Base64String base64)
        {
            if (string.IsNullOrWhiteSpace(base64.base64))
            {
                return BadRequest("Dữ liệu Base64 không hợp lệ.");
            }

            
            byte[] csvBytes;

            try
            {
                csvBytes = Convert.FromBase64String(base64.base64);
            }
            catch
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Đây không phải chuỗi base64",
                    Status = (int)HttpStatusCode.BadRequest
                });
            }

            
            string csvContent = Encoding.UTF8.GetString(csvBytes);

            using var reader = new StringReader(csvContent);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                Delimiter = ",",
                BadDataFound = null
            });

            var records = csv.GetRecords<dynamic>().ToList();

            return Ok(records);
        }

        [HttpPost]
        [Route(nameof(CsvToJson))]
        public IActionResult CsvToJson(IFormFile file)
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

            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                Delimiter = ",",
                BadDataFound = null
            });

            var records = csv.GetRecords<dynamic>().ToList();

            return Ok(records);
        }

        [HttpGet]
        [Route("GetPro")]
        public IActionResult GetPro()
        {
            var products = _context.Products.Include(x => x.Category).ToList();

            return Ok(_maper.Map<List<ProductResponseDTO>>(products));
        }

        [HttpPost]
        [Route(nameof(JsonToCsv))]
        public IActionResult JsonToCsv([FromBody] List<Dictionary<string, string>> jsonData)
        {
            if (jsonData == null || jsonData.Count == 0)
            {
                return BadRequest("Dữ liệu JSON không hợp lệ.");
            }

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",", // Định dạng CSV với dấu phẩy, có thể đổi thành ";" nếu cần
                Encoding = Encoding.UTF8
            };

            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, new UTF8Encoding(false));
            using var csv = new CsvWriter(writer, csvConfig);

            // Lấy danh sách header từ dictionary
            var headers = new List<string>(jsonData[0].Keys);
            foreach (var header in headers)
            {
                csv.WriteField(header);
            }
            csv.NextRecord();

            // Ghi dữ liệu vào CSV
            foreach (var row in jsonData)
            {
                foreach (var header in headers)
                {
                    csv.WriteField(row.TryGetValue(header, out var value) ? value : "");
                }
                csv.NextRecord();
            }

            writer.Flush();
            var fileBytes = memoryStream.ToArray();
            return Ok(File(fileBytes, "text/csv", "data.csv"));
        }

        [HttpGet]
        [Route("DownloadCsv")]
        public IActionResult DownloadCsv()
        {
            var csvContent = "Id,Name,Age\n1,John,30\n2,Jane,28"; // Nội dung CSV
            var bytes = Encoding.UTF8.GetBytes(csvContent);
            var fileName = "data.csv";

            return Ok(File(bytes, "text/csv", fileName));
        }

        [HttpPost]
        [Route("BinaryCsvToJson")]
        public IActionResult BinaryCsvToJson([FromBody] byte[] fileBytes)
        {
            if (fileBytes == null || fileBytes.Length == 0)
            {
                return BadRequest("Dữ liệu nhị phân không hợp lệ hoặc rỗng.");
            }

            string csvContent = Encoding.UTF8.GetString(fileBytes);

            using var reader = new StringReader(csvContent);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                Delimiter = ",",
                BadDataFound = null
            });

            var records = csv.GetRecords<dynamic>().ToList();
            return Ok(records);
        }

        [HttpGet]
        [Route("GetCsvFile")]
        public IActionResult GetCsvFile()
        {
            string fileName = "BIS Seeburger EDI ITL integration EVN(SYM - SEEBURGER - EVN).csv";
            string path = Path.Combine(Directory.GetCurrentDirectory(), "File", fileName);
            
            if (!System.IO.File.Exists(path))
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message= "File không tồn tại.",
                    Status = (int)HttpStatusCode.NotFound
                });
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            string contentType = "application/octet-stream";

            return Ok(File(fileBytes, contentType, fileName));
        }
    }

    public class Base64String
    {
        public string base64 { get; set; } = null!;
    }


}
