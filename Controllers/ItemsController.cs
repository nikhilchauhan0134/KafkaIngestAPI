using DataCaptureApi.Models;
using DataCaptureApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DataCaptureApi.Controllers;

[ApiController]
[Route("api/items")]
public sealed class ItemsController : ControllerBase
{
    private readonly IItemRepository _items;

    public ItemsController(IItemRepository items)
    {
        _items = items;
    }

    [HttpPost]
    public async Task<ActionResult<Item>> Create(CreateItemRequest? request, CancellationToken cancellationToken)
    {
        if (request is null
            || string.IsNullOrWhiteSpace(request.Name)
            || string.IsNullOrWhiteSpace(request.Description))
        {
            return BadRequest(new
            {
                error = "A JSON object with non-empty name"
            });
        }

        if (request.Name.Trim().Length > 200 || request.Description.Trim().Length > 4000)
        {
            return BadRequest(new
            {
                error = "Name must be 200 characters or fewer"
            });
        }

        var item = await _items.AddAsync(request.Name.Trim(), request.Description.Trim(), cancellationToken);
        return Created($"/api/items/{item.Id}", item);
    }
}
