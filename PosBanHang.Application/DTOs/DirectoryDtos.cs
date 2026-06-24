using PosBanHang.Domain.Enums;

namespace PosBanHang.Application.DTOs;

public record BranchDto(Guid Id, string Name, string Address, string Phone, bool IsActive);
public record CategoryDto(Guid Id, string Name, string Description, bool IsActive);
public record CustomerDto(Guid Id, string FullName, string Phone, string Email, int LoyaltyPoint, string Rank, bool IsActive);
public record StaffDto(Guid Id, Guid BranchId, string FullName, string Email, string Phone, string Role, string Position, StaffStatus Status, decimal Salary, string AvatarUrl);
public record UpsertCustomerRequest(string FullName, string Phone, string Email, int LoyaltyPoint, string Rank, bool IsActive);
public record UpsertStaffRequest(Guid BranchId, string FullName, string Email, string Phone, string Role, string Position, StaffStatus Status, decimal Salary, string AvatarUrl);
public record DashboardDto(decimal RevenueToday, int OrdersToday, int ActiveOrders, int LowStockProducts, IReadOnlyList<ProductDto> TopProducts, IReadOnlyList<OrderDto> RecentOrders);
public record AiInsightDto(string Title, string Message, string Severity);
