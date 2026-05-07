window.malievCulture = {
  resolveCulture: function (fallback) {
    const stored = localStorage.getItem('maliev.culture');
    if (stored) {
      return stored;
    }

    const cookie = document.cookie
      .split('; ')
      .find((row) => row.startsWith('maliev.culture='));
    if (cookie) {
      return decodeURIComponent(cookie.split('=')[1]);
    }

    const languages = navigator.languages || [navigator.language || fallback];
    const browserCulture = languages.find((language) => language && language.toLowerCase().startsWith('th'));
    if (browserCulture) {
      return 'th-TH';
    }

    if (Intl.DateTimeFormat().resolvedOptions().timeZone === 'Asia/Bangkok') {
      return 'th-TH';
    }

    return fallback || 'en-US';
  },
  setCulture: function (culture) {
    localStorage.setItem('maliev.culture', culture);
    document.cookie = `maliev.culture=${encodeURIComponent(culture)};path=/;max-age=31536000;samesite=lax`;
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
