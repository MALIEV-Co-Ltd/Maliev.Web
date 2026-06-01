const malievChatbotBehavior = (() => {
  const storageKey = 'maliev.chatbot.behavior.v1';
  const maxEvents = 24;
  const maxSections = 8;
  const maxActiveContexts = 6;
  const minInputLength = 2;
  let initialized = false;
  let observer = null;
  let currentSection = null;
  let currentSectionPath = null;
  let currentSectionStartedAt = 0;
  let scrollFrame = 0;
  let inputTimer = 0;

  const now = () => Date.now();
  const pageKey = () => `${window.location.pathname || '/'}${window.location.search || ''}`;

  const clean = (value, maxLength) => {
    if (!value) {
      return null;
    }

    const text = String(value).replace(/\s+/g, ' ').trim();
    if (!text) {
      return null;
    }

    return text.length > maxLength ? text.slice(0, maxLength).trim() : text;
  };

  const collectActiveContexts = () => {
    const contexts = Array.from(document.querySelectorAll('[data-chatbot-active-context]'))
      .map(element => clean(element.dataset?.chatbotActiveContext, 120))
      .filter(Boolean);

    return Array.from(new Set(contexts)).slice(0, maxActiveContexts);
  };

  const createState = () => ({
    path: pageKey(),
    pageStartedAt: now(),
    lastSeenAt: now(),
    maxScrollPercent: 0,
    activeContexts: collectActiveContexts(),
    events: [],
    sections: []
  });

  const readState = () => {
    let state = null;
    try {
      const stored = window.sessionStorage.getItem(storageKey);
      state = stored ? JSON.parse(stored) : null;
    } catch {
      state = null;
    }

    if (!state || state.path !== pageKey()) {
      return createState();
    }

    state.events = Array.isArray(state.events) ? state.events.slice(0, maxEvents) : [];
    state.sections = Array.isArray(state.sections) ? state.sections.slice(0, maxSections) : [];
    state.activeContexts = collectActiveContexts();
    state.maxScrollPercent = Number.isFinite(state.maxScrollPercent) ? state.maxScrollPercent : 0;
    return state;
  };

  const writeState = state => {
    state.lastSeenAt = now();
    try {
      window.sessionStorage.setItem(storageKey, JSON.stringify(state));
    } catch {
    }
  };

  const addSectionDwell = (label, dwellMs) => {
    const cleaned = clean(label, 90);
    if (!cleaned || dwellMs < 700) {
      return;
    }

    const state = readState();
    const dwellSeconds = Math.max(1, Math.round(dwellMs / 1000));
    const existing = state.sections.find(section => section.label === cleaned);
    if (existing) {
      existing.dwellSeconds = Math.min(999, (existing.dwellSeconds || 0) + dwellSeconds);
      existing.lastSeenAt = new Date().toISOString();
    } else {
      state.sections.unshift({
        label: cleaned,
        dwellSeconds,
        lastSeenAt: new Date().toISOString()
      });
    }

    state.sections = state.sections
      .sort((left, right) => (right.dwellSeconds || 0) - (left.dwellSeconds || 0))
      .slice(0, maxSections);
    writeState(state);
  };

  const finalizeCurrentSection = () => {
    if (!currentSection || !currentSectionStartedAt) {
      return;
    }

    if (currentSectionPath !== pageKey()) {
      currentSection = null;
      currentSectionPath = null;
      currentSectionStartedAt = 0;
      return;
    }

    const startedAt = currentSectionStartedAt;
    currentSectionStartedAt = now();
    addSectionDwell(currentSection, now() - startedAt);
  };

  const activateSection = label => {
    const cleaned = clean(label, 90);
    if (!cleaned || cleaned === currentSection) {
      return;
    }

    finalizeCurrentSection();
    currentSection = cleaned;
    currentSectionPath = pageKey();
    currentSectionStartedAt = now();
  };

  const labelForElement = element => {
    if (!element) {
      return null;
    }

    const explicit = clean(element.dataset?.chatbotIntentLabel || element.dataset?.screenLabel || element.getAttribute?.('aria-label'), 90);
    if (explicit) {
      return explicit;
    }

    const labelledBy = element.getAttribute?.('aria-labelledby');
    if (labelledBy) {
      const labelElement = document.getElementById(labelledBy.split(/\s+/)[0]);
      const label = clean(labelElement?.textContent, 90);
      if (label) {
        return label;
      }
    }

    const heading = element.querySelector?.('h1, h2, h3, h4, strong, .card-meta, .material-filter-label, .quote-config-label');
    return clean(heading?.textContent || element.textContent, 90);
  };

  const recordEvent = (kind, label, detail) => {
    const cleanedKind = clean(kind, 64);
    const cleanedLabel = clean(label, 120);
    if (!cleanedKind || !cleanedLabel) {
      return null;
    }

    const state = readState();
    const event = {
      kind: cleanedKind,
      label: cleanedLabel,
      detail: clean(detail, 180),
      path: pageKey(),
      occurredAt: new Date().toISOString()
    };

    state.events.unshift(event);
    state.events = state.events.slice(0, maxEvents);
    writeState(state);
    return event;
  };

  const classifyClick = target => {
    const explicit = target.closest?.('[data-chatbot-intent]');
    if (explicit) {
      return {
        kind: explicit.dataset.chatbotIntent,
        label: explicit.dataset.chatbotIntentLabel || labelForElement(explicit)
      };
    }

    const quoteFormats = target.closest?.('.landing-quote-dropzone-format-button');
    if (quoteFormats) {
      return { kind: 'quote_format_check', label: 'Checked supported quote upload formats' };
    }

    const quoteDropzone = target.closest?.('.landing-quote-dropzone, .final-dropzone, .machine-configure-button');
    if (quoteDropzone) {
      return { kind: 'quote_upload_intent', label: labelForElement(quoteDropzone) || 'Opened quote upload route' };
    }

    const quoteLink = target.closest?.('a[href*="quote"], a[href*="quotes/new"], .service-page-hero .button.primary, .section-link[href*="quote"]');
    if (quoteLink) {
      return { kind: 'quote_route_interest', label: labelForElement(quoteLink) || 'Opened quote route' };
    }

    const serviceTab = target.closest?.('.home-services-tabs button, .home-services-cta, .service-card, .industry-sector-card');
    if (serviceTab) {
      return { kind: 'service_comparison_intent', label: labelForElement(serviceTab) || 'Compared service options' };
    }

    const workflowStep = target.closest?.('.process-grid button, [data-workflow-accent], [data-machine-variant], .machine-addon-copy');
    if (workflowStep) {
      return { kind: 'workflow_review_intent', label: labelForElement(workflowStep) || 'Reviewed manufacturing workflow' };
    }

    const materialControl = target.closest?.('.material-filter-row button, .material-mobile-card-heading button, .material-comparison-table button, .material-clear-selection');
    if (materialControl) {
      return { kind: 'material_comparison_intent', label: labelForElement(materialControl) || 'Compared materials' };
    }

    const shopControl = target.closest?.('.shop-collection-card, .product-card button, .shop-feature-action, .product-detail .button.primary');
    if (shopControl) {
      return { kind: 'shop_purchase_intent', label: labelForElement(shopControl) || 'Reviewed shop product' };
    }

    const contactControl = target.closest?.('.contact-submit-button, .contact-upload-panel, .contact-phone-reveal, .line-contact-link, .contact-map-actions button');
    if (contactControl) {
      return { kind: 'contact_support_intent', label: labelForElement(contactControl) || 'Reviewed contact options' };
    }

    return null;
  };

  const updateScrollDepth = () => {
    scrollFrame = 0;
    const state = readState();
    const scrollable = Math.max(1, document.documentElement.scrollHeight - window.innerHeight);
    const percent = Math.round(Math.min(100, Math.max(0, (window.scrollY / scrollable) * 100)));
    if (percent > state.maxScrollPercent) {
      state.maxScrollPercent = percent;
      writeState(state);
    }
  };

  const bindSections = () => {
    if (observer) {
      observer.disconnect();
      observer = null;
    }

    if (typeof IntersectionObserver !== 'function') {
      return;
    }

    const sections = Array.from(document.querySelectorAll('section, [data-screen-label]'))
      .filter(section => !section.closest('.customer-chatbot'));
    if (sections.length === 0) {
      return;
    }

    observer = new IntersectionObserver(entries => {
      const visible = entries
        .filter(entry => entry.isIntersecting && entry.intersectionRatio >= 0.45)
        .sort((left, right) => right.intersectionRatio - left.intersectionRatio)[0];
      if (visible) {
        activateSection(labelForElement(visible.target));
      }
    }, { threshold: [0.45, 0.65, 0.85] });

    sections.forEach(section => observer.observe(section));
  };

  const bind = () => {
    if (initialized) {
      return;
    }

    initialized = true;
    document.addEventListener('click', event => {
      const intent = classifyClick(event.target);
      if (intent) {
        recordEvent(intent.kind, intent.label);
      }
    }, true);

    document.addEventListener('change', event => {
      const input = event.target;
      if (input?.matches?.('.landing-quote-dropzone-input') && input.files?.length) {
        recordEvent('quote_files_selected', `${input.files.length} quote file${input.files.length === 1 ? '' : 's'} selected`);
      }
    }, true);

    document.addEventListener('input', event => {
      const input = event.target;
      if (!input?.matches?.('.shop-search input') || (input.value || '').trim().length < minInputLength) {
        return;
      }

      window.clearTimeout(inputTimer);
      inputTimer = window.setTimeout(() => {
        recordEvent('shop_search_intent', `Searched shop for "${clean(input.value, 60)}"`);
      }, 600);
    }, true);

    window.addEventListener('scroll', () => {
      if (scrollFrame) {
        return;
      }

      scrollFrame = window.requestAnimationFrame(updateScrollDepth);
    }, { passive: true });

    window.addEventListener('beforeunload', finalizeCurrentSection);
  };

  const snapshot = () => {
    finalizeCurrentSection();
    updateScrollDepth();
    const state = readState();
    state.pageDwellSeconds = Math.max(0, Math.round((now() - (state.pageStartedAt || now())) / 1000));
    writeState(state);
    return state;
  };

  return {
    init: () => {
      bind();
      bindSections();
      writeState(readState());
      return snapshot();
    },
    refresh: () => {
      finalizeCurrentSection();
      writeState(readState());
      currentSection = null;
      currentSectionPath = null;
      currentSectionStartedAt = 0;
      bindSections();
      return snapshot();
    },
    snapshot,
    recordEvent
  };
})();

