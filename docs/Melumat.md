# Market POS Sistemi — Peşəkar Arxitektura və Modul Planı

Əvvəlcə 2026-cı ilin aparıcı POS sistemlərini (Square, Clover, Lightspeed, IT Retail, Toast, Odoo POS, Auto-Star) və market/supermarket seqmentinin spesifik tələblərini araşdırdım. Aşağıda bu araşdırmaya əsaslanan tam arxitektura planı var.

## Araşdırmadan əsas nəticələr

Sənayedə 2026-cı il üçün ortaq trend budur: ən yaxşı POS sistemləri sürətli, təhlükəsiz, bir-birinə bağlı və inteqrasiya olunmuş ödəniş, güclü təhlükəsizlik, detallı analitika və real-vaxt inventar idarəetməsi təklif edən hərtərəfli həllərdir. Bunun ötəsində, market seqmenti üçün iki kritik fərq üzə çıxdı:

1. **Çəki ilə satılan məhsullar** — tərəzi ilə POS arasında dəqiq inteqrasiya lazımdır; çəki dəyəri seriya rabitəsi vasitəsilə ötürülür və sistem bunu qiymət bazası ilə birləşdirərək avtomatik hesablama aparır. RS-232 seriya bağlantısı, xüsusilə köhnə infrastrukturlu supermarketlərdə hələ də üstünlük təşkil edən köhnə standartdır.
2. **Offline dayanıqlıq** — bu, əlavə funksiya deyil, arxitektura qərarıdır: şəbəkə kəsildiyi anda kartları avtorizasiya etməyi dayandıran sistemlər əslində "offline" deyil, marketinq mənasında offline sayılır; həqiqi offline rejim terminalın şəbəkəsiz tam işləməsini, ödənişləri avtorizasiya etməsini və lokal məhsul datasını oxumasını tələb edir.

Aşağıdakı modul xəritəsi bu araşdırma və sizin WinForms/SQL Server/Zebra-ZPL təcrübəniz nəzərə alınmaqla hazırlanıb:## 1️⃣ ƏSAS MODULLAR

### 1.1 Satış (Checkout) modulu — sistemin ürəyi

- **Barkod skanlama** — 1D (EAN-13, UPC-A) və 2D (QR) dəstəyi, USB HID və Bluetooth skanerlərlə uyğunluq
- **Çəki ilə satılan məhsullar** — tərəzi inteqrasiyası. Kassir məhsulu tərəzidə "elan edir", tərəzi çəkini ötürür, sistem əvvəlcədən təyin olunmuş kq-qiymətini tətbiq edərək ümumi məbləği avtomatik hesablayır — əl ilə daxiletmə tələb olunmur. Bu, ət, pendir, tərəvəz kimi PLU-əsaslı məhsullar üçün mütləqdir və manual qiymət səhvlərinin qarşısını alır.
- **Çoxlu ödəniş üsulları** — nağd, bank kartı (POS-terminal), split payment (bir çekin bir neçə üsulla ödənməsi)
- **Səbət idarəetməsi** — sürətli məhsul axtarışı, miqdar dəyişdirmə, sətir üzrə endirim, "suspend/hold" (səbəti müvəqqəti saxlayıb başqa müştəriyə keçmək)
- **Qaytarma/refund/mübadilə** — orijinal çekə istinadla qismən və ya tam qaytarma
- **Qəbz** — thermal printerdən çap, həmçinin email/SMS variantı
- **Offline satış rejimi** — internet kəsiləndə belə satışın davam etməsi (aşağıda ətraflı)

### 1.2 İnventar (stok) idarəetməsi

