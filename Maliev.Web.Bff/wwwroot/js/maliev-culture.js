window.malievCulture = {
  applyDocumentCulture: function (culture) {
    const normalized = culture && culture.toLowerCase().startsWith('th') ? 'th-TH' : 'en-US';
    document.documentElement.lang = normalized === 'th-TH' ? 'th' : 'en';
    document.documentElement.dataset.culture = normalized;
    return normalized;
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
    const browserCulture = languages.find((language) => language && language.toLowerCase().startsWith('th'));
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
  getCulture: function () {
    return localStorage.getItem('maliev.culture') || 'en-US';
  },
  saveDraft: function (key, value) {
    localStorage.setItem(key, value);
  },
  readDraft: function (key) {
    return localStorage.getItem(key);
  }
};
