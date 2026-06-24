using PosBanHang.Domain.Enums;

namespace PosBanHang.Application.DTOs;

public record OrderItemRequest(Guid ProductId, int Quantity, string Note);
public record CreateOrderRequest(Guid BranchId, string TableName, Guid? CustomerId, string CashierName, IReadOnlyList<OrderItemRequest> Items, decimal Discount);
public record PayOrderRequest(PaymentMethod PaymentMethod);
public record OrderItemDto(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal, string Note);
public record OrderDto(Guid Id, string Code, Guid BranchId, string TableName, string CustomerName, string CashierName, OrderStatus Status, PaymentMethod? PaymentMethod, IReadOnlyList<OrderItemDto> Items, decimal Discount, decimal Total, DateTime CreatedAt, DateTime? PaidAt);
