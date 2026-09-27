using DataCaptureApi.Models;

namespace DataCaptureApi.Repositories;

public interface IItemRepository
{
    Task<Item> AddAsync(string name, string description, CancellationToken cancellationToken);
}
