
document.addEventListener("DOMContentLoaded", () => {

    // ==========================================
    // DATOS
    // ==========================================

    const $ = id => document.getElementById(id);
    const datos = JSON.parse($("repDatosJson").textContent);

    const pagos = (datos.pagos || []).map((p, id) => ({
        id,
        fecha: p.fecha,
        cliente: p.cliente || "Cliente sin nombre",
        metodo: p.metodo || "Otro",
        monto: Number(p.monto) || 0
    }));

    const citas = datos.citas || [];

    const colores = [
        "#d4af37", "#60a5fa", "#4ade80",
        "#a78bfa", "#f59e0b", "#22d3ee"
    ];

    const formato = new Intl.NumberFormat("es-CR", {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });

    const dinero = n => "₡" + formato.format(n || 0);
    const total = lista => lista.reduce((s, p) => s + p.monto, 0);

    const normalizar = s => String(s || "")
        .normalize("NFD")
        .replace(/[\u0300-\u036f]/g, "")
        .toLowerCase()
        .trim();

    const texto = (id, valor) => {
        if ($(id)) $(id).textContent = valor;
    };

    const fechaLocal = s => {
        const [y, m, d] = s.split("-").map(Number);
        return new Date(y, m - 1, d, 12);
    };

    const fechaISO = d =>
        `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;

    const moverDias = (s, n) => {
        const d = fechaLocal(s);
        d.setDate(d.getDate() + n);
        return fechaISO(d);
    };

    const mostrarFecha = s => fechaLocal(s).toLocaleDateString(
        "es-CR", { day: "2-digit", month: "short", year: "numeric" }
    );

    const escapar = s => String(s).replace(/[&<>"']/g, c => ({
        "&": "&amp;", "<": "&lt;", ">": "&gt;",
        '"': "&quot;", "'": "&#39;"
    })[c]);

    const estado = {
        metodo: "Todos",
        rango: "mes",
        agrupacion: "dia",
        pagina: 1,
        orden: "fecha",
        direccion: -1,
        resultados: []
    };

    const graficos = {};
    const porPagina = 10;

    // ==========================================
    // CONFIGURACION GENERAL CHART.JS
    // ==========================================

    if (typeof Chart === "undefined") {
        console.error("No se cargo Chart.js");
        return;
    }

    Chart.defaults.color = "#a5a5aa";
    Chart.defaults.font.family = "Arial, sans-serif";
    Chart.defaults.font.size = 11;
    Chart.defaults.borderColor = "#303034";

    const tooltip = {
        backgroundColor: "#242427",
        titleColor: "#f5f5f5",
        bodyColor: "#d4af37",
        borderColor: "#45413a",
        borderWidth: 1,
        padding: 12,
        displayColors: false,
        callbacks: {
            label: context => dinero(context.parsed.y ?? context.parsed ?? 0)
        }
    };

    function actualizarGrafico(id, configuracion) {
        if (graficos[id]) graficos[id].destroy();
        graficos[id] = new Chart($(id), configuracion);
    }

    // ==========================================
    // FILTROS DE FECHA
    // ==========================================

    function seleccionarRango(rango) {
        const hoy = new Date();
        const finHoy = fechaISO(hoy);

        let inicio = finHoy;
        let fin = finHoy;

        switch (rango) {
            case "7d":
                inicio = moverDias(finHoy, -6);
                break;

            case "30d":
                inicio = moverDias(finHoy, -29);
                break;

            case "mes":
                inicio = fechaISO(new Date(
                    hoy.getFullYear(), hoy.getMonth(), 1
                ));
                break;

            case "anterior":
                inicio = fechaISO(new Date(
                    hoy.getFullYear(), hoy.getMonth() - 1, 1
                ));
                fin = fechaISO(new Date(
                    hoy.getFullYear(), hoy.getMonth(), 0
                ));
                break;

            case "todo": {
                const fechas = [
                    ...pagos.map(p => p.fecha),
                    ...citas
                ].sort();

                inicio = fechas[0] || finHoy;
                fin = [finHoy, fechas[fechas.length - 1] || finHoy]
                    .sort().pop();
                break;
            }
        }

        $("repFechaInicio").value = inicio;
        $("repFechaFin").value = fin;

        estado.rango = rango;
        estado.pagina = 1;
        estado.agrupacion = rango === "todo" ? "mes" : "dia";

        document.querySelectorAll(".rep-preset").forEach(b => {
            b.classList.toggle("active", b.dataset.rango === rango);
        });

        actualizarAgrupacion();
        actualizarDashboard();
    }

    document.querySelectorAll(".rep-preset").forEach(b => {
        b.addEventListener("click", () =>
            seleccionarRango(b.dataset.rango)
        );
    });

    ["repFechaInicio", "repFechaFin"].forEach(id => {
        $(id).addEventListener("change", () => {
            estado.rango = "personalizado";
            estado.pagina = 1;

            document.querySelectorAll(".rep-preset").forEach(b =>
                b.classList.remove("active")
            );

            actualizarDashboard();
        });
    });

    // ==========================================
    // METODOS DE PAGO
    // ==========================================

    function inicializarMetodos() {
        const metodos = [
            "Todos",
            ...new Set(pagos.map(p => p.metodo))
        ];

        $("repMetodos").innerHTML = "";

        metodos.forEach(metodo => {
            const boton = document.createElement("button");

            boton.type = "button";
            boton.className = "rep-metodo-btn";
            boton.textContent = metodo;

            boton.classList.toggle(
                "active", estado.metodo === metodo
            );

            boton.addEventListener("click", () => {
                estado.metodo = metodo;
                estado.pagina = 1;
                inicializarMetodos();
                actualizarDashboard();
            });

            $("repMetodos").appendChild(boton);
        });
    }

    $("repBuscarCliente").addEventListener("input", () => {
        estado.pagina = 1;
        actualizarDashboard();
    });

    // ==========================================
    // AGRUPACION DIA / SEMANA / MES
    // ==========================================

    function actualizarAgrupacion() {
        document.querySelectorAll("[data-agrupa]").forEach(b => {
            b.classList.toggle(
                "active",
                b.dataset.agrupa === estado.agrupacion
            );
        });
    }

    document.querySelectorAll("[data-agrupa]").forEach(b => {
        b.addEventListener("click", () => {
            estado.agrupacion = b.dataset.agrupa;
            actualizarAgrupacion();
            graficoVentas(estado.resultados);
        });
    });

    function claveFecha(fecha) {
        if (estado.agrupacion === "mes") {
            return fecha.slice(0, 7);
        }

        if (estado.agrupacion === "semana") {
            const d = fechaLocal(fecha);
            return moverDias(fecha, -(d.getDay() + 6) % 7);
        }

        return fecha;
    }

    // ==========================================
    // FILTRO CENTRAL
    // ==========================================

    function filtrar(lista, inicio, fin, busqueda) {
        return lista.filter(p =>
            p.fecha >= inicio &&
            p.fecha <= fin &&
            (estado.metodo === "Todos" ||
                p.metodo === estado.metodo) &&
            normalizar(p.cliente).includes(busqueda)
        );
    }

    function actualizarDashboard() {
        const inicio = $("repFechaInicio").value;
        const fin = $("repFechaFin").value;

        if (!inicio || !fin || inicio > fin) {
            $("repErrorFechas").hidden = false;
            texto("repErrorFechas", "Selecciona un intervalo de fechas valido.");
            return;
        }

        $("repErrorFechas").hidden = true;

        const busqueda = normalizar($("repBuscarCliente").value);

        estado.resultados = filtrar(pagos, inicio, fin, busqueda);

        texto("repPeriodoTexto",
            `${mostrarFecha(inicio)} - ${mostrarFecha(fin)}`
        );

        estadisticas(estado.resultados, inicio, fin, busqueda);
        graficoVentas(estado.resultados);
        graficoMetodos(estado.resultados);
        graficoSemana(estado.resultados);
        rankingClientes(estado.resultados);
        tablaPagos();
    }

    // ==========================================
    // ESTADISTICAS
    // ==========================================

    function estadisticas(lista, inicio, fin, busqueda) {
        const monto = total(lista);
        const clientes = new Map();
        const dias = new Map();

        lista.forEach(p => {
            const nombre = normalizar(p.cliente);

            clientes.set(nombre, (clientes.get(nombre) || 0) + 1);
            dias.set(p.fecha, (dias.get(p.fecha) || 0) + p.monto);
        });

        const recurrentes = [...clientes.values()]
            .filter(n => n >= 2).length;

        const mejorDia = [...dias.entries()]
            .sort((a, b) => b[1] - a[1])[0];

        texto("repTotal", dinero(monto));
        texto("repCantidad", lista.length);
        texto("repPromedio", dinero(lista.length ? monto / lista.length : 0));
        texto("repCitas", citas.filter(f => f >= inicio && f <= fin).length);
        texto("repClientes", clientes.size);
        texto("repRecurrentes", recurrentes);
        texto("repMejorDia", mejorDia ? mostrarFecha(mejorDia[0]) : "Sin datos");
        texto("repGraficoTotal", dinero(monto));

        const indicador = $("repVariacion");
        indicador.classList.remove("positive", "negative");

        if (estado.rango === "todo") {
            indicador.textContent = "Sin comparacion";
            return;
        }

        const duracion = Math.round(
            (Date.UTC(...fin.split("-").map((n, i) => i === 1 ? Number(n) - 1 : Number(n))) -
             Date.UTC(...inicio.split("-").map((n, i) => i === 1 ? Number(n) - 1 : Number(n)))) /
            86400000
        ) + 1;

        const anterior = total(filtrar(
            pagos,
            moverDias(inicio, -duracion),
            moverDias(inicio, -1),
            busqueda
        ));

        if (!anterior) {
            indicador.textContent = "Sin base previa";
            return;
        }

        const diferencia = ((monto - anterior) / anterior) * 100;

        indicador.textContent =
            `${diferencia > 0 ? "+" : ""}${diferencia.toFixed(1)}%`;

        indicador.classList.add(
            diferencia >= 0 ? "positive" : "negative"
        );
    }

    // ==========================================
    // CHART.JS - EVOLUCION DE VENTAS
    // ==========================================

    function graficoVentas(lista) {
        const grupos = new Map();

        lista.forEach(p => {
            const clave = claveFecha(p.fecha);
            grupos.set(clave, (grupos.get(clave) || 0) + p.monto);
        });

        const fechas = [...grupos.keys()].sort();

        const etiquetas = fechas.map(f =>
            estado.agrupacion === "mes"
                ? f
                : mostrarFecha(f)
        );

        const ctx = $("repGraficoVentas").getContext("2d");

        const gradiente = ctx.createLinearGradient(0, 0, 0, 310);
        gradiente.addColorStop(0, "rgba(212,175,55,0.28)");
        gradiente.addColorStop(1, "rgba(212,175,55,0)");

        actualizarGrafico("repGraficoVentas", {
            type: "line",
            data: {
                labels: etiquetas,
                datasets: [{
                    label: "Ingresos",
                    data: fechas.map(f => grupos.get(f)),
                    borderColor: "#d4af37",
                    backgroundColor: gradiente,
                    fill: true,
                    tension: 0.35,
                    borderWidth: 3,
                    pointRadius: fechas.length > 35 ? 0 : 4,
                    pointHoverRadius: 6,
                    pointBackgroundColor: "#e4c65a"
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: {
                    intersect: false,
                    mode: "index"
                },
                plugins: {
                    legend: { display: false },
                    tooltip
                },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: { maxTicksLimit: 8 }
                    },
                    y: {
                        beginAtZero: true,
                        ticks: { callback: v => dinero(v) }
                    }
                }
            }
        });
    }

    // ==========================================
    // CHART.JS - METODOS DE PAGO
    // ==========================================

    function graficoMetodos(lista) {
        const grupos = new Map();

        lista.forEach(p => {
            grupos.set(p.metodo,
                (grupos.get(p.metodo) || 0) + p.monto
            );
        });

        const nombres = [...grupos.keys()];
        const valores = [...grupos.values()];
        const monto = total(lista);

        texto("repDonutTotal", dinero(monto));

        const leyenda = $("repMetodosLeyenda");
        leyenda.innerHTML = "";

        nombres.forEach((nombre, i) => {
            const fila = document.createElement("div");
            fila.className = "rep-legend-row";

            const info = document.createElement("div");
            info.className = "rep-legend-name";

            const punto = document.createElement("span");
            punto.className = "rep-legend-dot";
            punto.style.backgroundColor = colores[i % colores.length];

            const titulo = document.createElement("span");
            titulo.textContent = nombre;

            const porcentaje = document.createElement("strong");
            porcentaje.textContent = monto
                ? ((valores[i] / monto) * 100).toFixed(1) + "%"
                : "0%";
            porcentaje.title = dinero(valores[i]);

            info.append(punto, titulo);
            fila.append(info, porcentaje);
            leyenda.appendChild(fila);
        });

        actualizarGrafico("repDonut", {
            type: "doughnut",
            data: {
                labels: nombres.length ? nombres : ["Sin ingresos"],
                datasets: [{
                    data: monto > 0 ? valores : [1],
                    backgroundColor: monto > 0
                        ? nombres.map((_, i) => colores[i % colores.length])
                        : ["#35353a"],
                    borderWidth: 0,
                    hoverOffset: 6,
                    spacing: 3,
                    borderRadius: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: "72%",
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        ...tooltip,
                        enabled: monto > 0,
                        callbacks: {
                            label: c => `${c.label}: ${dinero(c.parsed)}`
                        }
                    }
                }
            }
        });
    }

    // ==========================================
    // CHART.JS - VENTAS POR DIA
    // ==========================================

    function graficoSemana(lista) {
        const valores = Array(7).fill(0);

        lista.forEach(p => {
            const dia = (fechaLocal(p.fecha).getDay() + 6) % 7;
            valores[dia] += p.monto;
        });

        actualizarGrafico("repGraficoSemana", {
            type: "bar",
            data: {
                labels: ["Lun", "Mar", "Mie", "Jue", "Vie", "Sab", "Dom"],
                datasets: [{
                    label: "Ingresos",
                    data: valores,
                    backgroundColor: "#d4af37",
                    hoverBackgroundColor: "#e4c65a",
                    borderRadius: 7,
                    maxBarThickness: 45
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip
                },
                scales: {
                    x: {
                        grid: { display: false }
                    },
                    y: {
                        beginAtZero: true,
                        ticks: { callback: v => dinero(v) }
                    }
                }
            }
        });
    }

    // ==========================================
    // RANKING DE CLIENTES
    // ==========================================

    function rankingClientes(lista) {
        const mapa = new Map();

        lista.forEach(p => {
            const clave = normalizar(p.cliente);
            if (!mapa.has(clave)) {
                mapa.set(clave, { nombre: p.cliente, monto: 0, pagos: 0 });
            }

            const cliente = mapa.get(clave);
            cliente.monto += p.monto;
            cliente.pagos++;
        });

        const ranking = [...mapa.values()]
            .sort((a, b) => b.monto - a.monto);

        texto("repClientesCount", `${ranking.length} clientes`);

        const contenedor = $("repRankingClientes");
        contenedor.innerHTML = "";

        const maximo = ranking[0]?.monto || 1;

        ranking.slice(0, 5).forEach((c, i) => {
            const fila = document.createElement("div");
            fila.className = "rep-client-row";

            fila.innerHTML = `
                <span class="rep-client-position">#${i + 1}</span>

                <span class="rep-client-avatar">
                    ${escapar(c.nombre[0]?.toUpperCase() || "C")}
                </span>

                <div class="rep-client-info">
                    <strong>${escapar(c.nombre)}</strong>
                    <small>${c.pagos} pagos</small>

                    <div class="rep-client-progress">
                        <div style="width:${Math.max(0, c.monto / maximo * 100)}%"></div>
                    </div>
                </div>

                <strong class="rep-client-total">
                    ${dinero(c.monto)}
                </strong>
            `;

            contenedor.appendChild(fila);
        });

        if (!ranking.length) {
            contenedor.innerHTML =
                '<div class="rep-ranking-empty">Sin clientes para este periodo.</div>';
        }
    }

    // ==========================================
    // TABLA Y PAGINACION
    // ==========================================

    function ordenados() {
        return [...estado.resultados].sort((a, b) => {
            const valor = estado.orden === "monto"
                ? a.monto - b.monto
                : a.fecha.localeCompare(b.fecha);

            return (valor || a.id - b.id) * estado.direccion;
        });
    }

    function tablaPagos() {
        const lista = ordenados();
        const paginas = Math.max(1, Math.ceil(lista.length / porPagina));

        estado.pagina = Math.min(estado.pagina, paginas);

        const inicio = (estado.pagina - 1) * porPagina;
        const registros = lista.slice(inicio, inicio + porPagina);

        $("repTablaBody").innerHTML = registros.map(p => {
            const tipo = normalizar(p.metodo);
            const clase = tipo === "efectivo" || tipo === "tarjeta"
                ? tipo : "otro";

            return `
                <tr>
                    <td>${mostrarFecha(p.fecha)}</td>
                    <td>
                        <div class="rep-table-client">
                            <span class="rep-table-avatar">
                                ${escapar(p.cliente[0]?.toUpperCase() || "C")}
                            </span>
                            <strong>${escapar(p.cliente)}</strong>
                        </div>
                    </td>
                    <td>
                        <span class="rep-method-badge ${clase}">
                            ${escapar(p.metodo)}
                        </span>
                    </td>
                    <td>
                        <strong class="rep-table-amount">
                            ${dinero(p.monto)}
                        </strong>
                    </td>
                </tr>
            `;
        }).join("");

        $("repTablaVacia").hidden = lista.length > 0;
        $("repPaginacion").hidden = paginas <= 1;
        $("repAnterior").disabled = estado.pagina <= 1;
        $("repSiguiente").disabled = estado.pagina >= paginas;

        texto("repRegistrosCount", `${lista.length} registros`);

        texto("repResumenTabla", lista.length
            ? `Mostrando ${inicio + 1}-${Math.min(inicio + porPagina, lista.length)} de ${lista.length} pagos`
            : "No hay transacciones para mostrar."
        );

        const numeros = $("repNumeros");
        numeros.innerHTML = "";

        const visibles = new Set([1, paginas]);

        for (let i = estado.pagina - 2; i <= estado.pagina + 2; i++) {
            if (i >= 1 && i <= paginas) visibles.add(i);
        }

        let anterior = 0;

        [...visibles].sort((a, b) => a - b).forEach(n => {
            if (anterior && n > anterior + 1) {
                const puntos = document.createElement("span");
                puntos.className = "rep-page-ellipsis";
                puntos.textContent = "...";
                numeros.appendChild(puntos);
            }

            const boton = document.createElement("button");
            boton.type = "button";
            boton.className = "rep-page-number";
            boton.textContent = n;
            boton.classList.toggle("active", n === estado.pagina);

            boton.addEventListener("click", () => {
                estado.pagina = n;
                tablaPagos();
            });

            numeros.appendChild(boton);
            anterior = n;
        });
    }

    document.querySelectorAll("[data-orden]").forEach(b => {
        b.addEventListener("click", () => {
            if (estado.orden === b.dataset.orden) {
                estado.direccion *= -1;
            } else {
                estado.orden = b.dataset.orden;
                estado.direccion = -1;
            }

            estado.pagina = 1;
            tablaPagos();
        });
    });

    $("repAnterior").addEventListener("click", () => {
        if (estado.pagina > 1) {
            estado.pagina--;
            tablaPagos();
        }
    });

    $("repSiguiente").addEventListener("click", () => {
        if (estado.pagina * porPagina < estado.resultados.length) {
            estado.pagina++;
            tablaPagos();
        }
    });

    // ==========================================
    // EXPORTAR CSV
    // ==========================================

    $("repExportar").addEventListener("click", () => {
        const lista = ordenados();

        if (!lista.length) {
            alert("No hay registros para exportar.");
            return;
        }

        const celda = valor => {
            let s = String(valor ?? "");
            if (/^[=+\-@\t\r\n]/.test(s)) s = "'" + s;
            return `"${s.replace(/"/g, '""')}"`;
        };

        const filas = [
            ["Fecha", "Cliente", "Metodo", "Monto"],
            ...lista.map(p => [
                p.fecha, p.cliente, p.metodo,
                p.monto.toFixed(2).replace(".", ",")
            ])
        ];

        const csv = filas.map(f => f.map(celda).join(";"))
            .join("\r\n");

        const blob = new Blob(["\uFEFF" + csv], {
            type: "text/csv;charset=utf-8"
        });

        const url = URL.createObjectURL(blob);
        const enlace = document.createElement("a");

        enlace.href = url;
        enlace.download =
            `Reporte_Ventas_${$("repFechaInicio").value}_${$("repFechaFin").value}.csv`;

        enlace.click();
        URL.revokeObjectURL(url);
    });

    // ==========================================
    // RESTABLECER
    // ==========================================

    $("repLimpiar").addEventListener("click", () => {
        estado.metodo = "Todos";
        estado.orden = "fecha";
        estado.direccion = -1;
        estado.pagina = 1;

        $("repBuscarCliente").value = "";

        inicializarMetodos();
        seleccionarRango("mes");
    });

    // ==========================================
    // INICIO
    // ==========================================

    inicializarMetodos();
    seleccionarRango("mes");

});
