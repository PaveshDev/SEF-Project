using LoopWorth.Application.DTOs;
using LoopWorth.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LoopWorth.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;
    public CategoriesController(AppDbContext context) { _context = context; }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var categories = await _context.Categories
            .Where(c => c.IsActive)
            .Select(c => new CategoryDto { Id = c.Id, Code = c.Code, Name = c.Name })
            .ToListAsync();
        return Ok(categories);
    }
}
