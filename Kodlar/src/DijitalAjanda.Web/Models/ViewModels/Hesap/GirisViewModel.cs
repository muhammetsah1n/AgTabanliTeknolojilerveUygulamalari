using System.ComponentModel.DataAnnotations;

namespace DijitalAjanda.Web.Models.ViewModels.Hesap;

public enum GirisTuru
{
    Kullanici,
    Yonetici
}

public class GirisViewModel
{
    [Required(ErrorMessage = "E-posta alanı zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi yaz.")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre alanı zorunludur.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Sifre { get; set; } = string.Empty;

    [Display(Name = "Beni hatırla")]
    public bool BeniHatirla { get; set; }

    public GirisTuru Tur { get; set; } = GirisTuru.Kullanici;

    public string? ReturnUrl { get; set; }

    public bool YoneticiMi => Tur == GirisTuru.Yonetici;
}
