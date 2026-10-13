
document.addEventListener("DOMContentLoaded", function () {

    // ==========================================
    // ELEMENTOS DE FOTO
    // ==========================================

    const avatarButton = document.getElementById("perfilAvatarButton");
    const avatarImagen = document.getElementById("perfilAvatarImagen");
    const avatarIniciales = document.getElementById("perfilIniciales");

    const fotoInput = document.getElementById("perfilFotoInput");
    const btnCambiarFoto = document.getElementById("perfilCambiarFoto");
    const btnEliminarFoto = document.getElementById("perfilEliminarFoto");

    const usuarioId = window.perfilUsuarioId;

    const claveFoto = `yampi_perfil_foto_${usuarioId}`;

    // ==========================================
    // CARGAR FOTO GUARDADA
    // ==========================================

    function mostrarFoto(imagen) {

        avatarImagen.src = imagen;
        avatarImagen.hidden = false;

        avatarIniciales.hidden = true;
        btnEliminarFoto.hidden = false;
    }

    function mostrarIniciales() {

        avatarImagen.removeAttribute("src");
        avatarImagen.hidden = true;

        avatarIniciales.hidden = false;
        btnEliminarFoto.hidden = true;
    }

    function cargarFoto() {

        try {

            const imagenGuardada = localStorage.getItem(claveFoto);

            if (imagenGuardada) {
                mostrarFoto(imagenGuardada);
            } else {
                mostrarIniciales();
            }

        } catch (error) {

            console.warn("No se pudo cargar la foto de perfil.", error);
            mostrarIniciales();
        }
    }

    // ==========================================
    // ABRIR SELECTOR DE ARCHIVOS
    // ==========================================

    function abrirSelectorFoto() {
        fotoInput.click();
    }

    avatarButton.addEventListener("click", abrirSelectorFoto);
    btnCambiarFoto.addEventListener("click", abrirSelectorFoto);

    // ==========================================
    // SELECCIONAR Y GUARDAR FOTO
    // ==========================================

    fotoInput.addEventListener("change", function () {

        const archivo = this.files[0];

        if (!archivo) return;

        const formatosPermitidos = [
            "image/jpeg",
            "image/png",
            "image/webp"
        ];

        if (!formatosPermitidos.includes(archivo.type)) {

            alert("Selecciona una imagen JPG, PNG o WEBP.");
            fotoInput.value = "";
            return;
        }

        if (archivo.size > 2 * 1024 * 1024) {

            alert("La imagen no puede superar los 2 MB.");
            fotoInput.value = "";
            return;
        }

        const lector = new FileReader();

        lector.onload = function (evento) {

            const imagenOriginal = new Image();

            imagenOriginal.onload = function () {

                // Redimensionar para ahorrar espacio
                // dentro del almacenamiento del navegador.

                const canvas = document.createElement("canvas");

                const tamaño = 320;

                canvas.width = tamaño;
                canvas.height = tamaño;

                const contexto = canvas.getContext("2d");

                const lado = Math.min(
                    imagenOriginal.width,
                    imagenOriginal.height
                );

                const origenX = (
                    imagenOriginal.width - lado
                ) / 2;

                const origenY = (
                    imagenOriginal.height - lado
                ) / 2;

                contexto.drawImage(
                    imagenOriginal,
                    origenX,
                    origenY,
                    lado,
                    lado,
                    0,
                    0,
                    tamaño,
                    tamaño
                );

                const imagenFinal = canvas.toDataURL(
                    "image/jpeg",
                    0.82
                );

                try {

                    localStorage.setItem(
                        claveFoto,
                        imagenFinal
                    );

                    mostrarFoto(imagenFinal);

                } catch (error) {

                    alert(
                        "No se pudo guardar la foto. El almacenamiento del navegador puede estar lleno o deshabilitado."
                    );

                }

                fotoInput.value = "";
            };

            imagenOriginal.onerror = function () {
                alert("No se pudo procesar la imagen.");
                fotoInput.value = "";
            };

            imagenOriginal.src = evento.target.result;
        };

        lector.readAsDataURL(archivo);
    });

    // ==========================================
    // ELIMINAR FOTO
    // ==========================================

    btnEliminarFoto.addEventListener("click", function () {

        const confirmar = confirm(
            "¿Deseas eliminar tu foto de perfil?"
        );

        if (!confirmar) return;

        try {
            localStorage.removeItem(claveFoto);
            mostrarIniciales();
        } catch (error) {
            alert("No se pudo eliminar la foto almacenada.");
        }
    });

    // ==========================================
    // GAVETA DE CONTRASEÑA
    // ==========================================

    const togglePassword = document.getElementById(
        "perfilTogglePassword"
    );

    const passwordDrawer = document.getElementById(
        "perfilPasswordDrawer"
    );

    const toggleTexto = document.getElementById(
        "perfilPasswordToggleText"
    );

    const inputPassword = document.getElementById(
        "perfilPassword"
    );

    const inputConfirmar = document.getElementById(
        "perfilConfirmarPassword"
    );

    const passwordError = document.getElementById(
        "perfilPasswordError"
    );

    function abrirGaveta() {

        passwordDrawer.classList.add("open");

        passwordDrawer.setAttribute(
            "aria-hidden",
            "false"
        );

        togglePassword.classList.add("active");

        togglePassword.setAttribute(
            "aria-expanded",
            "true"
        );

        toggleTexto.textContent = "Cancelar cambio";

        inputPassword.disabled = false;
        inputConfirmar.disabled = false;

        inputPassword.focus();
    }

    function cerrarGaveta() {

        passwordDrawer.classList.remove("open");

        passwordDrawer.setAttribute(
            "aria-hidden",
            "true"
        );

        togglePassword.classList.remove("active");

        togglePassword.setAttribute(
            "aria-expanded",
            "false"
        );

        toggleTexto.textContent = "Cambiar contraseña";

        // Limpiar para no enviar contraseñas
        // cuando la gaveta esta cerrada.

        inputPassword.value = "";
        inputConfirmar.value = "";

        inputPassword.disabled = true;
        inputConfirmar.disabled = true;

        passwordError.hidden = true;

        // Restaurar campos ocultos
        inputPassword.type = "password";
        inputConfirmar.type = "password";
    }

    togglePassword.addEventListener("click", function () {

        if (passwordDrawer.classList.contains("open")) {
            cerrarGaveta();
        } else {
            abrirGaveta();
        }
    });

    // ==========================================
    // MOSTRAR U OCULTAR CONTRASEÑA
    // ==========================================

    document.querySelectorAll(
        ".perfil-show-password"
    ).forEach(function (boton) {

        boton.addEventListener("click", function () {

            const objetivo = document.getElementById(
                this.dataset.target
            );

            if (!objetivo) return;

            const visible = objetivo.type === "text";

            objetivo.type = visible
                ? "password"
                : "text";

            const icono = this.querySelector("i");

            icono.className = visible
                ? "bi bi-eye"
                : "bi bi-eye-slash";

            this.setAttribute(
                "aria-label",
                visible
                    ? "Mostrar contraseña"
                    : "Ocultar contraseña"
            );
        });
    });

    // ==========================================
    // VALIDAR CONTRASEÑA ANTES DE GUARDAR
    // ==========================================

    const formulario = document.getElementById(
        "perfilFormulario"
    );

    formulario.addEventListener("submit", function (evento) {

        if (!passwordDrawer.classList.contains("open")) {
            return;
        }

        const password = inputPassword.value;
        const confirmar = inputConfirmar.value;

        passwordError.hidden = true;

        // Ambos vacios significa que no se cambia
        // la contraseña.

        if (!password && !confirmar) {
            return;
        }

        if (!password || password.length < 8) {

            evento.preventDefault();

            passwordError.textContent =
                "La contraseña debe tener al menos 8 caracteres.";

            passwordError.hidden = false;
            inputPassword.focus();

            return;
        }

        if (password !== confirmar) {

            evento.preventDefault();

            passwordError.textContent =
                "Las contraseñas no coinciden.";

            passwordError.hidden = false;
            inputConfirmar.focus();
        }
    });

    // ==========================================
    // INICIALIZAR
    // ==========================================

    cargarFoto();

});
