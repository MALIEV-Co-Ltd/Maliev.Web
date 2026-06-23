const babylonCdn = "/lib/babylonjs/babylon.min.js";
const babylonLoadersCdn = "/lib/babylonjs/babylonjs.loaders.min.js";
const sharedRuntime = globalThis.__malievManufacturingGizmo ??= {
  instances: new WeakMap(),
  babylonRuntime: null,
  babylonLoadersRuntime: null
};
const instances = sharedRuntime.instances;
const normalizedHeroModelSize = 2.28;

mountDocumentGizmos();

export function mountManufacturingGizmo(canvas, modelUrl = "", enableHoverMotion = false, usePlasticMaterial = false) {
  if (!canvas || instances.has(canvas)) {
    return;
  }

  const host = canvas.parentElement;
  const state = {
    canvas,
    host,
    modelUrl: typeof modelUrl === "string" ? modelUrl.trim() : "",
    enableHoverMotion: Boolean(enableHoverMotion),
    usePlasticMaterial: Boolean(usePlasticMaterial),
    scene: null,
    engine: null,
    disposed: false,
    started: false,
    visible: false,
    looping: false,
    observer: null,
    resizeObserver: null,
    resizeHandler: null,
    resizeFrame: 0,
    resizeWidth: 0,
    resizeHeight: 0,
    resizePixelRatio: 0,
    resizeRenderRatio: 0,
    visibilityHandler: null,
    pointerEnterHandler: null,
    pointerMoveHandler: null,
    pointerLeaveHandler: null,
    contextMenuHandler: null,
    cameraConfigurator: null,
    themeObserver: null,
    themeApplicator: null,
    landingFrame: null,
    highQualityRendering: Boolean(modelUrl)
  };

  instances.set(canvas, state);
  host?.classList.add("is-loading");

  const start = () => {
    if (state.started || state.disposed) {
      return;
    }

    state.started = true;
    loadBabylon(Boolean(state.modelUrl))
      .then(BABYLON => {
        if (state.disposed) {
          return undefined;
        }

        return state.modelUrl
          ? createLandingHeroScene(state, BABYLON)
          : createGizmoScene(state, BABYLON);
      })
      .catch(() => {
        state.host?.classList.add("is-offline");
      });
  };

  if ("IntersectionObserver" in window) {
    state.observer = new IntersectionObserver(entries => {
      state.visible = entries.some(entry => entry.isIntersecting && entry.intersectionRatio > 0);
      if (state.visible) {
        start();
        startRenderLoop(state);
      } else {
        stopRenderLoop(state);
      }
    }, { rootMargin: "180px 0px", threshold: 0.01 });
    state.observer.observe(canvas);
    requestAnimationFrame(() => {
      if (!state.started && isCanvasNearViewport(canvas)) {
        state.visible = true;
        start();
        startRenderLoop(state);
      }
    });
  } else {
    state.visible = true;
    start();
  }
}

function isCanvasNearViewport(canvas) {
  const rect = canvas.getBoundingClientRect();
  const margin = 180;
  return rect.width > 0 &&
    rect.height > 0 &&
    rect.bottom >= -margin &&
    rect.right >= -margin &&
    rect.top <= window.innerHeight + margin &&
    rect.left <= window.innerWidth + margin;
}

function mountDocumentGizmos() {
  if (typeof document === "undefined") {
    return;
  }

  const mountCanvas = canvas => mountManufacturingGizmo(
    canvas,
    canvas.dataset.modelUrl ?? "",
    canvas.dataset.enableHoverMotion === "true",
    canvas.dataset.usePlasticMaterial === "true");

  const mount = root => {
    if (root.matches?.("canvas[data-manufacturing-gizmo]")) {
      mountCanvas(root);
    }

    root
      .querySelectorAll?.("canvas[data-manufacturing-gizmo]")
      .forEach(mountCanvas);
  };

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", () => mount(document), { once: true });
  } else {
    queueMicrotask(() => mount(document));
  }

  if ("MutationObserver" in window) {
    const observer = new MutationObserver(records => {
      for (const record of records) {
        for (const node of record.addedNodes) {
          if (node instanceof Element) {
            mount(node.matches("canvas[data-manufacturing-gizmo]") ? node.parentElement ?? node : node);
          }
        }
      }
    });

    if (document.body) {
      observer.observe(document.body, { childList: true, subtree: true });
    } else {
      document.addEventListener("DOMContentLoaded", () => observer.observe(document.body, { childList: true, subtree: true }), { once: true });
    }
  }
}

export function disposeManufacturingGizmo(canvas) {
  const state = instances.get(canvas);
  if (!state) {
    return;
  }

  state.disposed = true;
  stopRenderLoop(state);
  state.observer?.disconnect();
  state.resizeObserver?.disconnect();

  if (state.resizeFrame) {
    cancelAnimationFrame(state.resizeFrame);
    state.resizeFrame = 0;
  }

  if (state.resizeHandler) {
    window.removeEventListener("resize", state.resizeHandler);
  }

  if (state.visibilityHandler) {
    document.removeEventListener("visibilitychange", state.visibilityHandler);
  }

  if (state.pointerEnterHandler) {
    state.host?.removeEventListener("pointerenter", state.pointerEnterHandler);
  }

  if (state.pointerMoveHandler) {
    state.host?.removeEventListener("pointermove", state.pointerMoveHandler);
  }

  if (state.pointerLeaveHandler) {
    state.host?.removeEventListener("pointerleave", state.pointerLeaveHandler);
  }

  if (state.contextMenuHandler) {
    state.canvas.removeEventListener("contextmenu", state.contextMenuHandler, true);
  }

  state.themeObserver?.disconnect();
  state.scene?.dispose();
  state.engine?.dispose();
  state.host?.classList.remove("is-loading", "is-ready", "is-offline");
  delete state.canvas.dataset.ready;
  instances.delete(canvas);
}

function loadBabylon(includeLoaders) {
  const core = loadBabylonCore();
  if (!includeLoaders) {
    return core;
  }

  return core.then(BABYLON => loadBabylonLoaders().then(() => BABYLON));
}

