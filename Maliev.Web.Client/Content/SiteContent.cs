using Maliev.Web.Shared.Localization;

namespace Maliev.Web.Client.Content;

internal static class SiteContent
{
    private const string DefaultQuoteEngineUrl = "https://quote.maliev.com";

    internal static string QuoteEngineUrl => ResolveQuoteEngineUrl();

    internal static string QuoteDemoUrl => $"{QuoteEngineUrl}/demo";

    internal static string QuoteNewUrl => $"{QuoteEngineUrl}/projects/new";

    internal static string QuoteProfileUrl => $"{QuoteEngineUrl}/profile";

    internal static string QuoteOrdersUrl => $"{QuoteEngineUrl}/orders";
    internal const string FdmThermoplasticsImageUrl = "https://images.unsplash.com/photo-1742971239045-afabc9f7d744?auto=format&fit=crop&w=900&q=80";
    internal const string PowderBedNylonImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/d/d4/SLS_3D_Systems_Printed_Duraform_HST_Pulley_Shaft_%2849014691207%29.jpg/500px-SLS_3D_Systems_Printed_Duraform_HST_Pulley_Shaft_%2849014691207%29.jpg";
    internal const string SlaResinImageUrl = "https://images.pexels.com/photos/12268465/pexels-photo-12268465.jpeg?auto=compress&cs=tinysrgb&w=900";
    internal const string EngineeringPolymerReviewImageUrl = "https://images.unsplash.com/photo-1756723902896-94e2d9332fbd?auto=format&fit=crop&w=900&q=80";
    private const string ThreeDimensionalPrinterImageUrl = "https://images.unsplash.com/photo-1756723902896-94e2d9332fbd?auto=format&fit=crop&w=1200&q=80";
    private const string ThreeDimensionalPrinterOperatorImageUrl = "https://images.unsplash.com/photo-1772566022519-e04921619df2?auto=format&fit=crop&w=1200&q=80";
    private const string MetalWorkshopImageUrl = "https://images.unsplash.com/photo-1764115424737-25aca6f47835?auto=format&fit=crop&w=1200&q=80";
    private const string PipeMachiningImageUrl = "https://images.unsplash.com/photo-1747999610489-de5c0ad00e56?auto=format&fit=crop&w=1200&q=80";
    private const string ThreeDimensionalScannerImageUrl = "https://images.unsplash.com/photo-1752056012968-5b094676afa9?auto=format&fit=crop&w=1200&q=80";
    internal const string CaliperInspectionImageUrl = "https://images.unsplash.com/photo-1758873263563-5ba4aa330799?auto=format&fit=crop&w=1200&q=80";
    internal const string DesignPlanningImageUrl = "https://images.unsplash.com/photo-1761864293839-95dcd7d2a6b3?auto=format&fit=crop&w=1200&q=80";
    internal const string InjectionMoldingLineImageUrl = "https://images.unsplash.com/photo-1730705788367-dbd288c40ee7?auto=format&fit=crop&w=1200&q=80";
    private const string FactoryPipeProductionImageUrl = "https://images.unsplash.com/photo-1699799678681-3c156c3c5553?auto=format&fit=crop&w=1200&q=80";

    internal static string ResolveBlogImageUrl(BlogPostContent post)
    {
        return BlogImageUrl(post.Slug);
    }

    private static string BlogImageUrl(string slug)
    {
        return $"/images/blog/{slug}.jpg";
    }

    internal static readonly IReadOnlyList<MetricItem> HeroMetrics =
    [
        new(Text("2018", "2018"), Text("founded in Thailand", "เริ่มต้นในประเทศไทย"), 2018),
        new(Text("12,000+", "12,000+"), Text("parts produced", "ชิ้นงานที่ผลิตแล้ว"), 12000, "+"),
        new(Text("850+", "850+"), Text("businesses served", "ธุรกิจที่ให้บริการ"), 850, "+")
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
        new(
            "fixture-turnaround",
            Text("Fixture turnaround", "ฟิกซ์เจอร์เร่งด่วน"),
            "3D printing · CNC",
            Text("A maintenance team moved from worn sample to replacement fixture in one production week.", "ทีมซ่อมบำรุงเปลี่ยนจากตัวอย่างสึกหรอเป็นฟิกซ์เจอร์ทดแทนภายในหนึ่งสัปดาห์การผลิต"),
            "DFM + order",
            MetalWorkshopImageUrl,
            [
                Section(
                    "The worn sample was not enough by itself",
                    "ตัวอย่างสึกหรออย่างเดียวไม่พอ",
                    "The team had a physical fixture, but the critical dimensions were hidden by wear and hand adjustments. MALIEV asked for photos of the machine setup, the mating part, a few checked dimensions, and the quantity needed for the next maintenance window.",
                    "ทีมมีฟิกซ์เจอร์จริง แต่ขนาดสำคัญถูกซ่อนด้วยการสึกและการแต่งมือ MALIEV จึงขอรูปการติดตั้งบนเครื่อง ชิ้นส่วนที่ประกบ ขนาดที่ตรวจวัดได้ และจำนวนที่ต้องใช้ในรอบซ่อมบำรุงถัดไป",
                    ("The sample was treated as evidence, not as a perfect master.", "ใช้ตัวอย่างเป็นหลักฐาน ไม่ใช่ Master ที่สมบูรณ์"),
                    ("Fit surfaces, clamp clearance, and operator handling were marked before quoting.", "ระบุผิวประกบ ระยะหลบ Clamp และการหยิบจับของ Operator ก่อนเสนอราคา")),
                Section(
                    "DFM split the part into fast and controlled features",
                    "DFM แยกส่วนที่ทำเร็วกับส่วนที่ต้องคุม",
                    "The replacement did not need every surface to be high precision. The quote separated functional faces, clearance slots, and cosmetic surfaces so the team could hold the important dimensions without over-machining the whole part.",
                    "ชิ้นส่วนทดแทนไม่จำเป็นต้องแม่นทุกผิว ใบเสนอราคาแยกผิวใช้งานจริง ร่องหลบ และผิวที่ไม่สำคัญ เพื่อคุมขนาดสำคัญโดยไม่ต้องกัดละเอียดทั้งชิ้น",
                    ("Printed previews confirmed reach, envelope, and fixture orientation.", "ต้นแบบพิมพ์ช่วยยืนยันระยะเอื้อม Envelope และทิศทางฟิกซ์เจอร์"),
                    ("CNC finishing was reserved for bearing and locating faces.", "เก็บงาน CNC เฉพาะผิวรับแรงและผิวกำหนดตำแหน่ง")),
                Section(
                    "The order stayed traceable",
                    "คำสั่งผลิตติดตามได้",
                    "The team could review the accepted route, quantity, and production status from the same customer workspace instead of coordinating the drawing, quote, and delivery through separate messages.",
                    "ทีมสามารถดูเส้นทางผลิตที่อนุมัติ จำนวน และสถานะผลิตจากพื้นที่ลูกค้าเดียวกัน โดยไม่ต้องแยกคุย Drawing ใบเสนอราคา และการส่งมอบหลายช่องทาง"),
                Section(
                    "What made the turnaround work",
                    "ปัจจัยที่ทำให้ส่งงานได้เร็ว",
                    "The fastest replacement work came from a narrow scope: confirm the fit, make the missing geometry manufacturable, and avoid adding tolerance where the fixture did not need it.",
                    "งานทดแทนที่เร็วที่สุดเกิดจากขอบเขตที่ชัด: ยืนยันการประกอบ ทำ Geometry ที่หายไปให้ผลิตได้ และไม่ใส่ tolerance ในตำแหน่งที่ฟิกซ์เจอร์ไม่ต้องการ")
            ],
            [
                Text("Functional faces were called out before production.", "ระบุผิวใช้งานจริงก่อนผลิต"),
                Text("Fast prototype checks reduced remake risk.", "ตรวจต้นแบบเร็วเพื่อลดความเสี่ยงงานแก้"),
                Text("Quote, DFM notes, and order status stayed in one workspace.", "ใบเสนอราคา หมายเหตุ DFM และสถานะงานอยู่ในพื้นที่เดียว")
            ]),
        new(
            "prototype-iteration",
            Text("Prototype iteration", "พัฒนาต้นแบบซ้ำ"),
            "CAD · resin · FDM",
            Text("A product team tested fit, finish, and assembly before locking a production route.", "ทีมสินค้าได้ทดสอบการประกอบ ผิวงาน และการใช้งานก่อนเลือกเส้นทางผลิตจริง"),
            "3 revisions",
            ThreeDimensionalPrinterOperatorImageUrl,
            [
                Section(
                    "Each revision had one decision to answer",
                    "แต่ละรอบแก้แบบต้องตอบคำถามเดียว",
                    "The first upload tried to answer too many questions at once: appearance, latch feel, screw fit, and wall thickness. MALIEV split the work into short prototype rounds so each revision had a clear pass or fail condition.",
                    "ไฟล์แรกพยายามตอบหลายคำถามพร้อมกัน ทั้งหน้าตา ความรู้สึกของสลัก รูสกรู และความหนาผนัง MALIEV จึงแบ่งเป็นรอบต้นแบบสั้น ๆ ให้แต่ละรอบมีเงื่อนไขผ่านหรือไม่ผ่านชัดเจน",
                    ("Round one checked envelope and assembly sequence.", "รอบแรกตรวจ Envelope และลำดับการประกอบ"),
                    ("Round two checked latch feel and local reinforcement.", "รอบสองตรวจความรู้สึกของสลักและการเสริมแรงเฉพาะจุด"),
                    ("Round three checked finish direction and production assumptions.", "รอบสามตรวจทิศทางผิวและสมมติฐานเส้นทางผลิต")),
                Section(
                    "Material choice followed the test",
                    "เลือกวัสดุตามสิ่งที่ต้องทดสอบ",
                    "Early fit parts used a faster printed route. Cosmetic surfaces moved to resin when detail mattered. Functional stress points stayed on stronger thermoplastics until the team knew whether the geometry worked.",
                    "ชิ้นงานลองประกอบใช้กระบวนการพิมพ์ที่เร็วกว่า ผิวโชว์ย้ายไปใช้เรซินเมื่อรายละเอียดสำคัญ จุดรับแรงยังใช้เทอร์โมพลาสติกที่แข็งแรงกว่า จนกว่าทีมจะรู้ว่า Geometry ใช้งานได้หรือไม่",
                    ("The quote recorded why each material was used.", "ใบเสนอราคาบันทึกเหตุผลการเลือกวัสดุแต่ละรอบ"),
                    ("The team avoided paying for final finish before the fit was proven.", "ทีมไม่ต้องจ่ายค่าผิวสุดท้ายก่อนพิสูจน์การประกอบ")),
                Section(
                    "DFM notes became the revision brief",
                    "หมายเหตุ DFM กลายเป็นโจทย์แก้แบบ",
                    "Instead of sending a loose request like 'make it stronger', the team used DFM comments to decide which ribs, holes, offsets, and screw bosses needed CAD changes before the next upload.",
                    "แทนที่จะส่งคำขอกว้าง ๆ เช่น 'ทำให้แข็งแรงขึ้น' ทีมใช้หมายเหตุ DFM ตัดสินใจว่า Rib รู Offset และ Boss สกรูตำแหน่งใดต้องแก้ CAD ก่อนอัปโหลดรอบถัดไป"),
                Section(
                    "The result was a production-ready direction, not just samples",
                    "ผลลัพธ์คือทิศทางผลิต ไม่ใช่แค่ตัวอย่าง",
                    "By the final prototype, the team knew which geometry had been proven, which finish was acceptable, and which dimensions still needed drawing control before ordering a small batch.",
                    "เมื่อถึงต้นแบบรอบสุดท้าย ทีมรู้ว่า Geometry ใดพิสูจน์แล้ว ผิวแบบใดยอมรับได้ และขนาดใดยังต้องควบคุมด้วย Drawing ก่อนสั่งล็อตเล็ก")
            ],
            [
                Text("Prototype rounds were tied to specific decisions.", "แต่ละรอบต้นแบบผูกกับการตัดสินใจเฉพาะ"),
                Text("Material and finish changed only when the test needed it.", "เปลี่ยนวัสดุและผิวเมื่อการทดสอบต้องการเท่านั้น"),
                Text("DFM comments became the CAD revision list.", "หมายเหตุ DFM กลายเป็นรายการแก้ CAD")
            ]),
        new(
            "scan-to-cad-repair",
            Text("Scan-to-CAD repair", "ซ่อมด้วยสแกนสู่ CAD"),
            "3D scanning · design",
            Text("A legacy component was captured, rebuilt, checked, and routed into replacement production.", "ชิ้นส่วนเดิมถูกสแกน ขึ้นแบบ ตรวจสอบ และนำไปผลิตทดแทน"),
            "±0.05mm check",
            ThreeDimensionalScannerImageUrl,
            [
                Section(
                    "Scanning captured the old part before CAD cleanup",
                    "สแกนชิ้นส่วนเดิมก่อนเก็บ CAD",
                    "The original part had no usable drawing and had worn mounting edges. MALIEV scanned the sample to capture the shape, then separated intentional geometry from damage, chips, and measurement noise.",
                    "ชิ้นส่วนเดิมไม่มี Drawing ที่ใช้ได้และขอบยึดสึก MALIEV สแกนตัวอย่างเพื่อเก็บรูปทรง แล้วแยก Geometry ที่ตั้งใจออกจากรอยเสียหาย รอยบิ่น และสัญญาณรบกวนจากการวัด",
                    ("The scan was aligned to the most reliable fit faces.", "จัดแนวข้อมูลสแกนจากผิวประกบที่น่าเชื่อถือที่สุด"),
                    ("Damaged edges were rebuilt from symmetry and mating dimensions.", "ขอบเสียหายถูกสร้างใหม่จากสมมาตรและขนาดประกบ")),
                Section(
                    "Reverse engineering made the file manufacturable",
                    "รีเวิร์สเอนจิเนียริ่งทำให้ไฟล์ผลิตได้",
                    "A raw mesh is useful for inspection, but it is rarely the right production file. The model was rebuilt into cleaner CAD with controlled holes, datum faces, and manufacturable radii before quoting.",
                    "Mesh ดิบมีประโยชน์สำหรับตรวจสอบ แต่ไม่ใช่ไฟล์ผลิตที่เหมาะเสมอไป โมเดลจึงถูกสร้างเป็น CAD ที่สะอาดขึ้น มีรูที่ควบคุมได้ ผิว Datum และรัศมีที่ผลิตได้ก่อนเสนอราคา",
                    ("STEP was used for manufacturing review.", "ใช้ STEP สำหรับตรวจเส้นทางผลิต"),
                    ("Mesh data remained as traceability evidence.", "เก็บข้อมูล Mesh เป็นหลักฐานย้อนกลับ")),
                Section(
                    "Deviation checks reduced replacement risk",
                    "ตรวจความคลาดเคลื่อนเพื่อลดความเสี่ยงงานทดแทน",
                    "The rebuilt file was compared against scan data and the mating constraints. Only the dimensions that affected fit were treated as acceptance points, which kept the work practical for replacement production.",
                    "ไฟล์ที่สร้างใหม่ถูกเทียบกับข้อมูลสแกนและเงื่อนไขประกบ เฉพาะขนาดที่มีผลต่อการใช้งานถูกใช้เป็นจุดรับงาน ทำให้งานทดแทนยังผลิตได้จริง"),
                Section(
                    "The repair path stayed open",
                    "เส้นทางซ่อมยังต่อยอดได้",
                    "Once the replacement CAD existed, the customer could reorder, adjust material, request small changes, or route the same geometry into another process without starting from the damaged part again.",
                    "เมื่อมี CAD ทดแทนแล้ว ลูกค้าสามารถสั่งซ้ำ ปรับวัสดุ ขอแก้เล็กน้อย หรือใช้ Geometry เดิมกับกระบวนการอื่นได้โดยไม่ต้องเริ่มจากชิ้นส่วนเสียอีกครั้ง")
            ],
            [
                Text("The raw scan and clean CAD served different purposes.", "ข้อมูลสแกนดิบและ CAD สะอาดใช้คนละหน้าที่"),
                Text("Damage was separated from intended geometry.", "แยกรอยเสียหายออกจาก Geometry ที่ต้องการ"),
                Text("Replacement production can be reordered from the rebuilt file.", "สั่งผลิตทดแทนซ้ำได้จากไฟล์ที่สร้างใหม่")
            ])
    ];

