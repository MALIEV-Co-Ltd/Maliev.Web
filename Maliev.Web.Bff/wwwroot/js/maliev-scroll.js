window.malievScroll = {
  scrollIntoView: function (element, offset) {
    if (!element || typeof element.getBoundingClientRect !== 'function') {
      return;
    }

    const stickyOffset = Number.isFinite(offset) ? offset : 92;
    const prefersReducedMotion = window.matchMedia?.('(prefers-reduced-motion: reduce)')?.matches === true;
    const top = element.getBoundingClientRect().top + window.scrollY - stickyOffset;

    window.scrollTo({
      top: Math.max(0, top),
      behavior: prefersReducedMotion ? 'auto' : 'smooth'
    });
  }
};
