using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;

namespace RoverScience
{
    [KSPAddon(KSPAddon.Startup.Flight, true)]
    public class Anomalies : MonoBehaviour
    {
        // public static CelestialBody HomeWorld = 
        public static Anomalies Instance = null;
        public static Dictionary<string, List<Anomaly>> anomaliesDict = new Dictionary<string, List<Anomaly>>();

        public class Anomaly
        {
            public string name = "anomaly";
            public Coords location = new Coords();
            //public double longitude = 0;
            //public double latitude = 0;
            public string id = "NA";
            // surface altitude is determined by DrawWaypoint
        }

        public Anomalies()
        {
            Instance = this;
            Utilities.Log("Attempted to load anomaly coordinates");
            LoadAnomalies();
        }

        public bool HasCurrentAnomalyBeenAnalyzed()
        {
            Rover rover = RoverScience.Instance.rover;
            string closestAnomalyID = rover.closestAnomaly.id;
            if (rover.anomaliesAnalyzed.Contains(closestAnomalyID))
            {
                return true;
            } else
            {
                return false;
            }

        }

        public List<Anomaly> GetAnomalies(string bodyName)
        {
            if (anomaliesDict.ContainsKey(bodyName))
            {
                return anomaliesDict[bodyName];
            } else
            {
                Utilities.Log("###### getAnomalies KEY DOES NOT EXIST!");
                return null;
            }
        }

        public bool HasAnomalies(string bodyName)
        {
            if (anomaliesDict.ContainsKey(bodyName))
            {
                return true;
            } else
            {
                return false;
            }
        }

        private void LoadAnomalies()
        {
            // anomaliesDict contains a list of [Anomaly]s that each contain longitude/latitude, name and id
            // load anomalies from Anomalies.cfg
            try
            {
                string fileName = KSPUtil.ApplicationRootPath + "GameData/RoverScience/Anomalies.cfg";
                Utilities.Log("loadAnomlies HAS ATTEMPTED TO LOAD FROM THIS PATH: " + fileName);

                ConfigNode mainNode = ConfigNode.Load(fileName);

                foreach (ConfigNode bodyNode in mainNode.GetNodes("BODY"))
                {
                    List<Anomaly> _anomalyList = new List<Anomaly>();
                    
                    foreach (ConfigNode anomalyNode in bodyNode.GetNodes("anomaly"))
                    {
                        Anomaly _anomaly = new Anomaly();
                        string[] latlong = Regex.Split(anomalyNode.GetValue("position"), " : ");
                        _anomaly.location.latitude = Convert.ToDouble(latlong[0]);
                        _anomaly.location.longitude = Convert.ToDouble(latlong[1]);

                        if (anomalyNode.HasValue("name"))
                        {
                            _anomaly.name = anomalyNode.GetValue("name");
                        }

                        if (anomalyNode.HasValue("id"))
                        {
                            _anomaly.id = anomalyNode.GetValue("id");
                        }


                        _anomalyList.Add(_anomaly);
                    }

                    anomaliesDict.Add(bodyNode.GetValue("name"), _anomalyList);
                }
            }
            catch
            {
                Utilities.Log("EXCEPTION: Catch Anomaly Coordinates Problem");
            }
        }

        public Anomaly ClosestAnomaly(Vessel vessel, string bodyName)
        {
            Utilities.Log("Checking for closest anomaly");
            if (Anomalies.Instance.HasAnomalies(bodyName))
            {
                var anomaliesList = Anomalies.Instance.GetAnomalies(bodyName);

                var closestAnomaly = anomaliesList[0]; // set initial

                // check and find closest anomaly
                int i = 0;
                double distanceClosest;
                double distanceCheck;
                Coords location = new Coords { latitude = vessel.latitude, longitude = vessel.longitude };
                foreach (Anomaly anomaly in anomaliesList)
                {
                    distanceClosest = GeomLib.GetDistanceBetweenTwoPoints(vessel.mainBody, location, closestAnomaly.location);
                    distanceCheck = GeomLib.GetDistanceBetweenTwoPoints(vessel.mainBody, location, anomaly.location);

                    //Utilities.Log("========" + i + "========");
                    //Utilities.Log("distanceClosest: " + distanceClosest);
                    //Utilities.Log("distanceCheck: " + distanceCheck);

                    //Utilities.Log("Current lat/long: " + location.latitude + "/" + location.longitude);
                    //Utilities.Log("Closest Anomaly lat/long: " + closestAnomaly.location.latitude + "/" + closestAnomaly.location.longitude);
                    //Utilities.Log("Check Anomaly lat/long: " + anomaly.location.latitude + "/" + anomaly.location.longitude);

                    //Utilities.Log("==========<END>==========");


                    if (distanceCheck < distanceClosest)
                    {
                        closestAnomaly = anomaly;
                    }
                    i++;
                }

                distanceClosest = GeomLib.GetDistanceBetweenTwoPoints(vessel.mainBody, location, closestAnomaly.location);
                Utilities.Log("======= RS: closest anomaly details =======");
                Utilities.Log("long/lat: " + closestAnomaly.location.longitude + "/" + closestAnomaly.location.latitude);
                Utilities.Log("instantaneous distance: " + distanceClosest);
                Utilities.Log("id: " + closestAnomaly.id);
                Utilities.Log("name: " + closestAnomaly.name);
                Utilities.Log("=== RS: closest anomaly details <<END>>====");

                return closestAnomaly;
            }
            Utilities.Log("No anomalies found for " + bodyName);
            return new Anomaly();
        }
    }
}
