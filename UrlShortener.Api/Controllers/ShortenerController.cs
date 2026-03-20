using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri) ||
        (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
    {
        return BadRequest("Invalid URL");
    }

    var link = new Link
    {
        OriginalUrl = request.Url,
        CreatedAt = DateTime.UtcNow,
        ClickCount = 0
    };

    _db.Links.Add(link);
    await _db.SaveChangesAsync();

    // теперь у нас есть Id
    link.ShortCode = _shortCodeService.Encode(link.Id);

    await _db.SaveChangesAsync();

    var shortUrl = $"{Request.Scheme}://{Request.Host}/r/{link.ShortCode}";

    return Ok(new
    {
        shortUrl
    });
}

    [HttpGet("/r/{code}")]
    public async Task<IActionResult> RedirectToOriginal(string code)
    {
        var link = await _db.Links
            .FirstOrDefaultAsync(l => l.ShortCode == code);

        if (link == null)
        {
            return NotFound("Link not found");
        }

        link.ClickCount++;

        await _db.SaveChangesAsync();

        return Redirect(link.OriginalUrl);
    }
}