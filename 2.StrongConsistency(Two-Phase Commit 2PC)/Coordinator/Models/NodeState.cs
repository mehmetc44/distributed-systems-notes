using System;
using Coordinator.Enums;

namespace Coordinator.Models;

public class NodeState
{
    public Guid Id { get; set; }
    public ReadyType IsReady { get; set; }
    public TransactionState TransactionState { get; set; }
    public Node Node { get; set; }
}
