# SongDTAInjector

A C# command line application to inject a DTA script into a decompressed Milo scene. This tool is intended for authors making custom content for Rock Band 3 to take their songs to the next level and do things that were previously completely impossible without modifying the game, all on a retail console.

While this tool is for Rock Band 3, it *may* work on other Rock Band titles. Be aware that if you try to use it for Rock Band 2 or something similar, you may have mixed results or it might just flat out crash the game.

## Usage

Using the tool is quite simple. First, get a DTA script that has one or more auto-executing blocks of code using the `#autorun` tag. It is very important that you have the `#autorun` tag included, or the script will *not* execute.

`#autorun {do {my cool script here}}`

An example script that disables all post processing effects and forces a video venue can be seen below.

`#autorun {do {{rnd toggle_all_postprocs} {meta_performer set_venue video_01}}}`

Next, save this file as script.dta or something similar. Get your decompressed .milo_ps3/xbox/wii scene and the DTA script and put them in the same folder as SongDTAInjector.

Now, open a command line and run a command similar to the following:

`SongDTAInjector script.dta song.milo_xbox song_out.milo_xbox`

This will inject the script into your milo scene. If it did not work, it is likely because your DTA is malformed, you forgot the `#autorun` tag, or your milo scene was compressed (in which case it will tell you and refuse to go any further). Put the newly created scene back into your CON/LIVE file and your script will execute the moment the loading screen "vignette" starts.



## Credits

* Maxton - For creating DtxCS

* InvoxiPlayGames - For extending DtxCS to include DTB serialization

* PikminGuts92 - For documentation of the structure of milo scenes


