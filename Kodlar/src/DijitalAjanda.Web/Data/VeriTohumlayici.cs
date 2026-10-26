using DijitalAjanda.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DijitalAjanda.Web.Data;

// Uygulama açılırken bekleyen migration'ları uygular, rolleri ve yönetici hesabını oluşturur
public static class VeriTohumlayici
{
    public static async Task CalistirAsync(IServiceProvider servisler)
    {
        using var kapsam = servisler.CreateScope();
        var sp = kapsam.ServiceProvider;
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(VeriTohumlayici));

        var db = sp.GetRequiredService<AjandaDbContext>();
        await db.Database.MigrateAsync();

        var rolYonetici = sp.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var rol in Roller.Tumu)
        {
            if (!await rolYonetici.RoleExistsAsync(rol))
            {
                await rolYonetici.CreateAsync(new IdentityRole(rol));
                logger.LogInformation("{Rol} rolü oluşturuldu.", rol);
            }
        }

        // Yönetici bilgileri depoya konmaz; user-secrets veya ortam değişkeninden okunur
        var config = sp.GetRequiredService<IConfiguration>();
        var adminEmail = config["Admin:Email"];
        var adminSifre = config["Admin:Sifre"];
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminSifre))
        {
            logger.LogWarning("Admin:Email / Admin:Sifre ayarı bulunamadı, yönetici hesabı oluşturulmadı.");
            return;
        }

        var kullaniciYonetici = sp.GetRequiredService<UserManager<Kullanici>>();
        var admin = await kullaniciYonetici.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new Kullanici
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                AdSoyad = "Yönetici"
            };
            var sonuc = await kullaniciYonetici.CreateAsync(admin, adminSifre);
            if (!sonuc.Succeeded)
            {
                logger.LogError("Yönetici hesabı oluşturulamadı: {Hatalar}",
                    string.Join(", ", sonuc.Errors.Select(h => h.Description)));
                return;
            }
            logger.LogInformation("Yönetici hesabı oluşturuldu: {Email}", adminEmail);
        }

        if (!await kullaniciYonetici.IsInRoleAsync(admin, Roller.Admin))
        {
            await kullaniciYonetici.AddToRoleAsync(admin, Roller.Admin);
        }
    }
}
