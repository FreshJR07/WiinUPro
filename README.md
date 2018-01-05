Christmas Release:

Bug Fix - Bluetooth Sync Radio Discovery Fix

Bug Fix - Disconnecting Controller while in Unmanaged Mode

Bug Fix - DeviceListener false triggers, will now only trigger by Nintendo HID devices

Feature - Bluetooth device power off / disconnect button

Feature - Bluetooth all devices power off / disconnect button

Feature - Auto disconnect / power off after user defined idle time

Feature - Auto Connect as XInput toggle

Feature - Open windows game controller panel

Feature - Device listener implemented and will detect/initialize devices upon established connection/disconnection immediately instead of searching for changes once every 5 sec with AutoRefresh

Feature - UI changes to make user understandability easier

Feature - Click on device icon to view MAC address


New Years Update: 

Feature - WiinUSoft.exe is the only file needed. All previous DLL's have been rolled into the executable itself.  They have also been compressed the to reduce filesize.  This single file is portable, but separate Scp driver installation is still needed on new machines.

Bug Fix - Disabled window resizing

Bug Fix - Disabled maximize/restore button

Feature - Click and drag anywhere to move window

Feature - Minimize instead of close (toggled on/off under settings)

Feature - Remember last closed window position and open in that position.

Feature - Tray Icon always visible, even when window is shown

Feature - Added bolded Exit button under settings, also bolded the exit button under tray icon right click

Feature - Changed RunAtStartup to use Task Scheduler instead of startup folder / registry entry (This is because Task scheduler startup programs have higher launch priority compared to start programs initiated by startup folder links or registry entries)

Feature - UserPrefs.config now gets saved into %AppData%/WiinUSoft folder and can be reset with button under settings

Feature - Executable supports flags/arguments when launching

-m to start minimized

-r to restart existing instance rather than giving focus existing instance



Instructions / Download:

Download WiinUSoft.exe located in WiinUSoft_Release folder on GitHub
Feature - Click on device icon to view MAC address

github.com/FreshPr/WiinUPro

github.com/FreshPr/WiinUPro/tree/master/WiinUSoft_Release



Compiling Instructions (Advanced):

If you have Visual Studio MsBuild 15.0 and the .NETFramework reference assemblies installed installed, then you can simply use build.bat to compile the program.  (change the directory of the project to match your location)
I installed the two above components with "vs_BuildTools.exe" supplied by microsoft.com. 
I used "Other Install" and selected the bare minimum amount of stuff required to compile.



Enjoy and Happy New Years!!
