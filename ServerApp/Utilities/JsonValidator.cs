using ServerApp.Models;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace ServerApp.Utilities;

/// <summary>
/// Utility class for validating JSON structure and data consistency
/// </summary>
public static class JsonValidator
{
    /// <summary>
    /// Validates that a Product object serializes to proper JSON format
    /// </summary>
    public static ValidationResult ValidateProductJson(Product product)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(product);
        
        // Validate the object using data annotations
        bool isValid = Validator.TryValidateObject(product, validationContext, validationResults, true);
        
        if (!isValid)
        {
            return new ValidationResult($"Product validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}");
        }
        
        try
        {
            // Test JSON serialization
            var json = JsonSerializer.Serialize(product, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });
            
            // Test JSON deserialization
            var deserializedProduct = JsonSerializer.Deserialize<Product>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            if (deserializedProduct == null)
            {
                return new ValidationResult("JSON deserialization returned null");
            }
            
            // Validate that key properties match after round-trip
            if (deserializedProduct.Id != product.Id ||
                deserializedProduct.Name != product.Name ||
                deserializedProduct.Price != product.Price ||
                deserializedProduct.Stock != product.Stock)
            {
                return new ValidationResult("JSON round-trip validation failed - data mismatch");
            }
            
            return ValidationResult.Success ?? new ValidationResult("Success");
        }
        catch (JsonException ex)
        {
            return new ValidationResult($"JSON serialization/deserialization failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            return new ValidationResult($"Unexpected error during JSON validation: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Generates sample JSON for documentation purposes
    /// </summary>
    public static string GenerateSampleProductJson()
    {
        var sampleProduct = new Product
        {
            Id = 1,
            Name = "Sample Product",
            Price = 99.99m,
            Stock = 10,
            Description = "This is a sample product for testing JSON structure",
            Category = "Electronics",
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };
        
        return JsonSerializer.Serialize(sampleProduct, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
    }
    
    /// <summary>
    /// Validates that a collection of products serializes correctly
    /// </summary>
    public static ValidationResult ValidateProductCollectionJson(IEnumerable<Product> products)
    {
        try
        {
            // Test collection serialization
            var json = JsonSerializer.Serialize(products, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });
            
            // Test collection deserialization
            var deserializedProducts = JsonSerializer.Deserialize<Product[]>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            if (deserializedProducts == null)
            {
                return new ValidationResult("JSON collection deserialization returned null");
            }
            
            if (deserializedProducts.Length != products.Count())
            {
                return new ValidationResult("JSON collection round-trip validation failed - count mismatch");
            }
            
            return ValidationResult.Success ?? new ValidationResult("Success");
        }
        catch (JsonException ex)
        {
            return new ValidationResult($"JSON collection serialization/deserialization failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            return new ValidationResult($"Unexpected error during JSON collection validation: {ex.Message}");
        }
    }
}