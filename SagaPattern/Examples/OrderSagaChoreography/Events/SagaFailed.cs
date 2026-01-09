using SagaPattern.Examples.OrderSaga.Context;

namespace SagaPattern.Examples.OrderSagaChoreography.Events;

public sealed record SagaFailed(OrderSagaContext Context, string FailedStep, string Error);
