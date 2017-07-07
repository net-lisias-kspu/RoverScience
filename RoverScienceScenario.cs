using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoverScience
{
    // This will handle future saving of upgrades
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[] { GameScenes.FLIGHT })]
    public class RoverScienceScenario : ScenarioModule
    {

        //private RoverScienceDB DB;
        //private RoverScience RS;
        //private RoverScienceGUI.GUIClass ConsoleGUI;

        public override void OnLoad(ConfigNode node)
        {
            if (RoverScience.Instance == null) return; // do not do if RoverScience not do

            Utilities.Log("RoverScienceScenario OnLoad @" + DateTime.Now);

            Utilities.Log("Attempted to LOAD FILE VIA KSPSCENARIO v2");

            //DB  = RoverScienceDB.Instance;
            //RS = RoverScience.Instance;
            //ConsoleGUI = RoverScience.Instance.roverScienceGUI.consoleGUI;

            LoadAnomaliesAnalyzed(node); // load anomalies
            

            // LEVELMAXDISTANCE
            if (node.HasValue("levelMaxDistance"))
            {
                RoverScienceDB.Instance.levelMaxDistance = Convert.ToInt32(node.GetValue("levelMaxDistance"));
            }
            else
            {
                node.AddValue("levelMaxDistance", RoverScienceDB.Instance.levelMaxDistance.ToString());
            }

            // LEVELPREDICTIONACCURACY
            if (node.HasValue("levelPredictionAccuracy"))
            {
                RoverScienceDB.Instance.levelPredictionAccuracy = Convert.ToInt32(node.GetValue("levelPredictionAccuracy"));
            }
            else
            {
                node.AddValue("levelPredictionAccuracy", RoverScienceDB.Instance.levelPredictionAccuracy.ToString());
            }

            // LEVELANALYZEDDECAY
            if (node.HasValue("levelAnalyzedDecay"))
            {
                RoverScienceDB.Instance.levelAnalyzedDecay = Convert.ToInt32(node.GetValue("levelAnalyzedDecay"));
            }
            else
            {
                node.AddValue("levelAnalyzedDecay", RoverScienceDB.Instance.levelAnalyzedDecay.ToString());
            }

            // CONSOLEGUI
            if (node.HasValue("console_x_y_show"))
            {
                string loadedString = node.GetValue("console_x_y_show");
                List<string> loadedStringList = new List<string>(loadedString.Split(','));

                RoverScienceDB.Instance.console_x_y_show = loadedStringList;

                //consoleGUI.rect.x = (float)Convert.ToDouble(loadedStringList[0]);
                //consoleGUI.rect.y = (float)Convert.ToDouble(loadedStringList[1]);
                //consoleGUI.isOpen = Convert.ToBoolean(loadedStringList[2]);
            }
            else
            {
                //string consoleDetailString = consoleGUI.rect.x + "," + consoleGUI.rect.y + "," + consoleGUI.isOpen;
                node.AddValue("console_x_y_show", string.Join(",", RoverScienceDB.Instance.console_x_y_show.ToArray()));
            }

            if (RoverScience.Instance.rover != null)
            {
                RoverScienceDB.Instance.UpdateRoverScience();
            }

            
        }

        public override void OnSave(ConfigNode node)
        {
            if (RoverScience.Instance == null) return; // do not do if RoverScience not do

            Utilities.Log("RoverScienceScenario OnSave @" + DateTime.Now);

            SaveAnomaliesAnalyzed(node);

            node.SetValue("levelMaxDistance", RoverScienceDB.Instance.levelMaxDistance.ToString(), true);
            node.SetValue("levelPredictionAccuracy", RoverScienceDB.Instance.levelPredictionAccuracy.ToString(), true);
            node.SetValue("levelAnalyzedDecay", RoverScienceDB.Instance.levelAnalyzedDecay.ToString(), true);
            node.SetValue("console_x_y_show", string.Join(",", RoverScienceDB.Instance.console_x_y_show.ToArray()), true);

            //node.SetValue("levelMaxDistance", roverScience.getUpgradeLevel(RSUpgrade.maxDistance).ToString(), true);
            //node.SetValue("levelPredictionAccuracy", roverScience.getUpgradeLevel(RSUpgrade.predictionAccuracy).ToString(), true);
            //node.SetValue("levelAnalyzedDecay", roverScience.getUpgradeLevel(RSUpgrade.analyzedDecay).ToString(), true);
        }

        public void SaveAnomaliesAnalyzed(ConfigNode node)
        {
            Utilities.Log("Attempting to save anomalies analyzed");
            List<string> anomaliesAnalyzed = RoverScienceDB.Instance.anomaliesAnalyzed;

            if (anomaliesAnalyzed.Any())
            {
                if (anomaliesAnalyzed.Count > 1)
                {
                    node.SetValue("anomalies_visited_id", string.Join(",", anomaliesAnalyzed.ToArray()), true);
                } else
                {
                    node.SetValue("anomalies_visited_id", anomaliesAnalyzed[0], true);
                }
            } else
            {
                Utilities.Log("no anomalies id to save");
            }
        }

        public void LoadAnomaliesAnalyzed(ConfigNode node)
        {
            if (node.HasValue("anomalies_visited_id"))
            {
                string loadedString = node.GetValue("anomalies_visited_id");
                List<string> loadedStringList = new List<string>(loadedString.Split(','));

                Utilities.Log("loadedString: " + loadedString);
                foreach (string s in loadedStringList)
                {
                    Utilities.Log("ID: " + s);
                }
                Utilities.Log("ID LOAD END");
                RoverScienceDB.Instance.anomaliesAnalyzed = loadedStringList; // load in new values in anomalies
                
            }
            else
            {
                Utilities.Log("No anomalies have been analyzed");
            }

        }
    }
}
