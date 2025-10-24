namespace CartService.Shared.Dtos;

public record CartDto(string UserId, List<CartItemDto> Items, decimal TotalAmount);