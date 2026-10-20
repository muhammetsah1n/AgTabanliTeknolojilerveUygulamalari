document.addEventListener("DOMContentLoaded", function () {
    const html = document.documentElement;
    const buton = document.getElementById("temaDegistir");
    if (!buton) return;

    function ikonuGuncelle() {
        const koyu = html.getAttribute("data-bs-theme") === "dark";
        buton.innerHTML = koyu ? '<i class="bi bi-sun"></i>' : '<i class="bi bi-moon-stars"></i>';
    }

    buton.addEventListener("click", function () {
        const yeniTema = html.getAttribute("data-bs-theme") === "dark" ? "light" : "dark";
        html.setAttribute("data-bs-theme", yeniTema);
        try { localStorage.setItem("tema", yeniTema); } catch (e) { }
        ikonuGuncelle();
    });

    ikonuGuncelle();

    document.querySelectorAll(".pasif-link").forEach(function (link) {
        link.addEventListener("click", function (e) { e.preventDefault(); });
    });
});
