using System.Security.Cryptography;

using Maliev.Web.Shared.Localization;
using Microsoft.AspNetCore.WebUtilities;

namespace Maliev.Web.Client.Content;

internal static class HeroCopyCatalog
{
    internal const string DefaultTargetKey = "3d-printing";
    internal const int MinimumVariantCount = 50;

    private static readonly IReadOnlyList<string> QueryKeys =
    [
        "service",
        "target",
        "keyword",
        "utm_term",
        "utm_content",
        "utm_campaign"
    ];

    private static readonly IReadOnlyList<HeroCopyProfile> Profiles =
    [
        Profile(
            "fdm-3d-printing",
            "3d-printing",
            "?service=fdm-3d-printing",
            Text("FDM 3D Printing", "งานพิมพ์ 3 มิติ FDM"),
            ["fdm", "fdm 3d printing", "fdm 3d printing service", "รับพิมพ์ 3 มิติ fdm", "พิมพ์ fdm", "พิมพ์ 3d fdm"],
            [
                Text("FDM 3D printed prototypes", "ต้นแบบพิมพ์ 3 มิติ FDM"),
                Text("FDM parts for functional testing", "ชิ้นงาน FDM สำหรับทดสอบใช้งาน"),
                Text("FDM fixtures and jigs", "ฟิกซ์เจอร์และจิ๊กงานพิมพ์ FDM"),
                Text("FDM enclosures and brackets", "เคสและขายึดงานพิมพ์ FDM"),
                Text("FDM low-volume parts", "ชิ้นงาน FDM จำนวนน้อย"),
                Text("FDM production aids", "อุปกรณ์ช่วยผลิตจาก FDM"),
                Text("FDM PLA and PETG parts", "ชิ้นงาน PLA และ PETG แบบ FDM"),
                Text("FDM concept models", "โมเดลคอนเซ็ปต์แบบ FDM"),
                Text("FDM workshop-ready builds", "งาน FDM พร้อมผลิตในเวิร์กช็อป"),
                Text("FDM parts from your CAD", "ชิ้นงาน FDM จากไฟล์ CAD ของคุณ")
            ],
            [
                Text("priced before production", "พร้อมประเมินราคาก่อนผลิต"),
                Text("with DFM checks included", "พร้อมตรวจ DFM ในขั้นตอนเดียว"),
                Text("ready for fast iteration", "พร้อมปรับแบบและทดลองเร็ว"),
                Text("ordered from one upload", "สั่งต่อได้จากการอัปโหลดครั้งเดียว"),
                Text("matched to material and quantity", "เลือกวัสดุและจำนวนให้เหมาะกับงาน")
            ],
            [
                Text("Upload STL, STEP, OBJ, or 3MF and move from material choice to ordering in one MALIEV workflow.", "อัปโหลด STL, STEP, OBJ หรือ 3MF แล้วเลือกวัสดุและสั่งผลิตต่อได้ในระบบเดียวของ MALIEV"),
                Text("Use FDM when you need affordable size, fast fit checks, and practical parts before committing to tooling.", "เลือก FDM เมื่อต้องการขนาดที่คุ้มค่า ตรวจฟิตเร็ว และชิ้นงานใช้งานจริงก่อนลงทุนทูลลิ่ง"),
                Text("Send CAD for FDM review, compare material options, and continue when the price and lead time fit.", "ส่ง CAD เพื่อตรวจงาน FDM เปรียบเทียบวัสดุ แล้วไปต่อเมื่อราคาและระยะเวลาตรงเป้าหมาย"),
                Text("MALIEV helps route FDM prototypes, fixtures, and low-volume parts into a production-ready quote path.", "MALIEV ช่วยพางาน FDM ทั้งต้นแบบ ฟิกซ์เจอร์ และชิ้นงานจำนวนน้อยเข้าสู่เส้นทางเสนอราคาที่พร้อมผลิต"),
                Text("Keep design, DFM feedback, pricing, and order handoff in one customer flow.", "รวมการออกแบบ ผลตรวจ DFM ราคา และการส่งต่อคำสั่งผลิตไว้ในขั้นตอนเดียว")
            ]),
        Profile(
            "3d-printing",
            "3d-printing",
            "?service=3d-printing",
            Text("3D Printing", "งานพิมพ์ 3 มิติ"),
            ["3d printing", "3d print", "additive manufacturing", "รับพิมพ์ 3 มิติ", "พิมพ์ 3d", "งานพิมพ์สามมิติ"],
            [
                Text("3D printed parts for prototypes", "ชิ้นงานพิมพ์ 3 มิติสำหรับต้นแบบ"),
                Text("3D printing for low-volume runs", "งานพิมพ์ 3 มิติสำหรับผลิตจำนวนน้อย"),
                Text("Production-grade printed polymers", "โพลีเมอร์พิมพ์ 3 มิติระดับผลิตจริง"),
                Text("Printed fixtures and test parts", "ฟิกซ์เจอร์และชิ้นทดสอบจากงานพิมพ์"),
                Text("Resin, FDM, and nylon parts", "ชิ้นงานเรซิน FDM และไนลอน"),
                Text("3D printing from CAD files", "พิมพ์ 3 มิติจากไฟล์ CAD"),
                Text("Custom 3D printed components", "ชิ้นส่วนพิมพ์ 3 มิติตามแบบ"),
                Text("Engineering 3D printing support", "บริการพิมพ์ 3 มิติเชิงวิศวกรรม"),
                Text("Fast prototype printing", "พิมพ์ต้นแบบรวดเร็ว"),
                Text("3D printed parts for product teams", "ชิ้นงานพิมพ์ 3 มิติสำหรับทีมสินค้า")
            ],
            [
                Text("quoted with material choices", "เสนอราคาพร้อมตัวเลือกวัสดุ"),
                Text("checked before you order", "ตรวจความพร้อมก่อนสั่งผลิต"),
                Text("built for fit and function", "ผลิตเพื่อทดสอบการประกอบและการใช้งาน"),
                Text("routed into production ordering", "ส่งต่อเข้าสู่การสั่งผลิตได้ทันที"),
                Text("matched to finish and quantity", "จับคู่ผิวงานและจำนวนให้เหมาะสม")
            ],
            [
                Text("Upload CAD once, compare printing routes, review DFM feedback, and continue to production ordering.", "อัปโหลด CAD ครั้งเดียว เปรียบเทียบเส้นทางพิมพ์ ตรวจ DFM และสั่งผลิตต่อได้"),
                Text("Use MALIEV for prototypes, fixtures, product samples, and polymer parts that need manufacturing discipline.", "ใช้ MALIEV สำหรับต้นแบบ ฟิกซ์เจอร์ ตัวอย่างสินค้า และชิ้นงานโพลีเมอร์ที่ต้องมีวินัยแบบงานผลิต"),
                Text("Choose process, material, finish, and quantity online before committing the order.", "เลือกกระบวนการ วัสดุ ผิวงาน และจำนวนออนไลน์ก่อนยืนยันคำสั่งผลิต"),
                Text("Move from STL, STEP, OBJ, or 3MF upload to a practical quote path without vendor handoffs.", "เริ่มจาก STL, STEP, OBJ หรือ 3MF แล้วเข้าสู่เส้นทางเสนอราคาโดยไม่ต้องส่งต่อหลายที่"),
                Text("Keep iteration fast while checking the details that affect print quality and cost.", "ทำรอบทดลองให้เร็ว พร้อมตรวจรายละเอียดที่กระทบคุณภาพและต้นทุนงานพิมพ์")
            ]),
        Profile(
            "resin-3d-printing",
            "3d-printing",
            "?service=resin-3d-printing",
            Text("Resin 3D Printing", "งานพิมพ์เรซิน 3 มิติ"),
            ["resin 3d printing", "sla 3d printing", "resin print", "พิมพ์เรซิน", "พิมพ์ sla", "งานเรซิน 3 มิติ"],
            [
                Text("Resin 3D printed detail parts", "ชิ้นงานเรซิน 3 มิติรายละเอียดสูง"),
                Text("SLA prototypes with smooth finish", "ต้นแบบ SLA ผิวเนียน"),
                Text("Resin models for product review", "โมเดลเรซินสำหรับรีวิวสินค้า"),
                Text("Fine-detail printed components", "ชิ้นส่วนพิมพ์รายละเอียดละเอียด"),
                Text("Resin parts for fit checks", "ชิ้นงานเรซินสำหรับตรวจฟิต"),
                Text("Small resin production batches", "ล็อตผลิตเรซินจำนวนน้อย"),
                Text("Visual prototypes in resin", "ต้นแบบโชว์งานด้วยเรซิน"),
                Text("Resin parts from CAD", "ชิ้นงานเรซินจากไฟล์ CAD"),
                Text("High-resolution 3D prints", "งานพิมพ์ 3 มิติความละเอียดสูง"),
                Text("Smooth prototype parts", "ชิ้นงานต้นแบบผิวเรียบ")
            ],
            [
                Text("reviewed before print", "ตรวจแบบก่อนพิมพ์"),
                Text("priced with finish expectations", "ประเมินราคาพร้อมเป้าหมายผิวงาน"),
                Text("ready for product presentation", "พร้อมสำหรับนำเสนอสินค้า"),
                Text("checked for wall and detail risk", "ตรวจความเสี่ยงผนังและรายละเอียด"),
                Text("ordered from one quote flow", "สั่งต่อได้จากขั้นตอนเสนอราคาเดียว")
            ],
            [
                Text("Upload CAD and route resin work through DFM, finish expectations, pricing, and order handoff.", "อัปโหลด CAD แล้วส่งงานเรซินผ่านการตรวจ DFM เป้าหมายผิวงาน ราคา และการสั่งผลิต"),
                Text("Use resin printing when appearance, small features, and smooth prototype surfaces matter.", "เลือกพิมพ์เรซินเมื่อรูปลักษณ์ รายละเอียดเล็ก และผิวต้นแบบมีความสำคัญ"),
                Text("MALIEV helps confirm printability, detail risk, and the right production path before you order.", "MALIEV ช่วยยืนยันความพร้อมพิมพ์ ความเสี่ยงรายละเอียด และเส้นทางผลิตก่อนสั่งงาน"),
                Text("Compare resin with other 3D printing routes when strength, finish, and lead time need balance.", "เปรียบเทียบเรซินกับเส้นทางพิมพ์อื่นเมื่อจำเป็นต้องบาลานซ์ความแข็งแรง ผิวงาน และระยะเวลา"),
                Text("Keep resin prototype quoting connected to CAD upload, review, and production ordering.", "เชื่อมงานเสนอราคาต้นแบบเรซินกับการอัปโหลด CAD การตรวจ และการสั่งผลิต")
            ]),
        Profile(
            "aluminum-cnc-milling",
            "cnc-machining",
            "?service=aluminum-cnc-milling",
            Text("Aluminum CNC Milling", "งานกัด CNC อลูมิเนียม"),
            ["aluminum cnc milling", "aluminium cnc milling", "cnc aluminum", "cnc aluminium", "กัดอลูมิเนียม cnc", "cnc อลูมิเนียม", "กัดอะลูมิเนียม"],
            [
                Text("Aluminum CNC milled parts", "ชิ้นงานกัด CNC อลูมิเนียม"),
                Text("CNC aluminum brackets", "ขายึดอลูมิเนียม CNC"),
                Text("Aluminum prototypes from STEP", "ต้นแบบอลูมิเนียมจากไฟล์ STEP"),
                Text("CNC milled aluminum housings", "เคสอลูมิเนียมกัด CNC"),
                Text("Machined aluminum fixtures", "ฟิกซ์เจอร์อลูมิเนียมแมชชีนนิ่ง"),
                Text("Aluminum tooling components", "ชิ้นส่วนทูลลิ่งอลูมิเนียม"),
                Text("Precision aluminum plates", "เพลตอลูมิเนียมความแม่นยำ"),
                Text("CNC aluminum production aids", "อุปกรณ์ช่วยผลิตอลูมิเนียม CNC"),
                Text("Aluminum parts with drawings", "ชิ้นงานอลูมิเนียมพร้อม Drawing"),
                Text("CNC milled parts for engineers", "ชิ้นงานกัด CNC สำหรับวิศวกร")
            ],
            [
                Text("reviewed for tolerance", "ตรวจ tolerance ก่อนผลิต"),
                Text("quoted with setup feedback", "เสนอราคาพร้อมความเห็นเรื่องการจับงาน"),
                Text("matched to surface finish", "จับคู่กับเป้าหมายผิวงาน"),
                Text("ready for production review", "พร้อมตรวจความพร้อมก่อนผลิต"),
                Text("ordered from CAD and drawings", "สั่งต่อได้จาก CAD และ Drawing")
            ],
            [
                Text("Send STEP files, drawings, material notes, and quantities for a CNC quote path built around aluminum.", "ส่งไฟล์ STEP, Drawing, หมายเหตุวัสดุ และจำนวน เพื่อขอราคา CNC ที่ออกแบบสำหรับงานอลูมิเนียม"),
                Text("MALIEV reviews tolerance, setup, finish, and production fit before the job moves forward.", "MALIEV ตรวจ tolerance การจับงาน ผิวงาน และความเหมาะสมการผลิตก่อนเดินงานต่อ"),
                Text("Use aluminum CNC milling for brackets, housings, plates, fixtures, and functional metal prototypes.", "ใช้การกัด CNC อลูมิเนียมสำหรับขายึด เคส เพลต ฟิกซ์เจอร์ และต้นแบบโลหะใช้งานจริง"),
                Text("Keep machining questions, DFM review, price, and order handoff in one quote flow.", "รวมคำถามแมชชีนนิ่ง ตรวจ DFM ราคา และการส่งต่อคำสั่งผลิตไว้ในขั้นตอนเดียว"),
                Text("Route ad traffic directly to aluminum-focused copy while still using the same MALIEV production workflow.", "พาทราฟฟิกโฆษณาเข้าสู่ข้อความเฉพาะงานอลูมิเนียม โดยยังใช้เส้นทางผลิตเดียวของ MALIEV")
            ]),
        Profile(
            "cnc-machining",
            "cnc-machining",
            "?service=cnc-machining",
            Text("CNC Machining", "CNC แมชชีนนิ่ง"),
            ["cnc machining", "cnc milling", "machining service", "cnc service", "งาน cnc", "รับกัด cnc", "แมชชีนนิ่ง"],
            [
                Text("CNC machined parts from CAD", "ชิ้นงาน CNC จากไฟล์ CAD"),
                Text("Machined plastic and metal parts", "ชิ้นงานกัดพลาสติกและโลหะ"),
                Text("CNC brackets and fixtures", "ขายึดและฟิกซ์เจอร์ CNC"),
                Text("Precision machined prototypes", "ต้นแบบแมชชีนนิ่งความแม่นยำ"),
                Text("CNC parts with drawings", "ชิ้นงาน CNC พร้อม Drawing"),
                Text("Tooling and production aids", "ทูลลิ่งและอุปกรณ์ช่วยผลิต"),
                Text("CNC components for assemblies", "ชิ้นส่วน CNC สำหรับงานประกอบ"),
                Text("Machined engineering plastics", "พลาสติกวิศวกรรมกัด CNC"),
                Text("Short-run CNC production", "ผลิต CNC จำนวนน้อย"),
                Text("CNC review before ordering", "ตรวจงาน CNC ก่อนสั่งผลิต")
            ],
            [
                Text("checked for setup risk", "ตรวจความเสี่ยงการจับงาน"),
                Text("priced around tolerance needs", "ประเมินราคาตามความต้องการ tolerance"),
                Text("matched to material and finish", "จับคู่วัสดุและผิวงาน"),
                Text("ready for production handoff", "พร้อมส่งต่อเข้าผลิต"),
                Text("quoted from STEP and drawings", "เสนอราคาจาก STEP และ Drawing")
            ],
            [
                Text("Send CAD, drawings, material, surface, tolerance, and quantity notes for CNC quote review.", "ส่ง CAD, Drawing, วัสดุ ผิวงาน tolerance และจำนวน เพื่อให้ทีมตรวจราคา CNC"),
                Text("MALIEV helps catch manufacturability risks before machining starts.", "MALIEV ช่วยตรวจความเสี่ยงการผลิตก่อนเริ่มแมชชีนนิ่ง"),
                Text("Use CNC machining for functional prototypes, fixtures, production aids, and end-use components.", "ใช้ CNC สำหรับต้นแบบใช้งานจริง ฟิกซ์เจอร์ อุปกรณ์ช่วยผลิต และชิ้นส่วนใช้งานจริง"),
                Text("Keep process selection, review, pricing, and production ordering in one customer path.", "รวมการเลือกกระบวนการ การตรวจ ราคา และการสั่งผลิตในเส้นทางลูกค้าเดียว"),
                Text("Match machining intent to the right material, tolerance, finish, and delivery expectation.", "จับคู่เป้าหมายแมชชีนนิ่งกับวัสดุ tolerance ผิวงาน และความคาดหวังการจัดส่ง")
            ]),
        Profile(
            "3d-scanning",
            "3d-scanning",
            "?service=3d-scanning",
            Text("3D Scanning", "สแกน 3 มิติ"),
            ["3d scanning", "3d scan", "reverse engineering", "scan to cad", "สแกน 3 มิติ", "สแกนสามมิติ", "รีเวิร์สเอนจิเนียริ่ง"],
            [
                Text("3D scanning for real parts", "สแกน 3 มิติจากชิ้นงานจริง"),
                Text("Scan-to-CAD reverse engineering", "สแกนสู่ CAD เพื่อรีเวิร์สเอนจิเนียริ่ง"),
                Text("Legacy part 3D scanning", "สแกน 3 มิติชิ้นส่วนเดิม"),
                Text("Replacement parts from scans", "ชิ้นส่วนทดแทนจากข้อมูลสแกน"),
                Text("Inspection-ready scan data", "ข้อมูลสแกนพร้อมใช้ตรวจสอบ"),
                Text("3D capture for repair work", "เก็บข้อมูล 3 มิติสำหรับงานซ่อม"),
                Text("Handmade parts into CAD", "เปลี่ยนชิ้นงานทำมือเป็น CAD"),
                Text("Scan data for production review", "ข้อมูลสแกนเพื่อทบทวนการผลิต"),
                Text("Measurable references from parts", "สร้างข้อมูลอ้างอิงที่วัดได้จากชิ้นงาน"),
                Text("Reverse engineering support", "บริการช่วยรีเวิร์สเอนจิเนียริ่ง")
            ],
            [
                Text("captured before remaking", "เก็บข้อมูลก่อนผลิตใหม่"),
                Text("prepared for CAD rebuilding", "พร้อมนำไปขึ้นแบบ CAD"),
                Text("checked against critical dimensions", "ตรวจเทียบมิติสำคัญ"),
                Text("ready for repair decisions", "พร้อมใช้ตัดสินใจงานซ่อม"),
                Text("routed into design or production", "ส่งต่อสู่งานออกแบบหรือผลิต")
            ],
            [
                Text("Share photos, rough size, target output format, and why the part needs capture.", "ส่งรูป ขนาดคร่าวๆ รูปแบบไฟล์ที่ต้องการ และเหตุผลที่ต้องเก็บข้อมูลชิ้นงาน"),
                Text("Use scanning when the part exists but the CAD file, drawing, or supplier history does not.", "ใช้การสแกนเมื่อมีชิ้นงานจริงแต่ไม่มี CAD, Drawing หรือประวัติ Supplier"),
                Text("MALIEV connects scanning, CAD rebuilding, inspection, and manufacturing next steps.", "MALIEV เชื่อมการสแกน ขึ้นแบบ CAD ตรวจสอบ และขั้นตอนผลิตถัดไป"),
                Text("Capture geometry before deciding whether to repair, remake, compare, or redesign.", "เก็บ Geometry ก่อนตัดสินใจซ่อม ผลิตใหม่ เทียบผล หรือออกแบบใหม่"),
                Text("Turn physical samples into usable data for engineering and production review.", "เปลี่ยนตัวอย่างจริงเป็นข้อมูลที่ใช้ตรวจทางวิศวกรรมและการผลิตได้")
            ]),
        Profile(
            "3d-design",
            "3d-design",
            "?service=3d-design",
            Text("3D Design", "ออกแบบ 3 มิติ"),
            ["3d design", "cad design", "product design", "design for manufacturing", "ออกแบบ 3 มิติ", "ออกแบบ cad", "ขึ้นแบบ 3d"],
            [
                Text("3D design for manufacturable parts", "ออกแบบ 3 มิติให้ผลิตได้จริง"),
                Text("CAD modeling from rough ideas", "ขึ้นแบบ CAD จากไอเดียคร่าวๆ"),
                Text("Product enclosures and mechanisms", "ออกแบบเคสสินค้าและกลไก"),
                Text("DFM-ready 3D design support", "ช่วยออกแบบ 3 มิติพร้อมตรวจ DFM"),
                Text("CAD cleanup before production", "ปรับไฟล์ CAD ก่อนผลิต"),
                Text("Design fixes for prototype parts", "แก้แบบสำหรับชิ้นงานต้นแบบ"),
                Text("Manufacturing-focused CAD work", "งาน CAD ที่คิดเผื่อการผลิต"),
                Text("3D models from sketches", "โมเดล 3 มิติจากสเก็ตช์"),
                Text("Design support before quoting", "ช่วยออกแบบก่อนขอราคา"),
                Text("Prototype geometry for testing", "Geometry ต้นแบบสำหรับทดสอบ")
            ],
            [
                Text("prepared for real manufacturing", "เตรียมพร้อมสำหรับงานผลิตจริง"),
                Text("checked before upload", "ตรวจความพร้อมก่อนอัปโหลด"),
                Text("routed into printing or CNC", "ส่งต่อสู่งานพิมพ์หรือ CNC"),
                Text("refined around fit and assembly", "ปรับแบบตามการประกอบและฟิต"),
                Text("ready for quote review", "พร้อมเข้าสู่การตรวจเสนอราคา")
            ],
            [
                Text("Start with sketches, photos, dimensions, broken samples, or an unfinished CAD file.", "เริ่มจากสเก็ตช์ รูปถ่าย ขนาด ตัวอย่างแตกหัก หรือไฟล์ CAD ที่ยังไม่สมบูรณ์"),
                Text("MALIEV connects design changes to the manufacturing process that will make the part.", "MALIEV เชื่อมการแก้แบบกับกระบวนการผลิตที่จะทำชิ้นงานจริง"),
                Text("Use design support when an idea needs geometry, DFM review, and a quote-ready file.", "ใช้บริการออกแบบเมื่อไอเดียต้องการ Geometry ตรวจ DFM และไฟล์ที่พร้อมเสนอราคา"),
                Text("Move from rough concept to manufacturable CAD without separating design from production planning.", "เดินจากคอนเซ็ปต์คร่าวๆ ไปสู่ CAD ที่ผลิตได้ โดยไม่แยกการออกแบบออกจากการวางแผนผลิต"),
                Text("Improve fit, wall thickness, tolerance notes, and assembly intent before ordering.", "ปรับฟิต ความหนาผนัง หมายเหตุ tolerance และเป้าหมายการประกอบก่อนสั่งผลิต")
            ]),
        Profile(
            "silicone-casting",
            "silicone-casting",
            "?service=silicone-casting",
            Text("Silicone Casting", "หล่อซิลิโคน"),
            ["silicone casting", "urethane casting", "vacuum casting", "rapid mold", "หล่อซิลิโคน", "หล่อยูรีเทน", "แม่พิมพ์เร็ว"],
            [
                Text("Silicone casting for pilot batches", "หล่อซิลิโคนสำหรับล็อตทดลอง"),
                Text("Urethane parts before hard tooling", "ชิ้นงานยูรีเทนก่อนทำแม่พิมพ์จริง"),
                Text("Small-batch cast parts", "ชิ้นงานหล่อจำนวนน้อย"),
                Text("Rapid molds for prototype runs", "แม่พิมพ์เร็วสำหรับล็อตต้นแบบ"),
                Text("Casting bridge to production", "งานหล่อเพื่อเชื่อมสู่การผลิตจริง"),
                Text("Soft and rigid cast components", "ชิ้นงานหล่อทั้งนิ่มและแข็ง"),
                Text("Short-run silicone-like parts", "ชิ้นงานคล้ายซิลิโคนจำนวนน้อย"),
                Text("Prototype casting from CAD", "หล่อต้นแบบจากไฟล์ CAD"),
                Text("Casting for repeated samples", "งานหล่อสำหรับตัวอย่างหลายชิ้น"),
                Text("Rapid tooling quote support", "ช่วยเสนอราคาเส้นทางแม่พิมพ์เร็ว")
            ],
            [
                Text("planned before tooling spend", "วางแผนก่อนลงทุนทูลลิ่ง"),
                Text("quoted around quantity and material", "ประเมินราคาตามจำนวนและวัสดุ"),
                Text("ready for pilot validation", "พร้อมทดสอบล็อตทดลอง"),
                Text("bridged from prototype to batch", "เชื่อมจากต้นแบบสู่ล็อตผลิต"),
                Text("reviewed for mold strategy", "ตรวจกลยุทธ์แม่พิมพ์ก่อนผลิต")
            ],
            [
                Text("Use casting when one prototype needs to become many similar test parts.", "ใช้งานหล่อเมื่อชิ้นต้นแบบหนึ่งชิ้นต้องกลายเป็นชิ้นทดสอบหลายชิ้นที่คล้ายกัน"),
                Text("MALIEV reviews part geometry, material behavior, quantity, and the rapid tooling path.", "MALIEV ตรวจ Geometry วัสดุ จำนวน และเส้นทางแม่พิมพ์เร็ว"),
                Text("Bridge the gap between a printed prototype and expensive production tooling.", "เชื่อมช่องว่างระหว่างต้นแบบพิมพ์ 3 มิติและแม่พิมพ์ผลิตจริงที่มีต้นทุนสูง"),
                Text("Share CAD, target feel, quantity, and deadline so the right casting path can be priced.", "ส่ง CAD ความรู้สึกผิวหรือความแข็งที่ต้องการ จำนวน และกำหนดเวลา เพื่อประเมินเส้นทางหล่อที่เหมาะสม"),
                Text("Keep casting decisions tied to DFM review, pricing, and production handoff.", "ผูกการตัดสินใจงานหล่อกับการตรวจ DFM ราคา และการส่งต่อผลิต")
            ]),
        Profile(
            "rapid-prototyping",
            "rapid-prototyping",
            "?service=rapid-prototyping",
            Text("Rapid Prototyping", "สร้างต้นแบบรวดเร็ว"),
            ["rapid prototyping", "prototype service", "prototype manufacturing", "สร้างต้นแบบ", "ทำต้นแบบรวดเร็ว", "รับทำ prototype"],
            [
                Text("Rapid prototypes for product teams", "ต้นแบบรวดเร็วสำหรับทีมสินค้า"),
                Text("Prototype parts ready for testing", "ชิ้นงานต้นแบบพร้อมทดสอบ"),
                Text("Multi-process prototype builds", "ต้นแบบหลายกระบวนการ"),
                Text("Fast design iteration support", "ช่วยทดลองแบบอย่างรวดเร็ว"),
                Text("Prototype fixtures and samples", "ฟิกซ์เจอร์และตัวอย่างต้นแบบ"),
                Text("From idea to usable prototype", "จากไอเดียสู่ต้นแบบที่ใช้งานได้"),
                Text("Prototype geometry from CAD", "Geometry ต้นแบบจาก CAD"),
                Text("Engineering prototypes with review", "ต้นแบบวิศวกรรมพร้อมตรวจ"),
                Text("Proof-of-concept manufacturing", "ผลิตชิ้นงานพิสูจน์คอนเซ็ปต์"),
                Text("Prototype path before production", "เส้นทางต้นแบบก่อนผลิตจริง")
            ],
            [
                Text("built around your deadline", "วางเส้นทางตามกำหนดเวลาของคุณ"),
                Text("routed through the right process", "เลือกกระบวนการที่เหมาะกับงาน"),
                Text("checked before each iteration", "ตรวจความพร้อมก่อนทดลองแต่ละรอบ"),
                Text("priced before you commit", "เห็นราคาก่อนตัดสินใจ"),
                Text("ready for the next build", "พร้อมไปต่อสู่รอบผลิตถัดไป")
            ],
            [
                Text("Combine printing, CNC, scanning, design, and finishing around the test you need to run.", "ผสมงานพิมพ์ CNC สแกน ออกแบบ และตกแต่ง ตามการทดสอบที่คุณต้องทำ"),
                Text("Share the product goal, must-fit dimensions, target route, and deadline.", "แจ้งเป้าหมายสินค้า ขนาดสำคัญ เส้นทางผลิตที่คาดหวัง และกำหนดเวลา"),
                Text("MALIEV helps prototype teams move quickly without skipping manufacturability review.", "MALIEV ช่วยให้ทีมต้นแบบเดินเร็วโดยไม่ข้ามการตรวจความเหมาะสมการผลิต"),
                Text("Use rapid prototyping when the next decision depends on a real part in hand.", "ใช้สร้างต้นแบบรวดเร็วเมื่อการตัดสินใจถัดไปต้องอาศัยชิ้นงานจริง"),
                Text("Keep every build connected to quote, DFM, material, and production decisions.", "เชื่อมทุกรอบทดลองกับราคา DFM วัสดุ และการตัดสินใจผลิต")
            ]),
        Profile(
            "deviation-analysis",
            "deviation-analysis",
            "?service=deviation-analysis",
            Text("Deviation Analysis", "วิเคราะห์ความคลาดเคลื่อน"),
            ["deviation analysis", "scan inspection", "scan to cad comparison", "dimensional inspection", "วิเคราะห์ความคลาดเคลื่อน", "ตรวจเทียบสแกน", "รายงานวัดชิ้นงาน"],
            [
                Text("Deviation analysis for real parts", "วิเคราะห์ความคลาดเคลื่อนจากชิ้นงานจริง"),
                Text("Scan-to-CAD inspection reports", "รายงานตรวจเทียบสแกนกับ CAD"),
                Text("Dimensional evidence before acceptance", "หลักฐานมิติก่อนรับงาน"),
                Text("Part comparison for suppliers", "ตรวจเทียบชิ้นงานสำหรับ Supplier"),
                Text("Wear and fit measurement support", "ช่วยวัดการสึกและการประกอบ"),
                Text("Inspection data for remakes", "ข้อมูลตรวจสำหรับผลิตใหม่"),
                Text("Critical dimensions checked", "ตรวจมิติสำคัญ"),
                Text("Measured findings from scan data", "ผลวัดจากข้อมูลสแกน"),
                Text("Deviation reports for engineers", "รายงานความคลาดเคลื่อนสำหรับวิศวกร"),
                Text("Evidence-led production decisions", "ตัดสินใจผลิตด้วยหลักฐานการวัด")
            ],
            [
                Text("checked before you remake", "ตรวจก่อนผลิตใหม่"),
                Text("reported against CAD", "รายงานเทียบกับ CAD"),
                Text("ready for acceptance review", "พร้อมใช้ทบทวนก่อนรับงาน"),
                Text("matched to critical dimensions", "จับคู่กับมิติสำคัญ"),
                Text("connected to repair decisions", "เชื่อมกับการตัดสินใจซ่อมหรือแก้ไข")
            ],
            [
                Text("Use inspection when tolerance, wear, supplier comparison, or fit evidence matters.", "ใช้การตรวจเมื่อ tolerance การสึก การเทียบ Supplier หรือหลักฐานการประกอบมีความสำคัญ"),
                Text("MALIEV connects scan data, CAD comparison, measured findings, and the next production decision.", "MALIEV เชื่อมข้อมูลสแกน การเทียบ CAD ผลวัด และการตัดสินใจผลิตถัดไป"),
                Text("Send the part context, CAD reference, critical dimensions, and the question the report must answer.", "ส่งบริบทชิ้นงาน ไฟล์ CAD อ้างอิง มิติสำคัญ และคำถามที่รายงานต้องตอบ"),
                Text("Know what changed before choosing whether to repair, remake, or accept the part.", "รู้ว่าส่วนใดเปลี่ยนไปก่อนเลือกว่าจะซ่อม ผลิตใหม่ หรือรับชิ้นงาน"),
                Text("Turn physical variation into a reviewable report for engineering and purchasing decisions.", "เปลี่ยนความต่างของชิ้นงานจริงเป็นรายงานที่ใช้ตัดสินใจทางวิศวกรรมและจัดซื้อได้")
            ])
    ];