function loadBabylonCore() {
  if (window.BABYLON) {
    return Promise.resolve(window.BABYLON);
  }

  if (!sharedRuntime.babylonRuntime) {
    sharedRuntime.babylonRuntime = new Promise((resolve, reject) => {
      const script = document.createElement("script");
      script.src = babylonCdn;
      script.async = true;
      script.crossOrigin = "anonymous";
      script.onload = () => window.BABYLON ? resolve(window.BABYLON) : reject(new Error("BabylonJS did not initialize."));
      script.onerror = reject;
      document.head.appendChild(script);
    });
  }

  return sharedRuntime.babylonRuntime;
}

function loadBabylonLoaders() {
  if (window.BABYLON?.GLTFFileLoader) {
    return Promise.resolve();
  }

  if (!sharedRuntime.babylonLoadersRuntime) {
    sharedRuntime.babylonLoadersRuntime = new Promise((resolve, reject) => {
      const script = document.createElement("script");
      script.src = babylonLoadersCdn;
      script.async = true;
      script.crossOrigin = "anonymous";
      script.onload = () => resolve();
      script.onerror = reject;
      document.head.appendChild(script);
    });
  }

  return sharedRuntime.babylonLoadersRuntime;
}

function createEngine(canvas, BABYLON, options = {}) {
  const highQuality = Boolean(options.highQuality);
  const engine = new BABYLON.Engine(canvas, true, {
    alpha: true,
    antialias: true,
    premultipliedAlpha: false,
    stencil: false,
    preserveDrawingBuffer: false,
    powerPreference: highQuality ? "high-performance" : "low-power"
  }, false);

  configureHardwareScaling(engine, getRenderPixelRatio(highQuality));
  return engine;
}

function createGizmoScene(state, BABYLON) {
  const { canvas } = state;
  const engine = createEngine(canvas, BABYLON);

  const scene = new BABYLON.Scene(engine);
  scene.clearColor = BABYLON.Color4.FromHexString("#ffffff00");
  scene.skipPointerMovePicking = true;

  const camera = new BABYLON.ArcRotateCamera(
    "camera",
    -Math.PI / 4,
    Math.PI / 2.7,
    5.2,
    new BABYLON.Vector3(0.15, 0.25, 0),
    scene);
  camera.lowerRadiusLimit = 3.4;
  camera.upperRadiusLimit = 6.4;
  camera.wheelPrecision = 70;
  camera.panningSensibility = 0;
  camera.attachControl(canvas, true);
  configureSceneAntialiasing(scene, camera, BABYLON);

  new BABYLON.HemisphericLight("fill", new BABYLON.Vector3(-0.4, 1, 0.2), scene).intensity = 0.86;
  const key = new BABYLON.DirectionalLight("key", new BABYLON.Vector3(-0.6, -0.8, -0.45), scene);
  key.position = new BABYLON.Vector3(4, 5, 4);
  key.intensity = 0.7;

  const root = new BABYLON.TransformNode("manufacturing-preview", scene);
  createFixture(scene, BABYLON, root);
  createAxes(scene, BABYLON, root);
  createBed(scene, BABYLON, root);

  const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  if (!reducedMotion) {
    scene.onBeforeRenderObservable.add(() => {
      root.rotation.y += engine.getDeltaTime() * 0.00014;
    });
  }

  configureSceneRuntime(state, engine, scene);
  markReady(state);
}

async function createLandingHeroScene(state, BABYLON) {
  const { canvas } = state;
  allowNativeContextMenu(state);
  const engine = createEngine(canvas, BABYLON, { highQuality: true });
  restoreNativeCanvasBehavior(state);
  const scene = new BABYLON.Scene(engine);
  scene.clearColor = BABYLON.Color4.FromHexString("#ffffff00");
  scene.skipPointerMovePicking = true;
  scene.environmentIntensity = 0.66;
  configureLandingCadToneMapping(scene, BABYLON);

  const camera = new BABYLON.ArcRotateCamera(
    "landing-camera",
    -Math.PI / 2.55,
    Math.PI / 1.95,
    5.15,
    new BABYLON.Vector3(0, 0.08, 0),
    scene);
  camera.lowerRadiusLimit = camera.radius;
  camera.upperRadiusLimit = camera.radius;
  camera.panningSensibility = 0;
  camera.inputs.clear();
  configureLandingHeroCamera(camera, state.host, BABYLON, state.landingFrame);

  const lights = configureLandingCadStudioLighting(scene, camera, BABYLON);
  configureSceneAntialiasing(scene, camera, BABYLON);
  configureCadAmbientOcclusion(scene, camera, BABYLON);

  const root = new BABYLON.TransformNode("landing-model-root", scene);
  const modelParts = splitModelUrl(state.modelUrl);
  const result = await BABYLON.SceneLoader.ImportMeshAsync("", modelParts.rootUrl, modelParts.fileName, scene);

  const topLevelMeshes = result.meshes.filter(mesh => !mesh.parent);
  for (const mesh of topLevelMeshes) {
    mesh.parent = root;
  }

  const renderMeshes = result.meshes.filter(mesh => mesh.getTotalVertices && mesh.getTotalVertices() > 0);
  for (const mesh of renderMeshes) {
    mesh.isPickable = false;
  }

  const plasticMaterial = state.usePlasticMaterial
    ? applyInjectionMoldedPlasticMaterial(renderMeshes, scene, BABYLON)
    : null;

  state.landingFrame = frameImportedModel(renderMeshes, root, BABYLON);
  const baseRotation = new BABYLON.Vector3(0.04, -0.18, 0.005);
  root.rotation.copyFrom(baseRotation);

  // Snapshot the world AABB *after* baseRotation is applied. This is the single
  // source of truth that downstream camera framing uses — recomputing this every
  // frame would pick up the levitation/hover offsets, which is not what we want.
  state.landingFrame = {
    ...state.landingFrame,
    worldBounds: captureWorldAabb(state.landingFrame?.meshes ?? renderMeshes, BABYLON)
  };

  const shadows = configureLandingCadShadows(scene, lights.key, state.landingFrame, renderMeshes, BABYLON);
  state.themeApplicator = () => applyLandingHeroTheme(
    scene,
    plasticMaterial,
    lights,
    shadows,
    BABYLON);
  state.themeApplicator();
  observeDocumentTheme(state);

  configureLandingHeroCamera(camera, state.host, BABYLON, state.landingFrame);

  const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  if (!reducedMotion) {
    addIdleLevitation(state, scene, root);
  }

  if (state.enableHoverMotion && !reducedMotion) {
    addHoverMotion(state, scene, camera, root, baseRotation, BABYLON);
  }

  configureSceneRuntime(state, engine, scene, () => configureLandingHeroCamera(camera, state.host, BABYLON, state.landingFrame));
  markReady(state);
}

