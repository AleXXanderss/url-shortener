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
    private readonly RedirectService _redirectService;

    public ShortenerController(
        AppDbContext db,
        ShortCodeService shortCodeService,
        RedirectService redirectService)
    {
        _db = db;
        _shortCodeService = shortCodeService;
        _redirectService = redirectService;
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

        link.ShortCode = _shortCodeService.Encode(link.Id);
        await _db.SaveChangesAsync();

        var shortUrl = $"{Request.Scheme}://{Request.Host}/r/{link.ShortCode}";

        return Ok(new { shortUrl });
    }

    [HttpGet("/r/{code}")]
    public async Task<IActionResult> RedirectToOriginal(string code)
    {
        var url = await _redirectService.GetOriginalUrlAsync(code);

        if (url == null)
            return NotFound("Link not found");

        await _redirectService.IncrementClickAsync(code);

        return Redirect(url);
    }
}