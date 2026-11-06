// Disponibilidad de turnos presenciales. El servidor vuelve a validar al guardar.
document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("formIngresoPresencial");
    if (!form) return;
    const horario = document.getElementById("horaIngreso");
    const fecha = document.getElementById("fechaIngreso");
    const barbero = document.getElementById("ingresoBarbero");
    const boton = form.querySelector('button[type="submit"]');
    let version = 0;

    async function refrescar() {
        const actual = ++version;
        const idBarbero = barbero?.value || form.dataset.barberoId;
        const fechaSeleccionada = fecha?.value || "";
        horario.value = "";
        Array.from(horario.options).forEach(o => {
            if (o.value) { o.disabled = true; o.hidden = true; }
        });
        boton.disabled = true;
        if (!idBarbero || !fechaSeleccionada) return;

        try {
            const url = new URL(form.dataset.ocupadosUrl, window.location.origin);
            url.searchParams.set("fecha", fechaSeleccionada);
            url.searchParams.set("idBarbero", idBarbero);
            const respuesta = await fetch(url, { credentials: "same-origin", cache: "no-store" });
            if (!respuesta.ok) throw new Error("No se pudo consultar disponibilidad.");
            const data = await respuesta.json();
            if (actual !== version) return;
            const disponibles = new Set(data.horariosPermitidos || []);
            Array.from(horario.options).forEach(o => {
                if (!o.value) return;
                o.disabled = !disponibles.has(o.value);
                o.hidden = !disponibles.has(o.value);
            });
            boton.disabled = disponibles.size === 0;
        } catch (e) {
            if (actual === version) {
                boton.disabled = true;
                console.error("Agenda presencial:", e.message);
            }
        }
    }
    fecha?.addEventListener("change", refrescar);
    barbero?.addEventListener("change", refrescar);
    refrescar();
});
