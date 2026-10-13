
document.addEventListener("DOMContentLoaded", function () {

    // ==========================================
    // ELEMENTOS PRINCIPALES
    // ==========================================

    const modalPago = document.getElementById("modalRegistrarPago");
    const modalCierre = document.getElementById("modalCierreCaja");

    const btnAbrirPago = document.getElementById("btnAbrirPago");
    const btnCerrarPago = document.getElementById("btnCerrarPago");
    const btnCancelarPago = document.getElementById("btnCancelarPago");

    const btnAbrirCierre = document.getElementById("btnAbrirCierre");
    const btnCerrarCierre = document.getElementById("btnCerrarModalCierre");
    const btnCancelarCierre = document.getElementById("btnCancelarCierre");

    const formPago = document.getElementById("formRegistrarPago");
    const formCierre = document.getElementById("formAplicarCierre");

    const inputCliente = document.getElementById("pagoCliente");
    const btnGuardarPago = document.getElementById("btnGuardarPago");
    const btnConfirmarCierre = document.getElementById("btnConfirmarCierre");

    let modalActivo = null;
    let focoAnterior = null;

    // ==========================================
    // FUNCIONES GENERALES DE MODALES
    // ==========================================

    function obtenerControles(modal) {

        return Array.from(
            modal.querySelectorAll(
                'button:not([disabled]):not([hidden]), ' +
                'input:not([disabled]):not([hidden]):not([tabindex="-1"]), ' +
                'select:not([disabled]):not([hidden]):not([tabindex="-1"])'
            )
        ).filter(function (control) {

            return control.getClientRects().length > 0;
        });
    }

    function abrirModal(modal) {

        if (!modal) return;

        // Evitar dos ventanas abiertas simultaneamente.

        if (modalActivo && modalActivo !== modal) {
            modalActivo.hidden = true;
        }

        focoAnterior = document.activeElement;

        modal.hidden = false;
        modalActivo = modal;

        document.body.classList.add("pagos-modal-open");

        const controles = obtenerControles(modal);

        if (modal === modalPago && inputCliente) {

            inputCliente.focus();

        } else if (controles.length > 0) {

            controles[0].focus();

        } else {

            modal.querySelector(".pagos-modal")?.focus();
        }
    }

    function cerrarModal(modal) {

        if (!modal) return;

        // Cerrar selector personalizado al salir.

        cerrarSelectPago();

        modal.hidden = true;

        if (modalActivo === modal) {
            modalActivo = null;
        }

        if (!modalActivo) {
            document.body.classList.remove("pagos-modal-open");
        }

        // Limpiar el formulario solo al cancelar el registro.

        if (modal === modalPago && formPago) {

            formPago.reset();
            reiniciarSelectPago();
        }

        if (
            focoAnterior &&
            typeof focoAnterior.focus === "function"
        ) {
            focoAnterior.focus();
        }
    }

    // ==========================================
    // MODAL REGISTRAR PAGO
    // ==========================================

    if (btnAbrirPago) {

        btnAbrirPago.addEventListener("click", function () {

            abrirModal(modalPago);

        });
    }

    if (btnCerrarPago) {

        btnCerrarPago.addEventListener("click", function () {

            cerrarModal(modalPago);

        });
    }

    if (btnCancelarPago) {

        btnCancelarPago.addEventListener("click", function () {

            cerrarModal(modalPago);

        });
    }

    // ==========================================
    // MODAL CIERRE DE CAJA
    // ==========================================

    if (btnAbrirCierre) {

        btnAbrirCierre.addEventListener("click", function () {

            abrirModal(modalCierre);

        });
    }

    if (btnCerrarCierre) {

        btnCerrarCierre.addEventListener("click", function () {

            cerrarModal(modalCierre);

        });
    }

    if (btnCancelarCierre) {

        btnCancelarCierre.addEventListener("click", function () {

            cerrarModal(modalCierre);

        });
    }

    // ==========================================
    // CERRAR VENTANAS DESDE EL FONDO
    // ==========================================

    [modalPago, modalCierre].forEach(function (modal) {

        if (!modal) return;

        modal.addEventListener("click", function (evento) {

            if (evento.target === modal) {

                cerrarModal(modal);
            }
        });
    });

    // ==========================================
    // ESCAPE Y NAVEGACION CON TAB
    // ==========================================

    document.addEventListener("keydown", function (evento) {

        if (!modalActivo || modalActivo.hidden) return;

        if (evento.key === "Escape") {

            if (
                customSelect &&
                customSelect.classList.contains("open")
            ) {

                cerrarSelectPago();
                selectTrigger?.focus();

            } else {

                cerrarModal(modalActivo);
            }

            return;
        }

        if (evento.key !== "Tab") return;

        // Si el selector esta abierto, cerrarlo.

        cerrarSelectPago();

        const controles = obtenerControles(modalActivo);

        if (controles.length === 0) return;

        const primero = controles[0];
        const ultimo = controles[controles.length - 1];

        if (
            evento.shiftKey &&
            document.activeElement === primero
        ) {

            evento.preventDefault();
            ultimo.focus();

        } else if (
            !evento.shiftKey &&
            document.activeElement === ultimo
        ) {

            evento.preventDefault();
            primero.focus();
        }
    });

    // ==========================================
    // SELECT PERSONALIZADO DE METODO DE PAGO
    // ==========================================

    const customSelect = document.getElementById(
        "pagosCustomSelect"
    );

    const selectNativo = document.getElementById(
        "pagoMetodo"
    );

    const selectTrigger = document.getElementById(
        "pagosSelectTrigger"
    );

    const selectOpciones = document.getElementById(
        "pagosSelectOpciones"
    );

    const selectTexto = document.getElementById(
        "pagosSelectTexto"
    );

    const opcionesPago = Array.from(
        document.querySelectorAll(".pagos-select-option")
    );

    function cerrarSelectPago() {

        if (!customSelect || !selectOpciones) return;

        customSelect.classList.remove("open");

        selectOpciones.hidden = true;

        selectTrigger?.setAttribute(
            "aria-expanded",
            "false"
        );
    }

    function abrirSelectPago() {

        if (!customSelect || !selectOpciones) return;

        customSelect.classList.add("open");

        selectOpciones.hidden = false;

        selectTrigger?.setAttribute(
            "aria-expanded",
            "true"
        );
    }

    function seleccionarMetodoPago(valor) {

        if (!selectNativo || !customSelect) return;

        selectNativo.value = valor;

        const opcion = opcionesPago.find(function (item) {

            return item.dataset.value === valor;
        });

        if (selectTexto) {

            selectTexto.textContent = opcion
                ? opcion.dataset.value
                : "Seleccione un metodo";
        }

        customSelect.classList.toggle(
            "selected",
            Boolean(valor)
        );

        opcionesPago.forEach(function (item) {

            const activo = item.dataset.value === valor;

            item.classList.toggle("active", activo);

            item.setAttribute(
                "aria-selected",
                String(activo)
            );
        });

        selectNativo.dispatchEvent(
            new Event("change", { bubbles: true })
        );

        cerrarSelectPago();
    }

    function reiniciarSelectPago() {

        if (selectNativo) {
            selectNativo.value = "";
        }

        if (selectTexto) {
            selectTexto.textContent = "Seleccione un metodo";
        }

        if (customSelect) {
            customSelect.classList.remove("selected");
        }

        opcionesPago.forEach(function (opcion) {

            opcion.classList.remove("active");

            opcion.setAttribute(
                "aria-selected",
                "false"
            );
        });

        cerrarSelectPago();
    }

    // ABRIR SELECT

    if (selectTrigger) {

        selectTrigger.addEventListener("click", function () {

            if (
                customSelect &&
                customSelect.classList.contains("open")
            ) {

                cerrarSelectPago();

            } else {

                abrirSelectPago();
            }
        });

        selectTrigger.addEventListener("keydown", function (evento) {

            if (
                evento.key === "ArrowDown" ||
                evento.key === "Enter" ||
                evento.key === " "
            ) {

                evento.preventDefault();

                abrirSelectPago();

                if (opcionesPago.length > 0) {

                    const activa = opcionesPago.find(function (opcion) {
                        return opcion.dataset.value === selectNativo.value;
                    });

                    (activa || opcionesPago[0]).focus();
                }
            }
        });
    }

    // SELECCIONAR OPCION

    opcionesPago.forEach(function (opcion, indice) {

        opcion.addEventListener("click", function () {

            seleccionarMetodoPago(this.dataset.value);

            selectTrigger?.focus();
        });

        opcion.addEventListener("keydown", function (evento) {

            if (
                evento.key === "ArrowDown" ||
                evento.key === "ArrowUp"
            ) {

                evento.preventDefault();

                const direccion =
                    evento.key === "ArrowDown" ? 1 : -1;

                const siguiente = (
                    indice + direccion + opcionesPago.length
                ) % opcionesPago.length;

                opcionesPago[siguiente]?.focus();
            }

            if (evento.key === "Escape") {

                evento.preventDefault();
                cerrarSelectPago();
                selectTrigger?.focus();
            }
        });
    });

    // CERRAR SELECT AL HACER CLIC FUERA

    document.addEventListener("click", function (evento) {

        if (
            customSelect &&
            !customSelect.contains(evento.target)
        ) {

            cerrarSelectPago();
        }
    });

    // RESTABLECER SELECT AL LIMPIAR FORMULARIO

    if (formPago) {

        formPago.addEventListener("reset", function () {

            reiniciarSelectPago();
        });
    }

    // ==========================================
    // VALIDACION Y REGISTRO DE PAGO
    // ==========================================

    let enviandoPago = false;

    if (formPago) {

        formPago.addEventListener("submit", function (evento) {

            if (enviandoPago) {

                evento.preventDefault();
                return;
            }

            // El metodo es obligatorio.

            if (
                !selectNativo ||
                !selectNativo.value
            ) {

                evento.preventDefault();

                alert(
                    "Debes seleccionar un metodo de pago."
                );

                selectTrigger?.focus();

                return;
            }

            const monto = document.getElementById("pagoMonto");

            if (
                monto &&
                (
                    !Number.isFinite(Number(monto.value)) ||
                    Number(monto.value) <= 0
                )
            ) {

                evento.preventDefault();

                alert("Ingresa un monto valido.");

                monto.focus();

                return;
            }

            // Evitar envios duplicados.

            enviandoPago = true;

            if (btnGuardarPago) {

                btnGuardarPago.disabled = true;

                btnGuardarPago.innerHTML = `
                    <i class="bi bi-hourglass-split"></i>
                    Registrando...
                `;
            }
        });
    }

    // ==========================================
    // CONFIRMACION Y ENVIO DEL CIERRE
    // ==========================================

    let enviandoCierre = false;

    if (formCierre) {

        formCierre.addEventListener("submit", function (evento) {

            if (enviandoCierre) {

                evento.preventDefault();
                return;
            }

            // El modal ya muestra el resumen
            // y permite confirmar el cierre.

            enviandoCierre = true;

            if (btnConfirmarCierre) {

                btnConfirmarCierre.disabled = true;

                btnConfirmarCierre.innerHTML = `
                    <i class="bi bi-hourglass-split"></i>
                    Aplicando cierre...
                `;
            }
        });
    }

    // ==========================================
    // BUSCADOR Y PAGINACION DEL HISTORIAL
    // ==========================================

    const buscarPagos = document.getElementById("buscarPagos");

    const filasPagos = Array.from(
        document.querySelectorAll(".pagos-data-row")
    );

    const sinResultados = document.getElementById(
        "pagosSinResultados"
    );

    const contenedorPaginacion = document.getElementById(
        "pagosPaginacion"
    );

    const numerosPagina = document.getElementById(
        "pagosNumerosPagina"
    );

    const btnAnterior = document.getElementById(
        "pagosPaginaAnterior"
    );

    const btnSiguiente = document.getElementById(
        "pagosPaginaSiguiente"
    );

    const resumenPaginacion = document.getElementById(
        "pagosResumenPaginacion"
    );

    // Cantidad de pagos por pagina.

    const pagosPorPagina = 10;

    let paginaActual = 1;

    let pagosFiltrados = [...filasPagos];

    // ==========================================
    // ACTUALIZAR PAGINACION
    // ==========================================

    function actualizarPaginacionPagos() {

        const totalRegistros = pagosFiltrados.length;

        const totalPaginas = Math.max(
            1,
            Math.ceil(totalRegistros / pagosPorPagina)
        );

        paginaActual = Math.max(
            1,
            Math.min(paginaActual, totalPaginas)
        );

        const inicio = (
            paginaActual - 1
        ) * pagosPorPagina;

        const fin = inicio + pagosPorPagina;

        // OCULTAR TODAS LAS FILAS

        filasPagos.forEach(function (fila) {

            fila.hidden = true;

            fila.style.display = "none";
        });

        // MOSTRAR LAS FILAS DE LA PAGINA

        pagosFiltrados
            .slice(inicio, fin)
            .forEach(function (fila) {

                fila.hidden = false;

                fila.style.display = "";
            });

        // MOSTRAR MENSAJE SIN RESULTADOS

        if (sinResultados) {

            sinResultados.hidden =
                totalRegistros > 0 ||
                filasPagos.length === 0;
        }

        // OCULTAR PAGINACION SI SOLO HAY UNA PAGINA

        if (contenedorPaginacion) {

            contenedorPaginacion.hidden =
                totalPaginas <= 1;
        }

        // ESTADO DE BOTONES

        if (btnAnterior) {
            btnAnterior.disabled = paginaActual <= 1;
        }

        if (btnSiguiente) {
            btnSiguiente.disabled = paginaActual >= totalPaginas;
        }

        // ==========================================
        // NUMEROS DE PAGINA
        // ==========================================

        if (numerosPagina) {

            numerosPagina.innerHTML = "";

            if (totalPaginas > 1) {

                // Mostrar primera, ultima y paginas
                // cercanas a la seleccionada.

                const paginasVisibles = new Set([
                    1,
                    totalPaginas
                ]);

                for (
                    let numero = paginaActual - 2;
                    numero <= paginaActual + 2;
                    numero++
                ) {

                    if (
                        numero >= 1 &&
                        numero <= totalPaginas
                    ) {

                        paginasVisibles.add(numero);
                    }
                }

                const numeros = Array.from(
                    paginasVisibles
                ).sort(function (a, b) {

                    return a - b;
                });

                let ultimaPagina = 0;

                numeros.forEach(function (numero) {

                    // SEPARADOR

                    if (
                        ultimaPagina > 0 &&
                        numero - ultimaPagina > 1
                    ) {

                        const separador = document.createElement("span");

                        separador.textContent = "...";

                        separador.style.color = "#898992";

                        separador.style.padding = "0 5px";

                        numerosPagina.appendChild(separador);
                    }

                    // BOTON NUMERICO

                    const boton = document.createElement("button");

                    boton.type = "button";

                    boton.className = "pagos-page-number";

                    boton.textContent = numero;

                    if (numero === paginaActual) {

                        boton.classList.add("active");

                        boton.setAttribute(
                            "aria-current",
                            "page"
                        );
                    }

                    boton.addEventListener("click", function () {

                        paginaActual = numero;

                        actualizarPaginacionPagos();
                    });

                    numerosPagina.appendChild(boton);

                    ultimaPagina = numero;
                });
            }
        }

        // ==========================================
        // RESUMEN DE PAGINACION
        // ==========================================

        if (resumenPaginacion) {

            if (totalRegistros === 0) {

                resumenPaginacion.textContent = "";

            } else {

                const primerRegistro = inicio + 1;

                const ultimoRegistro = Math.min(
                    fin,
                    totalRegistros
                );

                resumenPaginacion.textContent =
                    `Mostrando ${primerRegistro}-${ultimoRegistro} de ${totalRegistros} pagos`;
            }
        }
    }

    // ==========================================
    // NORMALIZAR TEXTO PARA BUSCAR
    // ==========================================

    function normalizarTexto(texto) {

        return String(texto || "")
            .normalize("NFD")
            .replace(/[\u0300-\u036f]/g, "")
            .toLocaleLowerCase("es")
            .trim();
    }

    // ==========================================
    // BUSCAR PAGOS
    // ==========================================

    if (buscarPagos) {

        buscarPagos.addEventListener("input", function () {

            const busqueda = normalizarTexto(this.value);

            pagosFiltrados = filasPagos.filter(function (fila) {

                const contenido = normalizarTexto(
                    fila.textContent
                );

                return contenido.includes(busqueda);
            });

            // Al buscar volver a pagina 1.

            paginaActual = 1;

            actualizarPaginacionPagos();
        });
    }

    // ==========================================
    // PAGINA ANTERIOR
    // ==========================================

    if (btnAnterior) {

        btnAnterior.addEventListener("click", function () {

            if (paginaActual > 1) {

                paginaActual--;

                actualizarPaginacionPagos();
            }
        });
    }

    // ==========================================
    // PAGINA SIGUIENTE
    // ==========================================

    if (btnSiguiente) {

        btnSiguiente.addEventListener("click", function () {

            const totalPaginas = Math.ceil(
                pagosFiltrados.length / pagosPorPagina
            );

            if (paginaActual < totalPaginas) {

                paginaActual++;

                actualizarPaginacionPagos();
            }
        });
    }

    // ==========================================
    // INICIALIZAR
    // ==========================================

    reiniciarSelectPago();

    actualizarPaginacionPagos();

});