- **Məhsul kataloqu** — SKU, barkod, kateqoriya, çəki/ədəd tipi, çoxlu barkod (bir məhsula bir neçə barkod, məs. həm istehsalçı, həm mağaza barkodu)
- **Real-time stok səviyyəsi** — hər satışdan dərhal sonra azalma
- **Partiya/lot və son istifadə tarixi izləmə** — ərzaq məhsulları üçün mütləqdir. Sistem FEFO (First-Expired-First-Out) məntiqini FIFO-dan üstün tutmalıdır, çünki son alınan partiya daha tez bitə bilər — FEFO israfı azaltmaq üçün ən effektiv üsul sayılır və pərakəndə/qida sənayesində standart tələb halına gəlib.
- **Avtomatik sifariş nöqtəsi (reorder point)** — minimum stok həddi keçiləndə PO təklifi
- **İnventarizasiya** — cycle count (gündəlik seçmə yoxlama) və tam fiziki sayım
- **Barkod/etiket çapı** — Zebra ZPL ilə sizin artıq təcrübəniz olan sahə; qiymət etiketləri, çəki etiketləri (deli/tərəvəz üçün)

### 1.3 Müştəri idarəetməsi (CRM)

- Müştəri profili, əlaqə, alış tarixçəsi
- **Sadiqlik proqramı** — bal toplama, səviyyələr (Silver/Gold/Platinum), doğum günü endirimi
- Endirim kartları, SMS/email marketinq siyahıları

### 1.4 İşçi (kassir) idarəetməsi

- Rol-əsaslı icazələr (kassir / meneceri / admin) — kimin endirim verə, qaytarma edə, qiyməti dəyişə biləcəyi
- **Növbə (shift) açma/bağlama** — kassa siyirtməsinin başlanğıc məbləği bəyan edilir, gün ərzində satışlar toplanır, sonda faktiki nəğd say ilə sistem gözlədiyi məbləğ müqayisə olunur (bu, sənayədə **X/Z hesabatı** adlanır — X aralıq, Z gün sonu yekun hesabatıdır)
- Bir kassaya bir neçə kassirin növbəli girişi, performans izləmə

### 1.5 Maliyyə və hesabatlıq

- Gündəlik/həftəlik/aylıq satış hesabatları, kateqoriya üzrə mənfəət marjası
- ƏDV və digər vergi hesablamaları
- Kassa balansının yoxlanılması, mühasibatlıq proqramına ixrac (Excel/1C)

### 1.6 Endirim/Promosiya mühərriki

- Faiz/məbləğ endirimləri, BOGO (Al 1 – Al 1 pulsuz), kupon kodları
- **Stacking qaydaları** — hansı endirimlərin birləşə biləcəyi, hansının "eksklüziv" olub başqası ilə birləşməyəcəyi. Bu qaydalar aydın müəyyən olunmadıqda kassada gözlənilməz qiymət nəticələri yaranır, ona görə də promosiya mühərriki əvvəlcədən təyin olunmuş prioritet ardıcıllığı ilə işləməlidir.

## 2️⃣ KÖMƏKÇİ MODULLAR

### 2.1 Təchizatçı və satınalma

- Təchizatçı bazası, məhsul-təchizatçı əlaqəsi, alış qiyməti tarixçəsi
- Purchase Order (PO) yaratma, mal qəbulu, PO-suz mal qəbulu

### 2.2 Çoxfilial (multi-branch) idarəetməsi

- Mərkəzi idarəetmə paneli — bütün filialların stok və satışına vahid baxış
- **Filiallar arası stok transferi** — bir filialda artıq, digərində çatışmayan mal üçün
- **Mərkəzləşdirilmiş satınalma** — hər filial ayrı-ayrı sifariş verəndə eyni təchizatçıya təkrarlanan sifarişlər göndərilir; mərkəzləşdirmə bu sifarişləri birləşdirərək həm inzibati yükü, həm də təchizatçı qiymətlərini yaxşılaşdırır.

### 2.3 Hardware inteqrasiyası (sizin ixtisas sahəniz)

- Barkod skanerlər (USB HID/Bluetooth), tərəzilər (RS-232/USB seriya protokolu)
- Thermal qəbz printerləri (ESC/POS)
- **Zebra etiket printerləri (ZPL)** — qiymət/çəki etiketləri
- Kassa siyirtməsi (cash drawer, adətən printerdən trigger olunur)
- Müştəri displeyi (pole display), ödəniş terminalı (POS-terminal)

