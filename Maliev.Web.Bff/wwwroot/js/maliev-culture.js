window.malievCulture = {
  applyDocumentCulture: function (culture) {
    const normalized = culture && culture.toLowerCase().startsWith('th') ? 'th-TH' : 'en-US';
    document.documentElement.lang = normalized === 'th-TH' ? 'th' : 'en';
    document.documentElement.dataset.culture = normalized;
    return normalized;
  },
  applyDocumentTheme: function (theme) {
    const normalized = theme && theme.toLowerCase() === 'dark' ? 'dark' : 'light';
    document.documentElement.dataset.theme = normalized;
    document.documentElement.style.colorScheme = normalized;
    return normalized;
  },
  preferredSystemTheme: function () {
    return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  },
  isThaiRegionSignal: function (language) {
    if (!language) {
      return false;
    }

    const normalized = language.toLowerCase();
    if (normalized.startsWith('th')) {
      return true;
    }

    try {
      return new Intl.Locale(language).region === 'TH';
    } catch {
      return normalized.endsWith('-th');
    }
  },
  resolveCulture: function (fallback) {
    const stored = localStorage.getItem('maliev.culture');
    if (stored) {
      return window.malievCulture.applyDocumentCulture(stored);
    }

    const cookie = document.cookie
      .split('; ')
      .find((row) => row.startsWith('maliev.culture='));
    if (cookie) {
      return window.malievCulture.applyDocumentCulture(decodeURIComponent(cookie.split('=')[1]));
    }

    const languages = navigator.languages || [navigator.language || fallback];
    const browserCulture = languages.find((language) => window.malievCulture.isThaiRegionSignal(language));
    if (browserCulture) {
      return window.malievCulture.applyDocumentCulture('th-TH');
    }

    if (Intl.DateTimeFormat().resolvedOptions().timeZone === 'Asia/Bangkok') {
      return window.malievCulture.applyDocumentCulture('th-TH');
    }

    return window.malievCulture.applyDocumentCulture(fallback || 'en-US');
  },
  setCulture: function (culture) {
    const normalized = window.malievCulture.applyDocumentCulture(culture);
    localStorage.setItem('maliev.culture', normalized);
    document.cookie = `maliev.culture=${encodeURIComponent(normalized)};path=/;max-age=31536000;samesite=lax`;
    document.cookie = `.AspNetCore.Culture=${encodeURIComponent(`c=${normalized}|uic=${normalized}`)};path=/;max-age=31536000;samesite=lax`;
  },
  resolveTheme: function (fallback) {
    const stored = localStorage.getItem('maliev.theme');
    if (stored) {
      return window.malievCulture.applyDocumentTheme(stored);
    }

    const cookie = document.cookie
      .split('; ')
      .find((row) => row.startsWith('maliev.theme='));
    if (cookie) {
      return window.malievCulture.applyDocumentTheme(decodeURIComponent(cookie.split('=')[1]));
    }

    return window.malievCulture.applyDocumentTheme(fallback || window.malievCulture.preferredSystemTheme());
  },
  setTheme: function (theme) {
    const normalized = window.malievCulture.applyDocumentTheme(theme);
    localStorage.setItem('maliev.theme', normalized);
    document.cookie = `maliev.theme=${encodeURIComponent(normalized)};path=/;max-age=31536000;samesite=lax`;
  },
  getCulture: function () {
    return localStorage.getItem('maliev.culture') || 'en-US';
  },
  getTheme: function () {
    return localStorage.getItem('maliev.theme') || window.malievCulture.preferredSystemTheme();
  },
  saveDraft: function (key, value) {
    localStorage.setItem(key, value);
  },
  readDraft: function (key) {
    return localStorage.getItem(key);
  }
};
