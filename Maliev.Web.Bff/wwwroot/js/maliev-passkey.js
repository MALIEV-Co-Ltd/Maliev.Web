(() => {
    const beginEndpoint = '/auth/passkey/begin';
    const completeEndpoint = '/auth/passkey/complete';

    window.malievPasskey = {
        isAvailable() {
            return typeof window.PublicKeyCredential !== 'undefined'
                && typeof navigator.credentials?.get === 'function';
        },

        async authenticate(returnUrl = '/account') {
            if (!this.isAvailable()) {
                return { success: false, error: 'Passkeys are not available in this browser.' };
            }

            try {
                const beginResponse = await fetch(beginEndpoint, {
                    method: 'POST',
                    credentials: 'same-origin',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ returnUrl })
                });
                if (!beginResponse.ok) {
                    return await failureFrom(beginResponse, 'Passkey sign-in could not be started.');
                }

                const begin = await beginResponse.json();
                const publicKey = begin.publicKey;
                publicKey.challenge = base64urlToArrayBuffer(publicKey.challenge);
                publicKey.allowCredentials = (publicKey.allowCredentials || []).map(credential => ({
                    ...credential,
                    id: base64urlToArrayBuffer(credential.id)
                }));

                const assertion = await navigator.credentials.get({ publicKey });
                if (!assertion || !assertion.response?.userHandle) {
                    return { success: false, error: 'This passkey could not identify its MALIEV account.' };
                }

                const completeResponse = await fetch(completeEndpoint, {
                    method: 'POST',
                    credentials: 'same-origin',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        credentialId: arrayBufferToBase64url(assertion.rawId),
                        authenticatorData: arrayBufferToBase64url(assertion.response.authenticatorData),
                        clientDataJson: arrayBufferToBase64url(assertion.response.clientDataJSON),
                        signature: arrayBufferToBase64url(assertion.response.signature),
                        userHandle: arrayBufferToBase64url(assertion.response.userHandle)
                    })
                });
                if (!completeResponse.ok) {
                    return await failureFrom(completeResponse, 'Passkey sign-in could not be completed.');
                }

                return { success: true, ...(await completeResponse.json()) };
            } catch (error) {
                if (error?.name === 'NotAllowedError') {
                    return { success: false, error: 'Passkey sign-in was cancelled or timed out.' };
                }

                return { success: false, error: 'Passkey sign-in is temporarily unavailable.' };
            }
        }
    };

    async function failureFrom(response, fallback) {
        try {
            const problem = await response.json();
            return { success: false, error: problem.detail || fallback, code: problem.code || null };
        } catch {
            return { success: false, error: fallback };
        }
    }

    function base64urlToArrayBuffer(value) {
        const base64 = value.replace(/-/g, '+').replace(/_/g, '/');
        const padded = base64 + '='.repeat((4 - (base64.length % 4)) % 4);
        const binary = window.atob(padded);
        return Uint8Array.from(binary, character => character.charCodeAt(0)).buffer;
    }

    function arrayBufferToBase64url(value) {
        const bytes = new Uint8Array(value);
        let binary = '';
        for (const byte of bytes) {
            binary += String.fromCharCode(byte);
        }

        return window.btoa(binary)
            .replace(/\+/g, '-')
            .replace(/\//g, '_')
            .replace(/=+$/g, '');
    }
})();
