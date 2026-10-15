# Architecture

```text
frontend/Laundry.Desktop (WPF/MVVM)
          | HTTPS API
backend/Laundry.Api -> Application -> Domain
                         |
                    Infrastructure
                         |
            PostgreSQL lokal / PostgreSQL cloud
                         |
       durable outbox, inbox, sync checkpoints
```

Desktop adalah satu aplikasi dengan dashboard berdasarkan role. Pemisahan folder tidak berarti dua aplikasi pengguna. Backend menjadi sumber otorisasi dan aturan bisnis. Sinkronisasi PostgreSQL lokal/cloud akan dibuat dengan event outbox/inbox idempoten; bukan replikasi multi-master mentah.
