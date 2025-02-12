using CoffeeHouseAPI.DTOs.AI;
using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseAPI.Enums;
using CoffeeHouseAPI.Helper;
using CoffeeHouseLib.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CoffeeHouseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIController : TCHControllerBase
    {
        readonly DbcoffeeHouseContext _context;
        public AIController(DbcoffeeHouseContext context)
        {
            _context = context;
        }


        [HttpGet]
        [Route("AIMenu")]
        public IActionResult AIMenu()
        {
            return Ok(GetMenuForAI());
        }

        private List<AIProduct> GetMenuForAI()
        {
            var products = _context.Products
                .Where(x => x.IsValid && x.Description2 != null && x.Material != null)
                .ToList();

            List<AIProduct> aIProducts = products.Select(product => new AIProduct
            {
                ProductId = product.Id,
                ProductName = product.ProductName,
                Description = Regex.Replace(product.Description2!.Trim(), @"\s+", " "),
                Material = Regex.Replace(product.Material!.Trim(), @"\s+", " ")
            }).ToList();

            return aIProducts;
        }

        [HttpPost]
        [Route("RecommendAI")]
        public async Task<IActionResult> RecommendAI([FromBody] MessageToAI request)
        {
            var builder = WebApplication.CreateBuilder();
            Aiconfig? aiKey = _context.Aiconfigs.FirstOrDefault();

            if (aiKey == null) throw new Exception("Wrong when call message");

            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={aiKey.Key}";

            var menuAI = GetMenuForAI();

            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };

            string stringMenu = System.Text.Json.JsonSerializer.Serialize(menuAI, options);

            string promt = aiKey.Promt.Replace("{0}", stringMenu);

            var payload = new
            {
                contents = new[] {
                    new {
                        role = "user",
                        parts = new[] {
                            new {
                                text = request.Message
                            }
                        },
                    }
                },
                systemInstruction = new
                {
                    role = "user",
                    parts = new[] {
                        new {
                            text = promt,
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 1,
                    topK = 40,
                    topP = 0.95,
                    maxOutputTokens = 8192,
                    responseMimeType = "text/plain"
                }
            };

            string jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("x-goog-api-key", aiKey.Key);

            HttpResponseMessage response = await httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API Germini call failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            }

            string result = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(result);

            if (doc.RootElement.TryGetProperty("candidates", out JsonElement candidates) &&
                candidates.GetArrayLength() > 0 &&
                candidates[0].TryGetProperty("content", out JsonElement contentRes) &&
                contentRes.TryGetProperty("parts", out JsonElement parts) &&
                parts.GetArrayLength() > 0 &&
                parts[0].TryGetProperty("text", out JsonElement textElement))
            {
                string text = textElement.GetString() ?? "";

                
                string jsonText = Regex.Replace(text, @"```json|```", "").Trim();
                jsonText = Regex.Unescape(jsonText);
                jsonText = jsonText.Replace("\n", "").Replace("\t", "").Replace("\r", "").Replace("\\", "");

                var productInfo = JsonConvert.DeserializeObject<List<ProductRecommendation>>(jsonText);

                if (productInfo != null)
                {
                    return Ok(new APIResponseBase
                    {
                        IsSuccess = true,
                        Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.GET),
                        Status = (int)HttpStatusCode.OK,
                        Value = productInfo
                    });
                }
                else
                {
                    return BadRequest(new APIResponseBase
                    {
                        IsSuccess = false,
                        Message = "Lỗi.",
                        Status = (int)HttpStatusCode.BadRequest,
                    });
                }
            }
            else
            {
                return BadRequest(new APIResponseBase
                {
                    IsSuccess = false,
                    Message = "Lỗi.",
                    Status = (int)HttpStatusCode.BadRequest,
                });
            }
        }
    }

    public class ProductRecommendation
    {
        public int? ProductId { get; set; }
        public string? ProductName { get; set; } = null!;
        public string Message { get; set; } = null!;
        public bool IsRelated { get; set; }
    }
}
