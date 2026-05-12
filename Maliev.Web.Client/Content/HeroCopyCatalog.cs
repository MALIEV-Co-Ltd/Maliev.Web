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
            ["fdm", "fdm 3d printing", "fdm 3d printing service", "fdm near me", "รับพิมพ์ 3 มิติ fdm", "พิมพ์ fdm", "พิมพ์ 3d fdm"],
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
                Text("ordered from one upload", "สั่งต่อจากไฟล์เดียว"),
                Text("matched to material and quantity", "ตรงวัสดุและจำนวน")
            ],
            [
                Text("Upload CAD, pick FDM material, review DFM, and order without vendor handoffs.", "อัปโหลด CAD เลือกวัสดุ FDM ตรวจ DFM และสั่งผลิตในที่เดียว"),
                Text("Choose FDM for fast fit checks, fixtures, and affordable functional parts.", "เลือก FDM สำหรับตรวจฟิต ฟิกซ์เจอร์ และชิ้นงานใช้งานจริง"),
                Text("Send CAD for FDM review, compare materials, and see price and lead time fast.", "ส่ง CAD ตรวจงาน FDM เทียบวัสดุ เห็นราคาและเวลาเร็ว"),
                Text("Route FDM prototypes, jigs, and low-volume parts into a production-ready quote.", "พางาน FDM ต้นแบบ จิ๊ก และล็อตเล็กเข้าสู่ใบเสนอราคาพร้อมผลิต"),
                Text("Keep design, DFM, pricing, and order handoff in one focused flow.", "รวมออกแบบ DFM ราคา และสั่งผลิตในขั้นตอนเดียว")
            ]),
        Profile(
            "3d-printing",
            "3d-printing",
            "?service=3d-printing",
            Text("3D Printing", "งานพิมพ์ 3 มิติ"),
            ["3d printing", "3d print", "3d printing near me", "3d printing service near me", "additive manufacturing", "รับพิมพ์ 3 มิติ", "พิมพ์ 3d", "พิมพ์ 3d ใกล้ฉัน", "ร้าน 3d print ใกล้ฉัน", "งานพิมพ์สามมิติ"],
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
                Text("fit and function ready", "พร้อมทดสอบฟิตและใช้งาน"),
                Text("ready to order", "พร้อมสั่งผลิต"),
                Text("matched to your quantity", "ตรงจำนวนและผิวงาน")
            ],
            [
                Text("Upload CAD once, compare print routes, review DFM, and move straight to order.", "อัปโหลด CAD ครั้งเดียว เทียบวิธีพิมพ์ ตรวจ DFM แล้วสั่งผลิตต่อ"),
                Text("Use MALIEV for prototypes, fixtures, samples, and polymer parts built for real use.", "ใช้ MALIEV สำหรับต้นแบบ ฟิกซ์เจอร์ ตัวอย่าง และชิ้นงานโพลีเมอร์"),
                Text("Choose process, material, finish, and quantity before you commit.", "เลือกกระบวนการ วัสดุ ผิวงาน และจำนวนก่อนตัดสินใจ"),
                Text("Move from STL, STEP, OBJ, or 3MF to a clear quote without vendor handoffs.", "เริ่มจาก STL, STEP, OBJ หรือ 3MF แล้วได้ราคาโดยไม่ส่งต่อหลายที่"),
                Text("Iterate faster while checking details that affect quality and cost.", "ทดลองเร็วขึ้น พร้อมตรวจปัจจัยที่กระทบคุณภาพและต้นทุน")
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
                Text("priced by finish target", "คิดราคาตามผิวงาน"),
                Text("presentation ready", "พร้อมนำเสนอสินค้า"),
                Text("checked for detail risk", "ตรวจความเสี่ยงรายละเอียด"),
                Text("ordered from one quote", "สั่งต่อจากใบเสนอราคา")
            ],
            [
                Text("Upload CAD for resin DFM, finish expectations, pricing, and order handoff.", "อัปโหลด CAD ตรวจ DFM เรซิน ดูผิวงาน ราคา และสั่งผลิตต่อ"),
                Text("Use resin 3D printing for smooth prototypes and fine product details.", "เลือกพิมพ์เรซินสำหรับต้นแบบผิวเนียนและรายละเอียดเล็ก"),
                Text("Confirm printability, detail risk, and the right production path before order.", "ยืนยันความพร้อมพิมพ์ ความเสี่ยง และเส้นทางผลิตก่อนสั่งงาน"),
                Text("Compare resin with other 3D print routes by strength, finish, and lead time.", "เทียบเรซินกับวิธีพิมพ์อื่นตามความแข็งแรง ผิวงาน และเวลา"),
                Text("Keep resin quoting tied to CAD upload, review, and production ordering.", "เชื่อมราคาเรซินกับ CAD การตรวจ และการสั่งผลิต")
            ]),
        Profile(
            "aluminum-cnc-milling",
            "cnc-machining",
            "?service=aluminum-cnc-milling",
            Text("Aluminum CNC Milling", "งานกัด CNC อลูมิเนียม"),
            ["aluminum cnc milling", "aluminium cnc milling", "aluminum cnc near me", "cnc aluminum", "cnc aluminium", "กัดอลูมิเนียม cnc", "cnc อลูมิเนียม", "กัดอะลูมิเนียม", "กัดอลูมิเนียมใกล้ฉัน"],
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
                Text("quoted with setup notes", "พร้อมหมายเหตุการจับงาน"),
                Text("matched to finish", "ตรงเป้าหมายผิวงาน"),
                Text("ready for production", "พร้อมตรวจผลิต"),
                Text("ordered from CAD and drawings", "สั่งต่อได้จาก CAD และ Drawing")
            ],
            [
                Text("Send STEP, drawings, material notes, and quantities for aluminum CNC pricing.", "ส่ง STEP, Drawing, วัสดุ และจำนวน เพื่อขอราคา CNC อลูมิเนียม"),
                Text("Review tolerance, setup, finish, and production fit before machining starts.", "ตรวจ tolerance การจับงาน ผิวงาน และความพร้อมก่อนกัดจริง"),
                Text("Use aluminum CNC for brackets, housings, plates, fixtures, and metal prototypes.", "ใช้ CNC อลูมิเนียมสำหรับขายึด เคส เพลต ฟิกซ์เจอร์ และต้นแบบโลหะ"),
                Text("Keep machining questions, DFM, price, and order handoff in one flow.", "รวมคำถาม CNC, DFM, ราคา และการสั่งผลิตในขั้นตอนเดียว"),
                Text("Match ad traffic to aluminum CNC copy and the same MALIEV production flow.", "พาทราฟฟิกสู่ข้อความ CNC อลูมิเนียมและขั้นตอนผลิตเดียวกัน")
            ]),
        Profile(
            "cnc-machining",
            "cnc-machining",
            "?service=cnc-machining",
            Text("CNC Machining", "CNC แมชชีนนิ่ง"),
            ["cnc", "cnc machining", "cnc milling", "cnc near me", "cnc shop near me", "machining service", "cnc service", "งาน cnc", "รับกัด cnc", "ร้าน cnc", "cnc ใกล้ฉัน", "ร้าน cnc ใกล้ฉัน", "โรงกลึง cnc", "แมชชีนนิ่ง"],
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
                Text("priced by tolerance need", "คิดราคาตาม tolerance"),
                Text("matched to material", "ตรงวัสดุและผิวงาน"),
                Text("ready for production", "พร้อมส่งผลิต"),
                Text("quoted from STEP and drawings", "เสนอราคาจาก STEP และ Drawing")
            ],
            [
                Text("Send CAD, drawings, material, tolerance, finish, and quantity for CNC review.", "ส่ง CAD, Drawing, วัสดุ tolerance ผิวงาน และจำนวนเพื่อตรวจ CNC"),
                Text("Catch setup and manufacturability risks before CNC machining starts.", "ตรวจความเสี่ยงการจับงานและผลิตก่อนเริ่ม CNC"),
                Text("Use CNC for functional prototypes, fixtures, production aids, and end-use parts.", "ใช้ CNC สำหรับต้นแบบ ฟิกซ์เจอร์ อุปกรณ์ช่วยผลิต และชิ้นงานจริง"),
                Text("Keep process choice, review, pricing, and production order in one path.", "รวมเลือกกระบวนการ ตรวจ ราคา และสั่งผลิตในเส้นทางเดียว"),
                Text("Match CNC intent to material, tolerance, finish, and delivery expectation.", "จับคู่เป้าหมาย CNC กับวัสดุ tolerance ผิวงาน และกำหนดส่ง")
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
                Text("Share photos, rough size, output format, and why the part needs capture.", "ส่งรูป ขนาดคร่าวๆ รูปแบบไฟล์ และเหตุผลที่ต้องสแกน"),
                Text("Use scanning when the part exists but CAD, drawings, or supplier history do not.", "ใช้สแกนเมื่อมีชิ้นงานจริงแต่ไม่มี CAD, Drawing หรือประวัติ Supplier"),
                Text("Connect 3D scanning, CAD rebuild, inspection, and manufacturing next steps.", "เชื่อมสแกน 3 มิติ ขึ้น CAD ตรวจสอบ และขั้นตอนผลิตถัดไป"),
                Text("Capture geometry before you repair, remake, compare, or redesign.", "เก็บ Geometry ก่อนซ่อม ผลิตใหม่ เทียบผล หรือออกแบบใหม่"),
                Text("Turn physical samples into usable data for engineering and production review.", "เปลี่ยนตัวอย่างจริงเป็นข้อมูลสำหรับวิศวกรรมและการผลิต")
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
                Text("ready for manufacturing", "พร้อมผลิตจริง"),
                Text("checked before upload", "ตรวจความพร้อมก่อนอัปโหลด"),
                Text("routed into printing or CNC", "ส่งต่อสู่งานพิมพ์หรือ CNC"),
                Text("refined for fit", "ปรับเพื่อการประกอบ"),
                Text("ready for quote review", "พร้อมเข้าสู่การตรวจเสนอราคา")
            ],
            [
                Text("Start from sketches, photos, dimensions, samples, or unfinished CAD.", "เริ่มจากสเก็ตช์ รูปถ่าย ขนาด ตัวอย่าง หรือไฟล์ CAD ที่ยังไม่จบ"),
                Text("Connect design edits to the process that will make the part.", "เชื่อมการแก้แบบกับกระบวนการที่จะผลิตชิ้นงาน"),
                Text("Use 3D design support when an idea needs geometry, DFM, and a quote-ready file.", "ใช้บริการออกแบบเมื่อไอเดียต้องการ Geometry, DFM และไฟล์พร้อมเสนอราคา"),
                Text("Move from rough concept to manufacturable CAD without splitting the workflow.", "เปลี่ยนคอนเซ็ปต์เป็น CAD ที่ผลิตได้โดยไม่แยกขั้นตอน"),
                Text("Improve fit, wall thickness, tolerance notes, and assembly intent before order.", "ปรับฟิต ผนัง tolerance และเป้าหมายประกอบก่อนสั่งผลิต")
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
                Text("priced by quantity", "คิดราคาตามจำนวน"),
                Text("ready for pilot validation", "พร้อมทดสอบล็อตทดลอง"),
                Text("bridged from prototype to batch", "เชื่อมจากต้นแบบสู่ล็อตผลิต"),
                Text("reviewed for mold path", "ตรวจเส้นทางแม่พิมพ์")
            ],
            [
                Text("Use casting when one prototype needs to become many test parts.", "ใช้งานหล่อเมื่อต้นแบบหนึ่งชิ้นต้องกลายเป็นชิ้นทดสอบหลายชิ้น"),
                Text("Review geometry, material behavior, quantity, and rapid tooling path.", "ตรวจ Geometry วัสดุ จำนวน และเส้นทางแม่พิมพ์เร็ว"),
                Text("Bridge printed prototypes to low-volume parts before hard tooling.", "ต่อยอดต้นแบบพิมพ์สู่ล็อตเล็กก่อนลงทุนแม่พิมพ์จริง"),
                Text("Share CAD, target feel, quantity, and deadline for the right casting quote.", "ส่ง CAD ผิวงาน จำนวน และกำหนดเวลาเพื่อประเมินงานหล่อ"),
                Text("Keep casting decisions tied to DFM, pricing, and production handoff.", "เชื่อมงานหล่อกับ DFM ราคา และการส่งต่อผลิต")
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
                Text("built around your deadline", "วางตามกำหนดเวลา"),
                Text("routed through the right process", "เลือกกระบวนการที่เหมาะกับงาน"),
                Text("checked before each build", "ตรวจก่อนสร้างแต่ละรอบ"),
                Text("priced before you commit", "เห็นราคาก่อนตัดสินใจ"),
                Text("ready for the next build", "พร้อมไปต่อสู่รอบผลิตถัดไป")
            ],
            [
                Text("Combine printing, CNC, scanning, design, and finish around the test you need.", "รวมพิมพ์ CNC สแกน ออกแบบ และผิวงานตามการทดสอบที่ต้องการ"),
                Text("Share the product goal, must-fit dimensions, process target, and deadline.", "แจ้งเป้าหมายสินค้า ขนาดสำคัญ กระบวนการ และกำหนดเวลา"),
                Text("Move quickly without skipping manufacturability review.", "เดินงานเร็วโดยไม่ข้ามการตรวจความพร้อมผลิต"),
                Text("Use rapid prototyping when the next decision needs a real part in hand.", "ใช้ต้นแบบรวดเร็วเมื่อการตัดสินใจต้องพึ่งชิ้นงานจริง"),
                Text("Keep every build connected to quote, DFM, material, and production decisions.", "เชื่อมทุกรอบกับราคา DFM วัสดุ และการตัดสินใจผลิต")
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
                Text("connected to repair", "เชื่อมกับการซ่อม")
            ],
            [
                Text("Use inspection when tolerance, wear, supplier comparison, or fit evidence matters.", "ใช้ตรวจเมื่อ tolerance การสึก การเทียบ Supplier หรือหลักฐานฟิตสำคัญ"),
                Text("Connect scan data, CAD comparison, measured findings, and the next decision.", "เชื่อมข้อมูลสแกน เทียบ CAD ผลวัด และการตัดสินใจถัดไป"),
                Text("Send part context, CAD reference, critical dimensions, and the key question.", "ส่งบริบทชิ้นงาน CAD อ้างอิง มิติสำคัญ และคำถามหลัก"),
                Text("Know what changed before you repair, remake, or accept the part.", "รู้จุดเปลี่ยนก่อนซ่อม ผลิตใหม่ หรือรับชิ้นงาน"),
                Text("Turn physical variation into a report engineering and purchasing can use.", "เปลี่ยนความต่างของชิ้นงานเป็นรายงานที่ทีมวิศวกรรมและจัดซื้อใช้ได้")
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
