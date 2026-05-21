window.malievQuoteDropzone = (() => {
  const registrations = new Map();
  const formatDismissals = new Map();
  const acceptedExtensions = new Set(["stl", "step", "stp", "3mf", "obj", "igs", "iges", "gltf", "glb"]);

  function register(dropzoneId, inputId, quoteEngineUrl) {
    const dropzone = document.getElementById(dropzoneId);
    const input = document.getElementById(inputId);
    if (!dropzone || !input) {
      return;
    }

    unregister(dropzoneId);

    const state = {
      isNavigating: false
    };

    const isInteractiveChild = event => event.target?.closest?.("[data-dropzone-interactive]");

    const openPicker = event => {
      event.preventDefault();
      if (isInteractiveChild(event)) {
        return;
      }

      if (!state.isNavigating) {
        input.click();
      }
    };

    const handleKeydown = event => {
      if (isInteractiveChild(event) || event.key !== "Enter" && event.key !== " ") {
        return;
      }

      openPicker(event);
    };

    const handleChange = () => {
      if (input.files?.length) {
        routeToQuoteEngine(Array.from(input.files), dropzone, quoteEngineUrl, state);
        input.value = "";
      }
    };

    const dragOver = event => {
      event.preventDefault();
      if (!state.isNavigating) {
        dropzone.classList.add("is-dragover");
        event.dataTransfer.dropEffect = "copy";
      }
    };

    const dragLeave = event => {
      if (!dropzone.contains(event.relatedTarget)) {
        dropzone.classList.remove("is-dragover");
      }
    };

    const drop = event => {
      event.preventDefault();
      dropzone.classList.remove("is-dragover");
      if (state.isNavigating || !event.dataTransfer?.files?.length) {
        return;
      }

      routeToQuoteEngine(Array.from(event.dataTransfer.files), dropzone, quoteEngineUrl, state);
    };

    dropzone.addEventListener("click", openPicker);
    dropzone.addEventListener("keydown", handleKeydown);
    dropzone.addEventListener("dragover", dragOver);
    dropzone.addEventListener("dragleave", dragLeave);
    dropzone.addEventListener("drop", drop);
    input.addEventListener("change", handleChange);

    registrations.set(dropzoneId, {
      dropzone,
      input,
      openPicker,
      handleKeydown,
      handleChange,
      dragOver,
      dragLeave,
      drop,
      state
    });
  }

  function unregister(dropzoneId) {
    const registration = registrations.get(dropzoneId);
    if (!registration) {
      return;
    }

    registration.dropzone.removeEventListener("click", registration.openPicker);
    registration.dropzone.removeEventListener("keydown", registration.handleKeydown);
    registration.dropzone.removeEventListener("dragover", registration.dragOver);
    registration.dropzone.removeEventListener("dragleave", registration.dragLeave);
    registration.dropzone.removeEventListener("drop", registration.drop);
    registration.input.removeEventListener("change", registration.handleChange);
    registrations.delete(dropzoneId);
  }

  function registerFormatDismissal(shellId, dotNetReference) {
    const shell = document.getElementById(shellId);
    if (!shell || !dotNetReference) {
      return;
    }

    unregisterFormatDismissal(shellId);

    const closeIfOutside = event => {
      if (!shell.contains(event.target)) {
        dotNetReference.invokeMethodAsync("CloseFormatsAsync");
      }
    };

    const closeOnEscape = event => {
      if (event.key === "Escape") {
        dotNetReference.invokeMethodAsync("CloseFormatsAsync");
      }
    };

    document.addEventListener("pointerdown", closeIfOutside, true);
    document.addEventListener("keydown", closeOnEscape, true);

    formatDismissals.set(shellId, {
      closeIfOutside,
      closeOnEscape
    });
  }

  function unregisterFormatDismissal(shellId) {
    const dismissal = formatDismissals.get(shellId);
    if (!dismissal) {
      return;
    }

    document.removeEventListener("pointerdown", dismissal.closeIfOutside, true);
    document.removeEventListener("keydown", dismissal.closeOnEscape, true);
    formatDismissals.delete(shellId);
  }

  function routeToQuoteEngine(files, dropzone, quoteEngineUrl, state) {
    const uploadable = files.filter(file => isAccepted(file.name));
    if (!uploadable.length) {
      showError(dropzone, "Use STL, STEP, STP, 3MF, OBJ, IGS, IGES, GLTF, or GLB files.");
      return;
    }

    state.isNavigating = true;
    setNavigating(dropzone, true);
    redirectToQuoteEngine(quoteEngineUrl);
  }

  function redirectToQuoteEngine(quoteEngineUrl) {
    const url = new URL(quoteEngineUrl, window.location.href);
    window.location.assign(url.toString());
  }

  function isAccepted(fileName) {
    const extension = fileName.split(".").pop()?.toLowerCase();
    return extension ? acceptedExtensions.has(extension) : false;
  }

  function setNavigating(dropzone, navigating) {
    dropzone.classList.toggle("is-opening", navigating);
    dropzone.disabled = navigating;
    const labelNode = dropzone.querySelector("[data-quote-dropzone-label]");
    if (labelNode) {
      labelNode.textContent = navigating
        ? dropzone.dataset.openingLabel || "Opening quote engine"
        : dropzone.dataset.idleLabel || labelNode.textContent;
    }
  }

  function showError(dropzone, message) {
    dropzone.classList.remove("is-opening");
    dropzone.classList.add("has-error");
    dropzone.dataset.error = message;
    window.setTimeout(() => {
      dropzone.classList.remove("has-error");
      delete dropzone.dataset.error;
    }, 6000);
  }

  return { register, registerFormatDismissal, unregister, unregisterFormatDismissal };
})();