    internal static readonly IReadOnlyList<BlogPostContent> BlogPosts =
    [
        new(
            "design-for-manufacturing",
            Text("Design for manufacturability before upload", "เตรียมแบบให้พร้อมผลิตก่อนอัปโหลด"),
            Text("Wall thickness, holes, threads, tolerances, and drawing notes that help the quote engine price the part cleanly.", "ความหนาผนัง รู เกลียว tolerance และหมายเหตุ Drawing ที่ช่วยให้ระบบประเมินราคาชิ้นงานได้ชัดเจน"),
            Text("DFM guide", "คู่มือ DFM"),
            DesignPlanningImageUrl,
            [
                Section(
                    "Wall thickness is the first DFM signal",
                    "ความหนาผนังคือสัญญาณ DFM แรก",
                    "Before upload, check whether the part has thin walls, isolated towers, deep pockets, or thick masses. Thin walls can crack, warp, or fail during finishing. Overly thick areas can increase print time, machining time, material use, and shrink risk. If a flexible wall is intentional, mark it in the drawing or note field so it is reviewed as a design feature rather than a mistake.",
                    "ก่อนอัปโหลด ควรตรวจว่าชิ้นงานมีผนังบาง เสาสูงเดี่ยว ร่องลึก หรือมวลหนามากหรือไม่ ผนังบางอาจแตก บิด หรือเสียหายตอนเก็บผิว ส่วนพื้นที่หนาเกินไปอาจเพิ่มเวลาพิมพ์ เวลากัด วัสดุ และความเสี่ยงหดตัว หากต้องการผนังยืดหยุ่นโดยตั้งใจ ให้ระบุใน Drawing หรือช่องหมายเหตุ เพื่อให้ตรวจเป็นคุณสมบัติของแบบ ไม่ใช่ข้อผิดพลาด",
                    ("Use ribs and fillets to support tall or load-bearing walls.", "ใช้ Rib และ Fillet ช่วยผนังสูงหรือผนังรับแรง"),
                    ("Avoid sudden thickness changes unless the transition is part of the function.", "หลีกเลี่ยงการเปลี่ยนความหนาฉับพลัน เว้นแต่เป็นฟังก์ชันของชิ้นงาน")),
                Section(
                    "Holes, threads, and inserts need manufacturing intent",
                    "รู เกลียว และ Insert ต้องมีเจตนาผลิตชัดเจน",
                    "A CAD hole is not always a finished hole. Printed holes may need clearance, reaming, or heat-set inserts. CNC tapped holes need tool access, thread depth, and a practical bottom condition. If the part uses screws, pins, bearings, or press-fit hardware, include the hardware size and whether the hole is clearance, tapped, or post-machined.",
                    "รูใน CAD ไม่ได้หมายความว่าเป็นรูพร้อมใช้งานเสมอไป รูพิมพ์อาจต้องเผื่อระยะ คว้าน หรือใส่ Insert แบบฝังร้อน รูต๊าป CNC ต้องมีทางเข้าเครื่องมือ ความลึกเกลียว และปลายรูที่ผลิตได้ หากชิ้นงานใช้สกรู พิน แบริ่ง หรือ Hardware แบบ Press-fit ให้ระบุขนาดและหน้าที่ของรูว่าเป็นรูหลวม รูต๊าป หรือรูเก็บงานหลังผลิต",
                    ("Call out thread standard, depth, and whether inserts are acceptable.", "ระบุมาตรฐานเกลียว ความลึก และยอมรับ Insert ได้หรือไม่"),
                    ("Separate cosmetic holes from controlled fit holes.", "แยกรูโชว์ออกจากรูที่ต้องควบคุมการประกอบ")),
                Section(
                    "Tolerances should be paid for only where they matter",
                    "ควรจ่ายค่า tolerance เฉพาะจุดสำคัญ",
                    "Tight tolerance everywhere makes custom parts expensive and sometimes impossible. Mark datums, mating faces, critical hole patterns, and inspection dimensions. Leave non-functional surfaces at process default. When tolerances come from assembly fit, send the mating part or the nominal interface dimensions so the review can check the real stack-up.",
                    "การกำหนด tolerance แคบทุกตำแหน่งทำให้งานเฉพาะแพงและบางครั้งผลิตไม่ได้ ควรระบุ Datum ผิวประกบ Pattern รูสำคัญ และขนาดที่ต้องตรวจ ปล่อยผิวที่ไม่ใช้งานให้เป็นค่ามาตรฐานของกระบวนการ หาก tolerance มาจากการประกอบ ให้ส่งชิ้นส่วนที่ประกบหรือขนาด Interface เพื่อให้ตรวจ Stack-up จริง",
                    ("Use drawings for dimensions that must be inspected.", "ใช้ Drawing สำหรับขนาดที่ต้องตรวจรับ"),
                    ("Mention the failure mode: too loose, too tight, leaks, rubs, or misaligns.", "ระบุอาการเสียที่ต้องป้องกัน เช่น หลวม แน่น รั่ว ขูด หรือเยื้องศูนย์")),
                Section(
                    "Upload the files that answer production questions",
                    "อัปโหลดไฟล์ที่ตอบคำถามการผลิต",
                    "STEP files help with machining, controlled faces, and feature review. STL or 3MF files are useful for printed geometry. Drawings explain tolerances, threads, finishing, materials, and acceptance notes. Photos of the mating area or the broken part help the team understand why a feature matters.",
                    "ไฟล์ STEP ช่วยงานกัด ผิวควบคุม และการตรวจ Feature ไฟล์ STL หรือ 3MF เหมาะกับ Geometry สำหรับพิมพ์ Drawing อธิบาย tolerance เกลียว ผิว วัสดุ และหมายเหตุรับงาน รูปพื้นที่ประกบหรือชิ้นส่วนเสียช่วยให้ทีมเข้าใจว่าทำไม Feature นั้นสำคัญ",
                    ("Upload CAD, drawing, quantity, material preference, and use environment together.", "อัปโหลด CAD, Drawing, จำนวน, วัสดุที่ต้องการ และสภาพการใช้งานพร้อมกัน"),
                    ("If the quote engine flags a risk, revise the file before ordering instead of treating the warning as decoration.", "หากระบบราคาแจ้งความเสี่ยง ให้แก้ไฟล์ก่อนสั่งผลิต ไม่ใช่มองเป็นข้อความประกอบ"))
            ],
            [
                Text("Define functional faces before asking for tight tolerance.", "ระบุผิวใช้งานจริงก่อนขอ tolerance แคบ"),
                Text("Tell MALIEV which holes are clearance, tapped, inserted, or post-machined.", "บอก MALIEV ว่ารูใดเป็นรูหลวม รูต๊าป รูใส่ Insert หรือรูเก็บงานหลังผลิต"),
                Text("Upload STEP plus drawing when fit and inspection matter.", "อัปโหลด STEP พร้อม Drawing เมื่องานประกอบและการตรวจรับสำคัญ")
            ]),
        new(
            "choosing-3d-printing-materials",
            Text("Choosing 3D printing materials", "เลือกวัสดุพิมพ์ 3 มิติ"),
            Text("Match strength, heat resistance, finish, and lead time before you order the part.", "เลือกวัสดุจากความแข็งแรง การทนร้อน ผิวงาน และระยะเวลาก่อนสั่งผลิต"),
            Text("Materials", "วัสดุ"),
            ThreeDimensionalPrinterImageUrl,
            [
                Section(
                    "Start from the part's job, not the material name",
                    "เริ่มจากหน้าที่ของชิ้นงาน ไม่ใช่ชื่อวัสดุ",
                    "A material that sounds stronger is not always better for the part. First decide what the part must prove: visual shape, fit, stiffness, impact, heat, chemicals, outdoor exposure, flexibility, or repeated handling. The right material is the one that matches that job with acceptable lead time and finish.",
                    "วัสดุที่ฟังดูแข็งแรงกว่าไม่ได้เหมาะกับทุกชิ้นงานเสมอไป ควรเริ่มจากสิ่งที่ชิ้นงานต้องพิสูจน์ เช่น รูปทรง การประกอบ ความแข็ง Impact ความร้อน สารเคมี งานกลางแจ้ง ความยืดหยุ่น หรือการหยิบจับซ้ำ วัสดุที่ดีคือวัสดุที่ตรงหน้าที่นั้นพร้อมเวลาและผิวที่ยอมรับได้",
                    ("Use PLA or basic resin when shape review matters more than load.", "ใช้ PLA หรือเรซินทั่วไปเมื่อต้องตรวจรูปร่างมากกว่ารับแรง"),
                    ("Move to PETG, ABS, ASA, TPU, PA12, or filled nylon when the test includes real use.", "ย้ายไป PETG, ABS, ASA, TPU, PA12 หรือไนลอนผสม เมื่อการทดสอบรวมการใช้งานจริง")),
                Section(
                    "Heat and environment decide many failures",
                    "ความร้อนและสภาพแวดล้อมเป็นตัวตัดสินความเสียหายจำนวนมาก",
                    "Parts left in a vehicle, near motors, in sunlight, near cleaning chemicals, or around warm fixtures need a different review from desk models. Heat can soften a polymer before it breaks. Chemicals can make a good-looking part brittle later. Tell the quote team about temperature, exposure time, oils, fuels, cleaners, UV, and whether the part is indoors or outdoors.",
                    "ชิ้นงานในรถ ใกล้มอเตอร์ กลางแดด ใกล้น้ำยา หรือใกล้ฟิกซ์เจอร์อุ่น ต้องตรวจต่างจากโมเดลตั้งโต๊ะ ความร้อนอาจทำให้พลาสติกนิ่มก่อนแตก สารเคมีอาจทำให้ชิ้นงานที่ดูดีเปราะภายหลัง แจ้งทีมราคาเรื่องอุณหภูมิ เวลาสัมผัส น้ำมัน เชื้อเพลิง น้ำยา UV และการใช้งานในร่มหรือกลางแจ้ง",
                    ("ASA is usually a better outdoor conversation than PLA.", "ASA มักเหมาะเริ่มคุยเรื่องงานกลางแจ้งมากกว่า PLA"),
                    ("PA12 and engineering resins are useful when repeated handling matters.", "PA12 และเรซินวิศวกรรมเหมาะเมื่อมีการหยิบจับหรือใช้งานซ้ำ")),
                Section(
                    "Surface finish is a process decision",
                    "ผิวงานคือการตัดสินใจกระบวนการ",
                    "Layer lines, support marks, powder texture, resin detail, and machining marks are not the same failure. A display prototype may need resin detail or post-finishing. A bracket may accept texture if the holes and stiffness are right. Decide which faces are visible and which faces are functional before choosing the material.",
                    "เส้นเลเยอร์ รอยซัพพอร์ต ผิวพาวเดอร์ รายละเอียดเรซิน และรอยเครื่องมือ ไม่ใช่ความเสียหายแบบเดียวกัน ต้นแบบโชว์อาจต้องใช้เรซินหรือเก็บผิวเพิ่ม ขายึดอาจยอมรับ Texture ได้ถ้ารูและความแข็งถูกต้อง ควรระบุผิวโชว์และผิวใช้งานก่อนเลือกวัสดุ",
                    ("Mark visible faces if cosmetic quality matters.", "ระบุผิวโชว์หากคุณภาพหน้าตาสำคัญ"),
                    ("Do not pay for cosmetic finish on test-only prototypes unless it affects the decision.", "อย่าจ่ายค่าผิวโชว์ในต้นแบบทดสอบ เว้นแต่มีผลต่อการตัดสินใจ")),
                Section(
                    "Use comparison to narrow the quote, not to replace review",
                    "ใช้การเปรียบเทียบเพื่อกรองทางเลือก ไม่ใช่แทนการตรวจ",
                    "Material tables help narrow the conversation, but final selection still depends on geometry, wall thickness, print orientation, quantity, finish, and acceptance criteria. When fit and appearance both matter, compare the material against the real mating part and the real handling condition, not only a data-sheet number. For uncertain parts, order one or two test pieces before committing to a production batch.",
                    "ตารางวัสดุช่วยกรองทางเลือก แต่การเลือกสุดท้ายยังขึ้นกับ Geometry ความหนาผนัง ทิศทางพิมพ์ จำนวน ผิวงาน และเกณฑ์รับงาน เมื่อทั้งการประกอบและหน้าตาสำคัญ ให้เทียบวัสดุกับชิ้นส่วนประกบจริงและสภาพการหยิบจับจริง ไม่ใช่ดูแค่ตัวเลขใน Datasheet สำหรับชิ้นงานที่ยังไม่แน่ใจ ควรสั่งทดสอบหนึ่งหรือสองชิ้นก่อนสั่งล็อตผลิต")
            ],
            [
                Text("Choose from use case, heat, chemicals, and visible surfaces.", "เลือกจากการใช้งาน ความร้อน สารเคมี และผิวโชว์"),
                Text("Use material comparison to narrow the quote conversation.", "ใช้การเปรียบเทียบวัสดุเพื่อกรองโจทย์ใบเสนอราคา"),
                Text("Prototype uncertain parts before buying a batch.", "ทำต้นแบบชิ้นที่ยังไม่แน่ใจก่อนสั่งล็อต")
            ]),
        new(
            "instant-part-pricing",
            Text("How instant part pricing works", "ระบบคำนวณราคาชิ้นงานทำงานอย่างไร"),
            Text("Upload CAD, review DFM feedback, adjust process and quantity, then continue to order when the price fits.", "อัปโหลด CAD ตรวจ DFM ปรับกระบวนการและจำนวน แล้วสั่งผลิตต่อเมื่อราคาเหมาะสม"),
            Text("Quote engine", "ระบบราคา"),
            FactoryPipeProductionImageUrl,
            [
                Section(
                    "Instant pricing is a quoting workspace, not a blind checkout",
                    "ระบบราคาทันทีคือพื้นที่ทำใบเสนอราคา ไม่ใช่ Checkout แบบไม่ตรวจ",
                    "The quote engine is designed to move CAD files, DFM feedback, material choices, quantity, lead time, and order tracking into one workflow. It gives customers a fast commercial path while still leaving room for manufacturability review before work is accepted.",
                    "ระบบราคาถูกออกแบบให้ไฟล์ CAD หมายเหตุ DFM การเลือกวัสดุ จำนวน ระยะเวลา และการติดตามงานอยู่ในเวิร์กโฟลว์เดียว ลูกค้าจึงเห็นเส้นทางการค้าเร็วขึ้น แต่ยังมีพื้นที่ให้ตรวจความเหมาะสมในการผลิตก่อนรับงาน",
                    ("The price changes when process, material, quantity, finish, or lead time changes.", "ราคาจะเปลี่ยนเมื่อเปลี่ยนกระบวนการ วัสดุ จำนวน ผิว หรือระยะเวลา"),
                    ("Warnings should be resolved before ordering parts that must fit or perform.", "ควรแก้คำเตือนก่อนสั่งชิ้นงานที่ต้องประกอบหรือใช้งานจริง")),
                Section(
                    "The file is checked for manufacturing signals",
                    "ไฟล์ถูกตรวจจากสัญญาณงานผลิต",
                    "A CAD upload can reveal volume, bounding box, wall patterns, hole features, mesh quality, and process risks. The engine uses these signals to prepare the workspace, while we can still ask for drawings, photos, or notes when geometry alone does not explain the requirement.",
                    "การอัปโหลด CAD ทำให้เห็นปริมาตร Bounding box รูปแบบผนัง Feature รู คุณภาพ Mesh และความเสี่ยงของกระบวนการ ระบบใช้สัญญาณเหล่านี้เพื่อเตรียมพื้นที่ราคา ขณะเดียวกัน MALIEV ยังอาจขอ Drawing รูป หรือหมายเหตุเมื่อ Geometry อย่างเดียวอธิบายความต้องการไม่พอ",
                    ("STEP is preferred when controlled faces and machining features matter.", "ควรใช้ STEP เมื่อผิวควบคุมและ Feature สำหรับกัดสำคัญ"),
                    ("STL, OBJ, and 3MF can work for print-focused geometry.", "STL, OBJ และ 3MF ใช้ได้กับ Geometry ที่เน้นงานพิมพ์")),
                Section(
                    "Customers can explore trade-offs before committing",
                    "ลูกค้าสำรวจข้อแลกเปลี่ยนก่อนยืนยันงานได้",
                    "Changing quantity, process, material, finish, and delivery expectation can expose the real cost driver. Sometimes a small CAD change reduces production risk more than changing material. Sometimes the same geometry needs two routes: a quick prototype and a controlled final batch. Treat those comparisons as a design conversation, because the cheapest option is not useful if it misses the fit, finish, or deadline that made the order necessary.",
                    "การเปลี่ยนจำนวน กระบวนการ วัสดุ ผิวงาน และระยะส่งมอบ ช่วยให้เห็นตัวผลักดันต้นทุนจริง บางครั้งการแก้ CAD เล็กน้อยลดความเสี่ยงผลิตได้มากกว่าการเปลี่ยนวัสดุ บางครั้ง Geometry เดียวต้องใช้สองเส้นทาง คือ ต้นแบบเร็วและล็อตสุดท้ายที่ควบคุมมากขึ้น ควรมองการเปรียบเทียบเหล่านี้เป็นบทสนทนาด้านแบบ เพราะตัวเลือกที่ถูกที่สุดไม่มีประโยชน์ถ้าพลาดการประกอบ ผิวงาน หรือกำหนดส่งที่ทำให้ต้องสั่งผลิต",
                    ("Use the workspace to compare, not to guess through separate emails.", "ใช้พื้นที่ราคาเพื่อเปรียบเทียบ แทนการเดาผ่านอีเมลแยกกัน"),
                    ("Keep notes with the quote so review context is not lost.", "เก็บหมายเหตุไว้กับใบเสนอราคาเพื่อไม่ให้บริบทการตรวจหาย")),
                Section(
                    "The accepted quote becomes production context",
                    "ใบเสนอราคาที่อนุมัติกลายเป็นบริบทผลิต",
                    "Once a customer accepts a quote, the selected files, material, quantity, DFM notes, lead time, and documents should continue into ordering and tracking. That continuity is what prevents vendor handoffs from becoming repeated clarification work. A good quote record also protects repeat orders: the next buyer can see which revision was approved, which warning was resolved, and what evidence was expected at delivery.",
                    "เมื่อลูกค้าอนุมัติใบเสนอราคา ไฟล์ วัสดุ จำนวน หมายเหตุ DFM ระยะเวลา และเอกสารที่เลือกควรต่อเนื่องไปถึงการสั่งผลิตและติดตามงาน ความต่อเนื่องนี้ช่วยไม่ให้การส่งต่องานกลายเป็นการถามข้อมูลซ้ำ บันทึกใบเสนอราคาที่ดียังช่วยงานสั่งซ้ำ เพราะผู้สั่งครั้งถัดไปเห็นได้ว่า Revision ใดถูกอนุมัติ คำเตือนใดถูกแก้ และต้องการหลักฐานอะไรตอนส่งมอบ")
            ],
            [
                Text("Pricing reacts to process, material, quantity, finish, and lead time.", "ราคาตอบสนองต่อกระบวนการ วัสดุ จำนวน ผิว และระยะเวลา"),
                Text("DFM warnings protect the order from avoidable production risk.", "คำเตือน DFM ช่วยป้องกันความเสี่ยงผลิตที่หลีกเลี่ยงได้"),
                Text("Accepted quote context should carry into order tracking.", "บริบทใบเสนอราคาที่อนุมัติควรต่อไปถึงการติดตามงาน")
            ]),
        PracticalNote(
            "fdm-print-orientation",
            "FDM print orientation affects strength",
            "ทิศทางพิมพ์ FDM มีผลต่อความแข็งแรง",
            "Layer direction changes how brackets, clips, and covers fail under load.",
            "ทิศทางเลเยอร์เปลี่ยนวิธีที่ขายึด คลิป และฝาครอบเสียหายเมื่อรับแรง",
            "FDM guide",
            "คู่มือ FDM",
            FdmThermoplasticsImageUrl,
            "Decide which direction the part is pulled, bent, screwed, or dropped. A part that looks correct can still split along layers if the main load is perpendicular to the print path.",
            "ตัดสินใจก่อนว่าชิ้นงานถูกดึง งอ ขันสกรู หรือกระแทกจากทิศใด ชิ้นงานที่รูปร่างถูกต้องอาจแยกตามเลเยอร์ได้ถ้าแรงหลักตั้งฉากกับแนวพิมพ์",
            "MALIEV uses the load direction to review orientation, support marks, build time, and whether a machined or nylon route is safer for the first functional test.",
            "MALIEV ใช้ทิศแรงเพื่อตรวจทิศพิมพ์ รอยซัพพอร์ต เวลา Build และดูว่าควรใช้กัดหรือไนลอนสำหรับการทดสอบใช้งานแรกหรือไม่",
            ("Mark the load direction, not only the outside shape.", "ระบุทิศแรง ไม่ใช่แค่รูปร่างภายนอก"),
            ("Expect different strength along and across layers.", "ความแข็งแรงตามเลเยอร์และข้ามเลเยอร์ไม่เท่ากัน"),
            ("Use a test coupon when the part carries real load.", "ใช้ชิ้นทดสอบเมื่อชิ้นงานรับแรงจริง")),
        PracticalNote(
            "fdm-wall-thickness",
            "Wall thickness for printed prototypes",
            "ความหนาผนังสำหรับต้นแบบพิมพ์",
            "Thin walls save material but can create cracks, curl, and fragile edges.",
            "ผนังบางช่วยลดวัสดุ แต่อาจทำให้แตก โก่ง และขอบเปราะ",
            "FDM guide",
            "คู่มือ FDM",
            FdmThermoplasticsImageUrl,
            "Before ordering, separate walls that only show shape from walls that hold screws, clips, or handling force. Add ribs or fillets where stiffness matters instead of making every wall thick.",
            "ก่อนสั่งผลิต ให้แยกผนังที่ใช้ดูรูปทรงออกจากผนังที่ต้องรับสกรู คลิป หรือแรงหยิบจับ ใช้ Rib หรือ Fillet ในจุดที่ต้องการความแข็งแทนการทำให้ทุกผนังหนา",
            "The review checks thin towers, sudden thickness changes, isolated tabs, and heavy masses because these affect warping, print time, and whether support cleanup can be done safely.",
            "การตรวจจะดูเสาบาง การเปลี่ยนความหนาฉับพลัน แท็บเดี่ยว และมวลหนา เพราะมีผลต่อการบิด เวลา Print และความปลอดภัยในการแกะซัพพอร์ต",
            ("Use ribs for stiffness before adding solid mass.", "ใช้ Rib เพิ่มความแข็งก่อนเพิ่มเนื้อตัน"),
            ("Call out flexible walls when they are intentional.", "ระบุผนังยืดหยุ่นเมื่อออกแบบไว้โดยตั้งใจ"),
            ("Thick sections can raise cost and shrink risk.", "เนื้อหนาเพิ่มต้นทุนและความเสี่ยงหดตัวได้")),
        PracticalNote(
            "fdm-heat-material-choice",
            "Heat decides many FDM material choices",
            "ความร้อนเป็นตัวตัดสินวัสดุ FDM หลายงาน",
            "A desk prototype and a part inside a warm machine should not be quoted the same way.",
            "ต้นแบบตั้งโต๊ะกับชิ้นส่วนในเครื่องที่อุ่นไม่ควรถูกเสนอราคาแบบเดียวกัน",
            "Materials",
            "วัสดุ",
            EngineeringPolymerReviewImageUrl,
            "Tell the team if the part sits in a car, near a motor, under sunlight, near hot air, or beside electronics. The important number is use temperature over time, not just peak temperature once.",
            "แจ้งทีมว่าชิ้นงานอยู่ในรถ ใกล้มอเตอร์ กลางแดด ใกล้ลมร้อน หรือใกล้อุปกรณ์ไฟฟ้าหรือไม่ ตัวเลขสำคัญคืออุณหภูมิใช้งานตามเวลา ไม่ใช่อุณหภูมิสูงสุดครั้งเดียว",
            "MALIEV compares PLA, PETG, ABS, ASA, nylon, resin, and machining routes against heat exposure, geometry, quantity, and surface expectation before confirming the quote.",
            "MALIEV เทียบ PLA, PETG, ABS, ASA, ไนลอน, เรซิน และงานกัดกับความร้อน Geometry จำนวน และความคาดหวังเรื่องผิวก่อนยืนยันราคา",
            ("Describe the real operating temperature.", "อธิบายอุณหภูมิใช้งานจริง"),
            ("Outdoor and vehicle parts need special review.", "งานกลางแจ้งและในรถต้องตรวจเป็นพิเศษ"),
            ("Heat can soften a part before it visibly breaks.", "ความร้อนทำให้ชิ้นงานนิ่มก่อนแตกให้เห็นได้")),
        PracticalNote(
            "heat-set-inserts-for-printed-parts",
            "Heat-set inserts make printed parts easier to service",
            "Insert ฝังร้อนช่วยให้งานพิมพ์ถอดประกอบง่ายขึ้น",
            "Repeated screws often need inserts instead of raw printed threads.",
            "สกรูที่ถอดประกอบซ้ำมักควรใช้ Insert แทนเกลียวพิมพ์ตรง",
            "Assembly",
            "การประกอบ",
            ThreeDimensionalPrinterOperatorImageUrl,
            "Decide whether the screw is installed once, opened for service, or loaded in tension. Include screw size, required pull-out resistance, and whether metal hardware is acceptable.",
            "ตัดสินใจก่อนว่าสกรูขันครั้งเดียว เปิดซ่อมซ้ำ หรือรับแรงดึง ระบุขนาดสกรู แรงดึงที่ต้องการ และยอมรับ Hardware โลหะได้หรือไม่",
            "The quote review checks boss diameter, wall around the insert, access for the heat tool, and whether post-processing after printing is included in the scope.",
            "การตรวจราคาจะดูเส้นผ่านศูนย์กลาง Boss ผนังรอบ Insert ทางเข้าเครื่องมือฝังร้อน และดูว่างานเก็บหลังพิมพ์รวมอยู่ในขอบเขตหรือไม่",
            ("Use inserts for repeated assembly.", "ใช้ Insert เมื่อต้องถอดประกอบซ้ำ"),
            ("Leave enough boss material around the insert.", "เหลือเนื้อ Boss รอบ Insert ให้พอ"),
            ("Call out screw size and service expectation.", "ระบุขนาดสกรูและความถี่การซ่อม")),
        PracticalNote(
            "fdm-layer-lines-and-finish",
            "Layer lines are a finishing decision",
            "เส้นเลเยอร์คือการตัดสินใจเรื่องผิว",
            "Visible layer lines may be acceptable for test fixtures but not for customer-facing parts.",
            "เส้นเลเยอร์อาจยอมรับได้ในฟิกซ์เจอร์ทดสอบ แต่ไม่เหมาะกับชิ้นงานที่ลูกค้าเห็น",
            "Finishing",
            "ผิวงาน",
            FdmThermoplasticsImageUrl,
            "Mark which surfaces are cosmetic, handled, sealed, painted, or hidden. Do not pay for cosmetic finishing on faces that will be buried inside an assembly.",
            "ระบุผิวที่เป็นผิวโชว์ ผิวจับ ผิวซีล ผิวทำสี หรือผิวซ่อน อย่าจ่ายค่าเก็บผิวโชว์ให้ด้านที่ถูกซ่อนใน Assembly",
            "MALIEV uses visible-face notes to choose orientation, support placement, sanding scope, painting risk, and whether resin, nylon, or CNC is a better route.",
            "MALIEV ใช้หมายเหตุผิวโชว์เพื่อเลือกทิศพิมพ์ ตำแหน่งซัพพอร์ต ขอบเขตขัด ความเสี่ยงทำสี และดูว่าเรซิน ไนลอน หรือ CNC เหมาะกว่าไหม",
            ("Separate visible faces from functional faces.", "แยกผิวโชว์ออกจากผิวใช้งาน"),
            ("Texture is acceptable when it does not affect the decision.", "Texture ยอมรับได้ถ้าไม่กระทบการตัดสินใจ"),
            ("Finishing time can dominate prototype cost.", "เวลาเก็บผิวอาจเป็นต้นทุนหลักของต้นแบบ")),
        PracticalNote(
            "sla-support-marks",
            "SLA support marks should be planned",
            "ควรวางแผนรอยซัพพอร์ตของ SLA",
            "High-detail resin parts still need support contact points and cleanup.",
            "งานเรซินละเอียดสูงยังมีจุดซัพพอร์ตและงานเก็บผิว",
            "Resin printing",
            "พิมพ์เรซิน",
            SlaResinImageUrl,
            "Decide which faces must remain clean, glossy, transparent, or dimensionally important. If a surface is a sealing face or lens-like face, call it out before the quote.",
            "ตัดสินใจว่าผิวใดต้องสะอาด เงา ใส หรือสำคัญด้านขนาด หากผิวนั้นเป็นผิวซีลหรือผิวคล้ายเลนส์ ให้ระบุก่อนเสนอราคา",
            "The review uses those notes to orient the part, place supports away from critical faces, and confirm whether sanding, coating, or a different process is required.",
            "การตรวจใช้หมายเหตุเหล่านี้เพื่อวางทิศพิมพ์ ย้ายซัพพอร์ตออกจากผิวสำคัญ และยืนยันว่าต้องขัด เคลือบ หรือเปลี่ยนกระบวนการหรือไม่",
            ("Support-free faces are a requirement, not an assumption.", "ผิวไร้รอยซัพพอร์ตคือข้อกำหนด ไม่ใช่สิ่งที่เดาเอา"),
            ("Clear parts need extra expectation setting.", "ชิ้นใสต้องกำหนดความคาดหวังเพิ่ม"),
            ("Resin detail does not remove the need for DFM.", "รายละเอียดเรซินไม่ได้แทนการตรวจ DFM")),
        PracticalNote(
            "resin-brittleness",
            "Resin prototypes can look strong and fail brittle",
            "ต้นแบบเรซินอาจดูแข็งแรงแต่แตกเปราะ",
            "Resin is excellent for detail, but not every resin is a load-bearing material.",
            "เรซินเหมาะกับรายละเอียด แต่ไม่ใช่ทุกสูตรที่รับแรงได้",
            "Resin printing",
            "พิมพ์เรซิน",
            SlaResinImageUrl,
            "Tell the team whether the part is for shape review, snap-fit testing, fluid contact, heat, or repeated handling. The same CAD may need standard resin, tough resin, nylon, or CNC depending on the test.",
            "แจ้งทีมว่าชิ้นงานใช้ตรวจทรง ทดสอบ Snap-fit สัมผัสของเหลว รับความร้อน หรือหยิบจับซ้ำ CAD เดียวกันอาจต้องใช้เรซินทั่วไป เรซิน Tough ไนลอน หรือ CNC ตามการทดสอบ",
            "MALIEV reviews thin tabs, clips, screw bosses, and impact areas because resin failures are often sudden and happen after the surface already looks acceptable.",
            "MALIEV ตรวจแท็บบาง คลิป Boss สกรู และจุดกระแทก เพราะเรซินมักเสียหายแบบฉับพลันแม้ผิวงานดูดีแล้ว",
            ("Use resin for detail, not by default for load.", "ใช้เรซินเพื่อรายละเอียด ไม่ใช่เลือกเป็นค่าเริ่มต้นสำหรับรับแรง"),
            ("Call out clips and snap fits.", "ระบุคลิปและ Snap-fit"),
            ("Choose material from the test, not the photo.", "เลือกวัสดุจากการทดสอบ ไม่ใช่จากรูปถ่าย")),
        PracticalNote(
            "clear-resin-expectations",
            "Clear resin needs optical expectations",
            "เรซินใสต้องกำหนดความคาดหวังด้านแสง",
            "Clear enough for visibility and optically clear are different requirements.",
            "ใสพอให้มองเห็นกับใสแบบ Optical เป็นคนละข้อกำหนด",
            "Resin printing",
            "พิมพ์เรซิน",
            SlaResinImageUrl,
            "Before ordering, decide whether the part only needs light transmission, internal visibility, a display look, or lens-like clarity. Include which faces are polished or coated.",
            "ก่อนสั่งผลิต ให้ตัดสินใจว่าชิ้นงานต้องแค่ให้แสงผ่าน มองเห็นด้านใน เป็นงานโชว์ หรือใสระดับเลนส์ ระบุผิวที่ต้องขัดหรือเคลือบ",
            "The review checks orientation, support marks, wall thickness, polishing access, coating scope, and whether machining or purchased transparent stock is more reliable.",
            "การตรวจจะดูทิศพิมพ์ รอยซัพพอร์ต ความหนาผนัง ทางเข้าขัด ขอบเขตเคลือบ และดูว่างานกัดจากแผ่นใสสำเร็จรูปน่าเชื่อถือกว่าหรือไม่",
            ("Define what clear means for the decision.", "นิยามคำว่าใสให้ตรงกับการตัดสินใจ"),
            ("Support marks affect optical surfaces.", "รอยซัพพอร์ตกระทบผิว Optical"),
            ("Polishing access should be designed in.", "ควรออกแบบให้เข้าถึงการขัดได้")),
        PracticalNote(
            "sls-nylon-powder-removal",
            "SLS nylon needs powder escape paths",
            "งานไนลอน SLS ต้องมีทางออกผง",
            "Closed cavities and tiny channels can trap powder after printing.",
            "โพรงปิดและช่องเล็กอาจกักผงหลังพิมพ์",
            "Powder bed",
            "พาวเดอร์เบด",
            PowderBedNylonImageUrl,
            "If the part has hollow sections, ducts, lattice, or cable channels, decide whether internal powder is acceptable and where cleaning access can be added.",
            "หากชิ้นงานมีโพรง ท่อ Lattice หรือช่องสายไฟ ให้ตัดสินใจว่าผงค้างด้านในยอมรับได้หรือไม่ และเพิ่มทางเข้าทำความสะอาดตรงไหนได้",
            "MALIEV reviews wall thickness, escape holes, minimum channel size, powder removal access, and whether the feature should be split or redesigned before ordering.",
            "MALIEV ตรวจความหนาผนัง รูระบายผง ขนาดช่องขั้นต่ำ ทางเข้าทำความสะอาด และดูว่าควรแยกชิ้นหรือแก้แบบก่อนสั่งหรือไม่",
            ("Do not hide powder-risk cavities in the CAD.", "อย่าซ่อนโพรงเสี่ยงกักผงไว้ใน CAD"),
            ("Escape holes are part of manufacturability.", "รูระบายผงเป็นส่วนหนึ่งของการผลิตได้"),
            ("Split parts can be easier to clean and inspect.", "การแยกชิ้นช่วยทำความสะอาดและตรวจได้ง่ายขึ้น")),
        PracticalNote(
            "nylon-dye-and-finish",
            "Nylon color and texture should be specified early",
            "ควรระบุสีและ Texture ของไนลอนตั้งแต่ต้น",
            "Powder-bed nylon usually has a fine texture that behaves differently from painted resin or FDM.",
            "ไนลอนพาวเดอร์เบดมักมี Texture ละเอียด ต่างจากเรซินทำสีหรือ FDM",
            "Finishing",
            "ผิวงาน",
            PowderBedNylonImageUrl,
            "Decide whether the part is a hidden functional part, a handled part, or a customer-facing part. Black dye, natural white, tumbling, or coating affect appearance and lead time.",
            "ตัดสินใจว่าชิ้นงานเป็นชิ้นซ่อน ชิ้นที่ถูกจับ หรือชิ้นที่ลูกค้าเห็น การย้อมดำ สีขาวธรรมชาติ การขัดถัง หรือเคลือบ มีผลต่อหน้าตาและระยะเวลา",
            "The quote review uses finish expectation to decide whether small text, sharp edges, sliding surfaces, and inspection points need extra handling after printing.",
            "การตรวจราคาใช้ความคาดหวังเรื่องผิวเพื่อตัดสินใจว่าตัวอักษรเล็ก ขอบคม ผิวสไลด์ และจุดตรวจต้องเก็บเพิ่มหลังพิมพ์หรือไม่",
            ("Natural nylon is not the same look as painted plastic.", "ไนลอนธรรมชาติไม่เหมือนพลาสติกทำสี"),
            ("Dye and tumbling should be in the quote scope.", "การย้อมและขัดถังควรอยู่ในขอบเขตราคา"),
            ("Texture can help grip but affect sliding.", "Texture ช่วยจับได้แต่กระทบการสไลด์")),
        PracticalNote(
            "tpu-flexible-parts",
            "Flexible TPU parts need a stiffness target",
            "ชิ้น TPU ยืดหยุ่นต้องมีเป้าความแข็ง",
            "Flexible does not describe wall thickness, hardness, or deformation limit.",
            "คำว่ายืดหยุ่นไม่ได้บอกความหนา ความแข็ง หรือระยะยุบตัว",
            "Flexible parts",
            "ชิ้นงานยืดหยุ่น",
            EngineeringPolymerReviewImageUrl,
            "Tell the team what the part should do: seal, cushion, grip, bend, snap, or protect. Include mating dimensions and whether compression set matters.",
            "แจ้งทีมว่าชิ้นงานต้องซีล กันกระแทก จับยึด งอ Snap หรือป้องกัน ระบุขนาดประกบและความสำคัญของการยุบค้าง",
            "MALIEV reviews hardness, wall section, print path, support cleanup, and whether casting or purchased rubber stock is a better route for the expected feel.",
            "MALIEV ตรวจค่าความแข็ง หน้าตัดผนัง แนวพิมพ์ การแกะซัพพอร์ต และดูว่าหล่อหรือใช้ยางสำเร็จรูปเหมาะกับความรู้สึกที่ต้องการกว่าหรือไม่",
            ("Define the flexible job: seal, grip, bend, or cushion.", "นิยามหน้าที่ยืดหยุ่น: ซีล จับ งอ หรือกันกระแทก"),
            ("Wall thickness changes stiffness quickly.", "ความหนาผนังเปลี่ยนความแข็งเร็ว"),
            ("Send mating dimensions for seals.", "ส่งขนาดประกบสำหรับงานซีล")),
        PracticalNote(
            "asa-outdoor-parts",
            "Outdoor printed parts need weather context",
            "งานพิมพ์กลางแจ้งต้องมีบริบทสภาพอากาศ",
            "UV, rain, heat, and screw load change material selection.",
            "UV ฝน ความร้อน และแรงสกรูเปลี่ยนการเลือกวัสดุ",
            "Materials",
            "วัสดุ",
            EngineeringPolymerReviewImageUrl,
            "State whether the part is under roof, direct sun, near heat, exposed to water, or cleaned with chemicals. Outdoor use is not one condition.",
            "ระบุว่าชิ้นงานอยู่ใต้หลังคา โดนแดดตรง ใกล้ความร้อน โดนน้ำ หรือโดนน้ำยา งานกลางแจ้งไม่ใช่สภาพเดียวกันทุกกรณี",
            "The review compares ASA, PETG, nylon, resin, coating, and machining options against geometry and acceptance criteria before quoting the part.",
            "การตรวจเทียบ ASA, PETG, ไนลอน, เรซิน, การเคลือบ และงานกัดกับ Geometry และเกณฑ์รับงานก่อนเสนอราคา",
            ("Outdoor use should be described specifically.", "ควรอธิบายงานกลางแจ้งอย่างเจาะจง"),
            ("UV resistance and heat resistance are separate.", "ทน UV กับทนร้อนเป็นคนละเรื่อง"),
            ("Coating can help only when the design supports it.", "การเคลือบช่วยได้เมื่อแบบรองรับ")),
        PracticalNote(
            "petg-chemical-contact",
            "Chemical contact changes polymer decisions",
            "การสัมผัสสารเคมีเปลี่ยนการเลือกโพลีเมอร์",
            "Cleaners, oils, fuels, and solvents can make a good-looking part fail later.",
            "น้ำยา น้ำมัน เชื้อเพลิง และตัวทำละลายอาจทำให้ชิ้นงานที่ดูดีเสียภายหลัง",
            "Materials",
            "วัสดุ",
            EngineeringPolymerReviewImageUrl,
            "List the chemical, concentration, temperature, contact time, and whether the part is wiped, soaked, splashed, or pressurized.",
            "ระบุชนิดสาร ความเข้มข้น อุณหภูมิ เวลา และรูปแบบสัมผัสว่าเช็ด แช่ กระเด็น หรือมีแรงดัน",
            "MALIEV uses this context to select candidate materials, flag test needs, and avoid treating cosmetic appearance as proof of chemical compatibility.",
            "MALIEV ใช้บริบทนี้เพื่อเลือกวัสดุที่เป็นไปได้ แจ้งความจำเป็นต้องทดสอบ และไม่ใช้หน้าตาชิ้นงานเป็นหลักฐานว่าเข้ากับสารเคมีได้",
            ("Name the chemical, not just the industry.", "ระบุชื่อสาร ไม่ใช่แค่อุตสาหกรรม"),
            ("Contact time matters as much as concentration.", "เวลาสัมผัสสำคัญพอๆ กับความเข้มข้น"),
            ("Test coupons reduce expensive batch risk.", "ชิ้นทดสอบลดความเสี่ยงล็อตแพง")),
        PracticalNote(
            "cnc-internal-corners",
            "CNC internal corners need tool radius",
            "มุมในของ CNC ต้องมีรัศมีเครื่องมือ",
            "Sharp inside corners are often a CAD wish, not a machinable feature.",
            "มุมในคมกริบมักเป็นความต้องการใน CAD ไม่ใช่ฟีเจอร์ที่กัดได้จริง",
            "CNC guide",
            "คู่มือ CNC",
            PipeMachiningImageUrl,
            "Before upload, identify slots, pockets, and mating corners that are drawn perfectly sharp. Decide whether a radius, relief, dogbone, or split part is acceptable.",
            "ก่อนอัปโหลด ให้หาช่อง Pocket และมุมประกบที่เขียนคมสนิท ตัดสินใจว่ารับรัศมี Relief Dogbone หรือแยกชิ้นได้หรือไม่",
            "MALIEV checks cutter access, corner radius, tool length, and inspection needs because corner geometry can drive setup time and feasibility.",
            "MALIEV ตรวจทางเข้า Cutter รัศมีมุม ความยาวเครื่องมือ และการตรวจ เพราะ Geometry มุมอาจเป็นตัวกำหนดเวลาตั้งงานและความเป็นไปได้",
            ("Use internal radii intentionally.", "ใช้รัศมีมุมในอย่างตั้งใจ"),
            ("Dogbones help square tabs fit into milled pockets.", "Dogbone ช่วยให้แท็บเหลี่ยมใส่ Pocket กัดได้"),
            ("Sharp CAD corners can raise cost sharply.", "มุม CAD ที่คมมากทำให้ต้นทุนเพิ่มมากได้")),
        PracticalNote(
            "cnc-deep-pockets",
            "Deep pockets are setup and chatter risks",
            "Pocket ลึกคือความเสี่ยงการตั้งงานและสั่น",
            "Depth-to-width ratio can matter more than the outside size of the part.",
            "อัตราลึกต่อกว้างอาจสำคัญกว่าขนาดนอกของชิ้นงาน",
            "CNC guide",
            "คู่มือ CNC",
            PipeMachiningImageUrl,
            "Decide whether a deep cavity really needs to be milled from solid, or whether the part can be split, printed, cast, or assembled from simpler pieces.",
            "ตัดสินใจว่าโพรงลึกต้องกัดจากตันจริงหรือไม่ หรือแยกชิ้น พิมพ์ หล่อ หรือประกอบจากชิ้นง่ายกว่าได้หรือไม่",
            "The review checks tool reach, wall vibration, chip evacuation, floor finish, and whether tolerance can be held at the bottom of the pocket.",
            "การตรวจจะดูระยะเอื้อมเครื่องมือ การสั่นของผนัง การคายเศษ ผิวพื้น Pocket และการคุม tolerance ที่ก้น Pocket",
            ("Deep features need tool access, not only CAD clearance.", "Feature ลึกต้องมีทางเข้าเครื่องมือ ไม่ใช่แค่ช่องว่างใน CAD"),
            ("Split parts can be cheaper than heroic machining.", "แยกชิ้นอาจถูกกว่างานกัดยากมาก"),
            ("Bottom tolerance is harder than top geometry.", "Tolerance ที่ก้น Pocket ยากกว่ารูปร่างด้านบน")),
        PracticalNote(
            "cnc-threaded-holes",
            "Threaded holes need depth and access notes",
            "รูเกลียวต้องระบุความลึกและทางเข้า",
            "A thread callout without depth can create quoting and inspection ambiguity.",
            "การระบุเกลียวโดยไม่มีความลึกทำให้เสนอราคาและตรวจรับไม่ชัด",
            "CNC guide",
            "คู่มือ CNC",
            CaliperInspectionImageUrl,
            "Call out thread standard, full thread depth, blind or through condition, and whether the screw must bottom out. Include hardware if fit is critical.",
            "ระบุมาตรฐานเกลียว ความลึกเกลียวเต็ม รูตันหรือรูทะลุ และสกรูต้องชนก้นหรือไม่ ส่ง Hardware หากการประกอบสำคัญ",
            "MALIEV reviews drill depth, tap access, bottom clearance, tool break risk, and whether inserts or post-machining are more appropriate.",
            "MALIEV ตรวจความลึกเจาะ ทางเข้าต๊าป ระยะเผื่อก้นรู ความเสี่ยงเครื่องมือหัก และดูว่า Insert หรืองานเก็บหลังผลิตเหมาะกว่าไหม",
            ("Thread depth is not the same as hole depth.", "ความลึกเกลียวไม่เท่ากับความลึกรู"),
            ("Blind holes need bottom clearance.", "รูตันต้องมีระยะเผื่อก้นรู"),
            ("Send screw details when assembly matters.", "ส่งรายละเอียดสกรูเมื่อการประกอบสำคัญ")),
        PracticalNote(
            "cnc-tight-tolerances",
            "Tight CNC tolerances should be paid for only where needed",
            "Tolerance CNC แคบควรจ่ายเฉพาะจุดที่จำเป็น",
            "Defaulting every dimension to tight tolerance makes parts expensive and slow.",
            "กำหนดทุกมิติให้แคบทำให้ชิ้นงานแพงและช้า",
            "Inspection",
            "การตรวจรับ",
            CaliperInspectionImageUrl,
            "Identify datums, mating faces, bearing seats, hole patterns, and inspection dimensions. Leave non-functional faces at process default.",
            "ระบุ Datum ผิวประกบ ที่นั่งแบริ่ง Pattern รู และมิติที่ต้องตรวจ ปล่อยผิวที่ไม่ใช้งานให้เป็นค่ามาตรฐานกระบวนการ",
            "The quote review separates critical dimensions from cosmetic or non-mating geometry so setup, inspection, and price match the real use case.",
            "การตรวจราคาแยกมิติสำคัญออกจาก Geometry โชว์หรือไม่ประกบ เพื่อให้การตั้งงาน การตรวจ และราคาตรงกับการใช้งานจริง",
            ("Use drawings for inspected dimensions.", "ใช้ Drawing สำหรับมิติที่ต้องตรวจ"),
            ("Do not tolerance decorative edges like bearing seats.", "อย่ากำหนดขอบตกแต่งเหมือนที่นั่งแบริ่ง"),
            ("Tight tolerance needs a reason.", "Tolerance แคบต้องมีเหตุผล")),
        PracticalNote(
            "cnc-aluminum-vs-plastic",
            "Aluminum and engineering plastic solve different problems",
            "อะลูมิเนียมและพลาสติกวิศวกรรมแก้คนละโจทย์",
            "Stiffness, weight, insulation, heat, and wear often decide the material.",
            "ความแข็ง น้ำหนัก ฉนวน ความร้อน และการสึกหรอมักเป็นตัวตัดสินวัสดุ",
            "Materials",
            "วัสดุ",
            MetalWorkshopImageUrl,
            "Decide whether the part needs strength, sliding, electrical insulation, corrosion resistance, low weight, or thermal transfer. Material is not only a price choice.",
            "ตัดสินใจว่าชิ้นงานต้องการความแข็งแรง การสไลด์ ฉนวนไฟฟ้า การกันสนิม น้ำหนักต่ำ หรือการถ่ายเทความร้อน วัสดุไม่ใช่แค่ตัวเลือกราคา",
            "MALIEV checks machining behavior, finishing, tolerance, thread durability, and whether the part should be printed first before committing to machined stock.",
            "MALIEV ตรวจพฤติกรรมการกัด ผิวงาน tolerance ความทนเกลียว และดูว่าควรพิมพ์ทดสอบก่อนใช้ Stock กัดหรือไม่",
            ("Choose material from function, not habit.", "เลือกวัสดุจากหน้าที่ ไม่ใช่ความเคยชิน"),
            ("Plastic can insulate where aluminum conducts.", "พลาสติกเป็นฉนวนในจุดที่อะลูมิเนียมนำไฟฟ้า"),
            ("Prototype geometry before expensive stock when uncertain.", "ทดสอบ Geometry ก่อนใช้ Stock แพงเมื่อยังไม่แน่ใจ")),
        PracticalNote(
            "cnc-surface-finish",
            "CNC surface finish affects both cost and inspection",
            "ผิว CNC มีผลทั้งต้นทุนและการตรวจ",
            "Tool marks, bead blast, anodize, and polishing are different quote scopes.",
            "รอยเครื่องมือ ยิงทราย อโนไดซ์ และขัดเงาเป็นขอบเขตราคาคนละแบบ",
            "Finishing",
            "ผิวงาน",
            MetalWorkshopImageUrl,
            "Mark cosmetic faces, sliding faces, sealing faces, and hidden faces separately. A uniform premium finish on all faces is rarely the economical choice.",
            "แยกผิวโชว์ ผิวสไลด์ ผิวซีล และผิวซ่อนออกจากกัน การเก็บผิวพรีเมียมทุกด้านมักไม่ใช่ทางประหยัด",
            "The review uses finish notes to plan tool path, secondary operations, masking, inspection, and whether finish expectations are compatible with the selected material.",
            "การตรวจใช้หมายเหตุผิวเพื่อวาง Toolpath งานรอง Masking การตรวจ และดูว่าความคาดหวังผิวเข้ากับวัสดุที่เลือกหรือไม่",
            ("Surface finish should be scoped per face.", "ควรกำหนดผิวตามแต่ละหน้า"),
            ("Cosmetic finish can change lead time.", "ผิวโชว์เปลี่ยนระยะเวลาได้"),
            ("Inspection faces should not be hidden under vague finish notes.", "ผิวตรวจรับไม่ควรถูกซ่อนด้วยหมายเหตุผิวกว้างๆ")),
        PracticalNote(
            "cnc-fixture-planning",
            "Fixture planning starts in the CAD",
            "การวางฟิกซ์เจอร์เริ่มตั้งแต่ CAD",
            "How a part is held can decide whether it can be machined cleanly.",
            "วิธีจับชิ้นงานอาจตัดสินว่างานกัดได้สะอาดหรือไม่",
            "CNC guide",
            "คู่มือ CNC",
            MetalWorkshopImageUrl,
            "Look for clampable faces, sacrificial tabs, reference datums, and whether both sides need machining. Thin parts and freeform parts often need extra holding strategy.",
            "ดูผิวที่จับงานได้ แท็บเผื่อ Datum อ้างอิง และดูว่าต้องกัดสองด้านหรือไม่ ชิ้นบางและชิ้น Freeform มักต้องวางกลยุทธ์จับงานเพิ่ม",
            "MALIEV reviews setups, flips, datum control, clamp marks, and inspection sequence so the quote reflects real machine time instead of only material volume.",
            "MALIEV ตรวจจำนวน Setup การกลับด้าน การคุม Datum รอยจับ และลำดับตรวจ เพื่อให้ราคาสะท้อนเวลาเครื่องจริง ไม่ใช่แค่ปริมาตรวัสดุ",
            ("Provide datum faces when fit matters.", "ระบุผิว Datum เมื่อการประกอบสำคัญ"),
            ("Thin parts need holding strategy.", "ชิ้นบางต้องมีกลยุทธ์จับงาน"),
            ("Setup count can be a major cost driver.", "จำนวน Setup อาจเป็นตัวผลักดันต้นทุนหลัก")),
        PracticalNote(
            "scan-datum-strategy",
            "3D scanning still needs a datum strategy",
            "งานสแกน 3 มิติก็ต้องมี Datum",
            "A scan mesh is useful only when it can be aligned to the manufacturing question.",
            "Mesh สแกนมีประโยชน์เมื่อ Align กับคำถามงานผลิตได้",
            "3D scanning",
            "สแกน 3 มิติ",
            ThreeDimensionalScannerImageUrl,
            "Decide what the scan must prove: overall shape, worn area, mating surface, hole position, or production deviation. Mark reference faces or holes when possible.",
            "ตัดสินใจว่างานสแกนต้องพิสูจน์อะไร เช่น รูปร่างรวม จุดสึก ผิวประกบ ตำแหน่งรู หรือความคลาดเคลื่อนการผลิต ระบุผิวหรือรูอ้างอิงเมื่อทำได้",
            "MALIEV uses datum intent to align scan data, separate noise from real deviation, and choose whether the output should be mesh, surface, or parametric CAD.",
            "MALIEV ใช้เจตนา Datum เพื่อ Align ข้อมูลสแกน แยก Noise ออกจากความคลาดเคลื่อนจริง และเลือกว่าจะส่งออกเป็น Mesh, Surface หรือ CAD แบบ Parametric",
            ("Scanning is measurement, not magic reconstruction.", "การสแกนคือการวัด ไม่ใช่การสร้างแบบใหม่อัตโนมัติ"),
            ("Datum intent changes the output quality.", "เจตนา Datum เปลี่ยนคุณภาพผลลัพธ์"),
            ("Mark the mating surfaces before scanning.", "ระบุผิวประกบก่อนสแกน")),
        PracticalNote(
            "scan-to-cad-output",
            "Scan-to-CAD output should match the next process",
            "ผลลัพธ์ Scan-to-CAD ต้องตรงกับกระบวนการถัดไป",
            "STL, STEP surface, and parametric CAD are not interchangeable deliverables.",
            "STL, STEP Surface และ CAD Parametric ไม่ใช่ Deliverable ที่แทนกันได้เสมอ",
            "Reverse engineering",
            "รีเวิร์สเอนจิเนียริ่ง",
            ThreeDimensionalScannerImageUrl,
            "Before quoting, decide whether you need a reference mesh, clean printable model, editable CAD, inspection report, or production-ready drawing.",
            "ก่อนเสนอราคา ให้ตัดสินใจว่าต้องการ Mesh อ้างอิง โมเดลพิมพ์สะอาด CAD แก้ไขได้ รายงานตรวจ หรือ Drawing พร้อมผลิต",
            "The review maps the output to the next workflow: printing, CNC, inspection, redesign, or replacement ordering. Higher editability usually requires more manual modeling.",
            "การตรวจจับคู่ผลลัพธ์กับเวิร์กโฟลว์ถัดไป เช่น พิมพ์ CNC ตรวจสอบ ออกแบบใหม่ หรือสั่งทดแทน ความแก้ไขได้ที่สูงขึ้นมักต้องใช้การขึ้นโมเดลมือมากขึ้น",
            ("Name the deliverable, not only the scan job.", "ระบุ Deliverable ไม่ใช่แค่งานสแกน"),
            ("Editable CAD takes more work than mesh cleanup.", "CAD แก้ไขได้ใช้เวลามากกว่า Mesh Cleanup"),
            ("Output should serve the next manufacturing step.", "ผลลัพธ์ต้องรับใช้ขั้นตอนผลิตถัดไป")),
        PracticalNote(
            "damaged-part-reconstruction",
            "Damaged parts need intent reconstruction",
            "ชิ้นส่วนเสียหายต้องสร้างเจตนาเดิมกลับมา",
            "A broken sample does not always show the original design.",
            "ตัวอย่างแตกไม่ได้แสดงแบบเดิมครบเสมอ",
            "Reverse engineering",
            "รีเวิร์สเอนจิเนียริ่ง",
            ThreeDimensionalScannerImageUrl,
            "Send photos of the assembly, the mating part, broken fragments, and what failure you want to prevent next time. Damage should not be copied blindly.",
            "ส่งรูป Assembly ชิ้นประกบ เศษแตก และความเสียหายที่ต้องการป้องกันครั้งหน้า ไม่ควรลอกความเสียหายกลับเข้าแบบอย่างไม่ตรวจ",
            "MALIEV separates intended surfaces from wear, cracks, deformation, and missing geometry before rebuilding a replacement file.",
            "MALIEV แยกผิวที่ตั้งใจออกจากรอยสึก รอยแตก การบิด และ Geometry ที่หายไป ก่อนสร้างไฟล์ทดแทน",
            ("Do not reproduce cracks as design features.", "อย่าสร้างรอยแตกกลับเป็นฟีเจอร์"),
            ("Send mating context with the broken sample.", "ส่งบริบทการประกบพร้อมตัวอย่างแตก"),
            ("Replacement CAD should improve reorderability.", "CAD ทดแทนควรช่วยให้สั่งซ้ำได้")),
        PracticalNote(
            "inspection-report-scope",
            "Inspection reports need a scope",
            "รายงานตรวจต้องมีขอบเขต",
            "Measuring everything is slower than measuring the dimensions that matter.",
            "การวัดทุกอย่างช้ากว่าการวัดมิติที่สำคัญจริง",
            "Inspection",
            "การตรวจรับ",
            CaliperInspectionImageUrl,
            "Decide which dimensions, datums, surfaces, or deviations determine acceptance. Include drawing tolerances and whether a simple check, scan comparison, or formal report is needed.",
            "ตัดสินใจว่ามิติ Datum ผิว หรือความคลาดเคลื่อนใดใช้รับงาน ส่ง tolerance ใน Drawing และระบุว่าต้องการตรวจง่ายๆ เทียบสแกน หรือรายงานเป็นทางการ",
            "MALIEV uses the scope to choose inspection tools, report format, sampling level, and whether scan-to-CAD comparison is useful.",
            "MALIEV ใช้ขอบเขตเพื่อเลือกเครื่องมือตรวจ รูปแบบรายงาน ระดับการสุ่ม และดูว่าการเทียบ Scan-to-CAD มีประโยชน์หรือไม่",
            ("Define acceptance dimensions before production.", "กำหนดมิติรับงานก่อนผลิต"),
            ("A report should answer a decision.", "รายงานควรตอบการตัดสินใจ"),
            ("Inspection scope affects quote and lead time.", "ขอบเขตตรวจมีผลต่อราคาและเวลา")),
        PracticalNote(
            "scan-surface-limits",
            "3D scans have surface and access limits",
            "งานสแกนมีข้อจำกัดเรื่องผิวและทางเข้า",
            "Glossy, transparent, dark, tiny, and hidden features can need special preparation.",
            "ผิวเงา ใส มืด เล็กมาก และจุดซ่อนอาจต้องเตรียมพิเศษ",
            "3D scanning",
            "สแกน 3 มิติ",
            ThreeDimensionalScannerImageUrl,
            "Tell the team if the part is transparent, reflective, black, oily, flexible, or assembled in a way that hides features. Photos help identify scan risk before scheduling.",
            "แจ้งทีมว่าชิ้นงานใส สะท้อนแสง สีดำ มีน้ำมัน ยืดหยุ่น หรือประกอบจนบัง Feature หรือไม่ รูปถ่ายช่วยเห็นความเสี่ยงก่อนนัดสแกน",
            "The review checks whether coating, disassembly, fixture support, multiple scans, or manual measurement is needed to capture the real requirement.",
            "การตรวจดูว่าต้องพ่นเคลือบ ถอดประกอบ จับยึด สแกนหลายรอบ หรือวัดมือเพิ่มเพื่อจับความต้องการจริงหรือไม่",
            ("Surface condition can affect scan quality.", "สภาพผิวมีผลต่อคุณภาพสแกน"),
            ("Hidden features may need disassembly.", "Feature ที่ซ่อนอาจต้องถอดประกอบ"),
            ("Photos reduce scheduling surprises.", "รูปถ่ายช่วยลดปัญหาหน้างาน")),
        PracticalNote(
            "silicone-master-preparation",
            "Silicone casting starts with the master",
            "งานหล่อซิลิโคนเริ่มจาก Master",
            "The mold repeats the quality and defects of the master pattern.",
            "แม่พิมพ์จะทำซ้ำทั้งคุณภาพและตำหนิของ Master",
            "Casting",
            "งานหล่อ",
            ThreeDimensionalPrinterOperatorImageUrl,
            "Decide whether the master is printed, machined, sanded, primed, polished, or sealed. Cosmetic targets should be fixed before mold making begins.",
            "ตัดสินใจว่า Master จะพิมพ์ กัด ขัด รองพื้น ขัดเงา หรือเคลือบ เป้าผิวโชว์ควรจบก่อนเริ่มทำแม่พิมพ์",
            "MALIEV reviews split lines, parting strategy, surface finish, fragile features, and whether the master can survive mold preparation.",
            "MALIEV ตรวจแนวแยกแม่พิมพ์ กลยุทธ์ Parting ผิวงาน Feature เปราะ และดูว่า Master ทนกระบวนการทำแม่พิมพ์ได้หรือไม่",
            ("A better master makes a better batch.", "Master ที่ดีทำให้ล็อตดีขึ้น"),
            ("Fix cosmetic defects before molding.", "แก้ตำหนิผิวก่อนทำแม่พิมพ์"),
            ("Parting lines should be chosen intentionally.", "ควรเลือกแนวแยกแม่พิมพ์อย่างตั้งใจ")),
        PracticalNote(
            "urethane-casting-shrink",
            "Urethane casting needs shrink and tolerance planning",
            "งานหล่อยูรีเทนต้องวางแผนการหดและ tolerance",
            "Small-batch molded parts are not automatically CNC-accurate.",
            "ชิ้นงานหล่อจำนวนน้อยไม่ได้แม่นระดับ CNC โดยอัตโนมัติ",
            "Casting",
            "งานหล่อ",
            InjectionMoldingLineImageUrl,
            "Define which dimensions matter after curing, which surfaces are cosmetic, and whether the part mates with hardware, gaskets, or another molded part.",
            "กำหนดมิติที่สำคัญหลัง Cure ผิวที่เป็นผิวโชว์ และการประกบกับ Hardware ปะเก็น หรือชิ้นหล่ออื่น",
            "The review checks material shrink, mold life, inserts, wall section, and whether secondary machining or drilling is required after casting.",
            "การตรวจดูการหดวัสดุ อายุแม่พิมพ์ Insert หน้าตัดผนัง และดูว่าต้องกัดหรือเจาะหลังหล่อหรือไม่",
            ("Casting tolerance should be discussed early.", "ควรคุย tolerance งานหล่อตั้งแต่ต้น"),
            ("Post-machining can control critical features.", "งานกัดหลังหล่อช่วยคุม Feature สำคัญได้"),
            ("Batch quantity affects mold strategy.", "จำนวนล็อตมีผลต่อกลยุทธ์แม่พิมพ์")),
        PracticalNote(
            "casting-bubbles-and-vents",
            "Bubbles and vents are design issues too",
            "ฟองอากาศและ Vent เป็นเรื่องของแบบด้วย",
            "Air needs a path out of the mold, especially around thin details and high points.",
            "อากาศต้องมีทางออกจากแม่พิมพ์ โดยเฉพาะจุดบางและจุดสูง",
            "Casting",
            "งานหล่อ",
            InjectionMoldingLineImageUrl,
            "Review pockets, blind corners, tall ribs, thin logos, and undercuts. Decide where small gates or witness marks are acceptable.",
            "ตรวจ Pocket มุมตัน Rib สูง โลโก้บาง และ Undercut ตัดสินใจว่าจุด Gate หรือรอยเล็กๆ ยอมรับได้ตรงไหน",
            "MALIEV plans vents, gates, mold orientation, and cleanup based on the features that trap air or make de-molding difficult.",
            "MALIEV วาง Vent, Gate, ทิศแม่พิมพ์ และงานเก็บจาก Feature ที่กักอากาศหรือถอดแบบยาก",
            ("Air traps should be solved before the mold is made.", "ควรแก้จุดกักอากาศก่อนทำแม่พิมพ์"),
            ("Gate marks need acceptable locations.", "รอย Gate ต้องมีตำแหน่งที่ยอมรับได้"),
            ("Thin logos may need simplification.", "โลโก้บางอาจต้องทำให้ง่ายขึ้น")),
        PracticalNote(
            "pilot-batch-planning",
            "Pilot batches should answer production questions",
            "ล็อตทดลองควรตอบคำถามก่อนผลิตจริง",
            "A pilot batch is most useful when it tests fit, finish, handling, and ordering assumptions.",
            "ล็อตทดลองมีประโยชน์เมื่อทดสอบการประกอบ ผิว การหยิบจับ และสมมติฐานการสั่งผลิต",
            "Production planning",
            "วางแผนผลิต",
            FactoryPipeProductionImageUrl,
            "Define what must be learned before scaling: assembly time, packaging, customer handling, failure rate, finish acceptance, or material performance.",
            "กำหนดสิ่งที่ต้องเรียนรู้ก่อนขยาย เช่น เวลา Assembly บรรจุภัณฑ์ การจับของลูกค้า อัตราเสีย ผิวรับงาน หรือประสิทธิภาพวัสดุ",
            "MALIEV uses pilot goals to choose the process, inspection level, quantity, batch documentation, and whether design changes should happen before repeat orders.",
            "MALIEV ใช้เป้าล็อตทดลองเพื่อเลือกกระบวนการ ระดับตรวจ จำนวน เอกสารล็อต และดูว่าควรแก้แบบก่อนสั่งซ้ำหรือไม่",
            ("Pilot batches need learning goals.", "ล็อตทดลองต้องมีเป้าการเรียนรู้"),
            ("Record what changed before reorder.", "บันทึกสิ่งที่เปลี่ยนก่อนสั่งซ้ำ"),
            ("Do not treat pilot success as full validation automatically.", "อย่าถือว่าล็อตทดลองผ่านเท่ากับ Validate เต็มโดยอัตโนมัติ")),
        PracticalNote(
            "pneumatic-injection-trials",
            "Pneumatic injection trials are for learning before tooling",
            "การทดลองฉีดระบบลมใช้เรียนรู้ก่อนลงทุนทูลลิ่ง",
            "Small thermoplastic trials help test shape, inserts, and material behavior before larger commitments.",
            "การทดลองเทอร์โมพลาสติกล็อตเล็กช่วยทดสอบทรง Insert และพฤติกรรมวัสดุก่อนลงทุนใหญ่",
            "Injection molding",
            "ฉีดพลาสติก",
            InjectionMoldingLineImageUrl,
            "Decide what the trial must prove: fill, insert fit, material feel, assembly, or rough cycle expectation. Keep the acceptance criteria practical for a trial tool.",
            "ตัดสินใจว่าการทดลองต้องพิสูจน์อะไร เช่น การเติมเต็ม การใส่ Insert ความรู้สึกวัสดุ การประกอบ หรือ Cycle คร่าวๆ เกณฑ์รับงานควรเหมาะกับ Tool ทดลอง",
            "MALIEV reviews shot size, material, gate, vent, insert handling, and whether the trial should lead to production tooling or remain a short-run method.",
            "MALIEV ตรวจ Shot size วัสดุ Gate, Vent, การจับ Insert และดูว่าการทดลองควรนำไปสู่ทูลลิ่งผลิตจริงหรือใช้เป็นวิธีล็อตสั้น",
            ("Trial tooling should answer one or two clear questions.", "Tool ทดลองควรตอบคำถามชัดๆ หนึ่งหรือสองข้อ"),
            ("Material behavior matters more than cosmetic perfection early.", "พฤติกรรมวัสดุสำคัญกว่าความสวยสมบูรณ์ในช่วงแรก"),
            ("Use results to decide the next tooling step.", "ใช้ผลทดลองตัดสินขั้นตอนทูลลิ่งถัดไป")),
        PracticalNote(
            "injection-molding-draft",
            "Draft angles prevent molding surprises",
            "Draft angle ช่วยป้องกันปัญหางานฉีด",
            "Vertical walls can lock parts in the mold or damage cosmetic faces.",
            "ผนังตั้งตรงอาจล็อกชิ้นงานในแม่พิมพ์หรือทำร้ายผิวโชว์",
            "Injection molding",
            "ฉีดพลาสติก",
            InjectionMoldingLineImageUrl,
            "Identify pull direction, visible faces, texture, and deep walls. Decide whether small draft changes are acceptable before quoting a molded route.",
            "ระบุทิศดึง ผิวโชว์ Texture และผนังลึก ตัดสินใจว่าปรับ Draft เล็กน้อยได้หรือไม่ก่อนเสนอราคากระบวนการหล่อหรือฉีด",
            "The review checks demolding, parting line, shutoff areas, undercuts, and whether the geometry needs sliders, inserts, or redesign.",
            "การตรวจดูการถอดแบบ แนวแยก Shutoff, Undercut และดูว่า Geometry ต้องใช้ Slider, Insert หรือแก้แบบหรือไม่",
            ("Pull direction should be clear.", "ควรระบุทิศดึงให้ชัด"),
            ("Textured faces usually need more draft.", "ผิว Texture มักต้องการ Draft มากขึ้น"),
            ("Undercuts change tooling cost.", "Undercut เปลี่ยนต้นทุนทูลลิ่ง")),
        PracticalNote(
            "injection-ribs-and-bosses",
            "Ribs and bosses need molding discipline",
            "Rib และ Boss ต้องออกแบบตามวินัยงานฉีด",
            "Too much local thickness can create sink marks, warpage, and long cooling time.",
            "ความหนาเฉพาะจุดมากเกินไปทำให้เกิด Sink บิด และเวลาเย็นนาน",
            "Injection molding",
            "ฉีดพลาสติก",
            InjectionMoldingLineImageUrl,
            "Before quoting, review screw bosses, ribs, gussets, and thick pads. Decide where strength is needed and where material can be cored out.",
            "ก่อนเสนอราคา ตรวจ Boss สกรู Rib, Gusset และ Pad หนา ตัดสินใจว่าต้องการความแข็งแรงตรงไหนและจุดใดควรคว้านเนื้อออก",
            "MALIEV checks thickness ratio, flow, cooling, screw load, and cosmetic surfaces so molded parts do not inherit prototype-only geometry.",
            "MALIEV ตรวจอัตราความหนา การไหล การเย็น แรงสกรู และผิวโชว์ เพื่อไม่ให้งานฉีดรับ Geometry ที่เหมาะแค่ต้นแบบ",
            ("Strength should come from ribs, not solid blocks.", "ความแข็งควรมาจาก Rib ไม่ใช่ก้อนตัน"),
            ("Bosses need wall support and screw intent.", "Boss ต้องมีผนังรองรับและเจตนาสกรู"),
            ("Sink marks are often designed in accidentally.", "Sink มักถูกออกแบบเข้ามาโดยไม่ตั้งใจ")),
        PracticalNote(
            "injection-gates-and-sink",
            "Gate and sink marks need acceptable locations",
            "Gate และ Sink ต้องมีตำแหน่งที่ยอมรับได้",
            "Plastic has to enter, cool, and shrink somewhere.",
            "พลาสติกต้องมีจุดเข้า เย็นตัว และหดตัว",
            "Injection molding",
            "ฉีดพลาสติก",
            InjectionMoldingLineImageUrl,
            "Mark visible surfaces, hidden surfaces, functional seals, and label areas. Gate marks and slight sink are more manageable when acceptable zones are known.",
            "ระบุผิวโชว์ ผิวซ่อน ผิวซีล และพื้นที่ Label รอย Gate และ Sink เล็กน้อยจัดการง่ายขึ้นเมื่อรู้โซนที่ยอมรับได้",
            "The review checks gate position, flow path, weld lines, thick sections, and cosmetic priorities before recommending a trial or production tooling path.",
            "การตรวจดูตำแหน่ง Gate ทางไหล Weld line จุดหนา และลำดับความสำคัญผิวก่อนแนะนำทดลองหรือทำ Tool ผลิตจริง",
            ("Every molded part needs a gate strategy.", "ชิ้นฉีดทุกชิ้นต้องมีกลยุทธ์ Gate"),
            ("Cosmetic zones should be marked in the quote.", "ควรระบุโซนผิวโชว์ในใบเสนอราคา"),
            ("Thick sections invite sink risk.", "จุดหนาชวนเกิด Sink")),
        PracticalNote(
            "enclosure-snap-fits",
            "Snap-fit enclosures need material and service intent",
            "เคส Snap-fit ต้องระบุวัสดุและการซ่อม",
            "A snap that works once may not survive repeated opening.",
            "Snap ที่ใช้ได้ครั้งเดียวอาจไม่ทนการเปิดซ้ำ",
            "3D design",
            "ออกแบบ 3 มิติ",
            DesignPlanningImageUrl,
            "Decide how many cycles the snap must survive, whether tools are allowed for opening, and what material flexibility is expected.",
            "ตัดสินใจว่า Snap ต้องทนกี่รอบ ใช้เครื่องมือเปิดได้หรือไม่ และคาดหวังความยืดหยุ่นวัสดุแบบใด",
            "MALIEV reviews hook geometry, strain, print orientation, resin brittleness, screw fallback, and whether a prototype route can validate the closure before production.",
            "MALIEV ตรวจ Geometry ของ Hook, Strain, ทิศพิมพ์ ความเปราะเรซิน ทางเลือกสกรู และดูว่าต้นแบบช่วย Validate การปิดก่อนผลิตได้หรือไม่",
            ("Snap fits need cycle expectations.", "Snap-fit ต้องมีความคาดหวังจำนวนรอบ"),
            ("Material flexibility controls the design.", "ความยืดหยุ่นวัสดุควบคุมแบบ"),
            ("Prototype snaps before committing to tooling.", "ทดสอบ Snap ด้วยต้นแบบก่อนลงทุน Tool")),
        PracticalNote(
            "screw-boss-strategy",
            "Screw bosses should be designed around the fastener",
            "Boss สกรูควรออกแบบรอบ Fastener",
            "Boss size, wall support, inserts, and torque all affect reliability.",
            "ขนาด Boss ผนังรองรับ Insert และแรงบิดมีผลต่อความน่าเชื่อถือ",
            "Assembly",
            "การประกอบ",
            DesignPlanningImageUrl,
            "Send screw size, assembly frequency, torque expectation, and whether the joint carries load or only closes a cover.",
            "ส่งขนาดสกรู ความถี่ประกอบ แรงบิดที่คาดหวัง และรอยต่อนั้นรับแรงหรือแค่ปิดฝา",
            "The review checks pilot hole, insert option, wall thickness, boss ribs, print orientation, and whether CNC or molding needs a different boss design.",
            "การตรวจดูรูนำ ตัวเลือก Insert ความหนาผนัง Rib รอบ Boss ทิศพิมพ์ และดูว่า CNC หรือ Mold ต้องใช้แบบ Boss ต่างกันหรือไม่",
            ("Design bosses from the screw outward.", "ออกแบบ Boss จากสกรูออกไปด้านนอก"),
            ("Torque and service cycles matter.", "แรงบิดและจำนวนรอบซ่อมสำคัญ"),
            ("Printed and molded bosses are not the same.", "Boss งานพิมพ์และงานฉีดไม่เหมือนกัน")),
        PracticalNote(
            "tolerance-stack-up",
            "Tolerance stack-up starts with the assembly",
            "Stack-up tolerance เริ่มจาก Assembly",
            "One tight dimension rarely explains the whole fit problem.",
            "มิติแคบเพียงจุดเดียวมักอธิบายปัญหาการประกอบไม่ครบ",
            "Inspection",
            "การตรวจรับ",
            CaliperInspectionImageUrl,
            "Share the mating part, datum scheme, clearance target, and failure mode: too loose, too tight, rubs, leaks, binds, or misaligns.",
            "ส่งชิ้นประกบ แผน Datum ระยะเผื่อ และอาการเสีย เช่น หลวม แน่น ขูด รั่ว ฝืด หรือเยื้องศูนย์",
            "MALIEV uses assembly context to decide which dimensions deserve tight tolerance and which can stay at process default.",
            "MALIEV ใช้บริบท Assembly เพื่อตัดสินว่ามิติใดควรคุมแคบและมิติใดปล่อยตามมาตรฐานกระบวนการได้",
            ("Fit is an assembly property.", "Fit เป็นคุณสมบัติของ Assembly"),
            ("Name the failure mode you are avoiding.", "ระบุอาการเสียที่ต้องหลีกเลี่ยง"),
            ("Do not tighten every dimension equally.", "อย่าคุมทุกมิติให้แคบเท่ากัน")),
        PracticalNote(
            "step-vs-stl-files",
            "STEP and STL answer different quote questions",
            "STEP และ STL ตอบคำถามราคาไม่เหมือนกัน",
            "Both can describe shape, but only one carries CAD faces cleanly for many reviews.",
            "ทั้งคู่บอกรูปร่างได้ แต่มีเพียงบางไฟล์ที่เก็บผิว CAD ได้ดีสำหรับการตรวจหลายแบบ",
            "File prep",
            "เตรียมไฟล์",
            DesignPlanningImageUrl,
            "Upload STEP when controlled faces, holes, machining, or editable CAD matter. Upload STL, OBJ, or 3MF when mesh-based printing is enough.",
            "อัปโหลด STEP เมื่อผิวควบคุม รู งานกัด หรือ CAD แก้ไขได้สำคัญ อัปโหลด STL, OBJ หรือ 3MF เมื่อ Mesh สำหรับพิมพ์เพียงพอ",
            "MALIEV uses file type to decide what can be measured, repaired, priced, and routed without asking for another upload.",
            "MALIEV ใช้ชนิดไฟล์เพื่อตัดสินว่าวัด ซ่อม ประเมินราคา และเลือกเส้นทางผลิตได้แค่ไหนโดยไม่ต้องขอไฟล์เพิ่ม",
            ("STEP is preferred for controlled CAD review.", "STEP เหมาะกับการตรวจ CAD ควบคุม"),
            ("Mesh files are useful for print-focused geometry.", "ไฟล์ Mesh เหมาะกับ Geometry เน้นพิมพ์"),
            ("Upload drawings when tolerances matter.", "อัปโหลด Drawing เมื่อ tolerance สำคัญ")),
        PracticalNote(
            "drawings-that-reduce-back-and-forth",
            "A simple drawing can save days of clarification",
            "Drawing ง่ายๆ ช่วยลดการถามกลับหลายวัน",
            "The drawing does not need to be beautiful; it needs to answer acceptance questions.",
            "Drawing ไม่จำเป็นต้องสวย แต่ต้องตอบคำถามรับงาน",
            "File prep",
            "เตรียมไฟล์",
            DesignPlanningImageUrl,
            "Mark critical dimensions, material, finish, thread notes, visible faces, quantity, and revision. Even a marked PDF can be enough when it is clear.",
            "ระบุมิติสำคัญ วัสดุ ผิว หมายเหตุเกลียว ผิวโชว์ จำนวน และ Revision แม้เป็น PDF ที่ Mark ไว้ก็พอได้ถ้าชัดเจน",
            "MALIEV uses the drawing to reduce ambiguity in pricing, inspection, and production handoff, especially when CAD geometry alone cannot explain intent.",
            "MALIEV ใช้ Drawing ลดความไม่ชัดในราคา การตรวจ และการส่งต่อผลิต โดยเฉพาะเมื่อ Geometry ใน CAD อธิบายเจตนาไม่พอ",
            ("A marked PDF is better than silent CAD.", "PDF ที่ Mark ไว้ดีกว่า CAD เงียบๆ"),
            ("Revision should travel with the quote.", "Revision ควรเดินไปกับใบเสนอราคา"),
            ("Drawings define what must be inspected.", "Drawing กำหนดสิ่งที่ต้องตรวจ")),
        PracticalNote(
            "quantity-breaks",
            "Quantity changes the right manufacturing route",
            "จำนวนเปลี่ยนเส้นทางผลิตที่เหมาะสม",
            "One part, ten parts, and two hundred parts should not always use the same process.",
            "หนึ่งชิ้น สิบชิ้น และสองร้อยชิ้นไม่ควรใช้กระบวนการเดียวกันเสมอไป",
            "Quoting",
            "การเสนอราคา",
            FactoryPipeProductionImageUrl,
            "Send target quantity, repeat expectation, and whether this is prototype, pilot batch, maintenance spare, or production replenishment.",
            "ส่งจำนวนเป้าหมาย ความคาดหวังสั่งซ้ำ และระบุว่าเป็นต้นแบบ ล็อตทดลอง อะไหล่ซ่อมบำรุง หรือเติมสต็อกผลิต",
            "MALIEV uses quantity to compare setup cost, material waste, tooling effort, finishing time, and whether a process switch reduces unit cost.",
            "MALIEV ใช้จำนวนเพื่อเทียบค่าตั้งงาน เศษวัสดุ งานทูลลิ่ง เวลาเก็บผิว และดูว่าการเปลี่ยนกระบวนการลดต้นทุนต่อชิ้นได้หรือไม่",
            ("Quote one-off and batch needs separately.", "ขอราคางานชิ้นเดียวและล็อตแยกโจทย์กัน"),
            ("Setup cost spreads across quantity.", "ค่าตั้งงานกระจายตามจำนวน"),
            ("Repeat orders deserve revision control.", "งานสั่งซ้ำควรคุม Revision")),
        PracticalNote(
            "lead-time-risk",
            "Fast lead time needs scope control",
            "ระยะเวลาสั้นต้องคุมขอบเขต",
            "Speed is possible when material, finish, inspection, and files are ready.",
            "ความเร็วเกิดขึ้นได้เมื่อวัสดุ ผิว การตรวจ และไฟล์พร้อม",
            "Quoting",
            "การเสนอราคา",
            FactoryPipeProductionImageUrl,
            "Decide which part of the scope is negotiable: finish, material, inspection level, quantity, or delivery date. A rush quote needs a clear trade-off.",
            "ตัดสินใจว่าส่วนใดของขอบเขตยืดหยุ่นได้ เช่น ผิว วัสดุ ระดับตรวจ จำนวน หรือวันส่ง งานด่วนต้องมีข้อแลกเปลี่ยนชัดเจน",
            "MALIEV checks machine availability, material readiness, DFM risk, post-processing, inspection time, and delivery route before promising speed.",
            "MALIEV ตรวจคิวเครื่อง วัสดุ ความเสี่ยง DFM งานหลังผลิต เวลาตรวจ และเส้นทางส่งก่อนรับปากเรื่องความเร็ว",
            ("Rush orders need clean inputs.", "งานด่วนต้องมีข้อมูลสะอาด"),
            ("Post-processing often controls lead time.", "งานหลังผลิตมักคุมระยะเวลา"),
            ("Decide what can change if speed matters most.", "ตัดสินใจว่าสิ่งใดเปลี่ยนได้ถ้าความเร็วสำคัญที่สุด")),
        PracticalNote(
            "cosmetic-vs-functional-surfaces",
            "Cosmetic and functional surfaces need separate notes",
            "ผิวโชว์และผิวใช้งานต้องแยกหมายเหตุ",
            "A beautiful surface is not always the surface that controls fit.",
            "ผิวสวยไม่ใช่ผิวที่คุมการประกอบเสมอไป",
            "Finishing",
            "ผิวงาน",
            CaliperInspectionImageUrl,
            "Mark visible faces, mating faces, sliding faces, sealing faces, and hidden faces. Each face may need a different manufacturing priority.",
            "ระบุผิวโชว์ ผิวประกบ ผิวสไลด์ ผิวซีล และผิวซ่อน ผิวแต่ละประเภทอาจมีลำดับความสำคัญการผลิตต่างกัน",
            "MALIEV uses surface intent to choose orientation, tool path, finish scope, inspection dimensions, and whether cosmetic work should wait until fit is validated.",
            "MALIEV ใช้เจตนาผิวเพื่อเลือกทิศพิมพ์ Toolpath ขอบเขตผิว มิติตรวจ และดูว่างานผิวควรรอจน Fit ผ่านก่อนหรือไม่",
            ("Visible does not always mean critical.", "ผิวโชว์ไม่ได้แปลว่าสำคัญด้านขนาดเสมอ"),
            ("Mating surfaces should be called out.", "ควรระบุผิวประกบ"),
            ("Validate fit before paying for final cosmetics.", "ตรวจ Fit ก่อนจ่ายค่าผิวสุดท้าย")),
        PracticalNote(
            "acceptance-criteria",
            "Acceptance criteria prevent subjective rework",
            "เกณฑ์รับงานช่วยลดการแก้งานตามความรู้สึก",
            "Good acceptance notes say what passes, not only what is preferred.",
            "หมายเหตุรับงานที่ดีบอกว่าอะไรผ่าน ไม่ใช่แค่ชอบอะไร",
            "Inspection",
            "การตรวจรับ",
            CaliperInspectionImageUrl,
            "Define dimensions, material, finish, quantity, delivery expectation, and allowable cosmetic marks. Include how the part will be checked when it arrives.",
            "กำหนดมิติ วัสดุ ผิว จำนวน ระยะส่ง และรอยผิวที่ยอมรับได้ ระบุวิธีตรวจเมื่อชิ้นงานถึงมือ",
            "MALIEV uses acceptance criteria to align quote, production, inspection, and customer review before work starts.",
            "MALIEV ใช้เกณฑ์รับงานเพื่อให้ใบเสนอราคา ผลิต ตรวจ และการรับของลูกค้าตรงกันก่อนเริ่มงาน",
            ("Write pass/fail notes before production.", "เขียนเงื่อนไขผ่าน/ไม่ผ่านก่อนผลิต"),
            ("Cosmetic tolerance should be explicit.", "Tolerance ด้านผิวควรชัดเจน"),
            ("Acceptance notes protect both teams.", "หมายเหตุรับงานช่วยทั้งสองฝ่าย")),
        PracticalNote(
            "reordering-revisions",
            "Repeat orders need revision discipline",
            "การสั่งซ้ำต้องคุม Revision",
            "A small CAD change can make an old quote unsafe to reuse.",
            "การแก้ CAD เล็กน้อยอาจทำให้ใบเสนอราคาเดิมไม่ปลอดภัยที่จะใช้ซ้ำ",
            "Ordering",
            "การสั่งงาน",
            FactoryPipeProductionImageUrl,
            "Keep file names, revision notes, drawing dates, and accepted quote references together. Explain what changed from the previous order.",
            "เก็บชื่อไฟล์ หมายเหตุ Revision วันที่ Drawing และเลขใบเสนอราคาที่อนุมัติไว้ด้วยกัน อธิบายสิ่งที่เปลี่ยนจากคำสั่งก่อน",
            "MALIEV uses revision context to decide whether the old process, material, inspection plan, and price still apply.",
            "MALIEV ใช้บริบท Revision เพื่อตัดสินว่ากระบวนการ วัสดุ แผนตรวจ และราคาเดิมยังใช้ได้หรือไม่",
            ("Never replace CAD silently.", "อย่าเปลี่ยน CAD แบบเงียบๆ"),
            ("Explain what changed since the last order.", "อธิบายสิ่งที่เปลี่ยนจากคำสั่งก่อน"),
            ("Accepted quote context should stay attached.", "บริบทใบเสนอราคาที่อนุมัติควรถูกแนบไว้")),
        PracticalNote(
            "maintenance-spare-parts",
            "Maintenance spares need failure context",
            "อะไหล่ซ่อมบำรุงต้องมีบริบทความเสียหาย",
            "Replacing a broken part is not the same as copying a broken part.",
            "การทำอะไหล่ทดแทนไม่ใช่การลอกชิ้นที่แตก",
            "Maintenance",
            "ซ่อมบำรุง",
            ThreeDimensionalScannerImageUrl,
            "Send where the part sits, how it failed, load direction, temperature, chemicals, and whether downtime or exact appearance matters more.",
            "ส่งตำแหน่งใช้งาน วิธีเสียหาย ทิศแรง อุณหภูมิ สารเคมี และระบุว่าลด Downtime หรือหน้าตาเหมือนเดิมสำคัญกว่า",
            "MALIEV reviews whether to scan, redesign, print, machine, or combine processes so the replacement solves the failure instead of reproducing it.",
            "MALIEV ตรวจว่าควรสแกน ออกแบบใหม่ พิมพ์ กัด หรือผสมกระบวนการ เพื่อให้อะไหล่แก้ปัญหา ไม่ใช่ทำซ้ำความเสียหาย",
            ("Share the failure mode.", "ส่งอาการเสียหาย"),
            ("Replacement can be improved from the original.", "อะไหล่ทดแทนอาจปรับปรุงจากของเดิมได้"),
            ("Downtime priorities affect process choice.", "ความสำคัญของ Downtime มีผลต่อกระบวนการ")),
        PracticalNote(
            "jigs-and-fixtures",
            "Jigs and fixtures should be designed around operators",
            "Jig และ Fixture ควรออกแบบรอบผู้ใช้งาน",
            "A fixture that is strong but slow to use can still fail production.",
            "ฟิกซ์เจอร์ที่แข็งแรงแต่ใช้งานช้าอาจยังไม่ตอบโจทย์ผลิต",
            "Fixtures",
            "ฟิกซ์เจอร์",
            ThreeDimensionalPrinterOperatorImageUrl,
            "Describe the operator action, cycle time, part orientation, clamping force, mistake-proofing need, and how the fixture will be stored or cleaned.",
            "อธิบายการทำงานของผู้ใช้ Cycle time ทิศวางชิ้น แรงจับ ความต้องการป้องกันผิดพลาด และวิธีเก็บหรือทำความสะอาดฟิกซ์เจอร์",
            "MALIEV reviews ergonomics, replaceable wear points, material route, printed versus machined features, and inspection surfaces.",
            "MALIEV ตรวจ Ergonomics จุดสึกที่เปลี่ยนได้ เส้นทางวัสดุ Feature ที่ควรพิมพ์หรือกัด และผิวตรวจรับ",
            ("Design for the hand that uses it.", "ออกแบบเพื่อมือของคนใช้"),
            ("Wear points should be replaceable when possible.", "จุดสึกควรเปลี่ยนได้เมื่อทำได้"),
            ("Cycle time is part of fixture quality.", "Cycle time เป็นส่วนหนึ่งของคุณภาพฟิกซ์เจอร์")),
        PracticalNote(
            "electronics-enclosures",
            "Electronics enclosures need thermal and service notes",
            "กล่องอิเล็กทรอนิกส์ต้องมีหมายเหตุความร้อนและการซ่อม",
            "Board size is only the beginning of enclosure design.",
            "ขนาดบอร์ดเป็นเพียงจุดเริ่มของการออกแบบกล่อง",
            "Enclosures",
            "กล่อง",
            DesignPlanningImageUrl,
            "Send board dimensions, connector positions, heat sources, mounting method, service access, cable strain relief, and whether the enclosure is indoor or outdoor.",
            "ส่งขนาดบอร์ด ตำแหน่ง Connector แหล่งความร้อน วิธีติดตั้ง ทางเปิดซ่อม Cable strain relief และการใช้งานในร่มหรือกลางแจ้ง",
            "MALIEV reviews wall thickness, bosses, snap or screw strategy, venting, material, and prototype route before production geometry is frozen.",
            "MALIEV ตรวจความหนาผนัง Boss กลยุทธ์ Snap หรือสกรู ช่องระบาย วัสดุ และเส้นทางต้นแบบก่อนล็อก Geometry ผลิต",
            ("Connectors control more geometry than the board outline.", "Connector คุม Geometry มากกว่าขอบบอร์ด"),
            ("Heat and service access should be known early.", "ควรรู้ความร้อนและทางซ่อมตั้งแต่ต้น"),
            ("Outdoor enclosures need material review.", "กล่องกลางแจ้งต้องตรวจวัสดุ")),
        PracticalNote(
            "robotics-end-effectors",
            "Robot end effectors need load and collision context",
            "End effector หุ่นยนต์ต้องมีบริบทแรงและการชน",
            "Weight, stiffness, cable routing, and crash recovery all affect manufacturing choices.",
            "น้ำหนัก ความแข็ง ทางเดินสาย และการกู้คืนหลังชนมีผลต่อการเลือกผลิต",
            "Robotics",
            "หุ่นยนต์",
            ThreeDimensionalPrinterOperatorImageUrl,
            "Send payload, robot model, motion speed, mounting pattern, sensor positions, cable path, and whether the part is sacrificial or long-life.",
            "ส่ง Payload รุ่นหุ่นยนต์ ความเร็วเคลื่อนที่ Pattern ยึด ตำแหน่ง Sensor ทางเดินสาย และชิ้นงานเป็นชิ้นเสียสละหรือใช้งานยาว",
            "MALIEV reviews printed, machined, and hybrid construction to balance weight, stiffness, replacement speed, and acceptance dimensions.",
            "MALIEV ตรวจโครงสร้างพิมพ์ กัด หรือผสม เพื่อบาลานซ์น้ำหนัก ความแข็ง ความเร็วเปลี่ยนอะไหล่ และมิติรับงาน",
            ("Payload and speed change design risk.", "Payload และความเร็วเปลี่ยนความเสี่ยงแบบ"),
            ("Sacrificial parts can be designed for fast replacement.", "ชิ้นเสียสละออกแบบให้เปลี่ยนเร็วได้"),
            ("Cable routing is part of manufacturability.", "ทางเดินสายเป็นส่วนหนึ่งของการผลิตได้")),
        PracticalNote(
            "automotive-check-fixtures",
            "Automotive check fixtures need repeatable references",
            "Check fixture ยานยนต์ต้องมีจุดอ้างอิงซ้ำได้",
            "A fixture is useful when the same part can be checked the same way every time.",
            "ฟิกซ์เจอร์มีประโยชน์เมื่อชิ้นเดิมถูกตรวจแบบเดิมได้ทุกครั้ง",
            "Fixtures",
            "ฟิกซ์เจอร์",
            CaliperInspectionImageUrl,
            "Define datum points, operator action, go/no-go condition, wear surfaces, labeling, and whether inspection is visual, tactile, or measured.",
            "กำหนดจุด Datum การทำงานของผู้ใช้ เงื่อนไข Go/No-go ผิวสึก Label และการตรวจเป็นแบบมอง จับ หรือวัด",
            "MALIEV reviews material, inserts, bushings, printed versus machined contact points, and whether scan data or CAD nominal should drive the fixture.",
            "MALIEV ตรวจวัสดุ Insert, Bushing จุดสัมผัสพิมพ์หรือกัด และดูว่าจะใช้ข้อมูลสแกนหรือ CAD Nominal เป็นตัวกำหนดฟิกซ์เจอร์",
            ("Repeatability is the fixture's job.", "ความซ้ำได้คือหน้าที่ของฟิกซ์เจอร์"),
            ("Wear surfaces should be planned.", "ควรวางแผนผิวสึก"),
            ("Go/no-go criteria need clear geometry.", "เกณฑ์ Go/No-go ต้องมี Geometry ชัด")),
        PracticalNote(
            "low-volume-production",
            "Low-volume production is not just many prototypes",
            "งานผลิตจำนวนน้อยไม่ใช่แค่ต้นแบบหลายชิ้น",
            "Batch control, repeatability, packaging, and inspection become part of the job.",
            "การคุมล็อต ความซ้ำได้ บรรจุภัณฑ์ และการตรวจกลายเป็นส่วนหนึ่งของงาน",
            "Production planning",
            "วางแผนผลิต",
            FactoryPipeProductionImageUrl,
            "Tell the team target quantity, reorder frequency, acceptable variation, packaging needs, and whether every part or sample parts must be inspected.",
            "แจ้งจำนวนเป้าหมาย ความถี่สั่งซ้ำ ความคลาดเคลื่อนที่ยอมรับได้ ความต้องการบรรจุ และต้องตรวจทุกชิ้นหรือสุ่มตรวจ",
            "MALIEV uses this to decide process stability, documentation, inspection plan, spare quantity, and whether a pilot run should happen first.",
            "MALIEV ใช้ข้อมูลนี้เพื่อเลือกความเสถียรกระบวนการ เอกสาร แผนตรวจ จำนวนเผื่อ และดูว่าควรทำล็อตทดลองก่อนหรือไม่",
            ("Production needs repeatability, not just shape.", "งานผลิตต้องการความซ้ำได้ ไม่ใช่แค่รูปร่าง"),
            ("Packaging can be part of quality.", "บรรจุภัณฑ์เป็นส่วนหนึ่งของคุณภาพได้"),
            ("Inspection level should match risk.", "ระดับตรวจควรตรงกับความเสี่ยง")),
        PracticalNote(
            "surface-text-labels",
            "Text and labels need process-aware sizing",
            "ตัวอักษรและ Label ต้องออกแบบตามกระบวนการ",
            "Embossed text that works in CAD can disappear after printing, sanding, or molding.",
            "ตัวอักษรนูนที่ดูดีใน CAD อาจหายหลังพิมพ์ ขัด หรือหล่อ",
            "Design details",
            "รายละเอียดแบบ",
            DesignPlanningImageUrl,
            "Decide whether text is decorative, functional, serialized, painted, engraved, or molded. Include minimum readability distance and whether labels can be stickers instead.",
            "ตัดสินใจว่าตัวอักษรเป็นตกแต่ง ใช้งานจริง Serial ทำสี แกะสลัก หรือขึ้นในแม่พิมพ์ ระบุระยะอ่านขั้นต่ำและยอมใช้สติกเกอร์แทนได้ไหม",
            "MALIEV checks feature height, line width, process resolution, finishing loss, and whether engraved, printed, or applied labeling is more reliable.",
            "MALIEV ตรวจความสูง Feature ความกว้างเส้น Resolution กระบวนการ การสูญเสียจากเก็บผิว และดูว่าแกะ พิมพ์ หรือแปะ Label น่าเชื่อถือกว่า",
            ("CAD-readable text is not always manufacturable.", "ตัวอักษรที่อ่านได้ใน CAD ไม่ได้ผลิตได้เสมอ"),
            ("Labels should match finishing plans.", "Label ต้องเข้ากับแผนผิว"),
            ("Use stickers when variable data changes often.", "ใช้สติกเกอร์เมื่อข้อมูลเปลี่ยนบ่อย")),
        PracticalNote(
            "small-holes-and-pins",
            "Small holes and pins need process allowance",
            "รูเล็กและ Pin ต้องเผื่อกระบวนการ",
            "Printed holes, drilled holes, and molded holes behave differently.",
            "รูพิมพ์ รูเจาะ และรูฉีดมีพฤติกรรมต่างกัน",
            "Fit",
            "การประกอบ",
            CaliperInspectionImageUrl,
            "Define whether the hole is clearance, press fit, bearing seat, dowel location, cable path, or cosmetic detail. Include pin size and fit expectation.",
            "กำหนดว่ารูเป็นรูหลวม Press fit ที่นั่งแบริ่ง ตำแหน่ง Dowel ทางสายไฟ หรือรายละเอียดโชว์ ระบุขนาด Pin และ Fit ที่คาดหวัง",
            "MALIEV reviews minimum hole size, post-drilling, reaming, tolerance, orientation, and whether the process can hold the fit directly.",
            "MALIEV ตรวจขนาดรูขั้นต่ำ การเจาะหลังผลิต คว้าน tolerance ทิศทาง และดูว่ากระบวนการคุม Fit ได้โดยตรงหรือไม่",
            ("Small holes often need post-processing.", "รูเล็กมักต้องเก็บหลังผลิต"),
            ("Fit type should be named.", "ควรระบุชนิด Fit"),
            ("Pin size belongs in the quote notes.", "ขนาด Pin ควรอยู่ในหมายเหตุราคา")),
        PracticalNote(
            "thin-tall-features",
            "Thin tall features are fragile in most processes",
            "Feature สูงบางเปราะในหลายกระบวนการ",
            "Towers, tabs, and needles can fail during build, cleanup, shipping, or assembly.",
            "เสา แท็บ และเข็มอาจเสียหายระหว่างผลิต แกะงาน ขนส่ง หรือประกอบ",
            "DFM guide",
            "คู่มือ DFM",
            DesignPlanningImageUrl,
            "Decide whether the feature is functional, cosmetic, or temporary. Add fillets, ribs, chamfers, or replaceable inserts when it must survive handling.",
            "ตัดสินใจว่า Feature นั้นใช้งานจริง โชว์ หรือชั่วคราว เพิ่ม Fillet, Rib, Chamfer หรือ Insert เปลี่ยนได้เมื่อจำเป็นต้องทนการจับ",
            "MALIEV checks process resolution, cleanup access, packing risk, and whether the feature should be machined separately or protected by a design change.",
            "MALIEV ตรวจ Resolution กระบวนการ ทางเข้าแกะงาน ความเสี่ยงแพ็ก และดูว่าควรกัดแยกหรือป้องกันด้วยการแก้แบบหรือไม่",
            ("Fragility can happen after printing too.", "ความเปราะอาจเกิดหลังพิมพ์ด้วย"),
            ("Fillets help more than sharp roots.", "Fillet ช่วยมากกว่ารากคม"),
            ("Shipping risk is part of manufacturability.", "ความเสี่ยงขนส่งเป็นส่วนหนึ่งของการผลิตได้")),
        PracticalNote(
            "part-splitting-strategy",
            "Splitting a part can improve manufacturability",
            "การแยกชิ้นช่วยให้ผลิตง่ายขึ้นได้",
            "One-piece CAD is not always the best manufacturing plan.",
            "CAD ชิ้นเดียวไม่ใช่แผนผลิตที่ดีที่สุดเสมอ",
            "DFM guide",
            "คู่มือ DFM",
            DesignPlanningImageUrl,
            "Look for trapped support, deep pockets, inaccessible finish, undercuts, or mixed tolerance requirements. Decide whether screws, adhesive, pins, or welding are acceptable.",
            "มองหา Support ที่ติดค้าง Pocket ลึก ผิวที่เข้าเก็บไม่ได้ Undercut หรือ tolerance หลายระดับ ตัดสินใจว่ายอมใช้สกรู กาว Pin หรือเชื่อมได้ไหม",
            "MALIEV reviews split lines, assembly method, tolerance stack, surface finish, and whether splitting reduces cost without hurting function.",
            "MALIEV ตรวจแนวแยก วิธีประกอบ Stack tolerance ผิวงาน และดูว่าการแยกลดต้นทุนโดยไม่เสียฟังก์ชันหรือไม่",
            ("One-piece is not automatically better.", "ชิ้นเดียวไม่ได้ดีกว่าเสมอ"),
            ("Assembly method should be part of DFM.", "วิธีประกอบควรเป็นส่วนหนึ่งของ DFM"),
            ("Split lines need acceptable locations.", "แนวแยกต้องมีตำแหน่งที่ยอมรับได้")),
        PracticalNote(
            "material-substitution",
            "Material substitution should be approved by function",
            "การเปลี่ยนวัสดุต้องอนุมัติจากหน้าที่ใช้งาน",
            "A cheaper or faster material is useful only if it still passes the job.",
            "วัสดุที่ถูกหรือเร็วกว่า มีประโยชน์เมื่อยังผ่านหน้าที่งาน",
            "Materials",
            "วัสดุ",
            EngineeringPolymerReviewImageUrl,
            "List must-have requirements and nice-to-have preferences separately: heat, stiffness, color, texture, chemicals, electrical behavior, and certification needs.",
            "แยกข้อกำหนดจำเป็นกับสิ่งที่อยากได้ เช่น ความร้อน ความแข็ง สี Texture สารเคมี พฤติกรรมไฟฟ้า และเอกสารรับรอง",
            "MALIEV can suggest substitutes only when the acceptance criteria are clear enough to protect the part from an unsafe swap.",
            "MALIEV แนะนำวัสดุทดแทนได้เมื่อเกณฑ์รับงานชัดพอที่จะป้องกันการเปลี่ยนที่ไม่ปลอดภัย",
            ("Separate must-have from preference.", "แยกสิ่งจำเป็นออกจากความชอบ"),
            ("Substitution needs acceptance criteria.", "การเปลี่ยนวัสดุต้องมีเกณฑ์รับงาน"),
            ("Faster is useful only when function remains safe.", "เร็วขึ้นมีประโยชน์เมื่อฟังก์ชันยังปลอดภัย")),
        PracticalNote(
            "cost-driver-review",
            "Find the cost driver before redesigning everything",
            "หาตัวผลักดันต้นทุนก่อนแก้แบบทั้งหมด",
            "Material volume is only one part of manufacturing cost.",
            "ปริมาตรวัสดุเป็นเพียงส่วนหนึ่งของต้นทุนผลิต",
            "Quoting",
            "การเสนอราคา",
            FactoryPipeProductionImageUrl,
            "Ask whether cost is driven by setup, build time, support cleanup, finish, inspection, material, tooling, or delivery speed.",
            "ถามว่าต้นทุนมาจากการตั้งงาน เวลา Build การแกะซัพพอร์ต ผิว การตรวจ วัสดุ ทูลลิ่ง หรือความเร็วส่งมอบ",
            "MALIEV uses quote feedback to point at the real driver so design changes target the constraint instead of randomly simplifying geometry.",
            "MALIEV ใช้ Feedback ราคาเพื่อชี้ตัวผลักดันจริง ทำให้การแก้แบบแก้ตรงข้อจำกัด ไม่ใช่ลด Geometry แบบสุ่ม",
            ("Cost is often setup or finishing, not only material.", "ต้นทุนมักอยู่ที่ตั้งงานหรือผิว ไม่ใช่แค่วัสดุ"),
            ("Redesign around the driver.", "แก้แบบตามตัวผลักดันต้นทุน"),
            ("Compare options before deleting useful features.", "เปรียบเทียบทางเลือกก่อนลบ Feature ที่มีประโยชน์")),
        PracticalNote(
            "quote-notes-that-help",
            "Good quote notes explain risk and priority",
            "หมายเหตุขอราคาที่ดีอธิบายความเสี่ยงและลำดับความสำคัญ",
            "A short note can be more valuable than another screenshot.",
            "หมายเหตุสั้นๆ อาจมีค่ากว่าภาพเพิ่มอีกหนึ่งภาพ",
            "Quoting",
            "การเสนอราคา",
            DesignPlanningImageUrl,
            "Write what the part does, what must fit, what can change, when it is needed, and what failure you are trying to avoid.",
            "เขียนว่าชิ้นงานทำอะไร ต้องประกอบกับอะไร อะไรเปลี่ยนได้ ต้องการเมื่อไร และกำลังหลีกเลี่ยงความเสียหายแบบใด",
            "MALIEV uses notes to choose the right review path, reduce clarification loops, and preserve context from quote to production order.",
            "MALIEV ใช้หมายเหตุเพื่อเลือกเส้นทางตรวจ ลดการถามกลับ และรักษาบริบทจากใบเสนอราคาถึงคำสั่งผลิต",
            ("Say what the part does.", "บอกว่าชิ้นงานทำหน้าที่อะไร"),
            ("Say what can change.", "บอกว่าสิ่งใดเปลี่ยนได้"),
            ("Say what failure you are preventing.", "บอกว่ากำลังป้องกันความเสียหายอะไร")),
        PracticalNote(
            "assembly-hardware-list",
            "Hardware lists prevent fit surprises",
            "รายการ Hardware ช่วยป้องกันปัญหาการประกอบ",
            "Screws, nuts, bearings, pins, magnets, and inserts define many part features.",
            "สกรู น็อต แบริ่ง Pin แม่เหล็ก และ Insert กำหนด Feature จำนวนมาก",
            "Assembly",
            "การประกอบ",
            CaliperInspectionImageUrl,
            "Send hardware sizes, datasheets, model numbers, or photos. Mark whether the hardware is customer-supplied or should be sourced by MALIEV.",
            "ส่งขนาด Datasheet รุ่น หรือรูป Hardware ระบุว่าเป็นของลูกค้าส่งให้หรือให้ MALIEV จัดหา",
            "The review uses hardware data to set hole size, clearance, boss design, tolerance, and assembly sequence.",
            "การตรวจใช้ข้อมูล Hardware เพื่อกำหนดขนาดรู ระยะเผื่อ แบบ Boss, tolerance และลำดับประกอบ",
            ("Hardware is part of the design input.", "Hardware เป็นข้อมูลออกแบบ"),
            ("Customer-supplied parts should be named.", "ควรระบุชิ้นส่วนที่ลูกค้าส่งให้"),
            ("Assembly sequence affects manufacturability.", "ลำดับประกอบมีผลต่อการผลิตได้")),
        PracticalNote(
            "packaging-for-custom-parts",
            "Packaging can protect the manufacturing decision",
            "บรรจุภัณฑ์ช่วยปกป้องผลลัพธ์งานผลิต",
            "Fragile geometry can pass inspection and still arrive damaged.",
            "Geometry เปราะอาจผ่านตรวจแต่เสียหายตอนถึงปลายทาง",
            "Delivery",
            "การจัดส่ง",
            FactoryPipeProductionImageUrl,
            "Tell the team if the part has sharp tips, cosmetic surfaces, calibrated edges, flexible sections, or loose accessories that need packaging attention.",
            "แจ้งทีมว่าชิ้นงานมีปลายแหลม ผิวโชว์ ขอบคาลิเบรต ส่วนยืดหยุ่น หรืออุปกรณ์แยกที่ต้องแพ็กพิเศษ",
            "MALIEV reviews packing orientation, protective inserts, labeling, and whether fragile features need temporary supports or design changes.",
            "MALIEV ตรวจทิศแพ็ก ตัวกันกระแทก Label และดูว่า Feature เปราะต้องมี Support ชั่วคราวหรือแก้แบบหรือไม่",
            ("Delivery risk starts in the design.", "ความเสี่ยงจัดส่งเริ่มจากแบบ"),
            ("Cosmetic faces need packing protection.", "ผิวโชว์ต้องป้องกันตอนแพ็ก"),
            ("Loose accessories should be listed.", "ควรระบุอุปกรณ์แยก")),
        PracticalNote(
            "prototype-vs-production-file",
            "Prototype files and production files need different discipline",
            "ไฟล์ต้นแบบและไฟล์ผลิตต้องมีวินัยต่างกัน",
            "A file that proves a concept may not be ready for repeat orders.",
            "ไฟล์ที่พิสูจน์ไอเดียได้อาจยังไม่พร้อมสั่งซ้ำ",
            "File prep",
            "เตรียมไฟล์",
            DesignPlanningImageUrl,
            "For prototypes, mark what is being tested. For production, lock revision, material, finish, tolerances, quantity, and acceptance criteria.",
            "สำหรับต้นแบบ ให้ระบุสิ่งที่กำลังทดสอบ สำหรับงานผลิต ให้ล็อก Revision วัสดุ ผิว tolerance จำนวน และเกณฑ์รับงาน",
            "MALIEV uses this distinction to decide how much DFM, documentation, inspection, and repeatability control belongs in the quote.",
            "MALIEV ใช้ความแตกต่างนี้เพื่อตัดสินระดับ DFM เอกสาร การตรวจ และการคุมความซ้ำในใบเสนอราคา",
            ("Prototype scope should state what is being learned.", "ขอบเขตต้นแบบควรบอกว่าสิ่งใดกำลังเรียนรู้"),
            ("Production files need revision control.", "ไฟล์ผลิตต้องคุม Revision"),
            ("Repeatability is a production requirement.", "ความซ้ำได้เป็นข้อกำหนดงานผลิต")),
        PracticalNote(
            "design-freeze-before-order",
            "Freeze the right things before ordering",
            "ล็อกสิ่งที่ถูกต้องก่อนสั่งผลิต",
            "Not every detail must be frozen, but the acceptance-critical details should be.",
            "ไม่จำเป็นต้องล็อกทุกอย่าง แต่จุดที่ใช้รับงานควรถูกล็อก",
            "Ordering",
            "การสั่งงาน",
            DesignPlanningImageUrl,
            "Decide which dimensions, materials, colors, finishes, and delivery dates are locked and which are still flexible.",
            "ตัดสินใจว่ามิติ วัสดุ สี ผิว และวันส่งใดล็อกแล้ว และสิ่งใดยังยืดหยุ่นได้",
            "MALIEV uses locked scope to prevent accidental quote drift and uses flexible scope to suggest lower-risk or faster alternatives.",
            "MALIEV ใช้ขอบเขตที่ล็อกเพื่อป้องกันราคาไหลโดยไม่ตั้งใจ และใช้ขอบเขตที่ยืดหยุ่นเพื่อเสนอทางเลือกที่เสี่ยงน้อยหรือเร็วกว่า",
            ("Freeze acceptance-critical details.", "ล็อกจุดที่ใช้รับงาน"),
            ("State what can still move.", "บอกว่าสิ่งใดยังเปลี่ยนได้"),
            ("Scope clarity keeps quote and production aligned.", "ขอบเขตชัดช่วยให้ราคาและผลิตตรงกัน")),
        PracticalNote(
            "when-to-use-cnc-after-printing",
            "Printed parts can still need CNC finishing",
            "ชิ้นงานพิมพ์อาจยังต้องเก็บด้วย CNC",
            "Hybrid routes can control critical holes and faces while keeping printed geometry fast.",
            "เส้นทางผสมช่วยคุมรูและผิวสำคัญ โดยยังใช้ความเร็วของงานพิมพ์",
            "Hybrid manufacturing",
            "ผลิตแบบผสม",
            ThreeDimensionalPrinterOperatorImageUrl,
            "Identify bearing seats, dowel holes, sealing faces, and surfaces that need better tolerance than the print process alone can hold.",
            "ระบุที่นั่งแบริ่ง รู Dowel ผิวซีล และผิวที่ต้องการ tolerance ดีกว่ากระบวนการพิมพ์เพียงอย่างเดียว",
            "MALIEV reviews whether printing plus drilling, reaming, tapping, or facing is faster and safer than machining the whole part from stock.",
            "MALIEV ตรวจว่าพิมพ์แล้วเจาะ คว้าน ต๊าป หรือปาดผิว เร็วและปลอดภัยกว่ากัดทั้งชิ้นจาก Stock หรือไม่",
            ("Use hybrid routes for critical features.", "ใช้เส้นทางผสมกับ Feature สำคัญ"),
            ("Printed geometry can carry noncritical shape.", "Geometry พิมพ์ใช้กับรูปทรงที่ไม่คุมมากได้"),
            ("Post-machining should be quoted explicitly.", "งานกัดหลังพิมพ์ควรระบุในราคา")),
        PracticalNote(
            "choosing-between-print-and-cast",
            "Print or cast depends on repeatability and feel",
            "เลือกพิมพ์หรือหล่อจากความซ้ำได้และสัมผัส",
            "A printed prototype can validate geometry; a casting can validate material feel and batch behavior.",
            "ต้นแบบพิมพ์ช่วยตรวจ Geometry ส่วนงานหล่อช่วยตรวจสัมผัสวัสดุและพฤติกรรมล็อต",
            "Process choice",
            "เลือกกระบวนการ",
            ThreeDimensionalPrinterImageUrl,
            "Decide whether the next question is shape, fit, material feel, color, flexibility, or repeat quantity.",
            "ตัดสินใจว่าคำถามถัดไปคือรูปร่าง การประกอบ สัมผัสวัสดุ สี ความยืดหยุ่น หรือจำนวนสั่งซ้ำ",
            "MALIEV compares print, cast, and machined routes based on the decision the part needs to answer, not only the CAD shape.",
            "MALIEV เทียบเส้นทางพิมพ์ หล่อ และกัดจากคำถามที่ชิ้นงานต้องตอบ ไม่ใช่แค่รูปทรง CAD",
            ("Use printing for fast geometry checks.", "ใช้พิมพ์เพื่อตรวจ Geometry เร็ว"),
            ("Use casting when material feel and batch behavior matter.", "ใช้หล่อเมื่อสัมผัสวัสดุและพฤติกรรมล็อตสำคัญ"),
            ("Process choice should follow the decision.", "กระบวนการควรตามการตัดสินใจ")),
        PracticalNote(
            "choosing-between-print-and-cnc",
            "Print or CNC depends on what must be controlled",
            "เลือกพิมพ์หรือ CNC จากสิ่งที่ต้องคุม",
            "Complex shape favors printing; controlled faces and holes often favor machining.",
            "รูปทรงซับซ้อนมักเหมาะกับพิมพ์ ส่วนผิวและรูที่ต้องคุมมักเหมาะกับกัด",
            "Process choice",
            "เลือกกระบวนการ",
            PipeMachiningImageUrl,
            "List the features that must be accurate: flat faces, hole patterns, bearing seats, threads, sealing faces, or only outside envelope.",
            "ระบุ Feature ที่ต้องแม่น เช่น ผิวเรียบ Pattern รู ที่นั่งแบริ่ง เกลียว ผิวซีล หรือแค่ Envelope ภายนอก",
            "MALIEV uses that feature list to compare printed, machined, and hybrid pricing before recommending the route.",
            "MALIEV ใช้รายการ Feature นั้นเทียบราคาพิมพ์ กัด และผสมก่อนแนะนำเส้นทาง",
            ("Control features drive process choice.", "Feature ที่ต้องคุมเป็นตัวเลือกกระบวนการ"),
            ("Complex external shapes can favor printing.", "รูปทรงนอกซับซ้อนอาจเหมาะกับพิมพ์"),
            ("Hybrid can be the practical middle path.", "เส้นทางผสมอาจเป็นทางกลางที่เหมาะ")),
        PracticalNote(
            "order-ready-checklist",
            "An order-ready file package reduces delay",
            "ชุดไฟล์พร้อมสั่งช่วยลดความล่าช้า",
            "CAD alone often misses material, tolerance, finish, and quantity decisions.",
            "CAD อย่างเดียวมักขาดวัสดุ tolerance ผิว และจำนวน",
            "File prep",
            "เตรียมไฟล์",
            DesignPlanningImageUrl,
            "Prepare CAD, drawing, quantity, material preference, finish, deadline, use environment, hardware list, and acceptance notes before requesting the final quote.",
            "เตรียม CAD, Drawing, จำนวน, วัสดุที่ต้องการ, ผิว, กำหนดส่ง, สภาพใช้งาน, รายการ Hardware และหมายเหตุรับงานก่อนขอราคาสุดท้าย",
            "MALIEV uses the package to reduce clarification, make DFM feedback more specific, and move accepted work into production with fewer handoffs.",
            "MALIEV ใช้ชุดข้อมูลเพื่อลดการถามกลับ ทำให้ DFM ชัดขึ้น และส่งงานที่อนุมัติเข้าผลิตโดยส่งต่อซ้ำน้อยลง",
            ("CAD plus context is faster than CAD alone.", "CAD พร้อมบริบทเร็วกว่ามีแค่ CAD"),
            ("Include quantity and deadline early.", "ใส่จำนวนและกำหนดส่งตั้งแต่ต้น"),
            ("Acceptance notes make production handoff cleaner.", "หมายเหตุรับงานทำให้ส่งต่อผลิตสะอาดขึ้น"))
    ];

