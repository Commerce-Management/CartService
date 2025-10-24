namespace CartService.Shared.Dtos;

public record CartItemDto(
    string Id,
    string ProductId,
    string ProductName,
    List<string> ImageUrls,
    string ShopId,
    string ShopName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);