## Environment 

This is my set up for configuring a development machine.

---

### Operating System - Environment 
(I prefer to install on local machine but if you don't have windows local you can create one in Azure) 

* Azure (optional)
  * Create [Azure](https://portal.azure.com) Windows 10 VM with VS2017
    * Size: Standard DS2 v2 (2 cores, 7 GB memory) - This should do.
    * Add Data Disk to VM via Azure. 
    * [Expand azure hard drive to 1GB](http://www.thefreezeteam.com/2017/01/15/azure-resize-os-drive/)
  * [Consider automating the hours of operation to save credits/money](https://docs.microsoft.com/en-us/azure/automation/automation-solution-vm-management)
* Run Windows 10 `Check for updates` and install.
* Turn on [Developer mode](https://docs.microsoft.com/en-us/windows/uwp/get-started/enable-your-device-for-development?ranMID=43674&ranEAID=je6NUbpObpQ&ranSiteID=je6NUbpObpQ-nPshwmccHxiorEZnfb1q_g&epi=je6NUbpObpQ-nPshwmccHxiorEZnfb1q_g&irgwc=1&OCID=AID681541_aff_7795_1243925&tduid=(ir__ryu66hskwgkfr3rexmlij6lydu2xhprhypuv6hfx00)(7795)(1243925)(je6NUbpObpQ-nPshwmccHxiorEZnfb1q_g)()&irclickid=_ryu66hskwgkfr3rexmlij6lydu2xhprhypuv6hfx00).
* Disable "Hide extensions for known file types"
* Enable "Show hidden files, folders, and drives"
* Run [VS2017 installer](https://visualstudio.microsoft.com/)
* Go to [nininte.com](https://ninite.com/) and install:
  * VSCode 
  * Chrome 
  * Notepad++ 
  * pin all 3 above to task bar.
* Open Chrome and set it as your default browser.
* Ensure [React devtools extention](https://chrome.google.com/webstore/detail/react-developer-tools/fmkadmapgofadopljbjfkapdkoienihi) is added to Chrome
* Ensure [Redux devtools extention](https://chrome.google.com/webstore/detail/redux-devtools/lmhkpmbekcpmknklioeibfkpmmfibljd?hl=en) is added to Chrome
* install [chocolatey](https://chocolatey.org/)

#### Cmder (Console Emulator)
* Download [Cmder](http://cmder.net/) with git-for-windows
* Extract into `Program Files\cmder` 
* Execute cmder.  
* In Cmder settings Configure PowerShell as default console. 
![](/content/images/2017/01/2017-01-15_1720.png)

* Under Startup->Tasks [Remove the -No Profile from the PowerShell configuration](https://superuser.com/questions/956182/cmder-powershell-ignores-profiles).
![](/content/images/2018/03/2018-03-15_2152.png)
* Select Keys & Macro and search for split. Set the following:
![](/content/images/2018/03/2018-03-15_2150.png)
* Open Cmder in Powershell and `Install-Module posh-git`
* Pin Cmder to task bar or Start Menu.
 

## Dev-Environment
* Install [yarn](https://yarnpkg.com/en/docs/install#windows-tab)
* Install [Node](https://nodejs.org/en/) 10.13.0 (for this blog)

Confirm Node is installed and in the path. Open Cmder and run `node -v`.  you should see the following:

```
λ  node -v
v10.13.0
```
While at it what determine the version of npm and yarn? `npm -v`

```
λ  npm -v
6.1.0

λ  yarn -v
1.12.3
```

### VS Code Plugins

* [VS Live Share extension pack](https://marketplace.visualstudio.com/items?itemName=MS-vsliveshare.vsliveshare-pack)
* C# `ext install csharp`
* TSLint `ext install tslint`
* ESLint `ext install vscode-eslint`
* PowerShell `ext install PowerShell`

### Visual Studio Extentions

* Code Maid
* ASP.NET Blazor Language Services
* VS Live Share Preview
* [Mads WebCompiler](https://github.com/madskristensen/WebCompiler)
* Pretty much anything Mads has is good.
  * F2 to Add new files Extension

### gmaster 

The best git client. https://gmaster.io/

> let's write some code.