    private static readonly ServiceDetailProfile ThreeDimensionalPrintingServiceDetail = new(
        Text("3D printing service details", "รายละเอียดบริการพิมพ์ 3 มิติ"),
        Text(
            "Use this section to choose the right print route, prepare files for DFM review, and understand how a quote becomes production work.",
            "ใช้ส่วนนี้เพื่อเลือกเส้นทางพิมพ์ เตรียมไฟล์ให้พร้อมตรวจ DFM และเข้าใจว่าใบเสนอราคาจะต่อไปสู่งานผลิตอย่างไร"),
        Text(
            "Prototype, validate, and produce polymer parts with the right print route.",
            "ทำต้นแบบ ตรวจแบบ และผลิตชิ้นงานโพลีเมอร์ด้วยเส้นทางพิมพ์ที่เหมาะสม"),
        Text(
            "We support FDM, SLA/resin, MJF/SLS nylon, and engineering polymer conversations for prototypes, jigs, fixtures, housings, display samples, and low-volume parts. We review manufacturability before accepting production so material, orientation, tolerance, finish, and delivery expectations stay realistic.",
            "เรารองรับงาน FDM, SLA/เรซิน, MJF/SLS ไนลอน และการเลือกโพลีเมอร์วิศวกรรมสำหรับต้นแบบ จิ๊ก ฟิกซ์เจอร์ เคส ตัวอย่างโชว์ และงานจำนวนน้อย เราตรวจความเหมาะสมในการผลิตก่อนรับงาน เพื่อให้วัสดุ ทิศทางพิมพ์ tolerance ผิวงาน และระยะส่งมอบอยู่บนความจริง"),
        [
            new(Text("STL", "STL"), Text("mesh for print-focused geometry", "mesh สำหรับงานพิมพ์โดยตรง"), Text("CAD", "CAD"), "file"),
            new(Text("STEP / STP", "STEP / STP"), Text("preferred when fit, holes, and faces matter", "เหมาะเมื่อผิวประกบ รู และมิติสำคัญ"), Text("CAD", "CAD"), "cad"),
            new(Text("OBJ", "OBJ"), Text("mesh with surface detail when needed", "mesh พร้อมรายละเอียดผิวเมื่อจำเป็น"), Text("CAD", "CAD"), "file"),
            new(Text("3MF", "3MF"), Text("print package with units and color data", "แพ็กเกจงานพิมพ์พร้อมหน่วยและสี"), Text("CAD", "CAD"), "file"),
            new(Text("IGES / IGS", "IGES / IGS"), Text("legacy surface data for review", "ข้อมูลพื้นผิวรุ่นเก่าสำหรับตรวจ"), Text("CAD", "CAD"), "cad"),
            new(Text("PDF / drawings", "PDF / Drawing"), Text("supporting tolerance, thread, and finish notes", "เอกสารประกอบ tolerance เกลียว และผิวงาน"), Text("Support", "ประกอบ"), "review")
        ],
        [
            new(
                Text("PLA, PETG, ABS / ASA, TPU, nylon", "PLA, PETG, ABS / ASA, TPU, ไนลอน"),
                Text("FDM for fast visual and functional checks, jigs, brackets, covers, and fixture trials where layer direction can be controlled.",
                    "FDM สำหรับตรวจรูปร่างและการใช้งานเร็ว จิ๊ก ขายึด ฝาครอบ และฟิกซ์เจอร์ทดลอง เมื่อต้องควบคุมทิศทางเลเยอร์"),
                Text("FDM", "FDM"),
                "thermoplastic",
                FdmThermoplasticsImageUrl),
            new(
                Text("Standard and engineering SLA resin", "เรซิน SLA ทั่วไปและเกรดวิศวกรรม"),
                Text("High-detail parts, cosmetic prototypes, small features, and fit models where smooth surfaces matter more than impact resistance.",
                    "ชิ้นงานรายละเอียดสูง ต้นแบบโชว์ ฟีเจอร์เล็ก และโมเดลประกอบ เมื่อผิวเรียบสำคัญกว่าการรับแรงกระแทก"),
                Text("SLA / resin", "SLA / เรซิน"),
                "resin",
                SlaResinImageUrl),
            new(
                Text("PA12 nylon and powder-bed routes", "PA12 ไนลอนและงานพิมพ์พาวเดอร์"),
                Text("Durable functional prototypes, snap fits, clips, housings, and low-volume parts that need strength without support scars.",
                    "ต้นแบบใช้งานจริง คลิป สแนปฟิต เคส และงานจำนวนน้อยที่ต้องการความแข็งแรงโดยไม่มีรอยซัพพอร์ตชัด"),
                Text("MJF / SLS", "MJF / SLS"),
                "nylon",
                PowderBedNylonImageUrl),
            new(
                Text("Production-grade polymer review", "การเลือกโพลีเมอร์ระดับผลิตจริง"),
                Text("Heat, UV, chemical, flexibility, and repeated-use requirements are confirmed during quotation before the part moves to production.",
                    "ยืนยันความต้องการด้านความร้อน UV สารเคมี ความยืดหยุ่น และการใช้งานซ้ำระหว่างเสนอราคา ก่อนเข้าสู่การผลิต"),
                Text("Engineering options", "ตัวเลือกวิศวกรรม"),
                "environment",
                EngineeringPolymerReviewImageUrl)
        ],
        [
            new(Text("Wall thickness and unsupported spans", "ความหนาผนังและช่วงลอยตัว"), Text("Thin walls, long bridges, and tall unsupported features change strength, surface quality, and print success.", "ผนังบาง ช่วงพาดยาว และฟีเจอร์สูงที่ไม่มีซัพพอร์ต มีผลต่อความแข็งแรง ผิวงาน และโอกาสพิมพ์สำเร็จ"), Text("", ""), "wall"),
            new(Text("Tolerances and fit-critical faces", "Tolerance และผิวประกบสำคัญ"), Text("Holes, sliding fits, press fits, mating faces, and assembly references need notes or drawings, not only a mesh.", "รู ระยะสวม ผิวประกบ และตำแหน่งอ้างอิงการประกอบต้องมีหมายเหตุหรือ Drawing ไม่ใช่มีแค่ mesh"), Text("", ""), "fit"),
            new(Text("Orientation and support marks", "ทิศทางพิมพ์และรอยซัพพอร์ต"), Text("Orientation controls visible layer lines, support scars, strength direction, and where cosmetic cleanup is needed.", "ทิศทางพิมพ์ควบคุมเส้นเลเยอร์ รอยซัพพอร์ต ทิศทางรับแรง และตำแหน่งที่ต้องเก็บผิว"), Text("", ""), "orientation"),
            new(Text("Threads, inserts, and hole strategy", "เกลียว Insert และรูปแบบรู"), Text("Call out tapped holes, heat-set inserts, clearance holes, post-machining, or drill-after-print requirements.", "ระบุรูต๊าป Insert ฝังร้อน รูหลวม รูเก็บงานหลังพิมพ์ หรือรูที่ต้องเจาะหลังผลิต"), Text("", ""), "mold"),
            new(Text("Heat, chemicals, UV, and handling", "ความร้อน สารเคมี UV และการหยิบจับ"), Text("Use environment decides whether PLA is enough or whether PETG, ASA, nylon, TPU, resin, or another route should be reviewed.", "สภาพใช้งานเป็นตัวตัดสินว่า PLA เพียงพอหรือควรตรวจ PETG, ASA, ไนลอน, TPU, เรซิน หรือเส้นทางอื่น"), Text("", ""), "chemical"),
            new(Text("Quantity and repeatability", "จำนวนและความสม่ำเสมอ"), Text("A single prototype, a small batch, and repeat orders need different orientation, QA, packaging, and lead-time assumptions.", "ต้นแบบหนึ่งชิ้น ล็อตเล็ก และงานสั่งซ้ำ ใช้สมมติฐานทิศทางพิมพ์ QA บรรจุ และระยะเวลาไม่เหมือนกัน"), Text("", ""), "quantity")
        ],
        [
            new(Text("Upload CAD and requirements", "อัปโหลด CAD และข้อกำหนด"), Text("Send files, quantity, deadline, material preference, finish notes, and what the part must prove.", "ส่งไฟล์ จำนวน กำหนดเวลา วัสดุที่ต้องการ หมายเหตุผิวงาน และสิ่งที่ชิ้นงานต้องพิสูจน์"), Text("", ""), "upload"),
            new(Text("Review DFM and route options", "ตรวจ DFM และเส้นทางผลิต"), Text("We review geometry risks and help compare FDM, resin, nylon, finish, and lead-time trade-offs.", "เราตรวจความเสี่ยง Geometry และช่วยเปรียบเทียบ FDM เรซิน ไนลอน ผิวงาน และข้อแลกเปลี่ยนด้านเวลา"), Text("", ""), "review"),
            new(Text("Confirm quote and production setup", "ยืนยันราคาและการตั้งค่างานผลิต"), Text("Approve the selected process, material, quantity, tolerance notes, and delivery expectation before production starts.", "อนุมัติกระบวนการ วัสดุ จำนวน หมายเหตุ tolerance และระยะส่งมอบก่อนเริ่มผลิต"), Text("", ""), "price"),
            new(Text("Produce, check, and deliver", "ผลิต ตรวจ และส่งมอบ"), Text("The accepted quote continues into production tracking, QA notes, photos when needed, and delivery coordination.", "ใบเสนอราคาที่อนุมัติจะต่อไปสู่การติดตามผลิต หมายเหตุ QA รูปประกอบเมื่อจำเป็น และการประสานส่งมอบ"), Text("", ""), "delivery")
        ],
        [
            Text("Intended use, load, temperature, chemicals, UV, and indoor or outdoor exposure.", "การใช้งาน แรงที่รับ อุณหภูมิ สารเคมี UV และสภาพในร่มหรือกลางแจ้ง"),
            Text("Fit-critical dimensions, mating parts, hole types, threads, inserts, and surfaces that must look good.", "มิติประกอบสำคัญ ชิ้นส่วนที่ประกบ ประเภทรู เกลียว Insert และผิวที่ต้องดูดี"),
            Text("Quantity, target lead time, color, finish, and whether one test piece is needed before a batch.", "จำนวน ระยะเวลาที่ต้องการ สี ผิวงาน และต้องการชิ้นทดสอบก่อนผลิตล็อตหรือไม่"),
            Text("STEP plus drawings when tolerance, fit, machining, or inspection matters.", "ใช้ STEP พร้อม Drawing เมื่อต้องคุม tolerance งานประกอบ งานกัดต่อ หรือการตรวจรับ")
        ]);

