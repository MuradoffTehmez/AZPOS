# .claude/skills/pos-phase-task-writer/SKILL.md

---

name: pos-phase-task-writer
description: Market POS layihəsi üçün yeni Faza/TASK-phaseN.md implementasiya promptu yazır, PRD.md və mövcud TASK fayllarının formatına uyğun. İstifadəçi yeni faza, modul və ya funksiya üçün implementasiya promptu, TASK faylı və ya icra planı istəməsi zamanı istifadə olunur.

---

# Yeni Faza TASK.md yazma qaydası

Bu skill "meta" xarakterlidir — məqsəd yeni TASK-phaseN.md-nin əvvəlki
fazalarla eyni ciddiyyət və formatda olmasını təmin etməkdir.

## Prosedur

1. Yeni yazıya başlamazdan əvvəl `docs/PRD.md` və mövcud bütün
   `docs/TASK-phase*.md` fayllarını oxu — yeni faza mövcud scope,
   entity adları və ya memarlıq qərarları ilə ziddiyyət təşkil etməsin.
2. Aşağıdakı struktura ciddi əməl et (əvvəlki fazalarla eyni):
   Rol/Kontekst → Əhatə → Yeni asılılıqlar → DB əlavələri → Memarlıq
   qaydaları → İcra addımları (nömrələnmiş, hər addımdan sonra **dayan
   və xülasə ver**) → Qadağalar → Qəbul meyarı.
3. Memarlıq qaydalarını təkrar yazmaq əvəzinə, `pos-core-conventions`,
   `pos-offline-sync`, `pos-hardware-adapters` və `pos-fiscal-compliance`
   skill-lərinə istinad et — yalnız həmin fazaya **xüsusi** olan yeni
   qaydaları yaz.
4. Yeni fazanın hər hansı açıq (hələ qərar verilməmiş) məhsul sualı
   varsa, onu susaraq fərz etmə — "Qərar tələb olunur" bölməsində açıq
   sual kimi qeyd et.

## Dil

Bütün başlıq/təsvir/qayda mətni Azərbaycan dilində, entity/sinif/metod
adları və kod nümunələri ingiliscə (layihənin ümumi konvensiyası).
