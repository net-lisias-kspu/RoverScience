using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RoverScience
{
    public partial class RoverScienceGUI
    {
        // Use this to change saved game's science for
        // selling and purchasing tech
        // WATCH OUT FOR QUICKSAVE/QUICKLOAD

        private float currentScience
        {
           get
           {
                return ResearchAndDevelopment.Instance.Science;
            }
        }

        private void drawUpgradeGUI(int windowID)
        {


            // UPGRADE WINDOW
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(Localizer.Format("#LOC_RoverScience_GUI_ScienceAvailable" + currentScience)); // Science Available: <<1>>
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            drawUpgradeType(RSUpgrade.maxDistance);
            drawUpgradeType(RSUpgrade.predictionAccuracy);
            drawUpgradeType(RSUpgrade.analyzedDecay);

            GUILayout.Label(Localizer.GetStringByTag("#LOC_RoverScience_GUI_UpgradesPermanent")); // All upgrades are permanent and apply across all rovers"
            GUILayout.EndVertical();
			GUI.DragWindow ();
        }

        private void drawUpgradeType(RSUpgrade upgradeType)
        {

            int currentLevel = roverScience.getUpgradeLevel(upgradeType);
            int nextLevel = currentLevel + 1;
            //double upgradeValueNow = roverScience.getUpgradeValue(upgradeType, currentLevel);
            //double upgradeValueNext = roverScience.getUpgradeValue(upgradeType, (nextLevel));

            string upgradeValueNow = roverScience.getUpgradeValueString(upgradeType, currentLevel);
            string upgradeValueNext = roverScience.getUpgradeValueString(upgradeType, (nextLevel));

            double upgradeCost = roverScience.getUpgradeCost(upgradeType, (nextLevel));

            

            GUILayout.BeginHorizontal();
            
            GUILayout.Label(roverScience.getUpgradeName(upgradeType));
            GUILayout.Space(5);
            GUILayout.Button(Localizer.Format("#LOC_RoverScience_GUI_BtnUpgCurrent", upgradeValueNow, currentLevel)); // Current: <<1>> [<<2>>]
            GUILayout.Button(Localizer.Format("#LOC_RoverScience_GUI_BtnUpgNext", (upgradeValueNext == "-1" ? Localizer.GetStringByTag("#LOC_RoverScience_GUI_Max") : upgradeValueNext.ToString()))); // Next <<1>>
            GUILayout.Button(Localizer.Format("#LOC_RoverScience_GUI_BtnUpgCost", (upgradeCost == -1 ? Localizer.GetStringByTag("#LOC_RoverScience_GUI_Max") : upgradeCost.ToString()))); // Cost <<1>>
            
            if (GUILayout.Button(Localizer.GetStringByTag("#LOC_RoverScience_GUI_BtnUpgrade"))) // UP
            {
				Debug.Log ("Upgrade button pressed - " + upgradeType);
                roverScience.upgradeTech(upgradeType);
            }
            
            GUILayout.EndHorizontal();

        }
    }
}