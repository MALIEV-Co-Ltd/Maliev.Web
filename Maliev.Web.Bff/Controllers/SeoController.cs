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
    private static readonly SitemapRoute[] PublicRoutes =
    [
        new("/"),
        new("/services"),
        new("/services/3d-printing", "2026-05-18"),
        new("/services/cnc-machining", "2026-05-18"),
        new("/services/3d-scanning", "2026-05-18"),
        new("/services/3d-design", "2026-05-18"),
        new("/services/silicone-casting", "2026-05-18"),
        new("/services/rapid-prototyping", "2026-05-18"),
        new("/services/deviation-analysis", "2026-05-18"),
        new("/materials"),
        new("/industries"),
        new("/case-studies"),
        new("/case-studies/fixture-turnaround"),
        new("/case-studies/prototype-iteration"),
        new("/case-studies/scan-to-cad-repair"),
        new("/blog"),
        new("/blog/design-for-manufacturing"),
        new("/blog/choosing-3d-printing-materials"),
        new("/blog/instant-part-pricing"),
        new("/shop"),
        new("/contact"),
        new("/faq"),
        new("/shipping-returns"),
        new("/privacy"),
        new("/cookie-policy"),
        new("/refund-policy"),
        new("/warranty-policy"),
        new("/terms"),
        new("/quote")
    ];

    /// <summary>
    /// Returns crawler rules for the public website.
    /// </summary>
    [HttpGet("/robots.txt")]
    public ContentResult Robots()
    {
        return Content("User-agent: *\nAllow: /\nDisallow: /account\nDisallow: /cart\nDisallow: /checkout\nDisallow: /web/\nSitemap: https://www.maliev.com/sitemap.xml\n", "text/plain");
    }

    /// <summary>
    /// Returns the public sitemap.
    /// </summary>
    [HttpGet("/sitemap.xml")]
    public ContentResult Sitemap()
    {
        var urls = PublicRoutes.Select(route =>
            string.IsNullOrWhiteSpace(route.LastModified)
                ? $"  <url><loc>https://www.maliev.com{route.Path}</loc></url>"
                : $"  <url><loc>https://www.maliev.com{route.Path}</loc><lastmod>{route.LastModified}</lastmod></url>");

        var xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">\n"
            + string.Join("\n", urls)
            + "\n</urlset>\n";
        return Content(xml, "application/xml");
    }

    private sealed record SitemapRoute(string Path, string? LastModified = null);
}
