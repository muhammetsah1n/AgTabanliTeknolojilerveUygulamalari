using System.ComponentModel.DataAnnotations;

namespace DijitalAjanda.Web.Models.Entities;

public class SuKaydi
{
    public int Id { get; set; }

    [Required]
    public string KullaniciId { get; set; } = string.Empty;
    public Kullanici? Kullanici { get; set; }

    public DateOnly Tarih { get; set; }

    // 1 bardak = 250 ml
    [Range(0, 40)]
    public int BardakSayisi { get; set; }
}
