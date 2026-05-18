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
                    "Material tables help narrow the conversation, but final selection still depends on geometry, wall thickness, print orientation, quantity, finish, and acceptance criteria. For uncertain parts, order one or two test pieces before committing to a production batch.",
                    "ตารางวัสดุช่วยกรองทางเลือก แต่การเลือกสุดท้ายยังขึ้นกับ Geometry ความหนาผนัง ทิศทางพิมพ์ จำนวน ผิวงาน และเกณฑ์รับงาน สำหรับชิ้นงานที่ยังไม่แน่ใจ ควรสั่งทดสอบหนึ่งหรือสองชิ้นก่อนสั่งล็อตผลิต")
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
                    "A CAD upload can reveal volume, bounding box, wall patterns, hole features, mesh quality, and process risks. The engine uses these signals to prepare the workspace, while MALIEV can still ask for drawings, photos, or notes when geometry alone does not explain the requirement.",
                    "การอัปโหลด CAD ทำให้เห็นปริมาตร Bounding box รูปแบบผนัง Feature รู คุณภาพ Mesh และความเสี่ยงของกระบวนการ ระบบใช้สัญญาณเหล่านี้เพื่อเตรียมพื้นที่ราคา ขณะเดียวกัน MALIEV ยังอาจขอ Drawing รูป หรือหมายเหตุเมื่อ Geometry อย่างเดียวอธิบายความต้องการไม่พอ",
                    ("STEP is preferred when controlled faces and machining features matter.", "ควรใช้ STEP เมื่อผิวควบคุมและ Feature สำหรับกัดสำคัญ"),
                    ("STL, OBJ, and 3MF can work for print-focused geometry.", "STL, OBJ และ 3MF ใช้ได้กับ Geometry ที่เน้นงานพิมพ์")),
                Section(
                    "Customers can explore trade-offs before committing",
                    "ลูกค้าสำรวจข้อแลกเปลี่ยนก่อนยืนยันงานได้",
                    "Changing quantity, process, material, finish, and delivery expectation can expose the real cost driver. Sometimes a small CAD change reduces production risk more than changing material. Sometimes the same geometry needs two routes: a quick prototype and a controlled final batch.",
                    "การเปลี่ยนจำนวน กระบวนการ วัสดุ ผิวงาน และระยะส่งมอบ ช่วยให้เห็นตัวผลักดันต้นทุนจริง บางครั้งการแก้ CAD เล็กน้อยลดความเสี่ยงผลิตได้มากกว่าการเปลี่ยนวัสดุ บางครั้ง Geometry เดียวต้องใช้สองเส้นทาง คือ ต้นแบบเร็วและล็อตสุดท้ายที่ควบคุมมากขึ้น",
                    ("Use the workspace to compare, not to guess through separate emails.", "ใช้พื้นที่ราคาเพื่อเปรียบเทียบ แทนการเดาผ่านอีเมลแยกกัน"),
                    ("Keep notes with the quote so review context is not lost.", "เก็บหมายเหตุไว้กับใบเสนอราคาเพื่อไม่ให้บริบทการตรวจหาย")),
                Section(
                    "The accepted quote becomes production context",
                    "ใบเสนอราคาที่อนุมัติกลายเป็นบริบทผลิต",
                    "Once a customer accepts a quote, the selected files, material, quantity, DFM notes, lead time, and documents should continue into ordering and tracking. That continuity is what prevents vendor handoffs from becoming repeated clarification work.",
                    "เมื่อลูกค้าอนุมัติใบเสนอราคา ไฟล์ วัสดุ จำนวน หมายเหตุ DFM ระยะเวลา และเอกสารที่เลือกควรต่อเนื่องไปถึงการสั่งผลิตและติดตามงาน ความต่อเนื่องนี้ช่วยไม่ให้การส่งต่องานกลายเป็นการถามข้อมูลซ้ำ")
            ],
            [
                Text("Pricing reacts to process, material, quantity, finish, and lead time.", "ราคาตอบสนองต่อกระบวนการ วัสดุ จำนวน ผิว และระยะเวลา"),
                Text("DFM warnings protect the order from avoidable production risk.", "คำเตือน DFM ช่วยป้องกันความเสี่ยงผลิตที่หลีกเลี่ยงได้"),
                Text("Accepted quote context should carry into order tracking.", "บริบทใบเสนอราคาที่อนุมัติควรต่อไปถึงการติดตามงาน")
            ])
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
            "MALIEV supports FDM, SLA/resin, MJF/SLS nylon, and engineering polymer conversations for prototypes, jigs, fixtures, housings, display samples, and low-volume parts. We review manufacturability before accepting production so material, orientation, tolerance, finish, and delivery expectations stay realistic.",
            "MALIEV รองรับงาน FDM, SLA/เรซิน, MJF/SLS ไนลอน และการเลือกโพลีเมอร์วิศวกรรมสำหรับต้นแบบ จิ๊ก ฟิกซ์เจอร์ เคส ตัวอย่างโชว์ และงานจำนวนน้อย เราตรวจความเหมาะสมในการผลิตก่อนรับงาน เพื่อให้วัสดุ ทิศทางพิมพ์ tolerance ผิวงาน และระยะส่งมอบอยู่บนความจริง"),
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
                "thermoplastic"),
            new(
                Text("Standard and engineering SLA resin", "เรซิน SLA ทั่วไปและเกรดวิศวกรรม"),
                Text("High-detail parts, cosmetic prototypes, small features, and fit models where smooth surfaces matter more than impact resistance.",
                    "ชิ้นงานรายละเอียดสูง ต้นแบบโชว์ ฟีเจอร์เล็ก และโมเดลประกอบ เมื่อผิวเรียบสำคัญกว่าการรับแรงกระแทก"),
                Text("SLA / resin", "SLA / เรซิน"),
                "resin"),
            new(
                Text("PA12 nylon and powder-bed routes", "PA12 ไนลอนและงานพิมพ์พาวเดอร์"),
                Text("Durable functional prototypes, snap fits, clips, housings, and low-volume parts that need strength without support scars.",
                    "ต้นแบบใช้งานจริง คลิป สแนปฟิต เคส และงานจำนวนน้อยที่ต้องการความแข็งแรงโดยไม่มีรอยซัพพอร์ตชัด"),
                Text("MJF / SLS", "MJF / SLS"),
                "nylon"),
            new(
                Text("Production-grade polymer review", "การเลือกโพลีเมอร์ระดับผลิตจริง"),
                Text("Heat, UV, chemical, flexibility, and repeated-use requirements are confirmed during quotation before the part moves to production.",
                    "ยืนยันความต้องการด้านความร้อน UV สารเคมี ความยืดหยุ่น และการใช้งานซ้ำระหว่างเสนอราคา ก่อนเข้าสู่การผลิต"),
                Text("Engineering options", "ตัวเลือกวิศวกรรม"),
                "environment")
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
            new(Text("Review DFM and route options", "ตรวจ DFM และเส้นทางผลิต"), Text("MALIEV reviews geometry risks and helps compare FDM, resin, nylon, finish, and lead-time trade-offs.", "MALIEV ตรวจความเสี่ยง Geometry และช่วยเปรียบเทียบ FDM เรซิน ไนลอน ผิวงาน และข้อแลกเปลี่ยนด้านเวลา"), Text("", ""), "review"),
            new(Text("Confirm quote and production setup", "ยืนยันราคาและการตั้งค่างานผลิต"), Text("Approve the selected process, material, quantity, tolerance notes, and delivery expectation before production starts.", "อนุมัติกระบวนการ วัสดุ จำนวน หมายเหตุ tolerance และระยะส่งมอบก่อนเริ่มผลิต"), Text("", ""), "price"),
            new(Text("Produce, check, and deliver", "ผลิต ตรวจ และส่งมอบ"), Text("The accepted quote continues into production tracking, QA notes, photos when needed, and delivery coordination.", "ใบเสนอราคาที่อนุมัติจะต่อไปสู่การติดตามผลิต หมายเหตุ QA รูปประกอบเมื่อจำเป็น และการประสานส่งมอบ"), Text("", ""), "delivery")
        ],
        [
            Text("Intended use, load, temperature, chemicals, UV, and indoor or outdoor exposure.", "การใช้งาน แรงที่รับ อุณหภูมิ สารเคมี UV และสภาพในร่มหรือกลางแจ้ง"),
            Text("Fit-critical dimensions, mating parts, hole types, threads, inserts, and surfaces that must look good.", "มิติประกอบสำคัญ ชิ้นส่วนที่ประกบ ประเภทรู เกลียว Insert และผิวที่ต้องดูดี"),
            Text("Quantity, target lead time, color, finish, and whether one test piece is needed before a batch.", "จำนวน ระยะเวลาที่ต้องการ สี ผิวงาน และต้องการชิ้นทดสอบก่อนผลิตล็อตหรือไม่"),
            Text("STEP plus drawings when tolerance, fit, machining, or inspection matters.", "ใช้ STEP พร้อม Drawing เมื่อต้องคุม tolerance งานประกอบ งานกัดต่อ หรือการตรวจรับ")
        ]);

    internal static LocalizedText Text(string en, string th)
    {
        return new LocalizedText { En = en, Th = th };
    }

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

        return service.Slug.Equals("3d-printing", StringComparison.OrdinalIgnoreCase)
            ? ThreeDimensionalPrintingServiceDetail
            : CreateDefaultServiceDetail(service);
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
                new(Text("Review route", "ตรวจเส้นทางผลิต"), Text("MALIEV checks process, material, DFM risks, and missing requirements.", "MALIEV ตรวจกระบวนการ วัสดุ ความเสี่ยง DFM และข้อมูลที่ขาด"), Text("", ""), "review"),
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
    string IconKey);

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
