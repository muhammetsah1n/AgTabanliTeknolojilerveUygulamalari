<div align="center">

# Dijital Akademik Ajanda ve Çalışma Alanı

Notlarını, yapılacaklarını, etkinliklerini ve alışkanlıklarını tek bir yerden yönetebileceğin,
bullet journal yöntemiyle tasarlanmış ağ tabanlı bir ajanda uygulaması.

![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16+-4169E1?style=flat-square&logo=postgresql&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=flat-square&logo=bootstrap&logoColor=white)

<sub>Ankara Üniversitesi · BLM4581 Ağ Tabanlı Teknolojiler ve Uygulamaları · 2026–2027 Güz</sub>

</div>

---

## Ekip — Dreamfire

| Ad Soyad | GitHub | Liderlik |
|---|---|---|
| Yağmur Kalyoncu |  [@Yagmurkalyoncu](https://github.com/Yagmurkalyoncu) | Hafta 1–3 |
| Muhammet Şahin  | [@muhammetsah1n](https://github.com/muhammetsah1n) | Hafta 4–6 |
| Şahnur Çöğür    | [@sahnurcgr](https://github.com/sahnurcgr) | Hafta 7–9 |

## Teknolojiler

- **ASP.NET Core MVC** (.NET 10)
- **PostgreSQL** + **Entity Framework Core** (Npgsql, Code-First, migration)
- **ASP.NET Core Identity** — giriş, kayıt, şifre sıfırlama, `Admin` / `Kullanici` rolleri
- **ASP.NET Core Web API** + **Swagger** (Swashbuckle) — yapılacaklar için REST API
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

## Yapılacaklar Web API'si

Bugün sayfasındaki yapılacaklar (gün görünümünde ekleme, işaretleme, metne tıklayıp düzenleme, silme; hafta ve ay
kutularında ekleme, düzenleme, silme) sayfa yenilenmeden bu API ile yapılır.
İstekler oturum çerezi ile yetkilendirilir; her kullanıcı yalnızca kendi maddelerini görür.

| Yöntem | Adres | Açıklama | Başarılı yanıt |
|---|---|---|---|
| GET | `/api/yapilacaklar?tarih=2026-11-09` | Günün yapılacakları (tarih verilmezse hepsi) | 200 |
| GET | `/api/yapilacaklar/{id}` | Tek yapılacak | 200 |
| POST | `/api/yapilacaklar` | `{ "baslik", "tarih" }` ile ekler | 201 |
| PUT | `/api/yapilacaklar/{id}` | `{ "baslik", "tarih", "tamamlandiMi" }` ile günceller | 200 |
| DELETE | `/api/yapilacaklar/{id}` | Siler | 204 |
| PATCH | `/api/yapilacaklar/{id}/tamamla` | `{ "tamamlandiMi": true }` ile işaretler; gövdesizse durumu tersine çevirir | 200 |

Hatalı istekler Türkçe mesajlı `400` (alan bazında `errors`), oturum yoksa `401`, kayıt yoksa `404` döner.

- **Swagger:** geliştirme ortamında <http://localhost:5211/swagger>. Önce uygulamada giriş yapın, sonra Swagger'dan deneyin.
- **Postman:** `Kodlar/tests/postman/DijitalAjanda-Yapilacaklar.postman_collection.json` koleksiyonunu içe aktarın,
  `email` / `sifre` değişkenlerine kayıtlı bir kullanıcı yazın ve Runner ile çalıştırın (giriş, CRUD ve doğrulama testleri).

## Roller

| Rol | Erişim |
|---|---|
| Ziyaretçi | Ana sayfa, kayıt, giriş, şifremi unuttum, gizlilik |
| Kullanici | Bugün sayfası (kendi not, yapılacak ve etkinlikleri), Yapılacaklar API'si, Alışkanlıklar, Kitaplık ve diğer ajanda sayfaları |
| Admin | Yönetim paneli (`/Yonetim`) — giriş ekranındaki **Yönetici girişi** sekmesinden |

Yetkisiz erişimler **Erişim engellendi**, olmayan sayfalar ve hatalar **Hata** sayfasına yönlendirilir.

## Veritabanı Tabloları

| Tablo | İlişki |
|---|---|
| AspNetUsers (Kullanici) | 1–N: Notlar, Gorevler, Etkinlikler, Stickerlar, Aliskanliklar, SuKayitlari, Kitaplar |
| Notlar, Gorevler, Etkinlikler | Bugün sayfasının not (–), yapılacak (• / ×) ve etkinlik (○) maddeleri; etkinliklerde tür (7 hazır tür ya da kullanıcının kendi türü), bitiş saati ve hatırlatıcı (kaç × dakika / saat / gün / hafta) |
| Aliskanliklar | 1–N: AliskanlikKayitlari (aynı gün tek kayıt) |
| SuKayitlari | Kullanıcı başına günde tek kayıt |
| SikcaSorulanSorular, IletisimMesajlari | Yönetici panelinden yönetilir |

## Klasör Yapısı

```text
ProjeDokümanları/            Proje kuralları ve plan belgesi
Kodlar/
├── tests/postman/           Yapılacaklar API'sinin Postman koleksiyonu
└── src/DijitalAjanda.Web/
    ├── Controllers/         Home, Hesap, Bugun, Tema, Aliskanliklar, Yonetim; Api/YapilacaklarController
    ├── Data/                DbContext, roller, seed işlemi, migration'lar
    ├── Models/              Entities (tablolar), ViewModels (Hesap ve Bugun form/sayfa modelleri), Api (istek/yanıt modelleri)
    ├── Services/            Bugün, madde ve yapılacak servisleri, e-posta, Türkçe Identity mesajları
    ├── Views/               Home, Hesap, Bugun (gün/hafta/ay), Shared (layout, header, footer, hata)
    ├── wwwroot/             site.css/js (tema), bugun.css/js ve kütüphaneler
    └── Program.cs
```

## Haftalık İlerleme

| Hafta | Lider | Kapsam | Durum |
|:---:|---|---|:---:|
| 1 | Yağmur Kalyoncu | Proje iskeleti, ortak sayfa düzeni, açık/koyu tema, ana sayfa | ✅ |
| 2 | Yağmur Kalyoncu | Entity'ler, PostgreSQL + migration, kayıt/giriş, şifre sıfırlama, roller, hata sayfaları | ✅ |
| 3 | Yağmur Kalyoncu | Bugün sayfası (gün/hafta/ay; hafta ve ay kutularında ekleme, düzenleme, silme; saat kadranıyla etkinlik ekleme; takvim ve resmi tatiller; yaklaşan etkinlikler), tanıtım sayfası, hesaba kaydedilen tema, responsive düzen | ✅ |
| 4 | Muhammet Şahin | Yapılacaklar Web API + Swagger, Bugün sayfasının API'ye bağlanması, etkinlik türü / bitiş saati / hatırlatıcı, model doğrulama, Postman testleri | ✅ |
| 5 | Muhammet Şahin | Alışkanlık ve su takibi | ⏳ |
| 6 | Muhammet Şahin | Kitaplık, sticker yükleme ve sürükleme | ⏳ |
| 7 | Şahnur Çöğür | Nager.Date harici API, SSS / Bize ulaşın sayfaları | ⏳ |
| 8 | Şahnur Çöğür | SignalR anlık güncelleme ve hatırlatıcılar, Admin paneli | ⏳ |
| 9 | Şahnur Çöğür | Testler, Docker ile yayına alma, dokümantasyon | ⏳ |


