🚢 PortForwardingManager
===

Integrate the [qBittorrent](https://www.qbittorrent.org) BitTorrent client with the [Private Internet Access (PIA)](https://www.privateinternetaccess.com/) virtual private network service.

## Features
- Automatically set the qBittorrent listening port to the PIA forwarded port any time it changes.
    - This makes NAT traversal work and avoids getting blocked from connecting to as many peers as possible, leading to likely faster transfer speeds.
- Automatically fix the random `Failed to listen on IP. IP: "1.2.3.4". Port: "TCP/12345". Reason: "The requested address is not valid in its context"` qBittorrent errors.
    - This error occurs intermittently and blocks all transfers until you restart qBittorrent or change the listening port.
 
## Requirements
- Windows
- [.NET Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework) or later (installed by default in Windows 10 v1903, Server 2022, and later)
- [qBittorrent](https://www.qbittorrent.org/download)
- [Private Internet Access desktop client](https://www.privateinternetaccess.com/download/windows-vpn)

## Installation
1. Download [`PortForwardingManager.zip`](https://github.com/Aldaviva/PortForwardingManager/releases/latest/download/PortForwardingService.zip) from the [latest release](https://github.com/Aldaviva/PortForwardingManager/releases/latest).
1. Extract the ZIP file to a directory such as `C:\Program Files\PortForwardingManager\`.
1. In an elevated PowerShell terminal, run
    ```ps1
    New-Service -Name "PortForwardingManager" -BinaryPathName "C:\Program Files\PortForwardingManager\PortFowardingManager.exe" -DisplayName "Port Forwarding Manager" -Description "Automatically update the qBittorrent listening port to be the Private Internet Access forwarded port whenever it changes."
    ```
    replacing the installation directory if you chose a different one in step 2.
1. Run `services.msc`.
1. Select the Port Fowarding Manager service.
1. Go to Properties › Log On, choose This account, and type in your Windows username and password.
    - This allows the service to edit the `qBittorrent.ini` configuration file in your user profile directory, which is used to set the listening port when qBittorrent is not running.
1. Start the service.
