document.addEventListener("DOMContentLoaded", function () {

    // =====================================================

    // FECHA ACTUAL

    // =====================================================

    // Fecha actual utilizando el calendario de Costa Rica

    const partesCR = new Intl.DateTimeFormat("en-US", {

        timeZone: "America/Costa_Rica",

        year: "numeric",

        month: "numeric",

        day: "numeric"

    }).formatToParts(new Date());

    function parteCR(tipo) {

        return Number(

            partesCR.find(p => p.type === tipo).value

        );

    }

    const hoy = new Date(

        parteCR("year"),

        parteCR("month") - 1,

        parteCR("day")

    );

    const hoyDia = hoy.getDate();

    const hoyMes = hoy.getMonth() + 1;

    const hoyAnio = hoy.getFullYear();

    // =====================================================

    // ELEMENTOS CREAR CITA

    // =====================================================

    const fechaHidden = document.getElementById("fecha");

    const horaFinal = document.getElementById("horaFinal");

    const btnHoy = document.getElementById("btnHoy");

    // =====================================================

    // ELEMENTOS HORARIOS

    // =====================================================

    const scheduleDia = document.querySelector('[data-name="scheduleDia"]');

    const scheduleMes = document.querySelector('[data-name="scheduleMes"]');

    const scheduleAnio = document.querySelector('[data-name="scheduleAnio"]');

    const fechaHorarios = document.getElementById("fechaHorarios");

    const btnHoyHorarios = document.getElementById("btnHoyHorarios");

    // =====================================================

    // ELEMENTOS EDITAR CITA

    // =====================================================

    const editDrawerContainer =

        document.getElementById("editDrawerContainer");

    const editIdBadge =

        document.getElementById("editIdBadge");

    const editInputIdCita =

        document.getElementById("editInputIdCita");

    const editInputIdCliente =

        document.getElementById("editInputIdCliente");

    const editNombreCliente =

        document.getElementById("editNombreCliente");

    const editAvatarCliente =

        document.getElementById("editAvatarCliente");

    const editFecha =

        document.getElementById("editFecha");

    const editHoraFinal =

        document.getElementById("editHoraFinal");

    const btnHoyEdit =

        document.getElementById("btnHoyEdit");

    // =====================================================

    // NOMBRES DE MESES

    // =====================================================

    const nombresMeses = [

        "",

        "Enero",

        "Febrero",

        "Marzo",

        "Abril",

        "Mayo",

        "Junio",

        "Julio",

        "Agosto",

        "Septiembre",

        "Octubre",

        "Noviembre",

        "Diciembre"

    ];

    // =====================================================

    // TODOS LOS SELECTORES

    // =====================================================

    const selectores = document.querySelectorAll(".custom-select");

    // =====================================================

    // CERRAR SELECTORES

    // =====================================================

    function cerrarTodos(excepto = null) {

        selectores.forEach(function (selector) {

            if (selector !== excepto) {

                selector.classList.remove("open");

            }

        });

    }

    // =====================================================

    // OBTENER SELECTOR

    // =====================================================

    function obtenerSelector(nombre) {

        return document.querySelector(

            `.custom-select[data-name="${nombre}"]`

        );

    }

    // =====================================================

    // OBTENER VALOR

    // =====================================================

    function obtenerValor(nombre) {

        const selector = obtenerSelector(nombre);

        return selector?.dataset.value || "";

    }

    // =====================================================

    // ESTABLECER VALOR

    // =====================================================

    function establecerValor(nombre, valor) {

        const selector = obtenerSelector(nombre);

        if (!selector) {

            return;

        }

        const strValor = String(valor);

        const opcion =

            selector.querySelector(

                `.custom-select-menu button[data-value="${strValor}"]`

            ) ||

            selector.querySelector(

                `.custom-select-menu button[data-value="${strValor.padStart(2, "0")}"]`

            );

        if (!opcion || opcion.disabled) {

            return;

        }

        selector.dataset.value = opcion.dataset.value;

        const texto =

            selector.querySelector(".custom-select-value");

        if (texto) {

            texto.textContent = opcion.textContent.trim();

        }

        selector.classList.add("has-value");

        selector

            .querySelectorAll(".custom-select-menu button")

            .forEach(function (boton) {

                boton.classList.toggle(

                    "selected",

                    boton === opcion

                );

            });

    }

    // =====================================================

    // LIMPIAR SELECTOR

    // =====================================================

    function limpiarSelector(nombre, texto) {

        const selector = obtenerSelector(nombre);

        if (!selector) {

            return;

        }

        selector.dataset.value = "";

        const valSpan =

            selector.querySelector(".custom-select-value");

        if (valSpan) {

            valSpan.textContent = texto;

        }

        selector.classList.remove("has-value");

        selector

            .querySelectorAll(".custom-select-menu button")

            .forEach(function (boton) {

                boton.classList.remove("selected");

            });

    }

    // =====================================================

    // CREAR CITA

    // =====================================================

    const selectorAnio = obtenerSelector("anio");

    if (selectorAnio) {

        const menu =

            selectorAnio.querySelector(".custom-select-menu");

        if (menu) {

            menu.innerHTML = "";

            for (

                let anio = hoyAnio;

                anio <= hoyAnio + 2;

                anio++

            ) {

                const boton =

                    document.createElement("button");

                boton.type = "button";

                boton.dataset.value = anio;

                boton.textContent = anio;

                menu.appendChild(boton);

            }

        }

    }

    // =====================================================

    // MESES CREAR CITA

    // =====================================================

    function actualizarMesesCita() {

        const anio =

            parseInt(obtenerValor("anio"));

        const selectorMes =

            obtenerSelector("mes");

        if (!selectorMes) {

            return;

        }

        const menu =

            selectorMes.querySelector(".custom-select-menu");

        const mesAnterior =

            parseInt(selectorMes.dataset.value);

        if (!menu) {

            return;

        }

        menu.innerHTML = "";

        if (!anio) {

            limpiarSelector("mes", "Mes");

            limpiarSelector("dia", "Dia");

            return;

        }

        const primerMes =

            anio === hoyAnio

                ? hoyMes

                : 1;

        for (

            let mes = primerMes;

            mes <= 12;

            mes++

        ) {

            const boton =

                document.createElement("button");

            boton.type = "button";

            boton.dataset.value = mes;

            boton.textContent = nombresMeses[mes];

            menu.appendChild(boton);

        }

        if (

            mesAnterior &&

            mesAnterior >= primerMes &&

            mesAnterior <= 12

        ) {

            establecerValor("mes", mesAnterior);

        } else {

            limpiarSelector("mes", "Mes");

            limpiarSelector("dia", "Dia");

        }

    }

    // =====================================================

    // DIAS CREAR CITA

    // =====================================================

    function actualizarDiasCita() {

        const anio =

            parseInt(obtenerValor("anio"));

        const mes =

            parseInt(obtenerValor("mes"));

        const selectorDia =

            obtenerSelector("dia");

        if (!selectorDia) {

            return;

        }

        const menu =

            selectorDia.querySelector(".custom-select-menu");

        const diaAnterior =

            parseInt(selectorDia.dataset.value);

        if (!menu) {

            return;

        }

        menu.innerHTML = "";

        if (!anio || !mes) {

            limpiarSelector("dia", "Dia");

            validarFechaCita();

            return;

        }

        const cantidadDias =

            new Date(anio, mes, 0).getDate();

        let primerDia =

            anio === hoyAnio && mes === hoyMes

                ? hoyDia

                : 1;

        for (

            let dia = primerDia;

            dia <= cantidadDias;

            dia++

        ) {

            const boton =

                document.createElement("button");

            boton.type = "button";

            boton.dataset.value = dia;

            boton.textContent =

                String(dia).padStart(2, "0");

            // No incluir domingos en la lista de fechas.
            const esDomingo = new Date(anio, mes - 1, dia).getDay() === 0;
            if (!esDomingo) {
                menu.appendChild(boton);
            }

        }

        if (

            diaAnterior &&

            diaAnterior >= primerDia &&

            diaAnterior <= cantidadDias

        ) {

            establecerValor("dia", diaAnterior);

        } else {

            limpiarSelector("dia", "Dia");

        }

        validarFechaCita();

    }

    // =====================================================

    // VALIDAR FECHA CREAR

    // =====================================================

    function validarFechaCita() {

        if (!fechaHidden) {

            return;

        }

        const dia =

            parseInt(obtenerValor("dia"));

        const mes =

            parseInt(obtenerValor("mes"));

        const anio =

            parseInt(obtenerValor("anio"));

        if (!dia || !mes || !anio) {

            fechaHidden.value = "";

            return;

        }

        fechaHidden.value =

            `${anio}-${String(mes).padStart(2, "0")}-${String(dia).padStart(2, "0")}`;

    }

    // =====================================================

    // VALIDAR HORA CREAR

    // =====================================================

    function actualizarHora() {

        if (!horaFinal) {

            return;

        }

        const hora =

            obtenerValor("hora");

        const minuto =

            obtenerValor("minuto");

        if (!hora || !minuto) {

            horaFinal.value = "";

            return;

        }

        horaFinal.value =

            `${String(hora).padStart(2, "0")}:${String(minuto).padStart(2, "0")}`;

    }

    // =====================================================

    // BOTON HOY - CREAR

    // =====================================================

    if (btnHoy) {

        btnHoy.addEventListener("click", function () {

            establecerValor("anio", hoyAnio);

            actualizarMesesCita();

            establecerValor("mes", hoyMes);

            actualizarDiasCita();

            establecerValor("dia", hoyDia);

            validarFechaCita();

        });

    }

    // =====================================================

    // EDITAR CITA - AÑOS

    // =====================================================

    function crearAniosEdit(anioInicial) {

        const selector =

            obtenerSelector("editAnio");

        if (!selector) {

            return;

        }

        const menu =

            selector.querySelector(".custom-select-menu");

        if (!menu) {

            return;

        }

        menu.innerHTML = "";

        const minAnio =

            Math.min(

                hoyAnio,

                anioInicial || hoyAnio

            );

        for (

            let anio = minAnio;

            anio <= hoyAnio + 2;

            anio++

        ) {

            const boton =

                document.createElement("button");

            boton.type = "button";

            boton.dataset.value = anio;

            boton.textContent = anio;

            menu.appendChild(boton);

        }

    }

    // =====================================================

    // EDITAR CITA - MESES

    // =====================================================

    function actualizarMesesEdit() {

        const anio =

            parseInt(obtenerValor("editAnio"));

        const selector =

            obtenerSelector("editMes");

        if (!selector) {

            return;

        }

        const menu =

            selector.querySelector(".custom-select-menu");

        const mesAnterior =

            parseInt(selector.dataset.value);

        if (!menu) {

            return;

        }

        menu.innerHTML = "";

        if (!anio) {

            return;

        }

        let primerMes = 1;

        if (anio === hoyAnio) {

            primerMes = hoyMes;

            if (

                mesAnterior &&

                mesAnterior < hoyMes

            ) {

                primerMes = mesAnterior;

            }

        }

        for (

            let mes = primerMes;

            mes <= 12;

            mes++

        ) {

            const boton =

                document.createElement("button");

            boton.type = "button";

            boton.dataset.value = mes;

            boton.textContent = nombresMeses[mes];

            menu.appendChild(boton);

        }

        if (

            mesAnterior &&

            mesAnterior >= primerMes &&

            mesAnterior <= 12

        ) {

            establecerValor(

                "editMes",

                mesAnterior

            );

        } else {

            limpiarSelector(

                "editMes",

                "Mes"

            );

            limpiarSelector(

                "editDia",

                "Dia"

            );

        }

    }

    // =====================================================

    // EDITAR CITA - DIAS

    // =====================================================

    function actualizarDiasEdit() {

        const anio =

            parseInt(obtenerValor("editAnio"));

        const mes =

            parseInt(obtenerValor("editMes"));

        const selector =

            obtenerSelector("editDia");

        if (!selector) {

            return;

        }

        const menu =

            selector.querySelector(".custom-select-menu");

        const diaAnterior =

            parseInt(selector.dataset.value);

        if (!menu) {

            return;

        }

        menu.innerHTML = "";

        if (!anio || !mes) {

            validarFechaEdit();

            return;

        }

        const cantidadDias =

            new Date(anio, mes, 0).getDate();

        let primerDia = 1;

        if (

            anio === hoyAnio &&

            mes === hoyMes

        ) {

            primerDia = hoyDia;

            if (

                diaAnterior &&

                diaAnterior < hoyDia

            ) {

                primerDia = diaAnterior;

            }

        }

        for (

            let dia = primerDia;

            dia <= cantidadDias;

            dia++

        ) {

            const boton =

                document.createElement("button");

            boton.type = "button";

            boton.dataset.value = dia;

            boton.textContent =

                String(dia).padStart(2, "0");

            // No incluir domingos en la lista de fechas.
            const esDomingo = new Date(anio, mes - 1, dia).getDay() === 0;
            if (!esDomingo) {
                menu.appendChild(boton);
            }

        }

        if (

            diaAnterior &&

            diaAnterior >= primerDia &&

            diaAnterior <= cantidadDias

        ) {

            establecerValor(

                "editDia",

                diaAnterior

            );

        } else {

            limpiarSelector(

                "editDia",

                "Dia"

            );

        }

        validarFechaEdit();

    }

    // =====================================================

    // EDITAR CITA - FECHA

    // =====================================================

    function validarFechaEdit() {

        if (!editFecha) {

            return;

        }

        const dia =

            parseInt(obtenerValor("editDia"));

        const mes =

            parseInt(obtenerValor("editMes"));

        const anio =

            parseInt(obtenerValor("editAnio"));

        if (!dia || !mes || !anio) {

            editFecha.value = "";

            return;

        }

        editFecha.value =

            `${anio}-${String(mes).padStart(2, "0")}-${String(dia).padStart(2, "0")}`;

    }

    // =====================================================

    // EDITAR CITA - HORA

    // =====================================================

    function actualizarHoraEdit() {

        if (!editHoraFinal) {

            return;

        }

        const hora =

            obtenerValor("editHora");

        const minuto =

            obtenerValor("editMinuto");

        if (!hora || !minuto) {

            editHoraFinal.value = "";

            return;

        }

        editHoraFinal.value =

            `${String(hora).padStart(2, "0")}:${String(minuto).padStart(2, "0")}`;

    }

    // =====================================================

    // BOTON HOY - EDITAR

    // =====================================================

    if (btnHoyEdit) {

        btnHoyEdit.addEventListener("click", function () {

            establecerValor(

                "editAnio",

                hoyAnio

            );

            actualizarMesesEdit();

            establecerValor(

                "editMes",

                hoyMes

            );

            actualizarDiasEdit();

            establecerValor(

                "editDia",

                hoyDia

            );

            validarFechaEdit();

        });

    }

    // =====================================================

    // ABRIR EDITAR

    // =====================================================

    document.addEventListener("click", function (event) {

        const botonEditar =

            event.target.closest(".btn-abrir-editar");

        if (!botonEditar) {

            return;

        }

        if (!editDrawerContainer) {

            return;

        }

        const id =

            botonEditar.dataset.id;

        const clienteId =

            botonEditar.dataset.clienteId;

        const clienteNombre =

            botonEditar.dataset.clienteNombre || "Sin cliente";

        const fecha =

            botonEditar.dataset.fecha;

        const hora =

            botonEditar.dataset.hora;

        const barbero =

            botonEditar.dataset.barbero;

        // ---------------------------------------------

        // DATOS BASICOS

        // ---------------------------------------------

        if (editInputIdCita) {

            editInputIdCita.value = id;

        }

        if (editInputIdCliente) {

            editInputIdCliente.value = clienteId;

        }

        if (editIdBadge) {

            editIdBadge.textContent = "#" + id;

        }

        if (editNombreCliente) {

            editNombreCliente.textContent =

                clienteNombre;

        }

        if (editAvatarCliente) {

            editAvatarCliente.textContent =

                clienteNombre.length > 0

                    ? clienteNombre.charAt(0).toUpperCase()

                    : "?";

        }

        // ---------------------------------------------

        // BARBERO

        // ---------------------------------------------

        editDrawerContainer

            .querySelectorAll('input[name="Barbero"]')

            .forEach(function (radio) {

                radio.checked =

                    radio.value === barbero;

            });

        // ---------------------------------------------

        // FECHA

        // ---------------------------------------------

        if (fecha) {

            const partesFecha =

                fecha.split("-");

            const anio =

                parseInt(partesFecha[0]);

            const mes =

                parseInt(partesFecha[1]);

            const dia =

                parseInt(partesFecha[2]);

            crearAniosEdit(anio);

            establecerValor(

                "editAnio",

                anio

            );

            actualizarMesesEdit();

            establecerValor(

                "editMes",

                mes

            );

            actualizarDiasEdit();

            establecerValor(

                "editDia",

                dia

            );

            validarFechaEdit();

        }

        // ---------------------------------------------

        // HORA

        // ---------------------------------------------

        if (hora) {

            const partesHora =

                hora.split(":");

            const horas =

                partesHora[0];

            const minutos =

                partesHora[1];

            establecerValor(

                "editHora",

                horas

            );

            establecerValor(

                "editMinuto",

                minutos

            );

            if (editHoraFinal) {

                editHoraFinal.value = hora;

            }

        }

        // ---------------------------------------------

        // ABRIR GAVETA

        // ---------------------------------------------

        editDrawerContainer.classList.remove("closed");

        setTimeout(function () {

            editDrawerContainer.scrollIntoView({

                behavior: "smooth",

                block: "nearest"

            });

        }, 100);

    });

    // =====================================================

    // CERRAR EDITAR

    // =====================================================

    document.addEventListener("click", function (event) {

        const botonCerrar =

            event.target.closest(".btn-cerrar-editar");

        if (!botonCerrar) {

            return;

        }

        if (!editDrawerContainer) {

            return;

        }

        // Cerrar gaveta

        editDrawerContainer.classList.add("closed");

        // Cerrar cualquier selector abierto dentro

        editDrawerContainer

            .querySelectorAll(".custom-select.open")

            .forEach(function (selector) {

                selector.classList.remove("open");

            });

    });

    // =====================================================

    // HORARIOS

    // =====================================================

    let scheduleDiaValor = hoyDia;

    let scheduleMesValor = hoyMes;

    let scheduleAnioValor = hoyAnio;

    function crearAniosHorarios() {

        if (!scheduleAnio) {

            return;

        }

        const menu =

            scheduleAnio.querySelector(

                ".custom-select-menu"

            );

        if (!menu) {

            return;

        }

        menu.innerHTML = "";

        for (

            let anio = hoyAnio;

            anio <= hoyAnio + 2;

            anio++

        ) {

            const boton =

                document.createElement("button");

            boton.type = "button";

            boton.dataset.value = anio;

            boton.textContent = anio;

            menu.appendChild(boton);

        }

        establecerValor(

            "scheduleAnio",

            scheduleAnioValor

        );

    }

    // =====================================================

    // HORARIOS - MESES

    // =====================================================

    function crearMesesHorarios() {

        if (!scheduleMes) {

            return;

        }

        const menu =

            scheduleMes.querySelector(

                ".custom-select-menu"

            );

        if (!menu) {

            return;

        }

        menu.innerHTML = "";

        const primerMes =

            scheduleAnioValor === hoyAnio

                ? hoyMes

                : 1;

        if (scheduleMesValor < primerMes) {

            scheduleMesValor = primerMes;

        }

        for (

            let mes = primerMes;

            mes <= 12;

            mes++

        ) {

            const boton =

                document.createElement("button");

            boton.type = "button";

            boton.dataset.value = mes;

            boton.textContent = nombresMeses[mes];

            menu.appendChild(boton);

        }

        establecerValor(

            "scheduleMes",

            scheduleMesValor

        );

    }

    // =====================================================

    // HORARIOS - DIAS

    // =====================================================

    function crearDiasHorarios() {

        if (!scheduleDia) {

            return;

        }

        const menu =

            scheduleDia.querySelector(

                ".custom-select-menu"

            );

        if (!menu) {

            return;

        }

        menu.innerHTML = "";

        const cantidadDias =

            new Date(

                scheduleAnioValor,

                scheduleMesValor,

                0

            ).getDate();

        const primerDia =

            scheduleAnioValor === hoyAnio &&

                scheduleMesValor === hoyMes

                ? hoyDia

                : 1;

        if (

            scheduleDiaValor < primerDia ||

            scheduleDiaValor > cantidadDias

        ) {

            scheduleDiaValor =

                primerDia;

        }

        for (

            let dia = primerDia;

            dia <= cantidadDias;

            dia++

        ) {

            const boton =

                document.createElement("button");

            boton.type = "button";

            boton.dataset.value = dia;

            boton.textContent =

                String(dia).padStart(2, "0");

            menu.appendChild(boton);

        }

        establecerValor(

            "scheduleDia",

            scheduleDiaValor

        );

        actualizarFechaHorarios();

    }

    // =====================================================

    // HORARIOS - FECHA

    // =====================================================

    function actualizarFechaHorarios() {

        if (!fechaHorarios) {

            return;

        }

        fechaHorarios.value =

            `${scheduleAnioValor}-${String(scheduleMesValor).padStart(2, "0")}-${String(scheduleDiaValor).padStart(2, "0")}`;

    }

    // =====================================================

    // HORARIOS - BOTON HOY

    // =====================================================

    if (btnHoyHorarios) {

        btnHoyHorarios.addEventListener(

            "click",

            function () {

                scheduleDiaValor = hoyDia;

                scheduleMesValor = hoyMes;

                scheduleAnioValor = hoyAnio;

                crearAniosHorarios();

                crearMesesHorarios();

                crearDiasHorarios();

                actualizarFechaHorarios();

            }

        );

    }

    // =====================================================

    // CLICK GENERAL DE SELECTORES

    // =====================================================

    document.addEventListener(

        "click",

        function (event) {

            // ---------------------------------------------

            // OPCION

            // ---------------------------------------------

            const opcion =

                event.target.closest(

                    ".custom-select-menu button"

                );

            if (opcion) {

                event.preventDefault();

                event.stopPropagation();

                if (opcion.disabled) {

                    return;

                }

                const selector =

                    opcion.closest(".custom-select");

                if (!selector) {

                    return;

                }

                const nombre =

                    selector.dataset.name;

                const valor =

                    opcion.dataset.value;

                // -----------------------------------------

                // HORARIOS

                // -----------------------------------------

                if (nombre === "scheduleAnio") {

                    scheduleAnioValor =

                        parseInt(valor);

                    establecerValor(

                        "scheduleAnio",

                        scheduleAnioValor

                    );

                    crearMesesHorarios();

                    crearDiasHorarios();

                }

                else if (nombre === "scheduleMes") {

                    scheduleMesValor =

                        parseInt(valor);

                    establecerValor(

                        "scheduleMes",

                        scheduleMesValor

                    );

                    crearDiasHorarios();

                }

                else if (nombre === "scheduleDia") {

                    scheduleDiaValor =

                        parseInt(valor);

                    establecerValor(

                        "scheduleDia",

                        scheduleDiaValor

                    );

                    actualizarFechaHorarios();

                }

                // -----------------------------------------

                // EDITAR

                // -----------------------------------------

                else if (

                    nombre &&

                    nombre.startsWith("edit")

                ) {

                    establecerValor(

                        nombre,

                        valor

                    );

                    if (nombre === "editAnio") {

                        actualizarMesesEdit();

                        actualizarDiasEdit();

                    }

                    else if (

                        nombre === "editMes"

                    ) {

                        actualizarDiasEdit();

                    }

                    else if (

                        nombre === "editDia"

                    ) {

                        validarFechaEdit();

                    }

                    else if (

                        nombre === "editHora" ||

                        nombre === "editMinuto"

                    ) {

                        actualizarHoraEdit();

                    }

                }

                // -----------------------------------------

                // CREAR

                // -----------------------------------------

                else {

                    establecerValor(

                        nombre,

                        valor

                    );

                    if (nombre === "anio") {

                        actualizarMesesCita();

                        actualizarDiasCita();

                    }

                    else if (

                        nombre === "mes"

                    ) {

                        actualizarDiasCita();

                    }

                    else if (

                        nombre === "dia"

                    ) {

                        validarFechaCita();

                    }

                    else if (

                        nombre === "hora" ||

                        nombre === "minuto"

                    ) {

                        actualizarHora();

                    }

                }

                selector.classList.remove("open");

                return;

            }

            // ---------------------------------------------

            // TRIGGER

            // ---------------------------------------------

            const trigger =

                event.target.closest(

                    ".custom-select-trigger"

                );

            if (trigger) {

                event.preventDefault();

                event.stopPropagation();

                const selector =

                    trigger.closest(".custom-select");

                if (!selector) {

                    return;

                }

                const estabaAbierto =

                    selector.classList.contains("open");

                cerrarTodos();

                if (!estabaAbierto) {

                    selector.classList.add("open");

                }

                return;

            }

            // ---------------------------------------------

            // CLICK FUERA

            // ---------------------------------------------

            cerrarTodos();

        },

        true

    );

    // =====================================================

    // INICIALIZACION CREAR CITA

    // =====================================================

    if (fechaHidden) {

        actualizarMesesCita();

        actualizarDiasCita();

        if (

            btnHoy &&

            !fechaHidden.value

        ) {

            btnHoy.click();

        }

    }

    // =====================================================

    // INICIALIZACION HORARIOS

    // =====================================================

    if (

        scheduleDia &&

        scheduleMes &&

        scheduleAnio &&

        fechaHorarios

    ) {

        crearAniosHorarios();

        crearMesesHorarios();

        crearDiasHorarios();

        actualizarFechaHorarios();

    }

    // =====================================================

    // IMPORTANTE:

    // LA GAVETA DE EDITAR SIEMPRE INICIA CERRADA

    // =====================================================

    if (editDrawerContainer) {

        editDrawerContainer.classList.add("closed");

    }

    // =====================================================

    // CONSULTA DE HORARIOS SIN RECARGA (AJAX / FETCH)

    // =====================================================

    const formHorarios = document.querySelector(".schedule-form");

    const contenedorResultados = document.getElementById("contenedorResultadosHorario");

    if (formHorarios && contenedorResultados) {

        formHorarios.addEventListener("submit", async function (e) {

            e.preventDefault(); // <-- Magia: Esto evita que la página se recargue

            // 1. Mostrar estado de carga

            contenedorResultados.innerHTML = `

                <div class="loading-horarios">

                    <i class="bi bi-arrow-repeat"></i>

                    <span>Buscando disponibilidad...</span>

                </div>

            `;

            try {

                // 2. Empaquetar los datos del formulario (fecha)

                const formData = new FormData(formHorarios);

                const url = formHorarios.getAttribute("action");

                // 3. Hacer la petición invisible al servidor

                const respuesta = await fetch(url, {

                    method: 'POST',

                    body: formData

                });

                if (respuesta.ok) {

                    // 4. Recibir todo el HTML que devuelve el servidor

                    const htmlText = await respuesta.text();

                    // 5. Convertir ese texto a un documento HTML navegable en memoria

                    const parser = new DOMParser();

                    const doc = parser.parseFromString(htmlText, 'text/html');

                    // 6. Recortar ÚNICAMENTE la cajita con los resultados

                    const nuevoResultado = doc.querySelector(".schedule-result");

                    // 7. Inyectarla en la página actual

                    if (nuevoResultado) {

                        contenedorResultados.innerHTML = ""; // Limpiamos el "cargando"

                        contenedorResultados.appendChild(nuevoResultado);

                    } else {

                        contenedorResultados.innerHTML = `

                            <div style="padding: 20px; text-align:center; color: var(--gray-4); font-size: 11px;">

                                No se pudo procesar la información.

                            </div>`;

                    }

                } else {

                    contenedorResultados.innerHTML = `

                        <div style="padding: 20px; text-align:center; color: var(--red); font-size: 11px;">

                            Error de conexión con el servidor.

                        </div>`;

                }

            } catch (error) {

                console.error("Error en la consulta AJAX:", error);

                contenedorResultados.innerHTML = `

                    <div style="padding: 20px; text-align:center; color: var(--red); font-size: 11px;">

                        Error en la red. Intente de nuevo.

                    </div>`;

            }

        });

    }

    // =====================================================

    // PAGINACION DEL HISTORIAL

    // =====================================================

    const historialFilas = document.querySelectorAll(".historial-cita-row");

    const historialPaginacion = document.getElementById("historialPaginacion");

    const historialNumeros = document.getElementById("historialNumeros");

    const historialAnterior = document.getElementById("historialAnterior");

    const historialSiguiente = document.getElementById("historialSiguiente");

    const citasPorPagina = 5;

    let paginaHistorial = 1;

    const totalPaginasHistorial = Math.ceil(

        historialFilas.length / citasPorPagina

    );

    function mostrarPaginaHistorial(pagina) {

        if (historialFilas.length === 0) {

            if (historialPaginacion) {

                historialPaginacion.style.display = "none";

            }

            return;

        }

        paginaHistorial = pagina;

        const inicio = (pagina - 1) * citasPorPagina;

        const fin = inicio + citasPorPagina;

        historialFilas.forEach(function (fila, index) {

            if (index >= inicio && index < fin) {

                fila.style.display = "";

            }

            else {

                fila.style.display = "none";

            }

        });

        // Crear numeros de pagina

        if (historialNumeros) {

            historialNumeros.innerHTML = "";

            for (let i = 1; i <= totalPaginasHistorial; i++) {

                const boton = document.createElement("button");

                boton.type = "button";

                boton.className = "pagination-number";

                boton.textContent = i;

                if (i === paginaHistorial) {

                    boton.classList.add("active");

                }

                boton.addEventListener("click", function () {

                    mostrarPaginaHistorial(i);

                });

                historialNumeros.appendChild(boton);

            }

        }

        // Boton anterior

        if (historialAnterior) {

            historialAnterior.disabled =

                paginaHistorial === 1;

        }

        // Boton siguiente

        if (historialSiguiente) {

            historialSiguiente.disabled =

                paginaHistorial === totalPaginasHistorial;

        }

        // Mostrar paginacion solamente cuando sea necesaria

        if (historialPaginacion) {

            historialPaginacion.style.display =

                totalPaginasHistorial > 1

                    ? "flex"

                    : "none";

        }

    }

    // BOTON ANTERIOR

    if (historialAnterior) {

        historialAnterior.addEventListener("click", function () {

            if (paginaHistorial > 1) {

                mostrarPaginaHistorial(

                    paginaHistorial - 1

                );

            }

        });

    }

    // BOTON SIGUIENTE

    if (historialSiguiente) {

        historialSiguiente.addEventListener("click", function () {

            if (paginaHistorial < totalPaginasHistorial) {

                mostrarPaginaHistorial(

                    paginaHistorial + 1

                );

            }

        });

    }

    // Iniciar en la primera pagina

    mostrarPaginaHistorial(1);

});
