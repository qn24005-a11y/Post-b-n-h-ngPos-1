using PosBanHang.Application.Common;
using PosBanHang.Application.DTOs;
using PosBanHang.Application.Interfaces;
using PosBanHang.Domain.Entities;
using PosBanHang.Domain.Enums;

namespace PosBanHang.Application.Services;

public class PosService(IPosStore store) : IPosService
{
    public DashboardDto GetDashboard()
    {
        var today = DateTime.UtcNow.Date;
        var paidToday = store.Orders.Where(order => order.Status == OrderStatus.Paid && order.PaidAt?.Date == today).ToList();
        var recentOrders = store.Orders.OrderByDescending(order => order.CreatedAt).Take(8).Select(MapOrder).ToList();
        var topProducts = store.Products.Where(product => !product.IsDeleted).OrderByDescending(product => product.Stock).Take(5).Select(MapProduct).ToList();

        return new DashboardDto(
            paidToday.Sum(order => order.Total),
            store.Orders.Count(order => order.CreatedAt.Date == today),
            store.Orders.Count(order => order.Status is OrderStatus.Pending or OrderStatus.Cooking or OrderStatus.Ready),
            store.Products.Count(product => !product.IsDeleted && product.Stock <= product.LowStockThreshold),
            topProducts,
            recentOrders);
    }

    public IReadOnlyList<BranchDto> GetBranches() =>
        store.Branches.Select(branch => new BranchDto(branch.Id, branch.Name, branch.Address, branch.Phone, branch.IsActive)).ToList();

    public IReadOnlyList<CategoryDto> GetCategories() =>
        store.Categories.Select(category => new CategoryDto(category.Id, category.Name, category.Description, category.IsActive)).ToList();

    public IReadOnlyList<CustomerDto> GetCustomers(string? search) =>
        Filter(store.Customers.AsEnumerable(), search, customer => $"{customer.FullName} {customer.Phone} {customer.Email}")
            .Select(customer => new CustomerDto(customer.Id, customer.FullName, customer.Phone, customer.Email, customer.LoyaltyPoint, customer.Rank, customer.IsActive))
            .ToList();

    public IReadOnlyList<StaffDto> GetStaff(string? search) =>
        Filter(store.Staff.AsEnumerable(), search, staff => $"{staff.FullName} {staff.Phone} {staff.Email} {staff.Role}")
            .Select(staff => new StaffDto(staff.Id, staff.BranchId, staff.FullName, staff.Email, staff.Phone, staff.Role, staff.Position, staff.Status, staff.Salary, staff.AvatarUrl))
            .ToList();

    public CustomerDto CreateCustomer(UpsertCustomerRequest request)
    {
        ValidateCustomer(request);
        var customer = new Customer
        {
            TenantId = DemoTenantId,
            FullName = request.FullName.Trim(),
            Phone = request.Phone.Trim(),
            Email = request.Email.Trim(),
            LoyaltyPoint = Math.Max(0, request.LoyaltyPoint),
            Rank = string.IsNullOrWhiteSpace(request.Rank) ? "Member" : request.Rank.Trim(),
            IsActive = request.IsActive
        };

        store.Customers.Add(customer);
        return MapCustomer(customer);
    }

    public CustomerDto UpdateCustomer(Guid id, UpsertCustomerRequest request)
    {
        ValidateCustomer(request);
        var customer = store.Customers.FirstOrDefault(item => item.Id == id)
            ?? throw new InvalidOperationException("Khong tim thay khach hang.");

        customer.FullName = request.FullName.Trim();
        customer.Phone = request.Phone.Trim();
        customer.Email = request.Email.Trim();
        customer.LoyaltyPoint = Math.Max(0, request.LoyaltyPoint);
        customer.Rank = string.IsNullOrWhiteSpace(request.Rank) ? "Member" : request.Rank.Trim();
        customer.IsActive = request.IsActive;
        return MapCustomer(customer);
    }

    public void DeleteCustomer(Guid id)
    {
        var customer = store.Customers.FirstOrDefault(item => item.Id == id)
            ?? throw new InvalidOperationException("Khong tim thay khach hang.");
        store.Customers.Remove(customer);
    }

    public StaffDto CreateStaff(UpsertStaffRequest request)
    {
        ValidateStaff(request);
        var staff = new Staff
        {
            TenantId = DemoTenantId,
            BranchId = request.BranchId,
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            Phone = request.Phone.Trim(),
            Role = request.Role.Trim(),
            Position = request.Position.Trim(),
            Status = request.Status,
            Salary = Math.Max(0, request.Salary),
            AvatarUrl = request.AvatarUrl
        };

        store.Staff.Add(staff);
        return MapStaff(staff);
    }

