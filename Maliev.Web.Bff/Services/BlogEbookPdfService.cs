using Maliev.Web.Client.Content;
using Maliev.Web.Shared.Localization;
using Microsoft.AspNetCore.Hosting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ZXing;
using ZXing.QrCode;
using ZXing.Rendering;

namespace Maliev.Web.Bff.Services;

/// <summary>
/// Renders public practical notes as downloadable magazine-style PDF booklets.
/// </summary>
public sealed class BlogEbookPdfService(IWebHostEnvironment environment)
{
    private readonly string _webRootPath = string.IsNullOrWhiteSpace(environment.WebRootPath)
        ? Path.Combine(environment.ContentRootPath, "wwwroot")
        : environment.WebRootPath;

    internal byte[] Generate(BlogPostContent post, string cultureName)
    {
        var normalizedCulture = SupportedCultures.Normalize(cultureName);
        return new BlogEbookDocument(post, normalizedCulture, _webRootPath).GeneratePdf();
    }

    private sealed class BlogEbookDocument(BlogPostContent post, string cultureName, string webRootPath) : IDocument
    {
        private const string PublicSiteBaseUrl = "https://www.maliev.com";
        private const string QuoteUrl = "https://quote.maliev.com/projects/new";
        private const string MaterialsUrl = "https://www.maliev.com/materials";

        private static readonly string AccentBlue = Colors.Blue.Darken2;
        private static readonly string BodyText = Colors.Grey.Darken3;
        private static readonly string DarkPanel = Colors.Grey.Darken4;
        private static readonly string Hairline = Colors.Grey.Lighten2;
        private static readonly string SoftPanel = Colors.Grey.Lighten5;
        private static readonly string WarmPanel = Colors.Grey.Lighten4;

        private string BlogPostUrl => $"{PublicSiteBaseUrl}/blog/{Uri.EscapeDataString(post.Slug)}";

        private string LogoPath => Path.Combine(webRootPath, "images", "logo.svg");

        public DocumentMetadata GetMetadata()
        {
            return new DocumentMetadata
            {
                Title = $"{post.Title.For(cultureName)} - MALIEV Practical Note",
                Author = "MALIEV Co., Ltd.",
                Subject = post.Summary.For(cultureName),
                Keywords = "MALIEV, manufacturing, practical note, quotation, DFM",
                Language = cultureName == SupportedCultures.ThaiCulture ? "th-TH" : "en-US"
            };
        }

        public DocumentSettings GetSettings()
        {
            return new DocumentSettings
            {
                PDFA_Conformance = PDFA_Conformance.PDFA_3A,
                PDFUA_Conformance = PDFUA_Conformance.PDFUA_1,
                ImageCompressionQuality = ImageCompressionQuality.High,
                ImageRasterDpi = 180
            };
        }

        public void Compose(IDocumentContainer container)
        {
            ComposeCover(container);
            ComposeTableOfContents(container);
            ComposeArticle(container);
            ComposeCompanyBackPage(container);
        }

        private void ComposeCover(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(38);
                page.DefaultTextStyle(TextStyle);

                page.Content()
                    .Background(Colors.White)
                    .Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Width(150).Svg(LogoPath).FitWidth();
                            row.ConstantItem(160).AlignRight().Text("Manufacturing Practical Note").FontSize(10).Bold().FontColor(AccentBlue);
                        });

                        column.Item().PaddingTop(30).Text(post.Category.For(cultureName).ToUpperInvariant()).FontSize(9).Bold().FontColor(AccentBlue);
                        column.Item().PaddingTop(8).SemanticHeader1().Text(post.Title.For(cultureName)).FontSize(32).Bold().FontColor(DarkPanel);
                        column.Item().PaddingTop(12).Text(post.Summary.For(cultureName)).FontSize(13).LineHeight(1.38f).FontColor(BodyText);

                        if (ResolveImagePath(post.ImageUrl) is { } coverImagePath)
                        {
                            column.Item().PaddingTop(22).Height(220).Background(WarmPanel).Image(coverImagePath).FitArea();
                        }

