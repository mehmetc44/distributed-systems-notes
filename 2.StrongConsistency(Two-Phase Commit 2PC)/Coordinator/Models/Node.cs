namespace Coordinator.Models;

public record class Node(string Name)
{
    public Guid Id { get; set; }
}
