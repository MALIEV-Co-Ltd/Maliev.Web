using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.Web.Bff.Controllers;

/// <summary>
/// SEO metadata endpoints for public crawlers.
/// </summary>
[ApiController]
[AllowAnonymous]
public sealed class SeoController : ControllerBase
{
    private static readonly string[] PublicRoutes =
    [
        "/",
        "/services/3d-printing",
        "/services/cnc-machining",
        "/services/3d-scanning",
        "/services/3d-design",
        "/materials",
        "/industries",
        "/case-studies",
        "/blog",
        "/shop",
        "/contact",
        "/faq",
        "/shipping-returns",
        "/privacy",
        "/terms"
    ];

    /// <summary>
    /// Returns crawler rules for the public website.
    /// </summary>
    [HttpGet("/robots.txt")]
    public ContentResult Robots()
    {
        return Content("User-agent: *\nAllow: /\nSitemap: https://www.maliev.com/sitemap.xml\n", "text/plain");
    }

    /// <summary>
    /// Returns the public sitemap.
    /// </summary>
    [HttpGet("/sitemap.xml")]
    public ContentResult Sitemap()
    {
        var urls = PublicRoutes.Select(route => $"  <url><loc>https://www.maliev.com{route}</loc></url>");
        var xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">\n"
            + string.Join("\n", urls)
            + "\n</urlset>\n";
        return Content(xml, "application/xml");
    }
}
