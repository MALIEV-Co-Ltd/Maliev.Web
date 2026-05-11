using Maliev.Web.Shared.Localization;

namespace Maliev.Web.Client.Content;

internal static class SiteContent
{
    internal const string QuoteEngineUrl = "https://quote.maliev.com";
    internal const string QuoteNewUrl = "https://quote.maliev.com/quotes/new";
    internal const string QuoteProfileUrl = "https://quote.maliev.com/profile";
    internal const string QuoteOrdersUrl = "https://quote.maliev.com/orders";

    internal static readonly IReadOnlyList<MetricItem> HeroMetrics =
    [
        new("48h", Text("typical first quote", "ใบเสนอราคาแรกโดยทั่วไป")),
        new("7", Text("manufacturing routes", "เส้นทางการผลิต")),
        new("±0.02mm", Text("inspection capable", "ความละเอียดการตรวจวัด"))
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
            "https://shop.maliev.com/cdn/shop/files/colors.61.png?v=1740108788&width=1200",
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
            "https://shop.maliev.com/cdn/shop/files/Screenshot_2025-10-03_223753.png?v=1759505883&width=1200",
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
            "https://shop.maliev.com/cdn/shop/files/spare-part.png?v=1756281837&width=1200",
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
            "https://shop.maliev.com/cdn/shop/files/colors.81.png?v=1740109006&width=1200",
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
            "https://shop.maliev.com/cdn/shop/files/colors.51.png?v=1740108657&width=1200",
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
            "https://shop.maliev.com/cdn/shop/files/machine-portrait.21.png?v=1737116109&width=1200",
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
            "https://shop.maliev.com/cdn/shop/files/110302574540_001.jpg?v=1759504756&width=1200",
            false,
            [Text("Scan comparison", "เทียบข้อมูลสแกน"), Text("Critical dimensions", "มิติสำคัญ"), Text("Report-ready findings", "ผลตรวจพร้อมรายงาน")])
    ];

    internal static readonly IReadOnlyList<CaseStudyContent> CaseStudies =
    [
        new("fixture-turnaround", Text("Fixture turnaround", "ฟิกซ์เจอร์เร่งด่วน"), "3D printing · CNC", Text("A maintenance team moved from worn sample to replacement fixture in one production week.", "ทีมซ่อมบำรุงเปลี่ยนจากตัวอย่างสึกหรอเป็นฟิกซ์เจอร์ทดแทนภายในหนึ่งสัปดาห์การผลิต"), "48h quote"),
        new("prototype-iteration", Text("Prototype iteration", "พัฒนาต้นแบบซ้ำ"), "CAD · resin · FDM", Text("A product team tested fit, finish, and assembly before locking a production route.", "ทีมสินค้าได้ทดสอบการประกอบ ผิวงาน และการใช้งานก่อนเลือกเส้นทางผลิตจริง"), "3 revisions"),
        new("scan-to-cad-repair", Text("Scan-to-CAD repair", "ซ่อมด้วยสแกนสู่ CAD"), "3D scanning · design", Text("A legacy component was captured, rebuilt, checked, and routed into replacement production.", "ชิ้นส่วนเดิมถูกสแกน ขึ้นแบบ ตรวจสอบ และนำไปผลิตทดแทน"), "±0.05mm check")
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

internal sealed record MetricItem(string Value, LocalizedText Label);

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

internal sealed record CaseStudyContent(string Slug, LocalizedText Title, string Meta, LocalizedText Summary, string Stat);
