using System.ComponentModel.DataAnnotations;

namespace DijitalAjanda.Web.Models.Entities;

public class Sticker
{
    public int Id { get; set; }

    [Required]
    public string KullaniciId { get; set; } = string.Empty;
    public Kullanici? Kullanici { get; set; }

    public DateOnly Tarih { get; set; }

    // wwwroot/uploads altındaki dosyanın yolu
    [Required, StringLength(260)]
    public string GorselYolu { get; set; } = string.Empty;

    // Sayfa üzerindeki konum (yüzde olarak)
    public double KonumX { get; set; }
    public double KonumY { get; set; }
}