    private static readonly ServiceDetailProfile CncMachiningServiceDetail = new(
        Text("CNC machining service details", "รายละเอียดบริการ CNC แมชชีนนิ่ง"),
        Text(
            "Use this page to prepare manufacturable CNC work: clean CAD, material intent, tolerance control, finish expectations, and order context.",
            "ใช้หน้านี้เพื่อเตรียมงาน CNC ให้ผลิตได้จริง ทั้ง CAD วัสดุ tolerance ผิวงาน และบริบทก่อนสั่งผลิต"),
        Text(
            "Machining works best when the functional faces are identified before pricing.",
            "งานกัดจะคุมได้ดีที่สุดเมื่อระบุผิวใช้งานจริงก่อนประเมินราคา"),
        Text(
            "We review the part for stock size, tool access, setup direction, fixture risk, sharp internal corners, tolerance stack-up, and finishing needs. The answer is not just whether the part can be cut; it is which surfaces must be controlled and which surfaces should stay at practical process default.",
            "เราตรวจขนาดวัตถุดิบ ทางเข้าเครื่องมือ ทิศทางการตั้งงาน ความเสี่ยงฟิกซ์เจอร์ มุมในที่คม Stack-up ของ tolerance และผิวงาน คำตอบไม่ใช่แค่ว่ากัดได้หรือไม่ แต่คือผิวใดต้องคุม และผิวใดควรปล่อยตามค่ากระบวนการที่เหมาะสม"),
        [
            new(Text("STEP / STP", "STEP / STP"), Text("preferred for machinable faces, holes, and setup review", "เหมาะสำหรับผิวกัด รู และการตรวจทิศทางตั้งงาน"), Text("CAD", "CAD"), "cad"),
            new(Text("PDF / drawings", "PDF / Drawing"), Text("tolerances, thread callouts, finish, and inspection dimensions", "tolerance เกลียว ผิวงาน และมิติที่ต้องตรวจรับ"), Text("Control", "ควบคุม"), "review"),
            new(Text("Sample photos", "รูปตัวอย่าง"), Text("mating parts, worn parts, or fixture context when CAD is not enough", "ชิ้นส่วนประกบ ชิ้นงานสึก หรือบริบทฟิกซ์เจอร์เมื่อ CAD ไม่พอ"), Text("Reference", "อ้างอิง"), "file"),
            new(Text("Material notes", "หมายเหตุวัสดุ"), Text("grade, hardness, color, anodize, surface, or substitute material limits", "เกรด ความแข็ง สี อโนไดซ์ ผิว หรือขอบเขตวัสดุทดแทน"), Text("Spec", "สเปก"), "chemical")
        ],
        [
            new(
                Text("Aluminum, brass, and mild steel", "อะลูมิเนียม ทองเหลือง และเหล็กทั่วไป"),
                Text("Good for brackets, plates, fixtures, machine parts, heat sinks, and housings when stiffness and cut faces matter.",
                    "เหมาะกับขายึด เพลท ฟิกซ์เจอร์ ชิ้นส่วนเครื่องจักร ฮีตซิงก์ และเคส เมื่อความแข็งและผิวกัดสำคัญ"),
                Text("Metals", "โลหะ"),
                "mold",
                PipeMachiningImageUrl),
            new(
                Text("Engineering plastics", "พลาสติกวิศวกรรม"),
                Text("POM, nylon, acrylic, PC, and similar plastics are reviewed for creep, chip control, clamping marks, and edge finish.",
                    "POM ไนลอน อะคริลิก PC และพลาสติกใกล้เคียง จะตรวจเรื่องครีป เศษตัด รอยจับงาน และคุณภาพขอบ"),
                Text("Plastics", "พลาสติก"),
                "thermoplastic",
                FactoryPipeProductionImageUrl),
            new(
                Text("Fixture and tooling work", "ฟิกซ์เจอร์และทูลลิ่ง"),
                Text("Locating faces, clamp clearance, threaded inserts, dowel holes, and replaceable wear surfaces are separated before quotation.",
                    "แยกผิวกำหนดตำแหน่ง ระยะหลบ Clamp รูเกลียว รู Dowel และผิวสึกหรอที่ต้องเปลี่ยน ก่อนเสนอราคา"),
                Text("Production aids", "อุปกรณ์ช่วยผลิต"),
                "fit",
                MetalWorkshopImageUrl),
            new(
                Text("Finish and post-process review", "ผิวงานและงานหลังเครื่อง"),
                Text("Anodize, bead blast, polishing, deburring, engraving, and inspection are scoped only where they change the customer decision.",
                    "อโนไดซ์ พ่นเม็ด เก็บเงา ลบคม แกะสลัก และการตรวจ จะกำหนดเฉพาะจุดที่มีผลต่อการตัดสินใจ"),
                Text("Finish", "ผิวงาน"),
                "finish",
                CaliperInspectionImageUrl)
        ],
        [
            new(Text("Tool access and internal radius", "ทางเข้าเครื่องมือและรัศมีมุมใน"), Text("Deep pockets, narrow slots, and square internal corners affect tool size, cycle time, and whether the geometry needs relief.", "Pocket ลึก ร่องแคบ และมุมในเหลี่ยม มีผลต่อขนาดดอก เวลาเครื่อง และการเผื่อ Relief"), Text("", ""), "wall"),
            new(Text("Datums and tolerance priorities", "Datum และลำดับความสำคัญของ tolerance"), Text("Only fit-critical faces, hole patterns, and bearing surfaces should carry tight tolerance unless the whole part truly requires inspection.", "ควรกำหนด tolerance แคบเฉพาะผิวประกบ Pattern รู และผิวรับแรง เว้นแต่ทั้งชิ้นต้องตรวจจริง"), Text("", ""), "fit"),
            new(Text("Setup direction and clamping", "ทิศทางตั้งงานและการจับยึด"), Text("The quote checks whether the part can be held safely without damaging cosmetic or functional surfaces.", "ใบเสนอราคาจะตรวจว่าจับงานได้ปลอดภัยโดยไม่ทำลายผิวโชว์หรือผิวใช้งาน"), Text("", ""), "orientation"),
            new(Text("Threads, inserts, and post-machining", "เกลียว Insert และงานหลังเครื่อง"), Text("Thread depth, access, bottom condition, and insert strategy must be clear before production.", "ต้องระบุความลึกเกลียว ทางเข้า ปลายรู และกลยุทธ์ Insert ก่อนผลิต"), Text("", ""), "mold")
        ],
        [
            new(Text("Upload CAD, drawing, and use case", "อัปโหลด CAD, Drawing และวิธีใช้"), Text("Send STEP, controlled dimensions, material preference, quantity, and the part's job in the assembly.", "ส่ง STEP มิติควบคุม วัสดุที่ต้องการ จำนวน และหน้าที่ของชิ้นงานในการประกอบ"), Text("", ""), "upload"),
            new(Text("Clarify setup and tolerance risk", "ชี้แจงความเสี่ยงตั้งงานและ tolerance"), Text("MALIEV identifies missing datums, sharp corners, hard-to-hold surfaces, and places where tolerance drives price.", "MALIEV ระบุ Datum ที่ขาด มุมคม ผิวที่จับยาก และตำแหน่งที่ tolerance ดันราคา"), Text("", ""), "review"),
            new(Text("Confirm material, finish, and inspection", "ยืนยันวัสดุ ผิว และการตรวจรับ"), Text("The quote locks the route, finish, delivery expectation, and any dimensions that need inspection evidence.", "ใบเสนอราคาล็อกเส้นทาง ผิวงาน ระยะส่งมอบ และมิติที่ต้องมีหลักฐานตรวจรับ"), Text("", ""), "price"),
            new(Text("Machine, deburr, inspect, and deliver", "กัด ลบคม ตรวจ และส่งมอบ"), Text("Accepted work moves into machining, finishing notes, inspection where scoped, and delivery coordination.", "งานที่อนุมัติจะเข้าสู่งานกัด หมายเหตุผิว การตรวจตามขอบเขต และการส่งมอบ"), Text("", ""), "delivery")
        ],
        [
            Text("Send STEP plus drawings when any dimension must be held or inspected.", "ส่ง STEP พร้อม Drawing เมื่อมีมิติที่ต้องคุมหรือตรวจรับ"),
            Text("Mark mating faces, threaded holes, dowel holes, bearing seats, and cosmetic faces.", "ระบุผิวประกบ รูเกลียว รู Dowel เบ้ารับ Bearing และผิวโชว์"),
            Text("Tell us material grade, finish, quantity, deadline, and whether substitutes are acceptable.", "แจ้งเกรดวัสดุ ผิวงาน จำนวน กำหนดเวลา และยอมรับวัสดุทดแทนได้หรือไม่")
        ]);