function addIdleLevitation(state, scene, root) {
  const baseY = root.position.y;
  const startedAt = performance.now();

  scene.onBeforeRenderObservable.add(() => {
    const elapsed = performance.now() - startedAt;
    const levitation = Math.sin(elapsed * 0.00055) * 0.025;
    root.position.y = baseY + levitation;
  });
}

function configureLandingHeroCamera(camera, host, BABYLON, frame) {
  const metrics = getLandingHeroViewportMetrics(host);

  camera.fov = metrics.fov;
  camera.lowerRadiusLimit = null;
  camera.upperRadiusLimit = null;

  if (frame?.worldBounds) {
    applyLandingHeroFraming(camera, frame.worldBounds, metrics, BABYLON);
  } else {
    camera.target = new BABYLON.Vector3(0, metrics.targetY, 0);
    camera.radius = metrics.fallbackRadius;
  }

  camera.lowerRadiusLimit = camera.radius;
  camera.upperRadiusLimit = camera.radius;
}

function getLandingHeroViewportMetrics(host) {
  const width = host?.clientWidth ?? 780;
  const height = host?.clientHeight ?? 520;
  const aspect = width / Math.max(height, 1);
  const compact = width < 560 || height < 360;
  const narrowTall = width < 700 && height >= 500 && aspect < 1.12;
  const balancedTablet = width >= 640 && width <= 920 && height >= 460;
  const wide = width > 920;

  // When the canvas covers the full viewport (desktop >= 961px), compute where
  // the right column's center falls as a fraction [0, 1] of the canvas width
  // so the camera can shift its target to keep the model in that column.
  let rightCenterFraction = null;
  if (wide && host) {
    const hero = host.closest(".landing-hero");
    const copy = hero?.querySelector(".landing-hero-copy");
    if (copy && host.clientWidth > 0) {
      const hostRect = host.getBoundingClientRect();
      const copyRect = copy.getBoundingClientRect();
      const copyRightNorm = (copyRect.right - hostRect.left) / hostRect.width;
      rightCenterFraction = clamp((copyRightNorm + 1) / 2, 0.52, 0.88);
    }
  }

  // Effective aspect ratio of just the right column so the framing math
  // sizes the model to fill that sub-region, not the full hero width.
  const effectiveAspect = rightCenterFraction != null
    ? 2 * (1 - rightCenterFraction) * aspect
    : aspect;
  const presentationColumnBias = wide ? 0.018 : 0;

  // `fill` is the fraction of the viewport's smaller half-angle that the
  // model's silhouette (rotated, scaled, world-space) should occupy. Higher
  // values pack the model tighter into the canvas; lower values add letterbox.
  // We bias slightly higher on wide layouts so the hero feels filled, and
  // pull back on narrow-tall (mobile portrait) so the headline above doesn't
  // collide with the model.
  return {
    aspect,
    effectiveAspect,
    rightCenterFraction: rightCenterFraction == null ? null : clamp(rightCenterFraction + presentationColumnBias, 0.52, 0.9),
    fov: narrowTall ? 0.5 : compact ? 0.58 : balancedTablet ? 0.48 : wide ? 0.45 : 0.46,
    fill: narrowTall ? 0.62 : compact ? 0.68 : balancedTablet ? 0.74 : wide ? 0.76 : 0.74,
    minRadius: narrowTall ? 3.6 : compact ? 2.8 : balancedTablet ? 3.4 : 3.6,
    maxRadius: narrowTall ? 12 : compact ? 11 : 11,
    fallbackRadius: narrowTall ? 6.4 : compact ? 5.6 : balancedTablet ? 5.4 : wide ? 5.6 : 5.6,
    targetY: compact ? -0.08 : wide ? -0.22 : -0.2
  };
}

/**
 * Frame the model using a single deterministic geometric fit against the
 * pre-rotated world AABB captured at scene init. This replaces a multi-step
 * pipeline that combined Babylon's FramingBehavior with manual scale
 * multipliers — the old pipeline used different bounding boxes for different
 * steps (pre-rotation `frame.size` for fit math, post-rotation behavior for
 * `zoomOnMeshesHierarchy`), which caused models with non-cube aspect ratios
 * to either overfill (clip) or underfill (look small).
 */
function applyLandingHeroFraming(camera, worldBounds, metrics, BABYLON) {
  const size = worldBounds.max.subtract(worldBounds.min);
  const center = worldBounds.min.add(worldBounds.max).scale(0.5);

  const halfH = Math.max(size.y / 2, 0.0001);
  const halfW = Math.max(Math.max(size.x, size.z) / 2, 0.0001);
  const halfD = Math.max(size.z, size.x) / 2;

  const fill = clamp(metrics.fill ?? 0.7, 0.4, 0.9);
  const verticalFov = Math.max(camera.fov, 0.01);

  // When the canvas covers the full hero, frame against the right-column
  // sub-aspect so the model fills that region rather than the full canvas.
  const fovAspect = metrics.effectiveAspect ?? metrics.aspect;
  const horizontalFov = 2 * Math.atan(Math.tan(verticalFov / 2) * Math.max(fovAspect, 0.01));

  // Distance at which the model's half-extent occupies `fill` fraction of the
  // viewport's half-angle. Take the larger of the two axes so neither clips.
  const verticalRadius = halfH / (fill * Math.tan(verticalFov / 2));
  const horizontalRadius = halfW / (fill * Math.tan(horizontalFov / 2));

  // Add half the model's depth so the *near* face of the model is what gets
  // fit, not the geometric center. Without this, deep models (assemblies
  // with thickness along the camera axis) appear larger than the fit target.
  const radius = Math.max(verticalRadius, horizontalRadius) + halfD;

  camera.radius = clamp(radius, metrics.minRadius, metrics.maxRadius);
  refreshCameraMatrices(camera);

  // Shift camera target X so the model appears centred in the right column of
  // the full-hero canvas. Moving target left pushes the model right in screen
  // space. The span is computed with the full-hero aspect so offset is correct.
  let targetX = center.x;
  if (metrics.rightCenterFraction != null) {
    const fullHfov = 2 * Math.atan(Math.tan(verticalFov / 2) * Math.max(metrics.aspect, 0.01));
    const halfSpan = Math.tan(fullHfov / 2) * camera.radius;
    targetX -= (metrics.rightCenterFraction - 0.5) * 2 * halfSpan;
  }

  camera.target = new BABYLON.Vector3(targetX, center.y + metrics.targetY, center.z);
}

