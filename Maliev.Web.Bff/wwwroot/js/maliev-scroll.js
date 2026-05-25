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

  scrollToTopForNavigation: function () {
    const root = document.documentElement;
    const body = document.body;
    const state = window.__malievNavigationScrollState ?? {
      rootBehavior: root.style.scrollBehavior,
      bodyBehavior: body.style.scrollBehavior
    };

    window.__malievNavigationScrollState = state;
    window.clearTimeout(window.__malievNavigationScrollRestoreId);

    const restoreScrollBehavior = () => {
      root.style.scrollBehavior = state.rootBehavior;
      body.style.scrollBehavior = state.bodyBehavior;
      window.__malievNavigationScrollState = null;
      window.__malievNavigationScrollRestoreId = null;
    };

    root.style.scrollBehavior = 'auto';
    body.style.scrollBehavior = 'auto';
    window.scrollTo({ left: 0, top: 0, behavior: 'auto' });
    window.__malievNavigationScrollRestoreId = window.setTimeout(restoreScrollBehavior, 800);
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

  bindHeroHeaderBleed: function () {
    const root = document.documentElement;
    if (root.dataset.heroHeaderBleedBound === 'true') {
      return;
    }

    root.dataset.heroHeaderBleedBound = 'true';

    let frame = null;
    const updateHeaderState = () => {
      const header = document.querySelector('.site-header');
      if (!header) {
        root.dataset.heroHeaderBleed = 'false';
        return;
      }

      const hero = document.querySelector('.landing-hero');
      if (!hero) {
        header.classList.remove('site-header--hero-bleed');
        root.dataset.heroHeaderBleed = 'false';
        return;
      }

      const heroRect = hero.getBoundingClientRect();
      const headerHeight = header.getBoundingClientRect().height || 66;
      const heroTouchesHeader = heroRect.top <= headerHeight + 2 && heroRect.bottom > headerHeight + 24;
      const shouldBleed = window.scrollY <= 24 && heroTouchesHeader;

      header.classList.toggle('site-header--hero-bleed', shouldBleed);
      root.dataset.heroHeaderBleed = shouldBleed ? 'true' : 'false';
    };

    const scheduleUpdate = () => {
      if (frame !== null) {
        return;
      }

      frame = window.requestAnimationFrame(() => {
        frame = null;
        updateHeaderState();
      });
    };

    window.addEventListener('scroll', scheduleUpdate, { passive: true });
    window.addEventListener('resize', scheduleUpdate, { passive: true });

    if (typeof MutationObserver === 'function') {
      const observer = new MutationObserver(scheduleUpdate);
      observer.observe(document.body, {
        childList: true,
        subtree: true
      });
    }

    scheduleUpdate();
  },

  bindMachineFeatureHandoff: function (section) {
    if (!section || section.dataset.machineHandoffBound === 'true') {
      return;
    }

    section.dataset.machineHandoffBound = 'true';

    const prefersReducedMotion = () => window.matchMedia?.('(prefers-reduced-motion: reduce)')?.matches === true;
    const introTitle = section.querySelector('.machine-feature-title--reveal');

    // Reveal intro title once it enters the viewport
    if (introTitle) {
      if (prefersReducedMotion() || typeof IntersectionObserver !== 'function') {
        introTitle.classList.add('is-visible');
      } else {
        const titleObserver = new IntersectionObserver(entries => {
          if (entries.some(entry => entry.isIntersecting)) {
            introTitle.classList.add('is-visible');
            titleObserver.disconnect();
          }
        }, {
          root: null,
          threshold: 0.2,
          rootMargin: '0px 0px -8% 0px'
        });

        titleObserver.observe(introTitle);
      }
    }

    const scroller = section.closest('.machine-feature-scroller') ?? section.parentElement;

    const readHeaderOffset = () => {
      const cssOffset = window.getComputedStyle(document.documentElement).getPropertyValue('--site-header-height');
      const parsedOffset = Number.parseFloat(cssOffset);
      if (Number.isFinite(parsedOffset) && parsedOffset > 0) {
        return parsedOffset;
      }

      return document.querySelector('.site-header')?.getBoundingClientRect?.().height ?? 72;
    };

    // Set data-machine-panel from both the scroll driver and keyboard handler.
    const switchMachinePanel = nextPanel => {
      if (section.dataset.machinePanel !== nextPanel) {
        section.dataset.machinePanel = nextPanel;
      }
    };

    const readMachineScrollState = () => {
      if (!scroller) {
        return null;
      }

      const scrollSpace = scroller.offsetHeight - section.offsetHeight;
      // Not enough scroll space means the scroller is in auto-height mode.
      if (scrollSpace < 80) {
        return null;
      }

      const headerOffset = readHeaderOffset();
      const rect = scroller.getBoundingClientRect();
      const scrolled = headerOffset - rect.top;
      const progress = Math.max(0, Math.min(1, scrolled / scrollSpace));
      const stickyStart = rect.top + window.scrollY - headerOffset;
      const stickyTolerance = Math.max(32, Math.min(96, window.innerHeight * 0.1));
      const stickyIsActive = rect.top <= headerOffset + stickyTolerance && rect.bottom >= window.innerHeight - 24;

      return { headerOffset, progress, rect, scrollSpace, stickyIsActive, stickyStart };
    };

    let snapPanel = null;
    let snapTimeoutId = null;

    // Compute scroll progress through the scroller and pick the correct panel.
    // Progress 0 = section just became sticky; 1 = scroller about to scroll out.
    const updatePanel = () => {
      if (snapPanel) {
        switchMachinePanel(snapPanel);
        return;
      }

      const state = readMachineScrollState();
      if (!state) {
        return;
      }

      const nextPanel = state.progress >= 0.5 ? 'details' : 'intro';
      switchMachinePanel(nextPanel);
    };

    const snapMachinePanel = nextPanel => {
      const state = readMachineScrollState();
      if (!state) {
        return false;
      }

      switchMachinePanel(nextPanel);
      snapPanel = nextPanel;
      window.clearTimeout(snapTimeoutId);
      snapTimeoutId = window.setTimeout(() => {
        snapPanel = null;
        snapTimeoutId = null;
        updatePanel();
      }, prefersReducedMotion() ? 80 : 720);

      const targetProgress = nextPanel === 'details' ? Math.max(0, (state.scrollSpace - 48) / state.scrollSpace) : 0;
      const behavior = prefersReducedMotion() ? 'auto' : 'smooth';
      window.scrollTo({
        top: Math.max(0, state.stickyStart + targetProgress * state.scrollSpace),
        behavior
      });
      return true;
    };

    let rafId = null;
    const onScroll = () => {
      if (rafId !== null) {
        return;
      }

      rafId = window.requestAnimationFrame(() => {
        rafId = null;
        updatePanel();
      });
    };

    window.addEventListener('scroll', onScroll, { passive: true });
    window.addEventListener('resize', onScroll, { passive: true });
    window.requestAnimationFrame(updatePanel);

    const onWheel = event => {
      if (prefersReducedMotion() || Math.abs(event.deltaY) < 12 || Math.abs(event.deltaY) < Math.abs(event.deltaX)) {
        return;
      }

      const state = readMachineScrollState();
      if (!state?.stickyIsActive) {
        return;
      }

      const currentPanel = snapPanel ?? section.dataset.machinePanel ?? (state.progress >= 0.5 ? 'details' : 'intro');
      const nextPanel = event.deltaY > 0 ? 'details' : 'intro';
      if (currentPanel === nextPanel) {
        return;
      }

      event.preventDefault();
      snapMachinePanel(nextPanel);
    };

    window.addEventListener('wheel', onWheel, { passive: false });

    // Keyboard: jump to the correct scroll position when arrow/page keys are pressed
    // while the section is in its sticky zone.
    window.addEventListener('keydown', event => {
      const directionByKey = {
        ArrowDown: 1,
        PageDown: 1,
        ' ': 1,
        ArrowUp: -1,
        PageUp: -1
      };
      const direction = directionByKey[event.key];
      if (!direction || !scroller) {
        return;
      }

      const state = readMachineScrollState();
      if (!state?.stickyIsActive) {
        return;
      }

      const currentPanel = snapPanel ?? section.dataset.machinePanel ?? (state.progress >= 0.5 ? 'details' : 'intro');
      const nextPanel = direction > 0 ? 'details' : 'intro';
      if (currentPanel === nextPanel) {
        return;
      }

      snapMachinePanel(nextPanel);
      event.preventDefault();
    });
  }
};

if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', () => window.malievScroll.bindHeroHeaderBleed(), { once: true });
} else {
  window.malievScroll.bindHeroHeaderBleed();
}
