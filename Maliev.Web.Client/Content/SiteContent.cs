using Maliev.Web.Shared.Localization;

namespace Maliev.Web.Client.Content;

internal static class SiteContent
{
    internal const string QuoteEngineUrl = "https://quote.maliev.com";
    internal const string QuoteDemoUrl = "https://quote.maliev.com/demo";
    internal const string QuoteNewUrl = "https://quote.maliev.com/projects/new";
    internal const string QuoteProfileUrl = "https://quote.maliev.com/profile";
    internal const string QuoteOrdersUrl = "https://quote.maliev.com/orders";
    private const string ThreeDimensionalPrinterImageUrl = "https://images.unsplash.com/photo-1756723902896-94e2d9332fbd?auto=format&fit=crop&w=1200&q=80";
    private const string ThreeDimensionalPrinterOperatorImageUrl = "https://images.unsplash.com/photo-1772566022519-e04921619df2?auto=format&fit=crop&w=1200&q=80";
    private const string MetalWorkshopImageUrl = "https://images.unsplash.com/photo-1764115424737-25aca6f47835?auto=format&fit=crop&w=1200&q=80";
    private const string PipeMachiningImageUrl = "https://images.unsplash.com/photo-1747999610489-de5c0ad00e56?auto=format&fit=crop&w=1200&q=80";
    private const string ThreeDimensionalScannerImageUrl = "https://images.unsplash.com/photo-1752056012968-5b094676afa9?auto=format&fit=crop&w=1200&q=80";
    private const string CaliperInspectionImageUrl = "https://images.unsplash.com/photo-1758873263563-5ba4aa330799?auto=format&fit=crop&w=1200&q=80";
    private const string DesignPlanningImageUrl = "https://images.unsplash.com/photo-1761864293839-95dcd7d2a6b3?auto=format&fit=crop&w=1200&q=80";
    private const string InjectionMoldingLineImageUrl = "https://images.unsplash.com/photo-1730705788367-dbd288c40ee7?auto=format&fit=crop&w=1200&q=80";
    private const string FactoryPipeProductionImageUrl = "https://images.unsplash.com/photo-1699799678681-3c156c3c5553?auto=format&fit=crop&w=1200&q=80";

    internal static readonly IReadOnlyList<MetricItem> HeroMetrics =
    [
        new(Text("2018", "2018"), Text("founded in Thailand", "เริ่มต้นในประเทศไทย")),
        new(Text("12,000+", "12,000+"), Text("parts produced", "ชิ้นงานที่ผลิตแล้ว")),
        new(Text("850+", "850+"), Text("businesses served", "ธุรกิจที่ให้บริการ"))
    ];

