# .claude/skills/pos-hardware-adapters/SKILL.md

---

name: pos-hardware-adapters
description: Market POS layihəsində barkod skaner, tərəzi, qəbz printeri və Zebra etiket printeri kimi hardware inteqrasiyaları üçün qaydalar. IBarcodeScanner, IScaleReader, IReceiptPrinter, ILabelPrinter və ya IBaggingAreaScale adapteri yazarkən və ya dəyişdirərkən istifadə olunur.

---

# Hardware inteqrasiya qaydaları

Hər fiziki cihaz `Application` qatında təyin olunan interfeys arxasında
gizlənir; real implementasiya `Infrastructure`-dadır.

## Məcburi qaydalar

- **Mock implementasiyanı heç vaxt silmə.** `appsettings.json` →
  `Hardware:UseMock` bayrağı ilə mock/real arasında keçid edilir. Mock
  CI/test mühiti üçün daimi lazımdır.
- Yeni cihaz/vendor = **mövcud interfeysi implement edən yeni sinif**,
  yeni interfeys yox (məs. ikinci tərəzi modeli üçün ayrıca
  `IScaleReader` yaratma, `SerialScaleReader`-in yeni implementasiyasını
  yaz).

## Protokol qeydləri

| Cihaz                               | Protokol                                                                                                                                             |
| ----------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------- |
| Tərəzi                              | RS-232 seriya (əsas), USB (alternativ). Çəki qəbul edilməzdən əvvəl "stabil" bayrağını yoxla — bəzi tərəzilər sabitləşənə qədər "unstable" göndərir. |
| Qəbz printeri                       | ESC/POS raw komandalar (kəsmə, bold, kassa siyirtməsi pulse siqnalı)                                                                                 |
| Zebra etiket printeri               | ZPL raw — Win32 print queue və ya TCP port 9100                                                                                                      |
| Barkod skaner                       | HID keyboard-wedge. Skaner ilə insan yazısını simvollar arası vaxtla ayırd et (< 50ms = skaner)                                                      |
| Bagging-area tərəzi (self-checkout) | `IScaleReader`-dən **ayrı** interfeys (`IBaggingAreaScale`) — funksiyası fərqlidir, qarışdırma                                                       |

## Test mühiti

Real cihaz mövcud olmayanda konfiqurasiya edilə bilən gecikmə/uğursuzluq
nisbəti olan mock yaz ki, xəta ssenariləri də test edilə bilsin —
yalnız "həmişə uğurlu" mock kifayət etmir.