### 2.4 Fiskal inteqrasiya — **Azərbaycan üçün məcburi** (aşağıda ətraflı)

### 2.5 Backup/Restore və Audit Log

- Avtomatik ehtiyat nüsxələmə (gündəlik/saatlıq)
- Bütün kritik əməliyyatların loqu — kim, nə vaxt, nə etdi (qiymət dəyişikliyi, endirim, silmə, giriş/çıxış)
- Session təhlükəsizliyi — sizin əvvəlki auditlərinizdə diqqət yetirdiyiniz sahə

### 2.6 Bildiriş sistemi

- Aşağı stok, son istifadə tarixi yaxınlaşan məhsullar, sinxronizasiya xətaları üçün xəbərdarlıq

### 2.7 Genişlənmə üçün (gələcək fazalar)

- Self-checkout kiosk, mobil POS (planşet əsaslı əlavə kassa), e-ticarət/onlayn sifariş inteqrasiyası

## 3️⃣ Azərbaycan üçün məcburi tələb: NKA və e-Qaimə inteqrasiyası

Bu, çox vaxt unudulan, amma real bazarda buraxılmaz bir tələbdir:

- **NKA (Nəzarət-Kassa Aparatı)** qanunvericilikdə malların təqdim edilməsi zamanı hesablaşmaların qeydiyyatında istifadə edilən, fiskal yaddaşa malik olan və vergi orqanlarında qeydiyyatdan keçən elektron avadanlıq və ya kompüter sistemi kimi tərif olunur.
- Vergilər Nazirliyi hazırda **yeni nəsil NKA-ların** tətbiqini davam etdirir — bu aparatlar ilkin mərhələdə yanacaqdoldurma məntəqələrində və şəbəkə sistemli marketlərdə quraşdırılır, hazırda isə Bakının iri market şəbəkələrində proqram təminatlarının Vergilər Nazirliyinin informasiya sistemi ilə real-vaxt inteqrasiyası prosesi gedir. Birinci mərhələyə Bravo Supermarket, Bizim Market, OBA Market, Araz Supermarket, Spar Supermarket kimi tanınmış market şəbəkələri daxildir — yəni sizin POS-un real bazarda çıxışı olsa, bu inteqrasiya ilə qarşılaşma ehtimalı yüksəkdir.
- 2026-cı il dəyişikliklərinə əsasən, əhaliyə nağdsız ödəmələr nəzarət-kassa aparatına vahid əməliyyat sistemində inteqrasiya edilmiş POS-terminal vasitəsilə aparılmalıdır — yəni bank POS-terminalı ilə NKA arasında proqram səviyyəsində rabitə tələb olunur.
- **Elektron qaimə-faktura** topdansatış/B2B əməliyyatlar üçün ayrıca tələbdir; pərakəndə satışda NKA çeki kifayətdir, lakin sistem bu ikisini fərqləndirməli və lazım gəldikdə e-qaimə generasiya edə bilməlidir.

**Praktiki tövsiyə:** `IFiscalDevice` kimi abstraksiya (interfeys) yaradın — real NKA hardware-i olmayanda "mock" implementasiya ilə test edin, sonradan real cihaz sərbəst plug-in kimi qoşulsun. Bu, sizin OOP təcrübənizə tam uyğun gələn Strategy/Adapter pattern həllidir.

## 4️⃣ Texniki arxitektura tövsiyəsi (C#/.NET üçün)

**Qatlı arxitektura (Layered/Clean Architecture):**

```
Presentation (WinForms/WPF) 
    → Business Logic (Services) 
        → Data Access (Repository + Unit of Work) 
            → Database (SQL Server)
```

**Əsas dizayn qərarları:**

