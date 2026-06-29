// Drives the landing hero "Make Studio" window: types a prompt into the composer,
// reveals a user message + an agent response, then cycles through the scenes.
// Content (all bilingual) lives in the Blazor markup; this module only orchestrates
// the timeline. Mirrors maliev-countup.js: self-mounting, IntersectionObserver-gated,
// and MutationObserver-resilient to interactive Server re-renders.

const HERO_SELECTOR = "[data-make-studio-hero]";
const mounted = new WeakSet();

const wait = ms => new Promise(resolve => setTimeout(resolve, ms));

mountHeroes(document);
observeHeroMutations();

export function mountHeroes(root = document) {
  if (!root) {
    return;
  }

  if (root.matches?.(HERO_SELECTOR)) {
    mountHero(root);
  }

  root.querySelectorAll?.(HERO_SELECTOR).forEach(mountHero);
}

function mountHero(root) {
  if (mounted.has(root)) {
    return;
  }

  const thread = root.querySelector(".thread");
  const projectName = root.querySelector(".pn");
  const placeholderEl = root.querySelector(".cinput .ph");
  const typedEl = root.querySelector(".cinput .typed");
  const caret = root.querySelector(".cinput .cur");

  if (!thread || !projectName || !placeholderEl || !typedEl || !caret) {
    return;
  }

  mounted.add(root);

  const placeholderText = placeholderEl.textContent;
  const idleProjectLabel = projectName.textContent;

  const chips = {};
  root.querySelectorAll(".qact[data-q]").forEach(chip => {
    chips[chip.dataset.q] = chip;
  });

  const scenes = Array.from(root.querySelectorAll(".msg.user[data-sc]"))
    .sort((a, b) => Number(a.dataset.sc) - Number(b.dataset.sc))
    .map(userEl => ({
      composer: userEl.dataset.msComposer || userEl.textContent.trim(),
      project: userEl.dataset.msProject || idleProjectLabel,
      action: userEl.dataset.msAction || "",
      userEl,
      aiEl: root.querySelector(`.msg.ai[data-sc="${userEl.dataset.sc}"]`)
    }))
    .filter(scene => scene.aiEl);

  if (!scenes.length) {
    return;
  }

  const resetComposer = () => {
    placeholderEl.textContent = placeholderText;
    typedEl.textContent = "";
    caret.style.display = "none";
  };

  const hideAll = () => {
    root.querySelectorAll(".msg").forEach(message => message.classList.remove("on"));
    Object.values(chips).forEach(chip => chip.classList.remove("active"));
    projectName.textContent = idleProjectLabel;
    thread.scrollTop = 0;
  };

  let visible = false;
  if ("IntersectionObserver" in window) {
    const observer = new IntersectionObserver(entries => {
      visible = entries.some(entry => entry.isIntersecting);
    }, { threshold: 0.15 });
    observer.observe(root);
  } else {
    visible = true;
  }

  const waitUntilVisible = () => {
    if (visible && !document.hidden) {
      return Promise.resolve();
    }

    return new Promise(resolve => {
      const check = () => {
        if (!root.isConnected || (visible && !document.hidden)) {
          resolve();
          return;
        }

        setTimeout(check, 220);
      };

      check();
    });
  };

  const typeComposer = async text => {
    placeholderEl.textContent = "";
    caret.style.display = "inline-block";
    typedEl.textContent = "";
    for (const character of text) {
      if (!root.isConnected) {
        return;
      }

      typedEl.textContent += character;
      await wait(26);
    }
  };

  const run = async () => {
    for (;;) {
      for (const scene of scenes) {
        if (!root.isConnected) {
          return;
        }

        await waitUntilVisible();
        hideAll();
        resetComposer();
        await wait(700);

        if (scene.action && chips[scene.action]) {
          chips[scene.action].classList.add("active");
        }
        await wait(480);

        await typeComposer(scene.composer);
        await wait(360);

        scene.userEl.classList.add("on");
        resetComposer();
        thread.scrollTop = thread.scrollHeight;
        await wait(900);

        scene.aiEl.classList.add("on");
        projectName.textContent = scene.project;
        await wait(120);
        thread.scrollTop = thread.scrollHeight;

        await wait(3600);
      }
    }
  };

  run();
}

function observeHeroMutations() {
  if (!("MutationObserver" in window)) {
    return;
  }

  const observer = new MutationObserver(records => {
    for (const record of records) {
      for (const node of record.addedNodes) {
        if (node instanceof Element) {
          mountHeroes(node);
        }
      }
    }
  });

  if (document.body) {
    observer.observe(document.body, { childList: true, subtree: true });
  }
}
