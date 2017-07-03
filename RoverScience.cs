using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//
// ROVERSCIENCE PLUGIN WAS CREATED BY THESPEARE					  //
// FOR KERBAL SPACE PROGRAM - PLEASE SEE FORUM THREAD FOR DETAILS //
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//
namespace RoverScience
{
	#pragma warning disable 0108

	public class RoverScience : PartModule
	{
		// Not necessarily updated per build. Mostly updated per major commits
		public readonly string RSVersion = "2.1.4";
		public static RoverScience Instance = null;
		public System.Random rand = new System.Random ();
		public ModuleScienceContainer container;
		public ModuleCommand command;
		public Rover rover;

		public int levelMaxDistance = 1;
		public int levelPredictionAccuracy = 1;
        public int levelAnalyzedDecay = 2;
        public readonly int maximum_levelMaxDistance = 5;
        public readonly int maximum_predictionAccuracy = 5;
        public readonly int maximum_levelAnalyzedDecay = 5;

        public double currentPredictionAccuracy
        {
            get
            {
				return getUpgradeValue(RSUpgrade.predictionAccuracy, levelPredictionAccuracy);
            }
        }

		public double currentMaxDistance
        {
            get
            {
				return getUpgradeValue(RSUpgrade.maxDistance, levelMaxDistance);
            }
        }

        private RoverScienceDB DB
        {
            get { return RoverScienceDB.Instance; }
        }


        public RoverScienceGUI roverScienceGUI = new RoverScienceGUI();
		public double distCounter;
		[KSPField (isPersistant = true)]
		public int amountOfTimesAnalyzed = 0;
		// Leave this alone. PartModule has its own vessel class which SHOULD do the job but
		// for some reason removing this seemed to destroy a lot of function
		Vessel vessel {
			get {
				if (HighLogic.LoadedSceneIsFlight) {
					return FlightGlobals.ActiveVessel;
				} else {
					Debug.Log ("Vessel vessel returned null!");
					return null;
				}
			}

		}

        public float scienceMaxRadiusBoost = 1;

        public double scienceDecayPercentage
        {
            get
            {
                return Math.Round((1 - scienceDecayScalar) * 100);
            }
        }

		public float scienceDecayScalar {
			get {
				return getScienceDecayScalar (amountOfTimesAnalyzed);
			}
		}

		public float bodyScienceScalar {
			get {
				return getBodyScienceScalar (vessel.mainBody.bodyName);
			}
		}

		public float bodyScienceCap {
			get {
				return getBodyScienceCap (vessel.mainBody.bodyName);
			}
		}

		[KSPEvent (guiActive = true, guiName = "Toggle Rover Terminal")]
		private void showGUI ()
		{
			roverScienceGUI.consoleGUI.toggle ();
            DrawWaypoint.Instance.toggleMarker();
        }

        [KSPAction ("Activate Console", actionGroup = KSPActionGroup.None)]
		private void showGUIAction (KSPActionParam param)
		{
			if (IsPrimary)
				showGUI ();
		}

		void OnDestroy ()
		{
			Debug.Log ("RoverScience OnDestroy()");
		}

        public void OnGUI()
        {
            roverScienceGUI.drawGUI();
        }

        public override void OnLoad (ConfigNode vesselNode)
        {
            Debug.Log("#X1 RoverScience OnLoad @" + DateTime.Now);
            Instance = this;

            if (rover == null)
            {
                Debug.Log("rover was null, creating new rover class (OnLoad)");
                rover = new Rover();
            }

           // try
            //{
            if (DB != null)  DB.updateRoverScience();
            //}catch{
            //}

        }

        public override void OnSave(ConfigNode vesselNode)
        {
            Debug.Log("RoverScience OnSave @" + DateTime.Now);
            // try
            // {
            if (DB != null) DB.updateDB();
            //} catch
            //{
            //}
        }





        public override void OnStart (PartModule.StartState state)
		{
			if (HighLogic.LoadedSceneIsFlight) {
				if (IsPrimary) {
					Debug.Log ("RoverScience 2 initiated!");
					Debug.Log ("RoverScience version: " + RSVersion);
	
					Instance = this;
                    
					Debug.Log ("RS Instance set - " + Instance);
	
					container = part.Modules ["ModuleScienceContainer"] as ModuleScienceContainer;
					command = part.Modules ["ModuleCommand"] as ModuleCommand;

                    // Must be called here otherwise they won't run their constructors for some reason
                    if (rover == null)
                    {
                        Debug.Log("rover was null, creating new rover class (OnStart)");
                        rover = new Rover();
                    }
					rover.scienceSpot = new ScienceSpot (Instance);
					rover.landingSpot = new LandingSpot (Instance);

                    //try
                    //{
                    if (DB != null) DB.updateRoverScience();
                    //}
                    //catch { }

                    rover.setClosestAnomaly(vessel.mainBody.bodyName);


                } else {
					Debug.Log ("ONSTART - Not primary");
				}

                if (Instance == null)
                {
                    Instance = this;
                    Debug.Log("Instance was null; workaround fix by declaring Instance anyway");
                }
			}

		}

