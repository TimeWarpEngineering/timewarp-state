// Reads document.title without eval so a Content Security Policy can omit unsafe-eval.
export function getDocumentTitle() {
  return document.title;
}
