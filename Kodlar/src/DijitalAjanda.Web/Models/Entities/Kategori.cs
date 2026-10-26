using System.ComponentModel.DataAnnotations;

namespace DijitalAjanda.Web.Models.Entities;

public class Kategori
{
    public int Id { get; set; }

    [Required]
    public string KullaniciId { get; set; } = string.Empty;
    public Kullanici? Kullanici { get; set; }

    [Required, StringLength(50)]
    public string Ad { get; set; } = string.Empty;

    // #RRGGBB biçiminde
    [Required, StringLength(7)]
    public string Renk { get; set; } = "#cddff2";

    public ICollection<Gorev> Gorevler { get; set; } = new List<Gorev>();
    public ICollection<Etkinlik> Etkinlikler { get; set; } = new List<Etkinlik>();
}
