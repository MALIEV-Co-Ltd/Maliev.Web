(() => {
    window.malievCart = window.malievCart || {};

    window.malievCart.syncCheckoutForm = form => {
        try {
            const value = window.localStorage.getItem("maliev.cart.v1");
            if (value && form?.elements?.ItemsJson) {
                form.elements.ItemsJson.value = value;
            }
        } catch {
        }

        return true;
    };
})();
