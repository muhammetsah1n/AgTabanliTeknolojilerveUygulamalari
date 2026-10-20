# Dijital Akademik Ajanda ve Çalışma Alanı

Görevlerini, etkinliklerini, notlarını ve alışkanlıklarını tek bir yerden yönetmek isteyen herkes için
geliştirilen ağ tabanlı bir web uygulaması. Matt Notebook tarzı kâğıt ajandaların sade görünümünü,
Notion benzeri sayfa/blok yapısıyla birleştirir.

> Ankara Üniversitesi Bilgisayar Mühendisliği – **BLM4581 Ağ Tabanlı Teknolojiler ve Uygulamaları**
> dersi dönem projesi (2026–2027 Güz).

## Ekip

| Ad Soyad | Öğrenci No | GitHub | 
|---|---|---|
| Yağmur Kalyoncu | 23291413 | [@Yagmurkalyoncu](https://github.com/Yagmurkalyoncu) |
| Muhammet Şahin | 23291284 | [@muhammetsah1n](https://github.com/muhammetsah1n) |
| Şahnur Çöğür | 23291625 | [@sahnurcgr](https://github.com/sahnurcgr) |

## Kullanılan Teknolojiler

- **ASP.NET Core MVC** (.NET 10)
- **PostgreSQL** + **Entity Framework Core** (Code-First, migration) 
- **ASP.NET Core Identity** (giriş, kayıt, Admin / Kullanıcı rolleri) 
- **Bootstrap 5.3**, **Bootstrap Icons**, **Inter** yazı tipi (hepsi `wwwroot/lib` altında, internet gerektirmez)

## Projeyi Çalıştırma

### Gereksinimler

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2026 (iş yükü: *ASP.NET and web development*) **veya** VS Code + C# Dev Kit
- PostgreSQL 16+

### Visual Studio ile

1. `Kodlar/DijitalAjanda.sln` dosyasını aç.
2. Üst menüdeki başlatma profilinden **http** veya **https** seç.
3. **F5** (hata ayıklamalı) ya da **Ctrl+F5** ile çalıştır. Tarayıcı otomatik açılır.

### Komut satırı ile

```bash
git clone https://github.com/muhammetsah1n/AgTabanliTeknolojilerveUygulamalari.git
cd AgTabanliTeknolojilerveUygulamalari
dotnet run --project Kodlar/src/DijitalAjanda.Web
```

Uygulama varsayılan olarak `http://localhost:5211` adresinde açılır.

### Veritabanı ve migration 

Veritabanı bağlantı dizesi **depoya konmaz**. Her geliştirici kendi bilgisayarında User Secrets ile tanımlar:

```bash
cd Kodlar/src/DijitalAjanda.Web
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=dijital_ajanda;Username=postgres;Password=SIFRENIZ"
```

Visual Studio'da aynı işlem: projeye sağ tık → **Manage User Secrets**.

Migration adımları:

```bash
dotnet ef database update --project Kodlar/src/DijitalAjanda.Web
```

## Klasör Yapısı

> Aşağıdaki yapı hedeflenen düzendir.

```
ProjeDokümanları/             # Ders proje kuralları ve plan belgesi
Kodlar/
├── DijitalAjanda.sln
└── src/DijitalAjanda.Web/
    ├── Controllers/          # Home, Panel, Gorevler, Notlar, Aliskanliklar, Hesap
    ├── Models/
    │   └── ViewModels/       # View'lara veri taşıyan sınıflar
    ├── Views/
    │   ├── Home/             # Ana sayfa, gizlilik
    │   └── Shared/           # _Layout, _Header, _Footer, Yakinda, Error
    ├── wwwroot/
    │   ├── css/site.css      # Tema renkleri, açık/koyu mod
    │   ├── js/site.js        # Tema değiştirme
    │   └── lib/              # Bootstrap, Bootstrap Icons, Inter, jQuery
    ├── appsettings.json
    └── Program.cs
```

## Haftalık İlerleme

| Hafta | Lider | Durum | Özet |
|---|---|---|---|
| 1 | Yağmur Kalyoncu | 🔄 | Proje iskeleti, ortak sayfa düzeni, tema (açık/koyu), header/footer, ana sayfa |
| 2 | Yağmur Kalyoncu | ⏳ | Veritabanı modelleri, migration, Identity ile giriş/kayıt ve roller |
| 3 | Yağmur Kalyoncu | ⏳ | Panel (dashboard) |
| 4 | Muhammet Şahin | ⏳ | Görev, etkinlik ve kategori CRUD |
| 5 | Muhammet Şahin | ⏳ | Görev Web API + Swagger, Kanban |
| 6 | Muhammet Şahin | ⏳ | Not sayfaları |
| 7 | Şahnur Çöğür | ⏳ | Alışkanlık takibi, harici API |
| 8 | Şahnur Çöğür | ⏳ | SignalR, Admin paneli |
| 9 | Şahnur Çöğür | ⏳ | Testler ve yayına alma |

