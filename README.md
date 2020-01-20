# suspend-peripherals-service

Windows C# service that detects sleep and hibernation events and writes commands to serial COM port


# What is this?

I've got a bunch of devices connected to my home desktop PC: Display, Printer, Speakers. They all draw some amount of power, even on standby. The winner here are 5.1 Creative speakers - they consume around 15W of my precious electricity. 


I've also found this USB relay board, that I've bought 10 years ago.

![USB relay board](/usb_relay_board.jpeg)


It wasn't easy to get it working, because the instructions and the driver are long gone. Luckily Windows 10 recognizes FT232BL chip as a USB serial port controller. Also I've found the correct baud rate (which is 2400) in an old ebay message from the seller.

Now I can use putty to send command over to COM port. Certain symbols enable and disable the relays. E.g. typing "a" would connect relay num1 and "i" would disconnect it.

The next challenge is to issue these commands when computer enters sleep and wakes up. I will not normally shutdown, because my Watt meter shows exactly 0W when PC is in sleep state. I can use WoL magic packets to wake it up remotely (woohoo).

Anyways, using Windows 10 task scheduler proved to be impossible. The event to turn off relays would sometimes fire after the PC is already sleeping.

I have no choice but to write my own service that gets notified by windows power management API that PC is going to sleep.

# What else

* I have to use hidden window to capture windows sleep and resume events as described in MSDN
* I'll run a timer to re-open the COM3 port in case the USB relay board gets unpluged
* Then also we want to turn power on when service starts and turn it off upon service shutdown
* I havent done C# since university 10 years ago, please don't hate me
* Can't just run service to debug. It needs to be installed and managed via Window services thingie
* Success! Now just need a proper enclosure to house the PCB and connect 220V power strip to the relays.
* To handle high currents (laser priter) I will use 2 relays per wire. I will not know which one is the phase wire, so better to disconnect both just to be safe.

# Is there a commercial solution?
YES!
https://www.amazon.co.uk/HQ-IEL-ES04HQ-PC-Power-Saving-Outlet/dp/B002XO4MPA

I had one of these. It measures current on a certain outlet and then is able to disconnect a relay when power falls beyond a certain threshold. Why this wasn't great:

1) Difficult to setup
2) There was a separate button to enable peripherals. Not sure that would have worked with WoL setup.
3) It broke :(
