# Rover Science :: Change Log

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
