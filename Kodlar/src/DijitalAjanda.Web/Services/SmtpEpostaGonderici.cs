using System.Net;
using System.Net.Mail;

namespace DijitalAjanda.Web.Services;

public class SmtpAyarlari
{
    public string? Host { get; set; }
    public int Port { get; set; } = 587;
    public bool Ssl { get; set; } = true;
    public string? Kullanici { get; set; }
    public string? Sifre { get; set; }
    public string Gonderen { get; set; } = "Dijital Ajanda <no-reply@dijitalajanda.local>";
}

public class SmtpEpostaGonderici : IEpostaGonderici
{
    private readonly SmtpAyarlari _ayarlar;
    private readonly ILogger<SmtpEpostaGonderici> _logger;

    public SmtpEpostaGonderici(IConfiguration config, ILogger<SmtpEpostaGonderici> logger)
    {
        _ayarlar = config.GetSection("Smtp").Get<SmtpAyarlari>() ?? new SmtpAyarlari();
        _logger = logger;
    }

    public async Task GonderAsync(string alici, string konu, string htmlIcerik)
    {
        // SMTP ayarı yoksa (geliştirme ortamı) e-posta konsola yazılır
        if (string.IsNullOrWhiteSpace(_ayarlar.Host))
        {
            _logger.LogWarning("SMTP ayarlanmadı, e-posta gönderilmedi. Alıcı: {Alici}, Konu: {Konu}\n{Icerik}",
                alici, konu, htmlIcerik);
            return;
        }

        using var mesaj = new MailMessage
        {
            From = new MailAddress(_ayarlar.Gonderen),
            Subject = konu,
            Body = htmlIcerik,
            IsBodyHtml = true
        };
        mesaj.To.Add(alici);

        using var istemci = new SmtpClient(_ayarlar.Host, _ayarlar.Port)
        {
            EnableSsl = _ayarlar.Ssl,
            Credentials = new NetworkCredential(_ayarlar.Kullanici, _ayarlar.Sifre)
        };
        await istemci.SendMailAsync(mesaj);
    }
}