    private static readonly ServiceDetailProfile ThreeDimensionalScanningServiceDetail = new(
        Text("3D scanning service details", "รายละเอียดบริการสแกน 3 มิติ"),
        Text(
            "Use scanning when the real object holds information that drawings, photos, or memory cannot provide.",
            "ใช้การสแกนเมื่อชิ้นงานจริงมีข้อมูลที่ Drawing รูปถ่าย หรือความจำไม่สามารถให้ได้"),
        Text(
            "The scan only helps when the purpose of capture is clear.",
            "ข้อมูลสแกนจะมีประโยชน์เมื่อรู้ชัดว่าต้องจับข้อมูลไปเพื่ออะไร"),
        Text(
            "MALIEV separates scan jobs by intent: reverse engineering, replacement parts, inspection, fit comparison, surface capture, or documentation. The output can be a raw mesh, cleaned mesh, STEP reconstruction, deviation report, or a package for downstream production.",
            "MALIEV แยกงานสแกนตามเป้าหมาย เช่น รีเวิร์สเอนจิเนียริ่ง ชิ้นส่วนทดแทน ตรวจรับงาน เทียบการประกอบ เก็บผิว หรือทำเอกสาร ผลลัพธ์อาจเป็น Mesh ดิบ Mesh ที่เก็บแล้ว STEP ที่สร้างใหม่ รายงานความคลาดเคลื่อน หรือแพ็กเกจสำหรับผลิตต่อ"),
        [
            new(Text("Part photos", "รูปชิ้นงาน"), Text("show size, finish, hidden faces, damage, and access limits", "แสดงขนาด ผิว จุดซ่อน รอยเสียหาย และข้อจำกัดการเข้าถึง"), Text("Reference", "อ้างอิง"), "file"),
            new(Text("Target output", "ผลลัพธ์ที่ต้องการ"), Text("mesh, STEP, comparison report, drawing, or production-ready file", "Mesh, STEP, รายงานเทียบ, Drawing หรือไฟล์พร้อมผลิต"), Text("Deliverable", "ผลส่งมอบ"), "review"),
            new(Text("Known dimensions", "ขนาดที่ทราบ"), Text("critical dimensions, datums, or scale checks for alignment", "มิติสำคัญ Datum หรือขนาดตรวจ Scale สำหรับจัดแนว"), Text("Control", "ควบคุม"), "fit"),
            new(Text("Use context", "บริบทการใช้งาน"), Text("replacement, inspection, archiving, fixture fit, or supplier comparison", "ทดแทน ตรวจรับ เก็บข้อมูล ประกอบฟิกซ์เจอร์ หรือเทียบ Supplier"), Text("Purpose", "เป้าหมาย"), "cad")
        ],
        [
            new(Text("Reverse engineering", "รีเวิร์สเอนจิเนียริ่ง"), Text("Raw scan data is rebuilt into cleaner CAD where holes, planes, cylinders, and datum faces need to be manufacturable.", "ข้อมูลสแกนดิบถูกสร้างเป็น CAD ที่สะอาดขึ้นเมื่อรู ระนาบ ทรงกระบอก และ Datum ต้องผลิตได้จริง"), Text("Scan to CAD", "สแกนสู่ CAD"), "cad", ThreeDimensionalScannerImageUrl),
            new(Text("Inspection and deviation maps", "ตรวจรับและแผนที่ความคลาดเคลื่อน"), Text("Compare scan data to CAD or a master part to show where the part is high, low, worn, bent, or outside tolerance.", "เทียบข้อมูลสแกนกับ CAD หรือ Master เพื่อแสดงจุดสูง ต่ำ สึก งอ หรือหลุด tolerance"), Text("Comparison", "เปรียบเทียบ"), "fit", CaliperInspectionImageUrl),
            new(Text("Replacement part capture", "เก็บข้อมูลชิ้นส่วนทดแทน"), Text("Damaged or legacy parts are captured, then intentional geometry is separated from wear and broken edges.", "ชิ้นส่วนเก่าหรือเสียหายจะถูกเก็บข้อมูล แล้วแยก Geometry ที่ตั้งใจออกจากรอยสึกและขอบแตก"), Text("Repair", "ซ่อมทดแทน"), "review", MetalWorkshopImageUrl),
            new(Text("Production handoff", "ส่งต่องานผลิต"), Text("The final package can move into 3D printing, CNC, molding review, or a controlled drawing workflow.", "แพ็กเกจสุดท้ายสามารถต่อไปยังงานพิมพ์ CNC ตรวจงานหล่อ หรือ Drawing ที่ควบคุมได้"), Text("Next process", "กระบวนการถัดไป"), "delivery", DesignPlanningImageUrl)
        ],
        [
            new(Text("Surface access and part condition", "การเข้าถึงผิวและสภาพชิ้นงาน"), Text("Glossy, transparent, black, flexible, dirty, or damaged surfaces may need preparation or scanning strategy changes.", "ผิวเงา ใส ดำ ยืดหยุ่น สกปรก หรือเสียหาย อาจต้องเตรียมผิวหรือเปลี่ยนกลยุทธ์สแกน"), Text("", ""), "finish"),
            new(Text("Scale, datums, and alignment", "Scale, Datum และการจัดแนว"), Text("Known dimensions and functional faces prevent the scan from becoming an unanchored shape.", "ขนาดที่ทราบและผิวใช้งานช่วยไม่ให้ข้อมูลสแกนเป็นรูปทรงที่ไม่มีจุดอ้างอิง"), Text("", ""), "fit"),
            new(Text("Output level", "ระดับผลลัพธ์"), Text("A raw mesh, cleaned mesh, STEP reconstruction, and deviation report solve different customer problems.", "Mesh ดิบ Mesh เก็บผิว STEP ที่สร้างใหม่ และรายงานความคลาดเคลื่อนตอบโจทย์คนละแบบ"), Text("", ""), "file"),
            new(Text("Acceptance and next step", "การรับงานและขั้นตอนถัดไป"), Text("The scan scope should name whether the customer needs evidence, production data, repair geometry, or a reorderable file.", "ขอบเขตงานสแกนควรระบุว่าต้องการหลักฐาน ข้อมูลผลิต Geometry ซ่อม หรือไฟล์ที่สั่งซ้ำได้"), Text("", ""), "review")
        ],
        [
            new(Text("Share photos and capture purpose", "ส่งรูปและเป้าหมายการสแกน"), Text("Explain whether the job is for repair, inspection, CAD rebuild, supplier check, or manufacturing handoff.", "อธิบายว่างานนี้เพื่อซ่อม ตรวจรับ สร้าง CAD เทียบ Supplier หรือส่งต่องานผลิต"), Text("", ""), "upload"),
            new(Text("Confirm access and preparation", "ยืนยันการเข้าถึงและการเตรียมชิ้นงาน"), Text("We check size, surface, hidden areas, fixtures, and whether part preparation is needed.", "เราตรวจขนาด ผิว จุดซ่อน ฟิกซ์เจอร์ และความจำเป็นในการเตรียมชิ้นงาน"), Text("", ""), "review"),
            new(Text("Capture, align, and process", "สแกน จัดแนว และประมวลผล"), Text("The data is aligned to useful references, cleaned, compared, or rebuilt depending on the agreed output.", "ข้อมูลถูกจัดแนวกับจุดอ้างอิงที่มีประโยชน์ เก็บผิว เทียบ หรือสร้างใหม่ตามผลลัพธ์ที่ตกลง"), Text("", ""), "fit"),
            new(Text("Deliver data and next recommendation", "ส่งข้อมูลและคำแนะนำถัดไป"), Text("The result includes files, notes, and the practical route for printing, CNC, design, or inspection follow-up.", "ผลลัพธ์รวมไฟล์ หมายเหตุ และเส้นทางที่เหมาะสำหรับพิมพ์ CNC ออกแบบ หรือการตรวจต่อ"), Text("", ""), "delivery")
        ],
        [
            Text("Send photos with rough dimensions and which faces cannot be missed.", "ส่งรูปพร้อมขนาดคร่าว ๆ และผิวที่ห้ามพลาด"),
            Text("State whether you need mesh, STEP, deviation report, drawing, or a file for production.", "ระบุว่าต้องการ Mesh, STEP, รายงานความคลาดเคลื่อน, Drawing หรือไฟล์เพื่อผลิต"),
            Text("Identify damaged, worn, cosmetic, or functional faces before capture.", "ระบุผิวเสียหาย สึก ผิวโชว์ หรือผิวใช้งานก่อนสแกน")
        ]);

