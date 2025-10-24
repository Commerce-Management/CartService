using System.ComponentModel.DataAnnotations;

namespace CartService.Core.Entities;

public class Cart
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; } 
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>(); 
}