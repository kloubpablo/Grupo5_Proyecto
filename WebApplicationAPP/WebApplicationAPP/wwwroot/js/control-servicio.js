
document.addEventListener("DOMContentLoaded", function () {

    // ==========================================
    // FUNCIONES GENERALES
    // ==========================================

    function mostrarError(elemento, mensaje) {

        if (!elemento) return;

        elemento.textContent = mensaje;
        elemento.hidden = false;
    }

    function limpiarError(elemento) {

        if (!elemento) return;

        elemento.textContent = "";
        elemento.hidden = true;
    }

    function restaurarBoton(boton, contenido) {

        if (!boton) return;

        boton.disabled = false;
        boton.innerHTML = contenido;
    }

    function validarPrecio(input) {

        if (!input) return false;

        const precio = Number(input.value);

        return input.value.trim() !== "" &&
            Number.isFinite(precio) &&
            precio >= 0;
    }


    // ==========================================
    // MODAL AGREGAR SERVICIO
    // ==========================================

    const modalAgregar = document.getElementById(
        "modalAgregarServicio"
    );

    const formAgregar = document.getElementById(
        "formAgregarServicio"
    );

    const agregarNombre = document.getElementById(
        "servicioAgregarNombre"
    );

    const agregarPrecio = document.getElementById(
        "servicioAgregarPrecio"
    );

    const agregarDescripcion = document.getElementById(
        "servicioAgregarDescripcion"
    );

    const agregarError = document.getElementById(
        "servicioAgregarError"
    );

    const btnAgregar = document.getElementById(
        "btnGuardarNuevoServicio"
    );


    if (modalAgregar && formAgregar) {

        // ==========================================
        // ABRIR MODAL AGREGAR
        // ==========================================

        modalAgregar.addEventListener(
            "show.bs.modal",
            function () {

                formAgregar.reset();
                limpiarError(agregarError);

                restaurarBoton(
                    btnAgregar,
                    '<i class="bi bi-check-lg"></i> Guardar servicio'
                );

            }
        );


        // ==========================================
        // VALIDAR NUEVO SERVICIO
        // ==========================================

        formAgregar.addEventListener(
            "submit",
            function (event) {

                limpiarError(agregarError);

                const nombre = agregarNombre.value.trim();
                const descripcion = agregarDescripcion.value.trim();

                if (!nombre) {

                    event.preventDefault();

                    mostrarError(
                        agregarError,
                        "Debes ingresar el nombre del servicio."
                    );

                    agregarNombre.focus();
                    return;
                }

                if (!validarPrecio(agregarPrecio)) {

                    event.preventDefault();

                    mostrarError(
                        agregarError,
                        "Debes ingresar un precio valido."
                    );

                    agregarPrecio.focus();
                    return;
                }

                if (!descripcion) {

                    event.preventDefault();

                    mostrarError(
                        agregarError,
                        "Debes ingresar una descripcion."
                    );

                    agregarDescripcion.focus();
                    return;
                }

                if (!formAgregar.checkValidity()) {
                    return;
                }

                // Evitar enviar dos veces
                if (btnAgregar) {

                    btnAgregar.disabled = true;

                    btnAgregar.innerHTML =
                        '<i class="bi bi-hourglass-split"></i> Guardando...';
                }

            }
        );


        // ==========================================
        // CERRAR MODAL AGREGAR
        // ==========================================

        modalAgregar.addEventListener(
            "hidden.bs.modal",
            function () {

                formAgregar.reset();
                limpiarError(agregarError);

                restaurarBoton(
                    btnAgregar,
                    '<i class="bi bi-check-lg"></i> Guardar servicio'
                );

            }
        );

    }


    // ==========================================
    // MODAL EDITAR SERVICIO
    // ==========================================

    const modalEditar = document.getElementById(
        "modalEditarServicio"
    );

    const formEditar = document.getElementById(
        "formEditarServicio"
    );

    const editarId = document.getElementById(
        "servicioEditId"
    );

    const editarNombre = document.getElementById(
        "servicioEditNombre"
    );

    const editarPrecio = document.getElementById(
        "servicioEditPrecio"
    );

    const editarDescripcion = document.getElementById(
        "servicioEditDescripcion"
    );

    const editarError = document.getElementById(
        "servicioEditarError"
    );

    const btnEditar = document.getElementById(
        "btnGuardarServicio"
    );


    if (modalEditar && formEditar) {

        // ==========================================
        // CARGAR DATOS DEL SERVICIO
        // ==========================================

        modalEditar.addEventListener(
            "show.bs.modal",
            function (event) {

                const boton = event.relatedTarget;

                if (!boton) return;

                limpiarError(editarError);

                const id = boton.getAttribute(
                    "data-id"
                ) || "";

                const nombre = boton.getAttribute(
                    "data-nombre"
                ) || "";

                const precio = boton.getAttribute(
                    "data-precio"
                ) || "";

                const descripcion = boton.getAttribute(
                    "data-descripcion"
                ) || "";

                // Asignar valores al formulario
                editarId.value = id;
                editarNombre.value = nombre;
                editarPrecio.value = precio;
                editarDescripcion.value = descripcion;

                restaurarBoton(
                    btnEditar,
                    '<i class="bi bi-check-lg"></i> Guardar cambios'
                );

            }
        );


        // ==========================================
        // VALIDAR EDICION
        // ==========================================

        formEditar.addEventListener(
            "submit",
            function (event) {

                limpiarError(editarError);

                const id = editarId.value.trim();
                const nombre = editarNombre.value.trim();
                const descripcion = editarDescripcion.value.trim();

                if (!id) {

                    event.preventDefault();

                    mostrarError(
                        editarError,
                        "No se pudo identificar el servicio."
                    );

                    return;
                }

                if (!nombre) {

                    event.preventDefault();

                    mostrarError(
                        editarError,
                        "El nombre del servicio es obligatorio."
                    );

                    editarNombre.focus();
                    return;
                }

                if (!validarPrecio(editarPrecio)) {

                    event.preventDefault();

                    mostrarError(
                        editarError,
                        "Debes ingresar un precio valido."
                    );

                    editarPrecio.focus();
                    return;
                }

                if (!descripcion) {

                    event.preventDefault();

                    mostrarError(
                        editarError,
                        "La descripcion no puede estar vacia."
                    );

                    editarDescripcion.focus();
                    return;
                }

                if (!formEditar.checkValidity()) {
                    return;
                }

                // Evitar envios duplicados
                if (btnEditar) {

                    btnEditar.disabled = true;

                    btnEditar.innerHTML =
                        '<i class="bi bi-hourglass-split"></i> Guardando...';
                }

            }
        );


        // ==========================================
        // CERRAR MODAL EDITAR
        // ==========================================

        modalEditar.addEventListener(
            "hidden.bs.modal",
            function () {

                formEditar.reset();
                editarId.value = "";

                limpiarError(editarError);

                restaurarBoton(
                    btnEditar,
                    '<i class="bi bi-check-lg"></i> Guardar cambios'
                );

            }
        );

    }

});
