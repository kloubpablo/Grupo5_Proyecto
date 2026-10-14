document.addEventListener("DOMContentLoaded", function () {
    // ==========================================
    // CITAS DE 30 MINUTOS - YAMPI BARBERSHOP
    // Lunes a sabado, 07:00 a 20:30
    // ==========================================

    const configuraciones = [
        {
            form: document.getElementById("formCrearCita"),
            fecha: document.getElementById("fecha"),
            hora: document.getElementById("horaFinal"),
            nombreDia: "dia",
            nombreMes: "mes",
            nombreAnio: "anio",
            nombreHora: "hora",
            nombreMinuto: "minuto",
            editar: false
        },
        {
            form: document.getElementById("formEditarCita"),
            fecha: document.getElementById("editFecha"),
            hora: document.getElementById("editHoraFinal"),
            nombreDia: "editDia",
            nombreMes: "editMes",
            nombreAnio: "editAnio",
            nombreHora: "editHora",
            nombreMinuto: "editMinuto",
            editar: true
        }
    ];

    // ==========================================
    // HORA REAL EN COSTA RICA
    // ==========================================

    function ahoraCostaRica() {
        const partes = new Intl.DateTimeFormat("en-US", {
            timeZone: "America/Costa_Rica",
            year: "numeric",
            month: "2-digit",
            day: "2-digit",
            hour: "2-digit",
            hourCycle: "h23"
        }).formatToParts(new Date());

        const valor = tipo => partes.find(p => p.type === tipo)?.value || "";

        return {
            fecha: `${valor("year")}-${valor("month")}-${valor("day")}`,
            hora: Number(valor("hour"))
        };
    }

    function esDomingo(fecha) {
        if (!/^\d{4}-\d{2}-\d{2}$/.test(fecha)) return false;
        const [anio, mes, dia] = fecha.split("-").map(Number);
        return new Date(Date.UTC(anio, mes - 1, dia)).getUTCDay() === 0;
    }

    function formatoFecha(anio, mes, dia) {
        return `${anio}-${String(mes).padStart(2, "0")}-${String(dia).padStart(2, "0")}`;
    }

    // ==========================================
    // SELECTORES PERSONALIZADOS
    // ==========================================

    function selector(nombre) {
        return document.querySelector(`.custom-select[data-name="${nombre}"]`);
    }

    function botones(nombre) {
        return Array.from(selector(nombre)?.querySelectorAll(
            ".custom-select-menu button[data-value]"
        ) || []);
    }

    function valorSeleccionado(nombre) {
        return selector(nombre)?.dataset.value || "";
    }

    function limpiarSeleccion(nombre, texto) {
        const elemento = selector(nombre);
        if (!elemento) return;

        elemento.dataset.value = "";
        elemento.classList.remove("has-value", "open");

        const etiqueta = elemento.querySelector(".custom-select-value");
        if (etiqueta) etiqueta.textContent = texto;

        botones(nombre).forEach(b => b.classList.remove("selected"));
    }

    function desactivarTrigger(nombre, desactivado) {
        const elemento = selector(nombre);
        const trigger = elemento?.querySelector(".custom-select-trigger");
        if (!trigger) return;

        trigger.disabled = desactivado;
        trigger.setAttribute("aria-disabled", String(desactivado));
        if (desactivado) elemento.classList.remove("open");
    }

    // ==========================================
    // DATOS DEL FORMULARIO
    // ==========================================

    function obtenerBarbero(config) {
        return config.form?.querySelector(
            'input[name="barbero"]:checked, input[name="Barbero"]:checked'
        )?.value.trim() || "";
    }

    function idExcluido(config) {
        return config.editar
            ? document.getElementById("editInputIdCita")?.value || ""
            : "";
    }

    function esHorarioValido(fecha, hora) {
        if (!fecha || !/^\d{2}:\d{2}$/.test(hora)) return false;
        if (esDomingo(fecha)) return false;

        const ahora = ahoraCostaRica();
        const [h, m] = hora.split(":").map(Number);

        if (h < 7 || h > 20 || (m !== 0 && m !== 30)) return false;
        if (fecha < ahora.fecha) return false;
        if (fecha === ahora.fecha && h <= ahora.hora) return false;

        return true;
    }

    function minutosTotales(hora) {
        const [h, m] = hora.split(":").map(Number);
        return h * 60 + m;
    }

    // Considerar toda la duracion de 30 minutos, no solo el inicio.
    function existeSolapamiento(horaNueva, horasOcupadas) {
        const inicio = minutosTotales(horaNueva);
        const fin = inicio + 30;

        return horasOcupadas.some(hora => {
            const otroInicio = minutosTotales(hora);
            return otroInicio < fin && inicio < otroInicio + 30;
        });
    }

    // ==========================================
    // MENSAJES
    // ==========================================

    function mostrarMensaje(config, texto) {
        if (!config.form) return;

        let elemento = config.form.querySelector(".cita-validacion-mensaje");
        if (!elemento) {
            elemento = document.createElement("div");
            elemento.className = "cita-validacion-mensaje";
            elemento.setAttribute("role", "alert");
            const pie = config.form.querySelector(".form-footer");
            if (pie) pie.before(elemento);
            else config.form.appendChild(elemento);
        }

        elemento.textContent = texto;
        elemento.hidden = !texto;
    }

    // ==========================================
    // OCULTAR DOMINGOS EN CREAR Y EDITAR
    // ==========================================

    function actualizarDias(config) {
        const anio = Number(valorSeleccionado(config.nombreAnio));
        const mes = Number(valorSeleccionado(config.nombreMes));
        const hoy = ahoraCostaRica().fecha;

        botones(config.nombreDia).forEach(b => {
            const dia = Number(b.dataset.value);
            const fecha = formatoFecha(anio, mes, dia);
            const prohibido = !anio || !mes || esDomingo(fecha) || fecha < hoy;
            b.hidden = prohibido;
            b.disabled = prohibido;
        });

        const fechaActual = config.fecha?.value || "";
        if (fechaActual && (esDomingo(fechaActual) || fechaActual < hoy)) {
            limpiarSeleccion(config.nombreDia, "Dia");
            if (config.fecha) config.fecha.value = "";
            if (config.hora) config.hora.value = "";
            limpiarSeleccion(config.nombreHora, "Hora");
            limpiarSeleccion(config.nombreMinuto, "Minutos");
        }
    }

    // ==========================================
    // CONSULTAR CITAS ACTIVAS Y BLOQUEADAS
    // ==========================================

    async function consultarOcupadas(config) {
        const params = new URLSearchParams({
            barbero: obtenerBarbero(config),
            fecha: config.fecha.value
        });

        const id = idExcluido(config);
        if (id) params.set("excluirId", id);

        // La ruta se calcula a partir del action del formulario.
        const url = new URL("HorariosOcupados", config.form.action);
        url.search = params.toString();

        const respuesta = await fetch(url, {
            credentials: "same-origin",
            cache: "no-store"
        });

        if (!respuesta.ok) {
            throw new Error("No se pudo comprobar la disponibilidad. Intenta nuevamente.");
        }

        const datos = await respuesta.json();
        return (datos.horasOcupadas || []).map(h => String(h).slice(0, 5));
    }

    // ==========================================
    // PINTAR SOLO HORARIOS DISPONIBLES
    // ==========================================

    function pintar(config) {
        const fecha = config.fecha?.value || "";
        const barbero = obtenerBarbero(config);
        const ocupadas = config.ocupadas || [];
        const listaHoras = botones(config.nombreHora);
        const listaMinutos = botones(config.nombreMinuto);

        const horarioLibre = (h, m) => {
            const hora = `${String(h).padStart(2, "0")}:${String(m).padStart(2, "0")}`;
            return Boolean(fecha && barbero) &&
                esHorarioValido(fecha, hora) &&
                !existeSolapamiento(hora, ocupadas);
        };

        // Todas las horas validas deben tener algun minuto libre.
        listaHoras.forEach(b => {
            const h = b.dataset.value;
            const disponible = ["00", "30"].some(m => horarioLibre(h, m));
            b.disabled = !disponible;
            b.hidden = !disponible;
        });

        let h = valorSeleccionado(config.nombreHora);
        if (h && !listaHoras.some(b => b.dataset.value === h && !b.disabled)) {
            limpiarSeleccion(config.nombreHora, "Hora");
            limpiarSeleccion(config.nombreMinuto, "Minutos");
            config.hora.value = "";
            h = "";
        }

        listaMinutos.forEach(b => {
            const m = b.dataset.value;
            const disponible = Boolean(h) &&
                (m === "00" || m === "30") && horarioLibre(h, m);
            b.disabled = !disponible;
            b.hidden = !disponible;
        });

        const m = valorSeleccionado(config.nombreMinuto);
        if (m && !listaMinutos.some(b => b.dataset.value === m && !b.disabled)) {
            limpiarSeleccion(config.nombreMinuto, "Minutos");
            config.hora.value = "";
        }

        desactivarTrigger(config.nombreHora,
            config.consultando || listaHoras.every(b => b.disabled));
        desactivarTrigger(config.nombreMinuto,
            config.consultando || !h || listaMinutos.every(b => b.disabled));

        if (!fecha || !barbero) {
            mostrarMensaje(config, "Selecciona un barbero y una fecha para ver las horas disponibles.");
        } else if (esDomingo(fecha)) {
            mostrarMensaje(config, "Los domingos la barberia esta cerrada.");
        } else if (listaHoras.every(b => b.disabled)) {
            mostrarMensaje(config, "No quedan horarios disponibles para este dia y barbero.");
        } else {
            mostrarMensaje(config, "");
        }
    }

    // ==========================================
    // SINCRONIZAR CONSULTAS SIN VACIAR EL MENU
    // ==========================================

    async function actualizar(config, forzar = false) {
        if (!config.form || !config.fecha || !config.hora) return;

        actualizarDias(config);

        const fecha = config.fecha.value;
        const barbero = obtenerBarbero(config);
        const clave = `${fecha}|${barbero}|${idExcluido(config)}`;
        const listaHoras = botones(config.nombreHora);

        // Sin fecha o sin barbero no hay ninguna consulta pendiente.
        if (!fecha || !barbero || esDomingo(fecha)) {
            config.version++;
            config.consultando = false;
            config.clave = "";
            config.ocupadas = [];
            pintar(config);
            return;
        }

        if (!forzar && clave === config.clave &&
            config.ocupadas && Date.now() - config.ultimaConsulta < 30000) {
            pintar(config);
            return;
        }

        const version = ++config.version;
        config.consultando = true;

        // No borrar opciones durante la consulta: bloquear los triggers.
        desactivarTrigger(config.nombreHora, true);
        desactivarTrigger(config.nombreMinuto, true);
        listaHoras.forEach(b => b.closest(".custom-select")?.classList.remove("open"));

        try {
            const ocupadas = await consultarOcupadas(config);
            if (version !== config.version) return;

            config.ocupadas = ocupadas;
            config.clave = clave;
            config.ultimaConsulta = Date.now();
            config.consultando = false;
            pintar(config);
        } catch (error) {
            if (version !== config.version) return;

            config.ocupadas = null;
            config.clave = "";
            config.consultando = false;

            // No permitir horarios si no se pudo consultar.
            botones(config.nombreHora).forEach(b => {
                b.disabled = true;
                b.hidden = true;
            });
            botones(config.nombreMinuto).forEach(b => {
                b.disabled = true;
                b.hidden = true;
            });
            desactivarTrigger(config.nombreHora, true);
            desactivarTrigger(config.nombreMinuto, true);
            mostrarMensaje(config, error.message);
        }
    }

    function sincronizar(config) {
        // Ejecutar tras las actualizaciones internas de citas.js.
        setTimeout(() => actualizar(config), 0);
    }

    // ==========================================
    // COMPROBACION FINAL ANTES DE GUARDAR
    // ==========================================

    async function guardar(config) {
        if (config.enviando || !config.form) return;

        const fecha = config.fecha?.value || "";
        const hora = config.hora?.value || "";
        const barbero = obtenerBarbero(config);

        if (!fecha || !hora || !barbero) {
            mostrarMensaje(config, "Debes seleccionar barbero, fecha, hora y minutos.");
            return;
        }

        if (!esHorarioValido(fecha, hora)) {
            mostrarMensaje(config,
                "Horario invalido: lunes a sabado, citas de 30 minutos entre 7:00 AM y 8:30 PM (Costa Rica).");
            actualizar(config, true);
            return;
        }

        config.enviando = true;
        const boton = config.form.querySelector('button[type="submit"]');
        if (boton) boton.disabled = true;

        try {
            const ocupadas = await consultarOcupadas(config);
            if (existeSolapamiento(hora, ocupadas)) {
                mostrarMensaje(config, "Este barbero ya tiene una cita o bloqueo en ese intervalo.");
                config.ocupadas = ocupadas;
                config.ultimaConsulta = Date.now();
                pintar(config);
                return;
            }

            if (!esHorarioValido(fecha, hora)) {
                mostrarMensaje(config, "La hora seleccionada ya no esta disponible.");
                actualizar(config, true);
                return;
            }

            // Se conservan todos los campos y el token antifalsificacion.
            // La validacion definitiva se repite en CitasController.
            HTMLFormElement.prototype.submit.call(config.form);
        } catch (error) {
            mostrarMensaje(config, error.message);
        } finally {
            config.enviando = false;
            if (boton) boton.disabled = false;
        }
    }

    // ==========================================
    // EVENTOS - FUNCIONAN SIN PRESIONAR HOY
    // ==========================================

    configuraciones.forEach(config => {
        if (!config.form) return;

        config.version = 0;
        config.consultando = false;
        config.enviando = false;
        config.clave = "";
        config.ocupadas = null;
        config.ultimaConsulta = 0;

        // Cuando se selecciona otro barbero.
        config.form.addEventListener("change", event => {
            if (event.target.matches(
                'input[name="barbero"], input[name="Barbero"]'
            )) sincronizar(config);
        });

        // Interceptar el envio para consultar disponibilidad otra vez.
        config.form.addEventListener("submit", event => {
            event.preventDefault();
            event.stopImmediatePropagation();
            guardar(config);
        }, true);

        // Mantener cerrados los desplegables que no tengan opciones.
        [config.nombreHora, config.nombreMinuto].forEach(nombre => {
            const disparador = selector(nombre)?.querySelector(".custom-select-trigger");
            if (!disparador) return;
            disparador.addEventListener("click", event => {
                if (!disparador.disabled) return;
                event.preventDefault();
                event.stopImmediatePropagation();
                selector(nombre)?.classList.remove("open");
                mostrarMensaje(config, "Selecciona una fecha, un barbero y una hora disponibles.");
            }, true);
        });
    });

    // El archivo citas.js maneja los selectores con eventos globales.
    // Capturamos los clics y esperamos que termine su actualizacion.
    document.addEventListener("click", event => {
        const boton = event.target.closest(
            '.custom-select-menu button, #btnHoy, #btnHoyEdit, .btn-abrir-editar'
        );
        if (!boton || boton.disabled) return;

        const nombre = boton.closest(".custom-select")?.dataset.name || "";

        setTimeout(() => {
            if (["dia", "mes", "anio", "hora", "minuto"].includes(nombre) ||
                boton.id === "btnHoy") {
                sincronizar(configuraciones[0]);
            }
            if (["editDia", "editMes", "editAnio", "editHora", "editMinuto"].includes(nombre) ||
                boton.id === "btnHoyEdit" || boton.classList.contains("btn-abrir-editar")) {
                sincronizar(configuraciones[1]);
            }
        }, 0);
    }, true);

    // Refrescar por si cambia la hora o se reserva desde otra sesion.
    setInterval(() => {
        if (document.visibilityState !== "visible") return;
        configuraciones.forEach(config => {
            if (config.form && !config.enviando) actualizar(config, true);
        });
    }, 30000);

    configuraciones.forEach(config => {
        if (config.form) actualizar(config, true);
    });
});
