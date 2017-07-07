using KSP.Localization;
using System;
using System.Linq;
using UnityEngine;

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//
// ROVERSCIENCE PLUGIN WAS CREATED BY THESPEARE					  //
// FOR KERBAL SPACE PROGRAM - PLEASE SEE FORUM THREAD FOR DETAILS //
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//
namespace RoverScience
{


    public class RoverScience : PartModule
	{
        private const float HomeWorldScienceScalar = 0.01f;
        private const float SunScienceScalar = 0f;
        private const float NearMoonScienceScalar = 0.3f;
        private const float FarMoonScienceScalar = 0.2f;

        // Not necessarily updated per build. Mostly updated per major commits
        public readonly string RSVersion = "2.3.1";
		public static RoverScience Instance = null;
        public static CelestialBody HomeWorld;

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

        public double CurrentPredictionAccuracy
        {
            get
            {
				return GetUpgradeValue(RSUpgrade.predictionAccuracy, levelPredictionAccuracy);
            }
        }

		public double CurrentMaxDistance
        {
            get
            {
				return GetUpgradeValue(RSUpgrade.maxDistance, levelMaxDistance);
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
		Vessel Vessel {
			get {
				if (HighLogic.LoadedSceneIsFlight) {
					return FlightGlobals.ActiveVessel;
				} else {
                    Utilities.Log ("RoverScience.Vessel null! - not flight");
					return null;
				}
			}

		}

        public float scienceMaxRadiusBoost = 1;

        public double ScienceDecayPercentage
        {
            get
            {
                return Math.Round((1 - ScienceDecayScalar) * 100);
            }
        }

		public float ScienceDecayScalar {
			get {
				return GetScienceDecayScalar (amountOfTimesAnalyzed);
			}
		}

		public float BodyScienceScalar {
			get {
				return GetBodyScienceScalar (Vessel.mainBody);
			}
		}

		public float BodyScienceCap {
			get {
				return GetBodyScienceCap (Vessel.mainBody);
			}
		}

		[KSPEvent (guiActive = true, guiName = "#LOC_RoverScience_GUI_ToggleTerminal")] // Toggle Rover Terminal
        private void ShowGUI ()
		{
			roverScienceGUI.consoleGUI.Toggle ();
            DrawWaypoint.Instance.ToggleMarker();
        }

        [KSPAction ("#LOC_RoverScience_GUI_ActivateConsole", actionGroup = KSPActionGroup.None)] // Activate Console
		private void ShowGUIAction (KSPActionParam param)
		{
			if (IsPrimary)
				ShowGUI ();
		}

		void OnDestroy ()
		{
            Utilities.Log ("RoverScience OnDestroy()");
		}

        public void OnGUI()
        {
            roverScienceGUI.DrawGUI();
        }

        public override void OnLoad (ConfigNode vesselNode)
        {
            Utilities.Log("#X1 RoverScience OnLoad @" + DateTime.Now);
            Instance = this;

            if (rover == null)
            {
                Utilities.Log("rover was null, creating new rover class (OnLoad)");
                rover = new Rover();
            }

           // try
            //{
            if (DB != null)  DB.UpdateRoverScience();
            //}catch{
            //}

        }

        public override void OnSave(ConfigNode vesselNode)
        {
            Utilities.Log("RoverScience OnSave @" + DateTime.Now);
            // try
            // {
            if (DB != null) DB.UpdateDB();
            //} catch
            //{
            //}
        }





        public override void OnStart (PartModule.StartState state)
		{
			if (HighLogic.LoadedSceneIsFlight) {
				if (IsPrimary) {
                    Utilities.Log ("Initiated! Version: " + RSVersion);
	
					Instance = this;

                    Utilities.Log ("RS Instance set - " + Instance);

                    HomeWorld = FlightGlobals.Bodies.Where(cb => cb.isHomeWorld).First(); // TODO: move this somewhere more appropriate

                    container = part.Modules ["ModuleScienceContainer"] as ModuleScienceContainer;
					command = part.Modules ["ModuleCommand"] as ModuleCommand;

                    // HACK: Must be called here otherwise they won't run their constructors for some reason
                    if (rover == null)
                    {
                        Utilities.Log("rover was null, creating new rover class (OnStart)");
                        rover = new Rover();
                    }
					rover.scienceSpot = new ScienceSpot (Instance);
					rover.landingSpot = new LandingSpot (Instance);

                    //try
                    //{
                    if (DB != null) DB.UpdateRoverScience();
                    //}
                    //catch { }

                    rover.SetClosestAnomaly();


                } else {
                    Utilities.Log ("ONSTART - Not primary");
				}

                // HACK: instance null unexpected.
                if (Instance == null)
                {
                    Instance = this;
                    Utilities.Log("Instance was null; workaround fix by declaring Instance anyway");
                }
			}

		}

		public override void OnUpdate ()
		{
			if (IsPrimary) {

				if (roverScienceGUI.consoleGUI.isOpen) {
					// Calculate rover traveled distance
					if (rover.ValidStatus)
						rover.CalculateDistanceTraveled (TimeWarp.deltaTime);

					rover.landingSpot.SetSpot ();
                    if (rover.landingSpot.established)
                    {
                        rover.SetRoverLocation();
                    }

                    if ((!rover.scienceSpot.established) && (!rover.ScienceSpotReached) && (ScienceDecayPercentage < 100))
                    {
                        rover.scienceSpot.CheckAndSet();
                    }
				}
			}
			KeyboardShortcuts ();
		}
		// Much credit to a.g. as his source helped to figure out how to utilize the experiment and its data
		// https://github.com/angavrilov/ksp-surface-survey/blob/master/SurfaceSurvey.cs#L276
		public void AnalyzeScienceSample ()
		{
			if (rover.ScienceSpotReached) {

				ScienceExperiment sciExperiment = ResearchAndDevelopment.GetExperiment ("RoverScienceExperiment");
				ScienceSubject sciSubject = ResearchAndDevelopment.GetExperimentSubject(sciExperiment, ExperimentSituations.SrfLanded, Vessel.mainBody, "", "");

				// 20 science per data
				sciSubject.subjectValue = 20;
				sciSubject.scienceCap = BodyScienceCap;

				// Divide by 20 to convert to data form
				float sciData = (rover.scienceSpot.potentialScience) / sciSubject.subjectValue;

                Utilities.Log ("sciData (potential/20): " + sciData);


                // Apply multipliers

                if (rover.AnomalySpotReached)
                {
                    Utilities.Log("RS: added anomaly id to save!");
                    Utilities.Log("RS: analyzed science at anomaly");

                    if (!rover.anomaliesAnalyzed.Contains(rover.closestAnomaly.id))
                    {
                        rover.anomaliesAnalyzed.Add(rover.closestAnomaly.id);
                    }

                } else
                {
                    // if a normal spot, we shall apply factors
                    Utilities.Log("RS: analyzed science at science spot");
                    sciData = sciData * ScienceDecayScalar * BodyScienceScalar * scienceMaxRadiusBoost;
                }



                Utilities.Log("RS: rover.scienceSpot.potentialScience: " + rover.scienceSpot.potentialScience);
                Utilities.Log("RS: sciData (post scalar): " + sciData);
                Utilities.Log("RS: scienceDecayScalar: " + ScienceDecayScalar);
                Utilities.Log("RS: bodyScienceScalar: " + BodyScienceScalar);
               
				

				if (sciData > 0.1) {
					if (StoreScience (container, sciSubject, sciData)) {
						container.ReviewData ();
                        amountOfTimesAnalyzed++;
                        Utilities.Log ("Science retrieved! - " + sciData);
					} else {
                        Utilities.Log ("Failed to add science to container!");
					}
				} else {

                    ScreenMessages.PostScreenMessage ("#LOC_RoverScience_GUI_ScienceTooLow", 5, ScreenMessageStyle.UPPER_CENTER); // Science value was too low - deleting data!
				}

				rover.scienceSpot.Reset ();

			} else {
                Utilities.Log ("Tried to analyze while not at spot?");
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

		private float GetScienceDecayScalar(int numberOfTimes)
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



		private float GetBodyScienceScalar (CelestialBody currentBody)
		{
            
            if (currentBody.isHomeWorld)
                return HomeWorldScienceScalar;
            if (currentBody == FlightGlobals.Bodies[0])
                return SunScienceScalar;

            if (currentBody.HasParent(HomeWorld))
            {
                return NearMoonScienceScalar; // TODO: Distinguish Mun/Minmus (and generic case)
            }
			return 1;
		}
       
        private float GetBodyScienceCap (CelestialBody currentBody)
		{
			float scalar = 1;
			float scienceCap = 1500;

            if (currentBody.isHomeWorld)
                scalar = 0.09f;
            else if (currentBody == FlightGlobals.Bodies[0])
                scalar = 0f;
            else if (currentBody.HasParent(HomeWorld))
            {
                scalar = 0.3f;
            }
            else
				scalar = 1f;

			return (scalar * scienceCap);
		}
        
        public string GetUpgradeName(RSUpgrade upgrade)
        {
            switch (upgrade)
            {
                case (RSUpgrade.maxDistance):
                    return Localizer.GetStringByTag("#LOC_RoverScience_GUI_MaxScanDistance"); //  "Max Scan Distance";
                case (RSUpgrade.predictionAccuracy):
                    return Localizer.GetStringByTag("#LOC_RoverScience_GUI_PrecisionAccuracy"); // "Prediction Accuracy";
                case (RSUpgrade.analyzedDecay):
                    return Localizer.GetStringByTag("#LOC_RoverScience_GUI_DecayLimit"); // "Analyzed Decay Limit";
                default:
                    return Localizer.GetStringByTag("#LOC_RoverScience_GUI_ErrorUpgradeName"); // "Failed to resolve getUpgradeName";
            }

        }

        public float GetUpgradeCost(RSUpgrade upgrade, int level)
		{

            if (level == 0) level = 1;
            if (level > GetUpgradeMaxLevel(upgrade)) return -1;

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

        public string GetUpgradeValueString(RSUpgrade upgrade, int level)
        {
            // This will come with unit for display
            switch (upgrade)
            {
                case (RSUpgrade.maxDistance):
                    if (levelMaxDistance >= maximum_levelMaxDistance)
                    {
                        return Localizer.GetStringByTag("#LOC_RoverScience_GUI_Max"); // "MAX";
                    }
                    else
                    {
                        return Localizer.Format("#LOC_RoverScience_GUI_Metres", GetUpgradeValue(RSUpgrade.maxDistance, level)); // <<1>>m
                    }

                case (RSUpgrade.predictionAccuracy):
                    if (levelPredictionAccuracy >= maximum_predictionAccuracy)
                    {
                        return Localizer.GetStringByTag("#LOC_RoverScience_GUI_Max"); // "MAX";
                    }
                    else
                    {
                        return Localizer.Format("#LOC_RoverScience_GUI_Percentage", GetUpgradeValue(RSUpgrade.predictionAccuracy, level)); // <<1>>%
                    }

                case (RSUpgrade.analyzedDecay):
                    if (levelAnalyzedDecay >= maximum_levelAnalyzedDecay)
                    {
                        return Localizer.GetStringByTag("#LOC_RoverScience_GUI_Max"); // "MAX";
                    }
                    else
                    {
                        return Localizer.Format("#LOC_RoverScience_GUI_nvalue", GetUpgradeValue(RSUpgrade.analyzedDecay, level)); // <<1>>n
                    }

                default:
                    return "Unable to resolve getUpgradeValueString()";
            }
        }

        public double GetUpgradeValue(RSUpgrade upgrade, int level)
		{

			if (level == 0) level = 1;
            if (level > GetUpgradeMaxLevel(upgrade)) return -1;

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

        public int GetUpgradeLevel(RSUpgrade upgradeType)
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

        public void SetUpgradeLevel(RSUpgrade upgradeType, int newValue)
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

        public int GetUpgradeMaxLevel(RSUpgrade upgradeType)
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

        public void UpgradeTech(RSUpgrade upgradeType)
        {
			Utilities.Log ("upgradeTech called: " + upgradeType);
            int nextLevel = GetUpgradeLevel(upgradeType) + 1;
            int currentLevel = GetUpgradeLevel(upgradeType);
            int maxLevel = GetUpgradeMaxLevel(upgradeType);
            float nextCost = GetUpgradeCost(upgradeType, nextLevel);
            string upgradeName = GetUpgradeName(upgradeType);

            // MAX LEVEL REACHED
            if (currentLevel >= maxLevel)
            {
                ScreenMessages.PostScreenMessage(Localizer.GetStringByTag("#LOC_RoverScience_GUI_MaxUpgradeLevel"), 3, ScreenMessageStyle.UPPER_CENTER); // Max Level reached for this upgrade
				return;
            }
            
            // NOT ENOUGH SCIENCE
            if (nextCost > ResearchAndDevelopment.Instance.Science)
            {
                ScreenMessages.PostScreenMessage(Localizer.GetStringByTag("#LOC_RoverScience_GUI_NotEnoughScience"), 3, ScreenMessageStyle.UPPER_CENTER); // "Not enough science to upgrade"
                return;
            }

            // UPGRADE METHOD
			if (upgradeType == RSUpgrade.maxDistance) {
				levelMaxDistance++;
                Utilities.Log ("Upgraded levelMaxDistance. Now level: " + levelMaxDistance);
			} else if (upgradeType == RSUpgrade.predictionAccuracy) {
				levelPredictionAccuracy++;
                Utilities.Log ("Upgraded predictionAccuracy. Now level: " + levelPredictionAccuracy);
			} else if (upgradeType == RSUpgrade.analyzedDecay)
            {
                levelAnalyzedDecay++;
                Utilities.Log("Upgraded levelAnalyzedDecay. Now level: " + levelAnalyzedDecay);
            }
            
            ResearchAndDevelopment.Instance.CheatAddScience(-nextCost);

            ScreenMessages.PostScreenMessage(Localizer.Format("#LOC_RoverScience_GUI_UpgradeMessage", upgradeName), 3, ScreenMessageStyle.UPPER_CENTER); // <<1>> has been upgraded"
        }

        public void SetScienceMaxRadiusBoost(int maxRadius)
        {
            //maxRadius' maximum value would only ever reach 2km (2000 meters)
            //this method updates the factor used to increase the science depending
            //on how far a given science spot has been spawned
            if (maxRadius < 150)
                scienceMaxRadiusBoost = 1;

			scienceMaxRadiusBoost = ((1f / 2000f) * maxRadius) + 1f;
        }

        public void KeyboardShortcuts ()
		{

			if (HighLogic.LoadedSceneIsFlight) {
				// CONSOLE WINDOW
				if (Input.GetKey (KeyCode.LeftControl) && Input.GetKey (KeyCode.R) && Input.GetKeyUp (KeyCode.S)) {
					roverScienceGUI.consoleGUI.Toggle ();
                    DrawWaypoint.Instance.ToggleMarker();
				}

				// DEBUG WINDOW
				if (Input.GetKey (KeyCode.RightControl) && Input.GetKeyUp (KeyCode.Keypad5)) {
					roverScienceGUI.debugGUI.Toggle ();
				}
			}
		}
		// TAKEN FROM KERBAL ENGINEERING REDUX SOURCE by cybutek
		// http://creativecommons.org/licenses/by-nc-sa/3.0/deed.en_GB
		// This is to hopefully prevent multiple instances of this PartModule from running simultaneously
		public bool IsPrimary {
			get {
				if (this.Vessel != null) {
					foreach (Part part in this.Vessel.parts) {
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

