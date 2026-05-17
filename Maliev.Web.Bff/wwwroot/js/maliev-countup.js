const counterSelector = "[data-count-up]";
const mountedCounters = new WeakSet();

mountMetricCounters(document);
observeMetricCounterMutations();

export function mountMetricCounters(root = document) {
  if (!root) {
    return;
  }

  if (root.matches?.(counterSelector)) {
    mountCounter(root);
  }

  root
    .querySelectorAll?.(counterSelector)
    .forEach(mountCounter);
}

function mountCounter(counter) {
  if (mountedCounters.has(counter)) {
    return;
  }

  const target = Number(counter.dataset.countTarget);
  if (!Number.isFinite(target)) {
    return;
  }

  mountedCounters.add(counter);
  const finalText = counter.dataset.countFinal || counter.textContent?.trim() || "";
  const suffix = counter.dataset.countSuffix || "";

  if (window.matchMedia?.("(prefers-reduced-motion: reduce)").matches) {
    counter.textContent = finalText;
    return;
  }

  const runAnimation = () => {
    const counterGroup = counter.closest(".metric-strip") ?? document;
    const counters = Array.from(counterGroup.querySelectorAll(counterSelector));
    const counterIndex = Math.max(0, counters.indexOf(counter));
    const staggerDelay = counterIndex * 180;

    setTimeout(() => animateCounter(counter, target, suffix, finalText), staggerDelay);
  };
  if ("IntersectionObserver" in window) {
    const observer = new IntersectionObserver(entries => {
      if (!entries.some(entry => entry.isIntersecting)) {
        return;
      }

      observer.disconnect();
      runAnimation();
    }, { rootMargin: "0px 0px -8% 0px", threshold: 0.35 });

    observer.observe(counter);
    return;
  }

  runAnimation();
}

function animateCounter(counter, target, suffix, finalText) {
  const formatter = new Intl.NumberFormat("en-US", { maximumFractionDigits: 0 });
  const startedAt = performance.now();
  const defaultDuration = 2400;
  const duration = Number(counter.dataset.countDuration || defaultDuration);
  counter.dataset.countState = "running";

  const tick = now => {
    const progress = Math.min(1, (now - startedAt) / duration);
    const eased = 1 - Math.pow(1 - progress, 3);
    const value = Math.round(target * eased);

    counter.textContent = `${formatter.format(value)}${suffix}`;

    if (progress < 1) {
      requestAnimationFrame(tick);
      return;
    }

    counter.textContent = finalText;
    counter.dataset.countState = "complete";
    counter.dataset.countAnimated = "true";
  };

  counter.textContent = `${formatter.format(0)}${suffix}`;
  requestAnimationFrame(tick);
}

function observeMetricCounterMutations() {
  if (!("MutationObserver" in window)) {
    return;
  }

  const observer = new MutationObserver(records => {
    for (const record of records) {
      for (const node of record.addedNodes) {
        if (node instanceof Element) {
          mountMetricCounters(node);
        }
      }
    }
  });

  if (document.body) {
    observer.observe(document.body, { childList: true, subtree: true });
  }
}