    private static readonly ServiceDetailProfile ThreeDimensionalDesignServiceDetail = new(
        Text("3D design service details", "รายละเอียดบริการออกแบบ 3 มิติ"),
        Text(
            "Use design support when the manufacturing problem is clear but the CAD is missing, incomplete, or not ready for production.",
            "ใช้บริการออกแบบเมื่อโจทย์ผลิตชัด แต่ยังไม่มี CAD ไฟล์ยังไม่สมบูรณ์ หรือยังไม่พร้อมผลิต"),
        Text(
            "Good design work starts from the decision the prototype must answer.",
            "งานออกแบบที่ดีเริ่มจากคำตอบที่ต้นแบบต้องพิสูจน์"),
        Text(
            "We can turn sketches, photos, rough dimensions, broken samples, or existing CAD into manufacturable files. The design conversation stays tied to process choices, wall thickness, fastening, tolerance, material, finish, and what the customer needs to learn before ordering parts.",
            "MALIEV เปลี่ยนสเก็ตช์ รูปถ่าย ขนาดคร่าว ๆ ตัวอย่างเสีย หรือ CAD เดิมให้เป็นไฟล์ที่ผลิตได้ บทสนทนาออกแบบจะผูกกับกระบวนการ ความหนา การยึด tolerance วัสดุ ผิวงาน และสิ่งที่ลูกค้าต้องรู้ก่อนสั่งผลิต"),
        [
            new(Text("Sketches or photos", "สเก็ตช์หรือรูปถ่าย"), Text("rough shape, installation space, product references, and design intent", "รูปร่างคร่าว พื้นที่ติดตั้ง Reference สินค้า และเจตนาออกแบบ"), Text("Input", "ข้อมูลตั้งต้น"), "file"),
            new(Text("Existing CAD", "CAD เดิม"), Text("files that need cleanup, DFM changes, split lines, holes, or assembly fixes", "ไฟล์ที่ต้องเก็บ แก้ DFM แยกชิ้น เพิ่มรู หรือแก้ประกอบ"), Text("CAD", "CAD"), "cad"),
            new(Text("Mating parts", "ชิ้นส่วนประกบ"), Text("hardware, boards, brackets, machine interfaces, or envelopes", "Hardware, แผงวงจร, ขายึด, Interface เครื่อง หรือ Envelope"), Text("Fit", "ประกอบ"), "fit"),
            new(Text("Manufacturing target", "เป้าหมายผลิต"), Text("prototype, printed part, CNC part, molding trial, or production-ready package", "ต้นแบบ งานพิมพ์ งาน CNC งานลองหล่อ หรือแพ็กเกจพร้อมผลิต"), Text("Route", "เส้นทาง"), "review")
        ],
        [
            new(Text("CAD cleanup and rebuild", "เก็บและสร้าง CAD ใหม่"), Text("Imported files are repaired, simplified, or rebuilt so faces, holes, and assemblies can be controlled.", "ไฟล์นำเข้าจะถูกซ่อม ลดความซับซ้อน หรือสร้างใหม่ เพื่อคุมผิว รู และการประกอบได้"), Text("Modeling", "ขึ้นแบบ"), "cad", DesignPlanningImageUrl),
            new(Text("DFM-driven product changes", "แก้แบบตาม DFM"), Text("Wall thickness, ribs, bosses, screw strategy, split lines, draft, and tool access are adjusted for the intended route.", "ปรับความหนา Rib, Boss, สกรู, เส้นแยกชิ้น, Draft และทางเข้าเครื่องมือตามเส้นทางผลิต"), Text("Manufacturable", "ผลิตได้"), "wall", ThreeDimensionalPrinterOperatorImageUrl),
            new(Text("Assembly and fit development", "พัฒนา Assembly และ Fit"), Text("Clearance, mating parts, service access, cable routes, hardware, and tolerances are reviewed before prototype ordering.", "ตรวจ Clearance ชิ้นส่วนประกบ ทางซ่อม สายไฟ Hardware และ tolerance ก่อนสั่งต้นแบบ"), Text("Fit", "ประกอบ"), "fit", CaliperInspectionImageUrl),
            new(Text("Production handoff package", "แพ็กเกจส่งผลิต"), Text("Final outputs can include STEP, mesh, drawings, exploded notes, BOM references, or revision notes for quote tracking.", "ผลลัพธ์สุดท้ายอาจมี STEP, Mesh, Drawing, หมายเหตุ Exploded, อ้างอิง BOM หรือ Revision เพื่อใช้ติดตามใบเสนอราคา"), Text("Handoff", "ส่งต่อ"), "delivery", MetalWorkshopImageUrl)
        ],
        [
            new(Text("Manufacturing route", "เส้นทางผลิต"), Text("A design for FDM, resin, CNC, casting, or molding uses different wall, radius, and split-line assumptions.", "แบบสำหรับ FDM เรซิน CNC งานหล่อ หรือแม่พิมพ์ ใช้สมมติฐานผนัง รัศมี และเส้นแยกชิ้นต่างกัน"), Text("", ""), "orientation"),
            new(Text("Fit and hardware", "การประกอบและ Hardware"), Text("Screws, inserts, boards, shafts, bearings, cables, and service access must be named early.", "ต้องระบุสกรู Insert บอร์ด เพลา Bearing สายไฟ และการเข้าถึงเพื่อซ่อมตั้งแต่ต้น"), Text("", ""), "fit"),
            new(Text("Wall thickness and strength", "ความหนาและความแข็งแรง"), Text("Ribs, bosses, fillets, and local reinforcements keep the design practical without overbuilding the whole part.", "Rib, Boss, Fillet และการเสริมเฉพาะจุดช่วยให้แบบผลิตได้โดยไม่ทำให้ทั้งชิ้นใหญ่เกินจำเป็น"), Text("", ""), "wall"),
            new(Text("Revision control", "การควบคุม Revision"), Text("Design changes should preserve the decision trail so quotes and prototype rounds stay understandable.", "การแก้แบบควรเก็บเหตุผลไว้ เพื่อให้ใบเสนอราคาและรอบต้นแบบเข้าใจต่อได้"), Text("", ""), "review")
        ],
        [
            new(Text("Send the problem, not only the shape", "ส่งโจทย์ ไม่ใช่แค่รูปทรง"), Text("Share what the part must hold, cover, align, protect, display, or prove.", "บอกว่าชิ้นงานต้องยึด ครอบ จัดแนว ป้องกัน โชว์ หรือพิสูจน์อะไร"), Text("", ""), "upload"),
            new(Text("Convert intent into CAD decisions", "แปลงเจตนาเป็นการตัดสินใจ CAD"), Text("We turn constraints into wall, rib, fastener, tolerance, split, and process choices.", "เราแปลงข้อจำกัดเป็นความหนา Rib จุดยึด tolerance การแยกชิ้น และกระบวนการ"), Text("", ""), "cad"),
            new(Text("Prototype the uncertain parts", "ทำต้นแบบส่วนที่ยังไม่แน่ใจ"), Text("The first print or machined sample should answer fit, handling, surface, or assembly risk.", "ต้นแบบแรกควรตอบเรื่องประกอบ การหยิบจับ ผิว หรือความเสี่ยง Assembly"), Text("", ""), "review"),
            new(Text("Lock a production-ready package", "ล็อกแพ็กเกจพร้อมผลิต"), Text("Final files move into quotation with drawings, notes, quantities, and acceptance expectations.", "ไฟล์สุดท้ายเข้าสู่ใบเสนอราคาพร้อม Drawing หมายเหตุ จำนวน และเกณฑ์รับงาน"), Text("", ""), "delivery")
        ],
        [
            Text("Bring sketches, photos, rough dimensions, existing CAD, or the broken sample story.", "นำสเก็ตช์ รูป ขนาดคร่าว CAD เดิม หรือเรื่องของตัวอย่างเสีย"),
            Text("Tell us the target process, use environment, quantity, and what must be tested first.", "แจ้งกระบวนการเป้าหมาย สภาพใช้งาน จำนวน และสิ่งที่ต้องทดสอบก่อน"),
            Text("List mating parts, fasteners, boards, cables, moving parts, and cosmetic faces.", "ระบุชิ้นส่วนประกบ จุดยึด บอร์ด สายไฟ ชิ้นส่วนเคลื่อนที่ และผิวโชว์")
        ]);

