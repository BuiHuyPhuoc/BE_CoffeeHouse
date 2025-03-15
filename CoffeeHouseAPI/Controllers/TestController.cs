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
using System.Linq;
using CoffeeHouseAPI.Services.Firebase;

namespace CoffeeHouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : TCHControllerBase
    {
        readonly DbcoffeeHouseContext _context;
        readonly IMapper _maper;
        readonly FirebaseService _firebaseService;

        public TestController(IMapper maper, DbcoffeeHouseContext context, FirebaseService firebaseService)
        {
            _context = context;
            _maper = maper;
            _firebaseService = firebaseService;
        }

        [HttpGet]
        [Route("Test")]
        public async Task<IActionResult> Test()
        {
            return Ok();
        }

        [HttpGet]
        [Route("TestNotify")]
        public async Task<IActionResult> TestNotify(string token)
        {
            await _firebaseService.SendNotificationAsync(token, "title", "testttt");
            return Ok();
        }

        [HttpGet]
        [Route("GetPort")]
        public IActionResult GetPort()
        {
            string port = this.GetUrlPort();
            return Ok(port);
        }

        //    [HttpPost]
        //    [Route("BinaryCsvToJson")]
        //    public IActionResult BinaryCsvToJson([FromBody] byte[] fileBytes)
        //    {
        //        if (fileBytes == null || fileBytes.Length == 0)
        //        {
        //            return BadRequest("Dữ liệu nhị phân không hợp lệ hoặc rỗng.");
        //        }

        //        string csvContent = Encoding.UTF8.GetString(fileBytes);

        //        using var reader = new StringReader(csvContent);
        //        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        //        {
        //            HasHeaderRecord = true,
        //            TrimOptions = TrimOptions.Trim,
        //            Delimiter = ",",
        //            BadDataFound = null
        //        });

        //        var records = csv.GetRecords<dynamic>().ToList();
        //        return Ok(records);
        //    }

        //    [HttpGet]
        //    [Route("GetCsvFile")]
        //    public IActionResult GetCsvFile()
        //    {
        //        string fileName = "BIS Seeburger EDI ITL integration EVN(SYM - SEEBURGER - EVN).csv";
        //        string path = Path.Combine(Directory.GetCurrentDirectory(), "File", fileName);

        //        if (!System.IO.File.Exists(path))
        //        {
        //            return BadRequest(new APIResponseBase
        //            {
        //                IsSuccess = false,
        //                Message = "File không tồn tại.",
        //                Status = (int)HttpStatusCode.NotFound
        //            });
        //        }

        //        byte[] fileBytes = System.IO.File.ReadAllBytes(path);
        //        string contentType = "application/octet-stream";

        //        return Ok(File(fileBytes, contentType, fileName));
        //    }

        //    [HttpGet]
        //    [Route("GetCsvJson")]
        //    public IActionResult GetCsvJson()
        //    {
        //        string fileName = "BIS Seeburger EDI ITL integration EVN(SYM - SEEBURGER - EVN).csv";
        //        string path = Path.Combine("/app/files", fileName);
        //        Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");
        //        if (!System.IO.File.Exists(path))
        //        {
        //            return BadRequest(new APIResponseBase
        //            {
        //                IsSuccess = false,
        //                Message = "File không tồn tại.",
        //                Status = (int)HttpStatusCode.NotFound
        //            });
        //        }

        //        using var reader = new StreamReader(path);
        //        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        //        {
        //            HasHeaderRecord = true,
        //            TrimOptions = TrimOptions.Trim,
        //            Delimiter = ",",
        //            BadDataFound = null
        //        });

        //        var records = csv.GetRecords<dynamic>().ToList();

        //        return Ok(records);
        //    }

        //}

        //public class Base64String
        //{
        //    public string base64 { get; set; } = null!;
        //}

    }
}
