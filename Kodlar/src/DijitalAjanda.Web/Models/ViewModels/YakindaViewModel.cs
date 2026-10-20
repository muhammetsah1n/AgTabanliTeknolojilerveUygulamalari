namespace DijitalAjanda.Web.Models.ViewModels;

public class YakindaViewModel
{
    public string Baslik { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
    public string Ikon { get; set; } = "bi-stars";
    public int PlanlananHafta { get; set; }
    public List<string> Ozellikler { get; set; } = new();
}
