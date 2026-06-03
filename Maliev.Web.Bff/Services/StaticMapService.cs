using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Maliev.Web.Bff.Services;

/// <summary>
/// Renders static map images from OpenStreetMap tiles with a marker pin at the given coordinates.
/// </summary>
public sealed class StaticMapService(IHttpClientFactory httpClientFactory)
{
    private const string TileServer = "https://tile.openstreetmap.org";
    private const string UserAgent = "MALIEV-Web/1.0";
    private const int TileSize = 256;

    /// <summary>
    /// Renders a static map image centered on the given coordinates with a marker.
    /// </summary>
    /// <param name="lat">Latitude of the marker.</param>
    /// <param name="lng">Longitude of the marker.</param>
    /// <param name="zoom">Map zoom level (1–19).</param>
    /// <param name="width">Output image width in pixels.</param>
    /// <param name="height">Output image height in pixels.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>PNG image bytes.</returns>
    public async Task<byte[]> RenderAsync(
        double lat,
        double lng,
        int zoom,
        int width,
        int height,
        CancellationToken cancellationToken)
    {
        var (markerPx, markerPy) = LatLngToGlobalPixel(lat, lng, zoom);

        var imgLeft = markerPx - width / 2.0;
        var imgTop = markerPy - height / 2.0;

        var maxTileIdx = (int)Math.Pow(2, zoom) - 1;
        var tMinX = Math.Max(0, (int)Math.Floor(imgLeft / TileSize));
        var tMinY = Math.Max(0, (int)Math.Floor(imgTop / TileSize));
        var tMaxX = Math.Min(maxTileIdx, (int)Math.Floor((imgLeft + width - 1) / TileSize));
        var tMaxY = Math.Min(maxTileIdx, (int)Math.Floor((imgTop + height - 1) / TileSize));

        using var httpClient = httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
        httpClient.Timeout = TimeSpan.FromSeconds(10);

        var tiles = new Dictionary<(int X, int Y), Image>();
        try
        {
            for (var tx = tMinX; tx <= tMaxX; tx++)
            for (var ty = tMinY; ty <= tMaxY; ty++)
            {
                var url = $"{TileServer}/{zoom}/{tx}/{ty}.png";
                var tileBytes = await httpClient.GetByteArrayAsync(url, cancellationToken);
                tiles[(tx, ty)] = Image.Load(tileBytes);
            }

            var backgroundColor = new Rgba32(0xF2, 0xEF, 0xE9);
            using var result = new Image<Rgba32>(width, height, backgroundColor);

            foreach (var (pos, tile) in tiles)
            {
                var destX = (int)(pos.X * TileSize - imgLeft);
                var destY = (int)(pos.Y * TileSize - imgTop);
                result.Mutate(ctx => ctx.DrawImage(tile, new Point(destX, destY), 1f));
            }

            var markX = (int)(markerPx - imgLeft);
            var markY = (int)(markerPy - imgTop);

            DrawCircle(result, markX + 1, markY + 2, 7, new Rgba32(0, 0, 0, 60));
            DrawCircle(result, markX, markY, 7, new Rgba32(0xE5, 0x39, 0x35));
            DrawCircle(result, markX, markY, 4, new Rgba32(0xFF, 0xFF, 0xFF));

            using var ms = new MemoryStream();
            await result.SaveAsync(ms, new PngEncoder());
            return ms.ToArray();
        }
        finally
        {
            foreach (var tile in tiles.Values)
            {
                tile.Dispose();
            }
        }
    }

    private static void DrawCircle(Image<Rgba32> image, int centerX, int centerY, int radius, Rgba32 color)
    {
        var r2 = radius * radius;
        for (var dy = -radius; dy <= radius; dy++)
        {
            var y = centerY + dy;
            if (y < 0 || y >= image.Height)
            {
                continue;
            }

            for (var dx = -radius; dx <= radius; dx++)
            {
                if (dx * dx + dy * dy > r2)
                {
                    continue;
                }

                var x = centerX + dx;
                if (x < 0 || x >= image.Width)
                {
                    continue;
                }

                image[x, y] = color;
            }
        }
    }

    /// <summary>
    /// Renders a placeholder image shown when the actual map cannot be generated.
    /// </summary>
    /// <param name="width">Output image width in pixels.</param>
    /// <param name="height">Output image height in pixels.</param>
    /// <returns>PNG image bytes.</returns>
    public byte[] RenderFallback(int width, int height)
    {
        var bg = new Rgba32(0xEE, 0xEB, 0xE4);
        var border = new Rgba32(0xD4, 0xCF, 0xC5);
        var pinColor = new Rgba32(0xA0, 0x9A, 0x8F);

        using var image = new Image<Rgba32>(width, height, bg);

        for (var x = 0; x < width; x++)
        {
            image[x, 0] = border;
            image[x, height - 1] = border;
        }

        for (var y = 0; y < height; y++)
        {
            image[0, y] = border;
            image[width - 1, y] = border;
        }

        var cx = width / 2;
        var cy = height / 2;

        DrawCircle(image, cx, cy - 4, 7, pinColor);
        for (var dy = -3; dy <= 3; dy++)
        {
            var y = cy + 9 + dy;
            if (y >= 0 && y < height)
            {
                image[cx, y] = pinColor;
            }
        }

        using var ms = new MemoryStream();
        image.Save(ms, new PngEncoder());
        return ms.ToArray();
    }

    private static (double X, double Y) LatLngToGlobalPixel(double lat, double lng, int zoom)
    {
        var n = Math.Pow(2, zoom);
        var x = (lng + 180.0) / 360.0 * n * TileSize;
        var latRad = lat * Math.PI / 180.0;
        var y = (1.0 - Math.Log(Math.Tan(latRad) + 1.0 / Math.Cos(latRad)) / Math.PI) / 2.0 * n * TileSize;
        return (x, y);
    }
}
