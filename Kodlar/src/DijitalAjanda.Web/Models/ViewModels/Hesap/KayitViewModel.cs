using System.ComponentModel.DataAnnotations;

namespace DijitalAjanda.Web.Models.ViewModels.Hesap;

public class KayitViewModel
{
    [Required(ErrorMessage = "Ad soyad alanı zorunludur.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Ad soyad 2–100 karakter olmalı.")]
    [Display(Name = "Ad soyad")]
    public string AdSoyad { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta alanı zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi yaz.")]
    [StringLength(256)]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre alanı zorunludur.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalı.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Sifre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifreyi tekrar yaz.")]
    [Compare(nameof(Sifre), ErrorMessage = "Şifreler aynı değil.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre (tekrar)")]
    public string SifreTekrar { get; set; } = string.Empty;
}