/**
 * Compute a world-space AABB by forcing every mesh's bounding info to refresh
 * against its current world matrix. This catches the post-rotation extents,
 * which is what the camera fit math actually needs to see.
 */
function captureWorldAabb(meshes, BABYLON) {
  let min = new BABYLON.Vector3(Number.POSITIVE_INFINITY, Number.POSITIVE_INFINITY, Number.POSITIVE_INFINITY);
  let max = new BABYLON.Vector3(Number.NEGATIVE_INFINITY, Number.NEGATIVE_INFINITY, Number.NEGATIVE_INFINITY);
  let found = false;

  for (const mesh of meshes) {
    if (!mesh?.getBoundingInfo) {
      continue;
    }

    updateWorldMatrixChain(mesh);
    const worldMatrix = mesh.getWorldMatrix?.();
    const info = mesh.getBoundingInfo();
    if (worldMatrix && info?.update) {
      info.update(worldMatrix);
    }

    const vectors = info?.boundingBox?.vectorsWorld;
    if (!vectors) {
      continue;
    }

    for (const vector of vectors) {
      min = BABYLON.Vector3.Minimize(min, vector);
      max = BABYLON.Vector3.Maximize(max, vector);
      found = true;
    }
  }

  return found ? { min, max } : null;
}

function refreshCameraMatrices(camera) {
  camera.getViewMatrix(true);
  camera.getProjectionMatrix?.(true);
}

function addHoverMotion(state, scene, camera, root, baseRotation, BABYLON) {
  const host = state.host ?? state.canvas;
  const pointer = { x: 0, y: 0, targetX: 0, targetY: 0, strength: 0, targetStrength: 0 };
  const baseAlpha = camera.alpha;
  const baseBeta = camera.beta;

  state.pointerEnterHandler = () => {
    pointer.targetStrength = 1;
  };

  state.pointerMoveHandler = event => {
    const rect = host.getBoundingClientRect();
    if (!rect.width || !rect.height) {
      return;
    }

    pointer.targetStrength = 1;
    pointer.targetX = ((event.clientX - rect.left) / rect.width - 0.5) * 2;
    pointer.targetY = (0.5 - (event.clientY - rect.top) / rect.height) * 2;
    pointer.targetX = Math.max(-1, Math.min(1, pointer.targetX));
    pointer.targetY = Math.max(-1, Math.min(1, pointer.targetY));
  };

  state.pointerLeaveHandler = () => {
    pointer.targetX = 0;
    pointer.targetY = 0;
    pointer.targetStrength = 0;
  };

  host.addEventListener("pointerenter", state.pointerEnterHandler, { passive: true });
  host.addEventListener("pointermove", state.pointerMoveHandler, { passive: true });
  host.addEventListener("pointerleave", state.pointerLeaveHandler, { passive: true });

  scene.onBeforeRenderObservable.add(() => {
    const delta = Math.min(48, state.engine.getDeltaTime() || 16.67);
    const pointerFollow = 1 - Math.pow(0.001, delta / 720);
    const hoverFade = 1 - Math.pow(0.001, delta / 920);
    pointer.x += (pointer.targetX - pointer.x) * pointerFollow;
    pointer.y += (pointer.targetY - pointer.y) * pointerFollow;
    pointer.strength += (pointer.targetStrength - pointer.strength) * hoverFade;

    const hoverX = pointer.x * pointer.strength;
    const hoverY = pointer.y * pointer.strength;

    root.rotation.x = baseRotation.x + hoverY * 0.055;
    root.rotation.y = baseRotation.y - hoverX * 0.12;
    root.rotation.z = baseRotation.z + hoverX * 0.014;
    camera.alpha = baseAlpha - hoverX * 0.026;
    camera.beta = clamp(baseBeta + hoverY * 0.018, 0.72, 1.36);
  });
}

function allowNativeContextMenu(state) {
  state.contextMenuHandler = event => {
    event.stopImmediatePropagation();
  };

  state.canvas.addEventListener("contextmenu", state.contextMenuHandler, true);
}

function restoreNativeCanvasBehavior(state) {
  state.canvas.style.touchAction = "auto";
  state.canvas.style.cursor = "default";
}

function splitModelUrl(modelUrl) {
  const absolute = new URL(modelUrl, document.baseURI).href;
  const slashIndex = absolute.lastIndexOf("/") + 1;
  return {
    rootUrl: absolute.substring(0, slashIndex),
    fileName: absolute.substring(slashIndex)
  };
}

function applyInjectionMoldedPlasticMaterial(meshes, scene, BABYLON) {
  const plastic = new BABYLON.PBRMaterial("injection-molded-plastic", scene);
  plastic.metallic = 0;
  plastic.roughness = 0.74;
  plastic.microSurface = 0.34;
  plastic.specularIntensity = 0.2;
  plastic.environmentIntensity = 0.36;
  plastic.clearCoat.isEnabled = false;
  plastic.clearCoat.intensity = 0;
  plastic.clearCoat.roughness = 0.82;
  plastic.metadata = { cadMeshes: meshes };

  for (const mesh of meshes) {
    mesh.material = plastic;
    mesh.useVertexColors = false;
    mesh.hasVertexAlpha = false;
  }

  disableCadMeshEdges(meshes);

  return plastic;
}

