// Service Implementation Template for .NET 8+
// This template demonstrates best practices for building robust services
// Uses ErrorOr for result handling, Dapper repositories, NSubstitute for testing

using ErrorOr;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace YourNamespace.Application.Services;

/// <summary>
/// Configuration options for the service
/// </summary>
public class ProductServiceOptions
{
    public const string SectionName = "ProductService";

    public int DefaultPageSize { get; set; } = 50;
    public int MaxPageSize { get; set; } = 200;
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(15);
    public bool EnableEnrichment { get; set; } = true;
}

/// <summary>
/// Service interface - define the contract
/// </summary>
public interface IProductService
{
    Task<ErrorOr<Product>> GetByIdAsync(string id, CancellationToken ct = default);
    Task<ErrorOr<PagedResult<Product>>> SearchAsync(ProductSearchRequest request, CancellationToken ct = default);
    Task<ErrorOr<Product>> CreateAsync(CreateProductRequest request, CancellationToken ct = default);
    Task<ErrorOr<Product>> UpdateAsync(string id, UpdateProductRequest request, CancellationToken ct = default);
    Task<ErrorOr<Deleted>> DeleteAsync(string id, CancellationToken ct = default);
}

/// <summary>
/// Service implementation with full patterns
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly ICacheService _cache;
    private readonly IValidator<CreateProductRequest> _createValidator;
    private readonly IValidator<UpdateProductRequest> _updateValidator;
    private readonly ILogger<ProductService> _logger;
    private readonly ProductServiceOptions _options;

    public ProductService(
        IProductRepository repository,
        ICacheService cache,
        IValidator<CreateProductRequest> createValidator,
        IValidator<UpdateProductRequest> updateValidator,
        ILogger<ProductService> logger,
        IOptions<ProductServiceOptions> options)
    {
        _repository = repository;
        _cache = cache;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<ErrorOr<Product>> GetByIdAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return Error.Validation("Product.InvalidId", "Product ID is required");
        }

        // Try cache first
        var cacheKey = GetCacheKey(id);
        var cached = await _cache.GetAsync<Product>(cacheKey, ct);

        if (cached != null)
        {
            _logger.LogDebug("Cache hit for product {ProductId}", id);
            return cached;
        }

        // Fetch from repository
        var product = await _repository.GetByIdAsync(id, ct);

        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", id);
            return Error.NotFound("Product.NotFound", $"Product '{id}' not found");
        }

        // Populate cache
        await _cache.SetAsync(cacheKey, product, _options.CacheDuration, ct);

        return product;
    }

    public async Task<ErrorOr<PagedResult<Product>>> SearchAsync(
        ProductSearchRequest request,
        CancellationToken ct = default)
    {
        // Sanitize pagination
        var pageSize = Math.Clamp(request.PageSize ?? _options.DefaultPageSize, 1, _options.MaxPageSize);
        var page = Math.Max(request.Page ?? 1, 1);

        var sanitizedRequest = request with
        {
            PageSize = pageSize,
            Page = page
        };

        // Execute search
        var (items, totalCount) = await _repository.SearchAsync(sanitizedRequest, ct);

        var result = new PagedResult<Product>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return result;
    }

    public async Task<ErrorOr<Product>> CreateAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        // Validate
        var validation = await _createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return validation.Errors
                .Select(e => Error.Validation(e.PropertyName, e.ErrorMessage))
                .ToList();
        }

        // Check for duplicates
        var existing = await _repository.GetBySkuAsync(request.Sku, ct);
        if (existing != null)
        {
            return Error.Conflict("Product.DuplicateSku", $"Product with SKU '{request.Sku}' already exists");
        }

        // Create entity
        var product = new Product
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = request.Name,
            Sku = request.Sku,
            Price = request.Price,
            CategoryId = request.CategoryId,
            CreatedAt = DateTime.UtcNow
        };

        // Persist
        var created = await _repository.CreateAsync(product, ct);

        _logger.LogInformation("Created product {ProductId} with SKU {Sku}", created.Id, created.Sku);

        return created;
    }

    public async Task<ErrorOr<Product>> UpdateAsync(
        string id,
        UpdateProductRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return Error.Validation("Product.InvalidId", "Product ID is required");
        }

        // Validate
        var validation = await _updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return validation.Errors
                .Select(e => Error.Validation(e.PropertyName, e.ErrorMessage))
                .ToList();
        }

        // Fetch existing
        var existing = await _repository.GetByIdAsync(id, ct);
        if (existing == null)
        {
            return Error.NotFound("Product.NotFound", $"Product '{id}' not found");
        }

        // Apply updates (only non-null values)
        if (request.Name != null) existing.Name = request.Name;
        if (request.Price.HasValue) existing.Price = request.Price.Value;
        if (request.CategoryId.HasValue) existing.CategoryId = request.CategoryId.Value;
        existing.UpdatedAt = DateTime.UtcNow;

        // Persist
        var updated = await _repository.UpdateAsync(existing, ct);

        // Invalidate cache
        await _cache.RemoveAsync(GetCacheKey(id), ct);

        _logger.LogInformation("Updated product {ProductId}", id);

        return updated;
    }

    public async Task<ErrorOr<Deleted>> DeleteAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return Error.Validation("Product.InvalidId", "Product ID is required");
        }

        var existing = await _repository.GetByIdAsync(id, ct);
        if (existing == null)
        {
            return Error.NotFound("Product.NotFound", $"Product '{id}' not found");
        }

        // Soft delete
        await _repository.DeleteAsync(id, ct);

        // Invalidate cache
        await _cache.RemoveAsync(GetCacheKey(id), ct);

        _logger.LogInformation("Deleted product {ProductId}", id);

        return Result.Deleted;
    }

    private static string GetCacheKey(string id) => $"product:{id}";
}

// Supporting types
public record CreateProductRequest(string Name, string Sku, decimal Price, int CategoryId);
public record UpdateProductRequest(string? Name = null, decimal? Price = null, int? CategoryId = null);
public record ProductSearchRequest(
    string? SearchTerm = null,
    int? CategoryId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    int? Page = null,
    int? PageSize = null);

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

public class Product
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// Validators using FluentValidation
public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU is required")
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters")
            .Matches(@"^[A-Z0-9\-]+$").WithMessage("SKU must contain only uppercase letters, numbers, and hyphens");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category is required");
    }
}