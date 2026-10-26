using System.ComponentModel.DataAnnotations;

namespace DijitalAjanda.Web.Models.Entities;

public class Gorev
{
    public int Id { get; set; }

    [Required]
    public string KullaniciId { get; set; } = string.Empty;
    public Kullanici? Kullanici { get; set; }

    public int? KategoriId { get; set; }
    public Kategori? Kategori { get; set; }

    [Required, StringLength(150)]
    public string Baslik { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Aciklama { get; set; }

    public GorevDurumu Durum { get; set; } = GorevDurumu.Yapilacak;

    public Oncelik Oncelik { get; set; } = Oncelik.Orta;

    // Bugün sayfasındaki yapılacaklar bu tarihe göre listelenir
    public DateOnly? SonTarih { get; set; }

    // Kanban sütunu içindeki sıra
    public int Sira { get; set; }

    public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;

    public ICollection<AltGorev> AltGorevler { get; set; } = new List<AltGorev>();
}
