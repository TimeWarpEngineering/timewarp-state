function configureBlazor(applicationVersion) {
  const clientApplicationKey = 'clientApplication';
  const clientApplicationValue = localStorage.getItem(clientApplicationKey);
  const clientLoaded = clientApplicationValue === applicationVersion;
  window.TimeWarp = {
    clientApplicationKey,
    applicationVersion,
    clientLoaded
  };

  const clientSideBlazorScript = '_framework/blazor.webassembly.js';
  const serverSideBlazorScript = '_framework/blazor.server.js';
  const forceLoadClientUrl = 'loadclient=true';

  const forceClientSide = window.location.search.includes(forceLoadClientUrl);
  const source = clientLoaded || forceClientSide ? clientSideBlazorScript : serverSideBlazorScript;
  console.log(`Using script: ${source}`);

  // Add the script element
  var blazorScript = document.createElement('script');
  blazorScript.setAttribute('src', source);
  document.body.appendChild(blazorScript);
  window.onload = loadClient;
}

function loadClient() {
  if (!window.TimeWarp.clientLoaded) {
    console.log('set the flag in localstorage');
    localStorage.setItem(window.TimeWarp.clientApplicationKey, window.TimeWarp.applicationVersion);
    console.log("load the client iframe into cache");
    var iframe = document.createElement('iframe');
    iframe.setAttribute('id', 'loaderFrame');
    iframe.setAttribute('style', 'width:0; height:0; border:0; border:none');
    document.body.appendChild(iframe);
    const iframeSource = "?loadclient=true";
    iframe.setAttribute("src", iframeSource);
  }
}