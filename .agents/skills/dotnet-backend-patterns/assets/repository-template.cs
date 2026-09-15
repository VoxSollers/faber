// Repository Implementation Template for .NET 8+
// Dapper (perfomance)-only repository pattern for data access

using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;

namespace YourNamespace.Infrastructure.Data;

// Interface

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);
    Task<(IReadOnlyList<Product> Items, int TotalCount)> SearchAsync(ProductSearchRequest request, CancellationToken ct = default);
    Task<Product> CreateAsync(Product product, CancellationToken ct = default);
    Task<Product> UpdateAsync(Product product, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<string> ids, CancellationToken ct = default);
}

// Dapper Implementation

public class DapperProductRepository(
    IDbConnection connection,
    ILogger<DapperProductRepository> logger) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, Name, Sku, Price, CategoryId, Stock, CreatedAt, UpdatedAt
            FROM Products
            WHERE Id = @Id AND IsDeleted = 0
            """;

        return await connection.QueryFirstOrDefaultAsync<Product>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, Name, Sku, Price, CategoryId, Stock, CreatedAt, UpdatedAt
            FROM Products
            WHERE Sku = @Sku AND IsDeleted = 0
            """;

        return await connection.QueryFirstOrDefaultAsync<Product>(
            new CommandDefinition(sql, new { Sku = sku }, cancellationToken: ct));
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> SearchAsync(
        ProductSearchRequest request,
        CancellationToken ct = default)
    {
        var whereClauses = new List<string> { "IsDeleted = 0" };
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            whereClauses.Add("(Name LIKE @SearchTerm OR Sku LIKE @SearchTerm)");
            parameters.Add("SearchTerm", $"%{request.SearchTerm}%");
        }

        if (request.CategoryId.HasValue)
        {
            whereClauses.Add("CategoryId = @CategoryId");
            parameters.Add("CategoryId", request.CategoryId.Value);
        }

        if (request.MinPrice.HasValue)
        {
            whereClauses.Add("Price >= @MinPrice");
            parameters.Add("MinPrice", request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            whereClauses.Add("Price <= @MaxPrice");
            parameters.Add("MaxPrice", request.MaxPrice.Value);
        }

        var whereClause = string.Join(" AND ", whereClauses);
        var page = request.Page ?? 1;
        var pageSize = request.PageSize ?? 50;
        var offset = (page - 1) * pageSize;

        parameters.Add("Offset", offset);
        parameters.Add("PageSize", pageSize);

        // Use multi-query for count + data in single roundtrip
        var sql = $"""
            SELECT COUNT(*) FROM Products WHERE {whereClause};

            SELECT Id, Name, Sku, Price, CategoryId, Stock, CreatedAt, UpdatedAt
            FROM Products
            WHERE {whereClause}
            ORDER BY Name
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, parameters, cancellationToken: ct));

        var totalCount = await multi.ReadSingleAsync<int>();
        var items = (await multi.ReadAsync<Product>()).ToList();

        return (items, totalCount);
    }

    public async Task<Product> CreateAsync(Product product, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO Products (Id, Name, Sku, Price, CategoryId, Stock, CreatedAt, IsDeleted)
            VALUES (@Id, @Name, @Sku, @Price, @CategoryId, @Stock, @CreatedAt, 0);

            SELECT Id, Name, Sku, Price, CategoryId, Stock, CreatedAt, UpdatedAt
            FROM Products WHERE Id = @Id;
            """;

        return await connection.QuerySingleAsync<Product>(
            new CommandDefinition(sql, product, cancellationToken: ct));
    }

    public async Task<Product> UpdateAsync(Product product, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE Products
            SET Name = @Name,
                Sku = @Sku,
                Price = @Price,
                CategoryId = @CategoryId,
                Stock = @Stock,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id AND IsDeleted = 0;

            SELECT Id, Name, Sku, Price, CategoryId, Stock, CreatedAt, UpdatedAt
            FROM Products WHERE Id = @Id;
            """;

        return await connection.QuerySingleAsync<Product>(
            new CommandDefinition(sql, product, cancellationToken: ct));
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE Products
            SET IsDeleted = 1, UpdatedAt = @UpdatedAt
            WHERE Id = @Id
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id, UpdatedAt = DateTime.UtcNow }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(
        IEnumerable<string> ids,
        CancellationToken ct = default)
    {
        var idList = ids.ToList();
        if (idList.Count == 0)
        {
            return Array.Empty<Product>();
        }

        const string sql = """
            SELECT Id, Name, Sku, Price, CategoryId, Stock, CreatedAt, UpdatedAt
            FROM Products
            WHERE Id IN @Ids AND IsDeleted = 0
            """;

        var results = await connection.QueryAsync<Product>(
            new CommandDefinition(sql, new { Ids = idList }, cancellationToken: ct));

        return results.ToList();
    }
}

// Multi-mapping for related data

public class DapperOrderRepository(IDbConnection connection) : IOrderRepository
{
    public async Task<Order?> GetOrderWithItemsAsync(int orderId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT o.*, oi.*, p.*
            FROM Orders o
            LEFT JOIN OrderItems oi ON o.Id = oi.OrderId
            LEFT JOIN Products p ON oi.ProductId = p.Id
            WHERE o.Id = @OrderId
            """;

        var orderDictionary = new Dictionary<int, Order>();

        await connection.QueryAsync<Order, OrderItem, Product, Order>(
            new CommandDefinition(sql, new { OrderId = orderId }, cancellationToken: ct),
            (order, item, product) =>
            {
                if (!orderDictionary.TryGetValue(order.Id, out var existingOrder))
                {
                    existingOrder = order;
                    existingOrder.Items = new List<OrderItem>();
                    orderDictionary.Add(order.Id, existingOrder);
                }

                if (item != null)
                {
                    item.Product = product;
                    existingOrder.Items.Add(item);
                }

                return existingOrder;
            },
            splitOn: "Id,Id");

        return orderDictionary.Values.FirstOrDefault();
    }
}

// Entity Definitions

public class Product
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public int Stock { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Category? Category { get; set; }
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Order
{
    public int Id { get; set; }
    public string CustomerOrderCode { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public Order? Order { get; set; }
    public Product? Product { get; set; }
}