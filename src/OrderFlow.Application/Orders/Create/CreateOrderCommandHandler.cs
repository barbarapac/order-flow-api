using Mediator;
using OrderFlow.Application._Shared;
using OrderFlow.Domain._Shared;
using OrderFlow.Domain.Orders;
using OrderFlow.Domain.Products;

namespace OrderFlow.Application.Orders.Create;

public sealed class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateOrderCommand, Result<CreateOrderResponse>>
{
    public async ValueTask<Result<CreateOrderResponse>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var drafts = new List<NewOrderItem>();

        foreach (var item in request.Items)
        {
            var product = await productRepository.GetByIdAsync(item.ProductId, cancellationToken);

            if (product is null)
            {
                return Result<CreateOrderResponse>.Failure(Error.NotFound("product.not_found", $"Produto '{item.ProductId}' não encontrado."));
            }

            if (product.AvailableQuantity < item.Quantity)
            {
                return Result<CreateOrderResponse>.Failure(Error.Conflict("order.insufficient_stock", $"Estoque insuficiente para o produto '{product.Name}'."));
            }

            drafts.Add(new NewOrderItem(product.Id, product.UnitPrice, item.Quantity));
        }

        var order = Order.Create(request.CustomerId, request.Currency, drafts);

        orderRepository.Add(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CreateOrderResponse>.Success(CreateOrderResponse.From(order));
    }
}
