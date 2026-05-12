const babylonCdn = "https://cdn.jsdelivr.net/npm/babylonjs@9.6.0/babylon.min.js";
const babylonLoadersCdn = "https://cdn.jsdelivr.net/npm/babylonjs-loaders@9.6.0/babylonjs.loaders.min.js";
const instances = new WeakMap();
let babylonRuntime;
let babylonLoadersRuntime;

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
    visibilityHandler: null,
    pointerEnterHandler: null,
    pointerMoveHandler: null,
    pointerLeaveHandler: null,
    contextMenuHandler: null,
    cameraConfigurator: null,
    themeObserver: null,
    themeApplicator: null
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
  } else {
    state.visible = true;
    start();
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

  if (!babylonRuntime) {
    babylonRuntime = new Promise((resolve, reject) => {
      const script = document.createElement("script");
      script.src = babylonCdn;
      script.async = true;
      script.crossOrigin = "anonymous";
      script.onload = () => window.BABYLON ? resolve(window.BABYLON) : reject(new Error("BabylonJS did not initialize."));
      script.onerror = reject;
      document.head.appendChild(script);
    });
  }

  return babylonRuntime;
}

function loadBabylonLoaders() {
  if (window.BABYLON?.GLTFFileLoader) {
    return Promise.resolve();
  }

  if (!babylonLoadersRuntime) {
    babylonLoadersRuntime = new Promise((resolve, reject) => {
      const script = document.createElement("script");
      script.src = babylonLoadersCdn;
      script.async = true;
      script.crossOrigin = "anonymous";
      script.onload = () => resolve();
      script.onerror = reject;
      document.head.appendChild(script);
    });
  }

  return babylonLoadersRuntime;
}

function createEngine(canvas, BABYLON) {
  const engine = new BABYLON.Engine(canvas, true, {
    alpha: true,
    antialias: true,
    premultipliedAlpha: false,
    stencil: false,
    preserveDrawingBuffer: false,
    powerPreference: "low-power"
  }, false);

  configureHardwareScaling(engine);
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
  const engine = createEngine(canvas, BABYLON);
  restoreNativeCanvasBehavior(state);
  const scene = new BABYLON.Scene(engine);
  scene.clearColor = BABYLON.Color4.FromHexString("#ffffff00");
  scene.skipPointerMovePicking = true;
  scene.environmentIntensity = 0.42;

  const camera = new BABYLON.ArcRotateCamera(
    "landing-camera",
    -Math.PI / 2.55,
    Math.PI / 2.65,
    5.15,
    new BABYLON.Vector3(0, 0.08, 0),
    scene);
  camera.lowerRadiusLimit = camera.radius;
  camera.upperRadiusLimit = camera.radius;
  camera.panningSensibility = 0;
  camera.inputs.clear();
  configureLandingHeroCamera(camera, state.host, BABYLON);

  const fill = new BABYLON.HemisphericLight("landing-fill", new BABYLON.Vector3(-0.5, 1, 0.2), scene);

  const key = new BABYLON.DirectionalLight("landing-key", new BABYLON.Vector3(-0.5, -0.75, -0.45), scene);
  key.position = new BABYLON.Vector3(4.5, 6, 5);

  const rim = new BABYLON.PointLight("landing-rim", new BABYLON.Vector3(-3.4, 2.1, -2.5), scene);

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

  frameImportedModel(renderMeshes, root, BABYLON);
  state.themeApplicator = () => applyLandingHeroTheme(
    scene,
    plasticMaterial,
    { fill, key, rim },
    BABYLON);
  state.themeApplicator();
  observeDocumentTheme(state);

  const baseRotation = new BABYLON.Vector3(-0.05, -0.36, 0.02);
  root.rotation.copyFrom(baseRotation);

  const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  if (state.enableHoverMotion && !reducedMotion) {
    addHoverMotion(state, scene, camera, root, baseRotation, BABYLON);
  }

  configureSceneRuntime(state, engine, scene, () => configureLandingHeroCamera(camera, state.host, BABYLON));
  markReady(state);
}

