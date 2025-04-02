using LiteDB;

namespace Primify.Tests.Models;

public sealed class Order
{
    public OrderId Id { get; set; }
    public CustomerName Customer { get; set; }
    public Quantity Quantity { get; set; }
}
