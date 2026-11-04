document.addEventListener("DOMContentLoaded", function () {
    const html = document.documentElement;
    const buton = document.getElementById("temaDegistir");

    function ikonuGuncelle() {
        if (!buton) return;
        const koyu = html.getAttribute("data-bs-theme") === "dark";
        buton.innerHTML = koyu ? '<i class="bi bi-sun"></i>' : '<i class="bi bi-moon-stars"></i>';
    }

    // Giriş yapılmışsa seçim hesaba da kaydedilir (bkz. TemaController)
    function hesabaKaydet(tema) {
        const adres = document.querySelector('meta[name="tema-adresi"]');
        const anahtar = document.querySelector('meta[name="istek-dogrulama"]');
        if (!adres || !anahtar) return;

        fetch(adres.content, {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded",
                "RequestVerificationToken": anahtar.content
            },
            body: "tema=" + (tema === "dark" ? "Koyu" : "Acik")
        }).catch(function () { });
    }

    if (buton) {
        buton.addEventListener("click", function () {
            const yeniTema = html.getAttribute("data-bs-theme") === "dark" ? "light" : "dark";
            html.setAttribute("data-bs-theme", yeniTema);
            try { localStorage.setItem("tema", yeniTema); } catch (e) { }
            ikonuGuncelle();
            hesabaKaydet(yeniTema);
        });
    }

    // Aynı tarayıcıda açık diğer sekmeler de temayı hemen değiştirir
    window.addEventListener("storage", function (e) {
        if (e.key === "tema" && (e.newValue === "dark" || e.newValue === "light")) {
            html.setAttribute("data-bs-theme", e.newValue);
            ikonuGuncelle();
        }
    });

    ikonuGuncelle();

    document.querySelectorAll(".pasif-link").forEach(function (link) {
        link.addEventListener("click", function (e) { e.preventDefault(); });
    });
});
