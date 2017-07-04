using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RoverScience
{

    public class LandingSpot
    {
        public bool established = false;
        public COORDS location = new COORDS();
        public System.Random rand = new System.Random();
		RoverScience roverScience = null;

		public LandingSpot (RoverScience roverScience)
		{
				this.roverScience = roverScience;
		}

        public Rover Rover
        {
            get
            {
				return roverScience.rover;
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
                    Debug.Log("Vessel vessel returned null!");
                    return null;
                }
            }

        }

        public void SetSpot()
        {
            // check if LandingSpot has already been established
            if (!established)
            {
                // SET LANDING SITE
	                if (Rover.NumberWheelsLanded > 0)
	                {
	                    // set x by y position
	                    location.longitude = Vessel.longitude;
                        location.latitude = Vessel.latitude;

	                    Rover.ResetDistanceTraveled();

	                    established = true;

	                    Debug.Log("Landing site has been established!");

                        Rover.SetClosestAnomaly(Vessel.mainBody.name);
	                }
			
            }
            else
            {
                // RESET LANDING SITE
                if ((Rover.NumberWheelsLanded == 0) && (Vessel.heightFromTerrain > 10)) 
                    Reset();
			}
        }

        public void Reset()
        {
            established = false;
            location.longitude = 0;
            location.latitude = 0;

            Rover.ResetDistanceTraveled();
            Rover.distanceTraveledTotal = 0;
            Debug.Log("Landing Spot reset!");
        }
    }
}
