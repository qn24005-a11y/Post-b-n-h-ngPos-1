using PosBanHang.Domain.Entities;

namespace PosBanHang.Application.Interfaces;

public interface IPosStore
{
    List<Branch> Branches { get; }
    List<Category> Categories { get; }
    List<Product> Products { get; }
    List<Customer> Customers { get; }
    List<Staff> Staff { get; }
    List<Order> Orders { get; }
    List<Invoice> Invoices { get; }
}
