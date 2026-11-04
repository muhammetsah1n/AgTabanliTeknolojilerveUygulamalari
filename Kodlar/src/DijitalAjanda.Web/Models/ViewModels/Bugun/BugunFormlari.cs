using System.ComponentModel.DataAnnotations;

namespace DijitalAjanda.Web.Models.ViewModels.Bugun;

// Her form, işlemden sonra aynı görünüme ve tarihe dönebilmek için bu iki alanı taşır
public class SayfaKonumu
{
    public Gorunum Gorunum { get; set; } = Gorunum.Gun;
    public DateOnly Odak { get; set; }
}

public class YeniNotFormu : SayfaKonumu
{
    public DateOnly Tarih { get; set; }

    [Required(ErrorMessage = "Not boş olamaz.")]
    [StringLength(1000, ErrorMessage = "Not en fazla {1} karakter olabilir.")]
    public string Metin { get; set; } = string.Empty;
}

public class NotGuncelleFormu : SayfaKonumu
{
    public int Id { get; set; }

    // Boş bırakılan not silinir
    [StringLength(1000, ErrorMessage = "Not en fazla {1} karakter olabilir.")]
    public string? Metin { get; set; }
}

public class YeniYapilacakFormu : SayfaKonumu
{
    public DateOnly Tarih { get; set; }

    [Required(ErrorMessage = "Yapılacak boş olamaz.")]
    [StringLength(150, ErrorMessage = "Yapılacak en fazla {1} karakter olabilir.")]
    public string Baslik { get; set; } = string.Empty;
}

public class YapilacakGuncelleFormu : SayfaKonumu
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Yapılacak boş olamaz.")]
    [StringLength(150, ErrorMessage = "Yapılacak en fazla {1} karakter olabilir.")]
    public string Baslik { get; set; } = string.Empty;
}

public class YeniEtkinlikFormu : SayfaKonumu
{
    [Required(ErrorMessage = "Etkinlik adı boş olamaz.")]
    [StringLength(150, ErrorMessage = "Etkinlik adı en fazla {1} karakter olabilir.")]
    public string Baslik { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tarih seçin.")]
    public DateOnly? Tarih { get; set; }

    [Required(ErrorMessage = "Saat seçin.")]
    public TimeOnly? Saat { get; set; }
}

public class EtkinlikGuncelleFormu : SayfaKonumu
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Etkinlik adı boş olamaz.")]
    [StringLength(150, ErrorMessage = "Etkinlik adı en fazla {1} karakter olabilir.")]
    public string Baslik { get; set; } = string.Empty;

    // Boşsa etkinliğin saati değişmez
    public TimeOnly? Saat { get; set; }
}

public class MaddeSilFormu : SayfaKonumu
{
    public int Id { get; set; }
}
