namespace Context.Domain.Entities;

public sealed class Product
{
    public int Id { get; set; }

    public required string Name { get; set; }
}
