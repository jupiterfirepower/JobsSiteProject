using System.Text.Json.Serialization;

namespace Jobs.Entities.DTO;

public class EmploymentTypeDto
{
    [JsonPropertyName("employmentTypeId")]
    public int EmploymentTypeId { get; init; }
    
    [JsonPropertyName("employmentTypeName")]
    public string EmploymentTypeName { get; init; }
    
    [JsonPropertyName("created")]
    public DateTime Created { get; init; }
    
    [JsonPropertyName("modified")]
    public DateTime Modified { get; init; }
}