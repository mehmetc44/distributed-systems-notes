using System;
using Coordinator.Abstraction;

namespace Coordinator.Services;

public class TransactionService : ITransactionService
{
    public Task<bool> CheckServicesReadyAsync(Guid TransactionId)
    {
        throw new NotImplementedException();
    }

    public Task CheckTransactionStateServicesAsync(Guid TransactionId)
    {
        throw new NotImplementedException();
    }

    public Task CommitAsync(Guid TransactionId)
    {
        throw new NotImplementedException();
    }

    public Task<Guid> CreateTransactionAsync()
    {
        throw new NotImplementedException();
    }

    public Task PrapareServicesAsync(Guid TransactionId)
    {
        throw new NotImplementedException();
    }

    public Task RollBackAsync(Guid TransactionId)
    {
        throw new NotImplementedException();
    }
}
