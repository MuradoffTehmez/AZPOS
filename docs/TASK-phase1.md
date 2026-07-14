# İMPLEMENTASİYA PROMPTU — Market POS Sistemi (Faza 1 / MVP)

## ROL

Sən C#/.NET üzrə enterprise-səviyyəli desktop tətbiqlər (xüsusilə POS/ERP sistemləri) qurmuş baş proqramçısan. Clean Architecture, SOLID prinsipləri və production-ready kod yazırsan. Qısa yol axtarmırsan, amma over-engineering da etmirsən — Faza 1-in əhatəsindən kənara çıxmırsan.

Başlamazdan əvvəl `PRD.md` faylını tam oxu. Bu prompt onun icra xülasəsidir; ziddiyyət yarandıqda `PRD.md` prioritetlidir.

## LAYİHƏ HAQQINDA

Azərbaycan bazarı üçün market/supermarket tipli pərakəndə satış nöqtəsi POS sistemi. Offline-first, NKA/e-Kassa inteqrasiyasına hazır, çoxfilial arxitekturasına genişlənə bilən struktur üzərində qurulur. Hazırkı iş yalnız **Faza 1 (MVP)**-i əhatə edir: Satış, əsas İnventar, əsas RBAC, Növbə idarəetməsi, sadə hesabat.

## TEXNİKİ STACK (qəti qərar — dəyişmək istəsəniz mənə bildirin, əks halda bununla davam et)

| Qat | Texnologiya |
|---|---|
| Runtime | .NET 8 (LTS) |
| UI | Windows Forms + MaterialSkin2 |
| ORM | Entity Framework Core 8, Code-First + Migrations |
| Mərkəzi DB | SQL Server (Express, dev üçün) |
| Offline keş | SQLite (EF Core Sqlite provider) |
| DI | Microsoft.Extensions.DependencyInjection |
| Logging | Serilog (fayl + console sink) |
| Test | xUnit + Moq |

## MEMARLIQ QAYDALARI (məcburi)

1. **Qatlı struktur** — `Domain` heç bir NuGet asılılığı olmayan təmiz POCO layihəsi olmalıdır. `Application` yalnız `Domain`-ə, `Infrastructure` `Application`-dakı interfeyslərə, `UI` isə yalnız `Application`-a müraciət edir. UI birbaşa `Infrastructure`-ı və ya `DbContext`-i tanımamalıdır.
2. **Repository + Unit of Work** — bütün DB girişi bu qat arxasında gizlənir. UI heç vaxt birbaşa `DbContext` çağırmır.
3. **Snapshot pattern (məcburi)** — `SaleItem` cədvəlində məhsulun adı və qiyməti satış anında ayrıca sahədə (`ProductNameSnapshot`, `UnitPriceSnapshot`) donur. `Product.Price` sonradan dəyişsə belə, keçmiş satışlar dəyişməməlidir.
4. **Offline-first** — `Sale` yazılması həmişə əvvəlcə lokal SQLite-a olur (`IsSyncedToServer = false`), arxa fonda `MarketPOS.Sync` background service mərkəzi SQL Server-ə göndərir. UI heç vaxt "internet yoxdur" səbəbi ilə satışı bloklamamalıdır.
5. **Xəta idarəetməsi** — AzAgroPOS-dakı `Program.cs` yanaşmanıza bənzər struktur: SQL/DB xətaları, COM (hardware) xətaları və gözlənilməz xətalar ayrıca catch bloklarında tutulur, keçici (transient) DB xətaları üçün retry (exponential backoff, max 3 cəhd) tətbiq olunur, tətbiq səviyyəsində strukturlaşdırılmış exit/error kodları istifadə olunur.
6. **Async/await** — bütün DB və hardware çağırışları async, UI thread bloklanmır.
7. **Hardware inteqrasiyaları interfeys arxasında** — `IBarcodeScanner`, `IScaleReader`, `IReceiptPrinter`, `ILabelPrinter` (ZPL) — real cihaz olmadıqda mock implementasiya ilə işləməlidir ki, sənin real mühitin olmadan da kod test edilə bilsin.

## LAYİHƏ STRUKTURU
MarketPOS.sln
├── src/
│   ├── MarketPOS.Domain/          → Entities, enum-lar, dəyər obyektləri (asılılıqsız)
│   ├── MarketPOS.Application/     → Interfeyslər, servislər, DTO-lar, iş məntiqi
│   ├── MarketPOS.Infrastructure/  → EF Core DbContext, Repository-lər, hardware adapterləri
│   ├── MarketPOS.UI/              → WinForms (Presentation)
│   └── MarketPOS.Sync/            → Offline→online sinxronizasiya background service
├── tests/
│   ├── MarketPOS.Domain.Tests/
│   └── MarketPOS.Application.Tests/
├── docs/
│   └── PRD.md
└── README.md

## KODLAŞDIRMA STANDARTLARI

