function configureBlazor(applicationVersion) {
  console.log("served from Server");
  const executionSideKey = 'executionSide';
  const executionSideValue = localStorage.getItem(executionSideKey);
  const clientApplicationKey = 'clientApplication';
  const clientApplicationValue = localStorage.getItem(clientApplicationKey);
  const clientLoaded = clientApplicationValue === applicationVersion;
  window.TimeWarp = {
    applicationVersion,
    clientApplicationKey,
    clientLoaded,
    executionSideKey
  };
  if (executionSideValue === null) {
    localStorage.setItem(window.TimeWarp.executionSideKey, "To force a side set this to client/server");
  }
  const clientSideBlazorScript = '_framework/components.webassembly.js';
  const serverSideBlazorScript = '_framework/components.server.js';
  if (executionSideValue === 'client') {
    source = clientSideBlazorScript;
  } else if (executionSideValue === 'server') {
    source = serverSideBlazorScript;
  } else {
    source = clientLoaded ? clientSideBlazorScript : serverSideBlazorScript;
  }

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