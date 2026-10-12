// Applies the DevTools theme on <html>. "system" clears data-theme so prefers-color-scheme wins.
window.timeWarpDevToolsTheme = {
  apply(theme) {
    const root = document.documentElement;
    if (!theme || theme === "system") {
      root.removeAttribute("data-theme");
      return;
    }

    root.setAttribute("data-theme", theme);
  }
};
