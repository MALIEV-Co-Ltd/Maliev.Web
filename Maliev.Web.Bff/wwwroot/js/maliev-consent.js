(() => {
  const StorageKey = 'maliev.cookieConsent.v1';
  const EssentialConsent = 'essential';
  const OptionalConsent = 'optional';

  const normalize = (value) => {
    if (!value) {
      return null;
    }

    const normalized = String(value).toLowerCase();
    return normalized === OptionalConsent || normalized === EssentialConsent ? normalized : null;
  };

  const readRecord = () => {
    try {
      const stored = localStorage.getItem(StorageKey);
      if (!stored) {
        return null;
      }

      const legacyConsent = normalize(stored);
      if (legacyConsent) {
        return { level: legacyConsent, optional: legacyConsent === OptionalConsent };
      }

      const parsed = JSON.parse(stored);
      const level = normalize(parsed && parsed.level);
      return level ? { ...parsed, level, optional: level === OptionalConsent } : null;
    } catch {
      return null;
    }
  };

  const applyDocumentState = (level) => {
    const normalized = normalize(level);
    document.documentElement.dataset.cookieConsent = normalized || 'unset';
    return normalized;
  };

  window.malievConsent = {
    get: function () {
      const record = readRecord();
      return applyDocumentState(record && record.level);
    },
    set: function (level) {
      const normalized = normalize(level) || EssentialConsent;
      const record = {
        level: normalized,
        optional: normalized === OptionalConsent,
        updatedAt: new Date().toISOString(),
        version: 1
      };

      localStorage.setItem(StorageKey, JSON.stringify(record));
      applyDocumentState(normalized);
      window.dispatchEvent(new CustomEvent('maliev:cookie-consent', { detail: record }));
      return normalized;
    },
    hasOptional: function () {
      return window.malievConsent.get() === OptionalConsent;
    },
    clear: function () {
      localStorage.removeItem(StorageKey);
      return applyDocumentState(null);
    }
  };

  window.malievConsent.get();
})();
