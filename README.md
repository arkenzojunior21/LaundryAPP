# LaundryAPP

Aplikasi desktop laundry Windows dengan satu executable WPF untuk Admin dan Pelanggan, serta ASP.NET Core Web API. Struktur proyek dipisah agar batas frontend dan backend tegas.

## Struktur

- `frontend/Laundry.Desktop` — WPF client (MVVM, UI Indonesia, role-aware dashboard).
- `backend/Laundry.Domain` — aturan bisnis dan model inti tanpa dependensi framework.
- `backend/Laundry.Application` — use case dan kontrak aplikasi.
- `backend/Laundry.Api` — HTTP API, otorisasi, serta komposisi dependency.
- `backend/Laundry.Infrastructure` — persistence PostgreSQL, sync, integrasi provider (tahap berikutnya).
- `backend/tests` — unit, integration, dan E2E tests.
- `docs` — rencana, keputusan, pertanyaan terbuka, dan arsitektur.

## Prasyarat

- Windows 10/11
- .NET SDK 10.0 atau LTS yang disepakati (saat ini SDK belum terpasang pada mesin pengembangan).
- PostgreSQL untuk mode lokal admin dan lingkungan cloud/server.

## Setelah SDK terpasang

```powershell
dotnet restore Laundry.sln
dotnet build Laundry.sln
dotnet test Laundry.sln
dotnet run --project backend/Laundry.Api
dotnet run --project frontend/Laundry.Desktop
```

`Laundry.Api` saat ini adalah fondasi development in-memory. Adapter PostgreSQL/Npgsql, autentikasi produksi, dan sinkronisasi durable dicatat dalam rencana implementasi dan tidak diklaim aktif.

## Keamanan dan provider

Jangan menyimpan secret di repository. Salin konfigurasi dari `.env.example` dan isi melalui secret store/variabel lingkungan. Provider WhatsApp dan payment gateway belum dipilih sehingga hanya boleh memakai adapter mock hingga kredensial sandbox resmi tersedia.
