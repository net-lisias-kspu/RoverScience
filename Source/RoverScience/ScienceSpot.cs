using System;
using KSP.Localization;

namespace RoverScience
{
    public class ScienceSpot
	{

		System.Random rand = new System.Random();
		public Coords location = new Coords ();

		public int potentialScience;
        public int randomRadius = 0;
        
        // The spot's actual potential name
		public string potentialGenerated = "";

        // This is what will be shown as the prediction
        public string predictedSpot = "";
        private string[] potentialStrings = new string[] {
            // "Very High!", "High", "Normal", "Low", "Very Low!"
            Localizer.GetStringByTag("#LOC_RoverScience_GUI_Potential1"),
            Localizer.GetStringByTag("#LOC_RoverScience_GUI_Potential2"),
            Localizer.GetStringByTag("#LOC_RoverScience_GUI_Potential3"),
            Localizer.GetStringByTag("#LOC_RoverScience_GUI_Potential4"),
            Localizer.GetStringByTag("#LOC_RoverScience_GUI_Potential5")
        };



        public bool established = false;
		RoverScience roverScience = null;

        public int minDistance = 5;
        public int minDistanceDefault = 5;

		public ScienceSpot (RoverScience roverScience)
		{
				this.roverScience = roverScience;
		}

		public Rover Rover {
			get {
				return roverScience.rover;
			}
		}

		public RoverScienceGUI RoverScienceGUI
		{
			get
			{
				return roverScience.roverScienceGUI;
			}
		}

        Vessel Vessel
        {
            get
            {
                if (HighLogic.LoadedSceneIsFlight)
                {
                    return FlightGlobals.ActiveVessel;
                }
                else
                {
                    Utilities.LogVerbose("ScienceSpot.Vessel null - not flight!");
                    return null;
                }
            }

        }

        public enum Potentials
        {
            vhigh, high, normal, low, vlow
        }

        public string GetPotentialString(Potentials potential)
        {
            switch (potential)
            {
                case (Potentials.vhigh):
                    return potentialStrings[0];
                case (Potentials.high):
                    return potentialStrings[1];
                case (Potentials.normal):
                    return potentialStrings[2];
                case (Potentials.low):
                    return potentialStrings[3];
                case (Potentials.vlow):
                    return potentialStrings[4];
            }
            return "Potential string unresolved";
        }

		public void GenerateScience(bool anomaly = false)
		{
            Utilities.LogVerbose ("generateScience()");

            // anomaly flag will set a high science value
            if (anomaly)
            {
                potentialGenerated = "anomaly"; // (doesn't really matter what we write here, as it's superceded by predictScience()

                if (rand.Next(0, 100) < 1)
                {
                    potentialScience = 500;
                    return;
                } else
                {
                    potentialScience = 300;
                    return;
                }
            }

			if (rand.Next (0, 100) < 1) {
                potentialGenerated = GetPotentialString (Potentials.vhigh);
				potentialScience = rand.Next (400, 500);
				return;
			} 

			if (rand.Next (0, 100) < 8) {
                potentialGenerated = GetPotentialString(Potentials.high);
				potentialScience = rand.Next (200, 400);
				return;
			} 

			if (rand.Next (0, 100) < 65) {
                potentialGenerated = GetPotentialString(Potentials.normal);
				potentialScience = rand.Next (70, 200);
				return;
			} 

			if (rand.Next (0, 100) < 70) {
                potentialGenerated = GetPotentialString(Potentials.low);
				potentialScience = rand.Next (30, 70);
				return;
			}

            potentialGenerated = GetPotentialString(Potentials.vlow);
			potentialScience = rand.Next (0, 30);
            

		}

        // This handles what happens after the distance traveled passes the distance roll
        // If the roll is successful establish a science spot
		public void CheckAndSet()
        {
            // Once distance traveled passes the random check distance
            if ((Rover.DistanceToClosestAnomaly <= 100) && (!Anomalies.Instance.HasCurrentAnomalyBeenAnalyzed()))
            {
                SetLocation(Rover.closestAnomaly.location.longitude, Rover.closestAnomaly.location.latitude, anomaly: true);
                Rover.ResetDistanceTraveled();


            }
            else if (Rover.distanceTraveled >= Rover.distanceCheck)
            {
				
                Rover.ResetDistanceTraveled();

                RoverScienceGUI.AddRandomConsoleJunk();

                // Reroll distanceCheck value
                Rover.distanceCheck = rand.Next(20, 50);
					
                // farther you are from established site the higher the chance of striking science!

				int rNum = rand.Next(0, 100);
				double dist = Rover.DistanceFromLandingSpot;

                // chanceAlgorithm will be modelled on y = 7 * sqrt(x) with y as chance and x as distance from landingspot

                double chanceAlgorithm = (7 * Math.Sqrt(dist));


				double chance = (chanceAlgorithm < 75) ? chanceAlgorithm : 75;

                Utilities.LogVerbose ("rNum: " + rNum);
                Utilities.LogVerbose ("chance: " + chance);
                Utilities.LogVerbose ("rNum <= chance: " + ((double)rNum <= chance));
					
                // rNum is a random number between 0 and 100
                // chance is the percentage number we check for to determine a successful roll
                // higher chance == higher success roll chance
                if ((double)rNum <= chance)
                {
						
                    SetLocation(random: true);
                    Utilities.LogVerbose ("setLocation");

                    RoverScienceGUI.ClearConsole();

                    Utilities.LogVerbose("Distance from spot is: " + Rover.DistanceFromScienceSpot);
                    Utilities.LogVerbose("Bearing is: " + Rover.BearingToScienceSpot);
                    Utilities.LogVerbose("Something found");
							
                }
                else
                {
                    // Science hotspot not found
                    Utilities.LogVerbose("Science hotspot not found!");
                }


            }

        }

