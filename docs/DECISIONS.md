# Architecture Decisions

## ADR-001 — Boundary frontend/backend

Frontend WPF berada di `frontend/`, sedangkan Domain, Application, Infrastructure, API, dan tests backend berada di `backend/`. Keduanya berada dalam satu solution agar satu produk tetap mudah dibangun dan dilacak.

## ADR-002 — Domain-first order rules

Aturan finansial inti berada di `Laundry.Domain`, bukan controller atau code-behind. Harga pesanan memakai `PriceSnapshot`; pembayaran berhasil append-only secara konseptual dan pengiriman ulang dicegah melalui idempotency key.

## ADR-003 — Persistence produksi

Production wajib memakai PostgreSQL dan EF Core/Npgsql untuk lokal serta cloud. Repository in-memory yang ada bersifat scaffold development semata dan tidak boleh dipakai untuk rilis.

## ADR-004 — Provider abstraction

WhatsApp dan payment gateway tidak diikat ke vendor sebelum provider resmi dipilih. Adapter mock/sandbox adalah satu-satunya mode yang sah sampai kredensial dan validasi webhook tersedia.
