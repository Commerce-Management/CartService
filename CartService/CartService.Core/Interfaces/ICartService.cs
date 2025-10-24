using CartService.Shared.Dtos;

namespace CartService.Core.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartByUserIdAsync(Guid userId); 
    
    Task AddItemToCartAsync(Guid userId, Guid productId, int quantity);
    Task RemoveItemFromCartAsync(Guid userId, Guid cartItemId);
    Task UpdateCartItemQuantityAsync(Guid userId, Guid cartItemId, int quantity);
    Task ClearCartAsync(Guid userId); 
    Task<decimal> CalculateCartTotalAsync(Guid userId);
}