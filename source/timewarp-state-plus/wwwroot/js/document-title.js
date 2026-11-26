// #region Purpose
// Reads document.title for TimeWarp.State.Plus without eval.
// #endregion

// Reads document.title without eval so a Content Security Policy can omit unsafe-eval.
export function getDocumentTitle() {
  return document.title;
}
