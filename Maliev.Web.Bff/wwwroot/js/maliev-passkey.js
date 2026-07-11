(() => {
    window.malievPasskey = window.malievPasskey || {};

    window.malievPasskey.isAvailable = () => {
        return typeof PublicKeyCredential !== 'undefined';
    };

    window.malievPasskey.createPasskey = async (registerBeginUrl, registerCompleteUrl, principalId, deviceName) => {
        try {
            const beginResp = await fetch(registerBeginUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ principalId })
            });
            if (!beginResp.ok) return { success: false, error: 'Failed to start registration' };

            const options = await beginResp.json();

            const publicKey = {
                challenge: base64urlToArray(options.challenge),
                rp: { id: options.rpId, name: options.rpName },
                user: {
                    id: base64urlToArray(options.userId),
                    name: options.userName,
                    displayName: options.userDisplayName
                },
                pubKeyCredParams: options.pubKeyCredParams,
                authenticatorSelection: options.authenticatorSelection,
                attestation: options.attestation || 'none'
            };

            const credential = await navigator.credentials.create({ publicKey });
            if (!credential) return { success: false, error: 'User cancelled' };

            const completeResp = await fetch(registerCompleteUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    principalId,
                    credentialId: arrayToBase64url(new Uint8Array(credential.rawId)),
                    publicKey: arrayBufferToPem(credential.response.getPublicKey ? credential.response.getPublicKey() : null),
                    deviceName: deviceName || 'Passkey',
                    clientDataJson: new TextDecoder().decode(credential.response.clientDataJSON),
                    attestationObject: arrayToBase64url(new Uint8Array(credential.response.attestationObject))
                })
            });
            return await completeResp.json();
        } catch (e) {
            return { success: false, error: e.message || 'Passkey registration failed' };
        }
    };

    window.malievPasskey.authenticatePasskey = async (authBeginUrl, authCompleteUrl, principalId) => {
        try {
            const beginResp = await fetch(authBeginUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ principalId: principalId || null })
            });
            if (!beginResp.ok) return { success: false, error: 'Failed to start authentication' };

            const options = await beginResp.json();

            const publicKey = {
                challenge: base64urlToArray(options.challenge),
                rpId: options.rpId,
                allowCredentials: options.allowCredentials || [],
                userVerification: options.userVerification || 'required'
            };

            const assertion = await navigator.credentials.get({ publicKey });
            if (!assertion) return { success: false, error: 'User cancelled' };

            const userHandle = assertion.response.userHandle
                ? arrayToBase64url(new Uint8Array(assertion.response.userHandle))
                : null;

            const completeResp = await fetch(authCompleteUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    credentialId: arrayToBase64url(new Uint8Array(assertion.rawId)),
                    signature: arrayToBase64url(new Uint8Array(assertion.response.signature)),
                    authenticatorData: arrayToBase64url(new Uint8Array(assertion.response.authenticatorData)),
                    clientDataJson: new TextDecoder().decode(assertion.response.clientDataJSON),
                    userHandle: userHandle
                })
            });
            return await completeResp.json();
        } catch (e) {
            return { success: false, error: e.message || 'Passkey authentication failed' };
        }
    };

    window.getAntiForgeryToken = () => {
        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenElement ? tokenElement.value : '';
    };

    function base64urlToArray(base64url) {
        const base64 = base64url.replace(/-/g, '+').replace(/_/g, '/');
        const padding = 4 - (base64.length % 4);
        const padded = padding < 4 ? base64 + '='.repeat(padding) : base64;
        const raw = atob(padded);
        return Uint8Array.from(raw, c => c.charCodeAt(0)).buffer;
    }

    function arrayToBase64url(array) {
        let binary = '';
        for (let i = 0; i < array.length; i++) binary += String.fromCharCode(array[i]);
        const base64 = btoa(binary);
        return base64.replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
    }

    function arrayBufferToPem(keyData) {
        if (!keyData) return '';
        const base64 = btoa(String.fromCharCode(...new Uint8Array(keyData)));
        return '-----BEGIN PUBLIC KEY-----\n' + base64.match(/.{1,64}/g).join('\n') + '\n-----END PUBLIC KEY-----';
    }
})();
