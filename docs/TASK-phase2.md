# İMPLEMENTASİYA PROMPTU — Market POS Sistemi (Faza 2)

# Faza 1-in tam bitmiş və qəbul meyarlarını keçmiş olduğunu fərz edir. Eyni qovluqda PRD.md və TASK-phase1.md mövcud olmalıdır

## KONTEKST

Faza 1 tamamlanıb: Satış, əsas İnventar, əsas RBAC, Növbə idarəetməsi, sadə hesabat
işləkdir, bütün hardware interfeysləri (`IBarcodeScanner`, `IScaleReader`,
`IReceiptPrinter`, `ILabelPrinter`) mock implementasiya arxasındadır.

Faza 1-dəki bütün memarlıq qaydaları (qatlı struktur, Repository+UoW, snapshot
pattern, offline-first, async/await, DI) **dəyişmədən qüvvədədir**. Bu sənəd
yalnız Faza 2-yə aid ƏLAVƏ qaydaları və işləri təsvir edir.

## FAZA 2-NİN ƏHATƏSİ

1. CRM / Sadiqlik proqramı
2. Endirim/Promosiya mühərriki
3. Tam işçi idarəetməsi (PIN giriş, performans hesabatı)
4. Real hardware inteqrasiyası (mock → real keçid)

## YENİ TEXNİKİ ASILILIQLAR

| Paket | Məqsəd |
|---|---|
| `System.IO.Ports` | Tərəzi (RS-232) rabitəsi |
| ESC/POS raw komanda kitabxanası (və ya öz helper class-ınız) | Qəbz printeri |
| Win32 `RawPrinterHelper` və ya TCP socket (port 9100) | Zebra ZPL çapı |
| `BCrypt.Net-Next` (artıq Faza 1-də var) | PIN hash-lənməsi |

## VERİLƏNLƏR BAZASI ƏLAVƏLƏRİ
Customers(Id, FullName, Phone, Email, BirthDate, LoyaltyPointsBalance, TierId, CreatedAt)
LoyaltyTiers(Id, Name, MinSpendThreshold, PointsMultiplier, PerksDescription)
LoyaltyTransactions(Id, CustomerId, SaleId?, PointsChange, Reason, Timestamp)
Promotions(Id, Name, Type[PercentDiscount|FixedDiscount|BOGO|CouponCode],
Value, StartDate, EndDate, IsExclusive, Priority, IsActive)
PromotionRules(Id, PromotionId,
RuleType[MinPurchaseAmount|MinQuantity|CategoryId|CustomerTierId|TimeWindow],
RuleValue)
CouponCodes(Id, PromotionId, Code, MaxUsageTotal, UsageCount, ExpiresAt)
PromotionRedemptions(Id, PromotionId, SaleId, CustomerId?, DiscountAmount, RedeemedAt)
Employees: + PinHash (nullable, PIN-əsaslı sürətli giriş üçün)
Sales: + CustomerId (nullable FK)

## MEMARLIQ QAYDALARI — FAZA 2-YƏ ƏLAVƏ

1. **Promosiya mühərriki ayrıca domain servis olmalıdır** (`IPromotionEngine`),
   Checkout axınından asılı olmamalı — sonradan mobil POS və ya self-checkout
   (Faza 4) da eyni mühərrikdən istifadə edə bilsin.
2. **Qayda qiymətləndirmə ardıcıllığı deterministik olmalıdır**: `Priority`
   sahəsinə görə sırala, `IsExclusive = true` olan promosiya tətbiq olunduqda
   qalan bütün promosiyaları həmin çek üçün bağla. Bu məntiqi Chain of
   Responsibility və ya sadə strategiya siyahısı ilə tət­biq edin — həlli
   siz seçin, amma **nəticə hər zaman reproduksiya oluna bilən olmalıdır**
   (eyni səbət + eyni aktiv promosiyalar → hər zaman eyni endirim).
3. **Yekun qiymət heç vaxt mənfi ola bilməz** — endirim yekunu `LineTotal`-dan
   böyük olarsa, sistem 0-da kəsməli və bunu audit logunda qeyd etməlidir.