                        column.Item().PaddingTop(28).BorderTop(1).BorderColor(Hairline).PaddingTop(16).Row(row =>
                        {
                            row.RelativeItem().Column(company =>
                            {
                                company.Item().Text("Prepared by MALIEV Co., Ltd.").FontSize(9).Bold().FontColor(DarkPanel);
                                company.Item().Text("36/1 Moo 3, Khlong Khoi, Pak Kret, Nonthaburi 11120, Thailand").FontSize(8).FontColor(BodyText);
                                company.Item().Text("www.maliev.com | info@maliev.com").FontSize(8).FontColor(BodyText);
                                company.Item().PaddingTop(5).Text(text =>
                                {
                                    text.Span("Read online: ").FontSize(8).FontColor(Colors.Grey.Darken1);
                                    text.Hyperlink(BlogPostUrl, BlogPostUrl).FontSize(8).Underline().FontColor(AccentBlue);
                                });
                            });
                            row.ConstantItem(94).AlignRight().Hyperlink(BlogPostUrl).Element(item => ComposeQrCode(item, BlogPostUrl));
                        });
                    });
            });
        }

        private void ComposeTableOfContents(IDocumentContainer container)
        {
            container.Page(page =>
            {
                ConfigureStandardPage(page);
                page.Content().Column(column =>
                {
                    ComposePageEyebrow(column, "Contents");
                    column.Item().SemanticHeader1().Text("Read before quoting").FontSize(22).Bold().FontColor(DarkPanel);
                    column.Item().PaddingTop(8).Text(post.Summary.For(cultureName)).FontSize(10).LineHeight(1.35f).FontColor(BodyText);

                    column.Item().PaddingTop(18).SemanticTableOfContents().Column(toc =>
                    {
                        for (var index = 0; index < post.Sections.Count; index++)
                        {
                            var section = post.Sections[index];
                            var sectionId = SectionId(index + 1);
                            toc.Item().PaddingBottom(8).SemanticTableOfContentsItem().SemanticLink(section.Title.For(cultureName)).SectionLink(sectionId).Row(row =>
                            {
                                row.ConstantItem(30).Element(item => NumberBadge(item, index + 1));
                                row.RelativeItem().PaddingLeft(8).BorderBottom(1).BorderColor(Hairline).PaddingBottom(8).Text(section.Title.For(cultureName)).FontSize(11).Bold().FontColor(DarkPanel);
                                row.ConstantItem(34).AlignRight().Text(text =>
                                {
                                    text.Span("p. ").FontSize(8).FontColor(Colors.Grey.Darken1);
                                    text.BeginPageNumberOfSection(sectionId).FontSize(8).Bold().FontColor(DarkPanel);
                                });
                            });
                        }
                    });

                    column.Item().PaddingTop(16).Element(ComposeTakeawayPanel);
                });
                ComposeFooter(page);
            });
        }

        private void ComposeArticle(IDocumentContainer container)
        {
            container.Page(page =>
            {
                ConfigureStandardPage(page);
                page.Content().Column(column =>
                {
                    ComposePageEyebrow(column, "Article");

                    for (var index = 0; index < post.Sections.Count; index++)
                    {
                        var section = post.Sections[index];
                        column.Item().PaddingBottom(18).PreventPageBreak().Element(item => ComposeArticleSection(item, section, index + 1));
                    }
                });
                ComposeFooter(page);
            });
        }

        private void ComposeCompanyBackPage(IDocumentContainer container)
        {
            container.Page(page =>
            {
                ConfigureStandardPage(page);
                page.Content().Column(column =>
                {
                    ComposePageEyebrow(column, "About MALIEV");
                    column.Item().SemanticHeader1().Text("Manufacturing support from prototype to usable parts").FontSize(22).Bold().FontColor(DarkPanel);
                    column.Item().PaddingTop(10).Text("MALIEV helps engineering teams prepare manufacturable files, review material and process choices, quote production work, and keep each order traceable from upload through delivery.")
                        .FontSize(10)
                        .LineHeight(1.4f)
                        .FontColor(BodyText);

                    column.Item().PaddingTop(20).Row(row =>
                    {
                        row.RelativeItem().Element(item => ContactCard(item, "Get part price", QuoteUrl));
                        row.ConstantItem(12);
                        row.RelativeItem().Element(item => ContactCard(item, "Compare materials", MaterialsUrl));
                    });

                    column.Item().PaddingTop(18).Background(SoftPanel).Border(1).BorderColor(Hairline).Padding(14).Column(company =>
                    {
                        company.Item().Text("MALIEV Co., Ltd.").FontSize(13).Bold().FontColor(DarkPanel);
                        company.Item().PaddingTop(5).Text("36/1 Moo 3, Khlong Khoi, Pak Kret, Nonthaburi 11120, Thailand").FontSize(9).FontColor(BodyText);
                        company.Item().Text("www.maliev.com | info@maliev.com").FontSize(9).FontColor(BodyText);
                        company.Item().Text("Weekdays 10:00-18:00").FontSize(9).FontColor(BodyText);
                    });

                    column.Item().PaddingTop(24).BorderTop(1).BorderColor(Hairline).PaddingTop(10).Text("This booklet is a practical guide, not a final manufacturing acceptance document. Include drawings, material requirements, operating conditions, and inspection criteria with your quote request.")
                        .FontSize(8)
                        .LineHeight(1.35f)
                        .FontColor(Colors.Grey.Darken1);
                });
                ComposeFooter(page);
            });
        }

        private static TextStyle TextStyle(TextStyle style)
        {
            return style.FontFamily("Roboto", "Noto Sans Thai", "Segoe UI", "Arial").FontSize(10).FontColor(BodyText);
        }

        private void ConfigureStandardPage(PageDescriptor page)
        {
            page.Size(PageSizes.A4);
            page.Margin(42);
            page.DefaultTextStyle(TextStyle);
        }

        private static void ComposeFooter(PageDescriptor page)
        {
            page.Footer().BorderTop(1).BorderColor(Hairline).PaddingTop(8).Row(row =>
            {
                row.RelativeItem().Text("MALIEV").FontSize(7).Bold().FontColor(Colors.Grey.Darken1);
                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("Page ").FontSize(7).FontColor(Colors.Grey.Darken1);
                    text.CurrentPageNumber().FontSize(7).FontColor(Colors.Grey.Darken1);
                    text.Span(" of ").FontSize(7).FontColor(Colors.Grey.Darken1);
                    text.TotalPages().FontSize(7).FontColor(Colors.Grey.Darken1);
                });
            });
        }

        private static void ComposePageEyebrow(ColumnDescriptor column, string label)
        {
            column.Item().Text(label.ToUpperInvariant()).FontSize(8).Bold().FontColor(AccentBlue);
            column.Item().PaddingTop(4).PaddingBottom(14).LineHorizontal(1).LineColor(Hairline);
        }

        private void ComposeTakeawayPanel(IContainer container)
        {
            container.PreventPageBreak().Background(SoftPanel).Border(1).BorderColor(Hairline).Padding(12).Column(column =>
            {
                column.Item().Text("Before you upload").FontSize(13).Bold().FontColor(DarkPanel);
                column.Item().PaddingTop(8).Column(points =>
                {
                    foreach (var takeaway in post.Takeaways)
                    {
                        points.Item().PaddingBottom(6).Row(row =>
                        {
                            row.ConstantItem(10).Text("•").FontColor(AccentBlue).Bold();
                            row.RelativeItem().Text(takeaway.For(cultureName)).FontSize(9).LineHeight(1.25f).FontColor(BodyText);
                        });
                    }
                });
            });
        }

        private void ComposeArticleSection(IContainer container, ArticleSectionContent section, int number)
        {
            var sectionId = SectionId(number);
            container.SemanticSection().Section(sectionId).Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.ConstantItem(34).Element(item => NumberBadge(item, number));
                    row.RelativeItem().PaddingLeft(10).SemanticHeader2().Text(section.Title.For(cultureName)).FontSize(16).Bold().FontColor(DarkPanel);
                });

                if (section.Image is { } image)
                {
                    column.Item().PaddingTop(10).Element(item => ComposeArticleImage(item, image));
                }

                column.Item().PaddingTop(8).Text(section.Body.For(cultureName)).FontSize(10).LineHeight(1.38f).FontColor(BodyText);

                if (section.Items.Count > 0)
                {
                    column.Item().PaddingTop(10).Column(points =>
                    {
                        foreach (var point in section.Items)
                        {
                            points.Item().PaddingBottom(6).Background(SoftPanel).BorderLeft(3).BorderColor(AccentBlue).Padding(8).Text(point.For(cultureName))
                                .FontSize(9)
                                .LineHeight(1.25f)
                                .FontColor(BodyText);
                        }
                    });
                }
            });
        }

        private void ComposeArticleImage(IContainer container, ArticleImageContent image)
        {
            var imagePath = ResolveImagePath(image.Url);
            if (imagePath is null)
            {
                return;
            }

            container.SemanticImage(image.Alt.For(cultureName)).Column(column =>
            {
                column.Item().Height(150).Background(WarmPanel).Image(imagePath).FitArea();
                column.Item().PaddingTop(5).SemanticCaption().Text(image.Caption.For(cultureName)).FontSize(8).FontColor(Colors.Grey.Darken1);
            });
        }

        private static void NumberBadge(IContainer container, int number)
        {
            container
                .Background(AccentBlue)
                .PaddingVertical(5)
                .AlignCenter()
                .Text(number.ToString("00"))
                .FontSize(8)
                .Bold()
                .FontColor(Colors.White);
        }

        private static void ContactCard(IContainer container, string title, string url)
        {
            container.Hyperlink(url).Background(SoftPanel).Border(1).BorderColor(Hairline).Padding(12).Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text(title).FontSize(9).Bold().FontColor(DarkPanel);
                    column.Item().PaddingTop(5).Text(text => text.Hyperlink(url, url).FontSize(8).Underline().FontColor(AccentBlue));
                });
                row.ConstantItem(58).Element(item => ComposeQrCode(item, url));
            });
        }

        private string? ResolveImagePath(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            var normalized = url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var imagePath = Path.Combine(webRootPath, normalized);
            return File.Exists(imagePath) ? imagePath : null;
        }

        private static void ComposeQrCode(IContainer container)
        {
            ComposeQrCode(container, "https://www.maliev.com");
        }

        private static void ComposeQrCode(IContainer container, string url)
        {
            container.Background(Colors.White).Border(1).BorderColor(Hairline).Padding(4).AspectRatio(1).Svg(size =>
            {
                var writer = new QRCodeWriter();
                var width = Math.Max(96, (int)Math.Ceiling(size.Width));
                var height = Math.Max(96, (int)Math.Ceiling(size.Height));
                var qrCode = writer.encode(url, BarcodeFormat.QR_CODE, width, height);
                var renderer = new SvgRenderer { FontName = "Arial" };
                return renderer.Render(qrCode, BarcodeFormat.QR_CODE, null).Content;
            });
        }

        private static string SectionId(int number)
        {
            return $"section-{number:00}";
        }
    }
}