		public override void OnUpdate ()
		{
			if (IsPrimary) {

				if (roverScienceGUI.consoleGUI.isOpen) {
					// Calculate rover traveled distance
					if (rover.validStatus)
						rover.calculateDistanceTraveled (TimeWarp.deltaTime);

					rover.landingSpot.setSpot ();
                    if (rover.landingSpot.established)
                    {
                        rover.setRoverLocation();
                    }

                    if ((!rover.scienceSpot.established) && (!rover.scienceSpotReached) && (scienceDecayPercentage < 100))
                    {
                        rover.scienceSpot.checkAndSet();
                    }
				}
			}
			keyboardShortcuts ();
		}
		// Much credit to a.g. as his source helped to figure out how to utilize the experiment and its data
		// https://github.com/angavrilov/ksp-surface-survey/blob/master/SurfaceSurvey.cs#L276
		public void analyzeScienceSample ()
		{
			if (rover.scienceSpotReached) {

				ScienceExperiment sciExperiment = ResearchAndDevelopment.GetExperiment ("RoverScienceExperiment");
				ScienceSubject sciSubject = ResearchAndDevelopment.GetExperimentSubject(sciExperiment, ExperimentSituations.SrfLanded, vessel.mainBody, "", "");

				// 20 science per data
				sciSubject.subjectValue = 20;
				sciSubject.scienceCap = bodyScienceCap;

				// Divide by 20 to convert to data form
				float sciData = (rover.scienceSpot.potentialScience) / sciSubject.subjectValue;

				Debug.Log ("sciData (potential/20): " + sciData);


                // Apply multipliers

                if (rover.anomalySpotReached)
                {
                    Debug.Log("RS: added anomaly id to save!");
                    Debug.Log("RS: analyzed science at anomaly");

                    if (!rover.anomaliesAnalyzed.Contains(rover.closestAnomaly.id))
                    {
                        rover.anomaliesAnalyzed.Add(rover.closestAnomaly.id);
                    }

                } else
                {
                    // if a normal spot, we shall apply factors
                    Debug.Log("RS: analyzed science at science spot");
                    sciData = sciData * scienceDecayScalar * bodyScienceScalar * scienceMaxRadiusBoost;
                }
                


                Debug.Log("RS: rover.scienceSpot.potentialScience: " + rover.scienceSpot.potentialScience);
                Debug.Log("RS: sciData (post scalar): " + sciData);
                Debug.Log("RS: scienceDecayScalar: " + scienceDecayScalar);
                Debug.Log("RS: bodyScienceScalar: " + bodyScienceScalar);
               
				

				if (sciData > 0.1) {
					if (StoreScience (container, sciSubject, sciData)) {
						container.ReviewData ();
                        amountOfTimesAnalyzed++;
                        Debug.Log ("Science retrieved! - " + sciData);
					} else {
						Debug.Log ("Failed to add science to container!");
					}
				} else {

                    ScreenMessages.PostScreenMessage ("Science value was too low - deleting data!", 5, ScreenMessageStyle.UPPER_CENTER);
				}

				rover.scienceSpot.reset ();

			} else {
				Debug.Log ("Tried to analyze while not at spot?");
			}
		}

		public bool StoreScience (ModuleScienceContainer container, ScienceSubject subject, float data)
		{

			if (container.capacity > 0 && container.GetScienceCount () >= container.capacity)
				return false;
		
			if (container.GetStoredDataCount () != 0)
				return false;
				
			float xmitValue = 0.85f;
			float labBoost = 0.1f;

			ScienceData new_data = new ScienceData (data, xmitValue, labBoost, subject.id, subject.title);

			if (container.AddData (new_data))
				return true;
			

			return false;
		}

		private float getScienceDecayScalar(int numberOfTimes)
		{
            // This is the equation that models the decay of science per analysis made
            // y = 1.20^(-0.9*(x-2))
            // Always subject to adjustment
            //double scalar = (1.20 * Math.Exp (-0.9 * (numberOfTimes - levelAnalyzedDecay)));

            double scalar = (1.20 * Math.Exp(-0.4 * (numberOfTimes - levelAnalyzedDecay)));
            // decay pattern as such:  0.8, 0.54, 0.36, 0.24, 0.16, 0.1

            if (scalar > 1)
            {
                return 1;
            }

			return (float)scalar;
		}



