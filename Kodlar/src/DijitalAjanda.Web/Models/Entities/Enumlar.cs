namespace DijitalAjanda.Web.Models.Entities;

public enum GorevDurumu
{
    Yapilacak = 0,
    DevamEdiyor = 1,
    Bitti = 2
}

public enum Oncelik
{
    Dusuk = 0,
    Orta = 1,
    Yuksek = 2
}

public enum EtkinlikTuru
{
    Etkinlik = 0,
    Randevu = 1,
    Toplanti = 2
}

public enum KitapTuru
{
    Kitap = 0,
    Film = 1
}

public enum MesajKonusu
{
    Oneri = 0,
    Hata = 1,
    Diger = 2
}

public enum Tema
{
    Sistem = 0,
    Acik = 1,
    Koyu = 2
}
