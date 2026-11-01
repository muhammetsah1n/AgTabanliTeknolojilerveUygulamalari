<div align="center">

# Dijital Akademik Ajanda ve Çalışma Alanı

Görevlerini, etkinliklerini, notlarını ve alışkanlıklarını tek bir yerden yönetebileceğin,
bullet journal yöntemiyle tasarlanmış ağ tabanlı bir ajanda uygulaması.

![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16+-4169E1?style=flat-square&logo=postgresql&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=flat-square&logo=bootstrap&logoColor=white)

<sub>Ankara Üniversitesi · BLM4581 Ağ Tabanlı Teknolojiler ve Uygulamaları · 2026–2027 Güz</sub>

</div>

---

## Ekip — Dreamfire

| Ad Soyad | GitHub |
|---|---|---|
| Yağmur Kalyoncu |  [@Yagmurkalyoncu](https://github.com/Yagmurkalyoncu) |
| Muhammet Şahin  | [@muhammetsah1n](https://github.com/muhammetsah1n) | 
| Şahnur Çöğür    | [@sahnurcgr](https://github.com/sahnurcgr) | 

## Teknolojiler

- **ASP.NET Core MVC** (.NET 10)
- **PostgreSQL** + **Entity Framework Core** (Npgsql, Code-First, migration)
- **ASP.NET Core Identity** — giriş, kayıt, şifre sıfırlama, `Admin` / `Kullanici` rolleri
- **Bootstrap 5.3**, Bootstrap Icons, Inter ve Caveat yazı tipleri (`wwwroot/lib` altında, internet gerektirmez)

## Kurulum

### Gereksinimler

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 16+](https://www.postgresql.org/download/)
- Visual Studio 2026 veya VS Code + C# Dev Kit
- EF Core aracı: `dotnet tool install --global dotnet-ef`

### 1. Depoyu klonla

```bash
git clone https://github.com/muhammetsah1n/AgTabanliTeknolojilerveUygulamalari.git
cd AgTabanliTeknolojilerveUygulamalari
```

### 2. Gizli ayarları tanımla

Bağlantı dizesi ve yönetici bilgileri depoda tutulmaz; **User Secrets** ile tanımlanır
(Visual Studio'da: projeye sağ tık → *Manage User Secrets*).

```bash
cd Kodlar/src/DijitalAjanda.Web
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=dijital_ajanda;Username=postgres;Password=SIFRENIZ"
dotnet user-secrets set "Admin:Email" "admin@dreamfire.com"
dotnet user-secrets set "Admin:Sifre" "YONETICI_SIFRESI"
```

<details>
<summary>İsteğe bağlı: SMTP ayarları</summary>

<br>

Şifre sıfırlama kodları için kullanılır. Ayarlanmazsa 6 haneli kod uygulama konsoluna yazılır.

```bash
dotnet user-secrets set "Smtp:Host" "smtp.gmail.com"
dotnet user-secrets set "Smtp:Kullanici" "hesabiniz@gmail.com"
dotnet user-secrets set "Smtp:Sifre" "GMAIL_UYGULAMA_SIFRESI"
dotnet user-secrets set "Smtp:Gonderen" "Dijital Ajanda <hesabiniz@gmail.com>"
```

</details>

### 3. Çalıştır

```bash
dotnet run --project Kodlar/src/DijitalAjanda.Web
```

Uygulama <http://localhost:5211> adresinde açılır. Visual Studio'da `Kodlar/DijitalAjanda.sln` açılıp **F5** ile de çalıştırılabilir.

### Veritabanı ve migration

Uygulama açılışta bekleyen migration'ları uygular, `Admin` ve `Kullanici` rollerini ve yönetici hesabını oluşturur.
Elle yapmak için:

```bash
# Veritabanını güncelle
dotnet ef database update --project Kodlar/src/DijitalAjanda.Web

# Yeni migration ekle
dotnet ef migrations add DegisiklikAdi --project Kodlar/src/DijitalAjanda.Web -o Data/Migrations
```

> `relation ... already exists` hatası alırsan eski veritabanını sil ve uygulamayı yeniden başlat:
> `dotnet ef database drop --project Kodlar/src/DijitalAjanda.Web`

## Roller

| Rol | Erişim |
|---|---|
| Ziyaretçi | Ana sayfa, kayıt, giriş, şifremi unuttum, gizlilik |
| Kullanici | Bugün, Görevler, Alışkanlıklar ve diğer ajanda sayfaları |
| Admin | Yönetim paneli (`/Yonetim`) — giriş ekranındaki **Yönetici girişi** sekmesinden |

Yetkisiz erişimler **Erişim engellendi**, olmayan sayfalar ve hatalar **Hata** sayfasına yönlendirilir.

## Veritabanı Tabloları

| Tablo | İlişki |
|---|---|
| AspNetUsers (Kullanici) | 1–N: Kategoriler, Gorevler, Etkinlikler, Notlar, Stickerlar, Aliskanliklar, SuKayitlari, Kitaplar |
| Kategoriler | 1–N: Gorevler, Etkinlikler |
| Gorevler | 1–N: AltGorevler |
| Aliskanliklar | 1–N: AliskanlikKayitlari (aynı gün tek kayıt) |
| SuKayitlari | Kullanıcı başına günde tek kayıt |
| SikcaSorulanSorular, IletisimMesajlari | Yönetici panelinden yönetilir |

## Klasör Yapısı

```text
ProjeDokümanları/            Proje kuralları ve plan belgesi
Kodlar/
└── src/DijitalAjanda.Web/
    ├── Controllers/         Home, Hesap, Bugun, Gorevler, Aliskanliklar, Yonetim
    ├── Data/                DbContext, roller, seed işlemi, migration'lar
    ├── Models/              Entities (tablolar) ve ViewModels (form modelleri)
    ├── Services/            E-posta gönderimi, Türkçe Identity mesajları
    ├── Views/               Home, Hesap, Shared (layout, header, footer, hata)
    ├── wwwroot/             CSS, JS ve kütüphaneler
    └── Program.cs
```

## Haftalık İlerleme

| Hafta | Lider | Kapsam | Durum |
|:---:|---|---|:---:|
| 1 | Yağmur Kalyoncu | Proje iskeleti, ortak sayfa düzeni, açık/koyu tema, ana sayfa | ✅ |
| 2 | Yağmur Kalyoncu | Entity'ler, PostgreSQL + migration, kayıt/giriş, şifre sıfırlama, roller, hata sayfaları | ✅ |
| 3 | Yağmur Kalyoncu | Bugün sayfası (gün/hafta/ay), tanıtım sayfası, responsive tasarım | ⏳ |
| 4 | Muhammet Şahin | Kategori, görev ve etkinlik CRUD sayfaları | ⏳ |
| 5 | Muhammet Şahin | Görev Web API + Swagger, Kanban, alt görevler | ⏳ |
| 6 | Muhammet Şahin | Alışkanlık ve su takibi | ⏳ |
| 7 | Şahnur Çöğür | Kitaplık, sticker yükleme, Nager.Date harici API | ⏳ |
| 8 | Şahnur Çöğür | SignalR hatırlatıcılar, Admin paneli, SSS / iletişim | ⏳ |
| 9 | Şahnur Çöğür | Testler, Docker ile yayına alma, dokümantasyon | ⏳ |


