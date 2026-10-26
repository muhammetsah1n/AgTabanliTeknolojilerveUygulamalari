using System.ComponentModel.DataAnnotations;

namespace DijitalAjanda.Web.Models.Entities;

public class Etkinlik
{
    public int Id { get; set; }

    [Required]
    public string KullaniciId { get; set; } = string.Empty;
    public Kullanici? Kullanici { get; set; }

    public int? KategoriId { get; set; }
    public Kategori? Kategori { get; set; }

    [Required, StringLength(150)]
    public string Baslik { get; set; } = string.Empty;

    public EtkinlikTuru Tur { get; set; } = EtkinlikTuru.Etkinlik;

    public DateTime Baslangic { get; set; }

    public DateTime? Bitis { get; set; }

    // Etkinlikten kaç dakika önce hatırlatılacağı; null ise hatırlatma yok
    public int? HatirlaticiDk { get; set; }
}
