using DijitalAjanda.Web.Data;
using DijitalAjanda.Web.Models.Entities;
using DijitalAjanda.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Bağlantı dizesi depoda değil, user-secrets / ortam değişkeninde tutulur
var baglanti = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(baglanti))
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection ayarlanmadı. README.md'deki kurulum adımlarına bakın.");
}

builder.Services.AddDbContext<AjandaDbContext>(options => options.UseNpgsql(baglanti));

builder.Services
    .AddIdentity<Kullanici, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

        // Şifre sıfırlama için e-postaya 6 haneli kod üreten sağlayıcı
        options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
    })
    .AddEntityFrameworkStores<AjandaDbContext>()
    .AddDefaultTokenProviders()
    .AddErrorDescriber<TurkceIdentityHatalari>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Hesap/Giris";
    options.LogoutPath = "/Hesap/Cikis";
    options.AccessDeniedPath = "/Hesap/ErisimEngellendi";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
});

builder.Services.AddScoped<IEpostaGonderici, SmtpEpostaGonderici>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

await VeriTohumlayici.CalistirAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// 404, 403 gibi durum kodlarında da tasarımdaki hata sayfası gösterilir
app.UseStatusCodePagesWithReExecute("/Home/Error", "?kod={0}");

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
