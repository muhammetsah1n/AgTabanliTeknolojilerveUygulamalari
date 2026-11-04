using System.ComponentModel.DataAnnotations;

namespace DijitalAjanda.Web.Models.Entities;

// Bugün sayfasındaki yapılacak maddeleri (• yapılacak, × bitti)
public class Gorev
{
    public int Id { get; set; }

    [Required]
    public string KullaniciId { get; set; } = string.Empty;
    public Kullanici? Kullanici { get; set; }

    [Required, StringLength(150)]
    public string Baslik { get; set; } = string.Empty;

    public bool TamamlandiMi { get; set; }

    // Bugün sayfasındaki yapılacaklar bu tarihe göre listelenir
    public DateOnly? SonTarih { get; set; }

    // Gün içindeki sıra
    public int Sira { get; set; }

    public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;
}