function disableCadMeshEdges(meshes) {
  for (const mesh of meshes) {
    if (typeof mesh.disableEdgesRendering === "function") {
      mesh.disableEdgesRendering();
    }

    mesh.edgesWidth = 0;
  }
}

function configureCadAmbientOcclusion(scene, camera, BABYLON) {
  if (!BABYLON.SSAO2RenderingPipeline || !scene.enableGeometryBufferRenderer || !camera) {
    return null;
  }

  try {
    scene.enableGeometryBufferRenderer();
    const ssao = new BABYLON.SSAO2RenderingPipeline(
      "landing-cad-ambient-occlusion",
      scene,
      { ssaoRatio: 0.56, blurRatio: 0.5 },
      [camera]);
    ssao.radius = 1.18;
    ssao.totalStrength = 0.58;
    ssao.base = 0.08;
    ssao.expensiveBlur = false;
    return ssao;
  } catch {
    return null;
  }
}

function configureLandingCadShadows(scene, keyLight, frame, meshes, BABYLON) {
  if (!BABYLON.ShadowGenerator || !frame?.worldBounds || !keyLight) {
    return null;
  }

  scene.shadowsEnabled = true;
  keyLight.shadowEnabled = true;
  keyLight.shadowMinZ = 0.4;
  keyLight.shadowMaxZ = 18;

  const shadowGenerator = new BABYLON.ShadowGenerator(2048, keyLight);
  shadowGenerator.useBlurExponentialShadowMap = true;
  shadowGenerator.blurKernel = 22;
  shadowGenerator.bias = 0.00045;
  shadowGenerator.normalBias = 0.035;
  shadowGenerator.transparencyShadow = true;

  for (const mesh of meshes) {
    if (!mesh.getTotalVertices || mesh.getTotalVertices() <= 0) {
      continue;
    }

    mesh.receiveShadows = false;
    shadowGenerator.addShadowCaster(mesh, false);
  }

  const bounds = frame.worldBounds;
  const size = bounds.max.subtract(bounds.min);
  const center = bounds.min.add(bounds.max).scale(0.5);
  const footprint = Math.max(size.x, size.z, 1.25);
  const shadowCatcher = BABYLON.MeshBuilder.CreateGround("landing-shadow-catcher", {
    width: footprint * 1.88,
    height: footprint * 1.52,
    subdivisions: 1
  }, scene);
  shadowCatcher.position = new BABYLON.Vector3(
    center.x + size.x * 0.04,
    bounds.min.y - Math.max(size.y * 0.035, 0.035),
    center.z + size.z * 0.08);
  shadowCatcher.isPickable = false;
  shadowCatcher.receiveShadows = true;

  const shadowMaterial = new BABYLON.StandardMaterial("landing-shadow-catcher-material", scene);
  shadowMaterial.diffuseColor = BABYLON.Color3.FromHexString("#f3f6f9");
  shadowMaterial.specularColor = BABYLON.Color3.Black();
  shadowMaterial.alpha = 0.1;
  shadowMaterial.opacityTexture = createLandingShadowOpacityTexture(scene, BABYLON);
  shadowMaterial.disableLighting = false;
  shadowMaterial.transparencyMode = BABYLON.Material.MATERIAL_ALPHABLEND;
  shadowCatcher.material = shadowMaterial;

  return { shadowGenerator, shadowCatcher, shadowMaterial };
}

function createLandingShadowOpacityTexture(scene, BABYLON) {
  const texture = new BABYLON.DynamicTexture(
    "landing-shadow-catcher-opacity",
    { width: 512, height: 512 },
    scene,
    false);
  const context = texture.getContext();
  const gradient = context.createRadialGradient(256, 256, 46, 256, 256, 248);
  gradient.addColorStop(0, "rgba(255, 255, 255, 1)");
  gradient.addColorStop(0.55, "rgba(255, 255, 255, .58)");
  gradient.addColorStop(1, "rgba(255, 255, 255, 0)");
  context.clearRect(0, 0, 512, 512);
  context.fillStyle = gradient;
  context.fillRect(0, 0, 512, 512);
  texture.update();
  return texture;
}

function configureSceneAntialiasing(scene, camera, BABYLON) {
  if (!BABYLON.FxaaPostProcess || !camera) {
    return;
  }

  const fxaa = new BABYLON.FxaaPostProcess("canvas-fxaa", 1.0, camera);
  const maxSamples = scene.getEngine?.().getCaps?.().maxMSAASamples ?? 1;
  fxaa.samples = Math.min(4, Math.max(1, maxSamples));
}

function configureLandingCadToneMapping(scene, BABYLON) {
  if (!scene.imageProcessingConfiguration) {
    return;
  }

  scene.imageProcessingConfiguration.toneMappingEnabled = true;
  scene.imageProcessingConfiguration.toneMappingType = BABYLON.ImageProcessingConfiguration.TONEMAPPING_ACES;
  scene.imageProcessingConfiguration.exposure = 1.06;
  scene.imageProcessingConfiguration.contrast = 1.18;
}