    private static readonly ServiceDetailProfile SiliconeCastingServiceDetail = new(
        Text("Silicone casting service details", "รายละเอียดบริการหล่อซิลิโคน"),
        Text(
            "Use casting when a single prototype is not enough, but hard production tooling is still too early.",
            "ใช้การหล่อเมื่อต้นแบบหนึ่งชิ้นไม่พอ แต่ยังเร็วเกินไปสำหรับแม่พิมพ์ผลิตจริง"),
        Text(
            "Casting decisions start with the master, the mold, and what the batch must prove.",
            "การตัดสินใจงานหล่อเริ่มจาก Master แม่พิมพ์ และสิ่งที่ล็อตต้องพิสูจน์"),
        Text(
            "We review the master part, surface finish, split lines, air traps, wall sections, shrink risk, color, hardness, and quantity before accepting a cast batch. The goal is to produce useful pilot parts without pretending the route behaves like full injection molding.",
            "เราตรวจ Master ผิวงาน เส้นแยกแม่พิมพ์ จุดอากาศค้าง ความหนา ความเสี่ยงหด สี ความแข็ง และจำนวนก่อนรับล็อตหล่อ เป้าหมายคือผลิตล็อตทดลองที่ใช้ได้ โดยไม่สื่อว่ากระบวนการนี้เหมือนฉีดพลาสติกจริงทุกอย่าง"),
        [
            new(Text("Master file or part", "ไฟล์หรือชิ้น Master"), Text("CAD, printed master, machined master, or sample to duplicate", "CAD, Master พิมพ์, Master กัด หรือชิ้นตัวอย่างที่ต้องทำซ้ำ"), Text("Master", "Master"), "cad"),
            new(Text("Material behavior", "พฤติกรรมวัสดุ"), Text("soft grip, rigid shell, clear, color, shore hardness, or chemical exposure", "ผิวนิ่ม โครงแข็ง ใส สี Shore hardness หรือการสัมผัสสารเคมี"), Text("Spec", "สเปก"), "chemical"),
            new(Text("Batch quantity", "จำนวนล็อต"), Text("pilot quantity, expected repeats, approval sample, and delivery target", "จำนวนทดลอง การสั่งซ้ำ ชิ้นอนุมัติ และกำหนดส่ง"), Text("Batch", "ล็อต"), "quantity"),
            new(Text("Visible surfaces", "ผิวโชว์"), Text("surfaces that must stay clean, polished, textured, or free of split-line marks", "ผิวที่ต้องสะอาด เงา มี Texture หรือไม่มีรอยเส้นแยกแม่พิมพ์"), Text("Finish", "ผิวงาน"), "finish")
        ],
        [
            new(Text("Rigid urethane parts", "ชิ้นงานยูรีเทนแข็ง"), Text("Useful for covers, housings, short-run product shells, and pilot parts where production tooling is not ready.", "เหมาะกับฝาครอบ เคส เปลือกสินค้า และล็อตทดลองเมื่อแม่พิมพ์ผลิตยังไม่พร้อม"), Text("Short-run", "ล็อตสั้น"), "mold", InjectionMoldingLineImageUrl),
            new(Text("Soft silicone-like parts", "ชิ้นงานนิ่มคล้ายซิลิโคน"), Text("Reviewed for grip feel, shore hardness, tear risk, pigment, oil contact, and repeated handling.", "ตรวจความรู้สึกจับ Shore hardness ความเสี่ยงฉีก สี การสัมผัสน้ำมัน และการหยิบจับซ้ำ"), Text("Flexible", "ยืดหยุ่น"), "chemical", FactoryPipeProductionImageUrl),
            new(Text("Master preparation", "การเตรียม Master"), Text("The master controls surface quality. Printing, sanding, machining, sealing, or polishing may be needed before mold work.", "Master เป็นตัวกำหนดผิวงาน อาจต้องพิมพ์ ขัด กัด เคลือบ หรือเก็บเงาก่อนทำแม่พิมพ์"), Text("Tooling", "ทูลลิ่ง"), "finish", ThreeDimensionalPrinterOperatorImageUrl),
            new(Text("Pilot production review", "ตรวจล็อตทดลอง"), Text("The quote separates approval samples, batch quantity, finishing, QA, and delivery assumptions.", "ใบเสนอราคาแยกชิ้นอนุมัติ จำนวนล็อต งานเก็บผิว QA และสมมติฐานส่งมอบ"), Text("Batch", "ล็อต"), "review", CaliperInspectionImageUrl)
        ],
        [
            new(Text("Split lines and venting", "เส้นแยกแม่พิมพ์และทางระบาย"), Text("Parting lines, trapped air, undercuts, and thin sections decide whether the mold can repeat cleanly.", "เส้นแยก จุดอากาศค้าง Undercut และผนังบาง เป็นตัวตัดสินว่าแม่พิมพ์ทำซ้ำได้สะอาดหรือไม่"), Text("", ""), "orientation"),
            new(Text("Master surface quality", "คุณภาพผิว Master"), Text("The mold copies surface defects, layer lines, sanding marks, and polishing choices.", "แม่พิมพ์จะคัดลอกรอยเสีย เส้นเลเยอร์ รอยขัด และระดับความเงาของ Master"), Text("", ""), "finish"),
            new(Text("Material and hardness", "วัสดุและความแข็ง"), Text("Softness, color, clarity, heat, chemicals, and tear resistance must be selected before batch work.", "ต้องเลือกความนิ่ม สี ความใส ความร้อน สารเคมี และการฉีกขาดก่อนผลิตล็อต"), Text("", ""), "chemical"),
            new(Text("Shrink, bubbles, and acceptance", "การหด ฟอง และเกณฑ์รับงาน"), Text("Pilot casting needs realistic acceptance criteria for bubbles, flash, color, dimensions, and touch-up.", "งานหล่อล็อตทดลองต้องมีเกณฑ์รับจริงสำหรับฟอง Flash สี ขนาด และงานแต่ง"), Text("", ""), "review")
        ],
        [
            new(Text("Confirm master and use case", "ยืนยัน Master และการใช้งาน"), Text("Send CAD or sample, target material feel, visible surfaces, quantity, and delivery target.", "ส่ง CAD หรือชิ้นตัวอย่าง ความรู้สึกวัสดุเป้าหมาย ผิวโชว์ จำนวน และกำหนดส่ง"), Text("", ""), "upload"),
            new(Text("Review mold strategy", "ตรวจกลยุทธ์แม่พิมพ์"), Text("We check split line, venting, master finish, expected repeats, and risk areas.", "เราตรวจเส้นแยก ทางระบาย ผิว Master จำนวนซ้ำ และจุดเสี่ยง"), Text("", ""), "review"),
            new(Text("Approve sample and batch scope", "อนุมัติตัวอย่างและขอบเขตล็อต"), Text("The quote defines sample approval, quantity, material, finish, and inspection expectations.", "ใบเสนอราคากำหนดการอนุมัติชิ้นตัวอย่าง จำนวน วัสดุ ผิว และเกณฑ์ตรวจ"), Text("", ""), "price"),
            new(Text("Cast, finish, and deliver", "หล่อ เก็บงาน และส่งมอบ"), Text("Batch work moves through molding, trimming, finishing, QA notes, and delivery coordination.", "งานล็อตจะผ่านแม่พิมพ์ ตัดแต่ง เก็บผิว หมายเหตุ QA และประสานส่งมอบ"), Text("", ""), "delivery")
        ],
        [
            Text("Send CAD or master part photos with dimensions and visible surface priorities.", "ส่ง CAD หรือรูปชิ้น Master พร้อมขนาดและลำดับผิวโชว์"),
            Text("Tell us hardness, color, clarity, flexibility, chemicals, and target quantity.", "แจ้งความแข็ง สี ความใส ความยืดหยุ่น สารเคมี และจำนวนเป้าหมาย"),
            Text("Expect a review of parting line, bubbles, shrink, flash, and sample approval before batch work.", "คาดว่าจะมีการตรวจเส้นแยก ฟอง การหด Flash และอนุมัติตัวอย่างก่อนผลิตล็อต")
        ]);

    private static readonly ServiceDetailProfile RapidPrototypingServiceDetail = new(
        Text("Rapid prototyping service details", "รายละเอียดบริการสร้างต้นแบบรวดเร็ว"),
        Text(
            "Use rapid prototyping when the next decision matters more than picking a final process immediately.",
            "ใช้สร้างต้นแบบรวดเร็วเมื่อการตัดสินใจรอบถัดไปสำคัญกว่าการเลือกกระบวนการสุดท้ายทันที"),
        Text(
            "A prototype round should answer one clear risk at a time.",
            "ต้นแบบแต่ละรอบควรตอบความเสี่ยงหลักทีละข้อ"),
        Text(
            "MALIEV combines printing, CNC, scanning, design, finishing, and inspection to help customers test fit, appearance, assembly, handling, and production assumptions. The quote keeps the prototype goal explicit so speed does not turn into vague samples.",
            "MALIEV ผสมงานพิมพ์ CNC สแกน ออกแบบ เก็บผิว และตรวจ เพื่อช่วยลูกค้าทดสอบการประกอบ หน้าตา Assembly การหยิบจับ และสมมติฐานผลิต ใบเสนอราคาจะเก็บเป้าหมายต้นแบบให้ชัด เพื่อไม่ให้ความเร็วกลายเป็นตัวอย่างที่ตอบโจทย์กว้างเกินไป"),
        [
            new(Text("Prototype decision", "การตัดสินใจของต้นแบบ"), Text("fit, size, latch feel, appearance, load, customer demo, or installation check", "ประกอบ ขนาด ความรู้สึกสลัก หน้าตา รับแรง เดโมลูกค้า หรือตรวจติดตั้ง"), Text("Goal", "เป้าหมาย"), "review"),
            new(Text("Files and references", "ไฟล์และ Reference"), Text("CAD, photos, sketches, mating parts, drawings, or samples", "CAD รูป สเก็ตช์ ชิ้นส่วนประกบ Drawing หรือตัวอย่าง"), Text("Input", "ข้อมูลตั้งต้น"), "file"),
            new(Text("Deadline and batch need", "กำหนดเวลาและจำนวน"), Text("single proof, parallel options, approval sample, or small pilot batch", "ชิ้นพิสูจน์หนึ่งชิ้น ตัวเลือกหลายแบบ ชิ้นอนุมัติ หรือล็อตทดลองเล็ก"), Text("Schedule", "เวลา"), "delivery"),
            new(Text("Production direction", "ทิศทางผลิตจริง"), Text("whether the prototype should point toward print, CNC, casting, molding, or redesign", "ต้นแบบควรชี้ไปสู่งานพิมพ์ CNC หล่อ แม่พิมพ์ หรือแก้แบบใหม่"), Text("Next", "ถัดไป"), "cad")
        ],
        [
            new(Text("Fast printed checks", "ตรวจเร็วด้วยงานพิมพ์"), Text("FDM, resin, and nylon routes cover shape, fit, appearance, and early functional checks.", "FDM เรซิน และไนลอนครอบคลุมการตรวจรูปร่าง ประกอบ หน้าตา และฟังก์ชันเบื้องต้น"), Text("Print", "พิมพ์"), "thermoplastic", ThreeDimensionalPrinterImageUrl),
            new(Text("Machined fit and tooling aids", "งานกัดเพื่อ Fit และอุปกรณ์ช่วยผลิต"), Text("CNC is used when flatness, hole control, bearing seats, or fixture contact faces matter.", "ใช้ CNC เมื่อความเรียบ การคุมรู เบ้า Bearing หรือผิวสัมผัสฟิกซ์เจอร์สำคัญ"), Text("CNC", "CNC"), "mold", PipeMachiningImageUrl),
            new(Text("Scan, repair, and compare", "สแกน ซ่อม และเทียบ"), Text("Scanning captures existing parts or checks whether a prototype deviates from the intended geometry.", "การสแกนเก็บชิ้นงานเดิมหรือตรวจว่าต้นแบบคลาดจาก Geometry เป้าหมายแค่ไหน"), Text("Scan", "สแกน"), "fit", ThreeDimensionalScannerImageUrl),
            new(Text("Design iteration support", "ช่วยแก้แบบรอบถัดไป"), Text("DFM comments become the CAD revision brief for ribs, holes, clearances, walls, and fasteners.", "หมายเหตุ DFM กลายเป็นโจทย์แก้ CAD สำหรับ Rib รู Clearance ผนัง และจุดยึด"), Text("Design", "ออกแบบ"), "cad", DesignPlanningImageUrl)
        ],
        [
            new(Text("One decision per round", "หนึ่งการตัดสินใจต่อหนึ่งรอบ"), Text("A prototype that tests everything at once usually hides which risk was actually solved.", "ต้นแบบที่ทดสอบทุกอย่างพร้อมกันมักซ่อนว่าความเสี่ยงไหนถูกแก้จริง"), Text("", ""), "review"),
            new(Text("Fit, finish, and strength are separate", "Fit, ผิว และความแข็งแรงเป็นคนละเรื่อง"), Text("The fastest material for shape may not be the right material for impact, heat, or cosmetics.", "วัสดุที่เร็วที่สุดสำหรับรูปทรงอาจไม่เหมาะกับ Impact ความร้อน หรือหน้าตา"), Text("", ""), "chemical"),
            new(Text("Revision notes must survive", "หมายเหตุ Revision ต้องต่อเนื่อง"), Text("Each round should record what changed, what passed, and what still blocks ordering.", "แต่ละรอบควรบันทึกว่าเปลี่ยนอะไร ผ่านอะไร และอะไรยังขวางการสั่งผลิต"), Text("", ""), "file"),
            new(Text("Prototype route should not overpromise production", "เส้นทางต้นแบบไม่ควรสัญญาแทนงานผลิต"), Text("A fast sample can prove fit while the final batch still needs another process, material, or finish.", "ตัวอย่างเร็วพิสูจน์ Fit ได้ แต่ล็อตจริงอาจยังต้องใช้กระบวนการ วัสดุ หรือผิวอีกแบบ"), Text("", ""), "orientation")
        ],
        [
            new(Text("Define the prototype question", "กำหนดคำถามของต้นแบบ"), Text("State what the next sample must prove and what would count as pass or fail.", "ระบุว่าตัวอย่างถัดไปต้องพิสูจน์อะไร และอะไรนับว่าผ่านหรือไม่ผ่าน"), Text("", ""), "upload"),
            new(Text("Choose the fastest honest route", "เลือกเส้นทางเร็วที่ยังซื่อสัตย์กับโจทย์"), Text("MALIEV proposes print, CNC, design, scan, or mixed process based on the decision needed.", "MALIEV เสนอพิมพ์ CNC ออกแบบ สแกน หรือกระบวนการผสมตามคำตอบที่ต้องการ"), Text("", ""), "review"),
            new(Text("Review result and revise CAD", "ตรวจผลและแก้ CAD"), Text("Fit notes, photos, DFM comments, and measurement feedback become the next revision list.", "หมายเหตุ Fit รูปถ่าย DFM และผลวัด กลายเป็นรายการแก้รอบถัดไป"), Text("", ""), "fit"),
            new(Text("Move the proven route into ordering", "ส่งเส้นทางที่พิสูจน์แล้วเข้าสั่งผลิต"), Text("The accepted prototype direction becomes the basis for quote, batch, finishing, and delivery scope.", "ทิศทางต้นแบบที่พิสูจน์แล้วกลายเป็นฐานของใบเสนอราคา ล็อต ผิวงาน และการส่งมอบ"), Text("", ""), "price")
        ],
        [
            Text("Tell us the single decision the prototype must answer first.", "บอกการตัดสินใจข้อแรกที่ต้นแบบต้องตอบ"),
            Text("Send CAD, photos, sketches, mating parts, deadline, and target quantity.", "ส่ง CAD รูป สเก็ตช์ ชิ้นส่วนประกบ กำหนดเวลา และจำนวนเป้าหมาย"),
            Text("Separate visual review, fit review, functional review, and final production assumptions.", "แยกการตรวจหน้าตา การประกอบ ฟังก์ชัน และสมมติฐานผลิตจริง")
        ]);

    private static readonly ServiceDetailProfile DeviationAnalysisServiceDetail = new(
        Text("Deviation analysis service details", "รายละเอียดบริการวิเคราะห์ความคลาดเคลื่อน"),
        Text(
            "Use deviation analysis when a part must be accepted, rejected, repaired, or redesigned with evidence.",
            "ใช้วิเคราะห์ความคลาดเคลื่อนเมื่อชิ้นงานต้องรับงาน ปฏิเสธ ซ่อม หรือแก้แบบด้วยหลักฐาน"),
        Text(
            "A useful report starts from the datum and the decision the data must support.",
            "รายงานที่ใช้ได้เริ่มจาก Datum และการตัดสินใจที่ข้อมูลต้องรองรับ"),
        Text(
            "MALIEV aligns scan data to CAD, a master part, or functional references, then explains where the part differs and why that difference matters. The output can support supplier discussions, remake decisions, fit troubleshooting, wear checks, or production acceptance.",
            "MALIEV จัดแนวข้อมูลสแกนกับ CAD, Master หรือจุดอ้างอิงการใช้งาน แล้วอธิบายว่าชิ้นงานต่างตรงไหนและทำไมความต่างนั้นสำคัญ ผลลัพธ์ช่วยคุยกับ Supplier ตัดสินใจผลิตใหม่ แก้ปัญหา Fit ตรวจการสึก หรือรับงานผลิต"),
        [
            new(Text("Nominal reference", "ข้อมูลอ้างอิงเป้าหมาย"), Text("CAD, master part, drawing, inspection points, or expected dimensions", "CAD, Master, Drawing, จุดตรวจ หรือขนาดเป้าหมาย"), Text("Reference", "อ้างอิง"), "cad"),
            new(Text("Physical part", "ชิ้นงานจริง"), Text("sample to scan, production part, worn part, failed part, or supplier sample", "ตัวอย่างสำหรับสแกน ชิ้นผลิต ชิ้นสึก ชิ้นเสีย หรือชิ้นจาก Supplier"), Text("Scan", "สแกน"), "file"),
            new(Text("Decision threshold", "เกณฑ์ตัดสินใจ"), Text("what deviation is acceptable, what blocks assembly, and what needs evidence", "ความคลาดที่รับได้ จุดที่ขวางการประกอบ และสิ่งที่ต้องมีหลักฐาน"), Text("Acceptance", "รับงาน"), "fit"),
            new(Text("Report need", "รูปแบบรายงาน"), Text("color map, critical dimensions, section views, screenshots, or written findings", "แผนที่สี มิติสำคัญ ภาพตัด Screenshot หรือข้อสรุปเป็นข้อความ"), Text("Output", "ผลลัพธ์"), "review")
        ],
        [
            new(Text("Scan-to-CAD comparison", "เทียบสแกนกับ CAD"), Text("Color maps and point checks show whether the physical part matches nominal geometry where it matters.", "แผนที่สีและจุดตรวจแสดงว่าชิ้นงานจริงตรงกับ Geometry เป้าหมายในจุดที่สำคัญหรือไม่"), Text("CAD compare", "เทียบ CAD"), "fit", CaliperInspectionImageUrl),
            new(Text("Master-to-sample review", "เทียบ Master กับตัวอย่าง"), Text("Useful when no perfect CAD exists but an approved reference part can define the expected form.", "เหมาะเมื่อไม่มี CAD ที่สมบูรณ์ แต่มีชิ้นอ้างอิงที่อนุมัติแล้วเพื่อกำหนดรูปทรงคาดหวัง"), Text("Reference", "อ้างอิง"), "review", ThreeDimensionalScannerImageUrl),
            new(Text("Wear and failure investigation", "ตรวจการสึกและความเสียหาย"), Text("Deviation data helps separate wear, bending, shrink, assembly stress, and manufacturing error.", "ข้อมูลความคลาดช่วยแยกการสึก การงอ การหด แรงจาก Assembly และความผิดพลาดผลิต"), Text("Root cause", "สาเหตุ"), "environment", MetalWorkshopImageUrl),
            new(Text("Acceptance package", "แพ็กเกจรับงาน"), Text("Findings can be delivered as report images, annotated dimensions, notes, and recommendation for remake or release.", "ผลตรวจส่งเป็นภาพรายงาน มิติที่ Annotate หมายเหตุ และคำแนะนำให้ผลิตใหม่หรือปล่อยผ่าน"), Text("Report", "รายงาน"), "delivery", DesignPlanningImageUrl)
        ],
        [
            new(Text("Alignment method", "วิธีจัดแนว"), Text("Best-fit, datum alignment, feature alignment, and functional alignment answer different questions.", "Best-fit, Datum, Feature และ Functional alignment ตอบคำถามต่างกัน"), Text("", ""), "orientation"),
            new(Text("Critical dimensions", "มิติสำคัญ"), Text("The report should focus on dimensions that decide fit, seal, motion, assembly, or acceptance.", "รายงานควรเน้นมิติที่ตัดสินการประกอบ ซีล การเคลื่อนที่ Assembly หรือการรับงาน"), Text("", ""), "fit"),
            new(Text("Surface and scan limits", "ข้อจำกัดผิวและการสแกน"), Text("Reflective, transparent, flexible, damaged, and inaccessible areas affect confidence and must be stated.", "ผิวสะท้อน ใส ยืดหยุ่น เสียหาย และจุดที่เข้าไม่ถึง มีผลต่อความมั่นใจและต้องระบุ"), Text("", ""), "finish"),
            new(Text("Decision-ready findings", "ข้อสรุปที่ใช้ตัดสินใจได้"), Text("The report should say what changed, why it matters, and what action follows.", "รายงานควรบอกว่าสิ่งใดเปลี่ยน ทำไมสำคัญ และต้องทำอะไรต่อ"), Text("", ""), "review")
        ],
        [
            new(Text("Define the decision", "กำหนดการตัดสินใจ"), Text("Tell MALIEV whether this is for acceptance, supplier discussion, repair, remake, or redesign.", "บอก MALIEV ว่างานนี้เพื่อรับงาน คุย Supplier ซ่อม ผลิตใหม่ หรือแก้แบบ"), Text("", ""), "upload"),
            new(Text("Choose reference and alignment", "เลือกข้อมูลอ้างอิงและการจัดแนว"), Text("CAD, master part, datum features, and functional faces decide how deviation will be interpreted.", "CAD, Master, Datum และผิวใช้งาน เป็นตัวตัดสินว่าจะตีความความคลาดอย่างไร"), Text("", ""), "cad"),
            new(Text("Scan and compare the important zones", "สแกนและเทียบโซนสำคัญ"), Text("MALIEV focuses capture and analysis on areas that affect the customer's decision.", "MALIEV เน้นการเก็บและวิเคราะห์ในพื้นที่ที่มีผลต่อการตัดสินใจของลูกค้า"), Text("", ""), "fit"),
            new(Text("Deliver report and action", "ส่งรายงานและแนวทางต่อ"), Text("The output explains pass, watch, fail, remake, adjust CAD, or investigate process causes.", "ผลลัพธ์อธิบายว่าผ่าน เฝ้าระวัง ไม่ผ่าน ผลิตใหม่ แก้ CAD หรือสืบสาเหตุกระบวนการ"), Text("", ""), "delivery")
        ],
        [
            Text("Provide CAD, drawing, master part, or expected dimensions before comparison.", "ให้ CAD, Drawing, Master หรือขนาดเป้าหมายก่อนเทียบ"),
            Text("Name the datum, fit faces, and tolerance points that decide acceptance.", "ระบุ Datum ผิวประกอบ และจุด tolerance ที่ใช้ตัดสินรับงาน"),
            Text("Tell us what decision the report must support: release, reject, remake, repair, or redesign.", "บอกว่ารายงานต้องรองรับการตัดสินใจใด: ปล่อยผ่าน ปฏิเสธ ผลิตใหม่ ซ่อม หรือแก้แบบ")
        ]);

    internal static LocalizedText Text(string en, string th)
    {
        return new LocalizedText { En = en, Th = th };
    }

    private static BlogPostContent PracticalNote(
        string slug,
        string titleEn,
        string titleTh,
        string summaryEn,
        string summaryTh,
        string categoryEn,
        string categoryTh,
        string imageUrl,
        string decisionEn,
        string decisionTh,
        string reviewEn,
        string reviewTh,
        params (string En, string Th)[] takeaways)
    {
        _ = imageUrl;

        var takeawayItems = takeaways.Length > 0
            ? takeaways
            :
            [
                (summaryEn, summaryTh)
            ];
        var seoSummaryEn = summaryEn.Length >= 70
            ? summaryEn
            : $"{summaryEn} Use it to reduce quoting delay, DFM risk, and production rework.";
        var seoSummaryTh = summaryTh.Length >= 45
            ? summaryTh
            : $"{summaryTh} ใช้ลดการถามกลับ ความเสี่ยง DFM และงานแก้หลังผลิต";
        var profile = ResolvePracticalNoteProfile(slug, categoryEn);

        return new BlogPostContent(
            slug,
            Text(titleEn, titleTh),
            Text(seoSummaryEn, seoSummaryTh),
            Text(categoryEn, categoryTh),
            BlogImageUrl(slug),
            [
                Section(
                    profile.ContextTitleEn,
                    profile.ContextTitleTh,
                    $"{seoSummaryEn} {profile.ContextBodyEn} For {titleEn.ToLowerInvariant()}, the useful question is not whether the model looks finished on screen; it is what the part must prove after it is produced, handled, inspected, and installed.",
                    $"{seoSummaryTh} {profile.ContextBodyTh} สำหรับ {titleTh} คำถามที่มีประโยชน์ไม่ใช่แค่โมเดลดูเสร็จบนหน้าจอหรือไม่ แต่คือชิ้นงานต้องพิสูจน์อะไรหลังผลิต จับ ตรวจ และนำไปประกอบจริง",
                    takeawayItems),
                Section(
                    profile.DecisionTitleEn,
                    profile.DecisionTitleTh,
                    $"{decisionEn} {profile.DecisionBodyEn} Put the decision in the quote notes, drawing, or acceptance criteria so the reviewer can separate must-have requirements from preferences that can change during DFM review.",
                    $"{decisionTh} {profile.DecisionBodyTh} ใส่การตัดสินใจนี้ไว้ในหมายเหตุใบเสนอราคา Drawing หรือเกณฑ์รับงาน เพื่อให้ผู้ตรวจแยกข้อกำหนดที่ห้ามเปลี่ยนออกจากความต้องการที่ปรับได้ระหว่างตรวจ DFM",
                    ("Name the feature, face, hole, surface, material behavior, or assembly condition that decides success.", "ระบุ Feature, ผิว, รู, พฤติกรรมวัสดุ หรือสภาพประกอบที่ใช้ตัดสินความสำเร็จ"),
                    ("Include quantity, deadline, finish expectation, and use environment before comparing prices.", "ใส่จำนวน กำหนดส่ง ผิวที่คาดหวัง และสภาพใช้งานก่อนเทียบราคา")),
                Section(
                    profile.ReviewTitleEn,
                    profile.ReviewTitleTh,
                    $"{reviewEn} {profile.ReviewBodyEn} The review turns the requirement into practical checks for process route, material suitability, setup access, tolerance risk, finishing effort, inspection points, and whether a prototype or pilot batch should come before a repeat order.",
                    $"{reviewTh} {profile.ReviewBodyTh} การตรวจจะแปลงข้อกำหนดเป็นจุดตรวจจริง เช่น เส้นทางกระบวนการ ความเหมาะสมของวัสดุ ทางเข้าการจับงาน ความเสี่ยง tolerance เวลาเก็บผิว จุดตรวจรับ และควรทำต้นแบบหรือล็อตทดลองก่อนสั่งซ้ำหรือไม่",
                    ("The same geometry may need different processes when the acceptance decision changes.", "Geometry เดียวกันอาจต้องใช้กระบวนการต่างกันเมื่อเกณฑ์รับงานเปลี่ยน"),
                    ("Review should reduce ambiguity before production starts, not after the part arrives.", "การตรวจควรลดความกำกวมก่อนผลิต ไม่ใช่หลังชิ้นงานส่งถึงมือ")),
                Section(
                    profile.ExampleTitleEn,
                    profile.ExampleTitleTh,
                    $"{profile.ExampleBodyEn} A visual sample may prioritize surface and lead time. A fit-check part may prioritize holes, datums, and mating faces. A production spare may prioritize repeatability, packaging, revision control, and inspection evidence. State which decision {titleEn.ToLowerInvariant()} must support before comparing prices.",
                    $"{profile.ExampleBodyTh} ตัวอย่างโชว์งานอาจให้ความสำคัญกับผิวและเวลา ชิ้นทดสอบประกอบอาจเน้นรู Datum และผิวประกบ ส่วนอะไหล่ใช้งานซ้ำอาจเน้นความซ้ำได้ บรรจุภัณฑ์ Revision และหลักฐานตรวจรับ ควรระบุว่า {titleTh} ต้องช่วยตัดสินใจเรื่องใดก่อนเทียบราคา",
                    ("Use photos of mating parts or failed samples when CAD alone does not explain the risk.", "ใช้รูปชิ้นส่วนประกบหรือตัวอย่างเสียเมื่อ CAD อย่างเดียวอธิบายความเสี่ยงไม่พอ"),
                    ("If the part has a go/no-go dimension, call it out before the quote is accepted.", "ถ้าชิ้นงานมีมิติ Go/No-go ให้ระบุก่อนอนุมัติใบเสนอราคา")),
                Section(
                    profile.ChecklistTitleEn,
                    profile.ChecklistTitleTh,
                    $"{profile.ChecklistBodyEn} Before approving the quote, confirm the CAD revision, drawing or notes, material target, quantity, deadline, visible surfaces, critical dimensions, hardware assumptions, and acceptance criteria. This keeps the guidance connected to the real order instead of becoming general advice.",
                    $"{profile.ChecklistBodyTh} ก่อนอนุมัติใบเสนอราคา ควรยืนยัน Revision ของ CAD, Drawing หรือหมายเหตุ, วัสดุเป้าหมาย, จำนวน, กำหนดส่ง, ผิวโชว์, มิติสำคัญ, สมมติฐาน Hardware และเกณฑ์รับงาน เพื่อให้คำแนะนำเชื่อมกับคำสั่งผลิตจริง ไม่ใช่เป็นคำแนะนำทั่วไป",
                    takeawayItems)
            ],
            takeaways.Select(item => Text(item.En, item.Th)).ToArray());
    }

