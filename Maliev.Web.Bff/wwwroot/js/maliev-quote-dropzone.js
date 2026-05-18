window.malievQuoteDropzone = (() => {
  const registrations = new Map();
  const acceptedExtensions = new Set(["stl", "step", "stp", "obj", "3mf", "iges", "igs"]);

  function register(dropzoneId, inputId, quoteEngineUrl) {
    const dropzone = document.getElementById(dropzoneId);
    const input = document.getElementById(inputId);
    if (!dropzone || !input) {
      return;
    }

    unregister(dropzoneId);

    const state = {
      isUploading: false,
      abortController: null
    };

    const openPicker = event => {
      event.preventDefault();
      if (!state.isUploading) {
        input.click();
      }
    };

    const handleChange = async () => {
      if (input.files?.length) {
        await uploadAndRedirect(Array.from(input.files), dropzone, quoteEngineUrl, state);
        input.value = "";
      }
    };

    const dragOver = event => {
      event.preventDefault();
      if (!state.isUploading) {
        dropzone.classList.add("is-dragover");
        event.dataTransfer.dropEffect = "copy";
      }
    };

    const dragLeave = event => {
      if (!dropzone.contains(event.relatedTarget)) {
        dropzone.classList.remove("is-dragover");
      }
    };

    const drop = async event => {
      event.preventDefault();
      dropzone.classList.remove("is-dragover");
      if (state.isUploading || !event.dataTransfer?.files?.length) {
        return;
      }

      await uploadAndRedirect(Array.from(event.dataTransfer.files), dropzone, quoteEngineUrl, state);
    };

    dropzone.addEventListener("click", openPicker);
    dropzone.addEventListener("dragover", dragOver);
    dropzone.addEventListener("dragleave", dragLeave);
    dropzone.addEventListener("drop", drop);
    input.addEventListener("change", handleChange);

    registrations.set(dropzoneId, {
      dropzone,
      input,
      openPicker,
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

    registration.state.abortController?.abort();
    registration.dropzone.removeEventListener("click", registration.openPicker);
    registration.dropzone.removeEventListener("dragover", registration.dragOver);
    registration.dropzone.removeEventListener("dragleave", registration.dragLeave);
    registration.dropzone.removeEventListener("drop", registration.drop);
    registration.input.removeEventListener("change", registration.handleChange);
    registrations.delete(dropzoneId);
  }

  async function uploadAndRedirect(files, dropzone, quoteEngineUrl, state) {
    const uploadable = files.filter(file => isAccepted(file.name));
    if (!uploadable.length) {
      showError(dropzone, "Use STL, STEP, STP, OBJ, 3MF, IGES, or IGS files.");
      return;
    }

    const quoteSessionId = randomId();
    const completed = [];
    state.isUploading = true;
    state.abortController = new AbortController();
    setUploading(dropzone, true, `Uploading ${uploadable.length} file${uploadable.length === 1 ? "" : "s"}`);

    try {
      for (const file of uploadable) {
        const initiation = await initiateUpload(file, quoteSessionId, state.abortController.signal);
        await putFile(file, initiation.proxyUploadUrl, state.abortController.signal);
        const upload = await completeUpload(initiation.uploadId, state.abortController.signal);
        completed.push({
          uploadId: upload.uploadId || initiation.uploadId,
          fileId: upload.fileId || null,
          fileName: upload.fileName || file.name,
          storagePath: upload.storagePath || initiation.storagePath,
          contentType: file.type || "application/octet-stream",
          fileSizeBytes: file.size,
          status: upload.status || "Completed"
        });
      }

      redirectToQuoteEngine(quoteEngineUrl, quoteSessionId, completed);
    } catch (error) {
      if (error.name !== "AbortError") {
        showError(dropzone, error.message || "Upload failed. Please try again.");
      }
    } finally {
      state.abortController = null;
      state.isUploading = false;
      setUploading(dropzone, false);
    }
  }

  async function initiateUpload(file, quoteSessionId, signal) {
    const response = await fetch("/web/v1/quote/uploads/resumable", {
      method: "POST",
      credentials: "include",
      signal,
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        quoteSessionId,
        fileName: file.name,
        contentType: file.type || "application/octet-stream",
        fileSize: file.size
      })
    });

    if (!response.ok) {
      throw new Error(`Upload session failed with HTTP ${response.status}.`);
    }

    return await response.json();
  }

  async function putFile(file, proxyUploadUrl, signal) {
    const response = await fetch(proxyUploadUrl, {
      method: "PUT",
      credentials: "include",
      signal,
      headers: {
        "Content-Type": file.type || "application/octet-stream",
        "Content-Range": `bytes 0-${file.size - 1}/${file.size}`
      },
      body: file
    });

    if (!response.ok) {
      throw new Error(`File upload failed with HTTP ${response.status}.`);
    }
  }

  async function completeUpload(uploadId, signal) {
    const response = await fetch(`/web/v1/quote/uploads/resumable/${encodeURIComponent(uploadId)}/complete`, {
      method: "POST",
      credentials: "include",
      signal,
      headers: { "Content-Type": "application/json" },
      body: "{}"
    });

    if (!response.ok) {
      throw new Error(`Upload completion failed with HTTP ${response.status}.`);
    }

    return await response.json();
  }

  function redirectToQuoteEngine(quoteEngineUrl, quoteSessionId, files) {
    const url = new URL(quoteEngineUrl, window.location.href);
    const payload = base64UrlEncode(JSON.stringify({
      quoteSessionId,
      source: "maliev-web",
      files
    }));

    url.searchParams.set("handoff", payload);
    window.location.assign(url.toString());
  }

  function isAccepted(fileName) {
    const extension = fileName.split(".").pop()?.toLowerCase();
    return extension ? acceptedExtensions.has(extension) : false;
  }

  function setUploading(dropzone, uploading, label) {
    dropzone.classList.toggle("is-uploading", uploading);
    dropzone.disabled = uploading;
    const labelNode = dropzone.querySelector("[data-quote-dropzone-label]");
    if (labelNode) {
      labelNode.textContent = uploading
        ? label || dropzone.dataset.uploadingLabel || "Uploading files"
        : dropzone.dataset.idleLabel || labelNode.textContent;
    }
  }

  function showError(dropzone, message) {
    dropzone.classList.remove("is-uploading");
    dropzone.classList.add("has-error");
    dropzone.dataset.error = message;
    window.setTimeout(() => {
      dropzone.classList.remove("has-error");
      delete dropzone.dataset.error;
    }, 6000);
  }

  function randomId() {
    if (window.crypto?.randomUUID) {
      return window.crypto.randomUUID();
    }

    return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, character => {
      const random = Math.random() * 16 | 0;
      const value = character === "x" ? random : (random & 0x3 | 0x8);
      return value.toString(16);
    });
  }

  function base64UrlEncode(value) {
    const bytes = new TextEncoder().encode(value);
    let binary = "";
    bytes.forEach(byte => binary += String.fromCharCode(byte));
    return window.btoa(binary)
      .replace(/\+/g, "-")
      .replace(/\//g, "_")
      .replace(/=+$/g, "");
  }

  return { register, unregister };
})();