function configureLandingCadStudioLighting(scene, camera, BABYLON) {
  const fill = new BABYLON.HemisphericLight("landing-fill", new BABYLON.Vector3(-0.2, 1, 0.18), scene);
  fill.diffuse = BABYLON.Color3.FromHexString("#f4f7fb");
  fill.specular = BABYLON.Color3.FromHexString("#ffffff");

  const key = new BABYLON.DirectionalLight("landing-key", new BABYLON.Vector3(-0.48, -0.76, -0.43), scene);
  key.position = new BABYLON.Vector3(4.8, 6.2, 5.4);
  key.diffuse = BABYLON.Color3.FromHexString("#fff7eb");
  key.specular = BABYLON.Color3.FromHexString("#ffffff");

  const softbox = new BABYLON.DirectionalLight("landing-softbox", new BABYLON.Vector3(0.62, -0.46, -0.28), scene);
  softbox.position = new BABYLON.Vector3(-5.6, 4.2, 3.4);
  softbox.diffuse = BABYLON.Color3.FromHexString("#dcecff");
  softbox.specular = BABYLON.Color3.FromHexString("#f6fbff");

  const rim = new BABYLON.DirectionalLight("landing-rim", new BABYLON.Vector3(0.35, -0.18, 0.92), scene);
  rim.position = new BABYLON.Vector3(-3.8, 2.4, -4.2);
  rim.diffuse = BABYLON.Color3.FromHexString("#9fd0ff");
  rim.specular = BABYLON.Color3.FromHexString("#ffffff");

  const bounce = new BABYLON.PointLight("landing-bounce", new BABYLON.Vector3(0, -3.2, 2.4), scene);
  bounce.range = 8;

  const cameraHeadlight = new BABYLON.DirectionalLight("landing-camera-headlight", new BABYLON.Vector3(0, -0.08, 1), scene);
  cameraHeadlight.diffuse = BABYLON.Color3.FromHexString("#f7fbff");
  cameraHeadlight.specular = BABYLON.Color3.FromHexString("#ffffff");
  updateLandingHeadlight(camera, cameraHeadlight, BABYLON);
  scene.onBeforeRenderObservable.add(() => updateLandingHeadlight(camera, cameraHeadlight, BABYLON));

  return { fill, key, softbox, rim, bounce, cameraHeadlight };
}

function updateLandingHeadlight(camera, cameraHeadlight, BABYLON) {
  const fallback = new BABYLON.Vector3(0, -0.08, 1);
  if (!camera?.position || !camera?.target) {
    cameraHeadlight.direction = fallback;
    return;
  }

  const direction = camera.target.subtract(camera.position);
  if (!Number.isFinite(direction.lengthSquared()) || direction.lengthSquared() <= 0.0001) {
    cameraHeadlight.direction = fallback;
    return;
  }

  cameraHeadlight.position = camera.position.clone();
  cameraHeadlight.direction = direction.normalize();
}

function frameImportedModel(meshes, root, BABYLON) {
  const bounds = computeMeshBounds(meshes, BABYLON);
  if (!bounds) {
    return null;
  }

  const displayBounds = bounds.display ?? bounds;
  const frameMeshes = displayBounds.meshes ?? meshes;
  const normalizedFrame = normalizeImportedModelDimensions(displayBounds, root, frameMeshes);

  return {
    meshes: frameMeshes,
    size: normalizedFrame.size
  };
}

function normalizeImportedModelDimensions(displayBounds, root, frameMeshes) {
  const size = displayBounds.max.subtract(displayBounds.min);
  const maxDimension = Math.max(size.x, size.y, size.z) || 1;
  const scale = normalizedHeroModelSize / maxDimension;
  root.scaling.setAll(scale);
  root.position.copyFrom(displayBounds.center.scale(-scale));

  for (const mesh of frameMeshes) {
    updateWorldMatrixChain(mesh);
  }

  return {
    scale,
    size: size.scale(scale)
  };
}

function computeMeshBounds(meshes, BABYLON) {
  const renderMeshes = meshes.filter(mesh => mesh.getBoundingInfo);
  if (!renderMeshes.length) {
    return null;
  }

  let min = new BABYLON.Vector3(Number.POSITIVE_INFINITY, Number.POSITIVE_INFINITY, Number.POSITIVE_INFINITY);
  let max = new BABYLON.Vector3(Number.NEGATIVE_INFINITY, Number.NEGATIVE_INFINITY, Number.NEGATIVE_INFINITY);
  const entries = [];

  for (const mesh of renderMeshes) {
    updateWorldMatrixChain(mesh);
    const vectors = mesh.getBoundingInfo().boundingBox.vectorsWorld;
    let meshMin = new BABYLON.Vector3(Number.POSITIVE_INFINITY, Number.POSITIVE_INFINITY, Number.POSITIVE_INFINITY);
    let meshMax = new BABYLON.Vector3(Number.NEGATIVE_INFINITY, Number.NEGATIVE_INFINITY, Number.NEGATIVE_INFINITY);

    for (const vector of vectors) {
      min = BABYLON.Vector3.Minimize(min, vector);
      max = BABYLON.Vector3.Maximize(max, vector);
      meshMin = BABYLON.Vector3.Minimize(meshMin, vector);
      meshMax = BABYLON.Vector3.Maximize(meshMax, vector);
    }

    const meshSize = meshMax.subtract(meshMin);
    const meshSpan = Math.max(meshSize.x, meshSize.y, meshSize.z);
    if (Number.isFinite(meshSpan) && meshSpan > 0) {
      entries.push({
        mesh,
        min: meshMin,
        max: meshMax,
        center: meshMin.add(meshMax).scale(0.5),
        span: meshSpan,
        vertices: mesh.getTotalVertices?.() ?? 0
      });
    }
  }

  const aggregate = {
    min,
    max,
    center: min.add(max).scale(0.5),
    meshes: renderMeshes
  };

  return {
    ...aggregate,
    display: selectDominantModelFrame(entries, aggregate, BABYLON) ?? aggregate
  };
}

function selectDominantModelFrame(entries, aggregate, BABYLON) {
  if (entries.length < 2) {
    return null;
  }

  const aggregateSize = aggregate.max.subtract(aggregate.min);
  const aggregateSpan = Math.max(aggregateSize.x, aggregateSize.y, aggregateSize.z);
  const largestMeshSpan = entries.reduce((span, entry) => Math.max(span, entry.span), 0);
  const sparseAssemblyRatio = aggregateSpan / Math.max(largestMeshSpan, 0.0001);

  if (!Number.isFinite(sparseAssemblyRatio) || sparseAssemblyRatio < 18) {
    return null;
  }

  const dominant = entries
    .slice()
    .sort((left, right) => (right.vertices * right.span) - (left.vertices * left.span))[0];

  if (!dominant) {
    return null;
  }

  return {
    min: dominant.min,
    max: dominant.max,
    center: dominant.center,
    meshes: [dominant.mesh]
  };
}

