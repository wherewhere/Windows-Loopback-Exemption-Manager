# Project Description

This project is enhancement of [UWP-Loopback-Exemption-Manager](https://github.com/themerror/UWP-Loopback-Exemption-Manager).

[![LICENSE](https://img.shields.io/github/license/wherewhere/Windows-Loopback-Exemption-Manager.svg?label=License&style=flat-square)](https://github.com/wherewhere/Windows-Loopback-Exemption-Manager/blob/master/LICENSE "LICENSE")
[![Issues](https://img.shields.io/github/issues/wherewhere/Windows-Loopback-Exemption-Manager.svg?label=Issues&style=flat-square)](https://github.com/wherewhere/Windows-Loopback-Exemption-Manager/issues "Issues")
[![Stargazers](https://img.shields.io/github/stars/wherewhere/Windows-Loopback-Exemption-Manager.svg?label=Stars&style=flat-square)](https://github.com/wherewhere/Windows-Loopback-Exemption-Manager/stargazers "Stargazers")

[![Microsoft Store](https://img.shields.io/badge/download-下载-magenta.svg?label=Microsoft%20Store&logo=data:image/svg+xml;base64,PHN2ZyByb2xlPSJpbWciIHZpZXdCb3g9IjAgMCAyNCAyNCIgZmlsbD0iI2ZmZiIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48dGl0bGU+TWljcm9zb2Z0IFN0b3JlPC90aXRsZT48cGF0aCBkPSJNMTEuNCA5LjZ2NC4ySDcuMlY5LjZoNC4yem0wIDkuNlYxNUg3LjJ2NC4yaDQuMnptNS40LTkuNnY0LjJoLTQuMlY5LjZoNC4yem0wIDkuNlYxNWgtNC4ydjQuMmg0LjJ6TTcuMiA1LjRWMi43YzAtMS4xNi45NC0yLjEgMi4xLTIuMWg1LjRjMS4xNiAwIDIuMS45NCAyLjEgMi4xdjIuN2g2LjNhLjkuOSAwIDAgMSAuOS45djEzLjhhMy4zIDMuMyAwIDAgMS0zLjMgMy4zSDMuM0EzLjMgMy4zIDAgMCAxIDAgMjAuMVY2LjNhLjkuOSAwIDAgMSAuOS0uOWg2LjN6TTkgMi43djIuN2g2VjIuN2EuMy4zIDAgMCAwLS4zLS4zSDkuM2EuMy4zIDAgMCAwLS4zLjN6TTEuOCAyMC4xYTEuNSAxLjUgMCAwIDAgMS41IDEuNWgxNy40YTEuNSAxLjUgMCAwIDAgMS41LTEuNVY3LjJIMS44djEyLjl6Ii8+PC9zdmc+&style=for-the-badge&color=11a2f8)](https://www.microsoft.com/store/apps/9MVSFKBXNJV9 "Microsoft Store")
[![GitHub All Releases](https://img.shields.io/github/downloads/wherewhere/Windows-Loopback-Exemption-Manager/total.svg?label=DOWNLOAD&logo=github&style=for-the-badge)](https://github.com/wherewhere/Windows-Loopback-Exemption-Manager/releases/latest "GitHub All Releases")

GUI to enable Loopback Exemptions for Universal Windows Apps and Windows 8, 8.1, 10 Modern UI Apps.

By default, Windows Modern UI and Universal Apps are forbidden to send network traffic to the local Computer. In order to debug Apps with a tool, we need to enable Loopback capabilities for those Apps.
This tool enables the management of the Apps that can connect to the local Computer.

# More information about this topic
- [Revisiting Fiddler and Win8+ Immersive applications](https://docs.microsoft.com/en-us/archive/blogs/fiddler/revisiting-fiddler-and-win8-immersive-applications)
- [Fiddler and Windows 8 Metro-style applications](https://docs.microsoft.com/en-us/archive/blogs/fiddler/fiddler-and-windows-8-metro-style-applications)
- [How to enable loopback and troubleshoot network isolation (Windows Store apps)](https://docs.microsoft.com/en-us/previous-versions/windows/apps/hh780593(v=win.10))

# Network Isolation APIs
Check the source code of this project on a sample usage of the Network Isolation APIs : 
- NetworkIsolationEnumAppContainers 
- NetworkIsolationFreeAppContainers 
- NetworkIsolationGetAppContainerConfig 
- NetworkIsolationSetAppContainerConfig

![Loopback Exemption Manager](LoopbackSampleExecution.png)

This tool does basically the same thing as Fiddler EnableLoopback Utility.