# İMPLEMENTASİYA PROMPTU — Market POS Sistemi (Faza 4A — Analitika/BI)

# TASK-phase4a-analytics.md. Faza 1-3-ün tam bitmiş olduğunu fərz edir

## KONTEKST

Faza 1-3-ün bütün memarlıq qaydaları (qatlı struktur, Repository+UoW,
snapshot pattern, offline-first, BranchId scoping) qüvvədədir. Bu fayl
yalnız Faza 4A-ya aid işləri təsvir edir və digər 4X fazalarından asılı
deyil (paralel icra edilə bilər).

## ƏHATƏ

Read-optimized reporting qatı + daxili WinForms dashboard. Məqsəd: heç bir
analitik sorğu canlı checkout performansına təsir etməsin.

## YENİ ASILILIQ

| Paket | Məqsəd |
|---|---|
| `LiveCharts2` | WinForms chart-lar (line, bar, heatmap) |

## VERİLƏNLƏR BAZASI ƏLAVƏLƏRİ
-- Kanal ayırdedilməsi bütün 4X fazaları üçün ortaq sahədir, buradan başladılır:
Sales: + Channel enum [InStore|MobileTablet|SelfCheckout|Online]  (default: InStore, backfill mövcud sətirlər)
DailySalesSummary(BranchId, SummaryDate, Channel, TotalRevenue, TotalTransactions,
TotalItemsSold, TotalDiscount, TotalTax, AvgBasketSize)
ProductSalesSummary(BranchId, ProductId, SummaryDate, QuantitySold, Revenue, ProfitMargin)
HourlySalesHeatmap(BranchId, DateHour, TransactionCount, Revenue)
ReportAggregationRuns(Id, RunDate, StartedAt, CompletedAt?, Status, RowsProcessed)

> Qeyd: 4B/4C/4D fayllarında "`Sales.Channel` artıq mövcud olmalıdır" — əgər
> bu fazanı icra etməmisinizsə, həmin fazanın özündə bu sahəni əlavə edin
> (hər migration additiv və idempotent olmalıdır).

## MEMARLIQ QAYDALARI

1. Dashboard **yalnız** summary cədvəllərindən oxuyur — istisna: "bugün"
   plitəsi (canlı ədəd) cari günün indekslənmiş, dar sorğusu ilə hesablanır.
2. Aggregation servisi ayrıca `MarketPOS.Reporting` layihəsində/qovluğunda
   təcrid olunur — domain məntiqinə qarışmır.
3. `IHostedService` əsaslı background job: default gecə saatı konfiqurasiya
   edilə bilər (`appsettings.json`), əlavə olaraq admin panelindən manual
   "yenidən hesabla" düyməsi.
4. Aggregation **idempotent** olmalıdır — gecikmiş sync olunan offline
   satışlar üçün eyni gün üçün təkrar run zədəsiz overwrite etməlidir
   (`ReportAggregationRuns` bunu izləyir).
5. Dashboard tile-ları composable UserControl-lar kimi yazılsın —
   gələcəkdə 4B/4C/4D öz metriklərini əlavə edə bilsin deyə monolit forma
   qurulmasın.

## İCRA ADDIMLARI — HƏR ADDIMDAN SONRA DAYAN

1. `Sales.Channel` migrasiyası (backfill mövcud sətirləri `InStore` ilə) +
   Reporting domeni (summary cədvəllər) + migration.
2. Aggregation background service — nightly job + manual trigger.
3. Dashboard shell (tile-based layout, boş plitələrlə).
4. Tile: Gəlir trendi (line chart, gün/həftə/ay filtri).
5. Tile: Top məhsullar (bar chart, kateqoriya filtri ilə).
6. Tile: Saatlıq yoğunluq (heatmap) — ştat planlaşdırma üçün.
7. Tile: Filiallararası müqayisə (əgər çoxfilialdırsa).
8. Excel/PDF ixracı (mövcud FR-FIN-04 infrastrukturunu genişləndirin).
9. Testlər: məlum `Sales`/`SaleItems` datası üçün aggregation nəticəsinin
   düzgünlüyü, təkrar run-un idempotentliyi.

## QADAĞALAR

- Dashboard sorğularının `Sales`/`SaleItems` cədvəllərinə birbaşa
  müraciəti (bugün plitəsi istisna olmaqla).
- Aggregation job-un sinxron/bloklayıcı işləməsi (checkout thread-ini
  gözlətməməlidir).

## QƏBUL MEYARI

- [ ] Dashboard 100k+ sətirlik `Sales` cədvəlində belə < 1 saniyəyə açılır
- [ ] Gecikmiş offline satış sync olandan sonra növbəti aggregation run onu
      düzgün daxil edir
- [ ] Bütün tile-lar filial dəyişdirildikdə düzgün yenilənir

---
Başla: 1-ci addımdan. Bitirdikdən sonra dayan və xülasə ver.