const isElement = value => value && value.nodeType === 1;

window.malievChatbot = {
  getJson: async function (path) {
    if (!path || typeof path !== 'string' || !path.startsWith('/')) {
      return null;
    }

    try {
      const response = await fetch(path, {
        method: 'GET',
        credentials: 'include',
        headers: {
          'Accept': 'application/json'
        }
      });

      if (!response.ok) {
        return null;
      }

      return await response.json();
    } catch {
      return null;
    }
  },

  postJson: async function (path, payload, timeoutMs) {
    if (!path || typeof path !== 'string' || !path.startsWith('/')) {
      return JSON.stringify({
        ok: false,
        status: 400,
        error: 'Invalid request path.'
      });
    }

    const timeout = Number.isFinite(timeoutMs) && timeoutMs > 0 ? timeoutMs : 45000;
    const controller = typeof AbortController === 'function' ? new AbortController() : null;
    const timeoutId = controller
      ? window.setTimeout(() => controller.abort(), timeout)
      : null;

    try {
      const response = await fetch(path, {
        method: 'POST',
        credentials: 'include',
        headers: {
          'Accept': 'application/json',
          'Content-Type': 'application/json'
        },
        signal: controller ? controller.signal : undefined,
        body: JSON.stringify(payload || {})
      });
      const body = await response.text();

      return JSON.stringify({
        ok: response.ok,
        status: response.status,
        body
      });
    } catch (error) {
      const timedOut = error && error.name === 'AbortError';
      return JSON.stringify({
        ok: false,
        status: timedOut ? 408 : 0,
        error: timedOut
          ? 'The assistant response timed out.'
          : (error && error.message ? error.message : 'Request failed.')
      });
    } finally {
      if (timeoutId) {
        window.clearTimeout(timeoutId);
      }
    }
  },

  openSignInPopup: function (url) {
    if (!url || typeof url !== 'string' || !url.startsWith('/')) {
      return false;
    }

    const width = 520;
    const height = 720;
    const left = Math.max(0, Math.round((window.screen.width - width) / 2));
    const top = Math.max(0, Math.round((window.screen.height - height) / 2));
    const popup = window.open(
      url,
      'maliev-chatbot-signin',
      `popup=yes,width=${width},height=${height},left=${left},top=${top},resizable=yes,scrollbars=yes`
    );

    if (!popup) {
      return false;
    }

    popup.focus();
    return true;
  },

  notifyAuthenticationComplete: function () {
    try {
      localStorage.setItem('maliev.chatbot.auth.completedAt', Date.now().toString());
    } catch {
    }

    try {
      if (window.opener && window.opener !== window) {
        window.opener.postMessage({ type: 'maliev.chatbot.authenticated' }, window.location.origin);
      }
    } catch {
    }

    window.setTimeout(() => {
      try {
        window.close();
      } catch {
      }
    }, 500);
  },

  readSharedSessionId: function (storageKey) {
    const key = storageKey || 'maliev.customerAssistant.session.v1';

    try {
      const stored = localStorage.getItem(key);
      if (stored) {
        const parsed = JSON.parse(stored);
        if (parsed && parsed.sessionId) {
          return parsed.sessionId;
        }
      }
    } catch {
    }

    try {
      const cookie = document.cookie
        .split('; ')
        .find(row => row.startsWith('maliev_customer_assistant_session='));
      if (!cookie) {
        return null;
      }

      const parsed = JSON.parse(decodeURIComponent(cookie.split('=').slice(1).join('=')));
      return parsed && parsed.sessionId ? parsed.sessionId : null;
    } catch {
      return null;
    }
  },

  writeSharedSession: function (storageKey, sessionId, userKey, language, isAuthenticated) {
    if (!sessionId) {
      return;
    }

    const key = storageKey || 'maliev.customerAssistant.session.v1';
    const payload = {
      sessionId,
      userKey: userKey || null,
      language: language || 'en',
      isAuthenticated: !!isAuthenticated,
      updatedAt: new Date().toISOString()
    };

    try {
      localStorage.setItem(key, JSON.stringify(payload));
    } catch {
    }

    try {
      const secure = window.location.protocol === 'https:' ? '; Secure' : '';
      const domain = window.location.hostname.endsWith('.maliev.com') ? '; Domain=.maliev.com' : '';
      document.cookie = `maliev_customer_assistant_session=${encodeURIComponent(JSON.stringify(payload))}; Path=/; Max-Age=2592000; SameSite=Lax${secure}${domain}`;
    } catch {
    }
  },

  clearSharedSession: function (storageKey) {
    const key = storageKey || 'maliev.customerAssistant.session.v1';

    try {
      localStorage.removeItem(key);
    } catch {
    }

    try {
      const secure = window.location.protocol === 'https:' ? '; Secure' : '';
      const domain = window.location.hostname.endsWith('.maliev.com') ? '; Domain=.maliev.com' : '';
      document.cookie = `maliev_customer_assistant_session=; Path=/; Max-Age=0; SameSite=Lax${secure}${domain}`;
    } catch {
    }
  },

  initBehaviorTracker: function () {
    return JSON.stringify(malievChatbotBehavior.init());
  },

  refreshBehaviorTracker: function () {
    return JSON.stringify(malievChatbotBehavior.refresh());
  },

  readBehaviorSnapshot: function () {
    return JSON.stringify(malievChatbotBehavior.snapshot());
  },

  recordBehaviorEvent: function (kind, label, detail) {
    return JSON.stringify(malievChatbotBehavior.recordEvent(kind, label, detail));
  },

  initComposerKeys: function (textarea, sendButton) {
    if (!isElement(textarea) || !isElement(sendButton)
      || typeof textarea.addEventListener !== 'function'
      || typeof textarea.removeEventListener !== 'function'
      || typeof sendButton.click !== 'function') {
      return;
    }

    if (textarea.__malievChatbotKeydown) {
      textarea.removeEventListener('keydown', textarea.__malievChatbotKeydown);
    }

    textarea.__malievChatbotKeydown = event => {
      if (event.key !== 'Enter' || event.shiftKey || event.altKey || event.ctrlKey || event.metaKey || event.isComposing) {
        return;
      }

      event.preventDefault();
      if (!sendButton.disabled) {
        sendButton.click();
      }
    };
    textarea.addEventListener('keydown', textarea.__malievChatbotKeydown);
  },

  fitComposer: function (textarea) {
    if (!isElement(textarea) || typeof textarea.style !== 'object') {
      return;
    }

    const styles = window.getComputedStyle(textarea);
    const lineHeight = Number.parseFloat(styles.lineHeight) || 20;
    const padding = (Number.parseFloat(styles.paddingTop) || 0) + (Number.parseFloat(styles.paddingBottom) || 0);
    const border = (Number.parseFloat(styles.borderTopWidth) || 0) + (Number.parseFloat(styles.borderBottomWidth) || 0);
    const minHeight = Math.ceil(lineHeight + padding + border);
    const maxHeight = Math.ceil(lineHeight * 5 + padding + border);

    if (!textarea.value) {
      textarea.style.height = `${minHeight}px`;
      textarea.style.overflowY = 'hidden';
      return;
    }

    textarea.style.height = 'auto';
    const nextHeight = Math.min(textarea.scrollHeight + border, maxHeight);
    textarea.style.height = `${Math.max(minHeight, nextHeight)}px`;
    textarea.style.overflowY = textarea.scrollHeight + border > maxHeight ? 'auto' : 'hidden';
  },

  isNearBottom: function (container) {
    if (!container) {
      return true;
    }

    const threshold = 24;
    return container.scrollHeight - container.scrollTop - container.clientHeight <= threshold;
  },

  scrollToBottom: function (container, smooth) {
    if (!container) {
      return;
    }

    window.requestAnimationFrame(() => {
      container.scrollTo({
        top: container.scrollHeight,
        behavior: smooth ? 'smooth' : 'auto'
      });
    });
  },

  initFooterAwareFloat: function () {
    const chatbot = document.querySelector('.customer-chatbot');
    if (!chatbot || chatbot.dataset.footerAwareBound === 'true') {
      return;
    }

    chatbot.dataset.footerAwareBound = 'true';

    let frame = 0;
    const clearance = 14;
    const maxLift = 132;

    const getCurrentLift = () => {
      const value = window.getComputedStyle(chatbot).getPropertyValue('--customer-chatbot-footer-lift');
      const parsed = Number.parseFloat(value);
      return Number.isFinite(parsed) ? Math.max(0, parsed) : 0;
    };

    const update = () => {
      frame = 0;

      const toggle = chatbot.querySelector('.customer-chatbot-toggle') || chatbot;
      const toggleRect = toggle.getBoundingClientRect();
      const currentLift = getCurrentLift();
      const naturalRect = {
        top: toggleRect.top + currentLift,
        right: toggleRect.right,
        bottom: toggleRect.bottom + currentLift,
        left: toggleRect.left
      };
      const legalTargets = document.querySelectorAll('.footer-legal-links a, .footer-legal-links button');
      let lift = 0;

      legalTargets.forEach(target => {
        const targetRect = target.getBoundingClientRect();
        if (targetRect.width <= 0 || targetRect.height <= 0 || targetRect.top >= window.innerHeight || targetRect.bottom <= 0) {
          return;
        }

        const horizontalOverlap = naturalRect.left - clearance < targetRect.right
          && naturalRect.right + clearance > targetRect.left;
        const verticalOverlap = naturalRect.top - clearance < targetRect.bottom
          && naturalRect.bottom + clearance > targetRect.top;
        if (!horizontalOverlap || !verticalOverlap) {
          return;
        }

        lift = Math.max(lift, Math.ceil(naturalRect.bottom + clearance - targetRect.top));
      });

      chatbot.style.setProperty('--customer-chatbot-footer-lift', `${Math.min(maxLift, Math.max(0, lift))}px`);
    };

    const schedule = () => {
      if (frame) {
        return;
      }

      frame = window.requestAnimationFrame(update);
    };

    window.addEventListener('scroll', schedule, { passive: true });
    window.addEventListener('resize', schedule);
    schedule();
  }
};
