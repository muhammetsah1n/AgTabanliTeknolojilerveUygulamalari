namespace DijitalAjanda.Web.Models.Entities;

// Alışkanlığın işaretlendiği her gün için bir kayıt; seri hesabı buradan yapılır
public class AliskanlikKaydi
{
    public int Id { get; set; }

    public int AliskanlikId { get; set; }
    public Aliskanlik? Aliskanlik { get; set; }

    public DateOnly Tarih { get; set; }
}
