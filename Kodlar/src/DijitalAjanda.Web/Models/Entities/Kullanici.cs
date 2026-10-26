using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace DijitalAjanda.Web.Models.Entities;

// AspNetUsers tablosu: Identity alanlarına ek olarak projeye özel alanlar
public class Kullanici : IdentityUser
{
    [Required, StringLength(100)]
    public string AdSoyad { get; set; } = string.Empty;

    public Tema TemaTercihi { get; set; } = Tema.Sistem;

    // 1 bardak = 250 ml, varsayılan hedef 8 bardak
    public int GunlukSuHedefiMl { get; set; } = 2000;

    public DateTime KayitTarihi { get; set; } = DateTime.UtcNow;

    public DateTime? SonGirisTarihi { get; set; }

    public bool AktifMi { get; set; } = true;

    public ICollection<Kategori> Kategoriler { get; set; } = new List<Kategori>();
    public ICollection<Gorev> Gorevler { get; set; } = new List<Gorev>();
    public ICollection<Etkinlik> Etkinlikler { get; set; } = new List<Etkinlik>();
    public ICollection<Not> Notlar { get; set; } = new List<Not>();
    public ICollection<Sticker> Stickerlar { get; set; } = new List<Sticker>();
    public ICollection<Aliskanlik> Aliskanliklar { get; set; } = new List<Aliskanlik>();
    public ICollection<SuKaydi> SuKayitlari { get; set; } = new List<SuKaydi>();
    public ICollection<Kitap> Kitaplar { get; set; } = new List<Kitap>();
}