- Sinif/metod adları: PascalCase, ingiliscə (`SaleService`, `CalculateTotal`)
- Private field-lər: `_camelCase`
- UI-da görünən bütün mətnlər (label, button, mesaj): **Azərbaycan dili**
- Kod şərhləri: ingiliscə, yalnız "niyə" izah edən yerlərdə (nə etdiyini kod özü göstərməlidir)
- Hər public metod üçün XML doc comment
- Nullable reference types aktiv (`<Nullable>enable</Nullable>`)

## FAZA 1 — VERİLƏNLƏR BAZASI SXEMİ (başlanğıc üçün minimal dəst)
Categories(Id, Name, ParentCategoryId?)
Products(Id, SKU, Barcode, Name, CategoryId, UnitType[Piece|Weight], Price, CostPrice, TaxRate, IsActive)
Inventory(ProductId, QuantityOnHand, ReorderLevel)
Roles(Id, Name)                          -- Cashier / Manager / Admin
Employees(Id, FullName, Username, PasswordHash, RoleId, IsActive)
Shifts(Id, EmployeeId, OpenedAt, ClosedAt?, OpeningCash, ClosingCash?, ExpectedCash?, Status)
Sales(Id, ShiftId, EmployeeId, SaleDate, TotalAmount, TaxAmount, DiscountAmount, PaymentMethod, Status, IsSyncedToServer)
SaleItems(Id, SaleId, ProductId, ProductNameSnapshot, UnitPriceSnapshot, Quantity, LineTotal)
AuditLogs(Id, EmployeeId, Action, EntityName, EntityId, OldValue, NewValue, Timestamp)

## İCRA ADDIMLARI — SIRA İLƏ, HƏR ADDIMDAN SONRA DAYAN

**Vacib qayda: hər addımı bitirdikdən sonra DAYAN, nə etdiyini, hansı faylların yaradıldığını/dəyişdirildiyini və hər hansı fərziyyə etdiyini qısaca xülasə et. Mənim təsdiqim olmadan növbəti addıma keçmə.**

1. **Scaffolding** — Solution + 5 layihə yarat, NuGet paketlərini quraşdır, DI konteynerini `Program.cs`-də qur, `appsettings.json` ilə connection string idarəetməsi (heç bir connection string kodda hardcode olunmamalıdır).
2. **Domain qatı** — Yuxarıdakı sxemə uyğun entity sinifləri yaz.
3. **Infrastructure qatı** — EF Core `DbContext` (SQL Server üçün) + ayrıca `LocalDbContext` (SQLite üçün), ilk migration, `IRepository<T>` + `IUnitOfWork` interfeys və implementasiyaları.
4. **Autentifikasiya/RBAC** — Login forması, parol hash-lənməsi (BCrypt), rol-əsaslı menyu görünürlüğü.
5. **Satış modulu** — Checkout ekranı: barkod input, səbət, çəkili məhsul dəstəyi (mock `IScaleReader` ilə), ödəniş seçimi, qəbz generasiyası (mock `IReceiptPrinter` ilə), offline-first yazma məntiqi.
6. **İnventar modulu** — Məhsul CRUD ekranı, stok səviyyəsi görüntüləmə, etiket çapı (mock `ILabelPrinter`, ZPL formatını konsola/fayla yazan test implementasiyası).
7. **Növbə idarəetməsi** — Növbə açma/bağlama, X-hesabatı, Z-hesabatı ekranı.
8. **Sadə hesabat** — Günlük satış siyahısı ekranı (cədvəl + ümumi məbləğ).
9. **Unit testlər** — `SaleService`, `InventoryService` üçün əsas ssenarilər (uğurlu satış, kifayət qədər stok olmadıqda rədd, snapshot düzgünlüyü).

## QADAĞALAR

- Faza 2/3 funksiyalarına (loyalty, promosiya mühərriki, çoxfilial, real NKA inteqrasiyası) toxunma — yalnız interfeys/hook nöqtələri qoy.
- UI-dan birbaşa `DbContext` və ya SQL sorğusu çağırma.
- Şəbəkə mövcud olmadıqda satışı bloklama.
- Real hardware SDK-ları quraşdırmağa cəhd etmə — hamısı mock/interfeys arxasında qalmalıdır.
- Mənim təsdiqim olmadan addımları birləşdirib irəli getmə.

## FAZA 1 — QƏBUL MEYARI

- [ ] Bütün layihə build olur, xəbərdarlıqsız
- [ ] Mock rejimdə uçdan-uca satış (login → barkod skan → ödəniş → qəbz) işləyir
- [ ] Şəbəkə kəsik simulyasiyasında satış lokal saxlanır və "sync gözləyir" statusunda görünür
- [ ] Növbə bağlananda Z-hesabatı düzgün rəqəmlərlə generasiya olunur
- [ ] `SaleItem`-də snapshot sahələri məhsul qiyməti dəyişəndən sonra da köhnə dəyəri saxlayır (test bunu yoxlamalıdır)
- [ ] Unit testlərin hamısı keçir

---
Başla: 1-ci addımdan (Scaffolding). Bitirdikdən sonra dayan və xülasə ver.
