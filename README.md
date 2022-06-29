# Toolsfactory.Protocols.D0

This little project consists of a library that allows reading the D0/IEC62056-21 protocol used by energy meters. The sample app makes use of this library and reads the data using an IR reader connected to a RaspPi via serial over USB. The App also shows how a linux service canb be implemented in .NET Core 3.1 by making use of the generic ServiceHost classes. 

Test Setup
--------

* RasPi 2
* .Net Core 3.1
* IR Reader (https://shop.weidmann-elektronik.de/index.php?page=product&info=24)
* Logarex LK13BD Energy Meter