    internal static readonly IReadOnlyList<ServicePageContent> Services =
    [
        new(
            "3d-printing",
            "01",
            "3D Printing",
            Text("3D Printing", "งานพิมพ์ 3 มิติ"),
            Text("FDM, resin, nylon, and production-grade polymer parts for prototypes, fixtures, and low-volume manufacturing.", "งาน FDM เรซิน ไนลอน และโพลีเมอร์ระดับผลิตจริง สำหรับต้นแบบ ฟิกซ์เจอร์ และงานจำนวนน้อย"),
            Text("Fast printed parts without losing manufacturing discipline.", "ชิ้นงานพิมพ์รวดเร็ว พร้อมวินัยแบบงานผลิตจริง"),
            Text("Upload STL, STEP, OBJ, or 3MF files, choose material and quantity, then continue to the quote engine for DFM review.", "อัปโหลดไฟล์ STL, STEP, OBJ หรือ 3MF เลือกวัสดุและจำนวน แล้วไปต่อที่ระบบขอราคาเพื่อตรวจ DFM"),
            ThreeDimensionalPrinterImageUrl,
            true,
            [Text("FDM · SLA · SLS · MJF", "FDM · SLA · SLS · MJF"), Text("Lead time from 24 hours", "เริ่มผลิตได้ภายใน 24 ชั่วโมง"), Text("PLA · PETG · PA12 · resin", "PLA · PETG · PA12 · เรซิน")]),
        new(
            "cnc-machining",
            "02",
            "CNC Machining",
            Text("CNC Machining", "CNC แมชชีนนิ่ง"),
            Text("Plastic and metal machining for precision brackets, tooling, production aids, and end-use components.", "งานกัดพลาสติกและโลหะสำหรับขายึด ทูลลิ่ง อุปกรณ์ช่วยผลิต และชิ้นส่วนใช้งานจริง"),
            Text("Machined parts reviewed for tolerance, setup, and production fit.", "ชิ้นงานกัดที่ตรวจความคลาดเคลื่อน การจับงาน และความเหมาะสมก่อนผลิต"),
            Text("Send STEP files, drawings, quantity, material, surface, and tolerance notes for review.", "ส่งไฟล์ STEP แบบ Drawing จำนวน วัสดุ ผิวงาน และจุดควบคุมสำคัญเพื่อให้ทีมตรวจสอบ"),
            PipeMachiningImageUrl,
            false,
            [Text("3-axis and fixture work", "งาน 3 แกนและฟิกซ์เจอร์"), Text("Aluminum and engineering plastics", "อะลูมิเนียมและพลาสติกวิศวกรรม"), Text("Tolerance review before quote", "ตรวจ tolerance ก่อนเสนอราคา")]),
        new(
            "3d-scanning",
            "03",
            "3D Scanning",
            Text("3D Scanning", "สแกน 3 มิติ"),
            Text("Reverse engineering and inspection capture for legacy parts, handmade parts, and production comparison.", "สแกนเพื่อรีเวิร์สเอนจิเนียริ่งและตรวจสอบชิ้นงานเดิม ชิ้นงานทำมือ และงานเทียบการผลิต"),
            Text("Real parts become measurable references.", "เปลี่ยนชิ้นงานจริงเป็นข้อมูลอ้างอิงที่วัดผลได้"),
            Text("Book scanning with photos, target dimensions, required output format, and the reason for capture.", "จองงานสแกนพร้อมรูป ขนาดอ้างอิง รูปแบบไฟล์ที่ต้องการ และเหตุผลการเก็บข้อมูล"),
            ThreeDimensionalScannerImageUrl,
            false,
            [Text("Scan to CAD", "สแกนสู่ CAD"), Text("Inspection references", "ข้อมูลอ้างอิงตรวจสอบ"), Text("Repair and replacement parts", "งานซ่อมและชิ้นส่วนทดแทน")]),
        new(
            "3d-design",
            "04",
            "3D Design",
            Text("3D Design", "ออกแบบ 3 มิติ"),
            Text("CAD modeling, DFM support, enclosure design, mechanisms, and manufacturable product development.", "ขึ้นแบบ CAD ตรวจ DFM ออกแบบเคส กลไก และพัฒนาสินค้าให้ผลิตได้จริง"),
            Text("Ideas become manufacturable geometry.", "เปลี่ยนไอเดียให้เป็นไฟล์ผลิตได้จริง"),
            Text("Start from sketches, photos, broken samples, or rough dimensions when a finished CAD file does not exist yet.", "เริ่มจากสเก็ตช์ รูปถ่าย ตัวอย่างแตกหัก หรือขนาดคร่าวๆ เมื่อยังไม่มีไฟล์ CAD"),
            DesignPlanningImageUrl,
            false,
            [Text("CAD cleanup", "ปรับไฟล์ CAD"), Text("DFM design changes", "แก้แบบเพื่อผลิต"), Text("Prototype to production", "ต้นแบบสู่การผลิต")]),
        new(
            "silicone-casting",
            "05",
            "Silicone Casting",
            Text("Silicone Casting", "หล่อซิลิโคน"),
            Text("Short-run urethane and silicone-like parts using rapid molds for prototypes and pilot batches.", "งานยูรีเทนและชิ้นงานคล้ายซิลิโคนจำนวนน้อยด้วยแม่พิมพ์เร็ว สำหรับต้นแบบและล็อตทดลอง"),
            Text("Bridge the gap between one prototype and production tooling.", "เชื่อมช่องว่างระหว่างต้นแบบหนึ่งชิ้นกับแม่พิมพ์ผลิตจริง"),
            Text("Use casting when you need multiple similar parts before committing to hard tooling.", "เลือกงานหล่อเมื่อต้องการหลายชิ้นใกล้เคียงกัน ก่อนลงทุนแม่พิมพ์จริง"),
            InjectionMoldingLineImageUrl,
            false,
            [Text("10-200 parts", "10-200 ชิ้น"), Text("Soft and rigid materials", "วัสดุนิ่มและแข็ง"), Text("Rapid tooling path", "เส้นทางแม่พิมพ์เร็ว")]),
        new(
            "rapid-prototyping",
            "06",
            "Rapid Prototyping",
            Text("Rapid Prototyping", "สร้างต้นแบบรวดเร็ว"),
            Text("Combine printing, machining, scanning, design, and finishing to move from idea to usable prototype quickly.", "ผสมงานพิมพ์ กัด สแกน ออกแบบ และตกแต่ง เพื่อเปลี่ยนไอเดียเป็นต้นแบบใช้งานได้อย่างรวดเร็ว"),
            Text("One workshop path for iteration.", "เส้นทางเดียวในเวิร์กช็อปสำหรับการทดลองซ้ำ"),
            Text("Share the product goal, must-fit dimensions, test deadline, and target manufacturing route.", "แจ้งเป้าหมายสินค้า ขนาดสำคัญ Deadline ทดสอบ และเส้นทางผลิตที่คาดหวัง"),
            ThreeDimensionalPrinterOperatorImageUrl,
            false,
            [Text("Design support", "ช่วยออกแบบ"), Text("Multi-process builds", "งานหลายกระบวนการ"), Text("Iteration planning", "วางแผนทดลองซ้ำ")]),
        new(
            "deviation-analysis",
            "07",
            "Deviation Analysis",
            Text("Deviation Analysis", "วิเคราะห์ความคลาดเคลื่อน"),
            Text("Scan-to-CAD comparison and measurement reports for parts that need evidence before acceptance.", "ตรวจเทียบสแกนกับ CAD และรายงานวัดผลสำหรับชิ้นงานที่ต้องมีหลักฐานก่อนรับงาน"),
            Text("Know what changed before deciding what to remake.", "รู้ความต่างก่อนตัดสินใจผลิตใหม่"),
            Text("Use inspection when tolerance, wear, fit, or supplier comparison matters.", "ใช้การตรวจเมื่อต้องควบคุม tolerance การสึก การประกอบ หรือเทียบ Supplier"),
            CaliperInspectionImageUrl,
            false,
            [Text("Scan comparison", "เทียบข้อมูลสแกน"), Text("Critical dimensions", "มิติสำคัญ"), Text("Report-ready findings", "ผลตรวจพร้อมรายงาน")])
    ];

