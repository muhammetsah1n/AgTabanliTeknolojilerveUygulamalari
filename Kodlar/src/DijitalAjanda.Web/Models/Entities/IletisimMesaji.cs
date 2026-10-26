using System.ComponentModel.DataAnnotations;

namespace DijitalAjanda.Web.Models.Entities;

public class IletisimMesaji
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string AdSoyad { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    public MesajKonusu Konu { get; set; } = MesajKonusu.Diger;

    [Required, StringLength(2000)]
    public string Mesaj { get; set; } = string.Empty;

    public DateTime Tarih { get; set; } = DateTime.UtcNow;

    public bool OkunduMu { get; set; }
}