    public StaffDto UpdateStaff(Guid id, UpsertStaffRequest request)
    {
        ValidateStaff(request);
        var staff = store.Staff.FirstOrDefault(item => item.Id == id)
            ?? throw new InvalidOperationException("Khong tim thay nhan su.");

        staff.BranchId = request.BranchId;
        staff.FullName = request.FullName.Trim();
        staff.Email = request.Email.Trim();
        staff.Phone = request.Phone.Trim();
        staff.Role = request.Role.Trim();
        staff.Position = request.Position.Trim();
        staff.Status = request.Status;
        staff.Salary = Math.Max(0, request.Salary);
        staff.AvatarUrl = request.AvatarUrl;
        return MapStaff(staff);
    }

    public void DeleteStaff(Guid id)
    {
        var staff = store.Staff.FirstOrDefault(item => item.Id == id)
            ?? throw new InvalidOperationException("Khong tim thay nhan su.");
        store.Staff.Remove(staff);
    }

    public PagedResult<ProductDto> GetProducts(string? search, Guid? categoryId, int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 5, 100);
        var query = store.Products.Where(product => !product.IsDeleted);

        if (categoryId.HasValue)
        {
            query = query.Where(product => product.CategoryId == categoryId);
        }

        query = Filter(query, search, product => $"{product.Name} {product.Sku} {product.Barcode}");
        var total = query.Count();
        var items = query.OrderBy(product => product.Name).Skip((page - 1) * pageSize).Take(pageSize).Select(MapProduct).ToList();

