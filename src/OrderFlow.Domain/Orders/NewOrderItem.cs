namespace OrderFlow.Domain.Orders;

// OrderItem.Create é internal: só o agregado Order pode construir seus itens. Este record
// existe para a Application conseguir passar os dados de cada item pela fronteira do agregado
// sem expor a construção de OrderItem diretamente.
public sealed record NewOrderItem(Guid ProductId, decimal UnitPrice, int Quantity);