function updateWorldMatrixChain(node) {
  if (!node) {
    return;
  }

  if (node.parent) {
    updateWorldMatrixChain(node.parent);
  }

  const worldMatrix = node.computeWorldMatrix?.(true);
  if (worldMatrix && node.getBoundingInfo) {
    node.getBoundingInfo().update(worldMatrix);
  }
}

function observeDocumentTheme(state) {
  if (!state.themeApplicator || !("MutationObserver" in window)) {
    return;
  }

  state.themeObserver = new MutationObserver(mutations => {
    if (!mutations.some(mutation => mutation.attributeName === "data-theme")) {
      return;
    }

    state.themeApplicator();
    state.scene?.render();
  });
  state.themeObserver.observe(document.documentElement, {
    attributes: true,
    attributeFilter: ["data-theme"]
  });
}

function applyLandingHeroTheme(scene, plasticMaterial, lights, shadows, BABYLON) {
  const dark = document.documentElement.dataset.theme === "dark";
  scene.clearColor = BABYLON.Color4.FromHexString("#00000000");
  scene.environmentIntensity = dark ? 0.56 : 0.4;
  scene.ambientColor = BABYLON.Color3.FromHexString(dark ? "#2d3642" : "#eef2f6");

  lights.fill.intensity = dark ? 0.78 : 0.46;
  lights.fill.groundColor = BABYLON.Color3.FromHexString(dark ? "#273240" : "#d9e1ea");

  lights.key.intensity = dark ? 2.35 : 1.95;

  lights.softbox.intensity = dark ? 0.72 : 0.42;
  lights.softbox.diffuse = BABYLON.Color3.FromHexString(dark ? "#c3dbf4" : "#d9e7f6");

  lights.rim.intensity = dark ? 1.34 : 0.82;
  lights.rim.diffuse = BABYLON.Color3.FromHexString(dark ? "#a7ccef" : "#bfd7ee");

  lights.bounce.intensity = dark ? 0.42 : 0.22;
  lights.bounce.diffuse = BABYLON.Color3.FromHexString(dark ? "#596b83" : "#e1e8ef");

  lights.cameraHeadlight.intensity = dark ? 0.62 : 0.34;
  lights.cameraHeadlight.diffuse = BABYLON.Color3.FromHexString(dark ? "#f2f7ff" : "#f6f9fc");

  if (shadows?.shadowMaterial) {
    shadows.shadowMaterial.diffuseColor = BABYLON.Color3.FromHexString(dark ? "#27313c" : "#d9dfe7");
    shadows.shadowMaterial.alpha = dark ? 0.18 : 0.1;
  }

  if (shadows?.shadowGenerator?.setDarkness) {
    shadows.shadowGenerator.setDarkness(dark ? 0.34 : 0.28);
  }

  if (plasticMaterial) {
    plasticMaterial.albedoColor = BABYLON.Color3.FromHexString(dark ? "#9ea8b4" : "#8f99a6");
    plasticMaterial.reflectivityColor = BABYLON.Color3.FromHexString(dark ? "#8f9ba8" : "#b5bec8");
    plasticMaterial.specularIntensity = dark ? 0.28 : 0.24;
    plasticMaterial.environmentIntensity = dark ? 0.34 : 0.24;
    plasticMaterial.clearCoat.intensity = 0;
    disableCadMeshEdges(plasticMaterial.metadata?.cadMeshes ?? []);
  }
}

function configureSceneRuntime(state, engine, scene, cameraConfigurator) {
  state.engine = engine;
  state.scene = scene;
  state.cameraConfigurator = cameraConfigurator ?? null;
  state.visibilityHandler = () => document.hidden ? stopRenderLoop(state) : startRenderLoop(state);
  document.addEventListener("visibilitychange", state.visibilityHandler);

  if ("ResizeObserver" in window) {
    state.resizeObserver = new ResizeObserver(() => scheduleResizeScene(state));
    state.resizeObserver.observe(state.host ?? state.canvas);
  }

  state.resizeHandler = () => scheduleResizeScene(state);
  window.addEventListener("resize", state.resizeHandler, { passive: true });

  resizeScene(state, true);
}

function markReady(state) {
  state.host?.classList.remove("is-loading");
  state.host?.classList.add("is-ready");
  state.canvas.dataset.ready = "true";
  resizeScene(state, true);
  state.scene.render();
  scheduleResizeScene(state, true);
  startRenderLoop(state);
}

function createFixture(scene, BABYLON, root) {
  const body = new BABYLON.StandardMaterial("body", scene);
  body.diffuseColor = BABYLON.Color3.FromHexString("#0a72ef");
  body.specularColor = BABYLON.Color3.FromHexString("#7bb9ff");
  body.roughness = 0.72;

  const top = new BABYLON.StandardMaterial("top", scene);
  top.diffuseColor = BABYLON.Color3.FromHexString("#c7edff");
  top.specularColor = BABYLON.Color3.FromHexString("#ffffff");

  const cut = new BABYLON.StandardMaterial("cut", scene);
  cut.diffuseColor = BABYLON.Color3.FromHexString("#f3f9ff");
  cut.specularColor = BABYLON.Color3.FromHexString("#d6ecff");

  const block = BABYLON.MeshBuilder.CreateBox("part-body", {
    width: 2.25,
    height: 0.72,
    depth: 1.45
  }, scene);
  block.position.y = 0.55;
  block.material = body;
  block.parent = root;
  block.enableEdgesRendering(0.9);
  block.edgesWidth = 1.2;
  block.edgesColor = new BABYLON.Color4(0.03, 0.22, 0.42, 0.5);

  const topFace = BABYLON.MeshBuilder.CreateBox("part-top", {
    width: 2.28,
    height: 0.04,
    depth: 1.48
  }, scene);
  topFace.position.y = 0.93;
  topFace.material = top;
  topFace.parent = root;

  const relief = BABYLON.MeshBuilder.CreateCylinder("pocket", {
    diameter: 0.48,
    height: 0.06,
    tessellation: 28
  }, scene);
  relief.position = new BABYLON.Vector3(-0.46, 0.97, -0.25);
  relief.material = cut;
  relief.parent = root;

  const boss = BABYLON.MeshBuilder.CreateCylinder("boss", {
    diameter: 0.44,
    height: 0.26,
    tessellation: 28
  }, scene);
  boss.position = new BABYLON.Vector3(0.58, 1.08, 0.22);
  boss.material = top;
  boss.parent = root;
  boss.enableEdgesRendering(0.75);
  boss.edgesWidth = 1;
  boss.edgesColor = new BABYLON.Color4(0.03, 0.22, 0.42, 0.45);
}

