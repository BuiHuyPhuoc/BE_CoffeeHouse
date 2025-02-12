using CoffeeHouseAPI.DTOs.AI;
using CoffeeHouseAPI.DTOs.APIPayload;
using CoffeeHouseAPI.Enums;
using CoffeeHouseAPI.Helper;
using CoffeeHouseLib.Models;
using Google.Api.Gax.ResourceNames;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static Google.Apis.Requests.BatchRequest;

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
            string? aiKey = builder.Configuration["AIConfig:GerminiKey"];

            if (aiKey == null) throw new Exception("Wrong when call message");

            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={aiKey}";

            var menuAI = GetMenuForAI();

            string AIPromt = "Với vai trò là một nữ Barista của một quán cafe - thức uống. Hãy giúp tôi đưa ra những thức uống mà khách hàng yêu cầu dựa vào dữ liệu mà tôi cung cấp cho bạn ở dưới đây: \n" +
                $"{System.Text.Json.JsonSerializer.Serialize(menuAI)} \n" +
                "Dựa vào yêu cầu của khách hàng, phân tích ra mùi vị, thành phần, công dụng để có thể đưa ra nhiều nhất 3 lựa chọn với tên món hợp lý nhé. Ví dụ như khách hàng yêu cầu \"Đồ uống ngọt, có vị sữa, béo\" thì có thể cân nhắc món Trà sữa. \n" +
                "Phản hồi của bạn luôn luôn có dạng chuỗi JSON như sau: [{ \"productId\": 22, \"productName\": \"Trà sữa truyền thống\", \"message\": \"\", \"isRelated\": true }] \n" +
                "Với productId và productName là id và tên của sản phẩm được cung cấp trong dữ liệu mẫu. 'message' là tin nhắn phản hồi từ bạn, message này không bao gồm chuỗi json đã được cung cấp từ trước. Chỉ có chứa tin nhắn phản hồi thông thường của bạn. 'isRelated' dùng để xác định câu trả lời của bạn từ 'message' có thuộc về lĩnh vực của bạn hay không. Lĩnh vực của bạn là một người Barista đang tư vấn cho khách hàng của mình về món đồ uống mà bạn cho rằng nó sẽ phù hợp yêu cầu của khách hàng. Nếu phù hợp thì trả về là true, còn không thì là false. Những thông tin như sản phẩm khuyến mãi, sản phẩm sắp ra mắt, ... và những thông tin không nằm trong dữ liệu được cung cấp, hãy trả lời từ chối một cách lịch sự với khách hàng. Cách xưng hô, hãy luôn xưng hô là quý khách(khách hàng) - em(bạn). Hãy luôn trả về một chuỗi json như ví dụ mẫu, kể cả message có là gì đi nữa.";

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
                systemInstruction = new { 
                    role = "user",
                    parts = new[] {
                        new {
                            text = AIPromt
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
            httpClient.DefaultRequestHeaders.Add("x-goog-api-key", aiKey);

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

                // Loại bỏ ```json và ```
                string jsonText = Regex.Replace(text, @"```json|```", "").Trim();
                jsonText = Regex.Unescape(jsonText);
                jsonText = jsonText.Replace("\n", "").Replace("\t", "").Replace("\r", "").Replace("\\", "");

                // Deserialize thành object
                var productInfo = JsonConvert.DeserializeObject<List<ProductRecommendation>>(jsonText);
                // var productInfo = JsonSerializer.Deserialize<List<ProductRecommendation>>(jsonText);

                if (productInfo != null)
                {
                    return Ok(new APIResponseBase
                    {
                        IsSuccess = true,
                        Message = GENERATE_DATA.API_ACTION_RESPONSE(true, API_ACTION.GET),
                        Status = (int)HttpStatusCode.OK,
                        Value = productInfo
                    });
                } else
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
