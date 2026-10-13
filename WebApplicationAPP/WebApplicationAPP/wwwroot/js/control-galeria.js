
document.addEventListener("DOMContentLoaded", function () {

    // ==========================================
    // MODAL AGREGAR IMAGEN
    // ==========================================

    const modalAgregar = document.getElementById(
        "modalAgregarImagen"
    );

    const formAgregar = document.getElementById(
        "formAgregarImagen"
    );

    const archivo = document.getElementById(
        "archivoGaleria"
    );

    const previewContainer = document.getElementById(
        "galeriaPreviewContainer"
    );

    const previewImagen = document.getElementById(
        "galeriaPreviewImagen"
    );

    const previewNombre = document.getElementById(
        "galeriaPreviewNombre"
    );

    const uploadError = document.getElementById(
        "galeriaUploadError"
    );

    const btnGuardar = document.getElementById(
        "btnGuardarImagen"
    );

    let imagenURL = null;


    // ==========================================
    // MOSTRAR ERROR
    // ==========================================

    function mostrarError(mensaje) {

        if (!uploadError) return;

        uploadError.textContent = mensaje;
        uploadError.hidden = false;
    }


    // ==========================================
    // LIMPIAR VISTA PREVIA
    // ==========================================

    function limpiarPreview() {

        if (imagenURL) {
            URL.revokeObjectURL(imagenURL);
            imagenURL = null;
        }

        if (previewImagen) {
            previewImagen.removeAttribute("src");
        }

        if (previewContainer) {
            previewContainer.hidden = true;
        }

        if (previewNombre) {
            previewNombre.textContent = "";
        }

        if (uploadError) {
            uploadError.textContent = "";
            uploadError.hidden = true;
        }
    }


    // ==========================================
    // VALIDAR ARCHIVO
    // ==========================================

    function archivoValido(imagen) {

        if (!imagen) {
            return false;
        }

        const extensionValida = /\.(jpg|jpeg)$/i.test(
            imagen.name
        );

        const tipoValido = imagen.type === "image/jpeg";

        return extensionValida && tipoValido;
    }


    // ==========================================
    // EVENTOS DE AGREGAR IMAGEN
    // ==========================================

    if (modalAgregar && formAgregar && archivo) {

        // Seleccionar imagen
        archivo.addEventListener("change", function () {

            limpiarPreview();

            const imagen = archivo.files[0];

            if (!imagen) {
                return;
            }

            if (!archivoValido(imagen)) {

                mostrarError(
                    "Selecciona una imagen JPG o JPEG valida."
                );

                archivo.value = "";
                return;
            }

            // Generar vista previa temporal
            imagenURL = URL.createObjectURL(imagen);

            if (previewImagen) {
                previewImagen.src = imagenURL;
            }

            if (previewNombre) {
                previewNombre.textContent = imagen.name;
            }

            if (previewContainer) {
                previewContainer.hidden = false;
            }

        });


        // Validar antes de subir
        formAgregar.addEventListener("submit", function (e) {

            const imagen = archivo.files[0];

            if (!archivoValido(imagen)) {

                e.preventDefault();

                mostrarError(
                    "Debes seleccionar una imagen JPG o JPEG."
                );

                return;
            }

            // Evitar doble envio
            if (btnGuardar) {
                btnGuardar.disabled = true;
                btnGuardar.innerHTML =
                    '<i class="bi bi-hourglass-split"></i> Guardando...';
            }

        });


        // Reiniciar al cerrar el modal
        modalAgregar.addEventListener(
            "hidden.bs.modal",
            function () {

                formAgregar.reset();
                limpiarPreview();

                if (btnGuardar) {
                    btnGuardar.disabled = false;
                    btnGuardar.innerHTML =
                        '<i class="bi bi-check-lg"></i> Guardar imagen';
                }

            }
        );

    }


    // ==========================================
    // MODAL ELIMINAR IMAGEN
    // ==========================================

    const modalEliminar = document.getElementById(
        "modalEliminarImagen"
    );

    const nombreEliminar = document.getElementById(
        "nombreImagenEliminar"
    );

    const btnEliminar = document.getElementById(
        "btnConfirmarEliminar"
    );


    // ==========================================
    // PREPARAR ELIMINACION
    // ==========================================

    if (modalEliminar && nombreEliminar && btnEliminar) {

        const urlEliminar = modalEliminar.dataset.deleteUrl;

        modalEliminar.addEventListener(
            "show.bs.modal",
            function (event) {

                const boton = event.relatedTarget;

                if (!boton || !urlEliminar) {
                    btnEliminar.removeAttribute("href");
                    return;
                }

                const nombreImagen = boton.getAttribute(
                    "data-imagen"
                );

                if (!nombreImagen) {
                    btnEliminar.removeAttribute("href");
                    return;
                }

                // Mostrar nombre en el modal
                nombreEliminar.textContent = nombreImagen;

                // Crear URL de eliminacion
                const separador = urlEliminar.includes("?")
                    ? "&"
                    : "?";

                btnEliminar.href =
                    urlEliminar +
                    separador +
                    "nombre=" +
                    encodeURIComponent(nombreImagen);

            }
        );

    }

});
