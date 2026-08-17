using System;
using Microsoft.EntityFrameworkCore;

namespace Coordinator.Context;

public class TwoPhaseCommitContext : DbContext
{
    public TwoPhaseCommitContext(DbContextOptions<TwoPhaseCommitContext> options) : base(options)
    {
    }

    public DbSet<Models.Node> Nodes { get; set; }
    public DbSet<Models.NodeState> NodeStates { get; set; }
}
