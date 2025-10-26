using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ServerApp.Models;

/// <summary>
/// Product model with proper JSON serialization and validation attributes
/// </summary>
public class Product
{
    /// <summary>
    /// Unique identifier for the product
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// Product name - required field
    /// </summary>
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Product name must be between 1 and 100 characters")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Product price - must be positive
    /// </summary>
    [Range(0.01, 999999.99, ErrorMessage = "Price must be between $0.01 and $999,999.99")]
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    /// <summary>
    /// Stock quantity - must be non-negative
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Stock must be a non-negative number")]
    [JsonPropertyName("stock")]
    public int Stock { get; set; }

    /// <summary>
    /// Optional product description
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Product category
    /// </summary>
    [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    /// <summary>
    /// Whether the product is currently available
    /// </summary>
    [JsonPropertyName("isAvailable")]
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// When the product was created
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}