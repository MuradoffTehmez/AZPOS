# .claude/skills/pos-fiscal-compliance/SKILL.md

---

name: pos-fiscal-compliance
description: Market POS layihəsində Azərbaycanın NKA/e-Kassa fiskal uyğunluq qaydaları. FiscalTransaction, IFiscalDevice, e-qaimə generasiyası, yaş-təsdiqi axını və ya checkout-un fiskal/vergi hissəsinə toxunan istənilən kod üzərində işləyərkən istifadə olunur.

---

# Fiskal uyğunluq qaydaları

Bu, layihənin ən hüquqi-həssas hissəsidir (bax: PRD.md bölmə 14). Səhv
burada həm qanuni, həm maliyyə riski yaradır.

## Məcburi qaydalar

- `IFiscalDevice` kontraktı **vendor-aqnostikdir**. Yazılı vendor
  sənədləşməsi (SDK, protokol) əlinizdə olmadan konkret NKA vendoru üçün
  təxmini implementasiya yazma — bunun əvəzinə `SimulatedFiscalDevice`
  istifadə et. Sənədsiz "təxmini" inteqrasiya real cihazla işləməyən
  yalançı təhlükəsizlik yaradır.
- `FiscalTransaction` yazısı satış tamamlananda **dərhal, sinxron**
  yaradılır (`Pending` statusu ilə), amma göndərilməsi **asinxrondur** —
  checkout heç vaxt fiskal ötürməni gözləmir.
- Fiskal növbə DB-də persistent olmalıdır (bax: pos-offline-sync skill-i
  — eyni prinsip). Retry exponential backoff ilə, max cəhddən sonra admin
  bildirişi.
- **Yaş-təsdiqi tələb edən məhsul üçün avtomatik təsdiq yolu yoxdur,
  istisnasız.** Nəzarətçi/meneceri təsdiqi olmadan sessiya davam edə
  bilməz.
- İstənilən manual override (yaş-təsdiqi, çəki-uyğunsuzluğu) **məcburi**
  audit qeydi yaradır — `EmployeeId` heç vaxt boş qala bilməz.

## E-qaimə vs NKA çeki

Pərakəndə satışda NKA fiskal çeki kifayətdir. Elektron qaimə-faktura
yalnız topdansatış/B2B əməliyyatlar üçündür — bu ikisini kodda
qarışdırma, `Sale` obyektində hansı tipin tələb olunduğunu aydın
müəyyənləşdir.
