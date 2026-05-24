using Maliev.Web.Client.Content;
using Maliev.Web.Shared.Localization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Maliev.Web.Bff.Services;

/// <summary>
/// Renders public practical notes as downloadable magazine-style PDF booklets.
/// </summary>
public sealed class BlogEbookPdfService
{
    private const int A4WordThreshold = 1_800;
    private const int A4SectionThreshold = 7;

    internal byte[] Generate(BlogPostContent post, string cultureName)
    {
        var normalizedCulture = SupportedCultures.Normalize(cultureName);
        var useA4 = ShouldUseA4(post, normalizedCulture);
        return new BlogEbookDocument(post, normalizedCulture, useA4).GeneratePdf();
    }

    private static bool ShouldUseA4(BlogPostContent post, string cultureName)
    {
        var wordCount = CountWords(post.Title.For(cultureName))
            + CountWords(post.Summary.For(cultureName))
            + post.Takeaways.Sum(takeaway => CountWords(takeaway.For(cultureName)))
            + post.Sections.Sum(section =>
                CountWords(section.Title.For(cultureName))
                + CountWords(section.Body.For(cultureName))
                + section.Items.Sum(item => CountWords(item.For(cultureName))));

        return wordCount >= A4WordThreshold || post.Sections.Count >= A4SectionThreshold;
    }