- **Offline-first — seçim deyil, arxitektura fundamentidir.** Real təcrübə göstərir ki, mərkəzi serverə tam asılı "thin client" kimi qurulan sistemlər şəbəkəsiz tam işləyə bilmir; həqiqi offline-dayanıqlı sistem terminalın tranzaksiyanı emal etməsini, kartı avtorizasiya etməsini və lokal məhsul datasını il əvvəldən keşləməsini tələb edir. Sizin üçün praktiki yanaşma: hər terminalda lokal SQLite/SQL Server LocalDB keşi + mərkəzi SQL Server-lə fon rejimində (background) sinxronizasiya növbəsi.
- **Qiymət/ad "snapshot" pattern-i** — real layihədə çox rast gəlinən səhv budur: sifariş sətri birbaşa `Product` cədvəlinə bağlanır, sonra qiymət dəyişəndə köhnə hesabatlar da dəyişir. Həll: `SaleItem` cədvəlində satış anındakı ad və qiyməti ayrıca sahə kimi saxlayın (`PriceAtSale`, `NameAtSale`) — bu, tarixi hesabatların düzgünlüyünü təmin edir.
- **Rol/icazə sistemini əvvəldən planlaşdırın** — sonradan mövcud sistemə əlavə etmək ağrılı olur, ona görə erkən mərhələdə tikin.
- **POS Client → Business API → Database** modeli — terminalların birbaşa verilənlər bazasına qoşulması əvəzinə aralıq API/servis qatı daha təhlükəsiz və miqyaslanabilirdir, xüsusilə çoxfilial ssenarisində.

**Dizayn pattern-ləri:**

| Pattern | Harada istifadə |
|---|---|
| Repository + Unit of Work | Data Access qatı |
| Strategy | Ödəniş üsulları, fiskal cihaz (NKA) inteqrasiyası |
| Observer | Stok azalanda bildiriş tetiklənməsi |
| Factory | Çek/etiket sənəd generasiyası (ZPL, ESC/POS) |
| Decorator | Endirim mühərrikinin qat-qat tətbiqi |

**Verilənlər bazası:** SQL Server sizin mövcud təcrübənizə uyğun düzgün seçimdir — şəbəkəli POS üçün etibarlı çoxistifadəçili giriş, real-time sinxronizasiya və backup dəstəyi verir.

## 5️⃣ Verilənlər bazası — əsas cədvəllər (icmal)

`Products`, `ProductBatches` (partiya/expiry), `Categories`, `Inventory`, `Sales`, `SaleItems`, `Customers`, `LoyaltyPoints`, `Employees`, `Roles`, `Shifts`, `CashDrawerLogs`, `Suppliers`, `PurchaseOrders`, `Branches`, `StockTransfers`, `Promotions`, `PromotionRules`, `AuditLogs`, `FiscalTransactions`

İstəsəniz, bunun tam ER-diaqramını (əlaqələr və açarlarla) ayrıca hazırlaya bilərəm.

## 6️⃣ Fazalı inkişaf planı

| Faza | Məzmun |
|---|---|
| **Faza 1 (MVP)** | Satış, əsas inventar, sadə hesabat, tək kassa |
| **Faza 2** | Müştəri/loyalty, endirim mühərriki, işçi/növbə idarəetməsi, barkod-etiket çapı |
| **Faza 3** | Çoxfilial, NKA/e-kassa inteqrasiyası, offline-sync yetkinləşdirmə |
| **Faza 4** | Analitika/BI paneli, mobil POS, self-checkout, e-ticarət |

---

Bu, tam skelet planıdır. Növbəti addım olaraq, hər hansı bir hissəni daha dərin işləyə bilərəm — məsələn, tam ER-diaqram, `Product`/`SaleItem` sinif strukturu, offline-sync alqoritminin detalları, ya da NKA inteqrasiyası üçün `IFiscalDevice` interfeysinin C# skeleti. Hansından başlamaq istərdiniz?Yuxarıdakı plan hazırdır — istənilən modulu daha dərindən açmağa hazıram.
