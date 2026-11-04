using System.Globalization;

namespace DijitalAjanda.Web.Models.ViewModels.Bugun;

// Sunucunun kültür ayarından bağımsız Türkçe tarih adları; hafta Pazartesi başlar
public static class TurkceTarih
{
    public static readonly string[] Aylar =
        { "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" };

    public static readonly string[] KisaAylar =
        { "Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara" };

    public static readonly string[] Gunler =
        { "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi", "Pazar" };

    public static readonly string[] KisaGunler = { "Pzt", "Sal", "Çar", "Per", "Cum", "Cmt", "Paz" };

    // Takvim başlıklarında kullanılan iki harfli gün adları
    public static readonly string[] TakvimGunleri = { "Pt", "Sa", "Ça", "Pe", "Cu", "Ct", "Pz" };

    // Sabit tarihli resmi tatiller; dini bayramlar 7. haftada Nager.Date API'sinden gelecek
    private static readonly Dictionary<(int Ay, int Gun), string> SabitTatiller = new()
    {
        [(1, 1)] = "Yılbaşı",
        [(4, 23)] = "Ulusal Egemenlik ve Çocuk Bayramı",
        [(5, 1)] = "Emek ve Dayanışma Günü",
        [(5, 19)] = "Atatürk’ü Anma, Gençlik ve Spor Bayramı",
        [(7, 15)] = "Demokrasi ve Millî Birlik Günü",
        [(8, 30)] = "Zafer Bayramı",
        [(10, 29)] = "Cumhuriyet Bayramı"
    };

    public static int GunSirasi(DateOnly tarih) => ((int)tarih.DayOfWeek + 6) % 7;

    public static DateOnly HaftaBasi(DateOnly tarih) => tarih.AddDays(-GunSirasi(tarih));

    public static string GunAdi(DateOnly tarih) => Gunler[GunSirasi(tarih)];

    public static string? Tatil(DateOnly tarih) =>
        SabitTatiller.TryGetValue((tarih.Month, tarih.Day), out var ad) ? ad : null;

    // 1 Kasım 2026, Pazar
    public static string UzunTarih(DateOnly tarih) =>
        $"{tarih.Day} {Aylar[tarih.Month - 1]} {tarih.Year}, {GunAdi(tarih)}";

    // 1 Kas, Paz
    public static string KisaTarih(DateOnly tarih) =>
        $"{tarih.Day} {KisaAylar[tarih.Month - 1]}, {KisaGunler[GunSirasi(tarih)]}";

    public static string AyBasligi(DateOnly tarih) => $"{Aylar[tarih.Month - 1]} {tarih.Year}";

    // 26 Eki – 01 Kas 2026 (yıl değişiyorsa ilk tarihe de yıl eklenir)
    public static string HaftaBasligi(DateOnly haftaBasi)
    {
        var haftaSonu = haftaBasi.AddDays(6);
        var ilkYil = haftaBasi.Year != haftaSonu.Year ? $" {haftaBasi.Year}" : "";
        return $"{haftaBasi.Day:00} {KisaAylar[haftaBasi.Month - 1]}{ilkYil} – " +
               $"{haftaSonu.Day:00} {KisaAylar[haftaSonu.Month - 1]} {haftaSonu.Year}";
    }

    // Formlarda ve adres satırında kullanılan biçimler (kültürden bağımsız)
    public static string Iso(DateOnly tarih) => tarih.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static string SaatMetni(TimeOnly saat) => saat.ToString("HH:mm", CultureInfo.InvariantCulture);
}
