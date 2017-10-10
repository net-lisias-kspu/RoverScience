using System;
using System.Collections.Generic;
using UnityEngine;

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
                Utilities.LogVerbose($"No anomalies for body {bodyName}");
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
            try
            {
                Utilities.Log("Reading anomalies from game database");
                var bodies = FlightGlobals.Bodies;
                int counter;
                foreach (var body in bodies)
                {
                    var anomalies = new List<Anomaly>();
                    PQSSurfaceObject[] sites = body.pqsSurfaceObjects;
                    counter = 0;
                    foreach (var site in sites)
                    {
                        anomalies.Add( new Anomaly {
                            id = (++counter).ToString(),
                            name = site.name,
                            location = new Coords
                            {
                                longitude = body.GetLongitude(site.transform.position),
                                latitude = body.GetLatitude(site.transform.position)
                            }
                        });
                    }
                    if (anomalies.Count > 0)
                    {
                        anomaliesDict.Add(body.name, anomalies);
                        Utilities.Log($"Added {anomalies.Count}  anomalies for body '{body.name}'");
                    }
                }

            }
            catch (Exception e)
            {
                Utilities.Log($"Exception: anomaly initialisation problem {e.Message}");
                Utilities.Log(e.StackTrace);
            }
        }

        public Anomaly ClosestAnomaly(Vessel vessel, string bodyName)
        {
            Utilities.LogVerbose("Checking for closest anomaly");
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

                    if (distanceCheck < distanceClosest)
                    {
                        closestAnomaly = anomaly;
                    }
                    i++;
                }

                distanceClosest = GeomLib.GetDistanceBetweenTwoPoints(vessel.mainBody, location, closestAnomaly.location);
                Utilities.LogVerbose("======= RS: closest anomaly details =======");
                Utilities.LogVerbose("long/lat: " + closestAnomaly.location.longitude + "/" + closestAnomaly.location.latitude);
                Utilities.LogVerbose("instantaneous distance: " + distanceClosest);
                Utilities.LogVerbose("id: " + closestAnomaly.id);
                Utilities.LogVerbose("name: " + closestAnomaly.name);
                Utilities.LogVerbose("=== RS: closest anomaly details <<END>>====");

                return closestAnomaly;
            }
            Utilities.LogVerbose("No anomalies found for " + bodyName);
            return new Anomaly();
        }
    }
}