    private static readonly IReadOnlyDictionary<string, HeroCopyProfile> ProfilesByKey = Profiles.ToDictionary(
        profile => profile.Key,
        StringComparer.OrdinalIgnoreCase);

    private static readonly IReadOnlyList<HeroAdTarget> _adTargets = Profiles
        .Select(profile => new HeroAdTarget(profile.Key, profile.ServiceSlug, profile.SuggestedQuery, profile.Label))
        .ToArray();

    internal static IReadOnlyList<HeroAdTarget> AdTargets => _adTargets;

    internal static HeroCopy Default => GetVariant(DefaultTargetKey, 0);

    internal static IReadOnlyList<HeroCopy> GetVariants(string? targetKey)
    {
        return ResolveProfile(targetKey).Variants;
    }

    internal static HeroCopy SelectRandom(string? targetKey)
    {
        var variants = GetVariants(targetKey);
        return variants[RandomNumberGenerator.GetInt32(variants.Count)];
    }

    internal static HeroCopy GetVariant(string? targetKey, int variantIndex)
    {
        var variants = GetVariants(targetKey);
        var normalizedIndex = Math.Abs(variantIndex % variants.Count);
        return variants[normalizedIndex];
    }

    internal static string ResolveTargetKey(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return DefaultTargetKey;
        }

