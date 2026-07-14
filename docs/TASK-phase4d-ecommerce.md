# İMPLEMENTASİYA PROMPTU — Market POS Sistemi (Faza 4D — E-ticarət)

# TASK-phase4d-ecommerce.md. Faza 1-3-ün tam bitmiş olduğunu fərz edir

## KONTEKST

**Fərziyyə:** Konkret e-ticarət platforması seçilməyib — sıfırdan REST API
qurulur ki, istənilən frontend (öz tikdiyiniz, məsələn CV Builder-dəki
Cloudflare/TypeScript stack-inə bənzər, və ya hazır platforma) buna qoşula
bilsin. Onlayn sifariş `Sales.Channel = Online` ilə mövcud modelə
inteqrasiya olunur — ayrıca paralel sistem qurulmur.

## ƏHATƏ

Kataloq-oxuma API-si, sifariş yaratma, "available-to-promise" stok
hesablaması, sifariş tamamlanma (fulfillment) izlənməsi.

## YENİ ASILILIQ

| Paket | Məqsəd |
|---|---|
| `ASP.NET Core Web API` (.NET 8) | REST səthi |
| `Microsoft.AspNetCore.RateLimiting` | API sürət limiti |

## VERİLƏNLƏR BAZASI ƏLAVƏLƏRİ

-- Sales.Channel = Online (4A-da tərif olunub, əgər icra olunmayıbsa burda əlavə edin)
OnlineOrders(Id, SaleId, CustomerId, DeliveryAddress?, FulfillmentType[Pickup|Delivery],
RequestedTime?, Status[Placed|Confirmed|Preparing|ReadyForPickup|OutForDelivery|Completed|Cancelled])
InventoryReservations(Id, BranchId, ProductId, Quantity, OnlineOrderId?, ExpiresAt, Status[Active|Released|Consumed])
ApiClients(Id, ClientName, ApiKeyHash, IsActive, RateLimitPerMinute)

## MEMARLIQ QAYDALARI

1. `MarketPOS.Api` — **nazik qat**: controller-lər birbaşa mövcud
   `Application` servislərini çağırır (`SaleService`, `InventoryService`),
   heç bir yeni biznes məntiqi API layihəsində yazılmır.
2. **Available-to-promise stok** = `BranchInventory.QuantityOnHand` −
   aktiv `InventoryReservations` cəmi. Kataloq endpoint-i həmişə bu
   hesablanmış rəqəmi qaytarır, xam `QuantityOnHand`-ı yox.
3. **Rezervasiya mexanizmi** — müştəri səbətə əlavə edəndə vaxt-məhdud
   (`ExpiresAt`, konfiqurasiya edilə bilən, məs. 15 dəqiqə) rezervasiya
   yaranır. Background job müntəzəm `Active` və vaxtı keçmiş rezervasiyaları
   `Released`-ə keçirir — bu, overselling-in qarşısını alır.
4. `OnlineOrders.Status` **ayrıca vəziyyət maşınıdır**, `Sale.Status`-dan
   (ödəniş/tranzaksiya tamamlanması) müstəqildir — fiziki
   yığma/hazırlanma prosesini izləyir, Faza 3-ün `StockTransfer`
   vəziyyət-maşını naxışı ilə eynidir (ardıcıl keçidlər, hər keçid audit
   olunur).
5. Autentifikasiya — sadə API key (bir etibarlı frontend müştərisi üçün
   kifayətdir, açıq bazar deyil), hash-lənmiş saxlanır, sürət limiti
   tətbiq olunur.
6. Status yeniləmələri üçün **polling**, webhook yox — MVP sadəliyi üçün
   şüurlu seçim (webhook çatdırılma zəmanəti ayrıca mürəkkəblikdir, sonra
   əlavə edilə bilər).

## İCRA ADDIMLARI — HƏR ADDIMDAN SONRA DAYAN

1. `MarketPOS.Api` layihə scaffold, API key auth middleware, sürət limiti.
2. `Sales.Channel = Online` (əgər 4A icra olunmayıbsa) + `OnlineOrders`,
   `InventoryReservations`, `ApiClients` migration.
3. Kataloq-oxuma endpoint-i (`GET /products`) — available-to-promise
   hesablaması ilə.
4. Rezervasiya endpoint-i (`POST /cart/reserve`) + auto-release
   background job.
5. Sifariş yaratma endpoint-i (`POST /orders`) — `Sale` (Channel=Online)
   + `OnlineOrder` yaradır, rezervasiyanı `Consumed`-ə keçirir.
6. Sifariş-status endpoint-i (`GET /orders/{id}/status`, polling üçün).
7. Daxili fulfillment ekranı (mövcud admin tətbiqində) — işçi
   Preparing → ReadyForPickup/OutForDelivery → Completed keçidlərini
   edir.
8. Testlər: paralel rezervasiyalarda overselling olmaması, vaxtı keçmiş
   rezervasiyanın avtomatik azad olunması, etibarsız API key-in rədd
   edilməsi.

## QADAĞALAR

+ Kataloq endpoint-ində xam `QuantityOnHand`-ı birbaşa qaytarmayın
  (həmişə rezervasiya çıxılmış rəqəm).
+ Rezervasiyasız birbaşa sifariş yaratma yolu qoymayın (race condition
  riski).
+ API-də yeni/paralel `SaleService` məntiqi yazmayın.

## QƏBUL MEYARI

+ [ ] 10 paralel sifariş cəhdi eyni son vahidə görə düzgün overselling-i
      önləyir (yalnız 1-i uğurlu olur, qalanları stok-yoxdur cavabı alır)
+ [ ] Vaxtı keçmiş rezervasiya 1 dəqiqə daxilində avtomatik azad olunur
+ [ ] Etibarsız API key `401` qaytarır, sürət limiti aşımı `429` qaytarır
+ [ ] Online sifariş daxili fulfillment ekranında düzgün statuslarla
      görünür

---
Başla: 1-ci addımdan. Bitirdikdən sonra dayan və xülasə ver.
