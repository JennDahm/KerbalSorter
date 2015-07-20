# KerbalSorter 1.0 (In Progress)
A mod for Kerbal Space Program that allows players to sort their crew rosters.

This mod adds a small toolbar to every crew roster in the game outside of flight mode for sorting your kerbals! Currently supported sorting criterion are:

* Name
* Class
* Level
* Gender
* Number of Flights

If you have lots of kerbals, or are simply very organized, this is the mod for you!

See also:  
Development thread: http://forum.kerbalspaceprogram.com/threads/124612

This mod includes version checking using [Mini-AVC.](http://forum.kerbalspaceprogram.com/threads/79745) If you opt-in, it will use the internet to check whether there is a new version available. Data is only read from the internet and no personal information is sent. For a more comprehensive version checking experience, please download the [KSP-AVC Plugin.](http://forum.kerbalspaceprogram.com/threads/79745)

## Future Plans

* Add filtering.


## To-Do

* Figure out how to change default crew auto-assignment.
* Add fly-in and fly-out animations for the sorting buttons on expansion and collapse?


## Bugs

These are the known bugs at the moment. Please file an issue if you find another one!
* In the Launch Windows and Editors, when dragging and dropping a kerbal into the list of available crew, the kerbal is not sorted back into the available list. I seem to have no way to detect when this is done, nor any way to disable it. If you drag and drop your kerbals, you can click on one of the kerbals in the available list to re-sort them.


## How to Contribute
Just fork it and submit a pull request to the dev branch when you've made the changes you want to see!

Setting up the project is easy. First, create a folder called "lib" in the base folder of the project, then copy the Assembly-CSharp and UnityEngine DLLs from your installation of KSP into that folder. Then, copy the images from the latest release into a new folder called "Images" in the output folder ("project_dir/KerbalSorter"). If you want to test with Mini-AVC, put the latest DLL alongside the mod in your test environment.

This project requires Visual Studio 2012 or greater. It should be compatible with the free version, and with newer versions.


## License

The code in this repository is licensed under the BSD 3-Clause license. Please see the LICENSE file included in this repo and in every release for the full text.

Four icons used in this mod (the wrench, steering wheel, and vial, as used in the sorting-by-class button, and the rocket in the sorting-by-num-flights button) were taken from game-icons.net, and are licensed by them under the Creative Commons 3.0 License. With the exception of the rocket, I did not modify them in any way, except to display them on buttons in the game. The rocket was modified from its original format to remove an exhaust effect and to rotate the ship, as well as to add arrows. All other icons were made by me (Jon Dahm) and are public domain.
