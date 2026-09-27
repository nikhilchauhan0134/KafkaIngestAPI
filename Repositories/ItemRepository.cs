using DataCaptureApi.Data;
using DataCaptureApi.Models;

namespace DataCaptureApi.Repositories;

public sealed class ItemRepository : IItemRepository
{
    private readonly AppDbContext _db;

    public ItemRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Item> AddAsync(string name, string description, CancellationToken cancellationToken)
    {
        var item = new Item
        {
            Name = name,
            Description = description
        };

        _db.Items.Add(item);
        await _db.SaveChangesAsync(cancellationToken);
        return item;
    }
}
