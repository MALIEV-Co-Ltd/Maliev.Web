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
  },

  bindMachineFeatureHandoff: function (section, detailsPanel) {
    if (!section || !detailsPanel || section.dataset.machineHandoffBound === 'true') {
      return;
    }

    section.dataset.machineHandoffBound = 'true';

    const prefersReducedMotion = () => window.matchMedia?.('(prefers-reduced-motion: reduce)')?.matches === true;
    const stickyOffset = () => {
      const header = document.querySelector('.site-header');
      return header?.getBoundingClientRect?.().height ?? 72;
    };
    const introPanel = section.querySelector('.machine-feature-panel--intro');
    let touchStartY = 0;
    let isScrolling = false;
    let hasSnappedToDetails = false;

    const introIsActive = () => {
      if (!introPanel) {
        return false;
      }

      const rect = introPanel.getBoundingClientRect();
      return rect.top < window.innerHeight * .42 && rect.bottom > window.innerHeight * .52;
    };

    const scrollToDetails = () => {
      if (isScrolling || !introIsActive()) {
        return false;
      }

      isScrolling = true;
      hasSnappedToDetails = true;
      const top = detailsPanel.getBoundingClientRect().top + window.scrollY - stickyOffset();
      window.scrollTo({
        top: Math.max(0, top),
        behavior: prefersReducedMotion() ? 'auto' : 'smooth'
      });

      window.setTimeout(() => {
        isScrolling = false;
      }, prefersReducedMotion() ? 120 : 760);

      return true;
    };

    const introHalfPassed = () => {
      if (!introPanel || hasSnappedToDetails || isScrolling) {
        return false;
      }

      const rect = introPanel.getBoundingClientRect();
      const halfPoint = rect.top + rect.height / 2;
      return halfPoint <= window.innerHeight / 2 && rect.bottom > window.innerHeight / 2;
    };

    const scheduleHalfwayHandoff = () => {
      if (!introHalfPassed()) {
        return;
      }

      scrollToDetails();
    };

    section.addEventListener('wheel', event => {
      if (event.deltaY > 12 && scrollToDetails()) {
        event.preventDefault();
      }
    }, { passive: false });

    section.addEventListener('touchstart', event => {
      touchStartY = event.touches?.[0]?.clientY ?? 0;
    }, { passive: true });

    section.addEventListener('touchmove', event => {
      const currentY = event.touches?.[0]?.clientY ?? touchStartY;
      if (touchStartY - currentY > 28 && scrollToDetails()) {
        event.preventDefault();
      }
    }, { passive: false });

    window.addEventListener('keydown', event => {
      if (!['ArrowDown', 'PageDown', ' '].includes(event.key)) {
        return;
      }

      if (scrollToDetails()) {
        event.preventDefault();
      }
    });

    window.addEventListener('scroll', scheduleHalfwayHandoff, { passive: true });
  }
};
