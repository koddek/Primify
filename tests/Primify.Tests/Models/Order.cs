using LiteDB;

namespace Primify.Tests.Models;

public sealed class Order
{
    public OrderId Id { get; set; } = default!;
    public CustomerName Customer { get; set; } = default!;
    public Quantity Quantity { get; set; } = default!;
}
