using System.Security.Claims;
using Asp.Versioning;
using CartService.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace CartService.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]

public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetCart()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            Log.Error("CartController.GetCart: Invalid user claim. Claims: {@Claims}",
                User.Claims.Select(c => new { c.Type, c.Value }));
            return Unauthorized(new
            {
                ErrorCode = "UserIdentificationFailed",
                Message = "Не удалось определить пользователя из токена."
            });
        }
        try
        {
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            return Ok(cart);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                ErrorCode = "BusinessRuleViolation",
                Message = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace
            });
        }
    }
    
    [Authorize]
    [HttpPost("{productId}")]
    public async Task<IActionResult> AddItemToCart(Guid productId, [FromQuery] int quantity)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            Log.Error("CartController.GetCart: Invalid user claim. Claims: {@Claims}",
                User.Claims.Select(c => new { c.Type, c.Value }));
            return Unauthorized(new
            {
                ErrorCode = "UserIdentificationFailed",
                Message = "Не удалось определить пользователя из токена."
            });
        }
        try
        {
            await _cartService.AddItemToCartAsync(userId, productId, quantity);
            return Ok(new { message = "Item added to cart successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpDelete("{cartItemId}")]
    public async Task<IActionResult> RemoveItemFromCart(Guid cartItemId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            Log.Error("CartController.GetCart: Invalid user claim. Claims: {@Claims}",
                User.Claims.Select(c => new { c.Type, c.Value }));
            return Unauthorized(new
            {
                ErrorCode = "UserIdentificationFailed",
                Message = "Не удалось определить пользователя из токена."
            });
        }
        try
        {
            await _cartService.RemoveItemFromCartAsync(userId, cartItemId);
            return Ok(new { message = "Item removed from cart successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [Authorize]
    [HttpPut("{cartItemId}")]
    public async Task<IActionResult> UpdateCartItemQuantity(Guid cartItemId, [FromQuery] int quantity)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            Log.Error("CartController.GetCart: Invalid user claim. Claims: {@Claims}",
                User.Claims.Select(c => new { c.Type, c.Value }));
            return Unauthorized(new
            {
                ErrorCode = "UserIdentificationFailed",
                Message = "Не удалось определить пользователя из токена."
            });
        }
        try
        {
            await _cartService.UpdateCartItemQuantityAsync(userId, cartItemId, quantity);
            return Ok(new { message = "Cart item quantity updated successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpDelete("clear")]
    public async Task<IActionResult> ClearCart()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            Log.Error("CartController.GetCart: Invalid user claim. Claims: {@Claims}",
                User.Claims.Select(c => new { c.Type, c.Value }));
            return Unauthorized(new
            {
                ErrorCode = "UserIdentificationFailed",
                Message = "Не удалось определить пользователя из токена."
            });
        }
        try
        {
            await _cartService.ClearCartAsync(userId);
            return Ok(new { message = "Cart cleared successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [Authorize]
    [HttpGet("price-total")]
    public async Task<IActionResult> GetCartTotal()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            Log.Error("CartController.GetCart: Invalid user claim. Claims: {@Claims}",
                User.Claims.Select(c => new { c.Type, c.Value }));
            return Unauthorized(new
            {
                ErrorCode = "UserIdentificationFailed",
                Message = "Не удалось определить пользователя из токена."
            });
        }
        try
        {
            var total = await _cartService.CalculateCartTotalAsync(userId);
            return Ok(new { total });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}