using CartService.Core.Interfaces;
using CartService.Shared.Protos.GrpcCartService;
using Grpc.Core;
using Serilog;

namespace CartService.Infrastructure.gRPC;

public class GrpcCartService : CartService.Shared.Protos.GrpcCartService.CartService.CartServiceBase
{
    private readonly ICartService _cartService;
    private readonly ILogger _logger;

    public GrpcCartService(ICartService cartService, ILogger logger)
    {
        _cartService = cartService;
        _logger = logger;
    }

    public override async Task<GetCartByUserIdResponse> GetCartByUserId(
        GetCartByUserIdRequest request,
        ServerCallContext context)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.UserId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "UserId is required"));

        if (!Guid.TryParse(request.UserId, out Guid userId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "UserId invalid"));

        var cartDto = await _cartService.GetCartByUserIdAsync(userId);
        var resp = new GetCartByUserIdResponse();

        if (cartDto == null || cartDto.Items == null || cartDto.Items.Count == 0)
            return resp;  

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in cartDto.Items)
        {
            if (string.IsNullOrWhiteSpace(item.ProductId) || !Guid.TryParse(item.ProductId, out _))
                continue;

            if (seen.Add(item.ProductId))
            {
                resp.Items.Add(new CartProduct
                {
                    UserId = request.UserId,
                    ProductId = item.ProductId
                });
            }
        }

        return resp;
    }
}