        public Coords GenerateRandomLocation(int minRadius, int maxRadius)
        {
            Coords randomSpot = new Coords();

            randomRadius = rand.Next(minRadius, maxRadius);
            roverScience.SetScienceMaxRadiusBoost(randomRadius);


            double bodyRadius = Vessel.mainBody.Radius;
            double randomAngle = rand.NextDouble() * (double)(1.9);
            double randomTheta = (randomAngle * (Math.PI));
            double angularDistance = randomRadius / bodyRadius;
            double currentLatitude = Vessel.latitude.ToRadians();
            double currentLongitude = Vessel.longitude.ToRadians();

            double spotLat = Math.Asin(Math.Sin(currentLatitude) * Math.Cos(angularDistance) +
                Math.Cos(currentLatitude) * Math.Sin(angularDistance) * Math.Cos(randomTheta));

            double spotLon = currentLongitude + Math.Atan2(Math.Sin(randomTheta) * Math.Sin(angularDistance) * Math.Cos(currentLatitude),
                Math.Cos(angularDistance) - Math.Sin(currentLatitude) * Math.Sin(spotLat));

            randomSpot.latitude = spotLat.ToDegrees();
            randomSpot.longitude = spotLon.ToDegrees();
            
            return randomSpot;
        }

        // Method to set location
        public void SetLocation(double longitude = 0, double latitude = 0, bool random = false, bool anomaly = false)
        {
            // generate random radius for where to spot placement
            // bool random will override whatever is entered

            if (!random)
            {
                location.latitude = latitude;
                location.longitude = longitude;
            } else
            {
                Coords randomSpot = GenerateRandomLocation(Rover.minRadius, Rover.maxRadius);
                location.latitude = randomSpot.latitude;
                location.longitude = randomSpot.longitude;
            }

            established = true;

			this.GenerateScience(anomaly);
            PredictSpot(anomaly);

            Rover.distanceTraveledTotal = 0;

            //Utilities.Log("== setLocation() ==");
            //Utilities.Log("randomAngle: " + Math.Round(randomAngle, 4));
            //Utilities.Log("randomTheta (radians): " + Math.Round(randomTheta, 4));
            //Utilities.Log("randomTheta (degrees?): " + Math.Round((randomTheta.ToDegrees()), 4));
            //Utilities.Log(" ");
            //Utilities.Log("randomRadius selected: " + randomRadius);
            //Utilities.Log("distance to ScienceSpot: " + rover.distanceFromScienceSpot);
              
            //Utilities.Log("lat/long: " + location.latitude + " " + location.longitude);
            //Utilities.Log("==================");

            DrawWaypoint.Instance.SetMarkerLocation(location.longitude, location.latitude, spawningObject: !anomaly);
            DrawWaypoint.Instance.ShowMarker();
        }



        public void PredictSpot(bool anomaly = false)
        {
            double predictionAccuracyChance = roverScience.CurrentPredictionAccuracy;

            int rNum = rand.Next(0, 100);

            if (anomaly)
            {
                predictedSpot = Localizer.GetStringByTag("#LOC_RoverScience_GUI_Anomaly3"); // "Anomaly detected! There is something very interesting nearby...";
            } else if (rNum < predictionAccuracyChance)
            {
                predictedSpot = potentialGenerated; // (full confidence)
            }
            else
            {
                // Select a random name if the roll was not successful
                // This is to possibly mislead the player
                predictedSpot = potentialStrings[rand.Next(0, potentialStrings.Length)];
            }


            Utilities.LogVerbose("Spot prediction attempted!");
        }

		public void Reset()
		{
			established = false;
			potentialScience = 0;
			location.longitude = 0;
			location.latitude = 0;

			Rover.ResetDistanceTraveled ();
			Rover.distanceTraveledTotal = 0;

            DrawWaypoint.Instance.HideMarker();
            DrawWaypoint.Instance.DestroyInterestingObject();
            
        }
	}

	

}


