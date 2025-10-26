using Microsoft.AspNetCore.Mvc;
using ServerApp.Models;
using ServerApp.Services;
using ServerApp.Utilities;
using System.ComponentModel.DataAnnotations;

namespace ServerApp.Controllers;

/// <summary>
/// API Controller for managing products with proper JSON responses
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Get all products
    /// </summary>
    /// <returns>List of products in JSON format</returns>
    /// <response code="200">Returns the list of products</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Product>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
    {
        try
        {
            _logger.LogInformation("Fetching all products");
            
            var products = await _productService.GetAllProductsAsync();
            
            _logger.LogInformation("Successfully retrieved {Count} products", products.Count());
            
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching products");
            return StatusCode(500, new { error = "An error occurred while fetching products" });
        }
    }

    /// <summary>
    /// Get a specific product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product details in JSON format</returns>
    /// <response code="200">Returns the product</response>
    /// <response code="404">If the product is not found</response>
    /// <response code="400">If the ID is invalid</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Product))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Invalid product ID requested: {Id}", id);
            return BadRequest(new { error = "Product ID must be a positive number" });
        }

        try
        {
            _logger.LogInformation("Fetching product with ID: {Id}", id);
            
            var product = await _productService.GetProductByIdAsync(id);
            
            if (product == null)
            {
                _logger.LogWarning("Product not found with ID: {Id}", id);
                return NotFound(new { error = $"Product with ID {id} not found" });
            }

            _logger.LogInformation("Successfully retrieved product: {ProductName}", product.Name);
            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching product {Id}", id);
            return StatusCode(500, new { error = "An error occurred while fetching the product" });
        }
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="product">Product details</param>
    /// <returns>Created product in JSON format</returns>
    /// <response code="201">Returns the newly created product</response>
    /// <response code="400">If the product data is invalid</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Product))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid product data provided for creation");
            return BadRequest(ModelState);
        }

        try
        {
            _logger.LogInformation("Creating new product: {ProductName}", product.Name);
            
            var createdProduct = await _productService.CreateProductAsync(product);
            
            _logger.LogInformation("Successfully created product with ID: {Id}", createdProduct.Id);
            
            return CreatedAtAction(
                nameof(GetProduct), 
                new { id = createdProduct.Id }, 
                createdProduct
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating product");
            return StatusCode(500, new { error = "An error occurred while creating the product" });
        }
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="product">Updated product details</param>
    /// <returns>Updated product in JSON format</returns>
    /// <response code="200">Returns the updated product</response>
    /// <response code="404">If the product is not found</response>
    /// <response code="400">If the product data is invalid</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Product))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Product>> UpdateProduct(int id, [FromBody] Product product)
    {
        if (id <= 0)
        {
            return BadRequest(new { error = "Product ID must be a positive number" });
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid product data provided for update");
            return BadRequest(ModelState);
        }

        try
        {
            _logger.LogInformation("Updating product with ID: {Id}", id);
            
            var updatedProduct = await _productService.UpdateProductAsync(id, product);
            
            if (updatedProduct == null)
            {
                _logger.LogWarning("Product not found for update with ID: {Id}", id);
                return NotFound(new { error = $"Product with ID {id} not found" });
            }

            _logger.LogInformation("Successfully updated product: {ProductName}", updatedProduct.Name);
            return Ok(updatedProduct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating product {Id}", id);
            return StatusCode(500, new { error = "An error occurred while updating the product" });
        }
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Success confirmation</returns>
    /// <response code="204">Product successfully deleted</response>
    /// <response code="404">If the product is not found</response>
    /// <response code="400">If the ID is invalid</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { error = "Product ID must be a positive number" });
        }

        try
        {
            _logger.LogInformation("Deleting product with ID: {Id}", id);
            
            var success = await _productService.DeleteProductAsync(id);
            
            if (!success)
            {
                _logger.LogWarning("Product not found for deletion with ID: {Id}", id);
                return NotFound(new { error = $"Product with ID {id} not found" });
            }

            _logger.LogInformation("Successfully deleted product with ID: {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting product {Id}", id);
            return StatusCode(500, new { error = "An error occurred while deleting the product" });
        }
    }

    /// <summary>
    /// Get API health status and basic info - useful for frontend connection testing
    /// </summary>
    /// <returns>API health and connection info</returns>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetApiHealth()
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();
            var productCount = products.Count();
            
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                api = new
                {
                    version = "1.0.0",
                    productCount = productCount,
                    endpoints = new[]
                    {
                        "GET /api/products",
                        "GET /api/products/{id}",
                        "POST /api/products", 
                        "PUT /api/products/{id}",
                        "DELETE /api/products/{id}"
                    }
                },
                database = new
                {
                    connected = true,
                    productCount = productCount,
                    lastUpdated = products.Any() ? products.Max(p => p.CreatedAt) : DateTime.MinValue
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(500, new { 
                status = "unhealthy", 
                error = "Service unavailable",
                timestamp = DateTime.UtcNow 
            });
        }
    }

    /// <summary>
    /// Validate JSON structure and format for products endpoint (for development)
    /// </summary>
    /// <returns>JSON validation results and sample structure</returns>
    /// <response code="200">Returns JSON validation results</response>
    [HttpGet("validate-json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> ValidateJsonStructure()
    {
        try
        {
            _logger.LogInformation("Validating JSON structure for products endpoint");
            
            // Get sample products
            var products = await _productService.GetAllProductsAsync();
            var productList = products.ToList();
            
            // Use JsonValidator to perform actual validation
            var collectionValidation = JsonValidator.ValidateProductCollectionJson(productList);
            var sampleJson = JsonValidator.GenerateSampleProductJson();
            
            var validationResults = new
            {
                endpoint = "/api/products",
                timestamp = DateTime.UtcNow,
                validation = new
                {
                    totalProducts = productList.Count,
                    jsonStructureValid = collectionValidation.Equals(ValidationResult.Success),
                    serializationTest = collectionValidation.Equals(ValidationResult.Success) ? "passed" : "failed",
                    deserializationTest = collectionValidation.Equals(ValidationResult.Success) ? "passed" : "failed",
                    validationMessage = collectionValidation.ErrorMessage
                },
                sampleJsonStructure = productList.FirstOrDefault(),
                sampleJsonFormatted = sampleJson,
                expectedFields = new
                {
                    id = "integer - Unique product identifier",
                    name = "string - Product name (required)",
                    price = "decimal - Product price (required, positive)",
                    stock = "integer - Available stock quantity (non-negative)",
                    description = "string - Product description (optional)",
                    category = "string - Product category (optional)",
                    isAvailable = "boolean - Product availability status",
                    createdAt = "datetime - When the product was created"
                },
                jsonFormatNotes = new
                {
                    propertyNaming = "camelCase",
                    dateFormat = "ISO 8601 (yyyy-MM-ddTHH:mm:ss.fffZ)",
                    priceFormat = "decimal with 2 decimal places",
                    nullHandling = "null values are omitted from JSON output"
                }
            };
            
            _logger.LogInformation("JSON structure validation completed successfully");
            
            return Ok(validationResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during JSON validation");
            return StatusCode(500, new { error = "An error occurred during JSON validation" });
        }
    }

    /// <summary>
    /// Get product categories - useful for frontend filters/dropdowns
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<string[]>> GetProductCategories()
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();
            var categories = products
                .Where(p => !string.IsNullOrEmpty(p.Category))
                .Select(p => p.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToArray();
                
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching categories");
            return StatusCode(500, new { error = "Failed to fetch categories" });
        }
    }

    /// <summary>
    /// Get products summary stats - useful for frontend dashboards
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetProductStats()
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();
            var productList = products.ToList();
            
            return Ok(new
            {
                total = productList.Count,
                inStock = productList.Count(p => p.Stock > 0),
                lowStock = productList.Count(p => p.Stock > 0 && p.Stock <= 5),
                outOfStock = productList.Count(p => p.Stock == 0),
                categories = productList.Select(p => p.Category).Distinct().Count(),
                priceRange = productList.Any() ? new
                {
                    min = productList.Min(p => p.Price),
                    max = productList.Max(p => p.Price),
                    average = Math.Round(productList.Average(p => (double)p.Price), 2)
                } : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching product stats");
            return StatusCode(500, new { error = "Failed to fetch product statistics" });
        }
    }
}