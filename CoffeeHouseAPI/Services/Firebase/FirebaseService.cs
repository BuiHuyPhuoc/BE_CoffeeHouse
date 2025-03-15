using CoffeeHouseAPI.DTOs.Image;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Newtonsoft.Json;
using System.Text;

namespace CoffeeHouseAPI.Services.Firebase
{
    public class FirebaseService
    {
        private readonly StorageClient _storageClient;
        private readonly HttpClient _httpClient;
        private readonly string _imageBucket = "shopoes-2e0b8.appspot.com";
        private readonly string _imageFolder = "coffee_house";
        private readonly string _firebaseServerKey;

        public FirebaseService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            var firebaseConfig = configuration.GetSection("FirebaseConfig").Get<FirebaseConfig>() ?? throw new Exception("Missing Firebase configuration");
            _firebaseServerKey = firebaseConfig.AuthSecret;
            _httpClient = httpClientFactory.CreateClient();

            var jsonCredentials = new
            {
                type = firebaseConfig.Type,
                project_id = firebaseConfig.ProjectId,
                private_key_id = firebaseConfig.PrivateKeyId,
                private_key = firebaseConfig.PrivateKey.Replace("\\n", "\n"),
                client_email = firebaseConfig.ClientEmail,
                client_id = firebaseConfig.ClientId,
                auth_uri = firebaseConfig.AuthUri,
                token_uri = firebaseConfig.TokenUri,
                auth_provider_x509_cert_url = firebaseConfig.AuthProviderX509CertUrl,
                client_x509_cert_url = firebaseConfig.ClientX509CertUrl
            };

            var jsonString = JsonConvert.SerializeObject(jsonCredentials);
            var credential = GoogleCredential.FromJson(jsonString);
            _storageClient = StorageClient.Create(credential);
        }

        public async Task<string> UploadImageAsync(ImageRequestDTO request)
        {
            byte[] imageBytes = Convert.FromBase64String(request.Content);
            string fileTypeLower = request.ImageType.ToLower();
            string fileName = $"{_imageFolder}/{Guid.NewGuid()}.{fileTypeLower}";

            using (MemoryStream stream = new MemoryStream(imageBytes))
            {
                await _storageClient.UploadObjectAsync(_imageBucket, fileName, $"image/{fileTypeLower}", stream);
            }

            return $"https://firebasestorage.googleapis.com/v0/b/{_imageBucket}/o/{Uri.EscapeDataString(fileName)}?alt=media";
        }

        public async Task<bool> SendNotificationAsync(string deviceToken, string title, string body)
        {
            var message = new
            {
                to = deviceToken,
                notification = new { title, body, sound = "default" },
                data = new { click_action = "FLUTTER_NOTIFICATION_CLICK" }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://fcm.googleapis.com/fcm/send")
            {
                Headers = { { "Authorization", _firebaseServerKey } },
                Content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
    }

    public class FirebaseConfig
    {
        public string Type { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string PrivateKeyId { get; set; } = string.Empty;
        public string PrivateKey { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string AuthUri { get; set; } = string.Empty;
        public string TokenUri { get; set; } = string.Empty;
        public string AuthProviderX509CertUrl { get; set; } = string.Empty;
        public string ClientX509CertUrl { get; set; } = string.Empty;
        public string DatabaseUrl { get; set; } = string.Empty;
        public string AuthSecret { get; set; } = string.Empty;
    }
}
