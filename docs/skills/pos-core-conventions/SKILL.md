# .claude/skills/pos-core-conventions/SKILL.md

---

name: pos-core-conventions
description: Market POS layihəsinin əsas memarlıq və kodlaşdırma qaydaları (C#/.NET). Domain, Application, Infrastructure və ya UI qatlarında istənilən sinif, servis, forma və ya migration yazılarkən və ya redaktə edilərkən istifadə olunur.

---

# Əsas memarlıq qaydaları

Bu layihə qatlı arxitektura üzərində qurulub: `Domain` → `Application` →
`Infrastructure` → `UI`. Heç bir qat özündən yuxarı qatı tanımır, heç bir
qat aralıq qatı ötürüb daha aşağı qata müraciət etmir.

## Məcburi qaydalar

- **UI heç vaxt `DbContext`-i birbaşa çağırmır.** Bütün DB girişi
  `IRepository<T>` + `IUnitOfWork` arxasındadır.
- **Snapshot pattern:** istənilən "tarixi qeyd" xarakterli entity (satış
  sətri, sifariş sətri, fiskal qeyd) əlaqəli olduğu məhsul/qiymət
  məlumatını öz üzərində dondurub saxlamalıdır (`*Snapshot` sahələri).
  Əsas cədvəldəki dəyər sonradan dəyişsə belə, tarixi qeyd dəyişməməlidir.
- **Async/await məcburidir** bütün IO əməliyyatları (DB, fayl, şəbəkə,
  hardware) üçün — UI thread heç vaxt bloklanmır.
- **Nullable reference types aktivdir** (`<Nullable>enable</Nullable>`),
  `!` və ya `#pragma` ilə susdurma yalnız əsaslandırılmış şərhlə.
- **Dependency Injection** — `new ServiceClass()` yox, konstruktor
  injection. Yeni servis yazanda `Program.cs`-də qeydiyyatı unutma.

## Adlandırma və dil

- Sinif/metod/dəyişən adları: PascalCase/camelCase, **ingiliscə**
  (`SaleService`, `CalculateTotal`, `_unitOfWork`)
- UI-da görünən bütün mətn (label, button, mesaj, xəta): **Azərbaycan
  dili**
- Kod şərhləri: ingiliscə, yalnız "niyə" izah edən yerlərdə
- Hər public metod üçün XML doc comment

## Qeyri-müəyyənlik halında

Scope, prioritet və ya qayda ilə bağlı şübhə yarandıqda, əvvəlcə
`docs/PRD.md` və uyğun `docs/TASK-phase*.md` faylına bax. Orada
cavab yoxdursa, fərziyyəni açıq şəkildə bildirib davam et — susma və
təxmin etmə.
