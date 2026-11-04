/* Yampi BarberShop: interacciones visuales compartidas de Seguridad */
(() => {
    "use strict";

    function iniciar() {
        // Mostrar/ocultar contraseñas sin modificar los names del controlador.
        document.querySelectorAll(".seg-page [data-toggle-password]").forEach((boton) => {
            boton.addEventListener("click", () => {
                const input = document.getElementById(boton.dataset.togglePassword);
                if (!input) return;

                const mostrar = input.type === "password";
                input.type = mostrar ? "text" : "password";
                boton.setAttribute("aria-pressed", String(mostrar));
                boton.setAttribute("aria-label", mostrar ? "Ocultar contraseña" : "Mostrar contraseña");
                boton.title = mostrar ? "Ocultar contraseña" : "Mostrar contraseña";
                const icono = boton.querySelector("i");
                if (icono) icono.className = mostrar ? "bi bi-eye-slash" : "bi bi-eye";
                input.focus({ preventScroll: true });
            });
        });

        // Validación de coincidencia: complemento de la validación obligatoria del servidor.
        document.querySelectorAll(".seg-page form[data-confirm-form]").forEach((formulario) => {
            const password = formulario.querySelector('input[name="password"], input[name="nuevaContrasena"]');
            const confirmar = formulario.querySelector('input[name="confirmPassword"], input[name="confirmarContrasena"]');
            if (!password || !confirmar) return;

            const validar = () => {
                confirmar.setCustomValidity(
                    confirmar.value && password.value !== confirmar.value
                        ? "Las contraseñas no coinciden."
                        : ""
                );
            };
            password.addEventListener("input", validar);
            confirmar.addEventListener("input", validar);
        });

        // Prevenir doble clic accidental; el servidor sigue verificando todo.
        document.querySelectorAll(".seg-page form").forEach((formulario) => {
            formulario.addEventListener("submit", () => {
                const boton = formulario.querySelector('button[type="submit"]');
                if (!boton || !formulario.checkValidity()) return;
                boton.disabled = true;
                const texto = boton.querySelector(".seg-submit-copy");
                if (texto) {
                    texto.dataset.originalText ??= texto.textContent;
                    texto.textContent = "Procesando...";
                }
            });
        });

        // Restaurar estado al volver mediante la caché de navegación.
        window.addEventListener("pageshow", () => {
            document.querySelectorAll('.seg-page button[type="submit"]').forEach((boton) => {
                boton.disabled = false;
                const texto = boton.querySelector(".seg-submit-copy");
                if (texto && texto.dataset.originalText) texto.textContent = texto.dataset.originalText;
            });
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", iniciar, { once: true });
    } else {
        iniciar();
    }
})();
