
document.addEventListener("DOMContentLoaded", function () {

    // =====================================================
    // GAVETA CREAR CLIENTE
    // =====================================================

    const crearContainer = document.getElementById("crearClienteContainer");
    const abrirCrear = document.getElementById("abrirCrearCliente");
    const cerrarCrear = document.getElementById("cerrarCrearCliente");

    function mostrarCrear() {
        if (!crearContainer) return;

        crearContainer.classList.remove("closed");

        if (abrirCrear) {
            abrirCrear.classList.add("active");
            abrirCrear.setAttribute("aria-expanded", "true");
        }

        setTimeout(function () {
            crearContainer.scrollIntoView({
                behavior: "smooth",
                block: "start"
            });
        }, 100);
    }

    function ocultarCrear() {
        if (!crearContainer) return;

        crearContainer.classList.add("closed");

        if (abrirCrear) {
            abrirCrear.classList.remove("active");
            abrirCrear.setAttribute("aria-expanded", "false");
        }
    }

    if (abrirCrear) {
        abrirCrear.addEventListener("click", function () {

            if (crearContainer.classList.contains("closed")) {
                mostrarCrear();
            } else {
                ocultarCrear();
            }

        });
    }

    if (cerrarCrear) {
        cerrarCrear.addEventListener("click", function () {
            ocultarCrear();
        });
    }


    // =====================================================
    // GAVETA EDITAR CLIENTE
    // =====================================================

    const editarContainer = document.getElementById("editarClienteContainer");
    const cerrarEditar = document.getElementById("cerrarEditarCliente");

    const editarId = document.getElementById("editarId");
    const editarNombre = document.getElementById("editarNombre");
    const editarTelefono = document.getElementById("editarTelefono");

    const botonesEditar = document.querySelectorAll(".btn-editar-cliente");

    function mostrarEditar(id, nombre, telefono) {
        if (!editarContainer) return;

        editarId.value = id;
        editarNombre.value = nombre;
        editarTelefono.value = telefono;

        editarContainer.classList.remove("closed");

        setTimeout(function () {
            editarContainer.scrollIntoView({
                behavior: "smooth",
                block: "center"
            });
        }, 100);
    }

    function ocultarEditar() {
        if (!editarContainer) return;

        editarContainer.classList.add("closed");
    }

    botonesEditar.forEach(function (boton) {

        boton.addEventListener("click", function () {

            const id = this.dataset.id;
            const nombre = this.dataset.nombre;
            const telefono = this.dataset.telefono;

            mostrarEditar(id, nombre, telefono);

        });

    });

    if (cerrarEditar) {
        cerrarEditar.addEventListener("click", function () {
            ocultarEditar();
        });
    }


    // =====================================================
    // BUSQUEDA INSTANTANEA
    // =====================================================

    const buscarInput = document.getElementById("buscarClientes");
    const mostrarTodos = document.getElementById("mostrarTodosClientes");

    const sinResultados = document.getElementById("clientesSinResultados");
    const resultadosTexto = document.getElementById("clientesResultadosTexto");

    const filas = Array.from(
        document.querySelectorAll(".cliente-fila")
    );

    let filasFiltradas = [...filas];

    function normalizar(texto) {
        return String(texto ?? "")
            .normalize("NFD")
            .replace(/[\u0300-\u036f]/g, "")
            .toLowerCase()
            .trim();
    }

    function filtrarClientes() {

        const busqueda = normalizar(buscarInput?.value);

        filasFiltradas = filas.filter(function (fila) {

            const id = normalizar(fila.dataset.id);
            const nombre = normalizar(fila.dataset.nombre);
            const telefono = normalizar(fila.dataset.telefono);

            return (
                id.includes(busqueda) ||
                nombre.includes(busqueda) ||
                telefono.includes(busqueda)
            );

        });

        paginaActual = 1;

        mostrarPagina();

    }

    if (buscarInput) {
        buscarInput.addEventListener("input", function () {
            filtrarClientes();
        });
    }

    // =====================================================
    // MOSTRAR TODOS
    // =====================================================

    if (mostrarTodos) {
        mostrarTodos.addEventListener("click", function () {

            if (buscarInput) {
                buscarInput.value = "";
            }

            filasFiltradas = [...filas];
            paginaActual = 1;

            mostrarPagina();

            if (buscarInput) {
                buscarInput.focus();
            }

        });
    }


    // =====================================================
    // PAGINACION
    // =====================================================

    const paginacion = document.getElementById("clientesPaginacion");
    const numeros = document.getElementById("clientesNumeros");
    const anterior = document.getElementById("clientesAnterior");
    const siguiente = document.getElementById("clientesSiguiente");

    const clientesPorPagina = 5;

    let paginaActual = 1;

    function mostrarPagina() {

        const totalClientes = filasFiltradas.length;

        const totalPaginas = Math.ceil(
            totalClientes / clientesPorPagina
        );

        // Mantener pagina valida
        paginaActual = Math.max(
            1,
            Math.min(paginaActual, totalPaginas || 1)
        );

        const inicio = (paginaActual - 1) * clientesPorPagina;
        const fin = inicio + clientesPorPagina;

        // Ocultar todas las filas
        filas.forEach(function (fila) {
            fila.style.display = "none";
        });

        // Mostrar solo clientes de la pagina actual
        filasFiltradas.slice(inicio, fin).forEach(function (fila) {
            fila.style.display = "";
        });

        // =====================================================
        // MENSAJE SIN RESULTADOS
        // =====================================================

        if (sinResultados) {
            sinResultados.style.display =
                totalClientes === 0 && filas.length > 0
                    ? "block"
                    : "none";
        }

        // =====================================================
        // CONTADOR DE RESULTADOS
        // =====================================================

        if (resultadosTexto) {

            const totalRegistrados = filas.length;
            const buscando = (buscarInput?.value || "").trim() !== "";

            if (buscando) {
                resultadosTexto.textContent =
                    `${totalClientes} de ${totalRegistrados} clientes encontrados`;
            } else {
                resultadosTexto.textContent =
                    `${totalRegistrados} clientes registrados`;
            }

        }

        // =====================================================
        // BOTON MOSTRAR TODOS
        // =====================================================

        if (mostrarTodos) {

            const hayBusqueda =
                (buscarInput?.value || "").trim().length > 0;

            mostrarTodos.disabled = !hayBusqueda;

        }

        // =====================================================
        // NUMEROS DE PAGINA
        // =====================================================

        if (numeros) {

            numeros.innerHTML = "";

            for (let i = 1; i <= totalPaginas; i++) {

                const boton = document.createElement("button");

                boton.type = "button";
                boton.className = "clientes-page-number";
                boton.textContent = i;

                if (i === paginaActual) {
                    boton.classList.add("active");
                }

                boton.addEventListener("click", function () {

                    paginaActual = i;
                    mostrarPagina();

                });

                numeros.appendChild(boton);

            }

        }

        // =====================================================
        // BOTONES ANTERIOR Y SIGUIENTE
        // =====================================================

        if (anterior) {
            anterior.disabled = paginaActual <= 1;
        }

        if (siguiente) {
            siguiente.disabled = paginaActual >= totalPaginas;
        }

        // Ocultar paginacion si solo hay una pagina
        if (paginacion) {
            paginacion.style.display =
                totalPaginas > 1 ? "flex" : "none";
        }

    }

    // =====================================================
    // EVENTOS DE PAGINACION
    // =====================================================

    if (anterior) {

        anterior.addEventListener("click", function () {

            if (paginaActual > 1) {
                paginaActual--;
                mostrarPagina();
            }

        });

    }

    if (siguiente) {

        siguiente.addEventListener("click", function () {

            const totalPaginas = Math.ceil(
                filasFiltradas.length / clientesPorPagina
            );

            if (paginaActual < totalPaginas) {
                paginaActual++;
                mostrarPagina();
            }

        });

    }


    // =====================================================
    // INICIALIZACION
    // =====================================================

    // El formulario de crear inicia cerrado
    ocultarCrear();

    // La gaveta de editar respeta el estado inicial
    // definido en Index.cshtml

    // Mostrar primera pagina
    mostrarPagina();

});
