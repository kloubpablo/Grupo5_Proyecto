
document.addEventListener("DOMContentLoaded", function () {

    // ==========================================
    // CONFIGURACION
    // ==========================================

    const INTERVALO_ACTUALIZACION = 60000;

    const botonActualizar = document.getElementById(
        "btnActualizarDashboard"
    );

    let actualizando = false;

    // ==========================================
    // ACTUALIZAR DASHBOARD
    // ==========================================

    function actualizarDashboard() {

        if (actualizando) {
            return;
        }

        // Evitar recargar una pestana oculta
        if (document.visibilityState !== "visible") {
            return;
        }

        actualizando = true;

        if (botonActualizar) {

            botonActualizar.disabled = true;

            botonActualizar.innerHTML = `
                <i class="bi bi-arrow-clockwise"></i>
                Actualizando...
            `;
        }

        // Vuelve a consultar los datos al servidor
        window.location.reload();
    }

    // ==========================================
    // BOTON ACTUALIZAR
    // ==========================================

    if (botonActualizar) {

        botonActualizar.addEventListener(
            "click",
            actualizarDashboard
        );

    }

    // ==========================================
    // ACTUALIZACION AUTOMATICA
    // ==========================================

    setInterval(function () {

        actualizarDashboard();

    }, INTERVALO_ACTUALIZACION);

});
