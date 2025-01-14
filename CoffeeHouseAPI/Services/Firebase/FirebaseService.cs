using Azure.Core;
using CoffeeHouseAPI.DTOs.Image;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace CoffeeHouseAPI.Services.Firebase
{
    public class FirebaseService
    {
        private readonly StorageClient _storageClient;
        private string _imageBucket = "shopoes-2e0b8.appspot.com";
        private string _imageFolder = "coffee_house";
        public FirebaseService(IConfiguration configuration)
        {
            var firebaseConfig = configuration.GetSection("FirebaseConfig").Get<FirebaseConfig>() ?? throw new Exception();

            var jsonCredentials = new {
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

            var jsonString = System.Text.Json.JsonSerializer.Serialize(jsonCredentials);
            var credential = GoogleCredential.FromJson(jsonString);

            _storageClient = StorageClient.Create(credential);
        }

        public async Task<string> UploadImageAsync(ImageRequestDTO request)
        {
            byte[] imageBytes = Convert.FromBase64String(request.Content);
            string fileTypeLower = request.ImageType.ToLower();

            string fileName = $"{_imageFolder}/{Guid.NewGuid()}.{fileTypeLower}";

            using (MemoryStream stream = new MemoryStream(imageBytes)) {
                await _storageClient.UploadObjectAsync(_imageBucket, fileName, $"image/{fileTypeLower}", stream);
            }

            string downloadUrl = $"https://firebasestorage.googleapis.com/v0/b/{_imageBucket}/o/{Uri.EscapeDataString(fileName)}?alt=media";
            return downloadUrl;
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
    }
}
