# Implementation Plan

Status: fase 0 dan fondasi awal dibuat pada 15 Oktober 2026. Build dan unit test fondasi sudah lulus pada SDK .NET 10.0.402 lokal.

| Fase | Lingkup | Status | Acceptance gate |
| --- | --- | --- | --- |
| 0 | inspeksi, dokumen, keputusan | selesai | docs tersedia; SDK tercatat sebagai blocker build |
| 1 | solution frontend/backend, WPF, API, DI | selesai awal | build solution dan unit tests lulus |
| 2 | PostgreSQL/EF migrations dan domain lengkap | belum | local/cloud migration, constraints, tests |
| 3 | auth, OTP, role/ownership | belum | IDOR, lockout, token tests |
| 4–5 | admin offline, order, pembayaran, struk | belum | flow walk-in offline tervalidasi |
| 6 | outbox/inbox, retry, checkpoint | belum | duplicate/crash/reconnect tests |
| 7 | dashboard pelanggan dan order online | belum | ownership dan cloud update tests |
| 8–9 | provider, audit, laporan, backup/update | belum | provider sandbox dan operational tests |

## Dependency dan risiko

1. SDK .NET belum terpasang, sehingga source belum dapat dikompilasi atau dites.
2. PostgreSQL lokal/cloud, payment gateway, dan WhatsApp provider belum dikonfigurasi.
3. Kebijakan harga, DP, slot, dan bisnis lain pada `OPEN_QUESTIONS.md` perlu diputuskan sebelum modul terkait dapat selesai.

## Batas implementasi awal

Fondasi menyediakan aturan harga minimum, pembayaran idempoten, larangan serah sebelum lunas, dan UI role-aware. Repository in-memory hanya development scaffold dan wajib diganti EF Core/Npgsql; tidak memenuhi persyaratan persistence/offline PRD.
