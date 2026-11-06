/* Yampi: menu por espacio real. No clona enlaces ni modifica permisos. */
(() => {
  'use strict';
  document.addEventListener('DOMContentLoaded', () => {
    const nav = document.getElementById('nav');
    const brand = nav?.querySelector('.yampi-nav-brand');
    const primary = document.getElementById('yampiPrimaryNav');
    const actions = nav?.querySelector('.yampi-priority-actions');
    const moreWrap = document.getElementById('yampiMoreWrap');
    const moreToggle = document.getElementById('yampiMenuToggle');
    const moreMenu = document.getElementById('yampiMenu');
    const backdrop = document.getElementById('yampiNavBackdrop');
    const isPhone = () => window.matchMedia('(max-width: 767px)').matches;
    const profileToggle = document.getElementById('yampiProfileToggle');
    const profileMenu = document.getElementById('yampiProfileMenu');
    const controlsToggle = document.getElementById('yampiControlsToggle');
    const controlsMenu = document.getElementById('yampiControlsMenu');
    if (!nav || !brand || !primary || !actions || !moreWrap || !moreToggle || !moreMenu) return;

    // Los elementos llegan del servidor filtrados por rol. Mover, nunca clonar.
    const items = Array.from(primary.children);
    let pending = false;
    const setOpen = (button, panel, open) => {
      if (!button || !panel) return;
      panel.hidden = !open;
      button.setAttribute('aria-expanded', String(open));
    };
    const setMobileOverlay = open => {
      if (backdrop) backdrop.hidden = !open;
      document.body.classList.toggle('yampi-mobile-nav-open', Boolean(open && isPhone()));
    };
    const closeMore = () => {
      setOpen(moreToggle, moreMenu, false);
      setMobileOverlay(false);
    };
    const closeProfile = () => setOpen(profileToggle, profileMenu, false);
    const closeControls = () => setOpen(controlsToggle, controlsMenu, false);
    const closeAll = () => { closeControls(); closeMore(); closeProfile(); };
    const value = s => parseFloat(s) || 0;
    const widthWithMargins = element => {
      const style = getComputedStyle(element);
      return element.getBoundingClientRect().width + value(style.marginLeft) + value(style.marginRight);
    };
    const primaryWidth = () => {
      const gap = value(getComputedStyle(primary).columnGap);
      const children = Array.from(primary.children);
      return children.reduce((sum, child) => sum + widthWithMargins(child), 0)
        + Math.max(0, children.length - 1) * gap;
    };
    const availableWidth = () => {
      const style = getComputedStyle(nav);
      const contentWidth = nav.clientWidth - value(style.paddingLeft) - value(style.paddingRight);
      const gap = value(style.columnGap);
      // margin-left:auto de acciones es espacio flexible, NO ancho reservado.
      return Math.max(0, contentWidth - brand.getBoundingClientRect().width - actions.getBoundingClientRect().width - gap * 2 - 6);
    };

    function relayout() {
      closeAll();
      // Empezar siempre con el estado original para que el menu no quede atascado.
      moreWrap.hidden = true;
      items.forEach(item => primary.appendChild(item));

      // En telefono siempre hay hamburguesa y todos los accesos van al lateral.
      // Las opciones ya llegan del servidor filtradas segun el rol y los permisos.
      if (isPhone()) {
        items.forEach(item => moreMenu.appendChild(item));
        moreWrap.hidden = false;
        return;
      }

      // En escritorio y laptop conservar el overflow inteligente actual.
      if (primaryWidth() <= availableWidth()) return;
      moreWrap.hidden = false; // Ahora el calculo ya reserva espacio para la hamburguesa.
      for (let i = items.length - 1; i >= 0 && primaryWidth() > availableWidth(); i--) {
        moreMenu.prepend(items[i]);
      }
      if (!moreMenu.children.length) moreWrap.hidden = true;
    }
    const schedule = () => {
      if (pending) return;
      pending = true;
      requestAnimationFrame(() => { pending = false; relayout(); });
    };

    moreToggle.addEventListener('click', e => {
      e.stopPropagation();
      const open = moreMenu.hidden;
      closeAll();
      setOpen(moreToggle, moreMenu, open);
      if (isPhone()) setMobileOverlay(open);
    });
    backdrop?.addEventListener('click', closeAll);
    profileToggle?.addEventListener('click', e => {
      e.stopPropagation();
      const open = profileMenu.hidden;
      closeAll();
      setOpen(profileToggle, profileMenu, open);
    });
    controlsToggle?.addEventListener('click', e => {
      e.stopPropagation();
      const open = controlsMenu.hidden;
      closeControls();
      closeProfile();
      // No cerrar hamburguesa si este submenu esta dentro de ella.
      if (!controlsToggle.closest('#yampiMenu')) closeMore();
      setOpen(controlsToggle, controlsMenu, open);
    });
    document.addEventListener('click', e => {
      if (!(e.target instanceof Element)) return;
      if (!e.target.closest('.yampi-more-wrap')) closeMore();
      if (!e.target.closest('.yampi-profile-slot')) closeProfile();
      if (!e.target.closest('.yampi-controls-item')) closeControls();
    });
    document.addEventListener('keydown', e => {
      if (e.key !== 'Escape') return;
      const focus = !controlsMenu?.hidden ? controlsToggle
        : !moreMenu.hidden ? moreToggle
        : !profileMenu?.hidden ? profileToggle : null;
      closeAll();
      focus?.focus();
    });
    moreMenu.addEventListener('click', e => {
      if (e.target.closest('a')) closeAll();
    });
    window.addEventListener('resize', schedule, {passive:true});
    window.addEventListener('pageshow', schedule);
    if (document.fonts?.ready) document.fonts.ready.then(schedule);
    if (typeof ResizeObserver !== 'undefined') {
      const observer = new ResizeObserver(schedule);
      observer.observe(nav);
    }
    relayout();
  });
})();