function configureLandingHeroCamera(camera, host, BABYLON) {
  const width = host?.clientWidth ?? 780;
  const height = host?.clientHeight ?? 520;
  const compact = width < 560 || height < 360;
  const wide = width > 920;

  camera.fov = compact ? 0.56 : 0.44;
  camera.radius = compact ? 6.9 : wide ? 7.35 : 7.05;
  camera.lowerRadiusLimit = camera.radius;
  camera.upperRadiusLimit = camera.radius;
  camera.target = new BABYLON.Vector3(0, 0.02, 0);
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
    pointer.targetY = ((event.clientY - rect.top) / rect.height - 0.5) * 2;
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
    root.rotation.y = baseRotation.y + hoverX * 0.12;
    root.rotation.z = baseRotation.z - hoverX * 0.014;
    camera.alpha = baseAlpha + hoverX * 0.026;
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
  plastic.roughness = 0.37;
  plastic.microSurface = 0.64;
  plastic.clearCoat.isEnabled = true;
  plastic.clearCoat.intensity = 0.22;
  plastic.clearCoat.roughness = 0.44;

  for (const mesh of meshes) {
    mesh.material = plastic;
  }

  return plastic;
}

function frameImportedModel(meshes, root, BABYLON) {
  const bounds = computeMeshBounds(meshes, BABYLON);
  if (!bounds) {
    return;
  }

  const size = bounds.max.subtract(bounds.min);
  const maxDimension = Math.max(size.x, size.y, size.z) || 1;
  const targetSize = 2.28;
  const scale = targetSize / maxDimension;
  root.scaling.setAll(scale);
  root.position.copyFrom(bounds.center.scale(-scale));
}

function computeMeshBounds(meshes, BABYLON) {
  const renderMeshes = meshes.filter(mesh => mesh.getBoundingInfo);
  if (!renderMeshes.length) {
    return null;
  }

  let min = new BABYLON.Vector3(Number.POSITIVE_INFINITY, Number.POSITIVE_INFINITY, Number.POSITIVE_INFINITY);
  let max = new BABYLON.Vector3(Number.NEGATIVE_INFINITY, Number.NEGATIVE_INFINITY, Number.NEGATIVE_INFINITY);

  for (const mesh of renderMeshes) {
    mesh.computeWorldMatrix(true);
    const vectors = mesh.getBoundingInfo().boundingBox.vectorsWorld;
    for (const vector of vectors) {
      min = BABYLON.Vector3.Minimize(min, vector);
      max = BABYLON.Vector3.Maximize(max, vector);
    }
  }

  return {
    min,
    max,
    center: min.add(max).scale(0.5)
  };
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

function applyLandingHeroTheme(scene, plasticMaterial, lights, BABYLON) {
  const dark = document.documentElement.dataset.theme === "dark";
  scene.clearColor = BABYLON.Color4.FromHexString("#00000000");
  scene.environmentIntensity = dark ? 0.34 : 0.42;

  lights.fill.intensity = dark ? 0.92 : 1.25;
  lights.key.intensity = dark ? 1.78 : 2.1;
  lights.rim.intensity = dark ? 1.18 : 0.64;
  lights.rim.diffuse = BABYLON.Color3.FromHexString(dark ? "#8fc3ff" : "#f6f6f6");

  if (plasticMaterial) {
    plasticMaterial.albedoColor = BABYLON.Color3.FromHexString(dark ? "#242a31" : "#171717");
    plasticMaterial.reflectivityColor = BABYLON.Color3.FromHexString(dark ? "#d7e5f5" : "#f5f5f5");
    plasticMaterial.clearCoat.intensity = dark ? 0.3 : 0.22;
  }

}

function configureSceneRuntime(state, engine, scene, cameraConfigurator) {
  state.engine = engine;
  state.scene = scene;
  state.cameraConfigurator = cameraConfigurator ?? null;
  state.visibilityHandler = () => document.hidden ? stopRenderLoop(state) : startRenderLoop(state);
  document.addEventListener("visibilitychange", state.visibilityHandler);

  if ("ResizeObserver" in window) {
    state.resizeObserver = new ResizeObserver(() => resizeScene(state));
    state.resizeObserver.observe(state.host ?? state.canvas);
  }

  state.resizeHandler = () => resizeScene(state);
  window.addEventListener("resize", state.resizeHandler, { passive: true });

  resizeScene(state);
}

function markReady(state) {
  state.host?.classList.remove("is-loading");
  state.host?.classList.add("is-ready");
  state.canvas.dataset.ready = "true";
  state.scene.render();
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

function configureHardwareScaling(engine) {
  const deviceRatio = Math.max(1, window.devicePixelRatio || 1);
  const mobile = window.matchMedia("(max-width: 640px)").matches;
  const maxRatio = mobile ? 1.35 : 2;
  const minimumRatio = mobile ? 1.1 : 1.2;
  const renderRatio = Math.min(maxRatio, Math.max(minimumRatio, deviceRatio));
  engine.setHardwareScalingLevel(1 / renderRatio);
}

function resizeScene(state) {
  if (!state.engine) {
    return;
  }

  state.cameraConfigurator?.();
  configureHardwareScaling(state.engine);
  state.engine.resize();
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
