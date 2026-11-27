// Alışkanlıklar ve Su takibi sayfaları. Bütün düğmeler JavaScript olmadan da normal form olarak çalışır;
// burada istekler sayfa yenilenmeden gönderilir ve sayfa sunucunun döndürdüğü durumla güncellenir.
document.addEventListener("DOMContentLoaded", function () {
    const hataKutusu = document.getElementById("alHata");
    let hataZamani;

    function hataGoster(mesaj) {
        if (!hataKutusu) { alert(mesaj); return; }
        hataKutusu.textContent = mesaj;
        hataKutusu.hidden = false;
        clearTimeout(hataZamani);
        hataZamani = setTimeout(function () { hataKutusu.hidden = true; }, 6000);
    }

    // Formdaki doğrulama anahtarıyla arka planda POST isteği; başarısızsa sunucunun mesajını fırlatır
    async function gonder(adres, form, alanlar) {
        const govde = new URLSearchParams(alanlar || {});
        const anahtar = form.querySelector('input[name="__RequestVerificationToken"]');
        if (anahtar) govde.set("__RequestVerificationToken", anahtar.value);

        const yanit = await fetch(adres, {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded",
                "X-Requested-With": "XMLHttpRequest"
            },
            body: govde
        });
        if (yanit.redirected || yanit.status === 401) throw new Error("Oturumun sona ermiş, lütfen yeniden giriş yap.");
        if (!yanit.ok) {
            let mesaj = "Kaydedilemedi, lütfen tekrar deneyin.";
            try { mesaj = (await yanit.json()).hata || mesaj; } catch (e) { }
            throw new Error(mesaj);
        }
        return yanit.json();
    }

    aliskanliklarSayfasi();
    suTakibiSayfasi();

    // ---------- Alışkanlıklar: haftalık ızgara ve açılır form ----------

    function aliskanliklarSayfasi() {
        const bolge = document.getElementById("alForm");
        if (!bolge) return;
        formuBagla(bolge);

        // Henüz alışkanlık yoksa ızgara (tablo) sayfada yoktur
        const izgara = document.getElementById("alIzgara");
        if (!izgara) return;

        const renkSinifi = (satir) =>
            Array.from(satir.querySelector(".al-nokta").classList).find((s) => s.startsWith("renk-")) || "";

        satirDuzenlemeyiBagla(izgara, renkSinifi);

        izgara.addEventListener("submit", async function (e) {
            const dugme = e.submitter;
            if (!dugme) return;
            e.preventDefault();

            const satir = dugme.closest("tr");

            // Silme
            if (dugme.dataset.sil !== undefined) {
                if (!confirm("“" + dugme.dataset.sil + "” alışkanlığı ve işaretlediğin bütün günler silinsin mi?")) return;
                dugme.disabled = true;
                try {
                    await gonder(dugme.formAction, izgara);
                    satir.remove();
                    // Son alışkanlık silinince tablo da kaldırılır
                    if (!izgara.querySelector("tbody tr")) location.reload();
                } catch (hata) {
                    hataGoster(hata.message);
                    dugme.disabled = false;
                }
                return;
            }

            // Kutuyu işaretleme / işareti kaldırma
            dugme.disabled = true;
            try {
                const durum = await gonder(dugme.formAction, izgara, { tarih: dugme.value });
                const sinif = renkSinifi(satir);
                dugme.classList.toggle("isaretli", durum.isaretli);
                if (sinif) dugme.classList.toggle(sinif, durum.isaretli);
                dugme.setAttribute("aria-pressed", durum.isaretli ? "true" : "false");

                const hafta = satir.querySelector("[data-hafta]");
                hafta.textContent = durum.haftadaYapilan + "/" + durum.haftalikHedef;
                hafta.classList.toggle("tamam", durum.haftadaYapilan >= durum.haftalikHedef);

                const seri = satir.querySelector("[data-seri]");
                seri.title = "En uzun seri: " + durum.enUzunSeri + " gün";
                seri.querySelector("span").textContent = durum.seri;
                seri.querySelector(".visually-hidden").textContent = "günlük seri, en uzun seri " + durum.enUzunSeri + " gün";
            } catch (hata) {
                hataGoster(hata.message);
            } finally {
                dugme.disabled = false;
            }
        });
    }

    // Yeni alışkanlık formu sayfanın üstünde açılır
    function formuBagla(bolge) {
        const form = bolge.querySelector("form");
        const adAlani = form.querySelector('[name="Ad"]');
        const hedefAlani = form.querySelector('[name="HaftalikHedef"]');

        function formuAc() {
            adAlani.value = "";
            hedefAlani.value = 5;
            form.querySelectorAll('[name="Renk"]').forEach(function (r, i) { r.checked = i === 1; });

            // Önceki denemeden kalan doğrulama mesajları temizlenir
            if (window.jQuery && jQuery(form).data("validator")) jQuery(form).validate().resetForm();
            form.querySelectorAll(".input-validation-error").forEach((a) => a.classList.remove("input-validation-error"));

            bolge.hidden = false;
            bolge.scrollIntoView({ behavior: "smooth", block: "nearest" });
            adAlani.focus();
        }

        function formuKapat() {
            bolge.hidden = true;
        }

        document.querySelectorAll('[data-form-ac="yeni"]').forEach(function (baglanti) {
            baglanti.addEventListener("click", function (e) {
                e.preventDefault();
                formuAc();
            });
        });
        bolge.querySelector("[data-form-kapat]").addEventListener("click", formuKapat);
        bolge.addEventListener("keydown", function (e) {
            if (e.key === "Escape") formuKapat();
        });
    }

    // Var olan alışkanlık kendi satırında düzenlenir: ad hücresi metin kutusu ve renk seçimine,
    // hafta hücresi hedef kutusuna, kalem düğmesi Kaydet'e, çöp kutusu Vazgeç'e dönüşür.
    function satirDuzenlemeyiBagla(izgara, renkSinifi) {
        // Renk seçenekleri üstteki yeni alışkanlık formundan alınır (aynı 7 renk)
        const renkler = Array.from(document.querySelectorAll("#alForm .renk-secenek"), (etiket) => ({
            kod: etiket.querySelector("input").value,
            sinif: etiket.querySelector("span").className,
            ad: etiket.title
        }));

        let acik = null;

        function dugme(sinif, ikon, etiket) {
            const d = document.createElement("button");
            d.type = "button";
            d.className = "al-islem " + sinif;
            d.title = etiket;
            d.setAttribute("aria-label", etiket);
            d.innerHTML = '<i class="bi ' + ikon + '" aria-hidden="true"></i>';
            return d;
        }

        function ac(baglanti) {
            if (acik) kapat();

            const satir = baglanti.closest("tr");
            const adHucre = satir.querySelector("th.al-ad-sutun");
            const haftaHucre = satir.querySelector("[data-hafta]");
            const islemler = satir.querySelector(".al-islemler");
            acik = {
                satir, baglanti, adHucre, haftaHucre, islemler,
                adHtml: adHucre.innerHTML,
                haftaHtml: haftaHucre.innerHTML,
                haftaTamam: haftaHucre.classList.contains("tamam"),
                eskiSinif: renkSinifi(satir),
                renk: baglanti.dataset.renk,
                yapilan: parseInt(haftaHucre.textContent, 10) || 0
            };
            satir.classList.add("duzenleniyor");

            // Ad ve renk
            const ad = document.createElement("input");
            ad.className = "girdi al-duzen-ad";
            ad.maxLength = 80;
            ad.value = baglanti.dataset.ad;
            ad.setAttribute("aria-label", "Alışkanlık adı");

            const nokta = adHucre.querySelector(".al-nokta");
            adHucre.querySelector("strong").replaceWith(ad);

            // 7 gün kutusu, aynı boyut ve konumda 7 renk seçeneğine dönüşür
            const gunHucreleri = Array.from(satir.children).slice(1, 8);
            const secenekler = renkler.map(function (r, i) {
                const secenek = document.createElement("button");
                secenek.type = "button";
                secenek.className = "al-duzen-renk " + r.sinif;
                secenek.title = r.ad;
                secenek.setAttribute("role", "radio");
                secenek.setAttribute("aria-label", "Renk: " + r.ad);
                secenek.setAttribute("aria-checked", r.kod === acik.renk ? "true" : "false");
                secenek.addEventListener("click", function () {
                    acik.renk = r.kod;
                    secenekler.forEach((x) => x.setAttribute("aria-checked", x === secenek ? "true" : "false"));
                    // Soldaki nokta seçilen rengi hemen gösterir
                    nokta.className = "al-nokta " + r.sinif;
                });
                gunHucreleri[i].appendChild(secenek);
                return secenek;
            });

            // Haftalık hedef
            const hedef = document.createElement("input");
            hedef.type = "number";
            hedef.min = 1;
            hedef.max = 7;
            hedef.inputMode = "numeric";
            hedef.className = "girdi al-duzen-hedef";
            hedef.value = baglanti.dataset.hedef;
            hedef.setAttribute("aria-label", "Haftalık hedef (gün)");
            haftaHucre.classList.remove("tamam");
            haftaHucre.replaceChildren(acik.yapilan + "/", hedef);

            // Kalem → Kaydet, çöp kutusu → Vazgeç
            islemler.querySelectorAll(".al-islem").forEach((x) => { x.hidden = true; });
            const kaydet = dugme("kaydet", "bi-check-lg", "Kaydet");
            const vazgec = dugme("", "bi-x-lg", "Vazgeç");
            kaydet.addEventListener("click", kaydetTikla);
            vazgec.addEventListener("click", function () { kapat(); baglanti.focus(); });
            islemler.append(kaydet, vazgec);
            Object.assign(acik, { ad, hedef, kaydet, vazgec, secenekler });

            // Enter kaydeder, Escape vazgeçer (Enter ızgara formunu göndermesin diye engellenir)
            [ad, hedef].forEach(function (alan) {
                alan.addEventListener("keydown", function (e) {
                    if (e.key === "Enter") { e.preventDefault(); kaydetTikla(); }
                    if (e.key === "Escape") { e.preventDefault(); kapat(); baglanti.focus(); }
                });
            });

            ad.focus();
            ad.select();
        }

        // Düzenleme modundan çıkar; kaydedilmediyse satır eski hâline döner
        function kapat() {
            if (!acik) return;
            acik.adHucre.innerHTML = acik.adHtml;
            acik.haftaHucre.innerHTML = acik.haftaHtml;
            acik.haftaHucre.classList.toggle("tamam", acik.haftaTamam);
            acik.kaydet.remove();
            acik.vazgec.remove();
            acik.secenekler.forEach((x) => x.remove());
            acik.islemler.querySelectorAll(".al-islem").forEach((x) => { x.hidden = false; });
            acik.satir.classList.remove("duzenleniyor");
            acik = null;
        }

        async function kaydetTikla() {
            const d = acik;
            if (!d || d.kaydet.disabled) return;
            const ad = d.ad.value.trim();
            const hedef = parseInt(d.hedef.value, 10);
            const hedefGecerli = hedef >= 1 && hedef <= 7;

            d.ad.classList.toggle("input-validation-error", !ad);
            d.hedef.classList.toggle("input-validation-error", !hedefGecerli);
            if (!ad) { hataGoster("Alışkanlık adı zorunludur."); d.ad.focus(); return; }
            if (!hedefGecerli) { hataGoster("Haftalık hedef 1 ile 7 gün arasında olmalı."); d.hedef.focus(); return; }

            d.kaydet.disabled = true;
            try {
                const sonuc = await gonder(d.baglanti.href, izgara, { Ad: ad, HaftalikHedef: hedef, Renk: d.renk });

                // Ad hücresi: renk noktası ve yeni ad
                const nokta = document.createElement("span");
                nokta.className = "al-nokta " + sonuc.renkSinifi;
                const yazi = document.createElement("strong");
                yazi.textContent = sonuc.ad;
                d.adHucre.replaceChildren(nokta, yazi);
                d.adHtml = d.adHucre.innerHTML;

                // Hafta hücresi: yapılan / yeni hedef
                d.haftaHtml = d.yapilan + "/" + sonuc.haftalikHedef;
                d.haftaTamam = d.yapilan >= sonuc.haftalikHedef;

                // İşaretli kutular yeni renge geçer, ekran okuyucu etiketleri yeni adı kullanır
                d.satir.querySelectorAll("button.al-kutu").forEach(function (kutu) {
                    if (kutu.classList.contains("isaretli")) {
                        if (d.eskiSinif) kutu.classList.remove(d.eskiSinif);
                        kutu.classList.add(sonuc.renkSinifi);
                    }
                    const etiket = kutu.getAttribute("aria-label");
                    kutu.setAttribute("aria-label", sonuc.ad + etiket.slice(etiket.lastIndexOf(" – ")));
                });

                Object.assign(d.baglanti.dataset, { ad: sonuc.ad, hedef: sonuc.haftalikHedef, renk: sonuc.renkKodu });
                d.baglanti.setAttribute("aria-label", "Düzenle: " + sonuc.ad);
                const sil = d.islemler.querySelector("[data-sil]");
                sil.dataset.sil = sonuc.ad;
                sil.setAttribute("aria-label", "Sil: " + sonuc.ad);

                kapat();
                d.baglanti.focus();
            } catch (hata) {
                hataGoster(hata.message);
                d.kaydet.disabled = false;
            }
        }

        izgara.querySelectorAll("[data-duzenle]").forEach(function (baglanti) {
            baglanti.addEventListener("click", function (e) {
                e.preventDefault();
                ac(baglanti);
            });
        });
    }

    // ---------- Su takibi ----------

    function suTakibiSayfasi() {
        const sayfa = document.querySelector(".su-sayfa");
        if (!sayfa) return;

        const enFazla = parseInt(sayfa.dataset.enFazlaBardak, 10);
        const ogeler = {};
        sayfa.querySelectorAll("[data-su]").forEach(function (o) { ogeler[o.dataset.su] = o; });

        const DAMLA = '<svg width="22" height="22" viewBox="0 0 24 24" stroke-width="2" stroke-linejoin="round"%S><path d="M12 3c3 4 6 7.5 6 11a6 6 0 0 1-12 0c0-3.5 3-7 6-11z"></path></svg>';

        function guncelle(durum) {
            ogeler.bugunLitre.textContent = durum.bugunLitre;
            ogeler.hedefLitre.textContent = durum.hedefLitre;
            ogeler.yuzde.textContent = durum.yuzde;
            ogeler.dolgu.style.height = durum.yuzde + "%";
            ogeler.bardak.setAttribute("aria-label", "Bardak yüzde " + durum.yuzde + " dolu");
            ogeler.tebrik.hidden = !durum.hedefeUlasildi;
            ogeler.seri.textContent = durum.seri;
            ogeler.ekle.disabled = durum.bardak >= enFazla;
            ogeler.geriAl.disabled = durum.bardak === 0;

            let damlalar = "";
            for (let i = 0; i < Math.max(durum.hedefBardak, durum.bardak); i++) {
                damlalar += DAMLA.replace("%S", i < durum.bardak ? ' class="dolu"' : "");
            }
            ogeler.damlalar.innerHTML = damlalar;

            // Son 7 gün grafiği: bugünün çubuğu ve ölçek (en yüksek çubuk 130 piksel)
            const bugun = ogeler.bugunCubuk;
            bugun.dataset.ml = durum.bugunMl;
            bugun.title = bugun.querySelector("small").textContent + ": " + durum.bugunMl + " ml";
            const sutunlar = sayfa.querySelectorAll(".su-cubuk-sutun");
            const enYuksek = Math.max(durum.hedefMl, ...Array.from(sutunlar, (s) => +s.dataset.ml));
            sutunlar.forEach(function (s) {
                const ml = +s.dataset.ml;
                const cubuk = s.querySelector(".su-cubuk");
                cubuk.style.height = (enYuksek ? Math.round(ml * 130 / enYuksek) : 0) + "px";
                cubuk.classList.toggle("tamam", durum.hedefMl > 0 && ml >= durum.hedefMl);
            });
        }

        sayfa.querySelectorAll("form[data-su-islem]").forEach(function (form) {
            form.addEventListener("submit", async function (e) {
                e.preventDefault();
                const dugme = form.querySelector("button");
                dugme.disabled = true;
                try {
                    guncelle(await gonder(form.action, form));
                } catch (hata) {
                    hataGoster(hata.message);
                    dugme.disabled = false;
                }
            });
        });
    }
});
