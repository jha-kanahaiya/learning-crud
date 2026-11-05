using LearningCrud.Web.Data;
using LearningCrud.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningCrud.Web.Controllers;

[AutoValidateAntiforgeryToken]
public sealed class LearningItemsController(LearningDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await dbContext.LearningItems
            .OrderByDescending(item => item.CreatedAtUtc)
            .ThenByDescending(item => item.Id)
            .ToListAsync(cancellationToken));

    public async Task<IActionResult> Details(int? id, CancellationToken cancellationToken)
    {
        if (id is null)
        {
            return NotFound();
        }

        var item = await dbContext.LearningItems
            .AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        return item is null ? NotFound() : View(item);
    }

    [HttpGet]
    public IActionResult Create() => View(new LearningItemInputModel());

    [HttpPost]
    public async Task<IActionResult> Create(
        LearningItemInputModel input,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(input);
        }

        var item = new LearningItem
        {
            Title = input.Title.Trim(),
            Description = NormalizeDescription(input.Description),
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.LearningItems.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Details), new { id = item.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id, CancellationToken cancellationToken)
    {
        var item = await FindItemAsync(id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        return View(new LearningItemInputModel
        {
            Title = item.Title,
            Description = item.Description
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(
        int id,
        LearningItemInputModel input,
        CancellationToken cancellationToken)
    {
        var item = await FindItemAsync(id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(input);
        }

        item.Title = input.Title.Trim();
        item.Description = NormalizeDescription(input.Description);
        await dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Details), new { id = item.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int? id, CancellationToken cancellationToken)
    {
        var item = await FindItemAsync(id, cancellationToken);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var item = await FindItemAsync(id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        dbContext.LearningItems.Remove(item);
        await dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    private Task<LearningItem?> FindItemAsync(int? id, CancellationToken cancellationToken)
    {
        if (id is null)
        {
            return Task.FromResult<LearningItem?>(null);
        }

        return dbContext.LearningItems
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    private static string? NormalizeDescription(string? description)
    {
        var normalized = description?.Trim();
        return string.IsNullOrEmpty(normalized) ? null : normalized;
    }
}
