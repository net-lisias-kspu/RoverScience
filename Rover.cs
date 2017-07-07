using System;
using System.Collections.Generic;
using UnityEngine;

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
		{
			get{
				return GeomLib.GetDistanceBetweenTwoPoints (Vessel.mainBody, location, landingSpot.location);
			}
		}

		public double DistanceFromScienceSpot
		{
			get{
                return GeomLib.GetDistanceBetweenTwoPoints(Vessel.mainBody, location, scienceSpot.location);
			}
		}

		public double BearingToScienceSpot
		{
			get {
                return GeomLib.GetBearingFromCoords(scienceSpot.location, location);
			}
		}   

		Vessel Vessel
		{
			get{
				return FlightGlobals.ActiveVessel;
			}
		}

		public double Heading
		{
			get{
				return GeomLib.GetRoverHeading (Vessel);
			}
		}

		public bool ScienceSpotReached
		{
			get {
				if (scienceSpot.established) {
					if (DistanceFromScienceSpot <= scienceSpot.minDistance) {
						return true;
					}
				}
				return false;
			}
		}

        public bool AnomalySpotReached
        {
            get
            {
                if (scienceSpot.established)
                {
                    if (DistanceToClosestAnomaly <= scienceSpot.minDistance)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool AnomalyPresent
        {
            get
            {
                return ((DistanceToClosestAnomaly <= 100) && !Anomalies.Instance.HasCurrentAnomalyBeenAnalyzed());
            }
        }

        public int NumberWheelsLanded
		{
			get
			{
				return GetWheelsLanded();
			}
		}

        public int NumberWheels
        {
            get
            {
                return GetWheelCount();
            }
        }

        public bool ValidStatus
        {
            get
            {
                return CheckRoverValidStatus();
            }
        }

        public double DistanceToClosestAnomaly
        {
            get
            {
                if (location == null)
                    Utilities.Log("location == null in DistanceToClosestAnomaly");
                if (closestAnomaly == null)
                    Utilities.Log("closestAnomaly == null in DistanceToClosestAnomaly");
                if (Vessel == null)
                    Utilities.Log("Vessel == null in DistanceToClosestAnomaly");

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

