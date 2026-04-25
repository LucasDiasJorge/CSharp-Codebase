using SagaPattern.Examples.OrderSaga.Context;

namespace SagaPattern.Examples.OrderSagaChoreography.Events;

public sealed record StartOrderSaga(OrderSagaContext Context);
