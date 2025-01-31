using DemoApi.Models;

namespace DemoApi.Services;

public class ProductService : IProductService
{
    private readonly List<Product> _products = new();
    private int _nextId = 1;

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await Task.FromResult(_products);
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await Task.FromResult(_products.FirstOrDefault(p => p.Id == id));
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        product.Id = _nextId++;
        _products.Add(product);
        return await Task.FromResult(product);
    }

    public async Task<Product?> UpdateProductAsync(int id, Product product)
    {
        var existingProduct = _products.FirstOrDefault(p => p.Id == id);
        if (existingProduct == null) return null;

        existingProduct.Name = product.Name;
        existingProduct.Description = product.Description;
        existingProduct.Price = product.Price;
        existingProduct.StockQuantity = product.StockQuantity;

        return await Task.FromResult(existingProduct);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null) return await Task.FromResult(false);

        _products.Remove(product);
        return await Task.FromResult(true);
    }
} 