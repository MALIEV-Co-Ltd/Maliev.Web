const babylonCdn = "https://cdn.jsdelivr.net/npm/babylonjs@9.6.0/babylon.min.js";
const instances = new WeakMap();
let babylonRuntime;

export function mountManufacturingGizmo(canvas) {
  if (!canvas || instances.has(canvas)) {
    return;
  }

  const host = canvas.parentElement;
  const state = {
    canvas,
    host,
    scene: null,
    engine: null,
    disposed: false,
    started: false,
    visible: false,
    looping: false,
    observer: null,
    resizeObserver: null,
    resizeHandler: null,
    visibilityHandler: null
  };

  instances.set(canvas, state);
  host?.classList.add("is-loading");

  const start = () => {
    if (state.started || state.disposed) {
      return;
    }

    state.started = true;
    loadBabylon()
      .then(BABYLON => {
        if (!state.disposed) {
          createGizmoScene(state, BABYLON);
        }
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

  state.scene?.dispose();
  state.engine?.dispose();
  state.host?.classList.remove("is-loading", "is-ready", "is-offline");
  delete state.canvas.dataset.ready;
  instances.delete(canvas);
}

function loadBabylon() {
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

function createGizmoScene(state, BABYLON) {
  const { canvas } = state;
  const engine = new BABYLON.Engine(canvas, true, {
    antialias: true,
    stencil: false,
    preserveDrawingBuffer: false,
    powerPreference: "low-power"
  }, false);

  configureHardwareScaling(engine);

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

  state.engine = engine;
  state.scene = scene;
  state.visibilityHandler = () => document.hidden ? stopRenderLoop(state) : startRenderLoop(state);
  document.addEventListener("visibilitychange", state.visibilityHandler);

  if ("ResizeObserver" in window) {
    state.resizeObserver = new ResizeObserver(() => resizeEngine(engine));
    state.resizeObserver.observe(state.host ?? canvas);
  }

  state.resizeHandler = () => resizeEngine(engine);
  window.addEventListener("resize", state.resizeHandler, { passive: true });

  resizeEngine(engine);
  state.host?.classList.remove("is-loading");
  state.host?.classList.add("is-ready");
  canvas.dataset.ready = "true";
  scene.render();
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
  const maxRatio = mobile ? 1.15 : 1.5;
  engine.setHardwareScalingLevel(Math.max(1, deviceRatio / maxRatio));
}

function resizeEngine(engine) {
  configureHardwareScaling(engine);
  engine.resize();
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
