# ConstantTWR
Maintain a constant or variable TWR during launch


If you are doing launches and getting frustrated by having to constantly modify the engine thrust to maintain the desired TWR, then this mod is for you.

If you have G force limits set in a hard mode, so that your Kerbals will die if they get too many G's, or if the vessel will break if there are too many G's, then this mod is for you.

The idea is simple:  Specify a TWR ratio to maintain during a launch.  The implementation is a bit more difficult.

This mod is under active development, feedback and ideas are welcome.

New Dependencies

Click Through Blocker
Currently, the mod has the ability vary the thrust of the vessel based on one of 4 different measurements:

Altitude
Speed
Mach
G-Limit
For each of these, the following can be specified:

A range of values of the measurement where the desired TWR can be set
A range of values for the TWR
Keep in mind that the TWR values you can set have nothing to do with the vessel;  the TWR range is merely a target for the throttle to be set to;  if the target TWR is greater than what the vessel can provide, the target throttle will be 100%.

The way it work is that when the measured value is at the lowest point of the range, then the lowest point of the TWR range is used as the target TWR.  As the measurement increases, the target TWR will increase, assuming there is a range of TWR values.   You can also set the TWR range so that the minimum is equal to the maximum, which would effectively have a constant TWR during the ascent.

You can specify multiple ranges, so that when the first range is finished, the next one will be used.  This will continue until all ranges have been used.

TWR profiles can be saved, but they are not specific to a vessel.  This allows you to define and name a TWR profile and save it, and then load it as necessary.

The windows are available both in the editor (VAB/SPH) and on the launch pad.  

When  you are ready with your profile, click the "Activate" button and then launch the vessel.

Now for what you are all waiting for:

Download Information

Spacedock: https://spacedock.info/mod/1429/ConstantTWR

Source code: https://github.com/linuxgurugamer/ConstantTWR

License: GPLv3