    internal static readonly IReadOnlyList<CaseStudyContent> CaseStudies =
    [
        new("fixture-turnaround", Text("Fixture turnaround", "ฟิกซ์เจอร์เร่งด่วน"), "3D printing · CNC", Text("A maintenance team moved from worn sample to replacement fixture in one production week.", "ทีมซ่อมบำรุงเปลี่ยนจากตัวอย่างสึกหรอเป็นฟิกซ์เจอร์ทดแทนภายในหนึ่งสัปดาห์การผลิต"), "DFM + order", MetalWorkshopImageUrl),
        new("prototype-iteration", Text("Prototype iteration", "พัฒนาต้นแบบซ้ำ"), "CAD · resin · FDM", Text("A product team tested fit, finish, and assembly before locking a production route.", "ทีมสินค้าได้ทดสอบการประกอบ ผิวงาน และการใช้งานก่อนเลือกเส้นทางผลิตจริง"), "3 revisions", ThreeDimensionalPrinterOperatorImageUrl),
        new("scan-to-cad-repair", Text("Scan-to-CAD repair", "ซ่อมด้วยสแกนสู่ CAD"), "3D scanning · design", Text("A legacy component was captured, rebuilt, checked, and routed into replacement production.", "ชิ้นส่วนเดิมถูกสแกน ขึ้นแบบ ตรวจสอบ และนำไปผลิตทดแทน"), "±0.05mm check", ThreeDimensionalScannerImageUrl)
    ];

    internal static readonly IReadOnlyList<BlogPostContent> BlogPosts =
    [
        new("design-for-manufacturing", Text("Design for manufacturability before upload", "เตรียมแบบให้พร้อมผลิตก่อนอัปโหลด"), Text("Wall thickness, holes, threads, tolerances, and drawing notes that help the quote engine price the part cleanly.", "ความหนาผนัง รู เกลียว tolerance และหมายเหตุ Drawing ที่ช่วยให้ระบบประเมินราคาชิ้นงานได้ชัดเจน"), Text("DFM guide", "คู่มือ DFM"), DesignPlanningImageUrl),
        new("choosing-3d-printing-materials", Text("Choosing 3D printing materials", "เลือกวัสดุพิมพ์ 3 มิติ"), Text("Match strength, heat resistance, finish, and lead time before you order the part.", "เลือกวัสดุจากความแข็งแรง การทนร้อน ผิวงาน และระยะเวลาก่อนสั่งผลิต"), Text("Materials", "วัสดุ"), ThreeDimensionalPrinterImageUrl),
        new("instant-part-pricing", Text("How instant part pricing works", "ระบบคำนวณราคาชิ้นงานทำงานอย่างไร"), Text("Upload CAD, review DFM feedback, adjust process and quantity, then continue to order when the price fits.", "อัปโหลด CAD ตรวจ DFM ปรับกระบวนการและจำนวน แล้วสั่งผลิตต่อเมื่อราคาเหมาะสม"), Text("Quote engine", "ระบบราคา"), FactoryPipeProductionImageUrl)
    ];

    internal static LocalizedText Text(string en, string th)
    {
        return new LocalizedText { En = en, Th = th };
    }

    internal static ServicePageContent GetService(string? slug)
    {
        return Services.FirstOrDefault(service => service.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase)) ?? Services[0];
    }
}

internal sealed record MetricItem(LocalizedText Value, LocalizedText Label);

internal sealed record ServicePageContent(
    string Slug,
    string Number,
    string SeoTitle,
    LocalizedText Title,
    LocalizedText Summary,
    LocalizedText ProofTitle,
    LocalizedText ProofText,
    string ImageUrl,
    bool Primary,
    IReadOnlyList<LocalizedText> Specs);

internal sealed record CaseStudyContent(string Slug, LocalizedText Title, string Meta, LocalizedText Summary, string Stat, string ImageUrl);

internal sealed record BlogPostContent(string Slug, LocalizedText Title, LocalizedText Summary, LocalizedText Category, string ImageUrl);
