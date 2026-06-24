namespace PosBanHang.Application.DTOs;

public record ProductDto(Guid Id, Guid BranchId, Guid CategoryId, string Name, string Sku, string? Barcode, decimal Price, int Stock, int LowStockThreshold, string ImageUrl, bool IsActive);
public record UpsertProductRequest(Guid BranchId, Guid CategoryId, string Name, string Sku, string? Barcode, decimal Price, int Stock, int LowStockThreshold, string ImageUrl, bool IsActive);
