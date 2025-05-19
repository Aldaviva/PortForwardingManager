![PortForwardingService](https://github.com/Aldaviva/PortForwardingManager/raw/master/PortForwardingService/pifmgr_30.ico) PortForwardingService
===

Integrate the [qBittorrent](https://www.qbittorrent.org) BitTorrent client with the [Private Internet Access (PIA)](https://www.privateinternetaccess.com/) virtual private network service.

<!-- MarkdownTOC autolink="true" bracket="round" autoanchor="false" levels="1,2" bullets="1.,-" -->

1. [Features](#features)
1. [Requirements](#requirements)
1. [Installation](#installation)
1. [Configuration](#configuration)

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