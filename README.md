# AltPlugAndPlay

## Description
This is the initial work done towards making a new alternative to Cisco's APIC-EM Plug and Play module.

It is more powerful in the sense that it will :
* Makes use of Option 82 for identifying where a device is located
* It will be tightly integrated with DHCP servers (currently Microsoft DHCP server)
* It will be designed to be integrated with Windows Certificate Server instead of integrating its own solution
* It is designed with command-line and configuration first in mind. It is meant to be used mostly from Powershell and Powershell DSC, 
other language bindings will be obvious
* It is designed initally to use TFTP as the protocol so that support can be provided for devices from all vendors with varying levels of security
* It will be event driven. Using Signal/R, it will not be necessary to poll the server for task completion or device events. All a client need do is
subscribe.
* It will expose the PnP interface to allow configuration management as well.

- More to come later.