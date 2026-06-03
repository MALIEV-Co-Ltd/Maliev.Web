(function () {
    const searches = new Map();
    const maps = new Map();
    let loaderPromise;
    let googleMapsAuthFailed = false;

    const themeObserver = new MutationObserver(() => {
        const isDark = document.documentElement.getAttribute("data-theme") === "dark";
        for (const { autocomplete } of searches.values()) {
            autocomplete.style.colorScheme = isDark ? "dark" : "light";
        }
    });
    themeObserver.observe(document.documentElement, { attributes: true, attributeFilter: ["data-theme"] });

    function loadGoogleMaps(apiKey) {
        if (window.google?.maps?.importLibrary) {
            return Promise.resolve();
        }

        if (loaderPromise) {
            return loaderPromise;
        }

        loaderPromise = new Promise((resolve, reject) => {
            const callbackName = `malievGoogleMapsReady_${Date.now()}`;
            const previousAuthFailure = window.gm_authFailure;
            window.gm_authFailure = () => {
                googleMapsAuthFailed = true;
                if (typeof previousAuthFailure === "function") {
                    previousAuthFailure();
                }
            };

            window[callbackName] = () => {
                delete window[callbackName];
                resolve();
            };

            const script = document.createElement("script");
            script.src = `https://maps.googleapis.com/maps/api/js?key=${encodeURIComponent(apiKey)}&v=weekly&loading=async&callback=${callbackName}`;
            script.async = true;
            script.onerror = () => {
                delete window[callbackName];
                reject(new Error("Google Maps JavaScript API could not be loaded."));
            };
            document.head.appendChild(script);
        });

        return loaderPromise;
    }

    function component(components, type) {
        return components?.find(item => item.types?.includes(type));
    }

    function componentText(components, type) {
        const match = component(components, type);
        return match?.longText || match?.long_name || match?.shortText || match?.short_name || null;
    }

    function componentShortText(components, type) {
        const match = component(components, type);
        return match?.shortText || match?.short_name || match?.longText || match?.long_name || null;
    }

    function firstComponentText(components, types) {
        for (const type of types) {
            const value = componentText(components, type);
            if (value) {
                return value;
            }
        }

        return null;
    }

    function joinParts(parts) {
        return parts.filter(Boolean).join(" ").trim() || null;
    }

    function normalizeSelection(source, place, location, components) {
        const lat = typeof location?.lat === "function" ? location.lat() : location?.lat;
        const lng = typeof location?.lng === "function" ? location.lng() : location?.lng;
        const streetNumber = componentText(components, "street_number");
        const route = componentText(components, "route");
        const premise = firstComponentText(components, ["premise", "subpremise"]);
        let addressLine1 = joinParts([premise, streetNumber, route]);
        if (!addressLine1) {
            const formatted = place?.formattedAddress || place?.formatted_address || "";
            const match = formatted.match(/^(\d+[\d\/\-\s]*\d*)\s*/);
            if (match) addressLine1 = match[1].trim();
        }

        return {
            source,
            placeId: place?.id || place?.place_id || null,
            displayName: place?.displayName || place?.name || null,
            formattedAddress: place?.formattedAddress || place?.formatted_address || null,
            addressLine1: addressLine1,
            district: firstComponentText(components, ["sublocality_level_2", "sublocality_level_1", "locality"]),
            city: firstComponentText(components, ["administrative_area_level_2", "locality", "sublocality_level_1"]),
            stateProvince: componentText(components, "administrative_area_level_1"),
            postalCode: componentText(components, "postal_code"),
            countryIso2: componentShortText(components, "country"),
            latitude: Number.isFinite(lat) ? lat : null,
            longitude: Number.isFinite(lng) ? lng : null
        };
    }

    async function initializeSearch(elementId, dotNetReference, config) {
        const container = document.getElementById(elementId);
        if (!container || !config?.apiKey) {
            return;
        }

        await loadGoogleMaps(config.apiKey);
        const { PlaceAutocompleteElement } = await google.maps.importLibrary("places");
        container.replaceChildren();

        const options = {};
        if (Array.isArray(config.includedRegionCodes) && config.includedRegionCodes.length > 0) {
            options.includedRegionCodes = config.includedRegionCodes;
        }

        const autocomplete = new PlaceAutocompleteElement(options);
        autocomplete.placeholder = "Search for your location";
        autocomplete.classList.add("maliev-google-place-autocomplete");
        autocomplete.style.colorScheme = document.documentElement.getAttribute("data-theme") === "dark" ? "dark" : "light";
        autocomplete.style.display = "block";
        autocomplete.style.width = "100%";
        container.appendChild(autocomplete);

        const handler = async event => {
            const place = event.placePrediction?.toPlace();
            if (!place) {
                return;
            }

            await place.fetchFields({
                fields: ["id", "displayName", "formattedAddress", "location", "addressComponents"]
            });

            await dotNetReference.invokeMethodAsync(
                "NotifyGoogleAddressSelected",
                normalizeSelection("GooglePlace", place, place.location, place.addressComponents));
        };

        autocomplete.addEventListener("gmp-select", handler);
        searches.set(elementId, { autocomplete, handler });
    }

    function notifyStatus(dotNetReference, status) {
        dotNetReference?.invokeMethodAsync("NotifyGoogleAddressPickerStatus", status).catch(() => { });
    }

    function renderMapUnavailable(container) {
        const isThai = (document.documentElement.lang || "").toLowerCase().startsWith("th");
        const panel = document.createElement("div");
        panel.className = "account-google-map-unavailable";
        panel.innerHTML = [
            `<strong>${isThai ? "ไม่สามารถโหลดแผนที่ได้" : "Map unavailable"}</strong>`,
            `<span>${isThai ? "ใช้ช่องค้นหาสถานที่หรือกรอกที่อยู่เอง" : "Use the location search field or enter the address manually."}</span>`
        ].join("");
        container.replaceChildren(panel);
    }

    function hasMapProviderError(container) {
        return googleMapsAuthFailed
            || Boolean(container.querySelector(".gm-err-container"))
            || Boolean(container.querySelector(".CizjDb-degraded-map-dialog-view"));
    }

    async function createAddressMarker(map, position, title, mapId) {
        if (mapId) {
            const { AdvancedMarkerElement } = await google.maps.importLibrary("marker");
            const advancedMarker = new AdvancedMarkerElement({
                map,
                position,
                gmpDraggable: true,
                title
            });

            return {
                addDragEnd: handler => advancedMarker.addListener("dragend", event => handler(event.latLng)),
                dispose: () => { advancedMarker.map = null; },
                setPosition: value => { advancedMarker.position = value; }
            };
        }

        if (!google.maps.Marker) {
            await google.maps.importLibrary("marker");
        }

        const marker = new google.maps.Marker({
            map,
            position,
            draggable: true,
            title
        });

        return {
            addDragEnd: handler => marker.addListener("dragend", event => handler(event.latLng)),
            dispose: () => marker.setMap(null),
            setPosition: value => marker.setPosition(value)
        };
    }

    async function initializeMap(elementId, dotNetReference, config, current) {
        const container = document.getElementById(elementId);
        if (!container || !config?.apiKey) {
            return;
        }

        try {
            await loadGoogleMaps(config.apiKey);
            if (googleMapsAuthFailed) {
                throw new Error("Google Maps rejected the browser API key for this domain.");
            }

            const { Map } = await google.maps.importLibrary("maps");
            await google.maps.importLibrary("geocoding");

            const position = {
                lat: Number(current?.latitude) || Number(config.defaultLatitude) || 13.7563,
                lng: Number(current?.longitude) || Number(config.defaultLongitude) || 100.5018
            };
            const mapId = typeof config.mapId === "string" && config.mapId.trim() ? config.mapId.trim() : null;
            const mapOptions = {
                center: position,
                zoom: Number(config.defaultZoom) || 12,
                streetViewControl: false,
                mapTypeControl: false,
                fullscreenControl: false
            };
            if (mapId) {
                mapOptions.mapId = mapId;
            }

            const map = new Map(container, mapOptions);
            const marker = await createAddressMarker(map, position, "Selected address", mapId);
            const geocoder = new google.maps.Geocoder();
            const publishLocation = async latLng => {
                const literal = typeof latLng.lat === "function"
                    ? { lat: latLng.lat(), lng: latLng.lng() }
                    : latLng;
                marker.setPosition(literal);

                const response = await geocoder.geocode({ location: literal });
                const result = response.results?.[0];
                if (!result) {
                    await dotNetReference.invokeMethodAsync("NotifyGoogleAddressSelected", {
                        source: "GoogleMapPin",
                        latitude: literal.lat,
                        longitude: literal.lng
                    });
                    return;
                }

                await dotNetReference.invokeMethodAsync(
                    "NotifyGoogleAddressSelected",
                    normalizeSelection("GoogleMapPin", result, literal, result.address_components));
            };

            const markerDragListener = marker.addDragEnd(publishLocation);
            const mapClickListener = map.addListener("click", event => publishLocation(event.latLng));
            const errorTimer = window.setTimeout(() => {
                if (!maps.has(elementId) || !hasMapProviderError(container)) {
                    return;
                }

                renderMapUnavailable(container);
                notifyStatus(dotNetReference, "MapUnavailable");
            }, 1200);

            maps.set(elementId, { map, marker, markerDragListener, mapClickListener, errorTimer });
        } catch (error) {
            console.warn("MALIEV address map unavailable.", error);
            renderMapUnavailable(container);
            notifyStatus(dotNetReference, "MapUnavailable");
        }
    }

    function disposeSearch(elementId) {
        const state = searches.get(elementId);
        if (state) {
            state.autocomplete.removeEventListener("gmp-select", state.handler);
            searches.delete(elementId);
        }
    }

    function disposeMap(elementId) {
        const state = maps.get(elementId);
        if (state?.errorTimer) {
            window.clearTimeout(state.errorTimer);
        }
        if (state?.markerDragListener) {
            state.markerDragListener.remove();
        }
        if (state?.mapClickListener) {
            state.mapClickListener.remove();
        }
        if (state?.marker) {
            state.marker.dispose();
        }
        maps.delete(elementId);
    }

    window.malievGoogleAddressPicker = {
        initializeSearch,
        initializeMap,
        disposeSearch,
        disposeMap
    };
})();
