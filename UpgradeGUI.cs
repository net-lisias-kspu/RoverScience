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

        private float CurrentScience
        {
           get
           {
                return ResearchAndDevelopment.Instance.Science;
            }
        }

        private void DrawUpgradeGUI(int windowID)
        {
            if (HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX)
            {
                ScreenMessages.PostScreenMessage(Localizer.GetStringByTag("#LOC_RoverScience_GUI_NotSandbox"), 3, ScreenMessageStyle.UPPER_CENTER); // Upgrades are not available in sandbox mode
                return;
            }

            // UPGRADE WINDOW
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(Localizer.Format("#LOC_RoverScience_GUI_ScienceAvailable" + CurrentScience)); // Science Available: <<1>>
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            DrawUpgradeType(RSUpgrade.maxDistance);
            DrawUpgradeType(RSUpgrade.predictionAccuracy);
            DrawUpgradeType(RSUpgrade.analyzedDecay);

            GUILayout.Label(Localizer.GetStringByTag("#LOC_RoverScience_GUI_UpgradesPermanent")); // All upgrades are permanent and apply across all rovers"
            GUILayout.EndVertical();
			GUI.DragWindow ();
        }

        private void DrawUpgradeType(RSUpgrade upgradeType)
        {

            int currentLevel = roverScience.GetUpgradeLevel(upgradeType);
            int nextLevel = currentLevel + 1;
            //double upgradeValueNow = roverScience.getUpgradeValue(upgradeType, currentLevel);
            //double upgradeValueNext = roverScience.getUpgradeValue(upgradeType, (nextLevel));

            string upgradeValueNow = roverScience.GetUpgradeValueString(upgradeType, currentLevel);
            string upgradeValueNext = roverScience.GetUpgradeValueString(upgradeType, (nextLevel));

            double upgradeCost = roverScience.GetUpgradeCost(upgradeType, (nextLevel));

            

            GUILayout.BeginHorizontal();
            
            GUILayout.Label(roverScience.GetUpgradeName(upgradeType));
            GUILayout.Space(5);
            GUILayout.Button(Localizer.Format("#LOC_RoverScience_GUI_BtnUpgCurrent", upgradeValueNow, currentLevel)); // Current: <<1>> [<<2>>]
            GUILayout.Button(Localizer.Format("#LOC_RoverScience_GUI_BtnUpgNext", (upgradeValueNext == "-1" ? Localizer.GetStringByTag("#LOC_RoverScience_GUI_Max") : upgradeValueNext.ToString()))); // Next <<1>>
            GUILayout.Button(Localizer.Format("#LOC_RoverScience_GUI_BtnUpgCost", (upgradeCost == -1 ? Localizer.GetStringByTag("#LOC_RoverScience_GUI_Max") : upgradeCost.ToString()))); // Cost <<1>>
            
            if (GUILayout.Button(Localizer.GetStringByTag("#LOC_RoverScience_GUI_BtnUpgrade"))) // UP
            {
				Debug.Log ("Upgrade button pressed - " + upgradeType);
                roverScience.UpgradeTech(upgradeType);
            }
            
            GUILayout.EndHorizontal();

        }
    }
}