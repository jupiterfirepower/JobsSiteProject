using System.Text.Json.Serialization;

namespace Jobs.DTO;

public record CategoryDto
{
    [JsonPropertyName("categoryId")]
    public int CategoryId { get; set; }
    
    [JsonPropertyName("categoryName")]
    public string CategoryName { get; set; }
    
    [JsonPropertyName("parentId")]
    public int? ParentId { get; set; }
    
    [JsonPropertyName("isVisible")]
    public bool IsVisible { get; set; } = true;
    
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;
    
    [JsonPropertyName("created")]
    public DateTime Created { get; init; }
    
    [JsonPropertyName("modified")]
    public DateTime Modified { get; init; } 
}