4. **Hardware: mock ilə real arasında konfiqurasiya keçidi** —
   `appsettings.json`-da `"Hardware": { "UseMock": true/false }`. DI
   konteynerində bu bayrağa görə real və ya mock implementasiya inject
   olunur. Mock-u **silməyin** — CI/test mühitində hələ də lazımdır.
5. **PIN girişi ayrıca autentifikasiya yolu**, tam username/parol axınını
   əvəz etmir, sürətli kassir dəyişimi üçün əlavədir (məs. növbə daxilində
   fasilə zamanı).

## İCRA ADDIMLARI — SIRA İLƏ, HƏR ADDIMDAN SONRA DAYAN

1. **CRM domain + migration** — `Customer`, `LoyaltyTier`, `LoyaltyTransaction`
   entity-ləri, ilkin 3 səviyyəli tier data seed (Silver/Gold/Platinum).
2. **Müştəri idarəetmə ekranı** — CRUD, telefon/ad üzrə axtarış, alış
   tarixçəsi görünüşü.
3. **Sadiqlik məntiqi** — `LoyaltyService.EarnPoints()` və `RedeemPoints()`,
   Checkout tamamlanma hadisəsinə hook (Observer pattern və ya domain event).
4. **Promosiya domeni** — `Promotion`, `PromotionRule`, `CouponCode`
   entity-ləri + `IPromotionEngine.Evaluate(Cart, Customer?)` interfeysi.
5. **Promosiya idarəetmə ekranı** (admin/meneceri) — yaratma, redaktə,
   aktiv/passiv etmə, tarix aralığı təyini.
6. **Checkout inteqrasiyası** — səbət dəyişdikcə promosiyaların avtomatik
   yenidən qiymətləndirilməsi, kupon kodu əl ilə daxiletmə sahəsi.
7. **Tam işçi idarəetməsi** — PIN giriş UI (rəqəm klaviaturası), performans
   hesabatı ekranı (satış həcmi, əməliyyat sayı, orta çek — sorğu əsaslı
   view, ayrıca cədvəl yaratmayın).
8. **Real barkod skaner** — HID keyboard-wedge tutma məntiqi: xarakterlər
   arası vaxtı ölçün (adətən < 50ms simvollar arası = skaner, daha yavaş =
   insan yazısı), Enter ilə tamamlanma.
9. **Real tərəzi** — `SerialPort` əsaslı `IScaleReader` implementasiyası,
   konfiqurasiya edilə bilən baud rate/parity (cihaz modelinə görə fərqlənir),
   stabillik bayrağının emalı (bəzi tərəzilər çəki sabitləşənə qədər "unstable"
   göndərir).
10. **Real qəbz printeri** — ESC/POS raw komandalar (kəsmə, bold, kassa
    siyirtməsi pulse siqnalı).
11. **Real Zebra etiket printeri** — ZPL sətirlərinin raw printer queue-ya
    (Win32) və ya şəbəkə üzərindən 9100 portuna göndərilməsi. Mövcud ZPL
    şablon təcrübənizi buraya köçürün.
12. **Testlər** — promosiya stacking/exclusivity ssenariləri, mənfi qiymət
    qorunması, loyalty bal hesablanması, hardware üçün loopback/simulyasiya
    testləri.

## QADAĞALAR

- Faza 3/4 əhatəsinə (çoxfilial, real NKA, BI, mobil) toxunmayın.
- Mock hardware implementasiyalarını silməyin və ya pozmayın.
- Promosiya qiymətləndirməsini UI qatına yazmayın — bu, Application/Domain
  qatında xalis məntiq olmalıdır ki, test edilə bilsin.
- PIN-i düz mətn (plain text) saxlamayın.

## QƏBUL MEYARI

- [ ] Eyni səbətə iki eksklüziv promosiya tətbiq edilə bilmir
- [ ] Stackable promosiyalar Priority ardıcıllığı ilə düzgün toplanır
- [ ] Loyalty balı satışdan sonra düzgün artır, qaytarmada düzgün azalır
- [ ] `UseMock: false` olduqda real Zebra printerə ZPL uğurla göndərilir
- [ ] Real tərəzidən gələn çəki Checkout-da 1 saniyə ərzində əks olunur
- [ ] Bütün yeni servislər üçün unit testlər keçir

---
Başla: 1-ci addımdan. Bitirdikdən sonra dayan və xülasə ver.
