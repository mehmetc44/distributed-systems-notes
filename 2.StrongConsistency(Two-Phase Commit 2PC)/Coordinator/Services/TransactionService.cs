using System;
using Coordinator.Abstraction;
using Coordinator.Context;
using Coordinator.Models;
using Microsoft.EntityFrameworkCore;

namespace Coordinator.Services;

public class TransactionService(IHttpClientFactory _httpClientFactory, TwoPhaseCommitContext _context) : ITransactionService
    {


        HttpClient _orderHttpClient = _httpClientFactory.CreateClient("Order.API");
        HttpClient _stockHttpClient = _httpClientFactory.CreateClient("Stock.API");
        HttpClient _paymentHttpClient = _httpClientFactory.CreateClient("PaymentAPI");

        public async Task<Guid> CreateTransactionAsync()
        {
            Guid transactionId = Guid.NewGuid();

            var nodes = await _context.Nodes.ToListAsync();
            nodes.ForEach(node => node.NodeStates = new List<NodeState>()
            {
                new(transactionId)
                {
                    IsReady = Enums.ReadyType.PENDING,
                    TransactionState = Enums.TransactionState.PENDING
                }
            });

            await _context.SaveChangesAsync();
            return transactionId;
        }
        public async Task PrepareServicesAsync(Guid transactionId)
        {
            var transactionNodes = await _context.NodeStates
                   .Include(ns => ns.Node)
                   .Where(ns => ns.TransactionId == transactionId)
                   .ToListAsync();

            foreach (var transactionNode in transactionNodes)
            {
                try
                {
                    var response = await (transactionNode.Node.Name switch
                    {
                        "Order.API" => _orderHttpClient.GetAsync("ready"),
                        "Stock.API" => _stockHttpClient.GetAsync("ready"),
                        "Payment.API" => _paymentHttpClient.GetAsync("ready"),
                    });

                    var result = bool.Parse(await response.Content.ReadAsStringAsync());
                    transactionNode.IsReady = result ? Enums.ReadyType.READY : Enums.ReadyType.FAILED;
                }
                catch (Exception)
                {
                    transactionNode.IsReady = Enums.ReadyType.FAILED;
                }
            }

            await _context.SaveChangesAsync();
        }
        public async Task<bool> CheckReadyServicesAsync(Guid transactionId)
            => (await _context.NodeStates
                         .Where(ns => ns.TransactionId == transactionId)
                         .ToListAsync()).TrueForAll(ns => ns.IsReady == Enums.ReadyType.READY);
        public async Task CommitAsync(Guid transactionId)
        {
            var transactionNodes = await _context.NodeStates
                                    .Where(ns => ns.TransactionId == transactionId)
                                    .Include(ns => ns.Node)
                                    .ToListAsync();

            foreach (var transactionNode in transactionNodes)
            {
                try
                {
                    var response = await (transactionNode.Node.Name switch
                    {
                        "Order.API" => _orderHttpClient.GetAsync("commit"),
                        "Stock.API" => _stockHttpClient.GetAsync("commit"),
                        "Payment.API" => _paymentHttpClient.GetAsync("commit")
                    });

                    var result = bool.Parse(await response.Content.ReadAsStringAsync());
                    transactionNode.TransactionState = result ? Enums.TransactionState.DONE : Enums.TransactionState.ABORT;
                }
                catch
                {
                    transactionNode.TransactionState = Enums.TransactionState.ABORT;
                }
            }

            await _context.SaveChangesAsync();
        }
        public async Task<bool> CheckTransactionStateServicesAsync(Guid transactionId)
            => (await _context.NodeStates
            .Where(ns => ns.TransactionId == transactionId)
            .ToListAsync()).TrueForAll(ns => ns.TransactionState == Enums.TransactionState.DONE);
        public async Task RollbackAsync(Guid transactionId)
        {
            var transactionNodes = await _context.NodeStates
                .Where(ns => ns.TransactionId == transactionId)
                .Include(ns => ns.Node)
                .ToListAsync();

            foreach (var transactionNode in transactionNodes)
            {
                try
                {
                    if (transactionNode.TransactionState == Enums.TransactionState.DONE)
                        _ = await (transactionNode.Node.Name switch
                        {
                            "Order.API" => _orderHttpClient.GetAsync("rollback"),
                            "Stock.API" => _stockHttpClient.GetAsync("rollback"),
                            "Payment.API" => _paymentHttpClient.GetAsync("rollback"),
                        });

                    transactionNode.TransactionState = Enums.TransactionState.ABORT;
                }
                catch
                {
                    transactionNode.TransactionState = Enums.TransactionState.ABORT;
                }
            }

            await _context.SaveChangesAsync();
        }

}