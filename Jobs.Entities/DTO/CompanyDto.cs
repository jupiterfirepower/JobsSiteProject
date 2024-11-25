using System.Text.Json.Serialization;

namespace Jobs.Entities.DTO;

public class CompanyDto
{
    [JsonPropertyName("companyId")]
    public int CompanyId { get; set; }
    [JsonPropertyName("companyName")]
    public string CompanyName { get; set; }
    [JsonPropertyName("companyDescription")]
    public string CompanyDescription { get; set; }
    [JsonPropertyName("companyLogoPath")]
    public string CompanyLogoPath { get; set; }
    [JsonPropertyName("companyLink")]
    public string CompanyLink { get; set; }
    [JsonPropertyName("isVisible")]
    public bool IsVisible { get; set; }
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}