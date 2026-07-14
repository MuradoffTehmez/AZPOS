# PRD — Market POS Sistemi

| Sahə | Dəyər |
|---|---|
| Sənəd növü | Product Requirements Document (PRD) |
| Versiya | 1.0 (Draft) |
| Tarix | 2026-07-10 |
| Müəllif / Product Owner | Təhməz |
| Status | Review üçün hazır |
| Platforma | Windows (C# / .NET, WinForms və ya WPF) |

## Məzmun

1. İcmal
2. Məqsəd və kontekst
3. Məqsədlər və uğur meyarları (KPI)
4. Əhatə dairəsi (Scope)
5. İstifadəçi rolları
6. Fərziyyələr və məhdudiyyətlər
7. Funksional tələblər
8. Qeyri-funksional tələblər
9. Sistem arxitekturası
10. Verilənlər modeli
11. İnteqrasiya tələbləri
12. UI/UX tələbləri
13. Hesabat tələbləri
14. Hüquqi/uyğunluq tələbləri (Azərbaycan)
15. Buraxılış planı (Roadmap)
16. Risklər və azaldılma strategiyası
17. Asılılıqlar
18. Qəbul meyarları (Definition of Done)
19. Lüğət
20. Açıq suallar

---

## 1. İcmal

Bu sənəd Azərbaycan bazarı üçün nəzərdə tutulan, C#/.NET əsaslı, market/supermarket tipli pərakəndə satış nöqtələri üçün peşəkar POS (Point of Sale) sisteminin tam funksional və texniki tələblərini müəyyən edir. Sistem satış, inventar, müştəri, işçi, maliyyə və fiskal əməliyyatları vahid platformada birləşdirir, offline-first arxitektura üzərində qurulur və Azərbaycanın NKA/e-Kassa tələblərinə uyğunlaşdırılır.

## 2. Məqsəd və kontekst

**Problem:** Kiçik və orta market/supermarketlərdə istifadə olunan mövcud həllər ya çox bahalıdır (xarici bulud-əsaslı sistemlər), ya da lokal tələblərə (NKA inteqrasiyası, Azərbaycan dili, tərəzi/PLU dəstəyi) cavab vermir, ya da köhnəlmiş, istifadəçi dostu olmayan interfeysə malikdir.

**Həll:** Yerli bazarın real ehtiyaclarına (fiskal qanunvericilik, çəki ilə satış, offline dayanıqlılıq) tam cavab verən, genişlənə bilən, müasir arxitekturaya malik POS sistemi.

**Biznes əsaslandırma:** Marketin gündəlik əməliyyatlarının sürətləndirilməsi, insan səhvlərinin azaldılması, inventar itkisinin (shrinkage) azaldılması və qanunvericiliyə tam uyğunluğun təmin edilməsi.

## 3. Məqsədlər və uğur meyarları (KPI)

| Meyar | Hədəf |
|---|---|
| Orta tranzaksiya vaxtı | < 2 saniyə (normal şəbəkə şəraitində) |
| Sistem uptime | ≥ 99.5% (planlaşdırılmış texniki xidmət xaric) |
| Offline → online sinxronizasiya uğuru | 100% (heç bir satış itməməli) |
| Stok dəqiqliyi | ≥ 98% (fiziki sayım ilə sistem arasında) |
| Yeni kassirin təlim müddəti | ≤ 30 dəqiqə (əsas satış əməliyyatları üçün) |
| İnventar israfı (spoilage) azalması | ≥ 20% (FEFO tətbiqindən sonra, 6 ay ərzində) |
| NKA fiskal ötürmə uğuru | 100% (real-time, gecikmə < 5 saniyə) |

## 4. Əhatə dairəsi (Scope)

### 4.1 Daxildir (MVP və sonrakı fazalar)

Satış/checkout, inventar idarəetməsi, müştəri (CRM), işçi idarəetməsi, maliyyə hesabatları, endirim mühərriki, təchizatçı/satınalma, çoxfilial dəstəyi, hardware inteqrasiyası (skaner, tərəzi, printer), NKA/fiskal inteqrasiya, backup/audit, bildiriş sistemi.

### 4.2 Xaricdir (bu versiyada deyil)

- Tam e-ticarət platforması (yalnız gələcək inteqrasiya nöqtəsi nəzərdə tutulur)
- Öz ödəniş prosessoru/eKvayrinq həlli (mövcud bank POS-terminalları ilə inteqrasiya olunur, yeni ödəniş sistemi yaradılmır)
- Mobil tətbiq (native iOS/Android) — yalnız Faza 4-də qiymətləndiriləcək
- Anbar idarəetmə sistemi (WMS) səviyyəsində robototexnika/RFID inteqrasiyası

## 5. İstifadəçi rolları

| Rol | Əsas məsuliyyət | Sistemdə giriş səviyyəsi |
|---|---|---|
| Kassir | Satış, qaytarma, növbə açma/bağlama | Məhdud — yalnız Satış modulu |
| Meneceri (mağaza) | Endirim təsdiqi, hesabatlar, işçi idarəetməsi, inventarizasiya | Geniş — filial səviyyəsində tam |
| Anbar/Stok məsulu | Mal qəbulu, inventarizasiya, PO | Orta — İnventar + Satınalma |
| Baş ofis Admin | Çoxfilial konfiqurasiya, qiymət/promosiya yayımı, sistem tənzimləmələri | Tam — bütün filiallar |
| Mühasib | Maliyyə hesabatlarının ixracı | Yalnız oxuma — Maliyyə modulu |

## 6. Fərziyyələr və məhdudiyyətlər

- Sistem əsasən Windows mühitində, lokal şəbəkədə işləyən terminallar üzərində istifadə olunacaq.
- İnternet bağlantısı qeyri-sabit ola bilər — offline-first arxitektura məcburidir.
- NKA hardware-i ayrıca satın alınır və Vergilər Nazirliyində qeydiyyatdan keçməlidir; bu sənəd yalnız proqram tərəfini əhatə edir.
- SQL Server lisenziyası mövcud fərz olunur (Express versiyası kiçik filiallar üçün kifayətdir).

## 7. Funksional tələblər

Prioritet: **M** = Must-have (MVP), **S** = Should-have (Faza 2), **C** = Could-have (Faza 3+)

### 7.1 Satış / Checkout modulu (FR-SALE)

| ID | Tələb | Prioritet |
|---|---|---|
| FR-SALE-01 | Barkod skanı (1D/2D) ilə məhsulu səbətə əlavə etmək | M |
| FR-SALE-02 | Barkodsuz axtarış — ad, SKU və ya kateqoriya üzrə | M |
| FR-SALE-03 | Çəki ilə satılan məhsullar üçün tərəzi inteqrasiyası (PLU kodu ilə avtomatik qiymət hesablama) | M |
| FR-SALE-04 | Səbətdə miqdar/qiymət redaktəsi (yalnız icazəli rollar) | M |
| FR-SALE-05 | Çoxlu ödəniş üsulu — nağd, kart, split payment (bir çekin bir neçə üsulla ödənməsi) | M |
| FR-SALE-06 | Satışı müvəqqəti dayandırma (hold) və bərpa etmə (resume) | S |
| FR-SALE-07 | Tam/qismən qaytarma, orijinal çekə istinadla | M |
| FR-SALE-08 | Qəbzin thermal printerdə çapı | M |
| FR-SALE-09 | Qəbzin email/SMS vasitəsilə göndərilməsi | S |
| FR-SALE-10 | Offline rejimdə satışın kəsintisiz davam etməsi | M |
| FR-SALE-11 | Sətir və çek səviyyəsində manual endirim tətbiqi (icazə əsaslı) | M |
| FR-SALE-12 | Sifariş sətrində qiymət/ad "snapshot" saxlanması (tarixi hesabatların qorunması üçün) | M |

### 7.2 İnventar (stok) idarəetməsi (FR-INV)

| ID | Tələb | Prioritet |
|---|---|---|
| FR-INV-01 | Məhsul kataloqu — SKU, barkod, kateqoriya, vahid (ədəd/kq/litr) üzrə tam CRUD | M |
| FR-INV-02 | Hər satışdan sonra stokun real-time azalması/artması | M |
| FR-INV-03 | Partiya (lot) və son istifadə tarixi izləmə | M |
| FR-INV-04 | FEFO-əsaslı satış prioritetləşdirmə (son bitmə tarixli partiya əvvəl satılır) | S |
| FR-INV-05 | Minimum stok həddi xəbərdarlığı və avtomatik PO təklifi | S |
| FR-INV-06 | Cycle count (seçmə) və tam fiziki inventarizasiya rejimi | M |
| FR-INV-07 | Barkod/qiymət etiketinin Zebra printerdə ZPL formatında çapı | M |
| FR-INV-08 | Bir məhsula çoxlu barkod əlaqələndirmə | S |
| FR-INV-09 | Filiallar/anbarlar arası stok ayrıca izlənməsi | S |

### 7.3 Müştəri idarəetməsi / CRM (FR-CRM)

| ID | Tələb | Prioritet |
|---|---|---|
| FR-CRM-01 | Müştəri profili yaratma və redaktə (ad, telefon, doğum günü) | M |
| FR-CRM-02 | Müştərinin alış tarixçəsinin görüntülənməsi | M |
| FR-CRM-03 | Sadiqlik balının toplanması və xərclənməsi | S |
| FR-CRM-04 | Səviyyə (Silver/Gold/Platinum) əsaslı fərqli endirim qaydaları | C |
| FR-CRM-05 | Doğum günü/xüsusi tarix əsaslı avtomatik endirim | C |
| FR-CRM-06 | SMS/email marketinq siyahısının ixracı | C |

### 7.4 İşçi idarəetməsi (FR-EMP)

| ID | Tələb | Prioritet |
|---|---|---|
| FR-EMP-01 | Rol-əsaslı giriş nəzarəti (RBAC) — Kassir/Meneceri/Admin/Mühasib | M |
| FR-EMP-02 | Növbə (shift) açma — başlanğıc nəğd məbləğinin bəyanı | M |
| FR-EMP-03 | Növbə bağlama — faktiki nəğd sayı ilə sistem gözləntisinin müqayisəsi | M |
| FR-EMP-04 | X-hesabatı (aralıq) və Z-hesabatı (gün sonu yekun) generasiyası | M |
| FR-EMP-05 | PIN və ya kart əsaslı sürətli kassir girişi | S |
| FR-EMP-06 | Kassir üzrə performans hesabatı (satış həcmi, əməliyyat sayı) | S |

### 7.5 Maliyyə və hesabatlıq (FR-FIN)

| ID | Tələb | Prioritet |
|---|---|---|
| FR-FIN-01 | Gündəlik/həftəlik/aylıq satış hesabatı | M |
| FR-FIN-02 | Kateqoriya/məhsul üzrə mənfəət marjası hesabatı | S |
| FR-FIN-03 | ƏDV və digər tətbiq olunan vergilərin avtomatik hesablanması | M |
| FR-FIN-04 | Hesabatların Excel/PDF formatında ixracı | M |
| FR-FIN-05 | Xarici mühasibatlıq proqramına (1C və s.) ixrac formatı | C |

### 7.6 Endirim/Promosiya mühərriki (FR-PROMO)

| ID | Tələb | Prioritet |
|---|---|---|
| FR-PROMO-01 | Faiz və ya sabit məbləğ əsaslı endirim yaratma | M |
| FR-PROMO-02 | BOGO (Al X – Al Y pulsuz/endirimli) promosiyaları | S |
| FR-PROMO-03 | Kupon kodu generasiyası və kassada tətbiqi | S |
| FR-PROMO-04 | Endirimlərin stacking/exclusivity qaydaları (hansı endirimlər birləşə bilər) | S |
| FR-PROMO-05 | Vaxt-əsaslı promosiyalar (məs. həftə sonu, saat aralığı) | C |

### 7.7 Təchizatçı və satınalma (FR-PUR)

| ID | Tələb | Prioritet |
|---|---|---|
| FR-PUR-01 | Təchizatçı bazasının idarə edilməsi (CRUD) | M |
| FR-PUR-02 | Purchase Order (PO) yaratma və göndərmə | S |
| FR-PUR-03 | Mal qəbulu — PO-lu və PO-suz | M |
| FR-PUR-04 | Təchizatçı üzrə tarixi alış qiymətlərinin saxlanması | S |

### 7.8 Çoxfilial idarəetməsi (FR-BRANCH)

| ID | Tələb | Prioritet |
|---|---|---|
| FR-BRANCH-01 | Mərkəzi paneldən bütün filialların stok/satış icmalına baxış | S |
| FR-BRANCH-02 | Filiallar arası stok transferi | S |
| FR-BRANCH-03 | Mərkəzləşdirilmiş qiymət və promosiya yayımı | C |
| FR-BRANCH-04 | Filial-səviyyəli istifadəçi icazələrinin ayrılması | S |

### 7.9 Hardware inteqrasiyası (FR-HW)

| ID | Tələb | Prioritet |
|---|---|---|
| FR-HW-01 | Barkod skaner dəstəyi (USB HID və Bluetooth) | M |
| FR-HW-02 | Tərəzi inteqrasiyası (RS-232 seriya və/və ya USB protokolu) | M |
| FR-HW-03 | Thermal qəbz printeri dəstəyi (ESC/POS) | M |
| FR-HW-04 | Zebra etiket printeri dəstəyi (ZPL) | M |
| FR-HW-05 | Kassa siyirtməsinin (cash drawer) avtomatik açılması | M |
| FR-HW-06 | Müştəri pole display dəstəyi | C |
| FR-HW-07 | Bank POS-terminalı ilə proqram səviyyəsində rabitə | M |

### 7.10 Fiskal / NKA inteqrasiyası (FR-FISCAL)

| ID | Tələb | Prioritet |
|---|---|---|
| FR-FISCAL-01 | NKA cihazı vasitəsilə fiskal çekin generasiyası | M |
| FR-FISCAL-02 | Əməliyyatların Vergilər Nazirliyinin mərkəzi sisteminə real-time ötürülməsi | M |
| FR-FISCAL-03 | Topdansatış əməliyyatları üçün elektron qaimə-fakturanın generasiyası | S |
| FR-FISCAL-04 | NKA və bank POS-terminalının vahid əməliyyat sistemində inteqrasiyası (nağdsız ödəmələr üçün) | M |
| FR-FISCAL-05 | `IFiscalDevice` abstraksiyası — real cihaz mövcud olmadıqda test/mock rejimi | S |

### 7.11 Backup və Audit (FR-SEC)

| ID | Tələb | Prioritet |
|---|---|---|
| FR-SEC-01 | Verilənlər bazasının avtomatik gündəlik ehtiyat nüsxələnməsi | M |
| FR-SEC-02 | Bütün kritik əməliyyatların (qiymət dəyişikliyi, endirim, silmə, giriş) audit logu | M |
| FR-SEC-03 | Session timeout və təhlükəsiz autentifikasiya | M |
| FR-SEC-04 | Şifrələrin hash formatında saxlanması (bcrypt/PBKDF2) | M |
| FR-SEC-05 | Həssas məlumatların (maaş, şəxsi identifikasiya) serializer/DTO səviyyəsində maskalanması | S |

### 7.12 Bildiriş sistemi (FR-NOTIF)

| ID | Tələb | Prioritet |
|---|---|---|
| FR-NOTIF-01 | Aşağı stok həddi xəbərdarlığı | S |
| FR-NOTIF-02 | Son istifadə tarixi yaxınlaşan məhsullar üçün xəbərdarlıq | S |
| FR-NOTIF-03 | Sinxronizasiya xətası/offline müddət bildirişi | M |

## 8. Qeyri-funksional tələblər (NFR)

### 8.1 Performans

| ID | Tələb |
|---|---|
| NFR-PERF-01 | Barkod skanından qiymətin ekranda görünməsinə qədər < 300ms |
| NFR-PERF-02 | Normal şəbəkə şəraitində bir tranzaksiya < 2 saniyə |
| NFR-PERF-03 | 10,000+ SKU-lu kataloqda axtarış performans itkisi olmadan işləməli |

### 8.2 Etibarlılıq və Offline dayanıqlılıq

| ID | Tələb |
|---|---|
| NFR-REL-01 | İnternet kəsintisində minimum 24 saat tam offline funksionallıq |
| NFR-REL-02 | Sinxronizasiya zamanı heç bir tranzaksiya itməməli və ya təkrarlanmamalı |
| NFR-REL-03 | Planlaşdırılmamış downtime aylıq < 0.5% |

### 8.3 Təhlükəsizlik

| ID | Tələb |
|---|---|
| NFR-SEC-01 | Server rejimində bütün şəbəkə ünsiyyəti TLS ilə şifrələnməli |
| NFR-SEC-02 | Minimum 3 səviyyəli RBAC (Kassir/Meneceri/Admin) |
| NFR-SEC-03 | Audit logu append-only olmalı — heç bir rol tərəfindən dəyişdirilə bilməz |

### 8.4 İstifadəçilik (Usability)

| ID | Tələb |
|---|---|
| NFR-UX-01 | Yeni kassir 30 dəqiqəlik təlimdən sonra əsas satış əməliyyatlarını sərbəst apara bilməli |
| NFR-UX-02 | Toxunma-ekran (touchscreen) uyğun, böyük düymələrlə interfeys |
| NFR-UX-03 | Təcrübəli kassirlər üçün klaviatura qısayolları |
| NFR-UX-04 | Bütün interfeys Azərbaycan dilində, i18n strukturu ilə (gələcək dil əlavəsi üçün) |

### 8.5 Miqyaslana bilmə

| ID | Tələb |
|---|---|
| NFR-SCALE-01 | Bir filialda 50-ə qədər paralel kassa terminalı dəstəyi |
| NFR-SCALE-02 | Əlavə inkişaf tələb etmədən çoxfilial arxitekturasına keçid imkanı |

## 9. Sistem arxitekturası

**Qatlı arxitektura:**
Presentation (WinForms/WPF) → Business Logic (Services) → Data Access (Repository + Unit of Work) → Database (SQL Server)

**Əsas prinsiplər:**

- **Offline-first** — hər terminalda lokal keş (SQLite/SQL Server LocalDB) + fon rejimində mərkəzi serverlə sinxronizasiya növbəsi.
- **Snapshot pattern** — satış sətirlərində əməliyyat anındakı qiymət/ad ayrıca saxlanılır ki, tarixi hesabatlar məhsul qiyməti dəyişəndə pozulmasın.
- **POS Client → Business API → Database** modeli — terminalların bazaya birbaşa deyil, aralıq servis qatı üzərindən qoşulması (xüsusilə çoxfilial ssenarisində vacibdir).

**Dizayn pattern-ləri:**

| Pattern | Tətbiq sahəsi |
|---|---|
| Repository + Unit of Work | Data Access qatı |
| Strategy | Ödəniş üsulları, fiskal cihaz inteqrasiyası |
| Observer | Stok azalanda bildiriş tetiklənməsi |
| Factory | Çek/etiket sənəd generasiyası (ZPL, ESC/POS) |
| Decorator | Endirim mühərrikinin qat-qat tətbiqi |

**Texnologiya stack-i:** C# / .NET, WinForms və ya WPF, SQL Server (mərkəzi), SQLite/LocalDB (lokal keş), MaterialSkin və ya oxşar UI kitabxanası.

## 10. Verilənlər modeli (əsas cədvəllər)

`Products`, `ProductBatches`, `Categories`, `Inventory`, `Sales`, `SaleItems`, `Customers`, `LoyaltyPoints`, `Employees`, `Roles`, `Shifts`, `CashDrawerLogs`, `Suppliers`, `PurchaseOrders`, `PurchaseOrderItems`, `Branches`, `StockTransfers`, `Promotions`, `PromotionRules`, `AuditLogs`, `FiscalTransactions`

> Tam ER-diaqram (əlaqələr, açarlar, tiplər) ayrıca texniki dizayn sənədində hazırlanmalıdır — bu PRD-nin əhatəsi xaricindədir.

## 11. İnteqrasiya tələbləri

| Kateqoriya | Detal |
|---|---|
| Barkod skaner | USB HID, Bluetooth |
| Tərəzi | RS-232 seriya (əsas), USB (alternativ) |
| Qəbz printeri | Thermal, ESC/POS protokolu |
| Etiket printeri | Zebra, ZPL formatı |
| Bank POS-terminalı | Bank tərəfindən təqdim olunan SDK/protokol üzrə |
| NKA / e-Kassa | Vergilər Nazirliyinin mərkəzi sistemi ilə real-time bağlantı (e-kassa.gov.az standartları) |

## 12. UI/UX tələbləri

- Əsas satış ekranı — bir baxışdan səbət, ümumi məbləğ və ödəniş düymələri görünməli
- Toxunma-ekran optimallaşdırılmış böyük düymələr, minimal klik sayı ilə çek tamamlama
- Xəta mesajları aydın, Azərbaycan dilində, texniki jarqonsuz
- Rol əsaslı menyu — kassir yalnız ona aid funksiyaları görməli
- Rəng kodlaşdırması — uğur (yaşıl), xəbərdarlıq (sarı), xəta (qırmızı)

## 13. Hesabat tələbləri

- Gün sonu Z-hesabatı (məcburi, çap edilə bilən)
- Məhsul/kateqoriya üzrə satış analitikası
- İşçi performans hesabatı
- Stok hərəkəti hesabatı (giriş/çıxış/transfer)
- Vergi hesabatı (ƏDV üzrə)

## 14. Hüquqi/uyğunluq tələbləri (Azərbaycan)

- Sistem NKA (Nəzarət-Kassa Aparatı) ilə inteqrasiya olunmalı; NKA fiskal yaddafa malik, vergi orqanlarında qeydiyyatdan keçən avadanlıqdır.
- Yeni nəsil NKA-lar vergi orqanının mərkəzi sisteminə real-vaxt rejimində qoşulur; bu tələb artıq iri market şəbəkələrində (Bravo, Bizim Market, OBA Market və s.) tətbiq olunmaqdadır.
- Nağdsız ödəmələr bank POS-terminalı vasitəsilə aparıldıqda, terminal NKA ilə vahid əməliyyat sistemində inteqrasiya olunmalıdır.
- Topdansatış/B2B əməliyyatlar üçün elektron qaimə-faktura generasiyası tələb oluna bilər.

## 15. Buraxılış planı (Roadmap)

| Faza | Məzmun | Prioritet mənbəyi |
|---|---|---|
| **Faza 1 — MVP** | Satış, əsas inventar, sadə hesabat, tək kassa, əsas RBAC | Bütün "M" (Must) tələblər |
| **Faza 2** | CRM/loyalty, endirim mühərriki, tam işçi/növbə idarəetməsi, etiket çapı | "S" (Should) tələblər |
| **Faza 3** | Çoxfilial, NKA/fiskal tam inteqrasiya, offline-sync yetkinləşdirmə | Qalan "S" + "C" (Could) |
| **Faza 4** | Analitika/BI paneli, mobil POS, self-checkout, e-ticarət inteqrasiyası | Gələcək genişlənmə |

## 16. Risklər və azaldılma strategiyası

| Risk | Ehtimal | Təsir | Azaldılma |
|---|---|---|---|
| İnternet kəsintiləri (regional) | Yüksək | Orta | Offline-first arxitektura, lokal keş |
| NKA sertifikatlaşdırma gecikməsi | Orta | Yüksək | Erkən mərhələdə vendor/vergi orqanı ilə əlaqə |
| Hardware uyğunsuzluğu (skaner/tərəzi) | Orta | Orta | Test edilmiş cihaz siyahısının əvvəlcədən müəyyənləşdirilməsi |
| Məlumat itkisi | Aşağı | Yüksək | Avtomatik backup + append-only audit log |
| İşçilərin yeni sistemə adaptasiyası | Orta | Orta | Sadə UI, 30 dəqiqəlik təlim proqramı |

## 17. Asılılıqlar

- NKA-sertifikatlı hardware təchizatçısı
- Bank POS-terminalı üçün API/SDK sənədləşməsi
- Zebra printer SDK/ZPL sənədləşməsi
- SQL Server lisenziyası (və ya Express versiyası)

## 18. Qəbul meyarları (Definition of Done — MVP)

- [ ] Bütün FR-SALE, FR-INV, FR-EMP "Must" tələbləri funksional və test edilib
- [ ] Offline rejimdə minimum 24 saat kəsintisiz satış test edilib
- [ ] NKA inteqrasiyası test mühitində fiskal çek generasiya edir
- [ ] Bir kassirin uçdan-uca satış əməliyyatı 2 saniyədən az başa çatır
- [ ] Bütün kritik əməliyyatlar audit logunda görünür
- [ ] Gün sonu Z-hesabatı düzgün generasiya olunur və çap edilir

## 19. Lüğət

| Termin | Mənası |
|---|---|
| POS | Point of Sale — satış nöqtəsi sistemi |
| SKU | Stock Keeping Unit — məhsulun unikal identifikatoru |
| PLU | Price Look-Up — çəkili məhsullar üçün qiymət kodu |
| FEFO | First-Expired-First-Out — son bitmə tarixli məhsulun əvvəl satılması |
| NKA | Nəzarət-Kassa Aparatı — fiskal kassa aparatı |
| ZPL | Zebra Programming Language — etiket çapı formatı |
| PO | Purchase Order — satınalma sifarişi |
| RBAC | Role-Based Access Control — rol-əsaslı giriş nəzarəti |
| MVP | Minimum Viable Product — minimal işlək versiya |

## 20. Açıq suallar

- Hansı bank(lar)ın POS-terminalı ilə inteqrasiya prioritetdir?
- Hansı NKA vendoru ilə işləniləcək (sertifikatlı təchizatçı seçimi)?
- MVP mərhələsində çoxfilial dəstəyi zəruridirmi, yoxsa tək filialla başlanacaq?
- Mühasibatlıq proqramı kimi hansı sistem istifadə olunur (1C, Excel, başqa)?
