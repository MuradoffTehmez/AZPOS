# .claude/skills/pos-offline-sync/SKILL.md

---

name: pos-offline-sync
description: Market POS layihəsində offline-first data yazma və sinxronizasiya qaydaları. Sale yaradılması, BranchInventory yenilənməsi, sync background service-ləri və ya şəbəkə kəsintisindən sağ çıxmalı istənilən entity üzərində işləyərkən istifadə olunur.

---

# Offline-first qaydaları

Hər terminal əvvəlcə lokala (SQLite) yazır, sonra fon rejimində mərkəzi
SQL Server-ə göndərir. **Şəbəkə yoxluğu heç vaxt satışı bloklamır.**

## Məcburi qaydalar

- Hər sinxronlaşacaq yazı `ClientTransactionGuid` daşıyır (client
  tərəfində generasiya olunan, server-side dedup açarı). Server eyni
  GUID-i ikinci dəfə görəndə sadəcə "artıq mövcuddur" qaytarır, təkrar
  yazmır.
- Sync növbəsi **DB-də persistent olmalıdır**, yalnız yaddaşda (in-memory)
  saxlanmamalıdır — tətbiq restart olanda `Pending` yazılar avtomatik
  bərpa olunmalıdır.
- Sync dispatcher background thread-də işləyir, checkout axınını heç vaxt
  gözlətmir.

## Konflikt həlli — entity tipinə görə

| Entity                                  | Strategiya                                                                                                  |
| --------------------------------------- | ----------------------------------------------------------------------------------------------------------- |
| `Sale` / `SaleItem`                     | Append-only, immutable. Server heç vaxt üzərinə yazmır və ya silmir. Konflikt praktiki olaraq mümkün deyil. |
| `BranchInventory.QuantityOnHand`        | Server-authoritative, last-write-wins, hər dəyişiklik `SyncConflictLogs`-a yazılır.                         |
| Kataloq/qiymət (`Product`, `Promotion`) | Yalnız HQ → filial bir istiqamətli axın. Filial terminalı bu datanı heç vaxt "yaza" bilməz.                 |

## BranchId scoping

Bütün sorğular `IBranchContext` vasitəsilə cari filiala scope olunur.
Yeni repository metodu yazanda özündən soruş: "bu sorğu təsadüfən bütün
filialları qaytarmırmı?" — susmaya (default) görə həmişə filial-scoped
olsun.
