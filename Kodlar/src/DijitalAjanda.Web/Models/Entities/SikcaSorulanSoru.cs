using System.ComponentModel.DataAnnotations;

namespace DijitalAjanda.Web.Models.Entities;

public class SikcaSorulanSoru
{
    public int Id { get; set; }

    [Required, StringLength(250)]
    public string Soru { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    public string Cevap { get; set; } = string.Empty;

    public int Sira { get; set; }
}
