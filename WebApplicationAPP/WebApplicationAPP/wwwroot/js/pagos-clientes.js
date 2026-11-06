// Modal administrativo: utilizar un cliente real por IdCliente, nunca un nombre libre.
document.addEventListener("DOMContentLoaded", () => {
    const buscar = document.getElementById("pagoCliente");
    const id = document.getElementById("pagoClienteId");
    const lista = document.getElementById("opcionesClientePago");
    const estado = document.getElementById("estadoClientePago");
    const formulario = document.getElementById("formRegistrarPago");
    if (!buscar || !id || !lista || !formulario) return;

    let timer, controlador, secuencia = 0;
    function limpiar() {
        lista.replaceChildren();
        lista.hidden = true;
        buscar.setAttribute("aria-expanded", "false");
    }

    buscar.addEventListener("input", () => {
        id.value = "";
        clearTimeout(timer);
        ++secuencia;
        controlador?.abort();
        limpiar();
        const termino = buscar.value.trim();
        if (termino.length < 2) {
            estado.textContent = "Escribe al menos dos caracteres.";
            return;
        }
        const actual = secuencia;
        timer = setTimeout(async () => {
            controlador = new AbortController();
            try {
                estado.textContent = "Buscando clientes...";
                const url = new URL("/Citas/BuscarClientes", location.origin);
                url.searchParams.set("q", termino);
                const res = await fetch(url, { credentials: "same-origin", signal: controlador.signal });
                if (!res.ok) throw new Error("Error de busqueda");
                const clientes = await res.json();
                if (actual !== secuencia) return;
                limpiar();
                if (!clientes.length) {
                    estado.textContent = "Sin resultados. Revisa los datos del cliente.";
                    return;
                }
                clientes.forEach(c => {
                    const boton = document.createElement("button");
                    boton.type = "button";
                    const titulo = document.createElement("strong");
                    titulo.textContent = c.nombre;
                    const detalle = document.createElement("small");
                    detalle.textContent = `${c.telefono || "Sin telefono"} · #${c.id}`;
                    boton.append(titulo, detalle);
                    boton.addEventListener("click", () => {
                        id.value = c.id;
                        buscar.value = c.nombre;
                        estado.textContent = `Seleccionado: ${c.nombre} (#${c.id})`;
                        limpiar();
                    });
                    lista.appendChild(boton);
                });
                lista.hidden = false;
                buscar.setAttribute("aria-expanded", "true");
                estado.textContent = `${clientes.length} resultados. Selecciona uno.`;
            } catch (e) {
                if (e.name !== "AbortError") estado.textContent = "No se pudo buscar clientes.";
            }
        }, 300);
    });
    buscar.addEventListener("keydown", e => {
        if (e.key === "ArrowDown" && !lista.hidden) {
            e.preventDefault();
            lista.querySelector("button")?.focus();
        }
        if (e.key === "Escape") limpiar();
    });
    formulario.addEventListener("reset", () => {
        id.value = "";
        estado.textContent = "Selecciona un cliente de los resultados.";
        limpiar();
    });
    // Capture: el JS original tambien valida el submit en fase de burbujeo.
    formulario.addEventListener("submit", e => {
        if (!id.value) {
            e.preventDefault();
            e.stopImmediatePropagation();
            estado.textContent = "Debes elegir un cliente de los resultados.";
            buscar.focus();
        }
    }, true);
});
