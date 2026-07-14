# İMPLEMENTASİYA PROMPTU — Market POS Sistemi (Faza 4B — Mobil POS)

# TASK-phase4b-mobile.md. Faza 1-3-ün tam bitmiş olduğunu fərz edir

## KONTEKST

**Fərziyyə:** Mobil POS = Windows planşetdə işləyən yüngül WPF tətbiqi,
Android/iOS DEYİL. Bu qərar, mövcud .NET/Windows hardware stack-i (Zebra,
seriya tərəzi) ilə uyğunluğu qoruyur və MAUI/cross-platform yenidən
yazılışından yayınır. Əgər gələcəkdə Android/iOS lazım olsa, bu ayrıca,
daha böyük bir təşəbbüs kimi planlaşdırılmalıdır — indi qərar verməyin.

## ƏHATƏ

Toxunma-optimallaşdırılmış, sadələşdirilmiş checkout + mağaza daxilində
gəzərək stok baxışı. Admin/konfiqurasiya ekranları YOXDUR.

## VERİLƏNLƏR BAZASI ƏLAVƏSİ
-- Yalnız 4A icra olunmayıbsa əlavə edin (əks halda artıq mövcuddur):
Sales: + Channel enum [...MobileTablet...]

## MEMARLIQ QAYDALARI

1. **Yeni presentation layihəsi, yeni məntiq YOX** — `MarketPOS.Mobile`
   (WPF) yalnız mövcud `MarketPOS.Application`/`Infrastructure`-a istinad
   edir. `SaleService`, `InventoryService` təkrar yazılmır.
2. Faza 3-ün offline-sync boru xətti **dəyişmədən** paylaşılır — planşet
   də adi terminal kimi `ClientTransactionGuid` ilə lokal SQLite-a yazır.
3. UI sadələşdirilir: az menyu səviyyəsi, böyük toxunma hədəfləri (min
   44x44px), yalnız checkout + stok-baxış ekranları.
4. Bluetooth hardware adapterləri mövcud interfeyslərin YENİ
   implementasiyalarıdır (`BluetoothBarcodeScanner : IBarcodeScanner`,
   `PortableReceiptPrinter : IReceiptPrinter`) — interfeys özü dəyişmir.
5. Şəbəkə davamlılığı — planşet mağaza daxilində gəzdiyi üçün WiFi
   kəsintisi tez-tez baş verə bilər; offline-first artıq buna hazırdır,
   amma bu fazada xüsusi test fokusu tələb olunur (aşağı bax).

## İCRA ADDIMLARI — HƏR ADDIMDAN SONRA DAYAN

1. `MarketPOS.Mobile` (WPF) layihə scaffold, mövcud layihələrə reference.
2. Sadələşdirilmiş checkout ekranı (toxunma-optimallaşdırılmış).
3. `BluetoothBarcodeScanner` adapteri (mock ilə başlayın, real Bluetooth
   SDK sənədləşməsi gələndə əvəzlənə bilər).
4. `PortableReceiptPrinter` adapteri (eyni yanaşma — mock əvvəl).
5. Gəzərək stok-baxış ekranı (yalnız oxuma, redaktə yoxdur).
6. Şəbəkə-kəsintisi simulyasiya testi: tranzaksiya ortasında WiFi kəsilir
   → satış lokal tamamlanır, sync növbəsinə düşür, WiFi qayıdanda
   itkisiz göndərilir.
7. Testlər: adapter unit testləri, sadələşdirilmiş checkout axını üçün UI
   smoke testləri.

## QADAĞALAR

- `Application`/`Infrastructure` qatlarında mobilə xüsusi filial (branch)
  yaratmayın — eyni servislər istifadə olunur.
- Admin/konfiqurasiya funksiyalarını mobilə köçürməyin.
- Android/iOS üçün heç bir kod/paket əlavə etməyin (fərziyyə xaricindədir).

## QƏBUL MEYARI

- [ ] Planşetdən edilən satış masaüstü terminaldan fərqlənmədən mərkəzi
      DB-də görünür (`Channel = MobileTablet` ilə)
- [ ] WiFi kəsintisi simulyasiyasında sıfır tranzaksiya itkisi
- [ ] Checkout ekranı 10 addımdan az toxunma ilə tamamlanır

---
Başla: 1-ci addımdan. Bitirdikdən sonra dayan və xülasə ver.
