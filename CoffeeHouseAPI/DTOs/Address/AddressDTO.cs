using System.Text.Json.Serialization;

namespace CoffeeHouseAPI.DTOs.Address
{
    public class AddressDTO
    {
        public int? Id { get; set; }

        public int? CustomerId { get; set; }

        public string AddressNumber { get; set; } = null!;

        public bool IsDefault { get; set; }

        public int Ward { get; set; }

        public int District { get; set; }

        public int Province { get; set; }

        public string FullName { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;
    }

    public class Province
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("codename")]
        public string Codename { get; set; } = null!;

        [JsonPropertyName("divisiontype")]
        public string DivisionType { get; set; } = null!;

        [JsonPropertyName("phonecode")]
        public int PhoneCode { get; set; }

        [JsonPropertyName("districts")]
        public List<District> Districts { get; set; } = new List<District>();
    }

    public class District
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("codename")]
        public string Codename { get; set; } = null!;

        [JsonPropertyName("divisiontype")]
        public string DivisionType { get; set; } = null!;

        [JsonPropertyName("shortcodename")]
        public string ShortCodename { get; set; } = null!;

        [JsonPropertyName("wards")]
        public List<Ward> Wards { get; set; } = new List<Ward>();
    }

    public class Ward
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("codename")]
        public string Codename { get; set; } = null!;

        [JsonPropertyName("divisiontype")]
        public string DivisionType { get; set; } = null!;

        [JsonPropertyName("shortcodename")]
        public string ShortCodename { get; set; } = null!;
    }

}
