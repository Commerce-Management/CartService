using System.ComponentModel.DataAnnotations;

namespace CartService.Core.Entities;

public class CartItem 
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public virtual Cart Cart { get; set; } = null!;
}