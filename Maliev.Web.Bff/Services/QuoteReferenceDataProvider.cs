using Maliev.Web.Shared.Localization;
using Maliev.Web.Shared.Quotes;

namespace Maliev.Web.Bff.Services;

internal static class QuoteReferenceDataProvider
{
    internal static QuoteReferenceDataDto Get()
    {
        return new QuoteReferenceDataDto
        {
            Processes =
            [
                Process("FDM", "3D Printing - FDM", "พิมพ์สามมิติ FDM", "Cost-effective plastic prototypes and fixtures.", "ต้นแบบและฟิกซ์เจอร์พลาสติกราคาคุ้มค่า", true),
                Process("SLA", "3D Printing - Resin", "พิมพ์เรซิน", "High-detail resin parts for fit checks and presentation models.", "งานเรซินรายละเอียดสูงสำหรับตรวจประกอบและโมเดลนำเสนอ", true),
                Process("CNC_MILL", "CNC Machining", "CNC แมชชีนนิ่ง", "Precision machined plastic and metal parts.", "งานกัด CNC สำหรับพลาสติกและโลหะความแม่นยำสูง", true),
                Process("SCAN", "3D Scanning", "สแกนสามมิติ", "Reverse engineering and inspection capture.", "งานสแกนเพื่อรีเวิร์สเอนจิเนียริ่งและตรวจสอบชิ้นงาน", false),
                Process("DESIGN", "3D Design", "ออกแบบสามมิติ", "CAD modeling and design-for-manufacturing support.", "บริการขึ้นแบบ CAD และปรับแบบเพื่อการผลิต", false)
            ],
            Materials =
            [
                Material("PLA", "FDM", "PLA", "PLA"),
                Material("PETG", "FDM", "PETG", "PETG"),
                Material("ABS", "FDM", "ABS", "ABS"),
                Material("STD_RESIN", "SLA", "Standard resin", "เรซินมาตรฐาน"),
                Material("AL6061", "CNC_MILL", "Aluminum 6061", "อะลูมิเนียม 6061"),
                Material("POM", "CNC_MILL", "POM / Delrin", "POM / Delrin"),
                Material("SCAN_MARKERS", "SCAN", "Marker-based scan", "สแกนด้วยมาร์กเกอร์"),
                Material("CAD_HOURS", "DESIGN", "Design engineering hours", "ชั่วโมงออกแบบ")
            ],
            LeadTimeCodes = ["STANDARD", "RUSH", "ECONOMY"]
        };
    }

    private static ServiceProcessDto Process(string code, string en, string th, string summaryEn, string summaryTh, bool instant)
    {
        return new ServiceProcessDto
        {
            Code = code,
            Name = new LocalizedText { En = en, Th = th },
            Summary = new LocalizedText { En = summaryEn, Th = summaryTh },
            SupportsInstantQuote = instant
        };
    }

    private static MaterialOptionDto Material(string code, string process, string en, string th)
    {
        return new MaterialOptionDto
        {
            Code = code,
            ProcessCode = process,
            Name = new LocalizedText { En = en, Th = th }
        };
    }
}
