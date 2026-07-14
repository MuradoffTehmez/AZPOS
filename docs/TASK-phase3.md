# İMPLEMENTASİYA PROMPTU — Market POS Sistemi (Faza 3)

# Faza 1 və Faza 2-nin tam bitmiş olduğunu fərz edir

## KONTEKST

Bu, ən kritik fazadır — Çoxfilial arxitekturası, NKA/fiskal inteqrasiya və
offline-sync yetkinləşdirməsi. NKA hissəsi Azərbaycan qanunvericiliyi ilə
bağlı olduğu üçün xüsusi diqqətlə yazılıb (bax: PRD.md bölmə 14).

**Vacib qeyd NKA haqqında:** Konkret NKA vendoru hələ seçilməyib (PRD-nin
Açıq suallar bölməsi). Ona görə bu fazada məqsəd real vendor SDK-nı inteqrə
etmək DEYİL — məqsəd vendor-aqnostik tam fiskal boru xəttini (pipeline)
qurmaq və `IFiscalDevice` kontraktını elə dəqiq yazmaqdır ki, vendor
seçiləndə yalnız BİR adapter sinfi yazmaq kifayət etsin. Real sertifikatlı
inteqrasiyanı bu promptun icrasından sonra, vendor sənədləri əlimizə
keçəndə ayrıca kiçik tapşırıq kimi edəcəyik.

## FAZA 3-ÜN ƏHATƏSİ

1. Çoxfilial (multi-branch) arxitekturası
2. Fiskal (NKA) boru xətti + e-qaimə generasiyası
3. Offline-sync yetkinləşdirməsi (konflikt həlli, idempotentlik)

## VERİLƏNLƏR BAZASI ƏLAVƏLƏRİ

Branches(Id, Name, Address, TaxId, IsActive)
-- Mövcud cədvəllərə BranchId FK əlavəsi: Employees, Sales, Shifts
-- Inventory cədvəli filial-səviyyəli olur:
BranchInventory(BranchId, ProductId, QuantityOnHand, ReorderLevel)  -- əvəz edir: Inventory
StockTransfers(Id, FromBranchId, ToBranchId, RequestedByEmployeeId,
Status[Pending|Approved|InTransit|Received|Cancelled],
CreatedAt, ReceivedAt)
StockTransferItems(Id, StockTransferId, ProductId, RequestedQty, ReceivedQty?)
FiscalTransactions(Id, SaleId, ClientTransactionGuid, NKASerialNumber?,
FiscalReceiptNumber?, FiscalSignature?,
TransmissionStatus[Pending|Sent|Confirmed|Failed|Retrying],
RetryCount, LastAttemptAt, ConfirmedAt, RawResponsePayload)
EInvoices(Id, SaleId?, RecipientTaxId, InvoiceType, Status, SubmittedAt, PayloadXml)
SyncConflictLogs(Id, EntityName, EntityId, LocalValue, ServerValue,
ResolutionStrategy, ResolvedAt, ResolvedBy)

`Sales` cədvəlinə əlavə: `ClientTransactionGuid` (client tərəfində
generasiya olunan, server-side dedup üçün unikal açar).

## MEMARLIQ QAYDALARI — FAZA 3-Ə ƏLAVƏ

1. **Bütün sorğular BranchId ilə scope olunmalıdır.** Repository qatında
   "current branch context" mexanizmi qurun (məs. `IBranchContext` — hazırkı
   terminalın bağlı olduğu filial), heç bir servis metodu filialı unudaraq
   bütün bazanı sorğulamamalıdır.
2. **Stock Transfer vəziyyət maşını** — `Pending → Approved → InTransit →
   Received`. Hər keçid ayrıca audit qeydi yaratmalıdır. `Cancelled` istənilən
   `Received`-dən əvvəlki mərhələdən mümkündür.
3. **Fiskal boru xətti həmişə asinxron və növbə-əsaslıdır** — satış
   tamamlananda `FiscalTransaction` yazısı `Pending` statusu ilə **dərhal**
   yaradılır (satışı bloklamadan), ayrıca background dispatcher servis bunu
   göndərir. Exponential backoff ilə retry (məs. 5, 15, 60, 300 saniyə),
   max cəhddən sonra `Failed` statusu + admin bildirişi.
4. **`FiscalTransaction` sırası persist olunmalıdır**, yalnız yaddaşda
   (in-memory queue) saxlanmamalıdır — tətbiq yenidən başladıqda `Pending`
   və `Retrying` statuslu yazılar avtomatik bərpa olunmalıdır.
5. **Sync idempotentliyi** — hər lokal satış `ClientTransactionGuid` ilə
   yaradılır, server bu GUID-i unikal indeks kimi yoxlayır; eyni GUID ikinci
   dəfə gələrsə, sadəcə "artıq mövcuddur" cavabı qaytarılır, təkrar yazılmır.
