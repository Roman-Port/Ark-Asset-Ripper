# Ark-Asset-Ripper
Rip Ark: Survival Evolved classes from UASSET to JSON for use in other implementations. This program converts the following...

* Dinos in all DLCs
* Items in all DLCs

# Pre-Computed Data Download
If you just want the Ark data, I have it available in JSON format [in another repo here](https://github.com/Roman-Port/Ark-Asset-JSON/tree/master).

## Known Issues
* While not an issue impacting usage, the code for this is very messy from me tweaking and testing to get this working. It could really use some cleanup.
* There is not a good way to find dinos from what I've seen, so this program manually searches for them in the ArkDevKit. This should be improved in the future.

## Dependencies
* [UModel](http://www.gildor.org/en/projects/umodel) for opening up Ark textures. You could disable this if you would like. 
* Possibly the ArkDevKit. __This could work with the standard Ark installation, but I am unsure. It has only been tested to work on the ArkDevKit files.__

## Usage
To use the Ark Asset Ripper, download the zip via GitHub or git and open the project in Visual Studio (2017). In ``Program.cs``, you'll have to change the following seven values:
* ``ARK_GAME_DIR`` - Change this to the ``/Content/`` folder of your ArkDevKit.
* ``UMODEL_PATH`` - Point this to your UModel installation folder. You'll need to copy the ``microsoft_sucks.bat`` batch file into UModel folder and point this path to this file. This file must be used because of the way Microsoft encodes text when running external programs.
* ``TEMP_FOLDER`` - This is a temp folder that will be used alongside UModel for converting images.
* ``OUTPUT_PATH`` - This is the output path. Make sure it ends in a backwards slash.
* ``GAME_MODE_PATH`` - This is the path to the ``Test_Game_Mode.uasset`` file in the ArkDevKit content folder. This should be in ``Content/PrimalEarth/CoreBlueprints/Test_Game_Mode.uasset``.
* ``CORE_MEDIA_PRIMAL_GAME_DATA_PATH`` - This is the path to the ``COREMEDIA_PrimalGameData_BP.uasset`` file in the ArkDevKit content folder. This should be in ``Content/PrimalEarth/CoreBlueprints/COREMEDIA_PrimalGameData_BP.uasset``.
* ``BASE_PRIMAL_GAME_DATAA_PATH`` - This is the path to the ``BASE_PrimalGameData_BP.uasset`` file in the ArkDevKit content folder. This should be in ``Content/PrimalEarth/CoreBlueprints/BASE_PrimalGameData_BP.uasset``.

Next, run the program. It could take a while to compute.
