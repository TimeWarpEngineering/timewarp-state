// TODO:  Make this Typescript attach it to window.TimeWarp to avoid conflicts with other libraries

// wwwroot/js/downloadFile.js

/**
 * Downloads a file from a stream.
 * @param {string} fileName - The name of the file to download.
 * @param {any} contentStreamReference - The stream reference of the content.
 */
async function downloadFileFromStream(fileName, contentStreamReference) {
  console.log('**** Javascript downloadFileFromStream');
  const arrayBuffer = await contentStreamReference.arrayBuffer();
  const blob = new Blob([arrayBuffer]);
  const url = URL.createObjectURL(blob);
  const anchorElement = document.createElement('a');
  anchorElement.href = url;
  anchorElement.download = fileName ?? 'download';
  anchorElement.click();
  anchorElement.remove();
  URL.revokeObjectURL(url);
}

// Add the function to the global window object.
// The name must align with the constant defined in TimeWarp.Architecture.JavaScriptInteropConstants in C#.
window.downloadFileFromStream = downloadFileFromStream;
