
document.addEventListener("DOMContentLoaded", function () {

    // ==========================================
    // ELEMENTOS
    // ==========================================

    const btnEditar = document.getElementById("btnEditarContacto");
    const btnCancelar = document.getElementById("btnCancelarContacto");

    const vista = document.getElementById("contactoVista");
    const edicion = document.getElementById("contactoEdicion");

    const formulario = document.getElementById("formContacto");

    if (!btnEditar || !btnCancelar || !vista || !edicion || !formulario) {
        return;
    }

    // ==========================================
    // ABRIR EDICION
    // ==========================================

    function abrirEdicion() {

        vista.hidden = true;
        edicion.hidden = false;

        btnEditar.hidden = true;

        edicion.scrollIntoView({
            behavior: "smooth",
            block: "start"
        });
    }

    // ==========================================
    // CERRAR EDICION
    // ==========================================

    function cerrarEdicion() {

        formulario.reset();

        edicion.hidden = true;
        vista.hidden = false;

        btnEditar.hidden = false;

        vista.scrollIntoView({
            behavior: "smooth",
            block: "start"
        });
    }

    // ==========================================
    // EVENTOS
    // ==========================================

    btnEditar.addEventListener("click", abrirEdicion);

    btnCancelar.addEventListener("click", cerrarEdicion);

    // ==========================================
    // ERRORES DE VALIDACION
    // ==========================================

    const errores = document.getElementById("contactoValidationErrors");

    if (errores) {
        abrirEdicion();
    }

});
