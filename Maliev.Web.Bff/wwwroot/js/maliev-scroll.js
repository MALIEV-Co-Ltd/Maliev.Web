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

  bindWorkflowStepReveal: function (section) {
    if (!section || section.dataset.workflowRevealBound === 'true') {
      return;
    }

    section.dataset.workflowRevealBound = 'true';

    const grid = section.querySelector('.process-grid');
    if (!grid) {
      return;
    }

    let hasRevealed = false;
    let removeVisibilityListeners = () => {};

    const reveal = () => {
      if (hasRevealed) {
        return;
      }

      hasRevealed = true;
      grid.classList.add('workflow-steps-visible');
      removeVisibilityListeners();
    };

    grid.classList.add('workflow-steps-ready');

    const gridIsVisible = () => {
      const rect = grid.getBoundingClientRect();
      return rect.top < window.innerHeight * .84 && rect.bottom > window.innerHeight * .16;
    };

    const prefersReducedMotion = window.matchMedia?.('(prefers-reduced-motion: reduce)')?.matches === true;
    if (prefersReducedMotion || typeof IntersectionObserver !== 'function') {
      reveal();
      return;
    }

    const observer = new IntersectionObserver(entries => {
      if (entries.some(entry => entry.isIntersecting)) {
        reveal();
        observer.disconnect();
      }
    }, {
      root: null,
      threshold: 0.28,
      rootMargin: '0px 0px -10% 0px'
    });

    observer.observe(grid);

    const revealWhenVisible = () => {
      if (gridIsVisible()) {
        reveal();
        observer.disconnect();
      }
    };

    removeVisibilityListeners = () => {
      window.removeEventListener('scroll', revealWhenVisible);
      window.removeEventListener('resize', revealWhenVisible);
    };

    window.addEventListener('scroll', revealWhenVisible, { passive: true });
    window.addEventListener('resize', revealWhenVisible, { passive: true });
    window.requestAnimationFrame(revealWhenVisible);
  },

  bindMachineFeatureHandoff: function (section) {
    if (!section || section.dataset.machineHandoffBound === 'true') {
      return;
    }

    section.dataset.machineHandoffBound = 'true';

    const prefersReducedMotion = () => window.matchMedia?.('(prefers-reduced-motion: reduce)')?.matches === true;
    const introTitle = section.querySelector('.machine-feature-title--reveal');
    let touchStartY = 0;
    let isSwitching = false;

    const revealIntroTitle = () => {
      introTitle?.classList.add('is-visible');
    };

    if (introTitle) {
      if (prefersReducedMotion() || typeof IntersectionObserver !== 'function') {
        revealIntroTitle();
      } else {
        const titleObserver = new IntersectionObserver(entries => {
          if (entries.some(entry => entry.isIntersecting)) {
            revealIntroTitle();
            titleObserver.disconnect();
          }
        }, {
          root: null,
          threshold: 0.35,
          rootMargin: '0px 0px -12% 0px'
        });

        titleObserver.observe(introTitle);
      }
    }

    const sectionIsActive = () => {
      const rect = section.getBoundingClientRect();
      return rect.top < window.innerHeight * .72 && rect.bottom > window.innerHeight * .28;
    };

    const switchMachinePanel = direction => {
      if (isSwitching || !sectionIsActive()) {
        return false;
      }

      const currentPanel = section.dataset.machinePanel === 'details' ? 'details' : 'intro';
      const nextPanel = direction > 0 ? 'details' : 'intro';
      if (currentPanel === nextPanel) {
        return false;
      }

      isSwitching = true;
      section.dataset.machinePanel = nextPanel;
      window.setTimeout(() => {
        isSwitching = false;
      }, prefersReducedMotion() ? 80 : 560);

      return true;
    };

    section.addEventListener('wheel', event => {
      if (Math.abs(event.deltaY) > 12 && switchMachinePanel(event.deltaY)) {
        event.preventDefault();
      }
    }, { passive: false });

    section.addEventListener('touchstart', event => {
      touchStartY = event.touches?.[0]?.clientY ?? 0;
    }, { passive: true });

    section.addEventListener('touchmove', event => {
      const currentY = event.touches?.[0]?.clientY ?? touchStartY;
      const deltaY = touchStartY - currentY;
      if (Math.abs(deltaY) > 28 && switchMachinePanel(deltaY)) {
        event.preventDefault();
      }
    }, { passive: false });

    window.addEventListener('keydown', event => {
      const directionByKey = {
        ArrowDown: 1,
        PageDown: 1,
        ' ': 1,
        ArrowUp: -1,
        PageUp: -1
      };
      const direction = directionByKey[event.key];
      if (!direction) {
        return;
      }

      if (switchMachinePanel(direction)) {
        event.preventDefault();
      }
    });
  }
};