        return new PagedResult<ProductDto>(items, page, pageSize, total);
    }

    public ProductDto CreateProduct(UpsertProductRequest request)
    {
        ValidateProduct(request);
        var product = new Product
        {
            TenantId = DemoTenantId,
            BranchId = request.BranchId,
            CategoryId = request.CategoryId,
            Name = request.Name.Trim(),
            Sku = request.Sku.Trim(),
            Barcode = request.Barcode,
            Price = request.Price,
            Stock = request.Stock,
            LowStockThreshold = request.LowStockThreshold,
            ImageUrl = request.ImageUrl,
            IsActive = request.IsActive
        };

        store.Products.Add(product);
        return MapProduct(product);
    }

    public ProductDto UpdateProduct(Guid id, UpsertProductRequest request)
    {
        ValidateProduct(request);
        var product = store.Products.FirstOrDefault(item => item.Id == id && !item.IsDeleted)
            ?? throw new InvalidOperationException("Khong tim thay san pham.");

        product.BranchId = request.BranchId;
        product.CategoryId = request.CategoryId;
        product.Name = request.Name.Trim();
        product.Sku = request.Sku.Trim();
        product.Barcode = request.Barcode;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.LowStockThreshold = request.LowStockThreshold;
        product.ImageUrl = request.ImageUrl;
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTime.UtcNow;
        return MapProduct(product);
    }

    public void DeleteProduct(Guid id)
    {
        var product = store.Products.FirstOrDefault(item => item.Id == id && !item.IsDeleted)
            ?? throw new InvalidOperationException("Khong tim thay san pham.");
        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;
    }

    public IReadOnlyList<OrderDto> GetOrders(string? status)
    {
        var query = store.Orders.AsEnumerable();
        if (Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(order => order.Status == parsedStatus);
        }

        return query.OrderByDescending(order => order.CreatedAt).Select(MapOrder).ToList();
    }

    public OrderDto CreateOrder(CreateOrderRequest request)
    {
        if (request.Items.Count == 0)
        {
            throw new InvalidOperationException("Don hang can it nhat mot san pham.");
        }

        var order = new Order
        {
            TenantId = DemoTenantId,
            BranchId = request.BranchId,
            TableName = string.IsNullOrWhiteSpace(request.TableName) ? "Takeaway" : request.TableName.Trim(),
            CustomerId = request.CustomerId,
            CustomerName = store.Customers.FirstOrDefault(customer => customer.Id == request.CustomerId)?.FullName ?? "Khach le",
            CashierName = string.IsNullOrWhiteSpace(request.CashierName) ? "Cashier" : request.CashierName.Trim(),
            Code = $"HD{DateTime.UtcNow:yyMMddHHmmss}",
            Discount = Math.Max(0, request.Discount),
            Status = OrderStatus.Cooking
        };

        foreach (var item in request.Items)
        {
            var product = store.Products.FirstOrDefault(candidate => candidate.Id == item.ProductId && candidate.IsActive && !candidate.IsDeleted)
                ?? throw new InvalidOperationException("San pham khong ton tai hoac da ngung ban.");

            if (product.Stock < item.Quantity)
            {
                throw new InvalidOperationException($"{product.Name} khong du ton kho.");
            }

            product.Stock -= item.Quantity;
            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                Note = item.Note
            });
        }

        store.Orders.Add(order);
        return MapOrder(order);
    }

    public OrderDto PayOrder(Guid id, PayOrderRequest request)
    {
        var order = store.Orders.FirstOrDefault(item => item.Id == id)
            ?? throw new InvalidOperationException("Khong tim thay don hang.");

        order.Status = OrderStatus.Paid;
        order.PaymentMethod = request.PaymentMethod;
        order.PaidAt = DateTime.UtcNow;
        store.Invoices.Add(new Invoice
        {
            OrderId = order.Id,
            Code = order.Code.Replace("HD", "INV"),
            Amount = order.Total,
            PaymentMethod = request.PaymentMethod
        });

        return MapOrder(order);
    }

    public OrderDto CancelOrder(Guid id)
    {
        var order = store.Orders.FirstOrDefault(item => item.Id == id)
            ?? throw new InvalidOperationException("Khong tim thay don hang.");

        if (order.Status == OrderStatus.Paid)
        {
            throw new InvalidOperationException("Don da thanh toan khong the huy truc tiep.");
        }

        if (order.Status != OrderStatus.Cancelled)
        {
            foreach (var item in order.Items)
            {
                var product = store.Products.FirstOrDefault(product => product.Id == item.ProductId);
                if (product is not null)
                {
                    product.Stock += item.Quantity;
                }
            }
        }

        order.Status = OrderStatus.Cancelled;
        return MapOrder(order);
    }

    public IReadOnlyList<AiInsightDto> GetAiInsights()
    {
        var lowStock = store.Products.Where(product => !product.IsDeleted && product.Stock <= product.LowStockThreshold).Take(3).ToList();
        var revenueToday = GetDashboard().RevenueToday;
        var insights = new List<AiInsightDto>
        {
            new("Doanh thu hom nay", revenueToday > 0 ? $"Da ghi nhan {revenueToday:n0} VND doanh thu trong ngay." : "Chua co hoa don thanh toan hom nay.", "info"),
            new("Khung gio POS", "Nen bo tri them nhan vien vao khung 18h-20h neu luong order tang.", "info")
        };

        insights.AddRange(lowStock.Select(product => new AiInsightDto("Canh bao ton kho", $"{product.Name} chi con {product.Stock} don vi, nen nhap them hang.", "warning")));
        return insights;
    }

    private static IEnumerable<T> Filter<T>(IEnumerable<T> source, string? search, Func<T, string> selector)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return source;
        }

        return source.Where(item => selector(item).Contains(search, StringComparison.OrdinalIgnoreCase));
    }

    private static ProductDto MapProduct(Product product) =>
        new(product.Id, product.BranchId, product.CategoryId, product.Name, product.Sku, product.Barcode, product.Price, product.Stock, product.LowStockThreshold, product.ImageUrl, product.IsActive);

    private static CustomerDto MapCustomer(Customer customer) =>
        new(customer.Id, customer.FullName, customer.Phone, customer.Email, customer.LoyaltyPoint, customer.Rank, customer.IsActive);

    private static StaffDto MapStaff(Staff staff) =>
        new(staff.Id, staff.BranchId, staff.FullName, staff.Email, staff.Phone, staff.Role, staff.Position, staff.Status, staff.Salary, staff.AvatarUrl);

    private static OrderDto MapOrder(Order order) =>
        new(order.Id, order.Code, order.BranchId, order.TableName, order.CustomerName, order.CashierName, order.Status, order.PaymentMethod,
            order.Items.Select(item => new OrderItemDto(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice, item.LineTotal, item.Note)).ToList(),
            order.Discount, order.Total, order.CreatedAt, order.PaidAt);

    private static void ValidateProduct(UpsertProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new InvalidOperationException("Ten san pham la bat buoc.");
        }

        if (request.Price < 0 || request.Stock < 0 || request.LowStockThreshold < 0)
        {
            throw new InvalidOperationException("Gia, ton kho va nguong canh bao phai lon hon hoac bang 0.");
        }
    }

    private static void ValidateCustomer(UpsertCustomerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Phone))
        {
            throw new InvalidOperationException("Ho ten va so dien thoai khach hang la bat buoc.");
        }
    }

    private static void ValidateStaff(UpsertStaffRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.Role))
        {
            throw new InvalidOperationException("Ho ten, so dien thoai va vai tro nhan su la bat buoc.");
        }
    }

    private static Guid DemoTenantId => Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
}
