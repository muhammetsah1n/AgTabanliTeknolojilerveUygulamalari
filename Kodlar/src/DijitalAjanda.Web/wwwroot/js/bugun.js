document.addEventListener("DOMContentLoaded", function () {
    const sayfa = document.querySelector(".bugun-sayfa");
    if (!sayfa) return;

    const AYLAR = ["Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık"];
    const AYK = ["Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara"];
    const GUNK = ["Pzt", "Sal", "Çar", "Per", "Cum", "Cmt", "Paz"];
    const TAKVIM_GUNLERI = ["Pt", "Sa", "Ça", "Pe", "Cu", "Ct", "Pz"];

    const iki = (n) => (n < 10 ? "0" : "") + n;
    const isoYap = (d) => d.getFullYear() + "-" + iki(d.getMonth() + 1) + "-" + iki(d.getDate());
    const coz = (t) => { const p = t.split("-"); return new Date(+p[0], +p[1] - 1, +p[2]); };
    const haftaGunu = (t) => (coz(t).getDay() + 6) % 7;
    const kisaTarih = (t) => { const d = coz(t); return d.getDate() + " " + AYK[d.getMonth()] + ", " + GUNK[haftaGunu(t)]; };
    const ayKaydir = (t, n) => { const d = coz(t); return isoYap(new Date(d.getFullYear(), d.getMonth() + n, 1)); };

    const adres = JSON.parse(sayfa.dataset.adresler);
    const konum = { Gorunum: sayfa.dataset.gorunum, Odak: sayfa.dataset.odak };
    const gunGorunumu = konum.Gorunum === "Gun";
    const bugun = sayfa.dataset.bugun;

    // ---------- Ortak yardımcılar ----------

    const uyari = document.getElementById("bugunUyari");
    let uyariZamani;
    function bildir(mesaj) {
        uyari.textContent = mesaj;
        uyari.hidden = false;
        clearTimeout(uyariZamani);
        uyariZamani = setTimeout(function () { uyari.hidden = true; }, 6000);
    }

    // Sayfayı yenilemeden sunucuya kayıt gönderir; hata olursa sunucunun mesajını fırlatır
    async function gonder(url, alanlar) {
        const anahtar = document.querySelector('meta[name="istek-dogrulama"]');
        const yanit = await fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded",
                "X-Requested-With": "XMLHttpRequest",
                "RequestVerificationToken": anahtar ? anahtar.content : ""
            },
            body: new URLSearchParams(Object.assign({}, konum, alanlar))
        });
        if (!yanit.ok) {
            let mesaj = "Kaydedilemedi, lütfen tekrar deneyin.";
            try { mesaj = (await yanit.json()).hata || mesaj; } catch (e) { }
            throw new Error(mesaj);
        }
        try { return await yanit.json(); } catch (e) { return {}; }
    }

    function imleciSonaAl(alan) {
        const uzunluk = alan.value.length;
        try { alan.setSelectionRange(uzunluk, uzunluk); } catch (e) { }
    }

    // Etkinliklerde metnin başındaki saat ayrılır: "14:30 Toplantı" → { saat: "14:30", metin: "Toplantı" }
    function saatiAyir(yazi) {
        const eslesme = /^([01]?\d|2[0-3])[:.]([0-5]\d)\s+(.+)$/s.exec(yazi.trim());
        if (!eslesme) return { saat: null, metin: yazi.trim() };
        return { saat: iki(+eslesme[1]) + ":" + eslesme[2], metin: eslesme[3].trim() };
    }

    // Form gönderilip sayfa yenilenince aynı kaydırma konumuna dönülür
    const kaydirmaAnahtari = "bugun-kaydirma";
    try {
        const kayitli = sessionStorage.getItem(kaydirmaAnahtari);
        if (kayitli !== null) {
            window.scrollTo(0, parseInt(kayitli, 10));
            sessionStorage.removeItem(kaydirmaAnahtari);
        }
    } catch (e) { }
    sayfa.addEventListener("submit", function () {
        try { sessionStorage.setItem(kaydirmaAnahtari, String(window.scrollY)); } catch (e) { }
    });

    // ---------- Gün: Notlar yazdıkça kaydedilir ----------

    const notSutunu = sayfa.querySelector("[data-notlar]");
    if (notSutunu) {
        const liste = notSutunu.querySelector(".not-listesi");
        const sablon = document.getElementById("notSablonu");
        const sayac = notSutunu.querySelector("[data-not-sayisi]");

        const notlariDuzenle = function () {
            const kartlar = liste.querySelectorAll(".not-kart");
            sayac.textContent = liste.querySelectorAll(".not-kart[data-id]").length + " madde";
            liste.classList.toggle("tek", kartlar.length < 2);
            kartlar.forEach(function (kart, i) {
                const alan = kart.querySelector("textarea");
                alan.setAttribute("aria-label", "Not " + (i + 1));
                alan.placeholder = i === 0 ? "Yazmaya başla…" : "Yeni madde…";
                kart.querySelector("[data-not-sil]").setAttribute("aria-label", "Not " + (i + 1) + " sil");
            });
        };

        const yeniKart = function (odakla) {
            const kart = sablon.content.firstElementChild.cloneNode(true);
            liste.appendChild(kart);
            notlariDuzenle();
            if (odakla) kart.querySelector("textarea").focus();
        };

        const kartiKaldir = function (kart) {
            kart.remove();
            if (!liste.querySelector(".not-kart")) yeniKart(false);
            notlariDuzenle();
        };

        // Aynı kartın kayıtları sırayla gönderilir; ilk kayıt bitmeden ikinci not oluşturulmaz
        const kaydet = async function (kart) {
            const alan = kart.querySelector("textarea");
            const metin = alan.value.trim();
            if (kart.dataset.id) {
                await gonder(adres.notGuncelle, { Id: kart.dataset.id, Metin: alan.value });
                if (!metin) kartiKaldir(kart);
            } else if (metin) {
                const sonuc = await gonder(adres.notEkle, { Tarih: notSutunu.dataset.tarih, Metin: alan.value });
                kart.dataset.id = sonuc.id;
            }
            notlariDuzenle();
        };

        liste.addEventListener("change", function (e) {
            const kart = e.target.closest(".not-kart");
            if (!kart) return;
            kart.siradaki = (kart.siradaki || Promise.resolve())
                .then(function () { return kaydet(kart); })
                .catch(function (h) { bildir(h.message); });
        });

        liste.addEventListener("click", async function (e) {
            const dugme = e.target.closest("[data-not-sil]");
            if (!dugme) return;
            const kart = dugme.closest(".not-kart");
            if (kart.dataset.id) {
                try { await gonder(adres.notSil, { Id: kart.dataset.id }); }
                catch (h) { bildir(h.message); return; }
            }
            kartiKaldir(kart);
        });

        liste.addEventListener("focusin", function (e) {
            if (e.target.matches("textarea")) imleciSonaAl(e.target);
        });

        notSutunu.querySelector("[data-not-ekle]").addEventListener("click", function () { yeniKart(true); });
        notlariDuzenle();
    }

    // ---------- Gün: Etkinlikler sütunundaki yuvarlak saat kadranı ----------

    const saatPenceresi = sayfa.querySelector(".saat-penceresi");
    if (saatPenceresi) {
        const saatDugmesi = sayfa.querySelector("[data-saat-ac]");
        const saatAnahtari = "bugun-etkinlik-saati";
        const secilenSaat = { saat: +saatPenceresi.dataset.saat, dakika: +saatPenceresi.dataset.dakika };
        try {
            const kayitli = JSON.parse(sessionStorage.getItem(saatAnahtari));
            if (kayitli && kayitli.saat >= 0 && kayitli.saat < 24) Object.assign(secilenSaat, kayitli);
        } catch (e) { }

        const isaretle = function (dugme, secili) {
            dugme.classList.toggle("secili", secili);
            dugme.setAttribute("aria-pressed", secili ? "true" : "false");
        };

        const saatiCiz = function () {
            const s = secilenSaat.saat, d = secilenSaat.dakika;
            const metin = iki(s) + ":" + iki(d);
            const saat12 = s % 12 === 0 ? 12 : s % 12;

            sayfa.querySelectorAll("[data-saat-metni]").forEach(function (el) { el.textContent = metin; });
            sayfa.querySelectorAll("[data-saat-girdi]").forEach(function (el) { el.value = metin; });
            saatPenceresi.querySelector("[data-akrep]").style.transform = "rotate(" + ((s % 12) * 30 + d * 0.5) + "deg)";
            saatPenceresi.querySelector("[data-yelkovan]").style.transform = "rotate(" + (d * 6) + "deg)";
            saatPenceresi.querySelectorAll("[data-saat-n]").forEach(function (b) { isaretle(b, +b.dataset.saatN === saat12); });
            saatPenceresi.querySelectorAll("[data-yarim]").forEach(function (b) { isaretle(b, (+b.dataset.yarim === 12) === (s >= 12)); });
            saatPenceresi.querySelectorAll("[data-dakika]").forEach(function (b) { isaretle(b, +b.dataset.dakika === d); });
            saatDugmesi.setAttribute("aria-label", "Saat seç, seçili " + metin);
            try { sessionStorage.setItem(saatAnahtari, JSON.stringify(secilenSaat)); } catch (e) { }
        };

        const saatPenceresiAc = function (ac) {
            saatPenceresi.hidden = !ac;
            saatDugmesi.setAttribute("aria-expanded", ac ? "true" : "false");
            if (ac) {
                const secili = saatPenceresi.querySelector(".kadran-saat.secili");
                if (secili) secili.focus({ preventScroll: true });
                saatPenceresi.scrollIntoView({ block: "nearest", behavior: "smooth" });
            }
        };

        saatDugmesi.addEventListener("click", function (e) {
            e.stopPropagation();
            saatPenceresiAc(saatPenceresi.hidden);
        });

        saatPenceresi.addEventListener("click", function (e) {
            e.stopPropagation();
            const b = e.target.closest("button");
            if (!b) return;
            if (b.dataset.saatN) {
                secilenSaat.saat = (+b.dataset.saatN % 12) + (secilenSaat.saat >= 12 ? 12 : 0);
            } else if (b.dataset.yarim) {
                secilenSaat.saat = (secilenSaat.saat % 12) + +b.dataset.yarim;
            } else if (b.dataset.dakika) {
                secilenSaat.dakika = +b.dataset.dakika;
            } else if (b.hasAttribute("data-saat-kapat")) {
                saatPenceresiAc(false);
                saatDugmesi.focus();
                return;
            }
            saatiCiz();
        });

        document.addEventListener("click", function () {
            if (!saatPenceresi.hidden) saatPenceresiAc(false);
        });
        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape" && !saatPenceresi.hidden) {
                saatPenceresiAc(false);
                saatDugmesi.focus();
            }
        });

        saatiCiz();
    }

    // ---------- Takvimler: sağdaki kart ve etkinlik sütunundaki açılır seçici ----------

    const takvimKarti = sayfa.querySelector("[data-takvim]");
    const ilkAy = JSON.parse(takvimKarti.dataset.takvim);
    const aylar = {};
    aylar[ilkAy.ay] = ilkAy;
    const secim = { tarih: konum.Odak, ay: ilkAy.ay };
    const tarihSecici = sayfa.querySelector(".tarih-secici");

    const ayGetir = async function (ay) {
        if (aylar[ay]) return aylar[ay];
        const yanit = await fetch(adres.takvim + "?ay=" + ay, { headers: { "X-Requested-With": "XMLHttpRequest" } });
        if (!yanit.ok) throw new Error("Takvim yüklenemedi.");
        aylar[ay] = await yanit.json();
        return aylar[ay];
    };

    const takvimiCiz = function () {
        const veri = aylar[secim.ay] || { etkinlikGunleri: [], tatiller: [] };
        const ayBasi = coz(secim.ay);
        const ilkGun = new Date(ayBasi);
        ilkGun.setDate(1 - haftaGunu(secim.ay));
        const gunSayisi = new Date(ayBasi.getFullYear(), ayBasi.getMonth() + 1, 0).getDate();
        const hucreSayisi = Math.ceil((haftaGunu(secim.ay) + gunSayisi) / 7) * 7;

        const tatiller = {};
        veri.tatiller.forEach(function (t) { tatiller[t.tarih] = t.ad; });
        const etkinlikGunleri = new Set(veri.etkinlikGunleri);

        sayfa.querySelectorAll("[data-takvim-baslik]").forEach(function (el) {
            el.textContent = AYLAR[ayBasi.getMonth()] + " " + ayBasi.getFullYear();
        });

        sayfa.querySelectorAll("[data-takvim-izgara]").forEach(function (izgara) {
            const parca = document.createDocumentFragment();
            TAKVIM_GUNLERI.forEach(function (ad) {
                const el = document.createElement("div");
                el.className = "ad";
                el.textContent = ad;
                parca.appendChild(el);
            });
            for (let i = 0; i < hucreSayisi; i++) {
                const gun = new Date(ilkGun);
                gun.setDate(ilkGun.getDate() + i);
                const t = isoYap(gun);
                const dugme = document.createElement("button");
                dugme.type = "button";
                dugme.dataset.tarih = t;
                dugme.textContent = gun.getDate();
                if (gun.getMonth() !== ayBasi.getMonth()) dugme.classList.add("diger-ay");
                if (tatiller[t]) dugme.classList.add("tatil");
                if (t === bugun) dugme.classList.add("bugun-gun");
                if (t === secim.tarih) dugme.classList.add("secili");
                const ipucu = (tatiller[t] ? tatiller[t] + " · " : "") + gun.getDate() + " " + AYLAR[gun.getMonth()] + (gunGorunumu ? " seç" : " aç");
                dugme.title = ipucu;
                dugme.setAttribute("aria-label", ipucu);
                dugme.setAttribute("aria-pressed", t === secim.tarih ? "true" : "false");
                if (etkinlikGunleri.has(t)) {
                    const nokta = document.createElement("span");
                    nokta.className = "nokta";
                    dugme.appendChild(nokta);
                }
                parca.appendChild(dugme);
            }
            izgara.replaceChildren(parca);
        });

        const tatilListesi = sayfa.querySelector("[data-tatil-listesi]");
        tatilListesi.replaceChildren.apply(tatilListesi, veri.tatiller
            .filter(function (t) { return coz(t.tarih).getMonth() === ayBasi.getMonth(); })
            .map(function (t) {
                const d = coz(t.tarih);
                const li = document.createElement("li");
                const gun = document.createElement("strong");
                gun.textContent = d.getDate() + " " + AYK[d.getMonth()];
                li.append(gun, t.ad);
                return li;
            }));

        const metin = kisaTarih(secim.tarih);
        sayfa.querySelectorAll("[data-tarih-metni]").forEach(function (el) { el.textContent = metin; });
        sayfa.querySelectorAll("[data-tarih-girdi]").forEach(function (el) { el.value = secim.tarih; });
        sayfa.querySelectorAll("[data-tarih-ac]").forEach(function (el) { el.setAttribute("aria-label", "Tarih seç, seçili " + metin); });
    };

    const tarihSeciciAc = function (ac) {
        if (!tarihSecici) return;
        tarihSecici.hidden = !ac;
        sayfa.querySelectorAll("[data-tarih-ac]").forEach(function (b) { b.setAttribute("aria-expanded", ac ? "true" : "false"); });
    };

    sayfa.addEventListener("click", async function (e) {
        const ayDugmesi = e.target.closest("[data-takvim-onceki], [data-takvim-sonraki]");
        if (ayDugmesi) {
            const yeniAy = ayKaydir(secim.ay, ayDugmesi.hasAttribute("data-takvim-onceki") ? -1 : 1);
            try { await ayGetir(yeniAy); } catch (h) { bildir(h.message); }
            secim.ay = yeniAy;
            takvimiCiz();
            return;
        }
        const gun = e.target.closest("[data-takvim-izgara] button");
        if (gun) {
            // Gün sayfasında etkinlik tarihi seçilir; hafta ve ayda o tarih açılır
            if (!gunGorunumu) {
                location.href = adres.sayfa + "?gorunum=" + konum.Gorunum + "&tarih=" + gun.dataset.tarih;
                return;
            }
            secim.tarih = gun.dataset.tarih;
            tarihSeciciAc(false);
            takvimiCiz();
            return;
        }
        if (e.target.closest("[data-tarih-ac]")) {
            tarihSeciciAc(tarihSecici.hidden);
        }
    });

    takvimiCiz();

    // ---------- Hafta / ay kutuları: yerinde düzenleme, silme ve ekleme ----------

    const ekleAdresi = { Not: adres.notEkle, Yapilacak: adres.yapilacakEkle, Etkinlik: adres.etkinlikEkle };
    const guncellemeAdresi = { Not: adres.notGuncelle, Yapilacak: adres.yapilacakGuncelle, Etkinlik: adres.etkinlikGuncelle };
    const silmeAdresi = { Not: adres.notSil, Yapilacak: adres.yapilacakSil, Etkinlik: adres.etkinlikSil };
    const TURLER = {
        Yapilacak: { ad: "Yapılacak", sinif: "tur-yapilacak", isaret: "•", ipucu: "yapılacak yaz…" },
        Etkinlik: { ad: "Etkinlik", sinif: "tur-etkinlik", isaret: "○", ipucu: "14:30 etkinlik…" },
        Not: { ad: "Not", sinif: "tur-not", isaret: "–", ipucu: "not yaz…" }
    };
    const TUR_SIRASI = ["Yapilacak", "Etkinlik", "Not"];

    // Maddenin görünen metni; ay görünümünde "16:00 Toplantı" gibi kısa, haftada tam metin ve altta saat
    const maddeyiGoster = function (dugme) {
        const kisa = !!dugme.dataset.kisa;
        const saat = dugme.dataset.saat;
        const metin = dugme.dataset.metin;
        dugme.querySelector(".madde-metin").textContent = kisa ? (saat ? saat + " " : "") + metin.split("\n")[0] : metin;
        let alt = dugme.querySelector("small");
        if (!kisa && saat) {
            if (!alt) { alt = document.createElement("small"); dugme.appendChild(alt); }
            alt.textContent = saat;
        } else if (alt) {
            alt.remove();
        }
        const tur = TURLER[dugme.dataset.tur];
        dugme.setAttribute("aria-label", tur.ad + ", düzenle: " + metin);
        dugme.parentElement.querySelector("[data-madde-sil]").setAttribute("aria-label", "Sil: " + metin);
    };

    const maddeOlustur = function (tur, id, metin, saat, kisa) {
        const kap = document.createElement("div");
        kap.className = "madde-kap";
        const dugme = document.createElement("button");
        dugme.type = "button";
        dugme.className = "madde " + TURLER[tur].sinif;
        dugme.setAttribute("data-madde", "");
        dugme.dataset.tur = tur;
        dugme.dataset.id = id;
        dugme.dataset.metin = metin;
        dugme.dataset.saat = saat || "";
        if (kisa) dugme.dataset.kisa = "1";
        const yazi = document.createElement("span");
        yazi.className = "madde-metin";
        dugme.appendChild(yazi);
        const sil = document.createElement("button");
        sil.type = "button";
        sil.className = "madde-x";
        sil.setAttribute("data-madde-sil", "");
        sil.innerHTML = '<i class="bi bi-x-lg"></i>';
        kap.append(dugme, sil);
        maddeyiGoster(dugme);
        return kap;
    };

    const maddeyiSil = async function (dugme) {
        await gonder(silmeAdresi[dugme.dataset.tur], { Id: dugme.dataset.id });
        dugme.closest(".madde-kap").remove();
    };

    const maddeyiDuzenle = function (dugme) {
        const tur = dugme.dataset.tur;
        const notMu = tur === "Not";
        const kap = dugme.closest(".madde-kap");
        // Etkinlikte saat de düzenlenebilsin diye metnin başına yazılır
        const ilkYazi = tur === "Etkinlik" && dugme.dataset.saat ? dugme.dataset.saat + " " + dugme.dataset.metin : dugme.dataset.metin;

        const alan = document.createElement(notMu ? "textarea" : "input");
        alan.className = dugme.className.replace(/\bmadde\b/, "madde-duzen");
        alan.value = ilkYazi;
        alan.maxLength = notMu ? 1000 : 160;
        alan.setAttribute("aria-label", notMu ? "Notu düzenle" : "Maddeyi düzenle");
        if (notMu) alan.rows = dugme.closest(".ay-hucre") ? 3 : 4;

        kap.classList.add("duzenleniyor");
        dugme.hidden = true;
        dugme.after(alan);
        alan.focus();
        imleciSonaAl(alan);

        let kapandi = false;
        const kapat = async function (kaydedilsin) {
            if (kapandi) return;
            kapandi = true;
            const yazi = alan.value;
            alan.remove();
            dugme.hidden = false;
            kap.classList.remove("duzenleniyor");

            if (!kaydedilsin || yazi.trim() === ilkYazi.trim()) {
                dugme.focus();
                return;
            }
            try {
                if (!yazi.trim()) {
                    // Boş bırakılan madde silinir
                    await maddeyiSil(dugme);
                    return;
                }
                if (tur === "Etkinlik") {
                    const ayrik = saatiAyir(yazi);
                    await gonder(guncellemeAdresi.Etkinlik, { Id: dugme.dataset.id, Baslik: ayrik.metin, Saat: ayrik.saat || "" });
                    dugme.dataset.metin = ayrik.metin;
                    if (ayrik.saat) dugme.dataset.saat = ayrik.saat;
                } else if (notMu) {
                    await gonder(guncellemeAdresi.Not, { Id: dugme.dataset.id, Metin: yazi });
                    dugme.dataset.metin = yazi.trim();
                } else {
                    await gonder(guncellemeAdresi.Yapilacak, { Id: dugme.dataset.id, Baslik: yazi.trim() });
                    dugme.dataset.metin = yazi.trim();
                }
                maddeyiGoster(dugme);
            } catch (h) {
                bildir(h.message);
            }
        };

        alan.addEventListener("keydown", function (e) {
            if (e.key === "Escape") {
                e.preventDefault();
                kapat(false);
            } else if (e.key === "Enter" && (!notMu || e.ctrlKey || e.metaKey)) {
                e.preventDefault();
                kapat(true);
            }
        });
        alan.addEventListener("blur", function () { kapat(true); });
    };

    // Kutuya yeni madde: + düğmesi alanı açar, Enter ekler ve yeni madde için alan açık kalır
    const ekleAlaniniAc = function (ekle, ac) {
        const alan = ekle.querySelector(".hucre-ekle-alan");
        ekle.classList.toggle("acik", ac);
        alan.hidden = !ac;
        if (ac) {
            ekle.querySelector(".hucre-ekle-girdi").focus();
        } else {
            ekle.querySelector(".hucre-ekle-girdi").value = "";
        }
    };

    const turuDegistir = function (dugme) {
        const sonraki = TUR_SIRASI[(TUR_SIRASI.indexOf(dugme.dataset.tur) + 1) % TUR_SIRASI.length];
        const tur = TURLER[sonraki];
        dugme.dataset.tur = sonraki;
        dugme.className = "tur-sec " + tur.sinif;
        dugme.textContent = tur.isaret;
        dugme.setAttribute("aria-label", "Tür: " + tur.ad + " (değiştirmek için tıkla)");
        dugme.parentElement.querySelector(".hucre-ekle-girdi").placeholder = tur.ipucu;
    };

    const kutuyaEkle = async function (ekle) {
        const girdi = ekle.querySelector(".hucre-ekle-girdi");
        const tur = ekle.querySelector("[data-tur-sec]").dataset.tur;
        const yazi = girdi.value.trim();
        if (!yazi) return;
        const tarih = ekle.dataset.tarih;
        const kisa = !!ekle.closest(".ay-hucre");

        let veri, metin = yazi, saat = null;
        if (tur === "Etkinlik") {
            const ayrik = saatiAyir(yazi);
            metin = ayrik.metin;
            saat = ayrik.saat || "09:00";
            veri = { Tarih: tarih, Saat: saat, Baslik: metin };
        } else if (tur === "Yapilacak") {
            veri = { Tarih: tarih, Baslik: metin };
        } else {
            veri = { Tarih: tarih, Metin: metin };
        }

        girdi.disabled = true;
        try {
            const sonuc = await gonder(ekleAdresi[tur], veri);
            const liste = ekle.parentElement.querySelector(".hucre-maddeler");
            liste.appendChild(maddeOlustur(tur, sonuc.id, metin, saat, kisa));
            girdi.value = "";
        } catch (h) {
            bildir(h.message);
        } finally {
            girdi.disabled = false;
            girdi.focus();
        }
    };

    sayfa.addEventListener("click", async function (e) {
        const sil = e.target.closest("[data-madde-sil]");
        if (sil) {
            try { await maddeyiSil(sil.closest(".madde-kap").querySelector("[data-madde]")); }
            catch (h) { bildir(h.message); }
            return;
        }
        const madde = e.target.closest("button[data-madde]");
        if (madde) {
            maddeyiDuzenle(madde);
            return;
        }
        const ac = e.target.closest("[data-hucre-ekle-ac]");
        if (ac) {
            ekleAlaniniAc(ac.closest("[data-hucre-ekle]"), true);
            return;
        }
        const turSec = e.target.closest("[data-tur-sec]");
        if (turSec) {
            turuDegistir(turSec);
            turSec.parentElement.querySelector(".hucre-ekle-girdi").focus();
        }
    });

    sayfa.addEventListener("keydown", function (e) {
        const girdi = e.target.closest(".hucre-ekle-girdi");
        if (!girdi) return;
        const ekle = girdi.closest("[data-hucre-ekle]");
        if (e.key === "Enter") {
            e.preventDefault();
            kutuyaEkle(ekle);
        } else if (e.key === "Escape") {
            e.preventDefault();
            ekleAlaniniAc(ekle, false);
            ekle.querySelector("[data-hucre-ekle-ac]").focus();
        }
    });

    // Ekleme alanından boş olarak çıkılırsa (tür düğmesine geçiş hariç) alan kapanır
    sayfa.addEventListener("focusout", function (e) {
        const ekle = e.target.closest("[data-hucre-ekle]");
        if (!ekle || ekle.contains(e.relatedTarget)) return;
        const girdi = ekle.querySelector(".hucre-ekle-girdi");
        if (!girdi.disabled && !girdi.value.trim()) ekleAlaniniAc(ekle, false);
    });
});
