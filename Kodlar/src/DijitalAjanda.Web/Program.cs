using DijitalAjanda.Web.Data;
using DijitalAjanda.Web.Models.Entities;
using DijitalAjanda.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

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

    // Web API isteklerinde giriş sayfasına yönlendirmek yerine 401 / 403 döner
    options.Events.OnRedirectToLogin = context => ApiYanitiVeyaYonlendir(context, StatusCodes.Status401Unauthorized);
    options.Events.OnRedirectToAccessDenied = context => ApiYanitiVeyaYonlendir(context, StatusCodes.Status403Forbidden);
});

builder.Services.AddScoped<IEpostaGonderici, SmtpEpostaGonderici>();
builder.Services.AddScoped<IBugunServisi, BugunServisi>();
builder.Services.AddScoped<IMaddeServisi, MaddeServisi>();
builder.Services.AddScoped<IYapilacakServisi, YapilacakServisi>();

builder.Services.AddControllersWithViews(options => TurkceDogrulamaMesajlari(options.ModelBindingMessageProvider))
    // Hatalı JSON'da iç hata metni yerine yukarıdaki Türkçe mesajlar gösterilir
    .AddJsonOptions(options => options.AllowInputFormatterExceptionMessages = false)
    .ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = ApiDogrulamaYaniti);

// Yapılacaklar Web API'sinin belgesi: /swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Dijital Ajanda API",
        Version = "v1",
        Description = "Bugün sayfasındaki yapılacak maddeleri için REST API. " +
                      "İstekler oturum çerezi ile yetkilendirilir: önce uygulamada giriş yapın, sonra bu sayfadan deneyin."
    });
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml"));
});

var app = builder.Build();

await VeriTohumlayici.CalistirAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.DocumentTitle = "Dijital Ajanda API");
}

// 404, 403 gibi durum kodlarında da tasarımdaki hata sayfası gösterilir (API istekleri JSON almaya devam eder)
app.UseWhen(
    context => !ApiIstegi(context.Request),
    sayfalar => sayfalar.UseStatusCodePagesWithReExecute("/Home/Error", "?kod={0}"));

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

static bool ApiIstegi(HttpRequest istek) => istek.Path.StartsWithSegments("/api");

static Task ApiYanitiVeyaYonlendir(Microsoft.AspNetCore.Authentication.RedirectContext<Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationOptions> context, int durumKodu)
{
    if (ApiIstegi(context.Request))
    {
        context.Response.StatusCode = durumKodu;
    }
    else
    {
        context.Response.Redirect(context.RedirectUri);
    }
    return Task.CompletedTask;
}

// Web API doğrulama hatası yanıtı (400, application/problem+json).
// Okunamayan JSON alanları ("$.tarih") alan adıyla ve Türkçe mesajla bildirilir; JSON okunamadığı için
// gövdenin "boş" sayılmasından doğan ikinci hata gösterilmez.
static Microsoft.AspNetCore.Mvc.IActionResult ApiDogrulamaYaniti(Microsoft.AspNetCore.Mvc.ActionContext context)
{
    const string OkunamadiMesaji = "Değer okunamadı; JSON biçimini ve alanın türünü kontrol edin.";
    var parametreler = context.ActionDescriptor.Parameters.Select(p => p.Name).ToHashSet();
    var jsonHatasiVar = context.ModelState.Keys.Any(k => k.StartsWith('$'));
    var hatalar = new Dictionary<string, string[]>();

    foreach (var (anahtar, kayit) in context.ModelState)
    {
        if (kayit.Errors.Count == 0) continue;

        if (anahtar.StartsWith('$'))
        {
            var alan = anahtar.TrimStart('$', '.');
            hatalar[alan.Length == 0 ? "govde" : alan] = new[] { OkunamadiMesaji };
        }
        else if (!(jsonHatasiVar && parametreler.Contains(anahtar)))
        {
            hatalar[anahtar] = kayit.Errors
                .Select(h => string.IsNullOrEmpty(h.ErrorMessage) ? OkunamadiMesaji : h.ErrorMessage)
                .ToArray();
        }
    }

    var sorun = new Microsoft.AspNetCore.Mvc.ValidationProblemDetails(hatalar)
    {
        Title = "Gönderilen bilgiler geçersiz.",
        Status = StatusCodes.Status400BadRequest,
        Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1"
    };
    sorun.Extensions["traceId"] = System.Diagnostics.Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
    return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(sorun) { ContentTypes = { "application/problem+json" } };
}

// Model bağlama hatalarının (ör. geçersiz tarih ya da sayı) Türkçe mesajları
static void TurkceDogrulamaMesajlari(Microsoft.AspNetCore.Mvc.ModelBinding.Metadata.DefaultModelBindingMessageProvider m)
{
    m.SetAttemptedValueIsInvalidAccessor((deger, alan) => $"'{deger}' değeri {alan} için geçerli değil.");
    m.SetNonPropertyAttemptedValueIsInvalidAccessor(deger => $"'{deger}' geçerli bir değer değil.");
    m.SetUnknownValueIsInvalidAccessor(alan => $"{alan} için girilen değer geçerli değil.");
    m.SetNonPropertyUnknownValueIsInvalidAccessor(() => "Girilen değer geçerli değil.");
    m.SetValueIsInvalidAccessor(deger => $"'{deger}' geçerli bir değer değil.");
    m.SetValueMustBeANumberAccessor(alan => $"{alan} bir sayı olmalı.");
    m.SetNonPropertyValueMustBeANumberAccessor(() => "Değer bir sayı olmalı.");
    m.SetValueMustNotBeNullAccessor(deger => $"'{deger}' değeri boş olamaz.");
    m.SetMissingBindRequiredValueAccessor(alan => $"{alan} alanı gerekli.");
    m.SetMissingKeyOrValueAccessor(() => "Değer gerekli.");
    m.SetMissingRequestBodyRequiredValueAccessor(() => "İstek gövdesi boş olamaz.");
}
