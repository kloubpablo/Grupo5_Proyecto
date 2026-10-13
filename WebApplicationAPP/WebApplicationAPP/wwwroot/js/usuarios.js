
document.addEventListener("DOMContentLoaded", () => {

    const $ = id => document.getElementById(id);
    const crear = $("crearUsuarioContainer");
    const editar = $("editarUsuarioContainer");
    const popover = $("usuariosRolePopover");
    const modal = $("usuariosEliminarModal");
    const toast = $("usuariosToast");
    const tbody = $("usuariosTableBody");

    const filas = () =>
        Array.from(tbody.querySelectorAll(".usuario-fila"));

    let filaActual = null;
    let botonRolActual = null;
    let paginaActual = 1;
    let filtradas = [];
    let toastTimer = null;

    const porPagina = 5;

    // ==========================================
    // NOTIFICACIONES
    // ==========================================

    function notificar(mensaje, tipo = "success") {
        clearTimeout(toastTimer);
        toast.textContent = mensaje;
        toast.className = "usuarios-toast " + tipo;
        toast.hidden = false;

        toastTimer = setTimeout(() => {
            toast.hidden = true;
        }, 3500);
    }

    // ==========================================
    // GAVETAS
    // ==========================================

    function cerrarGavetas() {
        crear.classList.add("closed");
        editar.classList.add("closed");
        cerrarSelects();
    }

    function abrirGaveta(elemento) {
        cerrarGavetas();
        cerrarPopover();
        elemento.classList.remove("closed");

        elemento.scrollIntoView({
            behavior: "smooth",
            block: "start"
        });
    }

    $("abrirCrearUsuario").addEventListener("click", () => {
        if (crear.classList.contains("closed")) {
            abrirGaveta(crear);
        } else {
            cerrarGavetas();
        }
    });

    ["cerrarCrearUsuario", "cancelarCrearUsuario",
     "cerrarEditarUsuario", "cancelarEditarUsuario"]
        .forEach(id => {
            $(id).addEventListener("click", cerrarGavetas);
        });

    // ==========================================
    // SELECTS PERSONALIZADOS
    // ==========================================

    function cerrarSelects(excepto = null) {
        document.querySelectorAll(".usuarios-custom-select")
            .forEach(select => {
                if (select === excepto) return;

                select.classList.remove("open");
                select.querySelector(".usuarios-select-trigger")
                    ?.setAttribute("aria-expanded", "false");
            });
    }

    function asignarSelect(input, valor) {
        if (!input) return;

        const select = input.closest(".usuarios-custom-select");
        const opciones = Array.from(
            select.querySelectorAll(".usuarios-select-option")
        );

        const opcion = opciones.find(
            o => o.dataset.value === String(valor)
        );

        input.value = opcion ? String(valor) : "";

        opciones.forEach(o => {
            o.classList.toggle("selected", o === opcion);
        });

        select.querySelector(".usuarios-select-text").textContent =
            opcion
                ? opcion.textContent.trim()
                : select.dataset.placeholder || "Seleccionar";
    }

    document.querySelectorAll(".usuarios-custom-select")
        .forEach(select => {

            const trigger = select.querySelector(
                ".usuarios-select-trigger"
            );
            const input = select.querySelector('input[type="hidden"]');

            trigger.addEventListener("click", evento => {
                evento.stopPropagation();

                const abrir = !select.classList.contains("open");
                cerrarSelects();

                select.classList.toggle("open", abrir);
                trigger.setAttribute("aria-expanded", String(abrir));
            });

            select.querySelectorAll(".usuarios-select-option")
                .forEach(opcion => {
                    opcion.addEventListener("click", evento => {
                        evento.stopPropagation();
                        asignarSelect(input, opcion.dataset.value);
                        cerrarSelects();
                    });
                });

            if (input.value) {
                asignarSelect(input, input.value);
            }
        });

    // ==========================================
    // CONTRASEÑAS
    // ==========================================

    document.querySelectorAll(".usuarios-toggle-password")
        .forEach(boton => {
            boton.addEventListener("click", () => {
                const input = $(boton.dataset.target);
                const mostrar = input.type === "password";

                input.type = mostrar ? "text" : "password";

                boton.querySelector("i").className = mostrar
                    ? "bi bi-eye-slash"
                    : "bi bi-eye";
            });
        });

    // ==========================================
    // VALIDACIONES
    // ==========================================

    function validarFormulario(form) {
        const password = form.querySelector('[name="password"]');
        const confirmar = form.querySelector(
            '[name="confirmarContraseña"]'
        );

        if (password && confirmar) {
            password.setCustomValidity("");
            confirmar.setCustomValidity("");

            if (password.value && password.value.length < 8) {
                password.setCustomValidity(
                    "La contraseña debe tener al menos 8 caracteres"
                );
            }

            if (password.value !== confirmar.value) {
                confirmar.setCustomValidity(
                    "Las contraseñas no coinciden"
                );
            }
        }

        if (!form.reportValidity()) return false;

        const rol = form.querySelector('[name="idRol"]');

        if (rol && !rol.value) {
            notificar("Debes seleccionar un rol", "error");
            return false;
        }

        return true;
    }

    document.querySelectorAll(".usuarios-form")
        .forEach(form => {
            const password = form.querySelector('[name="password"]');
            const confirmar = form.querySelector(
                '[name="confirmarContraseña"]'
            );

            [password, confirmar].forEach(input => {
                input?.addEventListener("input", () => {
                    password?.setCustomValidity("");
                    confirmar?.setCustomValidity("");
                });
            });
        });

    // ==========================================
    // AJAX
    // ==========================================

    async function enviarFormulario(form) {
        let respuesta;

        try {
            respuesta = await fetch(form.action, {
                method: "POST",
                body: new FormData(form),
                credentials: "same-origin",
                redirect: "manual",
                headers: {
                    "X-Requested-With": "XMLHttpRequest",
                    "Accept": "application/json"
                }
            });
        } catch {
            throw new Error("No se pudo conectar con el servidor");
        }

        if (respuesta.type === "opaqueredirect" ||
            (respuesta.status >= 300 && respuesta.status < 400)) {
            throw new Error(
                "El servidor redirigio la solicitud. Revisa la sesion."
            );
        }

        const tipo = respuesta.headers.get("content-type") || "";

        if (!tipo.toLowerCase().includes("application/json")) {
            if (respuesta.status === 400) {
                throw new Error(
                    "Solicitud rechazada. Revisa el token de seguridad."
                );
            }

            throw new Error(
                "El servidor no devolvio JSON. Comprueba que el controlador actualizado este ejecutandose."
            );
        }

        const datos = await respuesta.json();

        if (!respuesta.ok || !datos.ok) {
            throw new Error(
                datos.mensaje || "No se pudo completar la operacion"
            );
        }

        return datos;
    }

    async function guardar(form, alGuardar) {
        if (!validarFormulario(form)) return;

        const boton = form.querySelector('[type="submit"]');
        if (boton.disabled) return;

        boton.disabled = true;

        try {
            const datos = await enviarFormulario(form);
            alGuardar(datos);
            notificar(datos.mensaje || "Cambios guardados");
        } catch (error) {
            notificar(error.message, "error");
        } finally {
            boton.disabled = false;
        }
    }

    // ==========================================
    // CONSTRUIR FILA SEGURA
    // ==========================================

    function elemento(tag, clase, texto) {
        const nodo = document.createElement(tag);

        if (clase) nodo.className = clase;
        if (texto !== undefined) nodo.textContent = texto;

        return nodo;
    }

    function crearFila(usuario) {
        const tr = elemento("tr", "usuario-fila");

        tr.dataset.id = usuario.idUsuario;
        tr.dataset.nombre = usuario.nombre;
        tr.dataset.username = usuario.username;
        tr.dataset.correo = usuario.correoElectronico;
        tr.dataset.idrol = usuario.idRol;
        tr.dataset.rol = usuario.rolNombre;
        tr.dataset.estado = String(usuario.estado);

        const tdNombre = elemento("td");
        const persona = elemento("div", "usuarios-person");
        const avatar = elemento(
            "div",
            "usuarios-avatar",
            (usuario.nombre || "?").charAt(0).toUpperCase()
        );

        persona.append(
            avatar,
            elemento("strong", "usuario-nombre", usuario.nombre)
        );
        tdNombre.append(persona);

        const tdUsername = elemento("td");
        tdUsername.append(
            elemento("span", "usuario-username", usuario.username)
        );

        const tdCorreo = elemento("td");
        tdCorreo.append(
            elemento("span", "usuario-correo", usuario.correoElectronico)
        );

        const tdRol = elemento("td");
        const rol = elemento("span", "usuarios-role usuario-rol");
        rol.append(
            elemento("i", "bi bi-shield-check"),
            elemento("span", "", usuario.rolNombre)
        );
        tdRol.append(rol);

        const tdEstado = elemento("td");
        const estado = elemento(
            "span",
            "usuarios-status usuario-estado " +
                (usuario.estado ? "status-active" : "status-inactive")
        );

        estado.append(
            elemento("span", "status-dot"),
            elemento(
                "span",
                "status-text",
                usuario.estado ? "Activo" : "Inactivo"
            )
        );
        tdEstado.append(estado);

        const tdAcciones = elemento("td");
        const acciones = elemento("div", "usuarios-actions");

        function accion(clase, icono, texto) {
            const boton = elemento(
                "button",
                "usuarios-action-btn " + clase
            );
            boton.type = "button";
            boton.append(
                elemento("i", "bi " + icono),
                document.createTextNode(" " + texto)
            );
            return boton;
        }

        acciones.append(
            accion("action-edit btn-editar-usuario", "bi-pencil", "Editar"),
            accion("action-role btn-cambiar-rol", "bi-shield-lock", "Rol"),
            accion("action-delete btn-eliminar-usuario", "bi-trash", "Eliminar")
        );

        tdAcciones.append(acciones);

        tr.append(
            tdNombre,
            tdUsername,
            tdCorreo,
            tdRol,
            tdEstado,
            tdAcciones
        );

        return tr;
    }

    // ==========================================
    // ACTUALIZAR FILA
    // ==========================================

    function actualizarFila(fila, usuario) {
        if (!fila || !usuario) return;

        fila.dataset.nombre = usuario.nombre;
        fila.dataset.username = usuario.username;
        fila.dataset.correo = usuario.correoElectronico;
        fila.dataset.idrol = usuario.idRol;
        fila.dataset.rol = usuario.rolNombre;
        fila.dataset.estado = String(usuario.estado);

        const escribir = (selector, valor) => {
            const nodo = fila.querySelector(selector);
            if (nodo) nodo.textContent = valor;
        };

        escribir(".usuario-nombre", usuario.nombre);
        escribir(".usuario-username", usuario.username);
        escribir(".usuario-correo", usuario.correoElectronico);
        escribir(".usuario-rol span", usuario.rolNombre);
        escribir(
            ".usuarios-avatar",
            (usuario.nombre || "?").charAt(0).toUpperCase()
        );
        escribir(
            ".status-text",
            usuario.estado ? "Activo" : "Inactivo"
        );

        const estado = fila.querySelector(".usuario-estado");

        estado.classList.toggle("status-active", usuario.estado);
        estado.classList.toggle("status-inactive", !usuario.estado);

        actualizarEstadisticas();
        filtrarUsuarios(false);
    }

    function actualizarEstadisticas() {
        const todas = filas();
        const activos = todas.filter(
            f => f.dataset.estado === "true"
        ).length;

        $("totalUsuarios").textContent = todas.length;
        $("totalActivos").textContent = activos;
        $("totalInactivos").textContent = todas.length - activos;
    }

    // ==========================================
    // CREAR SIN RECARGAR
    // ==========================================

    $("formCrearUsuario").addEventListener("submit", async evento => {
        evento.preventDefault();

        const form = evento.currentTarget;

        await guardar(form, datos => {
            tbody.prepend(crearFila(datos.usuario));

            form.reset();
            asignarSelect($("crearRol"), "");

            form.querySelectorAll('input[type="password"]')
                .forEach(input => input.type = "password");

            cerrarGavetas();
            $("buscarUsuarios").value = "";
            actualizarEstadisticas();
            filtrarUsuarios();
        });
    });

    // ==========================================
    // EDITAR SIN RECARGAR
    // ==========================================

    document.addEventListener("click", evento => {
        const boton = evento.target.closest(".btn-editar-usuario");
        if (!boton) return;

        const fila = boton.closest(".usuario-fila");
        filaActual = fila;

        $("editarIdUsuario").value = fila.dataset.id;
        $("editarNombre").value = fila.dataset.nombre;
        $("editarUsername").value = fila.dataset.username;
        $("editarCorreo").value = fila.dataset.correo;

        asignarSelect($("editarRol"), fila.dataset.idrol);
        asignarSelect($("editarEstado"), fila.dataset.estado);

        $("editarPassword").value = "";
        $("editarConfirmar").value = "";

        abrirGaveta(editar);
    });

    $("formEditarUsuario").addEventListener("submit", async evento => {
        evento.preventDefault();

        const form = evento.currentTarget;
        const fila = filaActual;

        await guardar(form, datos => {
            actualizarFila(fila, datos.usuario);
            cerrarGavetas();
        });
    });

    // ==========================================
    // MENU FLOTANTE DE ROL
    // ==========================================

    function cerrarPopover() {
        popover.hidden = true;
        botonRolActual = null;
        cerrarSelects();
    }

    function posicionarPopover(boton) {
        if (!boton || popover.hidden) return;

        const rect = boton.getBoundingClientRect();
        const ancho = popover.offsetWidth;
        const alto = popover.offsetHeight;
        const margen = 12;

        const izquierda = Math.max(
            margen,
            Math.min(rect.left, window.innerWidth - ancho - margen)
        );

        let arriba = rect.bottom + 8;

        if (arriba + alto > window.innerHeight - margen) {
            arriba = rect.top - alto - 8;
        }

        arriba = Math.max(margen, arriba);

        popover.style.left = izquierda + "px";
        popover.style.top = arriba + "px";
    }

    document.addEventListener("click", evento => {
        const boton = evento.target.closest(".btn-cambiar-rol");
        if (!boton) return;

        evento.stopPropagation();

        if (!popover.hidden && botonRolActual === boton) {
            cerrarPopover();
            return;
        }

        const fila = boton.closest(".usuario-fila");
        filaActual = fila;
        botonRolActual = boton;

        cerrarGavetas();

        $("cambiarRolIdUsuario").value = fila.dataset.id;
        $("cambiarRolNombre").textContent = fila.dataset.nombre;

        asignarSelect($("cambiarRolNuevo"), fila.dataset.idrol);
        asignarSelect($("cambiarRolEstado"), fila.dataset.estado);

        popover.hidden = false;
        posicionarPopover(boton);
    });

    $("cancelarCambiarRol").addEventListener("click", cerrarPopover);

    $("formCambiarRol").addEventListener("submit", async evento => {
        evento.preventDefault();

        const form = evento.currentTarget;
        const fila = filaActual;

        await guardar(form, datos => {
            actualizarFila(fila, datos.usuario);
            cerrarPopover();
        });
    });

    // ==========================================
    // ELIMINAR SIN RECARGAR
    // ==========================================

    function cerrarModal() {
        modal.hidden = true;
    }

    document.addEventListener("click", evento => {
        const boton = evento.target.closest(".btn-eliminar-usuario");
        if (!boton) return;

        const fila = boton.closest(".usuario-fila");
        filaActual = fila;

        cerrarPopover();

        $("eliminarIdUsuario").value = fila.dataset.id;
        $("eliminarNombreUsuario").textContent = fila.dataset.nombre;

        modal.hidden = false;
    });

    $("cancelarEliminarUsuario").addEventListener("click", cerrarModal);

    modal.addEventListener("click", evento => {
        if (evento.target === modal) cerrarModal();
    });

    $("formEliminarUsuario").addEventListener("submit", async evento => {
        evento.preventDefault();

        const form = evento.currentTarget;
        const fila = filaActual;

        await guardar(form, () => {
            fila.remove();
            filaActual = null;

            cerrarModal();
            actualizarEstadisticas();
            filtrarUsuarios(false);
        });
    });

    // ==========================================
    // CERRAR MENUS
    // ==========================================

    document.addEventListener("click", evento => {
        if (!evento.target.closest(".usuarios-custom-select")) {
            cerrarSelects();
        }

        if (
            !popover.hidden &&
            !popover.contains(evento.target) &&
            !evento.target.closest(".btn-cambiar-rol")
        ) {
            cerrarPopover();
        }
    });

    document.addEventListener("keydown", evento => {
        if (evento.key !== "Escape") return;

        cerrarSelects();
        cerrarPopover();
        cerrarModal();
    });

    window.addEventListener("resize", () => {
        if (!popover.hidden && botonRolActual) {
            posicionarPopover(botonRolActual);
        }
    });

    window.addEventListener("scroll", () => {
        if (!popover.hidden && botonRolActual) {
            posicionarPopover(botonRolActual);
        }
    }, true);

    // ==========================================
    // BUSQUEDA Y PAGINACION
    // ==========================================

    const normalizar = texto =>
        String(texto ?? "")
            .normalize("NFD")
            .replace(/[\u0300-\u036f]/g, "")
            .toLowerCase()
            .trim();

    function filtrarUsuarios(reiniciar = true) {
        const busqueda = normalizar($("buscarUsuarios").value);

        filtradas = filas().filter(fila => {
            const contenido = [
                fila.dataset.id,
                fila.dataset.nombre,
                fila.dataset.username,
                fila.dataset.correo,
                fila.dataset.rol,
                fila.dataset.estado === "true" ? "activo" : "inactivo"
            ].join(" ");

            return normalizar(contenido).includes(busqueda);
        });

        if (reiniciar) paginaActual = 1;
        mostrarPagina();
    }

    function mostrarPagina() {
        const total = filtradas.length;
        const paginas = Math.ceil(total / porPagina);

        paginaActual = Math.max(
            1,
            Math.min(paginaActual, paginas || 1)
        );

        const inicio = (paginaActual - 1) * porPagina;
        const fin = inicio + porPagina;

        filas().forEach(fila => fila.hidden = true);

        filtradas.slice(inicio, fin).forEach(fila => {
            fila.hidden = false;
        });

        $("usuariosSinResultados").hidden = total !== 0;

        $("usuariosResultadosTexto").textContent =
            `${total} usuarios encontrados`;

        $("usuariosResumen").textContent = total
            ? `Mostrando ${inicio + 1}-${Math.min(fin, total)} de ${total}`
            : "";

        $("usuariosAnterior").disabled = paginaActual <= 1;
        $("usuariosSiguiente").disabled = paginaActual >= paginas;
        $("usuariosPaginacion").hidden = paginas <= 1;

        const numeros = $("usuariosNumeros");
        numeros.replaceChildren();

        for (let i = 1; i <= paginas; i++) {
            const boton = document.createElement("button");

            boton.type = "button";
            boton.className = "usuarios-page-number";
            boton.textContent = i;

            if (i === paginaActual) {
                boton.classList.add("active");
            }

            boton.addEventListener("click", () => {
                paginaActual = i;
                mostrarPagina();
            });

            numeros.appendChild(boton);
        }
    }

    $("buscarUsuarios").addEventListener("input", () => {
        filtrarUsuarios();
    });

    $("mostrarTodosUsuarios").addEventListener("click", () => {
        $("buscarUsuarios").value = "";
        filtrarUsuarios();
    });

    $("usuariosAnterior").addEventListener("click", () => {
        if (paginaActual > 1) {
            paginaActual--;
            mostrarPagina();
        }
    });

    $("usuariosSiguiente").addEventListener("click", () => {
        const paginas = Math.ceil(filtradas.length / porPagina);

        if (paginaActual < paginas) {
            paginaActual++;
            mostrarPagina();
        }
    });

    // ==========================================
    // INICIALIZACION
    // ==========================================

    filtrarUsuarios();
});
