using System.ComponentModel.DataAnnotations;

namespace DijitalAjanda.Web.Models.Entities;

public class AltGorev
{
    public int Id { get; set; }

    public int GorevId { get; set; }
    public Gorev? Gorev { get; set; }

    [Required, StringLength(150)]
    public string Baslik { get; set; } = string.Empty;

    public bool TamamlandiMi { get; set; }
}
