![PortForwardingService](https://github.com/Aldaviva/PortForwardingManager/raw/master/PortForwardingService/pifmgr_30.ico) PortForwardingService
===

[![Download count](https://img.shields.io/github/downloads/Aldaviva/PortForwardingManager/total?logo=github)](https://github.com/Aldaviva/PortForwardingManager/releases)

Integrate the [qBittorrent](https://www.qbittorrent.org) BitTorrent client with the [Private Internet Access (PIA)](https://www.privateinternetaccess.com/) virtual private network service.

<!-- MarkdownTOC autolink="true" bracket="round" autoanchor="false" levels="1,2" bullets="1.,-" -->

1. [Features](#features)
1. [Requirements](#requirements)
1. [Installation](#installation)
1. [Configuration](#configuration)
1. [Extensibility](#extensibility)

<!-- /MarkdownTOC -->

## Features
- Automatically set the qBittorrent listening port to the PIA forwarded port any time it changes.
    - This makes NAT traversal work and allows you to connect to as many peers as possible, likely speeding up your transfers.
    - UPnP does not integrate with PIA, only your router.
    - The VPN forwarded port can change at any time, so manually copying and pasting it from PIA into the qBittorrent settings would be annoying and unreliable.
- Automatically fix the random `Failed to listen on IP. IP: "1.2.3.4". Port: "TCP/12345". Reason: "The requested address is not valid in its context"` qBittorrent errors.
    - This error occurs intermittently and blocks all transfers until you restart qBittorrent or change the listening port.
 
## Requirements
- Windows
- [.NET Framework 4.8 runtime](https://dotnet.microsoft.com/en-us/download/dotnet-framework) or later (installed by default in Windows 10 v1903, Server 2022, and later)
- [qBittorrent](https://www.qbittorrent.org/download)
- [Private Internet Access desktop client](https://www.privateinternetaccess.com/download/windows-vpn)

## Installation
1. Download [`PortForwardingService.zip`](https://github.com/Aldaviva/PortForwardingManager/releases/latest/download/PortForwardingService.zip) from the [latest release](https://github.com/Aldaviva/PortForwardingManager/releases/latest).
1. Extract the ZIP file to a directory such as `C:\Program Files\PortForwardingService\`.
    - If you are upgrading an existing installation, don't overwrite `NLog.config` if you have previously modified it, or else your changes will be lost.
1. In an elevated PowerShell terminal, run
    ```ps1
    New-Service -Name "PortForwardingService" -BinaryPathName "C:\Program Files\PortForwardingService\PortFowardingService.exe" -DisplayName "Port Forwarding Service" -Description "Automatically update the qBittorrent listening port to be the Private Internet Access forwarded port whenever it changes."
    ```
    replacing the installation directory if you chose a different one in step 2.
1. Run `services.msc`.
1. Select `Port Fowarding Service`.
1. Go to Properties › Log On, choose `This account`, and type in your Windows username and password.
    - This allows the service to edit the `qBittorrent.ini` configuration file in your user profile directory, which is used to set the listening port when qBittorrent is not running.
1. Start the service.

## Configuration
1. [Enable VPN port forwarding.](https://helpdesk.privateinternetaccess.com/kb/articles/how-do-i-enable-port-forwarding-on-my-vpn)
    1. In the Private Internet Access desktop client, connect to a VPN server that allows port forwarding (does not have an ⇍ icon in the server list).
    1. Enable Settings › Network › Request Port Forwarding.
    1. Ensure PIA shows a forwarded port number under VPN IP, such as `↩ 12345`. If it doesn't, try reconnecting or choosing a different server.
1. Open qBittorrent › Tools › Options.
    1. Disable Connection › Use UPnP / NAT-PMP port forwarding from my router.
    1. Set Advanced › qBittorrent Section › Network Interface to the active PIA virtual network interface.
        - If you have enabled the WireGuard VPN protocol in PIA instead of OpenVPN, this interface will be named something like `wgpia0`.
        - If you're not sure which interface PIA is providing, open `ncpa.cpl` and rename the active connection that uses the Private Internet Access Network Adapter to a recognizable name like "PIA".
    1. Enable Web UI › Web User Interface.
    1. Set Web UI › Port to 8080.
    1. Enable Web UI › Bypass authentication for clients on localhost.
    1. Disable Web UI › Use UPnP / NAT-PMP to forward the port from my router.
    1. Disable Web UI › Use HTTPS instead of HTTP.
    1. Ensure the PIA forwarded port appears in qBittorrent › Connection › Listening Port. If it doesn't, try restarting Port Forwarding Service.

### Logging
By default, log messages are written to the text file `%LOCALAPPDATA%\PortForwardingService\logs\PortForwardingService.log`.

You can customize the [log format](https://nlog-project.org/config/?tab=layout-renderers), [filename](https://github.com/NLog/NLog/wiki/File-target), [level](https://github.com/NLog/NLog/wiki/Configuration-file#log-levels), and [other options](https://github.com/NLog/NLog/wiki/Configuration-file) by editing `NLog.config` in the installation directory.

## Extensibility
If you want this program to execute custom logic when the PIA forwarded port changes, such as sending you a notification, you can write a plugin.

These plugins take the form of .NET DLLs in the `plugins` subdirectory of the PortForwardingService installation directory. You can create such a library with the following steps.

### Prerequisites
- A .NET IDE
    - [Visual Studio 2026 Community Edition](https://visualstudio.microsoft.com/vs/community/) with the .NET Desktop Development workload
    - [Visual Studio Code](https://code.visualstudio.com/Download) with the [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) extension
    - [JetBrains Rider](https://www.jetbrains.com/rider/download/)

### Option A: Copy skeleton template project
1. Clone or download this repository, then copy the [`SamplePlugin`](https://github.com/Aldaviva/PortForwardingManager/tree/master/SamplePlugin) project.
1. Edit [`SampleProject.csproj`](https://github.com/Aldaviva/PortForwardingManager/blob/master/SamplePlugin/SamplePlugin.csproj) as desired, such as changing the `Authors` and `AssemblyName` properties.
1. Add your custom logic to [`SamplePlugin.cs`](https://github.com/Aldaviva/PortForwardingManager/blob/master/SamplePlugin/SamplePlugin.cs) where the TODOs indicate to put the behavior for initialization, handling forwarded port changes, and cleanup when the program shuts down.

### Option B: Create new empty library project
1. Create a new C# class library project.
1. Target .NET Framework 4.8.
    - If it doesn't let you choose this TFM, temporarily choose .NET Standard 2.0 or any other TFM, then change the `TargetFramework` property in the `.csproj` file to `net48`.
        ```xml
        <TargetFramework>net48</TargetFramework>
        ```
1. Declare a dependency on the [`PortForwardingService.Abstractions`](https://www.nuget.org/packages/PortForwardingService.Abstractions/) shared library.
    ```ps1
    dotnet add package PortFowardingService.Abstractions
    ```
    Make sure the `PackageReference` in the `.csproj` file has the `ExcludeAssets` attribute set to `runtime`, to prevent PortForwardingService from trying to load it twice.
1. Create a new class that implements the `PortForwardingService.Plugins.IPortForwardingServicePlugin` interface.
    ```cs
    #nullable enable

    using PortForwardingService.Plugins;

    namespace PortForwardingService.Plugins.SamplePlugin;

    public class SamplePlugin: IPortForwardingServicePlugin {

        public void OnForwardedPortChanged(ushort? newForwardedPort, ushort? oldForwardedPort) {
            // your logic here
        }

    }
    ```
1. Put your custom logic to handle forwarded port changes in the `OnForwardedPortChanged` method.
1. Put any initialization logic in a public, no-args constructor. This will get run once when the program starts.
    ```cs
    public class SamplePlugin: IPortForwardingServicePlugin {

        public SamplePlugin() {
            // initialization logic here
        }

        /// …

    }
    ```
1. If you have any cleanup logic to run when the program shuts down, such as deleting temporary files or disposing of objects, you can implement the `IDisposable` interface and its `Dispose` method.
    ```cs
    public class SamplePlugin: IPortForwardingServicePlugin, IDisposable {

        /// …

        public void Dispose() {
            // cleanup logic here
            GC.SuppressFinalize(this);
        }

    }
    ```
See [`SamplePlugin.cs`](https://github.com/Aldaviva/PortForwardingManager/blob/master/SamplePlugin/SamplePlugin.cs) for the complete example.

### Deploying
1. Build the project.
    ```ps1
    dotnet publish
    ```
1. Stop PortForwardingService.
1. Copy all of the DLLs and other files from the `bin\Debug\net48\publish` directory to the `plugins` subdirectory of the PortForwardingService installation directory, which you may have to create first.
1. Start PortForwardingService.