    private static PracticalNoteProfile ResolvePracticalNoteProfile(string slug, string categoryEn)
    {
        if (slug.StartsWith("fdm-", StringComparison.Ordinal) || slug.Contains("tpu", StringComparison.Ordinal) || slug.Contains("asa", StringComparison.Ordinal) || slug.Contains("petg", StringComparison.Ordinal) || slug.Contains("heat-set", StringComparison.Ordinal))
        {
            return new PracticalNoteProfile(
                "Start with load, heat, and layer direction",
                "เริ่มจากแรง ความร้อน และทิศเลเยอร์",
                "FDM and other thermoplastic routes are practical when the part's load path, heat exposure, screw behavior, and visible surfaces are understood before slicing. Orientation, infill, wall strategy, and post-processing are engineering choices, not decoration.",
                "งาน FDM และเทอร์โมพลาสติกใช้ได้ดีเมื่อเข้าใจทิศแรง ความร้อน พฤติกรรมสกรู และผิวโชว์ก่อนตั้งค่าพิมพ์ ทิศพิมพ์ Infill กลยุทธ์ผนัง และงานเก็บผิวเป็นการตัดสินใจเชิงวิศวกรรม ไม่ใช่ของตกแต่ง",
                "Define what the printed part must survive",
                "กำหนดว่าชิ้นงานพิมพ์ต้องทนอะไร",
                "A desk model, fixture, snap clip, outdoor bracket, and serviceable enclosure should not be priced from the same assumptions. State whether the part is pulled, bent, screwed, dropped, heated, cleaned, or exposed to sunlight.",
                "โมเดลตั้งโต๊ะ ฟิกซ์เจอร์ คลิปสแนป ขายึดกลางแจ้ง และเคสที่ต้องซ่อมบำรุง ไม่ควรถูกประเมินจากสมมติฐานเดียวกัน ควรระบุว่าชิ้นงานถูกดึง งอ ขันสกรู ตก กระทบร้อน ล้าง หรือโดนแดดหรือไม่",
                "How MALIEV reviews thermoplastic printing",
                "MALIEV ตรวจงานพิมพ์เทอร์โมพลาสติกอย่างไร",
                "The review checks layer direction, minimum wall, hole allowance, heat risk, support cleanup, insert access, and whether nylon, resin, CNC, or a hybrid route is safer for the first functional test.",
                "การตรวจจะดูทิศเลเยอร์ ความหนาต่ำสุด เผื่อรู ความเสี่ยงร้อน การแกะซัพพอร์ต ทางเข้า Insert และดูว่าไนลอน เรซิน CNC หรือเส้นทางผสมเหมาะกับการทดสอบใช้งานครั้งแรกกว่าหรือไม่",
                "Example: same shape, different FDM decision",
                "ตัวอย่าง: รูปร่างเหมือนกันแต่ตัดสินใจ FDM ต่างกัน",
                "Two covers can share the same outside shape but need different builds when one is only a visual mockup and the other holds screws beside a warm motor.",
                "ฝาครอบสองชิ้นอาจมีทรงภายนอกเหมือนกัน แต่ต้องตั้งค่างานต่างกันเมื่อชิ้นหนึ่งเป็นตัวอย่างโชว์ และอีกชิ้นต้องยึดสกรูข้างมอเตอร์ร้อน",
                "Thermoplastic order checklist",
                "เช็กลิสต์งานเทอร์โมพลาสติกก่อนสั่ง");
        }

        if (slug.Contains("resin", StringComparison.Ordinal) || slug.Contains("sla", StringComparison.Ordinal))
        {
            return new PracticalNoteProfile(
                "Separate detail, clarity, and strength",
                "แยกรายละเอียด ความใส และความแข็งแรง",
                "Resin printing can deliver fine features and smooth surfaces, but detail does not automatically mean toughness, optical clarity, or service life. The useful review starts by naming which faces are cosmetic, which dimensions must fit, and which features carry load.",
                "งานเรซินให้รายละเอียดและผิวเรียบได้ดี แต่รายละเอียดสูงไม่ได้แปลว่าทนแรง ใสระดับเลนส์ หรือใช้งานนานได้โดยอัตโนมัติ การตรวจที่ดีเริ่มจากระบุผิวโชว์ มิติประกอบ และฟีเจอร์ที่รับแรง",
                "State the resin test, not just the resin type",
                "ระบุการทดสอบของเรซิน ไม่ใช่แค่ชนิดเรซิน",
                "Tell the team whether the part is a cosmetic sample, snap-fit trial, transparent window, sealing surface, heat-exposed part, or handling prototype. Those decisions affect resin family, orientation, support placement, curing, sanding, and coating.",
                "บอกทีมว่าชิ้นงานเป็นตัวอย่างโชว์ ทดสอบสแนปฟิต หน้าต่างใส ผิวซีล ชิ้นงานโดนความร้อน หรือต้นแบบหยิบจับ เพราะสิ่งเหล่านี้มีผลต่อกลุ่มเรซิน ทิศพิมพ์ ตำแหน่งซัพพอร์ต การอบ การขัด และการเคลือบ",
                "How MALIEV reviews resin risks",
                "MALIEV ตรวจความเสี่ยงเรซินอย่างไร",
                "The review checks support scars, thin tabs, brittle clips, polishing access, trapped resin, wall thickness, and whether a nylon or machined route would be more reliable for functional testing.",
                "การตรวจจะดูรอยซัพพอร์ต แท็บบาง คลิปเปราะ ทางเข้าเก็บใส เรซินค้าง ความหนาผนัง และดูว่าไนลอนหรืองานกัดน่าเชื่อถือกว่าสำหรับการทดสอบใช้งานหรือไม่",
                "Example: a pretty resin part can still fail",
                "ตัวอย่าง: ชิ้นเรซินสวยแต่ยังล้มเหลวได้",
                "A small cover may look finished after curing, yet crack at a clip, show support marks on a visible face, or become cloudy if the required optical surface was not called out.",
                "ฝาครอบขนาดเล็กอาจดูเสร็จหลังอบ แต่แตกที่คลิป มีรอยซัพพอร์ตบนผิวโชว์ หรือขุ่นได้ถ้าไม่ได้ระบุผิว Optical ที่ต้องการ",
                "Resin order checklist",
                "เช็กลิสต์งานเรซินก่อนสั่ง");
        }

        if (slug.Contains("sls", StringComparison.Ordinal) || slug.Contains("nylon", StringComparison.Ordinal) || categoryEn.Contains("Powder", StringComparison.Ordinal))
        {
            return new PracticalNoteProfile(
                "Treat powder-bed parts as functional polymers",
                "มองงานพาวเดอร์เบดเป็นชิ้นงานโพลีเมอร์ใช้งานจริง",
                "Powder-bed nylon is useful for clips, hinges, housings, and low-volume parts because it avoids visible support scars, but the design still needs powder escape, wall control, dye expectations, and repeatability planning.",
                "ไนลอนพาวเดอร์เบดเหมาะกับคลิป บานพับ เคส และงานจำนวนน้อยเพราะไม่มีรอยซัพพอร์ตชัด แต่แบบยังต้องคุมทางออกผง ความหนาผนัง ความคาดหวังการย้อม และความสม่ำเสมอ",
                "Decide what powder, dye, and texture may do",
                "ตัดสินใจเรื่องผง สี และผิวสัมผัส",
                "Closed cavities, narrow channels, dark colors, rubbing surfaces, and customer-facing textures should be stated before quote approval. These details decide whether the part needs drain holes, blasting, dyeing, machining, or another process.",
                "โพรงปิด ช่องแคบ สีเข้ม ผิวเสียดสี และผิวที่ลูกค้าเห็น ควรถูกระบุก่อนอนุมัติราคา รายละเอียดเหล่านี้ตัดสินว่าชิ้นงานต้องมีรูระบายผง ยิงผิว ย้อมสี กัดต่อ หรือเปลี่ยนกระบวนการหรือไม่",
                "How MALIEV reviews powder-bed routes",
                "MALIEV ตรวจเส้นทางพาวเดอร์เบดอย่างไร",
                "The review checks trapped powder, minimum wall, feature cleanup, dye consistency, mating surfaces, batch nesting, and whether the first order should be a pilot batch before repeat production.",
                "การตรวจจะดูผงค้าง ความหนาขั้นต่ำ การเก็บ Feature ความสม่ำเสมอของสี ผิวประกบ การจัดงานในล็อต และควรเริ่มจากล็อตทดลองก่อนผลิตซ้ำหรือไม่",
                "Example: a nylon batch needs escape paths",
                "ตัวอย่าง: ล็อตไนลอนต้องมีทางออกผง",
                "A housing with the same external envelope can be easy to print as an open shell but risky as a sealed cavity if powder cannot be removed or verified.",
                "เคสที่มี Envelope ภายนอกเหมือนกันอาจผลิตง่ายเมื่อเป็น Shell เปิด แต่เสี่ยงเมื่อเป็นโพรงปิดที่เอาผงออกหรือตรวจยืนยันไม่ได้",
                "Powder-bed order checklist",
                "เช็กลิสต์งานพาวเดอร์เบดก่อนสั่ง");
        }

        if (slug.StartsWith("cnc-", StringComparison.Ordinal) || slug.Contains("cnc", StringComparison.Ordinal))
        {
            return new PracticalNoteProfile(
                "Identify the faces the cutter must control",
                "ระบุผิวที่เครื่องมือต้องคุม",
                "CNC work is strongest when the quote distinguishes controlled faces from default surfaces. Tool access, setup direction, internal corner radius, stock size, and tolerance stack-up affect cost more than the outside render suggests.",
                "งาน CNC ดีที่สุดเมื่อใบเสนอราคาแยกผิวที่ต้องคุมออกจากผิวค่ามาตรฐาน ทางเข้าเครื่องมือ ทิศตั้งงาน รัศมีมุมใน ขนาดวัตถุดิบ และ Stack-up ของ tolerance มีผลต่อต้นทุนมากกว่าภาพ Render ภายนอก",
                "Call out controlled dimensions and access",
                "ระบุมิติที่ต้องคุมและทางเข้าเครื่องมือ",
                "A STEP file should be paired with drawings or notes for threads, hole depths, bearing seats, sealing faces, surface finish, and inspection dimensions. Features that cannot be reached by a tool may need redesign or a second setup.",
                "ไฟล์ STEP ควรมาพร้อม Drawing หรือหมายเหตุสำหรับเกลียว ความลึกรู ที่นั่งแบริ่ง ผิวซีล ผิวงาน และมิติที่ต้องตรวจ Feature ที่เครื่องมือเข้าไม่ถึงอาจต้องแก้แบบหรือเพิ่ม Setup",
                "How MALIEV reviews CNC manufacturability",
                "MALIEV ตรวจความเหมาะสม CNC อย่างไร",
                "The review checks cutter reach, fixture strategy, pocket depth, internal radii, thread callouts, material machinability, finish effort, and whether only selected faces need tight tolerance.",
                "การตรวจจะดูระยะเข้าเครื่องมือ กลยุทธ์ฟิกซ์เจอร์ ความลึก Pocket รัศมีมุมใน หมายเหตุเกลียว ความง่ายในการกัดวัสดุ งานเก็บผิว และจุดที่ต้องคุม tolerance แคบเฉพาะตำแหน่ง",
                "Example: same bracket, different CNC cost",
                "ตัวอย่าง: ขายึดเหมือนกันแต่ต้นทุน CNC ต่างกัน",
                "A bracket can be low-risk when only its outer profile matters, but expensive when deep pockets, sharp internal corners, and multiple datum faces must all be controlled.",
                "ขายึดอาจเสี่ยงต่ำเมื่อคุมแค่รูปร่างภายนอก แต่แพงขึ้นมากเมื่อมี Pocket ลึก มุมในคม และหลายผิว Datum ที่ต้องคุมพร้อมกัน",
                "CNC order checklist",
                "เช็กลิสต์งาน CNC ก่อนสั่ง");
        }

        if (slug.Contains("injection", StringComparison.Ordinal) || slug.Contains("casting", StringComparison.Ordinal) || slug.Contains("urethane", StringComparison.Ordinal) || slug.Contains("silicone", StringComparison.Ordinal) || slug.Contains("pneumatic", StringComparison.Ordinal))
        {
            return new PracticalNoteProfile(
                "Review the mold behavior before the part",
                "ตรวจพฤติกรรมแม่พิมพ์ก่อนดูชิ้นงาน",
                "Casting and molding decisions are affected by draft, wall thickness, gates, vents, shrinkage, bubbles, master quality, and how many similar parts must be repeated. A shape that can be printed once may still be difficult to mold cleanly.",
                "งานหล่อและแม่พิมพ์ขึ้นกับ Draft ความหนาผนัง Gate Vent การหด ฟอง คุณภาพ Master และจำนวนชิ้นที่ต้องทำซ้ำ รูปทรงที่พิมพ์ครั้งเดียวได้ อาจยังทำแม่พิมพ์ให้สะอาดได้ยาก",
                "Decide batch size, surface, and release needs",
                "กำหนดจำนวนล็อต ผิว และการถอดแบบ",
                "Before quoting, state whether this is a visual sample, pilot batch, soft-touch part, functional housing, or pre-tooling bridge. The answer affects split lines, parting surfaces, vent strategy, finish, and acceptance criteria.",
                "ก่อนเสนอราคา ควรบอกว่างานนี้เป็นตัวอย่างโชว์ ล็อตทดลอง ชิ้นนิ่ม เคสใช้งาน หรือช่วงเชื่อมก่อนทำแม่พิมพ์จริง เพราะมีผลต่อเส้นแบ่งแม่พิมพ์ ผิวแยก Vent ผิวงาน และเกณฑ์รับงาน",
                "How MALIEV reviews molding and casting",
                "MALIEV ตรวจงานแม่พิมพ์และหล่ออย่างไร",
                "The review checks draft, undercuts, bubbles, vents, gate marks, shrink allowance, master finishing, expected rejects, and whether printing or machining should validate the geometry first.",
                "การตรวจจะดู Draft Undercut ฟอง Vent รอย Gate เผื่อหด งานเก็บ Master ของเสียที่คาด และควรใช้พิมพ์หรือกัดเพื่อตรวจ Geometry ก่อนหรือไม่",
                "Example: one master can define the whole batch",
                "ตัวอย่าง: Master หนึ่งชิ้นกำหนดทั้งล็อตได้",
                "A polished master can make a short run feel production-ready, while a rushed master can copy tool marks, bubbles, or dimensional errors into every cast part.",
                "Master ที่เก็บผิวดีทำให้ล็อตสั้นดูพร้อมผลิต แต่ Master ที่เร่งงานอาจคัดลอกรอยเครื่องมือ ฟอง หรือมิติผิดไปทุกชิ้นที่หล่อ",
                "Molding and casting order checklist",
                "เช็กลิสต์งานหล่อและแม่พิมพ์ก่อนสั่ง");
        }

        if (slug.StartsWith("scan", StringComparison.Ordinal) || slug.Contains("reconstruction", StringComparison.Ordinal))
        {
            return new PracticalNoteProfile(
                "Define what the scan must prove",
                "กำหนดว่าสแกนต้องพิสูจน์อะไร",
                "A scan is useful only when the required output is clear: reference mesh, inspection comparison, reverse-engineered CAD, repair geometry, or deviation evidence. Reflective, transparent, worn, flexible, and hidden surfaces all change confidence.",
                "งานสแกนมีประโยชน์เมื่อ Output ชัดเจน เช่น Mesh อ้างอิง การเทียบตรวจ CAD รีเวิร์สเอนจิเนียริ่ง Geometry ซ่อม หรือหลักฐาน Deviation ผิวสะท้อน ใส สึก ยืดหยุ่น และซ่อนอยู่ เปลี่ยนความมั่นใจของข้อมูล",
                "Choose datum, output, and confidence level",
                "เลือก Datum, Output และระดับความมั่นใจ",
                "Before quoting, state whether the result will drive acceptance, repair, redesign, replacement production, or supplier discussion. Include master parts, drawings, target dimensions, and photos of hard-to-access areas.",
                "ก่อนเสนอราคา ควรบอกว่าผลลัพธ์ใช้รับงาน ซ่อม แก้แบบ ผลิตทดแทน หรือคุย Supplier พร้อมส่ง Master, Drawing, ขนาดเป้าหมาย และรูปจุดที่เข้าถึงยาก",
                "How MALIEV reviews scan work",
                "MALIEV ตรวจงานสแกนอย่างไร",
                "The review checks alignment strategy, datum selection, surface limits, expected mesh cleanup, CAD reconstruction scope, inspection zones, and whether physical fixturing is needed during capture.",
                "การตรวจจะดูวิธีจัดแนว การเลือก Datum ข้อจำกัดผิว ขอบเขตเก็บ Mesh ขอบเขตสร้าง CAD โซนตรวจ และต้องใช้ฟิกซ์เจอร์ช่วยจับชิ้นงานตอนเก็บข้อมูลหรือไม่",
                "Example: a scan is not always CAD",
                "ตัวอย่าง: สแกนไม่ได้แปลว่าได้ CAD เสมอ",
                "A mesh can document an existing part quickly, but replacement manufacturing usually needs rebuilt surfaces, corrected damage, defined datums, and dimensions that can be inspected later.",
                "Mesh สามารถบันทึกชิ้นงานเดิมได้เร็ว แต่งานผลิตทดแทนมักต้องสร้างผิวใหม่ แยกความเสียหาย กำหนด Datum และมีมิติที่ตรวจซ้ำได้",
                "Scanning order checklist",
                "เช็กลิสต์งานสแกนก่อนสั่ง");
        }

        if (categoryEn.Contains("Assembly", StringComparison.Ordinal) || categoryEn.Contains("Enclosures", StringComparison.Ordinal) || slug.Contains("fixture", StringComparison.Ordinal) || slug.Contains("boss", StringComparison.Ordinal) || slug.Contains("hardware", StringComparison.Ordinal) || slug.Contains("enclosure", StringComparison.Ordinal) || slug.Contains("robot", StringComparison.Ordinal) || slug.Contains("maintenance", StringComparison.Ordinal))
        {
            return new PracticalNoteProfile(
                "Start from the assembly interface",
                "เริ่มจาก Interface การประกอบ",
                "Assembly parts are judged by fit, service access, hardware behavior, cable clearance, repeatability, and how quickly a technician can use the part without improvising. The CAD shape is only one part of that decision.",
                "ชิ้นงานประกอบถูกตัดสินจากการ Fit ทางเข้าเซอร์วิส พฤติกรรม Hardware ระยะสาย ความซ้ำได้ และช่างใช้งานได้เร็วโดยไม่ต้องดัดแปลงหรือไม่ รูปทรง CAD เป็นเพียงส่วนหนึ่งของการตัดสินใจ",
                "List hardware, mating parts, and service actions",
                "ระบุ Hardware ชิ้นประกบ และการซ่อมบำรุง",
                "Before upload, include screw sizes, insert types, pin fits, moving parts, cable paths, labels, load direction, and whether the part is assembled once or opened repeatedly.",
                "ก่อนอัปโหลด ให้ระบุขนาดสกรู ประเภท Insert ระยะพิน ชิ้นส่วนเคลื่อนที่ ทางเดินสาย ป้าย ทิศแรง และชิ้นงานถูกประกอบครั้งเดียวหรือเปิดซ้ำเพื่อซ่อม",
                "How MALIEV reviews assembly parts",
                "MALIEV ตรวจชิ้นงานประกอบอย่างไร",
                "The review checks bosses, snap fits, screw access, tool clearance, end-effector loads, fixture datums, cable relief, labeling, and whether printed, machined, or hybrid parts are the reliable route.",
                "การตรวจจะดู Boss, Snap-fit, ทางเข้าไขสกรู ระยะเครื่องมือ โหลด End-effector, Datum ฟิกซ์เจอร์, Cable relief, Label และดูว่าพิมพ์ กัด หรือทำแบบผสมเป็นเส้นทางที่น่าเชื่อถือกว่า",
                "Example: a fixture succeeds by repeatability",
                "ตัวอย่าง: ฟิกซ์เจอร์สำเร็จเพราะทำซ้ำได้",
                "A fixture, enclosure, or robot tool may look simple but fail if the hardware list is missing, the datum is ambiguous, or the operator cannot reach the fastener during use.",
                "ฟิกซ์เจอร์ เคส หรือเครื่องมือหุ่นยนต์อาจดูง่ายแต่ล้มเหลวได้ถ้าไม่มีรายการ Hardware, Datum กำกวม หรือผู้ปฏิบัติงานเข้าไม่ถึงสกรูตอนใช้งาน",
                "Assembly order checklist",
                "เช็กลิสต์ชิ้นงานประกอบก่อนสั่ง");
        }

        return new PracticalNoteProfile(
            "Turn the file into a manufacturing decision",
            "เปลี่ยนไฟล์ให้เป็นการตัดสินใจผลิต",
            "File preparation, pricing, process choice, production planning, packaging, and revision control all depend on context that CAD alone cannot carry. The quote should answer what the part must do, not only what the geometry looks like.",
            "การเตรียมไฟล์ ราคา การเลือกกระบวนการ แผนผลิต บรรจุภัณฑ์ และการควบคุม Revision ต้องใช้บริบทที่ CAD อย่างเดียวเก็บไม่ได้ ใบเสนอราคาควรตอบว่าชิ้นงานต้องทำอะไร ไม่ใช่แค่ Geometry เป็นอย่างไร",
            "Put the commercial and technical context together",
            "ใส่บริบทเชิงพาณิชย์และเทคนิคร่วมกัน",
            "Quantity, lead time, revision, material preference, inspection evidence, packaging, and acceptance criteria often change the best manufacturing route as much as the geometry does.",
            "จำนวน ระยะเวลา Revision วัสดุที่ต้องการ หลักฐานตรวจรับ บรรจุภัณฑ์ และเกณฑ์รับงาน มักเปลี่ยนเส้นทางผลิตที่เหมาะสมพอๆ กับ Geometry",
            "How MALIEV reviews file and order readiness",
            "MALIEV ตรวจความพร้อมไฟล์และคำสั่งอย่างไร",
            "The review checks whether the uploaded package contains the right CAD format, drawing notes, process target, material expectations, quantity, deadline, fit risk, and the decision the customer needs to make.",
            "การตรวจจะดูว่าชุดไฟล์มีรูปแบบ CAD ที่เหมาะ หมายเหตุ Drawing เป้าหมายกระบวนการ ความคาดหวังวัสดุ จำนวน กำหนดส่ง ความเสี่ยงประกอบ และการตัดสินใจที่ลูกค้าต้องการหรือไม่",
            "Example: price follows the real requirement",
            "ตัวอย่าง: ราคาตามข้อกำหนดจริง",
            "A quick visual prototype, a quote for ten customer samples, and a repeat order for spare parts can use the same CAD but require different review depth, packaging, QA evidence, and production assumptions.",
            "ต้นแบบโชว์เร็ว ตัวอย่างลูกค้า 10 ชิ้น และงานสั่งซ้ำเพื่อเป็นอะไหล่ อาจใช้ CAD เดียวกัน แต่ต้องการความลึกการตรวจ บรรจุภัณฑ์ หลักฐาน QA และสมมติฐานผลิตต่างกัน",
            "Order package checklist",
            "เช็กลิสต์ชุดข้อมูลก่อนสั่ง");
    }

    private sealed record PracticalNoteProfile(
        string ContextTitleEn,
        string ContextTitleTh,
        string ContextBodyEn,
        string ContextBodyTh,
        string DecisionTitleEn,
        string DecisionTitleTh,
        string DecisionBodyEn,
        string DecisionBodyTh,
        string ReviewTitleEn,
        string ReviewTitleTh,
        string ReviewBodyEn,
        string ReviewBodyTh,
        string ExampleTitleEn,
        string ExampleTitleTh,
        string ExampleBodyEn,
        string ExampleBodyTh,
        string ChecklistTitleEn,
        string ChecklistTitleTh,
        string ChecklistBodyEn = "Use the quote as a production handoff, not just a price snapshot.",
        string ChecklistBodyTh = "ใช้ใบเสนอราคาเป็นข้อมูลส่งต่องานผลิต ไม่ใช่แค่ภาพราคาชั่วคราว");

    private static ArticleSectionContent Section(string titleEn, string titleTh, string bodyEn, string bodyTh, params (string En, string Th)[] items)
    {
        return new ArticleSectionContent(
            Text(titleEn, titleTh),
            Text(bodyEn, bodyTh),
            items.Select(item => Text(item.En, item.Th)).ToArray());
    }

    private static string ResolveQuoteEngineUrl()
    {
        var configuredUrl = Environment.GetEnvironmentVariable("QuoteEngine__BaseUrl")
            ?? Environment.GetEnvironmentVariable("QUOTEENGINE_BASE_URL");

        return string.IsNullOrWhiteSpace(configuredUrl)
            ? DefaultQuoteEngineUrl
            : configuredUrl.TrimEnd('/');
    }

    internal static ServicePageContent GetService(string? slug)
    {
        return Services.FirstOrDefault(service => service.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase)) ?? Services[0];
    }

    internal static ServiceDetailProfile GetServiceDetail(string? slug)
    {
        var service = GetService(slug);

        return service.Slug switch
        {
            "3d-printing" => ThreeDimensionalPrintingServiceDetail,
            "cnc-machining" => CncMachiningServiceDetail,
            "3d-scanning" => ThreeDimensionalScanningServiceDetail,
            "3d-design" => ThreeDimensionalDesignServiceDetail,
            "silicone-casting" => SiliconeCastingServiceDetail,
            "rapid-prototyping" => RapidPrototypingServiceDetail,
            "deviation-analysis" => DeviationAnalysisServiceDetail,
            _ => CreateDefaultServiceDetail(service)
        };
    }

    private static ServiceDetailProfile CreateDefaultServiceDetail(ServicePageContent service)
    {
        return new ServiceDetailProfile(
            Text($"{service.Title.En} service details", $"รายละเอียดบริการ{service.Title.Th}"),
            Text(
                "Prepare process, material, files, quantity, lead time, and acceptance notes before requesting a manufacturing quote.",
                "เตรียมกระบวนการ วัสดุ ไฟล์ จำนวน ระยะเวลา และหมายเหตุรับงานก่อนขอราคางานผลิต"),
            service.ProofTitle,
            service.ProofText,
            [
                new(Text("STEP / STP", "STEP / STP"), Text("best for controlled faces and manufacturability review", "เหมาะสำหรับผิวควบคุมและการตรวจผลิต"), Text("CAD", "CAD"), "cad"),
                new(Text("STL / OBJ / 3MF", "STL / OBJ / 3MF"), Text("mesh data for print-focused or reference geometry", "ข้อมูล mesh สำหรับงานพิมพ์หรืออ้างอิง"), Text("CAD", "CAD"), "file"),
                new(Text("PDF / drawings", "PDF / Drawing"), Text("tolerance, finish, thread, and inspection notes", "หมายเหตุ tolerance ผิว เกลียว และการตรวจรับ"), Text("Support", "ประกอบ"), "review")
            ],
            service.Specs.Select(spec => new ServiceDetailItem(spec, Text("MALIEV confirms the right production route during quotation.", "MALIEV ยืนยันเส้นทางผลิตที่เหมาะสมระหว่างเสนอราคา"), Text("Capability", "ความสามารถ"), "thermoplastic")).ToArray(),
            [
                new(Text("Geometry and manufacturability", "Geometry และความเหมาะสมในการผลิต"), Text("Files are checked for features that affect process choice, setup, quality, and cost.", "ตรวจไฟล์จากฟีเจอร์ที่มีผลต่อกระบวนการ การตั้งงาน คุณภาพ และต้นทุน"), Text("", ""), "review"),
                new(Text("Material and environment", "วัสดุและสภาพใช้งาน"), Text("Use temperature, load, chemicals, surface finish, and quantity to narrow the material route.", "ใช้อุณหภูมิ แรง สารเคมี ผิวงาน และจำนวนเพื่อกรองวัสดุ"), Text("", ""), "environment"),
                new(Text("Tolerance and acceptance", "Tolerance และเกณฑ์รับงาน"), Text("Critical dimensions and visible surfaces should be called out before the order is accepted.", "ควรระบุมิติสำคัญและผิวโชว์ก่อนรับงาน"), Text("", ""), "fit")
            ],
            [
                new(Text("Upload files", "อัปโหลดไฟล์"), Text("Share CAD, drawings, photos, quantity, and the manufacturing goal.", "ส่ง CAD, Drawing, รูปถ่าย จำนวน และเป้าหมายงานผลิต"), Text("", ""), "upload"),
                new(Text("Review route", "ตรวจเส้นทางผลิต"), Text("We check process, material, DFM risks, and missing requirements.", "เราตรวจกระบวนการ วัสดุ ความเสี่ยง DFM และข้อมูลที่ขาด"), Text("", ""), "review"),
                new(Text("Approve quote", "อนุมัติราคา"), Text("Confirm the selected scope, price, and delivery expectation before production.", "ยืนยันขอบเขต ราคา และระยะส่งมอบก่อนผลิต"), Text("", ""), "price"),
                new(Text("Track production", "ติดตามผลิต"), Text("Accepted work continues into order tracking and delivery coordination.", "งานที่รับแล้วต่อไปสู่การติดตามผลิตและส่งมอบ"), Text("", ""), "delivery")
            ],
            [
                Text("CAD files plus drawings or photos when geometry alone does not explain the requirement.", "ไฟล์ CAD พร้อม Drawing หรือรูปถ่ายเมื่อ Geometry อย่างเดียวอธิบายโจทย์ไม่พอ"),
                Text("Material preference, quantity, finish, tolerance, deadline, and use environment.", "วัสดุที่ต้องการ จำนวน ผิวงาน tolerance กำหนดเวลา และสภาพใช้งาน"),
                Text("Critical surfaces, mating parts, and inspection requirements.", "ผิวสำคัญ ชิ้นส่วนที่ประกบ และข้อกำหนดการตรวจรับ")
            ]);
    }
}

internal sealed record MetricItem(LocalizedText Value, LocalizedText Label, int CountTarget, string Suffix = "");

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

internal sealed record ServiceDetailProfile(
    LocalizedText Title,
    LocalizedText Intro,
    LocalizedText BriefTitle,
    LocalizedText BriefBody,
    IReadOnlyList<ServiceDetailItem> SupportedFiles,
    IReadOnlyList<ServiceDetailItem> MaterialRoutes,
    IReadOnlyList<ServiceDetailItem> DfmGates,
    IReadOnlyList<ServiceDetailItem> OrderingSteps,
    IReadOnlyList<LocalizedText> QuoteChecklist);

internal sealed record ServiceDetailItem(
    LocalizedText Title,
    LocalizedText Body,
    LocalizedText Meta,
    string IconKey,
    string? ImageUrl = null);

internal sealed record CaseStudyContent(
    string Slug,
    LocalizedText Title,
    string Meta,
    LocalizedText Summary,
    string Stat,
    string ImageUrl,
    IReadOnlyList<ArticleSectionContent> Sections,
    IReadOnlyList<LocalizedText> Outcomes);

internal sealed record BlogPostContent(
    string Slug,
    LocalizedText Title,
    LocalizedText Summary,
    LocalizedText Category,
    string ImageUrl,
    IReadOnlyList<ArticleSectionContent> Sections,
    IReadOnlyList<LocalizedText> Takeaways);

internal sealed record ArticleSectionContent(LocalizedText Title, LocalizedText Body, IReadOnlyList<LocalizedText> Items);
