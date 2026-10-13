
document.addEventListener("DOMContentLoaded", () => {

    const $ = id => document.getElementById(id);
    const pagina = $("rolesPage");
    const tbody = $("rolesTableBody");
    const crear = $("crearRolContainer");
    const modal = $("rolesPermisosModal");
    const eliminarModal = $("rolesEliminarModal");
    const lista = $("rolesPermisosLista");
    const loading = $("rolesPermisosLoading");
    const contenido = $("rolesPermisosContenido");
    const toast = $("rolesToast");

    const token = document.querySelector(
        '#rolesTokenForm input[name="__RequestVerificationToken"]'
    )?.value;

    let rolActual = null;
    let rolEliminar = null;
    let guardando = false;
    let toastTimer = null;
    let consultaActual = 0;

    // ==========================================
    // NOTIFICACIONES
    // ==========================================

    function notificar(mensaje, error = false) {
        clearTimeout(toastTimer);

        toast.textContent = mensaje;
        toast.className = "roles-toast" + (error ? " error" : "");
        toast.hidden = false;

        toastTimer = setTimeout(() => {
            toast.hidden = true;
        }, 3500);
    }

    // ==========================================
    // PETICIONES AJAX
    // ==========================================

    async function peticion(url, metodo = "GET", datos = null) {
        const opciones = {
            method: metodo,
            credentials: "same-origin",
            headers: {
                "Accept": "application/json",
                "X-Requested-With": "XMLHttpRequest"
            }
        };

        if (metodo === "POST") {
            const formulario = new FormData();

            if (datos) {
                Object.entries(datos).forEach(([clave, valor]) => {
                    formulario.append(clave, valor);
                });
            }

            if (token) {
                formulario.append("__RequestVerificationToken", token);
            }

            opciones.body = formulario;
        }

        const respuesta = await fetch(url, opciones);

        if (respuesta.redirected) {
            throw new Error(
                "El servidor redirigio la solicitud. Revisa los permisos y la sesion."
            );
        }

        const tipo = respuesta.headers.get("content-type") || "";

        if (!tipo.includes("application/json")) {
            throw new Error(
                "El servidor no devolvio JSON. Verifica el controlador."
            );
        }

        const resultado = await respuesta.json();

        if (!respuesta.ok || !resultado.ok) {
            throw new Error(
                resultado.mensaje || "No se pudo completar la operacion"
            );
        }

        return resultado;
    }

    // ==========================================
    // ESTADISTICAS
    // ==========================================

    function actualizarEstadisticas() {
        const filas = Array.from(
            tbody.querySelectorAll(".rol-fila")
        );

        const activos = filas.filter(
            fila => fila.dataset.estado === "true"
        ).length;

        const valores = document.querySelectorAll(
            ".roles-stat strong"
        );

        if (valores.length >= 3) {
            valores[0].textContent = filas.length;
            valores[1].textContent = activos;
            valores[2].textContent = filas.length - activos;
        }
    }

    // ==========================================
    // BUSQUEDA
    // ==========================================

    function filtrar() {
        const buscar = $("buscarRoles").value
            .toLowerCase()
            .trim();

        let encontrados = 0;

        tbody.querySelectorAll(".rol-fila").forEach(fila => {
            const visible = fila.dataset.nombre
                .toLowerCase()
                .includes(buscar);

            fila.hidden = !visible;

            if (visible) encontrados++;
        });

        $("rolesSinResultados").hidden = encontrados !== 0;
    }

    $("buscarRoles").addEventListener("input", filtrar);

    // ==========================================
    // CREAR ROL
    // ==========================================

    function cerrarCrear() {
        crear.classList.add("closed");
    }

    $("abrirCrearRol").addEventListener("click", () => {
        const cerrado = crear.classList.contains("closed");

        crear.classList.toggle("closed", !cerrado);

        if (cerrado) {
            crear.scrollIntoView({
                behavior: "smooth",
                block: "start"
            });
        }
    });

    $("cerrarCrearRol").addEventListener("click", cerrarCrear);
    $("cancelarCrearRol").addEventListener("click", cerrarCrear);

    function crearFila(rol) {
        const fila = document.createElement("tr");
        fila.className = "rol-fila";
        fila.dataset.id = rol.idRol;
        fila.dataset.nombre = rol.nombre;
        fila.dataset.estado = String(rol.estado);

        const nombreTd = document.createElement("td");
        const persona = document.createElement("div");
        persona.className = "roles-person";

        const avatar = document.createElement("span");
        avatar.className = "roles-avatar";
        avatar.innerHTML = '<i class="bi bi-shield-lock"></i>';

        const nombre = document.createElement("strong");
        nombre.textContent = rol.nombre;

        persona.append(avatar, nombre);
        nombreTd.append(persona);

        const estadoTd = document.createElement("td");
        const estado = document.createElement("span");
        estado.className = "roles-status status-active";
        estado.innerHTML =
            '<span class="roles-status-dot"></span> Activo';

        estadoTd.append(estado);

        const accionesTd = document.createElement("td");
        const acciones = document.createElement("div");
        acciones.className = "roles-actions";

        function boton(clase, icono, texto) {
            const b = document.createElement("button");
            b.type = "button";
            b.className = "roles-action " + clase;
            b.innerHTML = `<i class="bi ${icono}"></i> ${texto}`;
            b.dataset.id = rol.idRol;
            return b;
        }

        const permisos = boton(
            "action-permissions btn-permisos",
            "bi-key",
            "Permisos"
        );
        permisos.dataset.nombre = rol.nombre;

        const toggle = boton(
            "action-toggle btn-toggle-rol",
            "bi-arrow-repeat",
            "Estado"
        );

        const eliminar = boton(
            "action-delete btn-eliminar-rol",
            "bi-trash",
            "Eliminar"
        );
        eliminar.dataset.nombre = rol.nombre;

        acciones.append(permisos, toggle, eliminar);
        accionesTd.append(acciones);

        fila.append(nombreTd, estadoTd, accionesTd);

        return fila;
    }

    $("formCrearRol").addEventListener("submit", async evento => {
        evento.preventDefault();

        const form = evento.currentTarget;

        if (!form.reportValidity()) return;

        const boton = form.querySelector('[type="submit"]');
        boton.disabled = true;

        try {
            const datos = await peticion(
                form.action,
                "POST",
                {
                    nombre: $("nombreRol").value
                }
            );

            tbody.prepend(crearFila(datos.rol));

            form.reset();
            cerrarCrear();
            actualizarEstadisticas();
            filtrar();

            notificar(datos.mensaje);
        } catch (error) {
            notificar(error.message, true);
        } finally {
            boton.disabled = false;
        }
    });

    // ==========================================
    // CAMBIAR ESTADO
    // ==========================================

        
    tbody.addEventListener("click", async evento => {
        const boton = evento.target.closest(".btn-toggle-rol");
        if (!boton || guardando) return;

        guardando = true;
        mostrarCargaBoton(boton);

        try {
            const datos = await peticion(
                pagina.dataset.toggleUrl,
                "POST",
                { id: boton.dataset.id }
            );

            const fila = boton.closest(".rol-fila");
            fila.dataset.estado = String(datos.estado);

            const estado = fila.querySelector(".roles-status");

            estado.classList.toggle("status-active", datos.estado);
            estado.classList.toggle("status-inactive", !datos.estado);

            const punto = document.createElement("span");
            punto.className = "roles-status-dot";

            estado.replaceChildren(
                punto,
                document.createTextNode(
                    datos.estado ? " Activo" : " Inactivo"
                )
            );

            actualizarEstadisticas();
            notificar(datos.mensaje);

        } catch (error) {
            notificar(error.message, true);
        } finally {
            guardando = false;
            quitarCargaBoton(boton);
        }
    });


    // ==========================================
    // ABRIR PERMISOS
    // ==========================================

    async function cargarPermisos(idRol) {
        const consulta = ++consultaActual;

        
        loading.hidden = false;
        loading.replaceChildren();

        const spinner = document.createElement("span");
        spinner.className = "roles-spinner";
        spinner.setAttribute("aria-hidden", "true");

        loading.append(
            spinner,
            document.createTextNode(" Cargando permisos...")
        );

        contenido.hidden = true;


        try {
            const base = pagina.dataset.permisosUrl;
            const url = new URL(base, location.origin);

            url.searchParams.set("id", idRol);

            const datos = await peticion(url.toString());

            if (consulta !== consultaActual) return;

            $("rolesPermisosNombre").textContent = datos.nombreRol;
            renderizarPermisos(datos.permisos);

            loading.hidden = true;
            contenido.hidden = false;

        } catch (error) {
            if (consulta !== consultaActual) return;

            loading.textContent = error.message;
            notificar(error.message, true);
        }
    }

    function renderizarPermisos(permisos) {
        lista.replaceChildren();

        permisos.forEach(permiso => {
            const item = document.createElement("div");
            item.className = "roles-permiso";

            if (permiso.seleccionado) {
                item.classList.add("activo");
            }

            const info = document.createElement("div");
            info.className = "roles-permiso-info";

            const icono = document.createElement("i");
            icono.className = "bi bi-key";

            const textos = document.createElement("div");

            const nombre = document.createElement("strong");
            nombre.className = "roles-permiso-nombre";
            nombre.textContent = permiso.nombre;

            const estado = document.createElement("span");
            estado.className = "roles-permiso-estado";
            estado.textContent = permiso.seleccionado
                ? "Activo"
                : "Inactivo";

            textos.append(nombre, estado);
            info.append(icono, textos);

            const boton = document.createElement("button");
            boton.type = "button";
            boton.className = "roles-permiso-toggle";
            boton.setAttribute("role", "switch");
            boton.setAttribute(
                "aria-checked",
                String(permiso.seleccionado)
            );
            boton.setAttribute(
                "aria-label",
                "Cambiar permiso " + permiso.nombre
            );

            
        boton.addEventListener("click", async () => {
            if (boton.disabled) return;

            boton.disabled = true;
            boton.classList.add("is-loading");
            boton.setAttribute("aria-busy", "true");

            try {
                const datos = await peticion(
                    pagina.dataset.togglePermisoUrl,
                    "POST",
                    {
                        idRol: rolActual,
                        idPrivilegio: permiso.idPrivilegio
                    }
                );

                permiso.seleccionado = datos.seleccionado;

                item.classList.toggle(
                    "activo",
                    datos.seleccionado
                );

                boton.setAttribute(
                    "aria-checked",
                    String(datos.seleccionado)
                );

                estado.textContent = datos.seleccionado
                    ? "Activo"
                    : "Inactivo";

                actualizarResumen();
                notificar(datos.mensaje);

            } catch (error) {
                notificar(error.message, true);
            } finally {
                boton.disabled = false;
                boton.classList.remove("is-loading");
                boton.removeAttribute("aria-busy");
            }
        });


            item.append(info, boton);
            lista.append(item);
        });

        actualizarResumen();
    }

    function actualizarResumen() {
        const todos = lista.querySelectorAll(".roles-permiso");
        const activos = lista.querySelectorAll(
            '.roles-permiso-toggle[aria-checked="true"]'
        );

        $("rolesPermisosResumen").textContent =
            `${activos.length} de ${todos.length} permisos activos`;
    }

    tbody.addEventListener("click", evento => {
        const boton = evento.target.closest(".btn-permisos");
        if (!boton) return;

        rolActual = boton.dataset.id;

        $("rolesPermisosNombre").textContent =
            boton.dataset.nombre;

        modal.hidden = false;
        cargarPermisos(rolActual);
    });

    
    let cerrandoPermisos = false;

    function cerrarPermisos() {
        if (modal.hidden || cerrandoPermisos) return;

        cerrandoPermisos = true;
        modal.classList.add("is-closing");

        const terminarCierre = () => {
            modal.hidden = true;
            modal.classList.remove("is-closing");

            rolActual = null;
            consultaActual++;
            cerrandoPermisos = false;
        };

        if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
            terminarCierre();
            return;
        }

        const ventana = modal.querySelector(".roles-modal");

        ventana.addEventListener(
            "animationend",
            terminarCierre,
            { once: true }
        );
    }


    $("cerrarPermisos").addEventListener(
        "click",
        cerrarPermisos
    );

    $("terminarPermisos").addEventListener(
        "click",
        cerrarPermisos
    );

    modal.addEventListener("click", evento => {
        if (evento.target === modal) cerrarPermisos();
    });

    // ==========================================
    // ELIMINAR
    // ==========================================

    tbody.addEventListener("click", evento => {
        const boton = evento.target.closest(".btn-eliminar-rol");
        if (!boton) return;

        rolEliminar = boton.dataset.id;

        $("rolesEliminarNombre").textContent =
            boton.dataset.nombre;

        eliminarModal.hidden = false;
    });

    function cerrarEliminar() {
        eliminarModal.hidden = true;
        rolEliminar = null;
    }

    $("cancelarEliminarRol").addEventListener(
        "click",
        cerrarEliminar
    );

    eliminarModal.addEventListener("click", evento => {
        if (evento.target === eliminarModal) cerrarEliminar();
    });

    $("confirmarEliminarRol").addEventListener("click", async () => {
        if (!rolEliminar) return;

        const boton = $("confirmarEliminarRol");
        boton.disabled = true;

        try {
            const datos = await peticion(
                pagina.dataset.eliminarUrl,
                "POST",
                { id: rolEliminar }
            );

            const fila = Array.from(
                tbody.querySelectorAll(".rol-fila")
            ).find(f => f.dataset.id === String(datos.idRol));

            fila?.remove();

            cerrarEliminar();
            actualizarEstadisticas();
            filtrar();

            notificar(datos.mensaje);

        } catch (error) {
            notificar(error.message, true);
        } finally {
            boton.disabled = false;
        }
    });

    
    function mostrarCargaBoton(boton) {
        if (boton.classList.contains("is-loading")) return;

        boton.dataset.htmlOriginal = boton.innerHTML;
        boton.classList.add("is-loading");
        boton.disabled = true;

        const spinner = document.createElement("span");
        spinner.className = "roles-spinner";
        spinner.setAttribute("aria-hidden", "true");

        boton.replaceChildren(
            spinner,
            document.createTextNode(" Guardando...")
        );
    }

    function quitarCargaBoton(boton) {
        boton.classList.remove("is-loading");
        boton.disabled = false;

        if (boton.dataset.htmlOriginal) {
            boton.innerHTML = boton.dataset.htmlOriginal;
            delete boton.dataset.htmlOriginal;
        }
    }


    // ==========================================
    // CERRAR CON ESCAPE
    // ==========================================

    document.addEventListener("keydown", evento => {
        if (evento.key !== "Escape") return;

        cerrarPermisos();
        cerrarEliminar();
    });

    filtrar();
});
