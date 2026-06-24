using PosBanHang.Application.Common;
using PosBanHang.Application.DTOs;

namespace PosBanHang.Application.Interfaces;

public interface IPosService
{
    DashboardDto GetDashboard();
    IReadOnlyList<BranchDto> GetBranches();
    IReadOnlyList<CategoryDto> GetCategories();
    IReadOnlyList<CustomerDto> GetCustomers(string? search);
    IReadOnlyList<StaffDto> GetStaff(string? search);
    CustomerDto CreateCustomer(UpsertCustomerRequest request);
    CustomerDto UpdateCustomer(Guid id, UpsertCustomerRequest request);
    void DeleteCustomer(Guid id);
    StaffDto CreateStaff(UpsertStaffRequest request);
    StaffDto UpdateStaff(Guid id, UpsertStaffRequest request);
    void DeleteStaff(Guid id);
    PagedResult<ProductDto> GetProducts(string? search, Guid? categoryId, int page, int pageSize);
    ProductDto CreateProduct(UpsertProductRequest request);
    ProductDto UpdateProduct(Guid id, UpsertProductRequest request);
    void DeleteProduct(Guid id);
    IReadOnlyList<OrderDto> GetOrders(string? status);
    OrderDto CreateOrder(CreateOrderRequest request);
    OrderDto PayOrder(Guid id, PayOrderRequest request);
    OrderDto CancelOrder(Guid id);
    IReadOnlyList<AiInsightDto> GetAiInsights();
}
