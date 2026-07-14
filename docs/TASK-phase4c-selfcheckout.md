# İMPLEMENTASİYA PROMPTU — Market POS Sistemi (Faza 4C — Self-checkout)

# TASK-phase4c-selfcheckout.md. Faza 1-3-ün tam bitmiş olduğunu fərz edir

## KONTEKST

**Fərziyyə:** Touchscreen kiosk + bagging-area tərəzi (çəki-uyğunsuzluğu
aşkarlayan loss-prevention), 1 nəzarətçi ~4-6 kiosk-a nəzarət edir. Kamera-
əsaslı AI loss-prevention bu mərhələdə YOXDUR (ayrı, daha bahalı təşəbbüs).

## ƏHATƏ

Müştərinin özünün skan/ödəniş etdiyi kiosk axını + nəzarətçi konsolu.

## VERİLƏNLƏR BAZASI ƏLAVƏLƏRİ

Kiosks(Id, BranchId, Name, IPAddress, Status[Online|Offline|Maintenance], LastHeartbeatAt)
SelfCheckoutSessions(Id, KioskId, BranchId, StartedAt, EndedAt?,
Status[InProgress|Completed|FlaggedForReview|Abandoned], SaleId?)
SelfCheckoutFlags(Id, SessionId,
FlagType[WeightMismatch|VoidAfterScan|UnexpectedItem|AgeVerificationRequired],
Details, RaisedAt, ResolvedByEmployeeId?, ResolvedAt?, Resolution)
-- Sales.Channel = SelfCheckout (4A-da tərif olunub)

## MEMARLIQ QAYDALARI

1. Kiosk **eyni** `SaleService`/checkout məntiqini istifadə edir (Faza
   1-dəki "composable POS" prinsipi) — yeni satış məntiqi yazılmır, yalnız
   yeni presentation səthi (`MarketPOS.Kiosk`) əlavə olunur.
2. `IBaggingAreaScale` — **yeni, ayrıca** interfeys (Faza 2-nin çəkili-
   məhsul `IScaleReader`-i ilə QARIŞDIRMAYIN, fərqli funksiyadır). Hər skan
   sonrası gözlənilən kumulyativ çəki ilə bagging-area faktiki çəkisi
   müqayisə olunur; tolerans həddi konfiqurasiya edilə bilər
   (`appsettings.json` → default dəyər layihə başlanğıcında sınaqla
   tənzimlənməlidir, sabit "düzgün" ədəd yoxdur).
3. Uyğunsuzluq tolerans həddini aşarsa → sessiya `FlaggedForReview`
   statusuna keçir, kiosk ekranı dayanır, `SelfCheckoutFlags` yazılır,
   nəzarətçi konsoluna real-time bildiriş gedir.
4. **Yaş-təsdiqi tələb edən məhsullar (alkoqol/tütün) həmişə dayanır** —
   nəzarətçi təsdiqi olmadan avtomatik davam etmə YOXDUR, istisnasız qayda.
5. Nəzarətçinin "override" əməliyyatı **məcburi audit qeydi** yaradır —
   bu, ən çox sui-istifadə olunan nöqtədir, ona görə `ResolvedByEmployeeId`
   heç vaxt boş qala bilməz.
6. Kiosk heartbeat — hər 30 saniyədə status ötürür; qaçırılan heartbeat
   nəzarətçi konsolunda "oflayn/donmuş" kimi işarələnir.

## İCRA ADDIMLARI — HƏR ADDIMDAN SONRA DAYAN

1. Kiosk domeni (`Kiosk`, `SelfCheckoutSession`, `SelfCheckoutFlags`) +
   migration.
2. `MarketPOS.Kiosk` (WPF, tam-ekran, müştəri-üzlü) layihə scaffold.
3. Kiosk checkout axını — mövcud `SaleService`-ə bağlı, sadə/aydın
   Azərbaycan dilli mesajlarla ("Zəhmət olmasa məhsulu bagging area-ya
   qoyun" və s.).
4. `IBaggingAreaScale` interfeysi + mock implementasiya (konfiqurasiya
   edilə bilən uğursuzluq nisbəti ilə test üçün).
5. Çəki-uyğunsuzluq yoxlama məntiqi (Application qatında, UI-dan asılı
   olmayan, ayrıca test edilə bilən servis).
6. Yaş-təsdiqi flag-ı + məcburi dayanma axını.
7. Nəzarətçi konsolu (`MarketPOS.AttendantConsole` və ya mövcud admin
   tətbiqinə modul) — canlı flag növbəsi, override/təsdiq düymələri.
8. Kiosk heartbeat background service + konsoldakı status indikatoru.
9. Testlər: tolerans daxilində/xaricində çəki ssenariləri, yaş-təsdiqi
   olmadan sessiyanın tamamlana bilməməsi, override-ın audit-logda
   göründüyünün yoxlanması.

## QADAĞALAR

- Kamera/AI-əsaslı aşkarlama əlavə etməyin (fərziyyə xaricindədir).
- Yaş-təsdiqi tələb edən məhsul üçün "avtomatik təsdiq" yolu yazmayın —
  bu, qanuni tələbi pozar.
- Override əməliyyatını audit-logsuz icazə verməyin.

## QƏBUL MEYARI

- [ ] Tolerans xaricində çəki fərqi 100% hallarda sessiyanı dayandırır
- [ ] Yaş-təsdiqi tələb edən məhsul nəzarətçi təsdiqi olmadan çekə düşmür
- [ ] Nəzarətçi konsolu 6 kiosk-un statusunu eyni ekranda göstərir
- [ ] Hər override əməliyyatı audit logunda işçi ID-si ilə görünür

---
Başla: 1-ci addımdan. Bitirdikdən sonra dayan və xülasə ver.