    private static int CountWords(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? 0
            : value.Split([' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries).Length;
    }

    private sealed class BlogEbookDocument(BlogPostContent post, string cultureName, bool useA4) : IDocument
    {
        private static readonly string AccentBlue = Colors.Blue.Darken2;
        private static readonly string BodyText = Colors.Grey.Darken3;
        private static readonly string DarkPanel = Colors.Grey.Darken4;
        private static readonly string Hairline = Colors.Grey.Lighten2;
        private static readonly string SoftPanel = Colors.Grey.Lighten5;

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

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
                page.Size(useA4 ? PageSizes.A4 : PageSizes.A5);
                page.Margin(0);
                page.DefaultTextStyle(TextStyle);

                page.Content()
                    .Background(DarkPanel)
                    .Padding(32)
                    .Column(column =>
                    {
                        column.Item().Text("MALIEV").FontSize(20).Bold().FontColor(Colors.White);
                        column.Item().PaddingTop(6).Text("Manufacturing Practical Note").FontSize(10).FontColor(Colors.Grey.Lighten2);

                        column.Item().ExtendVertical();

                        column.Item().Text(post.Category.For(cultureName).ToUpperInvariant()).FontSize(9).Bold().FontColor(AccentBlue);
                        column.Item().PaddingTop(8).Text(post.Title.For(cultureName)).FontSize(useA4 ? 34 : 28).Bold().FontColor(Colors.White);
                        column.Item().PaddingTop(12).Width(useA4 ? 430 : 300).Text(post.Summary.For(cultureName)).FontSize(12).LineHeight(1.35f).FontColor(Colors.Grey.Lighten2);

                        column.Item().PaddingTop(28).BorderTop(1).BorderColor(Colors.Grey.Darken2).PaddingTop(16).Row(row =>
                        {
                            row.RelativeItem().Column(company =>
                            {
                                company.Item().Text("Prepared by MALIEV Co., Ltd.").FontSize(9).Bold().FontColor(Colors.White);
                                company.Item().Text("36/1 Moo 3, Khlong Khoi, Pak Kret, Nonthaburi 11120, Thailand").FontSize(8).FontColor(Colors.Grey.Lighten2);
                                company.Item().Text("www.maliev.com | info@maliev.com").FontSize(8).FontColor(Colors.Grey.Lighten2);
                            });
                            row.ConstantItem(88).AlignRight().Text(useA4 ? "A4 reference edition" : "A5 booklet edition").FontSize(8).FontColor(Colors.Grey.Lighten2);
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
                    column.Item().Text("Read before quoting").FontSize(22).Bold().FontColor(DarkPanel);
                    column.Item().PaddingTop(8).Text(post.Summary.For(cultureName)).FontSize(10).LineHeight(1.35f).FontColor(BodyText);

                    column.Item().PaddingTop(18).Column(toc =>
                    {
                        for (var index = 0; index < post.Sections.Count; index++)
                        {
                            var section = post.Sections[index];
                            toc.Item().PaddingBottom(8).Row(row =>
                            {
                                row.ConstantItem(30).Element(item => NumberBadge(item, index + 1));
                                row.RelativeItem().PaddingLeft(8).BorderBottom(1).BorderColor(Hairline).PaddingBottom(8).Column(item =>
                                {
                                    item.Item().Text(section.Title.For(cultureName)).FontSize(11).Bold().FontColor(DarkPanel);
                                    item.Item().PaddingTop(2).Text(TrimToLength(section.Body.For(cultureName), 118)).FontSize(8).FontColor(Colors.Grey.Darken1);
                                });
                            });
                        }
                    });

                    column.Item().PaddingTop(16).Element(ComposeTakeawayPanel);
                });
                ComposeFooter(page, "Practical note");
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
                        column.Item().PaddingBottom(18).Element(item => ComposeArticleSection(item, section, index + 1));
                    }
                });
                ComposeFooter(page, post.Category.For(cultureName));
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
                    column.Item().Text("Manufacturing support from prototype to usable parts").FontSize(22).Bold().FontColor(DarkPanel);
                    column.Item().PaddingTop(10).Text("MALIEV helps engineering teams prepare manufacturable files, review material and process choices, quote production work, and keep each order traceable from upload through delivery.")
                        .FontSize(10)
                        .LineHeight(1.4f)
                        .FontColor(BodyText);

                    column.Item().PaddingTop(20).Row(row =>
                    {
                        row.RelativeItem().Element(item => ContactCard(item, "Get part price", SiteContent.QuoteNewUrl));
                        row.ConstantItem(12);
                        row.RelativeItem().Element(item => ContactCard(item, "Compare materials", "www.maliev.com/materials"));
                    });

                    column.Item().PaddingTop(18).Background(SoftPanel).Border(1).BorderColor(Hairline).Padding(14).Column(company =>
                    {
                        company.Item().Text("MALIEV Co., Ltd.").FontSize(13).Bold().FontColor(DarkPanel);
                        company.Item().PaddingTop(5).Text("36/1 Moo 3, Khlong Khoi, Pak Kret, Nonthaburi 11120, Thailand").FontSize(9).FontColor(BodyText);
                        company.Item().Text("www.maliev.com | info@maliev.com").FontSize(9).FontColor(BodyText);
                        company.Item().Text("Weekdays 10:00-18:00").FontSize(9).FontColor(BodyText);
                    });

                    column.Item().ExtendVertical();
                    column.Item().BorderTop(1).BorderColor(Hairline).PaddingTop(10).Text("This booklet is a practical guide, not a final manufacturing acceptance document. Include drawings, material requirements, operating conditions, and inspection criteria with your quote request.")
                        .FontSize(8)
                        .LineHeight(1.35f)
                        .FontColor(Colors.Grey.Darken1);
                });
                ComposeFooter(page, "MALIEV");
            });
        }

        private static TextStyle TextStyle(TextStyle style)
        {
            return style.FontFamily("Roboto", "Noto Sans Thai", "Segoe UI", "Arial").FontSize(10).FontColor(BodyText);
        }

        private void ConfigureStandardPage(PageDescriptor page)
        {
            page.Size(useA4 ? PageSizes.A4 : PageSizes.A5);
            page.Margin(useA4 ? 42 : 30);
            page.DefaultTextStyle(TextStyle);
        }

        private static void ComposeFooter(PageDescriptor page, string label)
        {
            page.Footer().BorderTop(1).BorderColor(Hairline).PaddingTop(8).Row(row =>
            {
                row.RelativeItem().Text(label).FontSize(7).FontColor(Colors.Grey.Darken1);
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
            container.Background(SoftPanel).Border(1).BorderColor(Hairline).Padding(12).Column(column =>
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
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.ConstantItem(34).Element(item => NumberBadge(item, number));
                    row.RelativeItem().PaddingLeft(10).Text(section.Title.For(cultureName)).FontSize(16).Bold().FontColor(DarkPanel);
                });

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

        private static void ContactCard(IContainer container, string title, string value)
        {
            container.Background(DarkPanel).Padding(12).Column(column =>
            {
                column.Item().Text(title).FontSize(8).FontColor(Colors.Grey.Lighten2);
                column.Item().PaddingTop(4).Text(value).FontSize(10).Bold().FontColor(Colors.White);
            });
        }

        private static string TrimToLength(string value, int maxLength)
        {
            if (value.Length <= maxLength)
            {
                return value;
            }

            return string.Concat(value.AsSpan(0, maxLength).TrimEnd(), "...");
        }
    }
}
