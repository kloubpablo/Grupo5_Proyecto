// Yampi BarberShop - landing publica (sin dependencias externas)
(() => {
    'use strict';

    const landing = document.getElementById('ypLanding');
    const menu = document.getElementById('ypMainMenu');
    const menuToggle = document.getElementById('ypMenuToggle');
    const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
    const motion = () => reducedMotion.matches ? 'auto' : 'smooth';

    // -------- 1. Navegacion publica y anclas suaves --------
    const cerrarMenu = () => {
        menu?.classList.remove('is-open');
        menuToggle?.setAttribute('aria-expanded', 'false');
        menuToggle?.setAttribute('aria-label', 'Abrir menú');
    };

    menuToggle?.addEventListener('click', () => {
        if (!menu) return;
        const abierto = menu.classList.toggle('is-open');
        menuToggle.setAttribute('aria-expanded', String(abierto));
        menuToggle.setAttribute('aria-label', abierto ? 'Cerrar menú' : 'Abrir menú');
    });

    document.addEventListener('click', (event) => {
        const enlace = event.target.closest?.('a[href*="#"]');
        if (!enlace) {
            if (menu?.classList.contains('is-open') &&
                !menu.contains(event.target) && !menuToggle?.contains(event.target)) cerrarMenu();
            return;
        }

        const url = new URL(enlace.href, window.location.href);
        if (url.origin !== window.location.origin || !url.hash) return;
        let id;
        try { id = decodeURIComponent(url.hash.slice(1)); }
        catch { return; }
        const destino = document.getElementById(id);
        if (!destino) return; // En otras vistas, dejar navegar al Index#ancla

        event.preventDefault();
        cerrarMenu();
        destino.scrollIntoView({ behavior: motion(), block: 'start' });
        history.replaceState(null, '', window.location.pathname + window.location.search + '#' + id);
    });

    document.addEventListener('keydown', (event) => {
        if (event.key === 'Escape') cerrarMenu();
    });

    if (!landing) return;
    landing.classList.add('yp-js');

    const secciones = [...document.querySelectorAll('#ypLanding .yp-anchor')];
    const links = [...document.querySelectorAll('.yp-nav-link[data-yp-section]')];
    if ('IntersectionObserver' in window) {
        const observadorMenu = new IntersectionObserver((entries) => {
            entries.forEach((entry) => {
                if (!entry.isIntersecting) return;
                links.forEach((link) => {
                    const activo = link.dataset.ypSection === entry.target.id;
                    link.classList.toggle('is-active', activo);
                    if (activo) link.setAttribute('aria-current', 'location');
                    else link.removeAttribute('aria-current');
                });
            });
        }, { rootMargin: '-20% 0px -65% 0px', threshold: 0 });
        secciones.forEach((section) => observadorMenu.observe(section));
    }

    // La pagina puede abrirse directamente con #galeria, #contacto, etc.
    if (window.location.hash) {
        try {
            const destino = document.getElementById(decodeURIComponent(window.location.hash.slice(1)));
            if (destino) requestAnimationFrame(() => destino.scrollIntoView({ behavior: 'auto', block: 'start' }));
        } catch { /* ignorar hash invalido */ }
    }

    // -------- 2. Hero: fotografias rotativas cada 7 segundos --------
    const hero = document.getElementById('inicio');
    const slides = [...document.querySelectorAll('.yp-hero-slide')];
    const dots = [...document.querySelectorAll('[data-hero-dot]')];
    const heroCounter = document.getElementById('ypHeroCounter');
    const heroNext = document.getElementById('ypHeroNext');
    let heroIndex = 0;
    let heroInterval = null;
    let heroPausado = false;

    const cambiarHero = (index) => {
        if (!slides.length) return;
        heroIndex = (index + slides.length) % slides.length;
        slides.forEach((slide, i) => slide.classList.toggle('is-active', i === heroIndex));
        dots.forEach((dot, i) => {
            dot.classList.toggle('is-active', i === heroIndex);
            if (i === heroIndex) dot.setAttribute('aria-current', 'true');
            else dot.removeAttribute('aria-current');
        });
        if (heroCounter) heroCounter.textContent =
            String(heroIndex + 1).padStart(2, '0') + ' / ' + String(slides.length).padStart(2, '0');
    };
    const pausarHero = () => { if (heroInterval) clearInterval(heroInterval); heroInterval = null; };
    const iniciarHero = () => {
        pausarHero();
        if (slides.length <= 1 || reducedMotion.matches || document.hidden || heroPausado) return;
        heroInterval = window.setInterval(() => cambiarHero(heroIndex + 1), 7000);
    };
    heroNext?.addEventListener('click', () => { cambiarHero(heroIndex + 1); iniciarHero(); });
    dots.forEach((dot) => dot.addEventListener('click', () => {
        cambiarHero(Number(dot.dataset.heroDot)); iniciarHero();
    }));
    hero?.addEventListener('focusin', () => { heroPausado = true; pausarHero(); });
    hero?.addEventListener('focusout', (e) => {
        if (hero.contains(e.relatedTarget)) return;
        heroPausado = false; iniciarHero();
    });
    document.addEventListener('visibilitychange', iniciarHero);
    if (reducedMotion.addEventListener) reducedMotion.addEventListener('change', iniciarHero);
    cambiarHero(0); iniciarHero();

    // -------- 3. Servicios: expandir si hay mas de 6 --------
    const servicesGrid = document.getElementById('ypServicesGrid');
    const moreServices = document.getElementById('ypMoreServices');
    moreServices?.addEventListener('click', () => {
        const expandido = servicesGrid?.classList.toggle('is-expanded') || false;
        moreServices.setAttribute('aria-expanded', String(expandido));
        moreServices.innerHTML = expandido ? 'Mostrar menos <span>↑</span>' : 'Ver todos los servicios <span>↓</span>';
        if (expandido) servicesGrid?.querySelectorAll('.yp-service-extra').forEach(c => c.classList.add('is-visible'));
    });

    // -------- 4. Galeria de paginas de 4 fotos --------
    const paginas = [...document.querySelectorAll('[data-gallery-page]')];
    const anterior = document.getElementById('ypGalleryPrev');
    const siguiente = document.getElementById('ypGalleryNext');
    const paginacion = document.getElementById('ypGalleryPageNumber');
    let paginaActual = 0;

    const mostrarPagina = (numero) => {
        if (!paginas.length) return;
        paginaActual = Math.max(0, Math.min(numero, paginas.length - 1));
        paginas.forEach((pagina, index) => {
            const activa = index === paginaActual;
            pagina.classList.toggle('is-active', activa);
            pagina.setAttribute('aria-hidden', String(!activa));
        });
        if (anterior) anterior.disabled = paginaActual === 0;
        if (siguiente) siguiente.disabled = paginaActual === paginas.length - 1;
        if (paginacion) paginacion.textContent =
            String(paginaActual + 1).padStart(2, '0') + ' / ' + String(paginas.length).padStart(2, '0');
    };
    anterior?.addEventListener('click', () => mostrarPagina(paginaActual - 1));
    siguiente?.addEventListener('click', () => mostrarPagina(paginaActual + 1));
    mostrarPagina(0);

    // -------- 5. Visor fotografico accesible --------
    const cards = [...document.querySelectorAll('[data-gallery-full]')];
    const lightbox = document.getElementById('ypLightbox');
    const bigImage = document.getElementById('ypLightboxImage');
    const caption = document.getElementById('ypLightboxCaption');
    const closeButton = document.getElementById('ypLightboxClose');
    const prevImage = document.getElementById('ypLightboxPrev');
    const nextImage = document.getElementById('ypLightboxNext');
    let imageIndex = 0;
    let previousFocus = null;

    const mostrarFoto = (numero) => {
        if (!cards.length || !bigImage) return;
        imageIndex = (numero + cards.length) % cards.length;
        bigImage.src = cards[imageIndex].dataset.galleryFull;
        bigImage.alt = 'Trabajo de Yampi BarberShop ' + (imageIndex + 1);
        if (caption) caption.textContent = 'TRABAJO ' + String(imageIndex + 1).padStart(2, '0')
            + ' / ' + String(cards.length).padStart(2, '0');
    };
    const abrirVisor = (numero) => {
        if (!lightbox) return;
        previousFocus = document.activeElement;
        mostrarFoto(numero);
        lightbox.hidden = false;
        document.body.classList.add('yp-modal-open');
        closeButton?.focus();
    };
    const cerrarVisor = () => {
        if (!lightbox) return;
        lightbox.hidden = true;
        document.body.classList.remove('yp-modal-open');
        if (bigImage) bigImage.removeAttribute('src');
        if (previousFocus instanceof HTMLElement) previousFocus.focus();
    };
    cards.forEach((card, index) => card.addEventListener('click', () => abrirVisor(index)));
    closeButton?.addEventListener('click', cerrarVisor);
    prevImage?.addEventListener('click', () => mostrarFoto(imageIndex - 1));
    nextImage?.addEventListener('click', () => mostrarFoto(imageIndex + 1));
    lightbox?.addEventListener('click', (event) => { if (event.target === lightbox) cerrarVisor(); });
    document.addEventListener('keydown', (event) => {
        if (!lightbox || lightbox.hidden) return;
        if (event.key === 'Escape') { cerrarVisor(); return; }
        if (event.key === 'ArrowLeft') { mostrarFoto(imageIndex - 1); return; }
        if (event.key === 'ArrowRight') { mostrarFoto(imageIndex + 1); return; }
        if (event.key === 'Tab') {
            const botones = [closeButton, prevImage, nextImage].filter(Boolean);
            const index = botones.indexOf(document.activeElement);
            if (event.shiftKey && index <= 0) { event.preventDefault(); botones[botones.length - 1].focus(); }
            if (!event.shiftKey && index === botones.length - 1) { event.preventDefault(); botones[0].focus(); }
        }
    });

    // -------- 6. Animaciones discretas al bajar --------
    const revelar = [...document.querySelectorAll('.yp-reveal')];
    if ('IntersectionObserver' in window && !reducedMotion.matches) {
        const observador = new IntersectionObserver((entries, observer) => {
            entries.forEach((entry) => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('is-visible');
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.07, rootMargin: '0px 0px 30px 0px' });
        revelar.forEach(elemento => observador.observe(elemento));
    } else {
        revelar.forEach(elemento => elemento.classList.add('is-visible'));
    }
})();
