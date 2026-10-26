using DijitalAjanda.Web.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DijitalAjanda.Web.Data;

public class AjandaDbContext : IdentityDbContext<Kullanici>
{
    public AjandaDbContext(DbContextOptions<AjandaDbContext> options) : base(options)
    {
    }

    public DbSet<Kategori> Kategoriler => Set<Kategori>();
    public DbSet<Gorev> Gorevler => Set<Gorev>();
    public DbSet<AltGorev> AltGorevler => Set<AltGorev>();
    public DbSet<Etkinlik> Etkinlikler => Set<Etkinlik>();
    public DbSet<Not> Notlar => Set<Not>();
    public DbSet<Sticker> Stickerlar => Set<Sticker>();
    public DbSet<Aliskanlik> Aliskanliklar => Set<Aliskanlik>();
    public DbSet<AliskanlikKaydi> AliskanlikKayitlari => Set<AliskanlikKaydi>();
    public DbSet<SuKaydi> SuKayitlari => Set<SuKaydi>();
    public DbSet<Kitap> Kitaplar => Set<Kitap>();
    public DbSet<SikcaSorulanSoru> SikcaSorulanSorular => Set<SikcaSorulanSoru>();
    public DbSet<IletisimMesaji> IletisimMesajlari => Set<IletisimMesaji>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Identity tablolarının (AspNetUsers, AspNetRoles...) kurulumu
        base.OnModelCreating(builder);

        builder.Entity<Kategori>(e =>
        {
            e.ToTable("Kategoriler");
            e.HasOne(k => k.Kullanici).WithMany(u => u.Kategoriler)
                .HasForeignKey(k => k.KullaniciId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(k => new { k.KullaniciId, k.Ad }).IsUnique();
        });

        builder.Entity<Gorev>(e =>
        {
            e.ToTable("Gorevler");
            e.HasOne(g => g.Kullanici).WithMany(u => u.Gorevler)
                .HasForeignKey(g => g.KullaniciId).OnDelete(DeleteBehavior.Cascade);
            // Kategori silinince görev kalır, kategorisiz olur
            e.HasOne(g => g.Kategori).WithMany(k => k.Gorevler)
                .HasForeignKey(g => g.KategoriId).OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(g => new { g.KullaniciId, g.SonTarih });
        });

        builder.Entity<AltGorev>(e =>
        {
            e.ToTable("AltGorevler");
            e.HasOne(a => a.Gorev).WithMany(g => g.AltGorevler)
                .HasForeignKey(a => a.GorevId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Etkinlik>(e =>
        {
            e.ToTable("Etkinlikler");
            e.HasOne(x => x.Kullanici).WithMany(u => u.Etkinlikler)
                .HasForeignKey(x => x.KullaniciId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Kategori).WithMany(k => k.Etkinlikler)
                .HasForeignKey(x => x.KategoriId).OnDelete(DeleteBehavior.SetNull);
            // Etkinlik saatleri kullanıcının girdiği yerel saat olarak saklanır
            e.Property(x => x.Baslangic).HasColumnType("timestamp without time zone");
            e.Property(x => x.Bitis).HasColumnType("timestamp without time zone");
            e.HasIndex(x => new { x.KullaniciId, x.Baslangic });
        });

        builder.Entity<Not>(e =>
        {
            e.ToTable("Notlar");
            e.HasOne(n => n.Kullanici).WithMany(u => u.Notlar)
                .HasForeignKey(n => n.KullaniciId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(n => new { n.KullaniciId, n.Tarih });
        });

        builder.Entity<Sticker>(e =>
        {
            e.ToTable("Stickerlar");
            e.HasOne(s => s.Kullanici).WithMany(u => u.Stickerlar)
                .HasForeignKey(s => s.KullaniciId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Aliskanlik>(e =>
        {
            e.ToTable("Aliskanliklar");
            e.HasOne(a => a.Kullanici).WithMany(u => u.Aliskanliklar)
                .HasForeignKey(a => a.KullaniciId).OnDelete(DeleteBehavior.Cascade);
            e.ToTable(t => t.HasCheckConstraint("CK_Aliskanliklar_HaftalikHedef", "\"HaftalikHedef\" BETWEEN 1 AND 7"));
        });

        builder.Entity<AliskanlikKaydi>(e =>
        {
            e.ToTable("AliskanlikKayitlari");
            e.HasOne(k => k.Aliskanlik).WithMany(a => a.Kayitlar)
                .HasForeignKey(k => k.AliskanlikId).OnDelete(DeleteBehavior.Cascade);
            // Bir alışkanlık aynı gün iki kez işaretlenemez
            e.HasIndex(k => new { k.AliskanlikId, k.Tarih }).IsUnique();
        });

        builder.Entity<SuKaydi>(e =>
        {
            e.ToTable("SuKayitlari");
            e.HasOne(s => s.Kullanici).WithMany(u => u.SuKayitlari)
                .HasForeignKey(s => s.KullaniciId).OnDelete(DeleteBehavior.Cascade);
            // Her kullanıcı için günde tek su kaydı
            e.HasIndex(s => new { s.KullaniciId, s.Tarih }).IsUnique();
        });

        builder.Entity<Kitap>(e =>
        {
            e.ToTable("Kitaplar");
            e.HasOne(k => k.Kullanici).WithMany(u => u.Kitaplar)
                .HasForeignKey(k => k.KullaniciId).OnDelete(DeleteBehavior.Cascade);
            e.ToTable(t => t.HasCheckConstraint("CK_Kitaplar_Puan", "\"Puan\" BETWEEN 0 AND 5"));
        });

        builder.Entity<SikcaSorulanSoru>().ToTable("SikcaSorulanSorular");
        builder.Entity<IletisimMesaji>().ToTable("IletisimMesajlari");
    }
}