		private float getBodyScienceScalar (string currentBodyName)
		{
			switch (currentBodyName) {
			case "Kerbin":
				return 0.01f;
			case "Sun":
				return 0;
			case "Mun":
				return 0.3f;
			case "Minmus":
				return 0.2f;
			default:
				return 1;
			}
		}
       
        private float getBodyScienceCap (string currentBodyName)
		{
			float scalar = 1;
			float scienceCap = 1500;

			switch (currentBodyName) {
			case "Kerbin":
				scalar = 0.09f;
				break;
			case "Sun":
				scalar = 0f;
				break;
			case "Mun":
				scalar = 0.3f;
				break;
			case "Minmus":
				scalar = 0.2f;
				break;
			default:
				scalar = 1f;
				break;
			}

			return (scalar * scienceCap);
		}
        
        public string getUpgradeName(RSUpgrade upgrade)
        {
            switch (upgrade)
            {
                case (RSUpgrade.maxDistance):
                    return "Max Scan Distance";
                case (RSUpgrade.predictionAccuracy):
                    return "Prediction Accuracy";
                case (RSUpgrade.analyzedDecay):
                    return "Analyzed Decay Limit";
                default:
                    return "Failed to resolve getUpgradeName";
            }

        }

        public float getUpgradeCost(RSUpgrade upgrade, int level)
		{

            if (level == 0) level = 1;
            if (level > getUpgradeMaxLevel(upgrade)) return -1;

			switch (upgrade)
			{
			case (RSUpgrade.maxDistance):
				if (level == 1) return 200;
				if (level == 2) return 250;
				if (level == 3) return 400;
				if (level == 4) return 550;
				if (level == 5) return 1000;

				return -1;
			case (RSUpgrade.predictionAccuracy):

				if (level == 1) return 200;
				if (level == 2) return 400;
				if (level == 3) return 500;
				if (level == 4) return 1000;
				if (level == 5) return 2100;

				return -1;
            case (RSUpgrade.analyzedDecay):

                if (level == 1) return 0;
                if (level == 2) return 0;
                if (level == 3) return 1000;
                if (level == 4) return 1000;
                if (level == 5) return 1000;

                return -1;
            default:
			return -1;
			}
		}

        public string getUpgradeValueString(RSUpgrade upgrade, int level)
        {
            // This will come with unit for display
            switch (upgrade)
            {
                case (RSUpgrade.maxDistance):
                    if (levelMaxDistance >= maximum_levelMaxDistance)
                    {
                        return "MAX";
                    }
                    else
                    {
                        return (getUpgradeValue(RSUpgrade.maxDistance, level) + "m");
                    }

                case (RSUpgrade.predictionAccuracy):
                    if (levelPredictionAccuracy >= maximum_predictionAccuracy)
                    {
                        return "MAX";
                    }
                    else
                    {
                        return (getUpgradeValue(RSUpgrade.predictionAccuracy, level) + "%");
                    }

                case (RSUpgrade.analyzedDecay):
                    if (levelAnalyzedDecay >= maximum_levelAnalyzedDecay)
                    {
                        return "MAX";
                    }
                    else
                    {
                        return (getUpgradeValue(RSUpgrade.analyzedDecay, level) + "n");
                    }

                default:
                    return "Unable to resolve getUpgradeValueString()";
            }
        }

        public double getUpgradeValue(RSUpgrade upgrade, int level)
		{

			if (level == 0) level = 1;
            if (level > getUpgradeMaxLevel(upgrade)) return -1;

			switch (upgrade)
			{
			case (RSUpgrade.maxDistance):
				if (level == 1) return 100;
				if (level == 2) return 500;
				if (level == 3) return 1000;
				if (level == 4) return 2000;
				if (level == 5) return 4000;

				return -1;

			case (RSUpgrade.predictionAccuracy):
				if (level == 1) return 10;
                if (level == 2) return 20;
				if (level == 3) return 50;
				if (level == 4) return 70;
				if (level == 5) return 80;

				return -1;

            case (RSUpgrade.analyzedDecay):
                if (level <= 2) return 2;
                if (level == 3) return 3;
                if (level == 4) return 4;
                if (level == 5) return 5;

                return -1;

            default:
			return -1;
			}
		}

        public int getUpgradeLevel(RSUpgrade upgradeType)
        {
            switch (upgradeType)
            {
                case (RSUpgrade.maxDistance):
                    return levelMaxDistance;
                case (RSUpgrade.predictionAccuracy):
                    return levelPredictionAccuracy;
                case (RSUpgrade.analyzedDecay):
                    return levelAnalyzedDecay;
                default:
                    return -1;
            }
        }

