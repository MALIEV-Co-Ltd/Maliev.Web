(() => {
  const libraryTimeoutMs = 10000;

  const waitForGoogleIdentity = async () => {
    const deadline = Date.now() + libraryTimeoutMs;
    while (Date.now() < deadline) {
      if (window.google?.accounts?.id) {
        return window.google.accounts.id;
      }
      await new Promise(resolve => window.setTimeout(resolve, 50));
    }

    throw new Error('Google Identity Services did not load.');
  };

  const readBody = async response => {
    const text = await response.text();
    if (!text) {
      return {};
    }

    try {
      return JSON.parse(text);
    } catch {
      return {};
    }
  };

  const postJson = async (path, payload) => {
    const response = await fetch(path, {
      method: 'POST',
      credentials: 'same-origin',
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(payload)
    });

    return {
      ok: response.ok,
      status: response.status,
      body: await readBody(response)
    };
  };

  const showStatus = (root, message, isError) => {
    const status = root.querySelector('.auth-google-official-status');
    if (!status) {
      return;
    }

    status.textContent = message || '';
    status.hidden = !message;
    status.classList.toggle('is-error', Boolean(isError));
  };

  const fail = (root, message) => {
    root.dataset.googleIdentityState = 'error';
    showStatus(root, message || 'Google sign-in is unavailable. Continue with email instead.', true);
  };

  const exchangeCredential = async (root, flow, credentialResponse) => {
    const credential = credentialResponse?.credential;
    if (!credential) {
      fail(root, 'Google did not return a sign-in credential. Reload this page and try again.');
      return;
    }

    root.dataset.googleIdentityState = 'exchanging';
    showStatus(root, 'Finishing secure sign-in…', false);

    try {
      const result = await postJson('/auth/google/exchange', {
        credential,
        nonce: flow.nonce,
        flowId: flow.flowId
      });

      if (!result.ok) {
        fail(
          root,
          result.body?.detail || (result.status === 409
            ? 'This email already has a MALIEV account. Sign in with email and password first.'
            : 'Google sign-in could not be completed. Reload this page and try again.'));
        return;
      }

      if (!result.body?.redirectUrl || typeof result.body.redirectUrl !== 'string') {
        fail(root, 'MALIEV could not finish your session. Reload this page and try again.');
        return;
      }

      window.location.assign(result.body.redirectUrl);
    } catch {
      fail(root, 'Google sign-in is temporarily unavailable. Continue with email or try again shortly.');
    }
  };

  const renderButton = async (root, returnUrl) => {
    if (!root || root.dataset.googleIdentityState === 'loading') {
      return;
    }

    const buttonHost = root.querySelector('.auth-google-official-button');
    if (!buttonHost) {
      return;
    }

    root.dataset.googleIdentityState = 'loading';
    showStatus(root, 'Loading secure Google sign-in…', false);

    try {
      const [, nonceResult] = await Promise.all([
        waitForGoogleIdentity(),
        postJson('/auth/google/nonce', { returnUrl: returnUrl || '/account' })
      ]);

      if (!nonceResult.ok || !nonceResult.body?.clientId || !nonceResult.body?.nonce || !nonceResult.body?.flowId) {
        fail(root, nonceResult.body?.detail || 'Google sign-in is unavailable. Continue with email instead.');
        return;
      }

      const flow = nonceResult.body;
      google.accounts.id.initialize({
        client_id: flow.clientId,
        callback: response => exchangeCredential(root, flow, response),
        nonce: flow.nonce,
        ux_mode: 'popup',
        auto_select: false,
        use_fedcm_for_button: true
      });

      buttonHost.replaceChildren();
      google.accounts.id.renderButton(buttonHost, {
        type: 'standard',
        theme: 'outline',
        size: 'large',
        text: 'continue_with',
        shape: 'rectangular',
        logo_alignment: 'left',
        width: Math.max(240, Math.min(400, Math.round(root.getBoundingClientRect().width || 320)))
      });
      root.dataset.googleIdentityState = 'ready';
      showStatus(root, '', false);
    } catch {
      fail(root, 'Google sign-in is unavailable. Continue with email instead.');
    }
  };

  window.malievGoogleIdentity = { renderButton };
})();