function createAxes(scene, BABYLON, root) {
  const xMaterial = axisMaterial(scene, BABYLON, "x-axis", "#ff5b4f");
  const yMaterial = axisMaterial(scene, BABYLON, "y-axis", "#16a34a");
  const zMaterial = axisMaterial(scene, BABYLON, "z-axis", "#0a72ef");

  createAxis(scene, BABYLON, root, "x", xMaterial, new BABYLON.Vector3(1, 0, 0), new BABYLON.Vector3(0, 0, -Math.PI / 2));
  createAxis(scene, BABYLON, root, "y", yMaterial, new BABYLON.Vector3(0, 1, 0), BABYLON.Vector3.Zero());
  createAxis(scene, BABYLON, root, "z", zMaterial, new BABYLON.Vector3(0, 0, 1), new BABYLON.Vector3(Math.PI / 2, 0, 0));
}

function createAxis(scene, BABYLON, root, name, material, direction, rotation) {
  const origin = new BABYLON.Vector3(-1.35, 0.08, -1.0);
  const length = 1.08;
  const shaftLength = 0.82;
  const shaft = BABYLON.MeshBuilder.CreateCylinder(`${name}-axis-shaft`, {
    height: shaftLength,
    diameter: 0.035,
    tessellation: 12
  }, scene);
  shaft.position = origin.add(direction.scale(shaftLength / 2));
  shaft.rotation = rotation;
  shaft.material = material;
  shaft.parent = root;

  const cone = BABYLON.MeshBuilder.CreateCylinder(`${name}-axis-tip`, {
    height: 0.18,
    diameterTop: 0,
    diameterBottom: 0.12,
    tessellation: 16
  }, scene);
  cone.position = origin.add(direction.scale(length));
  cone.rotation = rotation;
  cone.material = material;
  cone.parent = root;
}

function createBed(scene, BABYLON, root) {
  const material = axisMaterial(scene, BABYLON, "bed", "#d4d4d4");
  const lines = [];
  for (let i = -3; i <= 3; i++) {
    const offset = i * 0.42;
    lines.push([
      new BABYLON.Vector3(-1.55, 0, offset),
      new BABYLON.Vector3(1.55, 0, offset)
    ]);
    lines.push([
      new BABYLON.Vector3(offset, 0, -1.25),
      new BABYLON.Vector3(offset, 0, 1.25)
    ]);
  }

  for (const [index, points] of lines.entries()) {
    const line = BABYLON.MeshBuilder.CreateLines(`bed-line-${index}`, { points }, scene);
    line.color = material.diffuseColor;
    line.parent = root;
  }
}

function axisMaterial(scene, BABYLON, name, hex) {
  const material = new BABYLON.StandardMaterial(name, scene);
  material.diffuseColor = BABYLON.Color3.FromHexString(hex);
  material.specularColor = BABYLON.Color3.FromHexString("#ffffff");
  return material;
}

function getRenderPixelRatio(highQuality = false) {
  const deviceRatio = Math.max(1, window.devicePixelRatio || 1);
  const mobile = window.matchMedia("(max-width: 680px)").matches;
  const maxRatio = highQuality ? 2.5 : 2;
  const minimumRatio = highQuality ? (mobile ? 1.75 : 2) : (mobile ? 1.5 : 1.2);

  return Math.min(maxRatio, Math.max(minimumRatio, deviceRatio));
}

function configureHardwareScaling(engine, renderRatio = getRenderPixelRatio()) {
  engine.setHardwareScalingLevel(1 / renderRatio);
  return renderRatio;
}

function resizeScene(state, force = false) {
  if (!state.engine) {
    return;
  }

  const host = state.host ?? state.canvas;
  const rect = host.getBoundingClientRect();
  const width = Math.round(rect.width);
  const height = Math.round(rect.height);
  const pixelRatio = window.devicePixelRatio || 1;
  const renderRatio = getRenderPixelRatio(state.highQualityRendering);
  const expectedCanvasWidth = Math.round(width * renderRatio);
  const expectedCanvasHeight = Math.round(height * renderRatio);
  const canvas = state.canvas;
  const backingBufferMatches =
    Math.abs(canvas.width - expectedCanvasWidth) <= 1 &&
    Math.abs(canvas.height - expectedCanvasHeight) <= 1;

  if (!width || !height) {
    return;
  }

  if (!force &&
    state.resizeWidth === width &&
    state.resizeHeight === height &&
    state.resizePixelRatio === pixelRatio &&
    state.resizeRenderRatio === renderRatio &&
    backingBufferMatches) {
    return;
  }

  state.resizeWidth = width;
  state.resizeHeight = height;
  state.resizePixelRatio = pixelRatio;
  state.resizeRenderRatio = renderRatio;
  configureHardwareScaling(state.engine, renderRatio);
  state.engine.resize(true);
  state.cameraConfigurator?.();
  state.scene?.render();
}

function scheduleResizeScene(state, force = false) {
  if (state.disposed) {
    return;
  }

  if (state.resizeFrame) {
    cancelAnimationFrame(state.resizeFrame);
  }

  state.resizeFrame = requestAnimationFrame(() => {
    state.resizeFrame = 0;
    resizeScene(state, force);
  });
}

function startRenderLoop(state) {
  if (!state.engine || !state.scene || state.looping || state.disposed || !state.visible || document.hidden) {
    return;
  }

  state.looping = true;
  state.engine.runRenderLoop(() => {
    if (state.visible && !document.hidden && !state.disposed) {
      state.scene.render();
    }
  });
}

function stopRenderLoop(state) {
  if (!state.engine || !state.looping) {
    return;
  }

  state.engine.stopRenderLoop();
  state.looping = false;
}

function clamp(value, min, max) {
  return Math.min(max, Math.max(min, value));
}
