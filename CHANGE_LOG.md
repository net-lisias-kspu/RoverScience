# Rover Science :: Change Log

* 2016-1014: 2.2.0-A1 (theSpeare) for KSP 1.2.1 PRE-RELEASE
	+ RoverScience should be compatible for KSP1.2 now. Fixed all the issues with values not being saved/loaded, but it's all pretty shaky coding so (as I've said on the forums) I'm going to rewrite this mod from the ground up now. Should make it easier for others to understand the code and also (if anyone ever wanted to) take over in the future.
	+ THIS IS A PRE-RELEASE ALPHA. I've tested it and nothing seems to break the game (running without RoverScience, running with RoverScience part), but I need help confirming this before I push an official release out to SpaceDock and CKAN and such.
	+ Any help is greatly appreciated.
* 2016-1106: 2.2.0 (theSpeare) for KSP 1.2.1
	+ compatibility with KSP 1.2.1
* 2016-0925: 2.1.4 (theSpeare) for KSP 1.1.3
	+ New part models
	+ Model and texturing by KSPForum @akron. Wonderful work and wonderful guy. Thanks!
		- [Image](http://i.imgur.com/gZFbkjC.png)
			- MiniAVC Implementation
	+ Added MiniAVC support. Will compare with GitHub pushed version file.
			- Initial Minor Balancing Pass
			- Changed so that amount of times analyzed does not increase for generated science that is too low. There will be more balancing to come as discussion continues in the forum page.
* 2016-0919: 2.1.3 (theSpeare) for KSP 1.1.3
	+ 17 should now be fully fixed. Furthermore, saving/loading should be fine now.
	+ No more coding to be done until KSP1.2 is fully out, as I'm a bit tired of having to keep adapting between both versions. Will refactor as soon as I update for KSP1.2.
	+ Next update will only be for a new part model.
* 2016-0915: 2.1.2 (theSpeare) for KSP 1.1.3
	+ Made GUI prettier. Now with colours and revised text to improve
	+ understandability.
	+ Added new upgrade: "analyzedDecay". Upgrading this will increase the
	+ number of times a player may analyze before suffering science loss.
	+ Quickloading and quicksaving will now restore window positions. GUI
	+ position and display status is now saved in persistence & quicksave.
* 2016-0913: 2.1 (theSpeare) for KSP 1.1.3
	+ Interesting Rocks
	+ Rock models are now spawned at Science Spots. Two different models are included; more will be added later. Credit to udk_lethal_dose for the idea. Textures are a bit big; will look at compressing these further.
	+ Anomalies
	+ Anomalies are now recognized! Credit to etmoonshade for the idea.
	+ ScienceSpots will generate at anomalies once rovers are within 100m and no other scienceSpots are active.
	+ Anomalies provide a flat 300 science points (500, if you're lucky). However you can only analyze an anomaly ONCE.
	+ Most anomalies have been charted, except for two Duna ones that do not
	+ appear above ground anymore.
	+ Go send your rovers out to those anomalies!
* 2016-0910: 2.0.4 (theSpeare) for KSP 1.1.3
	+ fixed: fixed previous code that had an "if Instance=null". It is now
		- Instance == null" because I am an idiot. This was a mod-breaking issue,
	+ so hopefully GUI should be working now!
			- learned how to raycast to get surface altitude, reverted back to
			- sphere marker as it waaaaaay good
* 2016-0908: 2.0.3 (theSpeare) for KSP 1.1.3 PRE-RELEASE
	+ An attempted fix as detailed on issue #1.
	+ (from release 2.0.2) major feature:
	+ Been wanting to do this for awhile!
	+ Science spot location is now rendered by a capsule marker. The marker will reduce in size and alpha as the player vessel nears. I will be able to remove the old crappy compass mechanic. See gfy below for example.
	+ https://gfycat.com/EverlastingParchedAmericancicada
* 2016-0908: 2.0.2 (theSpeare) for KSP 1.1.3 PRE-RELEASE
	+ Been wanting to do this for awhile!
	+ Science spot location is now rendered by a capsule marker. The marker will reduce in size and alpha as the player vessel nears. I will be able to remove the old crappy compass mechanic. See gfy below for example.
	+ https://gfycat.com/EverlastingParchedAmericancicada
* 2016-0907: 2.0.1 (theSpeare) for KSP 1.1.3 PRE-RELEASE
	+ now checks for modules containing "wheel", rather than matching exact
	+ WheelModule names
	+ migrated saving method to use KSPScenario rather than an independent
	+ save file. Should work between quicksaves/loads now
* 2016-0905: 2.0.0 (theSpeare) for KSP 1.1.3 PRE-RELEASE
	+ Compatibility patching for 1.0+ Kerbal Space Program. No new features or major changes.
