namespace DijitalAjanda.Web.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    // null ise yakalanmamış bir hata (500), değilse 404 gibi bir durum kodu
    public int? DurumKodu { get; set; }

    public bool BulunamadiMi => DurumKodu == 404;
}
