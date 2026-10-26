using System.ComponentModel.DataAnnotations;

namespace DijitalAjanda.Web.Models.Entities;

public class Aliskanlik
{
    public int Id { get; set; }

    [Required]
    public string KullaniciId { get; set; } = string.Empty;
    public Kullanici? Kullanici { get; set; }

    [Required, StringLength(80)]
    public string Ad { get; set; } = string.Empty;

    // Haftada kaç gün yapılacağı (1–7)
    [Range(1, 7)]
    public int HaftalikHedef { get; set; } = 7;

    [Required, StringLength(7)]
    public string Renk { get; set; } = "#d4e8d4";

    public DateOnly BaslangicTarihi { get; set; }

    public ICollection<AliskanlikKaydi> Kayitlar { get; set; } = new List<AliskanlikKaydi>();
}
