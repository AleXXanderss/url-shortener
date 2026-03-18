using Microsoft.AspNetCore.Mvc;
using UrlShortener.Api.Services;
using UrlShortener.Api.Data;
using UrlShortener.Api.Models;
using UrlShortener.Api.Dtos;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Route("api/shorten")]
public class ShortenerController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ShortCodeService _shortCodeService;

    public ShortenerController(AppDbContext db, ShortCodeService shortCodeService)
    {
        _db = db;
        _shortCodeService = shortCodeService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateLinkRequest request)
    {
        var code = _shortCodeService.Generate();

        var link = new Link
        {
            OriginalUrl = request.Url,
            ShortCode = code,
            CreatedAt = DateTime.UtcNow
        };

        _db.Links.Add(link);

        await _db.SaveChangesAsync();

        return Ok(new
        {
            shortCode = code
        });
    }
}