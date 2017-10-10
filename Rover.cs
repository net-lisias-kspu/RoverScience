using System;
using System.Collections.Generic;

namespace RoverScience
{

    public class Rover
	{

		public System.Random rand = new System.Random();

		public ScienceSpot scienceSpot;
		public LandingSpot landingSpot;

		public Coords location = new Coords ();
		public double distanceTraveled = 0;
		public double distanceCheck = 20;
		public double distanceTraveledTotal = 0;

		public int minRadius = 40;
		public int maxRadius = 100;

        public List<string> anomaliesAnalyzed = new List<string>();
        public Anomalies.Anomaly closestAnomaly = new Anomalies.Anomaly();

		public double DistanceFromLandingSpot
    		=> GeomLib.GetDistanceBetweenTwoPoints (Vessel.mainBody, location, landingSpot.location);

		public double DistanceFromScienceSpot
	    	=> GeomLib.GetDistanceBetweenTwoPoints(Vessel.mainBody, location, scienceSpot.location);

		public double BearingToScienceSpot
            => GeomLib.GetBearingFromCoords(scienceSpot.location, location);

		Vessel Vessel => FlightGlobals.ActiveVessel;

		public double Heading => GeomLib.GetRoverHeading (Vessel);

        public bool ScienceSpotReached
            => (scienceSpot.established && DistanceFromScienceSpot <= scienceSpot.minDistance);

        public bool AnomalySpotReached
            => (scienceSpot.established && DistanceToClosestAnomaly <= scienceSpot.minDistance);

        public bool AnomalyPresent
            => ((DistanceToClosestAnomaly <= 100) && !Anomalies.Instance.HasCurrentAnomalyBeenAnalyzed());

        public int NumberWheelsLanded => GetWheelsLanded();

        public int NumberWheels => GetWheelCount();

        public bool ValidStatus => CheckRoverValidStatus();

        public double DistanceToClosestAnomaly
        {
            get
            {
                if (location == null)
                    Utilities.LogVerbose("location == null in DistanceToClosestAnomaly");
                if (closestAnomaly == null)
                    Utilities.LogVerbose("closestAnomaly == null in DistanceToClosestAnomaly");
                if (Vessel == null)
                    Utilities.LogVerbose("Vessel == null in DistanceToClosestAnomaly");

                return GeomLib.GetDistanceBetweenTwoPoints(Vessel.mainBody, location, closestAnomaly.location);
            }
        }

        public void CalculateDistanceTraveled(double deltaTime)
		{
			distanceTraveled += (RoverScience.Instance.vessel.srfSpeed) * deltaTime;
            if (!scienceSpot.established) distanceTraveledTotal += (RoverScience.Instance.vessel.srfSpeed) * deltaTime;
		}

        public void SetRoverLocation()
        {
            location.latitude = Vessel.latitude;
            location.longitude = Vessel.longitude;
        }

		public void ResetDistanceTraveled()
		{
			distanceTraveled = 0;
		}

        private bool CheckRoverValidStatus()
        {   
            // Checks if rover is landed with at least one wheel on with no time-warp.
            return ((TimeWarp.CurrentRate == 1) && (Vessel.horizontalSrfSpeed > (double)0.01) && (NumberWheelsLanded > 0));
        }

        private int GetWheelCount()
		{
			int wheelCount = 0;

			List<Part> vesselParts = FlightGlobals.ActiveVessel.Parts;

			foreach (Part part in vesselParts) {
				foreach (PartModule module in part.Modules) {
					if (module.moduleName == "ModuleWheelBase") {
						wheelCount++;

					}
				}
			}
			return wheelCount;
		}


		private int GetWheelsLanded()
		{
			int count = 0;
			List<Part> vesselParts = FlightGlobals.ActiveVessel.Parts; 
			foreach (Part part in vesselParts) {
				foreach (PartModule module in part.Modules) {
                    if ((module.moduleName.IndexOf("wheel", StringComparison.OrdinalIgnoreCase) >= 0)) {
                        if (part.GroundContact)
                            {
                                count++;
                            }
                        }
					}
			}
			return count;
		}

        //List<Anomalies.Anomaly> anomaliesList = new List<Anomalies.Anomaly>();

        public void SetClosestAnomaly()
        {
            // this is run on establishing landing spot (to avoid expensive constant foreach loops

            SetRoverLocation(); // (update rover location)
            closestAnomaly = Anomalies.Instance.ClosestAnomaly(Vessel, Vessel.mainBody.bodyName);

        }

    }

}

