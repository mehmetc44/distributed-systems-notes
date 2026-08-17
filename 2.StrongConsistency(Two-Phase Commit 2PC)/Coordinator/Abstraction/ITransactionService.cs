using System;

namespace Coordinator.Abstraction;

public interface ITransactionService
{
    Task<Guid> CreateTransactionAsync();
    Task PrapareServicesAsync(Guid TransactionId);
    Task<bool> CheckServicesReadyAsync(Guid TransactionId);
    Task CommitAsync(Guid TransactionId);
    Task CheckTransactionStateServicesAsync(Guid TransactionId);
    Task RollBackAsync(Guid TransactionId);

}
