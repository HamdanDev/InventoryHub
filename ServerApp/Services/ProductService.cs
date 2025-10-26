using ServerApp.Models;

namespace ServerApp.Services;

/// <summary>
/// Interface for product data operations
/// </summary>
public interface IProductService
{
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<Product?> GetProductByIdAsync(int id);
    Task<Product> CreateProductAsync(Product product);
    Task<Product?> UpdateProductAsync(int id, Product product);
    Task<bool> DeleteProductAsync(int id);
}

/// <summary>
/// In-memory implementation of product service for demo purposes
/// In a real application, this would connect to a database
/// </summary>
public class ProductService : IProductService
{
    private readonly List<Product> _products;

    public ProductService()
    {
        // Initialize with sample data
        _products = new List<Product>
        {
            new Product
            {
                Id = 1,
                Name = "Gaming Laptop",
                Price = 1299.99m,
                Stock = 15,
                Description = "High-performance gaming laptop with RTX 4070 GPU",
                Category = "Electronics",
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Product
            {
                Id = 2,
                Name = "Wireless Headphones",
                Price = 199.99m,
                Stock = 50,
                Description = "Premium wireless headphones with noise cancellation",
                Category = "Audio",
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new Product
            {
                Id = 3,
                Name = "Mechanical Keyboard",
                Price = 149.99m,
                Stock = 25,
                Description = "RGB mechanical keyboard with Cherry MX switches",
                Category = "Accessories",
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new Product
            {
                Id = 4,
                Name = "4K Monitor",
                Price = 399.99m,
                Stock = 8,
                Description = "27-inch 4K IPS monitor with USB-C connectivity",
                Category = "Monitors",
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Product
            {
                Id = 5,
                Name = "Smartphone",
                Price = 799.99m,
                Stock = 0,
                Description = "Latest flagship smartphone with 5G connectivity",
                Category = "Mobile",
                IsAvailable = false,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            }
        };
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        // Simulate async operation
        await Task.Delay(10);
        return _products.OrderBy(p => p.Name);
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        await Task.Delay(10);
        return _products.FirstOrDefault(p => p.Id == id);
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        await Task.Delay(10);
        
        // Generate new ID
        product.Id = _products.Max(p => p.Id) + 1;
        product.CreatedAt = DateTime.UtcNow;
        
        _products.Add(product);
        return product;
    }

    public async Task<Product?> UpdateProductAsync(int id, Product product)
    {
        await Task.Delay(10);
        
        var existingProduct = _products.FirstOrDefault(p => p.Id == id);
        if (existingProduct == null)
            return null;

        // Update properties
        existingProduct.Name = product.Name;
        existingProduct.Price = product.Price;
        existingProduct.Stock = product.Stock;
        existingProduct.Description = product.Description;
        existingProduct.Category = product.Category;
        existingProduct.IsAvailable = product.IsAvailable;

        return existingProduct;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        await Task.Delay(10);
        
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            return false;

        _products.Remove(product);
        return true;
    }
}
