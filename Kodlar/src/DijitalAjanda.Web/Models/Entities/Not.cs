using System.ComponentModel.DataAnnotations;

namespace DijitalAjanda.Web.Models.Entities;

// Bugün sayfasındaki günlük not maddeleri
public class Not
{
    public int Id { get; set; }

    [Required]
    public string KullaniciId { get; set; } = string.Empty;
    public Kullanici? Kullanici { get; set; }

    public DateOnly Tarih { get; set; }

    [Required, StringLength(1000)]
    public string Metin { get; set; } = string.Empty;

    public int Sira { get; set; }
}
