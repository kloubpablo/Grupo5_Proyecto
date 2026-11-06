document.addEventListener("DOMContentLoaded", () => {
    "use strict";

    const seccion = document.getElementById("seccionBuscarCliente");
    const formulario = document.getElementById("formCrearCita");
    if (!seccion || !formulario) return;

    const buscar = document.getElementById("buscarCliente");
    const zonaBusqueda = document.getElementById("clienteZonaBusqueda");
    const resultados = document.getElementById("resultadosClientes");
    const estado = document.getElementById("estadoBusquedaClientes");
    const inputId = document.getElementById("clienteSeleccionadoId");
    const seleccionado = document.getElementById("clienteSeleccionado");
    const nombreSeleccionado = document.getElementById("clienteNombreSeleccionado");
    const telefonoSeleccionado = document.getElementById("clienteTelefonoSeleccionado");
    const cambiar = document.getElementById("cambiarCliente");
    const nuevo = document.getElementById("mostrarRegistroCliente");
    const registro = document.getElementById("registroRapidoCliente");
    const guardar = document.getElementById("guardarNuevoCliente");
    const cancelar = document.getElementById("cancelarNuevoCliente");
    const error = document.getElementById("errorNuevoCliente");
    const nombre = document.getElementById("nuevoClienteNombre");
    const telefono = document.getElementById("nuevoClienteTelefono");
    const email = document.getElementById("nuevoClienteEmail");
    const buscarUrl = seccion.dataset.buscarUrl;
    const registrarUrl = seccion.dataset.registrarUrl;

    let temporizador = null;
    let peticion = null;
    let version = 0;

    function errorRegistro(mensaje) {
        error.textContent = mensaje;
        error.hidden = !mensaje;
    }

    function cancelarBusqueda() {
        clearTimeout(temporizador);
        version++;
        peticion?.abort();
        peticion = null;
    }

    function ocultarResultados() {
        resultados.replaceChildren();
        resultados.hidden = true;
        buscar.setAttribute("aria-expanded", "false");
    }

    function seleccionarCliente(cliente) {
        cancelarBusqueda();
        inputId.value = String(cliente.id);
        nombreSeleccionado.textContent = cliente.nombre || "Cliente";
        telefonoSeleccionado.textContent = cliente.telefono || "Sin telefono";
        zonaBusqueda.hidden = true;
        seleccionado.hidden = false;
        nuevo.hidden = true;
        registro.hidden = true;
        ocultarResultados();
        estado.textContent = "Cliente seleccionado.";
        cambiar.focus();
    }

    function limpiarCliente() {
        cancelarBusqueda();
        inputId.value = "";
        buscar.value = "";
        zonaBusqueda.hidden = false;
        seleccionado.hidden = true;
        nuevo.hidden = false;
        registro.hidden = true;
        errorRegistro("");
        estado.textContent = "Escribe al menos 2 caracteres.";
        ocultarResultados();
        buscar.focus();
    }

    function mostrarResultados(clientes) {
        ocultarResultados();
        if (!Array.isArray(clientes) || !clientes.length) {
            estado.textContent = "No se encontraron clientes.";
            return;
        }

        estado.textContent = `${clientes.length} cliente(s) encontrado(s).`;
        for (const cliente of clientes) {
            const opcion = document.createElement("button");
            opcion.type = "button";
            opcion.className = "cliente-resultado";

            const icono = document.createElement("i");
            icono.className = "bi bi-person-circle";
            icono.setAttribute("aria-hidden", "true");

            const datos = document.createElement("span");
            datos.className = "cliente-resultado-datos";
            const etiqueta = document.createElement("strong");
            etiqueta.textContent = cliente.nombre || "Cliente";
            const detalle = document.createElement("small");
            detalle.textContent = `${cliente.telefono || "Sin telefono"} · #${cliente.id}`;
            datos.append(etiqueta, detalle);
            opcion.append(icono, datos);
            opcion.addEventListener("click", () => seleccionarCliente(cliente));
            resultados.appendChild(opcion);
        }
        resultados.hidden = false;
        buscar.setAttribute("aria-expanded", "true");
    }

    buscar.addEventListener("input", () => {
        cancelarBusqueda();
        ocultarResultados();
        const valor = buscar.value.trim();
        if (valor.length < 2) {
            estado.textContent = "Escribe al menos 2 caracteres.";
            return;
        }

        const estaBusqueda = version;
        temporizador = setTimeout(async () => {
            peticion = new AbortController();
            estado.textContent = "Buscando...";
            try {
                const url = new URL(buscarUrl, window.location.href);
                url.searchParams.set("q", valor);
                const respuesta = await fetch(url, {
                    credentials: "same-origin",
                    signal: peticion.signal,
                    cache: "no-store"
                });
                if (!respuesta.ok) throw new Error("No se pudo buscar.");
                const datos = await respuesta.json();
                if (estaBusqueda !== version) return;
                mostrarResultados(datos);
            } catch (excepcion) {
                if (excepcion.name === "AbortError" || estaBusqueda !== version) return;
                ocultarResultados();
                estado.textContent = "No se pudo buscar. Intenta nuevamente.";
            }
        }, 300);
    });

    buscar.addEventListener("keydown", evento => {
        if (evento.key === "Escape") ocultarResultados();
        if (evento.key === "ArrowDown" && !resultados.hidden) {
            evento.preventDefault();
            resultados.querySelector("button")?.focus();
        }
    });

    resultados.addEventListener("keydown", evento => {
        const botones = Array.from(resultados.querySelectorAll("button"));
        const posicion = botones.indexOf(document.activeElement);
        if (evento.key === "Escape") {
            ocultarResultados();
            buscar.focus();
        } else if (evento.key === "ArrowDown") {
            evento.preventDefault();
            botones[Math.min(posicion + 1, botones.length - 1)]?.focus();
        } else if (evento.key === "ArrowUp") {
            evento.preventDefault();
            if (posicion <= 0) buscar.focus();
            else botones[posicion - 1].focus();
        }
    });

    cambiar.addEventListener("click", limpiarCliente);
    nuevo.addEventListener("click", () => {
        registro.hidden = !registro.hidden;
        ocultarResultados();
        errorRegistro("");
        if (!registro.hidden) nombre.focus();
    });
    cancelar.addEventListener("click", () => {
        registro.hidden = true;
        errorRegistro("");
        nuevo.focus();
    });

    guardar.addEventListener("click", async () => {
        errorRegistro("");
        const datos = {
            nombre: nombre.value.trim(),
            telefono: telefono.value.trim(),
            email: email.value.trim()
        };
        if (datos.nombre.length < 3 || datos.nombre.length > 100) {
            errorRegistro("El nombre debe tener entre 3 y 100 caracteres.");
            nombre.focus();
            return;
        }
        if (!/^[0-9+()\s-]+$/.test(datos.telefono) ||
            datos.telefono.replace(/\D/g, "").length < 8 || datos.telefono.length > 20) {
            errorRegistro("Ingresa un telefono valido.");
            telefono.focus();
            return;
        }
        if (datos.email && !email.checkValidity()) {
            errorRegistro("Ingresa un correo valido.");
            email.focus();
            return;
        }

        const token = formulario.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (!token) {
            errorRegistro("Falta el token de seguridad. Recarga la pagina.");
            return;
        }
        guardar.disabled = true;
        guardar.textContent = "Guardando...";
        try {
            const respuesta = await fetch(registrarUrl, {
                method: "POST",
                credentials: "same-origin",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": token
                },
                body: JSON.stringify(datos)
            });
            const contenido = await respuesta.json();
            if (!respuesta.ok) {
                errorRegistro(contenido.mensaje || "No se pudo registrar el cliente.");
                return;
            }
            nombre.value = "";
            telefono.value = "";
            email.value = "";
            seleccionarCliente(contenido);
        } catch {
            errorRegistro("Error de conexion al registrar el cliente.");
        } finally {
            guardar.disabled = false;
            guardar.replaceChildren();
            const icono = document.createElement("i");
            icono.className = "bi bi-check-lg";
            icono.setAttribute("aria-hidden", "true");
            guardar.append(icono, " Guardar cliente");
        }
    });

    // El submit es interceptado por citas-validacion.js; el servidor tambien valida.
    formulario.addEventListener("reset", () => {
        setTimeout(() => {
            inputId.value = "";
            seleccionado.hidden = true;
            zonaBusqueda.hidden = false;
            nuevo.hidden = false;
            registro.hidden = true;
            buscar.value = "";
            ocultarResultados();
            estado.textContent = "Escribe al menos 2 caracteres.";
        }, 0);
    });
});