        public void setUpgradeLevel(RSUpgrade upgradeType, int newValue)
        {
                if (upgradeType == RSUpgrade.maxDistance) {
                    levelMaxDistance = newValue;
                } else
                if (upgradeType == RSUpgrade.predictionAccuracy) {
                    levelPredictionAccuracy = newValue;
                } else
                if (upgradeType == RSUpgrade.analyzedDecay)
                {
                    levelAnalyzedDecay = newValue;
                }
        }

        public int getUpgradeMaxLevel(RSUpgrade upgradeType)
        {
            switch (upgradeType)
            {
                case (RSUpgrade.maxDistance):
                    return maximum_levelMaxDistance;
                case (RSUpgrade.predictionAccuracy):
                    return maximum_predictionAccuracy;
                case (RSUpgrade.analyzedDecay):
                    return maximum_levelAnalyzedDecay;
                default:
                    return -1;
            }
        }

        public void upgradeTech(RSUpgrade upgradeType)
        {
			Debug.Log ("upgradeTech called: " + upgradeType);
            int nextLevel = getUpgradeLevel(upgradeType) + 1;
            int currentLevel = getUpgradeLevel(upgradeType);
            int maxLevel = getUpgradeMaxLevel(upgradeType);
            float nextCost = getUpgradeCost(upgradeType, nextLevel);
            string upgradeName = getUpgradeName(upgradeType);

            // MAX LEVEL REACHED
            if (currentLevel >= maxLevel)
            {
                ScreenMessages.PostScreenMessage("Max Level reached for this upgrade",
                    3, ScreenMessageStyle.UPPER_CENTER);
				return;
            }
            
            // NOT ENOUGH SCIENCE
            if (nextCost > ResearchAndDevelopment.Instance.Science)
            {
                ScreenMessages.PostScreenMessage("Not enough science to upgrade", 
                    3, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            // UPGRADE METHOD
			if (upgradeType == RSUpgrade.maxDistance) {
				levelMaxDistance++;
				Debug.Log ("Upgraded levelMaxDistance. Now level: " + levelMaxDistance);
			} else if (upgradeType == RSUpgrade.predictionAccuracy) {
				levelPredictionAccuracy++;
				Debug.Log ("Upgraded predictionAccuracy. Now level: " + levelPredictionAccuracy);
			} else if (upgradeType == RSUpgrade.analyzedDecay)
            {
                levelAnalyzedDecay++;
                Debug.Log("Upgraded levelAnalyzedDecay. Now level: " + levelAnalyzedDecay);
            }
            
            ResearchAndDevelopment.Instance.CheatAddScience(-nextCost);

            ScreenMessages.PostScreenMessage(("" + upgradeName + " has been upgraded"),
                    3, ScreenMessageStyle.UPPER_CENTER);
        }

        public void setScienceMaxRadiusBoost(int maxRadius)
        {
            //maxRadius' maximum value would only ever reach 2km (2000 meters)
            //this method updates the factor used to increase the science depending
            //on how far a given science spot has been spawned
            if (maxRadius < 150)
                scienceMaxRadiusBoost = 1;

			scienceMaxRadiusBoost = ((1f / 2000f) * maxRadius) + 1f;
        }

        public void keyboardShortcuts ()
		{

			if (HighLogic.LoadedSceneIsFlight) {
				// CONSOLE WINDOW
				if (Input.GetKey (KeyCode.LeftControl) && Input.GetKey (KeyCode.R) && Input.GetKeyUp (KeyCode.S)) {
					roverScienceGUI.consoleGUI.toggle ();
                    DrawWaypoint.Instance.toggleMarker();
				}

				// DEBUG WINDOW
				if (Input.GetKey (KeyCode.RightControl) && Input.GetKeyUp (KeyCode.Keypad5)) {
					roverScienceGUI.debugGUI.toggle ();
				}
			}
		}
		// TAKEN FROM KERBAL ENGINEERING REDUX SOURCE by cybutek
		// http://creativecommons.org/licenses/by-nc-sa/3.0/deed.en_GB
		// This is to hopefully prevent multiple instances of this PartModule from running simultaneously
		public bool IsPrimary {
			get {
				if (this.vessel != null) {
					foreach (Part part in this.vessel.parts) {
						if (part.Modules.Contains (this.ClassID)) {
							if (this.part == part) {
								return true;
							} else {
								break;
							}
						}
					}
				}
				return false;
			}
		}
	}
}

