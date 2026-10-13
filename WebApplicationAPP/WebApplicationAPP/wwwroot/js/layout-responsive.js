console.log("YAMPI: layout.js cargado correctamente");

document.addEventListener("DOMContentLoaded", function () {

    // ==========================================
    // ELEMENTOS DEL MENU
    // ==========================================

    const nav = document.getElementById("nav");
    const toggle = document.getElementById("yampiMenuToggle");
    const menu = document.getElementById("yampiMenu");
    const backdrop = document.getElementById("yampiMenuBackdrop");

    if (!nav || !toggle || !menu || !backdrop) return;

    const mobile = window.matchMedia("(max-width: 1250px)");

    let previo = null;

    // ==========================================
    // CERRAR MENU
    // ==========================================

    function cerrar() {

        menu.classList.remove("yampi-menu-open");

        document.body.classList.remove("yampi-menu-open");

        toggle.setAttribute("aria-expanded", "false");
        toggle.setAttribute("aria-label", "Abrir menu");

        toggle.querySelector("i").className = "bi bi-list";

        backdrop.hidden = true;

        menu.inert = mobile.matches;

        if (previo && mobile.matches) {
            previo.focus();
        }
    }

    // ==========================================
    // ABRIR MENU
    // ==========================================

    function abrir() {

        previo = document.activeElement;

        menu.inert = false;

        menu.classList.add("yampi-menu-open");

        document.body.classList.add("yampi-menu-open");

        toggle.setAttribute("aria-expanded", "true");
        toggle.setAttribute("aria-label", "Cerrar menu");

        toggle.querySelector("i").className = "bi bi-x-lg";

        backdrop.hidden = false;

        const primero = menu.querySelector(
            "a:not(.dropdown-toggle)"
        );

        primero?.focus();
    }

    // ==========================================
    // BOTON HAMBURGUESA
    // ==========================================

    toggle.addEventListener("click", function () {

        if (menu.classList.contains("yampi-menu-open")) {
            cerrar();
        } else {
            abrir();
        }

    });

    // ==========================================
    // CERRAR AL TOCAR FUERA
    // ==========================================

    backdrop.addEventListener("click", cerrar);

    // ==========================================
    // CONTROL DEL TECLADO
    // ==========================================

    document.addEventListener("keydown", function (e) {

        if (
            e.key === "Escape" &&
            menu.classList.contains("yampi-menu-open")
        ) {
            cerrar();
        }

        if (
            e.key !== "Tab" ||
            !menu.classList.contains("yampi-menu-open")
        ) {
            return;
        }

        const elementos = [
            ...menu.querySelectorAll(
                'a[href], button:not([disabled])'
            )
        ].filter(el =>
            el.getClientRects().length &&
            !el.closest(".dropdown-menu:not(.show)")
        );

        if (!elementos.length) return;

        const primero = elementos[0];
        const ultimo = elementos[elementos.length - 1];

        if (e.shiftKey && document.activeElement === primero) {

            e.preventDefault();
            ultimo.focus();

        } else if (
            !e.shiftKey &&
            document.activeElement === ultimo
        ) {

            e.preventDefault();
            primero.focus();

        }

    });

    // ==========================================
    // CERRAR AL SELECCIONAR UNA OPCION
    // ==========================================

    menu.addEventListener("click", function (e) {

        const enlace = e.target.closest(
            'a:not(.dropdown-toggle):not([data-bs-toggle="dropdown"])'
        );

        if (enlace) {
            cerrar();
        }

    });

    // ==========================================
    // CAMBIO DE TAMANO DE PANTALLA
    // ==========================================

    function cambioPantalla() {

        if (!mobile.matches) {

            cerrar();
            menu.inert = false;

        } else if (
            !menu.classList.contains("yampi-menu-open")
        ) {

            menu.inert = true;

        }
    }

    mobile.addEventListener("change", cambioPantalla);

    // ==========================================
    // INICIALIZACION
    // ==========================================

    cambioPantalla();

});
