using System.ComponentModel.DataAnnotations;

namespace DijitalAjanda.Web.Models.ViewModels.Hesap;

// 1. adım: kodun gönderileceği e-posta
public class SifremiUnuttumViewModel
{
    [Required(ErrorMessage = "E-posta alanı zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi yaz.")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;
}
