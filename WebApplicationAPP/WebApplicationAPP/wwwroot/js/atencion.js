
document.addEventListener("DOMContentLoaded", function () {

    const $ = id => document.getElementById(id);

    // ==========================================
    // GAVETAS
    // ==========================================

    function configurarGaveta(opciones) {
        const boton = $(opciones.boton);
        const contenedor = $(opciones.contenedor);
        const cerrar = $(opciones.cerrar);
        const texto = $(opciones.texto);

        if (!boton || !contenedor) return;

        function ocultar() {
            contenedor.classList.add("closed");
            boton.classList.remove("active");
            boton.setAttribute("aria-expanded", "false");

            if (texto) texto.textContent = opciones.textoCerrado;
        }

        function abrir() {
            contenedor.classList.remove("closed");
            boton.classList.add("active");
            boton.setAttribute("aria-expanded", "true");

            if (texto) texto.textContent = opciones.textoAbierto;

            setTimeout(() => {
                contenedor.scrollIntoView({
                    behavior: "smooth",
                    block: "start"
                });
            }, 100);
        }

        boton.addEventListener("click", () => {
            if (contenedor.classList.contains("closed")) {
                abrir();
            } else {
                ocultar();
            }
        });

        cerrar?.addEventListener("click", ocultar);
        ocultar();
    }

    configurarGaveta({
        boton: "abrirCrearAtencion",
        contenedor: "crearAtencionContainer",
        cerrar: "cerrarCrearAtencion",
        texto: "textoCrearAtencion",
        textoAbierto: "Ocultar formulario",
        textoCerrado: "Agregar cliente"
    });

    configurarGaveta({
        boton: "abrirHistorialAtencion",
        contenedor: "historialAtencionContainer",
        cerrar: "cerrarHistorialAtencion",
        texto: "textoBotonHistorial",
        textoAbierto: "Ocultar historial",
        textoCerrado: "Ver historial de atencion"
    });

    // ==========================================
    // HISTORIAL Y PAGINACION
    // ==========================================

    const buscar = $("buscarHistorialAtencion");
    const mostrarTodos = $("mostrarTodoHistorial");
    const resultados = $("historialAtencionResultados");
    const sinResultados = $("historialAtencionSinResultados");
    const paginacion = $("historialAtencionPaginacion");
    const numeros = $("historialAtencionNumeros");
    const anterior = $("historialAtencionAnterior");
    const siguiente = $("historialAtencionSiguiente");
    const resumen = $("historialAtencionResumen");

    const filas = Array.from(
        document.querySelectorAll(".atencion-history-row")
    );

    const registrosPorPagina = 5;
    let paginaActual = 1;
    let filasFiltradas = [...filas];

    function normalizar(texto) {
        return String(texto ?? "")
            .normalize("NFD")
            .replace(/[\u0300-\u036f]/g, "")
            .toLowerCase()
            .trim();
    }

    function mostrarPagina() {
        const total = filasFiltradas.length;
        const paginas = Math.ceil(total / registrosPorPagina);

        paginaActual = Math.max(
            1,
            Math.min(paginaActual, paginas || 1)
        );

        const inicio = (paginaActual - 1) * registrosPorPagina;
        const fin = inicio + registrosPorPagina;

        filas.forEach(fila => {
            fila.style.display = "none";
        });

        filasFiltradas.slice(inicio, fin).forEach(fila => {
            fila.style.display = "";
        });

        if (sinResultados) {
            sinResultados.hidden = total !== 0;
        }

        if (resultados) {
            const hayBusqueda = !!buscar?.value.trim();

            resultados.textContent = hayBusqueda
                ? `${total} de ${filas.length} registros encontrados`
                : `${filas.length} registros finalizados`;
        }

        if (mostrarTodos) {
            mostrarTodos.disabled = !buscar?.value.trim();
        }

        if (numeros) {
            numeros.replaceChildren();

            for (let i = 1; i <= paginas; i++) {
                const boton = document.createElement("button");

                boton.type = "button";
                boton.className = "atencion-page-number";
                boton.textContent = i;

                if (i === paginaActual) {
                    boton.classList.add("active");
                    boton.setAttribute("aria-current", "page");
                }

                boton.addEventListener("click", () => {
                    paginaActual = i;
                    mostrarPagina();
                });

                numeros.appendChild(boton);
            }
        }

        if (anterior) {
            anterior.disabled = paginaActual <= 1;
        }

        if (siguiente) {
            siguiente.disabled = paginaActual >= paginas;
        }

        if (paginacion) {
            paginacion.style.display =
                paginas > 1 ? "flex" : "none";
        }

        if (resumen) {
            resumen.textContent = total === 0
                ? ""
                : `Mostrando ${inicio + 1}-${Math.min(fin, total)} de ${total} registros`;
        }
    }

    function filtrarHistorial() {
        const texto = normalizar(buscar?.value);

        filasFiltradas = filas.filter(fila => {
            return normalizar(fila.dataset.id).includes(texto)
                || normalizar(fila.dataset.cliente).includes(texto)
                || normalizar(fila.dataset.cita).includes(texto);
        });

        paginaActual = 1;
        mostrarPagina();
    }

    buscar?.addEventListener("input", filtrarHistorial);

    mostrarTodos?.addEventListener("click", () => {
        if (buscar) {
            buscar.value = "";
            buscar.focus();
        }

        filasFiltradas = [...filas];
        paginaActual = 1;
        mostrarPagina();
    });

    anterior?.addEventListener("click", () => {
        if (paginaActual > 1) {
            paginaActual--;
            mostrarPagina();
        }
    });

    siguiente?.addEventListener("click", () => {
        const paginas = Math.ceil(
            filasFiltradas.length / registrosPorPagina
        );

        if (paginaActual < paginas) {
            paginaActual++;
            mostrarPagina();
        }
    });

    mostrarPagina();

    // ==========================================
    // NOTIFICACIONES
    // ==========================================

    const toast = $("atencionPagoToast");
    let toastTimer;

    function notificar(mensaje, error = false) {
        if (!toast) return;

        clearTimeout(toastTimer);

        toast.textContent = mensaje;
        toast.classList.toggle("error", error);
        toast.hidden = false;

        toastTimer = setTimeout(() => {
            toast.hidden = true;
        }, 5000);
    }

    // ==========================================
    // SELECTOR PERSONALIZADO
    // ==========================================

    const metodoSelect = $("pagoMetodo");
    const metodoDropdown = $("pagoMetodoDropdown");
    const metodoTrigger = $("pagoMetodoTrigger");
    const metodoTexto = $("pagoMetodoTexto");
    const metodoOpciones = $("pagoMetodoOpciones");

    function cerrarOpciones() {
        if (!metodoDropdown || !metodoTrigger) return;

        metodoDropdown.classList.remove("open");
        metodoTrigger.setAttribute("aria-expanded", "false");
    }

    function seleccionarMetodo(valor) {
        if (!metodoSelect || !metodoTexto) return;

        metodoSelect.value = valor;

        const nombres = {
            Efectivo: "Efectivo",
            Tarjeta: "Tarjeta"
        };

        metodoTexto.textContent =
            nombres[valor] || "Seleccione un metodo";

        metodoDropdown?.classList.toggle(
            "has-value",
            !!valor
        );

        metodoOpciones
            ?.querySelectorAll("[data-value]")
            .forEach(opcion => {
                const seleccionado =
                    opcion.dataset.value === valor;

                opcion.classList.toggle(
                    "selected",
                    seleccionado
                );

                opcion.setAttribute(
                    "aria-selected",
                    String(seleccionado)
                );
            });

        cerrarOpciones();
    }

    metodoTrigger?.addEventListener("click", () => {
        const abierto = metodoDropdown.classList.toggle("open");

        metodoTrigger.setAttribute(
            "aria-expanded",
            String(abierto)
        );
    });

    metodoOpciones
        ?.querySelectorAll("[data-value]")
        .forEach(opcion => {
            opcion.addEventListener("click", () => {
                seleccionarMetodo(opcion.dataset.value);
                metodoTrigger?.focus();
            });
        });

    document.addEventListener("click", evento => {
        if (
            metodoDropdown &&
            !metodoDropdown.contains(evento.target)
        ) {
            cerrarOpciones();
        }
    });

    // ==========================================
    // MODAL DE PAGO
    // ==========================================

    const modal = $("registrarPagoModal");
    const formulario = $("formRegistrarPago");
    const guardar = $("guardarPagoBtn");

    let cerrando = false;
    let enviando = false;
    let ultimoBoton = null;

    function abrirPago(id, cliente) {
        if (!modal || !formulario || enviando) return;

        formulario.reset();
        seleccionarMetodo("");

        $("pagoAtencionId").value = id;
        $("pagoClienteVisible").value = cliente;

        modal.classList.remove("is-closing");
        modal.hidden = false;

        document.body.classList.add("atencion-pago-open");

        $("pagoMonto").focus();
    }

    function cerrarPago() {
        if (!modal || modal.hidden || cerrando || enviando) {
            return;
        }

        cerrando = true;
        cerrarOpciones();

        modal.classList.add("is-closing");

        function terminar() {
            modal.hidden = true;
            modal.classList.remove("is-closing");
            document.body.classList.remove("atencion-pago-open");

            cerrando = false;
            ultimoBoton?.focus();
        }

        if (window.matchMedia(
            "(prefers-reduced-motion: reduce)"
        ).matches) {
            terminar();
            return;
        }

        modal.querySelector(".atencion-pago-modal")
            .addEventListener("animationend", terminar, {
                once: true
            });
    }

    $("cerrarPagoModal")?.addEventListener("click", cerrarPago);
    $("cancelarPagoModal")?.addEventListener("click", cerrarPago);

    modal?.addEventListener("click", evento => {
        if (evento.target === modal) {
            cerrarPago();
        }
    });

    document.addEventListener("keydown", evento => {
        if (evento.key === "Escape") {
            cerrarOpciones();
            cerrarPago();
        }
    });

    document.querySelectorAll(".btn-finalizar-pago")
        .forEach(boton => {
            boton.addEventListener("click", () => {
                if (enviando || cerrando) return;

                ultimoBoton = boton;

                abrirPago(
                    boton.dataset.id,
                    boton.dataset.cliente
                );
            });
        });

    // ==========================================
    // REGISTRAR PAGO CON AJAX
    // ==========================================

    formulario?.addEventListener("submit", async evento => {
        evento.preventDefault();

        if (enviando) return;

        if (!formulario.reportValidity()) return;

        if (!metodoSelect?.value) {
            notificar("Seleccione un metodo de pago.", true);
            metodoTrigger?.focus();
            return;
        }

        enviando = true;
        guardar.disabled = true;

        const contenidoOriginal = guardar.innerHTML;

        guardar.replaceChildren();

        const spinner = document.createElement("span");
        spinner.className = "atencion-pago-spinner";
        spinner.setAttribute("aria-hidden", "true");

        guardar.append(
            spinner,
            document.createTextNode(" Registrando pago...")
        );

        try {
            const respuesta = await fetch(formulario.action, {
                method: "POST",
                credentials: "same-origin",
                redirect: "manual",
                body: new FormData(formulario),
                headers: {
                    "X-Requested-With": "XMLHttpRequest",
                    "Accept": "application/json"
                }
            });

            if (respuesta.type === "opaqueredirect") {
                throw new Error(
                    "El servidor redirigio la solicitud. Comprueba tu sesion y el permiso Pagos/Crear."
                );
            }

            const tipo =
                respuesta.headers.get("content-type") || "";

            if (!tipo.includes("application/json")) {
                console.error(
                    "Respuesta inesperada:",
                    respuesta.status,
                    respuesta.url
                );

                if (respuesta.status === 404) {
                    throw new Error(
                        "No existe la ruta Pagos/RegistrarAjax. Verifica el controlador."
                    );
                }

                if (respuesta.status === 400) {
                    throw new Error(
                        "Solicitud rechazada (400). Comprueba el token antifalsificacion y los campos enviados."
                    );
                }

                if (respuesta.status === 401 ||
                    respuesta.status === 403) {
                    throw new Error(
                        "Acceso denegado. Comprueba tu sesion y el permiso Pagos/Crear."
                    );
                }

                throw new Error(
                    `El servidor devolvio HTTP ${respuesta.status} en lugar de JSON. Revisa la consola y Network.`
                );
            }

            const datos = await respuesta.json();

            if (!respuesta.ok || !datos.ok) {
                throw new Error(
                    datos.mensaje ||
                    `No se pudo registrar el pago (HTTP ${respuesta.status}).`
                );
            }

            window.location.reload();

        } catch (error) {
            console.error("Error registrando pago:", error);
            notificar(error.message, true);

        } finally {
            enviando = false;
            guardar.disabled = false;
            guardar.innerHTML = contenidoOriginal;
        }
    });
});
