using Mediator;
using OrderFlow.Application._Shared;
using OrderFlow.Domain._Shared;
using OrderFlow.Domain.Orders;
using OrderFlow.Domain.Products;

namespace OrderFlow.Application.Orders.Place;

public sealed class PlaceOrderCommandHandler(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<PlaceOrderCommand, Result<PlaceOrderResponse>>
{
    public async ValueTask<Result<PlaceOrderResponse>> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var drafts = new List<OrderItemDraft>();

        foreach (var item in request.Items)
        {
            var product = await productRepository.GetByIdAsync(item.ProductId, cancellationToken);

            if (product is null)
            {
                return Result<PlaceOrderResponse>.Failure(Error.NotFound("product.not_found", $"Produto '{item.ProductId}' não encontrado."));
            }

            if (product.AvailableQuantity < item.Quantity)
            {
                return Result<PlaceOrderResponse>.Failure(
                    Error.Conflict("order.insufficient_stock", $"Estoque insuficiente para o produto '{product.Name}'."));
            }

            drafts.Add(new OrderItemDraft(product.Id, product.UnitPrice, item.Quantity));
        }

        var order = Order.Place(request.CustomerId, request.Currency, drafts);

        orderRepository.Add(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PlaceOrderResponse>.Success(PlaceOrderResponse.From(order));
    }
}