6. **Konflikt həlli strategiyası, entity tipinə görə fərqli:**
   - `Sale`/`SaleItem` — **append-only**, heç vaxt server tərəfindən üzərinə
     yazılmır ya da silinmir; konflikt praktiki olaraq mümkün deyil, çünki
     satış immutable-dır.
   - `BranchInventory.QuantityOnHand` — server-authoritative last-write-wins,
     amma hər dəyişiklik `SyncConflictLogs`-a yazılır ki, mənfi stok
     yarandıqda geriyə izlənə bilsin.
   - Qiymət/məhsul kataloqu dəyişiklikləri — yalnız mərkəzdən (HQ) enir,
     filial terminalı bunları heç vaxt "yaza" bilməz (bir istiqamətli axın).
7. **`IFiscalDevice` kontraktı dəqiq təyin olunmalıdır** — metodlar:
   `Task<FiscalResult> SendTransactionAsync(FiscalRequest request)`,
   `Task<FiscalStatus> CheckStatusAsync(string transactionId)`. Bu interfeys
   arxasında iki implementasiya olacaq: `SimulatedFiscalDevice` (bu fazada
   yazılacaq, real vendor davranışını təqlid edir — gecikmə, təsadüfi uğursuzluq
   nisbəti konfiqurasiya edilə bilər) və gələcək `<VendorName>FiscalDevice`
   (vendor seçiləndə əlavə olunacaq, bu fazada YAZILMAYACAQ).

## İCRA ADDIMLARI — SIRA İLƏ, HƏR ADDIMDAN SONRA DAYAN

1. **Branch domain + migration** — `Branch` entity, mövcud cədvəllərə
   `BranchId` FK əlavəsi, `IBranchContext` servisi.
2. **Inventory-ni filial-səviyyəsinə köçürmə** — `Inventory` → `BranchInventory`
   miqrasiyası, mövcud Faza 1/2 kodunda bütün istinadların yenilənməsi
   (bura diqqətlə yanaşın — geriyə uyğunluğu pozmayın, mövcud testlər
   keçməlidir).
3. **Filial admin paneli** — filial yaratma/redaktə, işçi-filial təyinatı.
4. **Stock Transfer axını** — sorğu yaratma, təsdiq, "yolda" statusu,
   qəbul ekranı (qəbul edilən miqdar sorğu miqdarından fərqli ola bilər —
   bunu dəstəkləyin).
5. **Mərkəzləşdirilmiş qiymət/promosiya yayımı** — HQ-dan bütün filiallara
   push mexanizmi (bir istiqamətli, filial bunu redaktə edə bilməz).
6. **FiscalTransaction domeni + persistent növbə** — entity, background
   dispatcher (`IHostedService` və ya oxşar), retry/backoff məntiqi.
7. **SimulatedFiscalDevice** — realistik gecikmə (500ms–3san) və konfiqurasiya
   edilə bilən uğursuzluq nisbəti (məs. appsettings-də `%5 fail rate`) ilə.
8. **E-qaimə generasiya servisi** — topdansatış işarələnmiş satışlar üçün
   XML payload strukturu (PRD-də qeyd olunan struktura uyğun, dəqiq sxem
   vendor/DVX sənədləşməsi əlimizə keçəndə dəqiqləşdiriləcək — hələlik
   məntiqi struktur və "TODO: real sxemə uyğunlaşdır" şərhi ilə).
9. **Sync konflikt mexanizmi** — `ClientTransactionGuid` dedup, entity-tipinə
   görə fərqli strategiya (yuxarı bax), `SyncConflictLogs` yazılması.
10. **Sync sağlamlıq ekranı (admin)** — hər terminal üzrə gözləyən/uğursuz
    fiskal əməliyyat və sync sayı.
11. **Testlər (bu fazada xüsusilə vacib):**
    - İki terminal eyni vaxtda offline satış edir, sonra sync olur → 0
      itki, 0 dublikat
    - Tətbiq fiskal göndərmə zamanı çökür və yenidən başladılır → `Pending`
      yazı avtomatik bərpa olunur
    - `SimulatedFiscalDevice` uğursuz olduqda retry ardıcıllığı düzgün
      işləyir və max cəhddən sonra admin bildirişi tetiklənir

## QADAĞALAR

- Real NKA vendor SDK-nı təxmin edərək (sənədsiz) implementasiya etməyin —
  bu, real cihazla işləməyən "yalançı təhlükəsizlik" yaradar. Yalnız
  simulyasiya edin.
- Heç bir konfliktdə tamamlanmış `Sale` yazısını silməyin/dəyişməyin.
- Filial terminalından mərkəzi qiymət/kataloq datasını birbaşa yazdırmayın.
- Faza 4 əhatəsinə (BI, mobil, self-checkout, e-ticarət) toxunmayın.

## QƏBUL MEYARI

- [ ] 3 fərqli filial arasında stok transferi uçdan-uca işləyir
- [ ] Fiskal növbə tətbiq restart-dan sağ çıxır (persistent)
- [ ] Simulyasiya edilmiş 3 uğursuz cəhddən sonra admin bildirişi görünür
- [ ] Eyni `ClientTransactionGuid` ilə iki dəfə göndərilən satış serverdə
      bir dəfə qeyd olunur
- [ ] Bütün Faza 1/2 testləri hələ də keçir (regressiya yoxdur)

---
Başla: 1-ci addımdan. Bitirdikdən sonra dayan və xülasə ver.
