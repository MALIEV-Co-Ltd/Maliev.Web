// Lightweight, reusable scroll-reveal. Elements marked [data-reveal] fade/rise in
// when they enter the viewport; items inside a [data-reveal-group] stagger.
// Mirrors maliev-countup.js: self-mounting, IntersectionObserver-gated,
// reduced-motion aware, and MutationObserver-resilient to interactive re-renders.

const SELECTOR = "[data-reveal]";
const mounted = new WeakSet();

// Gate the hidden state on JS being present, so content is never stuck invisible
// if this module fails to load.
document.documentElement.classList.add("js-reveal");

mountReveals(document);
observeRevealMutations();

export function mountReveals(root = document) {
  if (!root) {
    return;
  }

  if (root.matches?.(SELECTOR)) {
    mountReveal(root);
  }

  root.querySelectorAll?.(SELECTOR).forEach(mountReveal);
}

function mountReveal(el) {
  if (mounted.has(el)) {
    return;
  }

  mounted.add(el);

  if (window.matchMedia?.("(prefers-reduced-motion: reduce)").matches || !("IntersectionObserver" in window)) {
    el.classList.add("is-revealed");
    return;
  }

  const observer = new IntersectionObserver(entries => {
    for (const entry of entries) {
      if (!entry.isIntersecting) {
        continue;
      }

      const group = entry.target.closest("[data-reveal-group]");
      if (group) {
        const peers = Array.from(group.querySelectorAll(SELECTOR));
        const index = Math.max(0, peers.indexOf(entry.target));
        entry.target.style.transitionDelay = `${Math.min(index, 8) * 45}ms`;
      }

      entry.target.classList.add("is-revealed");
      observer.unobserve(entry.target);
    }
  }, { rootMargin: "0px 0px -8% 0px", threshold: 0.12 });

  observer.observe(el);
}

function observeRevealMutations() {
  if (!("MutationObserver" in window)) {
    return;
  }

  const observer = new MutationObserver(records => {
    for (const record of records) {
      for (const node of record.addedNodes) {
        if (node instanceof Element) {
          mountReveals(node);
        }
      }
    }
  });

  if (document.body) {
    observer.observe(document.body, { childList: true, subtree: true });
  }
}
