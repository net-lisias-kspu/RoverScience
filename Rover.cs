using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RoverScience
{

	public class COORDS
	{
		public double latitude;
		public double longitude;
	}


	// Much of the coordinate work with latitude/longitude in this source is only functional with the work here:
	// http://www.movable-type.co.uk/scripts/latlong.html

	public class Rover
	{

		public System.Random rand = new System.Random();

		public ScienceSpot scienceSpot;
		public LandingSpot landingSpot;

		public COORDS location = new COORDS ();
		public double distanceTraveled = 0;
		public double distanceCheck = 20;
		public double distanceTraveledTotal = 0;

		public int minRadius = 40;
		public int maxRadius = 100;

        public List<string> anomaliesAnalyzed = new List<string>();

		public double DistanceFromLandingSpot
		{
			get{
				return GetDistanceBetweenTwoPoints (location, landingSpot.location);
			}
		}

		public double DistanceFromScienceSpot
		{
			get{
                return GetDistanceBetweenTwoPoints(location, scienceSpot.location);
			}
		}

		public double BearingToScienceSpot
		{
			get {
                return GetBearingFromCoords(scienceSpot.location);
			}
		}   

		Vessel Vessel
		{
			get{
				return FlightGlobals.ActiveVessel;
			}
		}

        RoverScience RoverScience
        {
            get
            {
                return RoverScience.Instance;
            }
        }

		public double Heading
		{
			get{
				return GetRoverHeading ();
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
                return GetDistanceBetweenTwoPoints(location, closestAnomaly.location);
            }
        }

        public void CalculateDistanceTraveled(double deltaTime)
		{
			distanceTraveled += (RoverScience.vessel.srfSpeed) * deltaTime;
            if (!scienceSpot.established) distanceTraveledTotal += (RoverScience.vessel.srfSpeed) * deltaTime;
		}

        public void SetRoverLocation()
        {
            location.latitude = Vessel.latitude;
            location.longitude = Vessel.longitude;
        }

		public double GetDistanceBetweenTwoPoints(COORDS _from, COORDS _to)
		{
            
            double bodyRadius = Vessel.mainBody.Radius;
			double dLat = (_to.latitude - _from.latitude).ToRadians();
			double dLon = (_to.longitude - _from.longitude).ToRadians();
			double lat1 = _from.latitude.ToRadians();
			double lat2 = _to.latitude.ToRadians();

			double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
				Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
			double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
			double d = bodyRadius * c;

			return Math.Round(d, 4);
		}


		public double GetBearingFromCoords(COORDS target)
		{
			// Rover x,y position

			double dLat = (target.latitude - location.latitude).ToRadians();
			double dLon = (target.longitude - location.longitude).ToRadians();
			double lat1 = location.latitude.ToRadians();
			double lat2 = target.latitude.ToRadians();

			double y = Math.Sin(dLon) * Math.Cos(lat2);
			double x = Math.Cos(lat1) * Math.Sin(lat2) -
				Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);

			double bearing = Math.Atan2(y, x).ToDegrees();
			//bearing = (bearing + 180) % 360;

			//return bearing % 360;
			return (bearing + 360) % 360;
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

        private double GetRoverHeading()
		{
            //Vector3d coM = vessel.findLocalCenterOfMass();
            Vector3d coM = Vessel.localCoM;
            Vector3d up = (coM - Vessel.mainBody.position).normalized;
			Vector3d north = Vector3d.Exclude(up, (Vessel.mainBody.position + 
				(Vector3d)Vessel.mainBody.transform.up * Vessel.mainBody.Radius) - coM).normalized;

			Quaternion rotationSurface = Quaternion.LookRotation(north, up);
			Quaternion rotationVesselSurface = Quaternion.Inverse(Quaternion.Euler(90, 0, 0) * Quaternion.Inverse(Vessel.GetTransform().rotation) * rotationSurface);
			return rotationVesselSurface.eulerAngles.y;
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

        List<Anomalies.Anomaly> anomaliesList = new List<Anomalies.Anomaly>();
        public Anomalies.Anomaly closestAnomaly = new Anomalies.Anomaly();

        public void SetClosestAnomaly(string bodyName)
        {
            // this is run on establishing landing spot (to avoid expensive constant foreach loops

            SetRoverLocation(); // (update rover location)
            double distanceClosest = 0;
            double distanceCheck = 0;

            if (Anomalies.Instance.HasAnomalies(bodyName))
            {
                anomaliesList = Anomalies.Instance.GetAnomalies(bodyName);

                closestAnomaly = anomaliesList[0]; // set initial

                // check and find closest anomaly
                int i = 0;
                foreach (Anomalies.Anomaly anomaly in anomaliesList)
                {
                    distanceClosest = GetDistanceBetweenTwoPoints(location, closestAnomaly.location);
                    distanceCheck = GetDistanceBetweenTwoPoints(location, anomaly.location);

                    //Debug.Log("========" + i + "========");
                    //Debug.Log("distanceClosest: " + distanceClosest);
                    //Debug.Log("distanceCheck: " + distanceCheck);

                    //Debug.Log("Current lat/long: " + location.latitude + "/" + location.longitude);
                    //Debug.Log("Closest Anomaly lat/long: " + closestAnomaly.location.latitude + "/" + closestAnomaly.location.longitude);
                    //Debug.Log("Check Anomaly lat/long: " + anomaly.location.latitude + "/" + anomaly.location.longitude);

                    //Debug.Log("==========<END>==========");


                    if (distanceCheck < distanceClosest)
                    {
                        closestAnomaly = anomaly;
                    }
                    i++;
                }

                distanceClosest = GetDistanceBetweenTwoPoints(location, closestAnomaly.location);
                Debug.Log("======= RS: closest anomaly details =======");
                Debug.Log("long/lat: " + closestAnomaly.location.longitude + "/" + closestAnomaly.location.latitude);
                Debug.Log("instantaneous distance: " + GetDistanceBetweenTwoPoints(location, closestAnomaly.location));
                Debug.Log("id: " + closestAnomaly.id);
                Debug.Log("name: " + closestAnomaly.name);
                Debug.Log("=== RS: closest anomaly details <<END>>====");
            }
        }

    }


	public static class NumericExtensions
	{
		public static double ToRadians(this double val)
		{
			return (Math.PI / 180) * val;
		}

		public static double ToDegrees(this double val)
		{
			return (180 / Math.PI) * val;
		}
	}

}

