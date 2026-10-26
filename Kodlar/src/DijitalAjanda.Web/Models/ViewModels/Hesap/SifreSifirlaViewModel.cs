using System.ComponentModel.DataAnnotations;

namespace DijitalAjanda.Web.Models.ViewModels.Hesap;

// 2. adım: e-postaya gelen 6 haneli kod ve yeni şifre
public class SifreSifirlaViewModel
{
    [Required(ErrorMessage = "E-posta alanı zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi yaz.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sıfırlama kodunu yaz.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Kod 6 haneli olmalı.")]
    [Display(Name = "Sıfırlama kodu")]
    public string Kod { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yeni şifre alanı zorunludur.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Yeni şifre en az 6 karakter olmalı.")]
    [DataType(DataType.Password)]
    [Display(Name = "Yeni şifre")]
    public string YeniSifre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yeni şifreyi tekrar yaz.")]
    [Compare(nameof(YeniSifre), ErrorMessage = "Şifreler aynı değil.")]
    [DataType(DataType.Password)]
    [Display(Name = "Yeni şifre (tekrar)")]
    public string YeniSifreTekrar { get; set; } = string.Empty;
}
