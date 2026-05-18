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

  initComposerKeys: function (textarea, sendButton) {
    if (!textarea || !sendButton) {
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
    if (!textarea) {
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
  }
};
