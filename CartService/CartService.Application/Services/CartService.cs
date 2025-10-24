using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CartService.Core.Entities;
using CartService.Core.Interfaces;
using CartService.Infrastructure.Context;
using CartService.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CartService.Shared.Protos.GrpcProductService;
using CartService.Shared.Protos.GrpcShopService; // product.proto (csharp_namespace)

namespace CartService.Application.Services
{
    public class CartService(CartDbContext context,
        ProductService.ProductServiceClient productServiceClient,
        ShopService.ShopServiceClient shopServiceClient) : ICartService
    {
        public async Task<CartDto> GetCartByUserIdAsync(Guid userId)
        {
            var cart = await EnsureCartAsync(userId);

            var productIds = cart.CartItems.Select(ci => ci.ProductId.ToString()).Distinct().ToList();

            // 1. Получаем продукты через ProductService
            var productsResponse = await productServiceClient.GetProductsByIdsAsync(
                new GetProductsByIdsRequest { ProductIds = { productIds } });

            var productsById = productsResponse.Products
                .Where(p => Guid.TryParse(p.Id, out _))
                .ToDictionary(p => Guid.Parse(p.Id), p => p);

            // 2. Собираем уникальные shop_ids
            var shopIds = productsResponse.Products
                .Select(p => p.ShopId)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList();

            // 3. Получаем имена магазинов через ShopService
            var shopsById = new Dictionary<string, string>();

            foreach (var shopId in shopIds)
            {
                try
                {
                    var shop = await shopServiceClient.GetShopByIdAsync(
                        new GetShopByIdRequest { ShopId = shopId });

                    shopsById[shopId] = shop.Name ?? "Unknown Shop";
                }
                catch
                {
                    shopsById[shopId] = "Unknown Shop";
                }
            }

            // 4. Формируем ответ
            var items = cart.CartItems.Select(ci =>
            {
                productsById.TryGetValue(ci.ProductId, out var product);

                var productName = product?.Name ?? "Product";
                var imageUrls = product?.ImageUrls?.ToList() ?? new List<string>();
                var shopId = product?.ShopId ?? string.Empty;
                var shopName = shopsById.TryGetValue(shopId, out var sn) ? sn : "Unknown Shop";

                return new CartItemDto(
                    Id: ci.Id.ToString(),
                    ProductId: ci.ProductId.ToString(),
                    ProductName: productName,
                    ImageUrls: imageUrls,
                    ShopId: shopId,
                    ShopName: shopName,
                    UnitPrice: ci.UnitPrice,
                    Quantity: ci.Quantity,
                    TotalPrice: ci.UnitPrice * ci.Quantity
                );
            }).ToList();

            var totalAmount = items.Sum(i => i.TotalPrice);

            return new CartDto(userId.ToString(), items, totalAmount);
        }



        public async Task AddItemToCartAsync(Guid userId, Guid productId, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");

            var cart = await EnsureCartAsync(userId);

            var resp = await productServiceClient.GetProductsByIdsAsync(
                new GetProductsByIdsRequest { ProductIds = { productId.ToString() } });

            var p = resp.Products.FirstOrDefault();
            if (p is null)
                throw new KeyNotFoundException($"Product {productId} not found.");

            var unitPrice = MinorToDecimal(p.Price);  

            var existing = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (existing != null)
            {
                checked { existing.Quantity += quantity; }
            }
            else
            {
                var item = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = unitPrice  
                };
                context.CartItems.Add(item);
                cart.CartItems.Add(item);
            }

            await SaveWithRetryAsync();
        }

        public async Task RemoveItemFromCartAsync(Guid userId, Guid cartItemId)
        {
            var cart = await context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                throw new ArgumentException("Cart not found");

            var item = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
            if (item == null)
                throw new ArgumentException("Cart item not found");

            context.CartItems.Remove(item);
            await SaveWithRetryAsync();
        }

        public async Task UpdateCartItemQuantityAsync(Guid userId, Guid cartItemId, int quantity)
        {
            var cart = await context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                throw new ArgumentException("Cart not found");

            var item = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
            if (item == null)
                throw new ArgumentException("Cart item not found");

            if (quantity <= 0)
            {
                context.CartItems.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }

            await SaveWithRetryAsync();
        }

        public async Task ClearCartAsync(Guid userId)
        {
            var cart = await context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                throw new ArgumentException("Cart not found");

            context.CartItems.RemoveRange(cart.CartItems);
            await SaveWithRetryAsync();
        }

        public async Task<decimal> CalculateCartTotalAsync(Guid userId)
        {
            var cart = await context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return 0m;

            return cart.CartItems.Sum(ci => ci.UnitPrice * ci.Quantity);
        }


        private async Task<Cart> EnsureCartAsync(Guid userId)
        {
            var cart = await context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null) return cart;

            cart = new Cart { UserId = userId };
            context.Carts.Add(cart);
            await context.SaveChangesAsync();

            return await context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId) ?? cart;
        }

        private static decimal MinorToDecimal(long minor) =>
            decimal.Divide(minor, 100m);  

        private async Task SaveWithRetryAsync()
        {
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                foreach (var entry in context.ChangeTracker.Entries().Where(e => e.State != EntityState.Detached))
                    await entry.ReloadAsync();

                await context.SaveChangesAsync();
            }
        }
    }
}