        var query = ExtractQuery(url);
        if (string.IsNullOrWhiteSpace(query))
        {
            return DefaultTargetKey;
        }

        var parsed = QueryHelpers.ParseQuery(query);
        var values = QueryKeys
            .Where(parsed.ContainsKey)
            .Select(key => parsed[key].ToString())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();

        foreach (var value in values)
        {
            var exactMatch = Profiles.FirstOrDefault(profile => Matches(value, profile.Key) || Matches(value, profile.SuggestedQuery));
            if (exactMatch is not null)
            {
                return exactMatch.Key;
            }
        }

        var aliasMatch = Profiles
            .Select(profile => new
            {
                Profile = profile,
                Alias = profile.Aliases
                    .Where(alias => values.Any(value => ContainsNormalized(value, alias)))
                    .OrderByDescending(alias => NormalizeSearchText(alias).Length)
                    .FirstOrDefault()
            })
            .Where(match => !string.IsNullOrWhiteSpace(match.Alias))
            .OrderByDescending(match => NormalizeSearchText(match.Alias!).Length)
            .FirstOrDefault();

        if (aliasMatch is not null)
        {
            return aliasMatch.Profile.Key;
        }

        return DefaultTargetKey;
    }

    private static HeroCopyProfile ResolveProfile(string? targetKey)
    {
        return !string.IsNullOrWhiteSpace(targetKey) && ProfilesByKey.TryGetValue(targetKey, out var profile)
            ? profile
            : ProfilesByKey[DefaultTargetKey];
    }

    private static string ExtractQuery(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri.Query;
        }

        var queryStart = url.IndexOf('?', StringComparison.Ordinal);
        return queryStart >= 0 ? url[queryStart..] : string.Empty;
    }

    private static bool Matches(string value, string candidate)
    {
        return string.Equals(NormalizeSearchText(value), NormalizeSearchText(candidate), StringComparison.Ordinal);
    }

    private static bool ContainsNormalized(string value, string candidate)
    {
        return NormalizeSearchText(value).Contains(NormalizeSearchText(candidate), StringComparison.Ordinal);
    }

    private static string NormalizeSearchText(string value)
    {
        var decoded = Uri.UnescapeDataString(value)
            .Replace('+', ' ')
            .Replace('-', ' ')
            .Replace('_', ' ')
            .Replace("?", " ", StringComparison.Ordinal)
            .Replace("=", " ", StringComparison.Ordinal)
            .ToLowerInvariant();

        return string.Join(' ', decoded.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private static HeroCopyProfile Profile(
        string key,
        string serviceSlug,
        string suggestedQuery,
        HeroPhrase label,
        IReadOnlyList<string> aliases,
        IReadOnlyList<HeroPhrase> leads,
        IReadOnlyList<HeroPhrase> accents,
        IReadOnlyList<HeroPhrase> bodies)
    {
        return new HeroCopyProfile(key, serviceSlug, suggestedQuery, label.ToLocalizedText(), aliases, leads, accents, bodies);
    }

    private static HeroPhrase Text(string en, string th)
    {
        return new HeroPhrase(en, th);
    }
}

internal sealed record HeroAdTarget(string Key, string ServiceSlug, string SuggestedQuery, LocalizedText Label);

internal sealed record HeroCopy(
    string TargetKey,
    int VariantIndex,
    LocalizedText HeadlineLead,
    LocalizedText HeadlineAccent,
    LocalizedText Body,
    LocalizedText MetaDescription);

internal sealed record HeroCopyProfile(
    string Key,
    string ServiceSlug,
    string SuggestedQuery,
    LocalizedText Label,
    IReadOnlyList<string> Aliases,
    IReadOnlyList<HeroPhrase> Leads,
    IReadOnlyList<HeroPhrase> Accents,
    IReadOnlyList<HeroPhrase> Bodies)
{
    internal IReadOnlyList<HeroCopy> Variants { get; } = BuildVariants(Key, Leads, Accents, Bodies);

    private static IReadOnlyList<HeroCopy> BuildVariants(
        string key,
        IReadOnlyList<HeroPhrase> leads,
        IReadOnlyList<HeroPhrase> accents,
        IReadOnlyList<HeroPhrase> bodies)
    {
        var variants = new List<HeroCopy>(leads.Count * accents.Count);

        for (var leadIndex = 0; leadIndex < leads.Count; leadIndex++)
        {
            for (var accentIndex = 0; accentIndex < accents.Count; accentIndex++)
            {
                var variantIndex = variants.Count;
                var lead = leads[leadIndex];
                var accent = accents[accentIndex];
                var body = bodies[(leadIndex + accentIndex) % bodies.Count];
                variants.Add(new HeroCopy(
                    key,
                    variantIndex,
                    lead.ToLocalizedText(),
                    accent.ToLocalizedText(),
                    body.ToLocalizedText(),
                    CreateMetaDescription(lead, accent)));
            }
        }

        if (variants.Count < HeroCopyCatalog.MinimumVariantCount)
        {
            throw new InvalidOperationException($"Hero copy profile '{key}' generated only {variants.Count} variants.");
        }

        return variants;
    }

    private static LocalizedText CreateMetaDescription(HeroPhrase lead, HeroPhrase accent)
    {
        return SiteContent.Text(
            $"{lead.En} {accent.En}. Upload CAD to MALIEV for DFM review, pricing, and production ordering.",
            $"{lead.Th} {accent.Th} อัปโหลด CAD กับ MALIEV เพื่อตรวจ DFM ดูราคา และสั่งผลิตต่อได้");
    }
}

internal sealed record HeroPhrase(string En, string Th)
{
    internal LocalizedText ToLocalizedText()
    {
        return SiteContent.Text(En, Th);
    }
}
