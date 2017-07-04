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

        private bool analyzeButtonPressedOnce = false;
		private string inputMaxDistance = "100";

        GUIStyle boldFont = new GUIStyle();
        GUIStyle noWrapFont = new GUIStyle();


        
        private string SetRichColor(string s, string color)
        {
            // color to be inputted as "#xxxxxx";
            if (color == "green")
            {
                color = "#00ff00ff";
            }
            else if (color == "red")
            {
                color = "#ff0000ff";
            } else if (color == "blue")
            {
                color = "#add8e6ff";
            } else if (color == "yellow")
            {
                color = "#ffff00ff";
            } else if (color == "orange")
            {
                color = "#ffa500ff";
            }

            return ("<color=" + color + ">" + s + "</color>");
        }

        private string PotentialFontColor(string name)
        {
            if (name == "Very High!" || name == "High")
            {
                return SetRichColor(name, "green");
            } else if (name == "Normal")
            {
                return SetRichColor(name, "blue");
            } else if (name == "Low")
            {
                return SetRichColor(name, "yellow");
            } else
            {
                return SetRichColor(name, "red");
            }
        }

        private string PredictionFontColor(double percentage)
        {
            if (percentage > 70) return SetRichColor(Localizer.Format("#LOC_RoverScience_GUI_Percentage", percentage.ToString()), "green");
            else if (percentage >= 50) return SetRichColor(Localizer.Format("#LOC_RoverScience_GUI_Percentage", percentage.ToString()), "yellow");
            else return SetRichColor(Localizer.Format("#LOC_RoverScience_GUI_Percentage", percentage.ToString()), "red");
        }

        private string DecayFontColor (double percentage)
        {
            if (percentage > 70) return SetRichColor(Localizer.Format("#LOC_RoverScience_GUI_Percentage", percentage.ToString()), "red");
            else if (percentage >= 50) return SetRichColor(Localizer.Format("#LOC_RoverScience_GUI_Percentage", percentage.ToString()), "yellow");
            else return SetRichColor(Localizer.Format("#LOC_RoverScience_GUI_Percentage", percentage.ToString()), "green");
        }


        private void DrawRoverConsoleGUI(int windowID)
        {
            if (Rover.scienceSpot.established && Rover.ScienceSpotReached)
            {
                consoleGUI.rect.height = 559;
            } else if (Rover.scienceSpot.established)
            {
                consoleGUI.rect.height = 495;
            } else if (!Rover.scienceSpot.established)
            {
                consoleGUI.rect.height = 466;
            }


            boldFont = new GUIStyle(GUI.skin.label);
            noWrapFont = new GUIStyle(GUI.skin.label);

            boldFont.fontStyle = FontStyle.Bold;
            boldFont.wordWrap = false;

            noWrapFont.wordWrap = false;

            GUILayout.BeginVertical(GUIStyles.consoleArea);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, new GUILayoutOption[] { GUILayout.Width(240), GUILayout.Height(340) });

            GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.Label(Localizer.GetStringByTag("#LOC_RoverScience_GUI_SpotsAnalyzed") , boldFont); // "Science Spots Analyzed: "
            GUILayout.Label(roverScience.amountOfTimesAnalyzed.ToString(), boldFont);
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
            GUILayout.Label(Localizer.GetStringByTag("#LOC_RoverScience_GUI_ReuseLoss") + DecayFontColor(roverScience.ScienceDecayPercentage), boldFont); // "Science Loss due to re-use: "
            GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();

            GUICenter("_____________________");
            GUIBreakline();

            if (!Rover.landingSpot.established)
            {
                GUILayout.Label(Localizer.GetStringByTag("#LOC_RoverScience_GUI_NoLanding")); // > No landing spot established!
                GUILayout.Label(Localizer.GetStringByTag("#LOC_RoverScience_GUI_NoLandingWheels")); // > You must first establish a landing spot by landing somewhere. Make sure you have wheels!
                GUIBreakline();
                GUILayout.Label(Localizer.Format("#LOC_RoverScience_GUI_WheelsDetected", Rover.NumberWheels)); // > Rover wheels detected: <<1>> 
                GUILayout.Label(Localizer.Format("#LOC_RoverScience_GUI_WheelsLanded", Rover.NumberWheelsLanded)); // > Rover wheels landed: <<1>> 

            } else {
                if (!Rover.scienceSpot.established)
                {
                    // PRINT OUT CONSOLE CONTENTS

                    if (roverScience.ScienceDecayPercentage >= 100)
                    {
                        GUILayout.Label(SetRichColor(Localizer.GetStringByTag("#LOC_RoverScience_GUI_ScienceLimit"), "red")); //  > You have analyzed too many times.\n> Science loss is now at 100%.\n> Send another rover.
                    } else {
                        GUILayout.Label(Localizer.GetStringByTag("#LOC_RoverScience_GUI_DriveAround")); // > Drive around to search for science spots . . .
                        GUILayout.Label(Localizer.Format("#LOC_RoverScience_GUI_ScanningRange", Rover.maxRadius)); // > Currently scanning at range: " + rover.maxRadius + "m");
                        //GUILayout.Label("> Total dist. traveled searching: " + Math.Round(rover.distanceTraveledTotal, 2));
                        GUIBreakline();
                        foreach (string line in consolePrintOut)
                        {
                            GUILayout.Label(line);
                        }

                        if (Vessel.mainBody.bodyName == "Kerbin")
                        {
                            GUILayout.Label(SetRichColor(Localizer.GetStringByTag("#LOC_RoverScience_GUI_HomeWorld"), "red")); // > WARNING - there is very little rover science for Kerbin!
                        }
                    }

                } else {
                    if (!Rover.ScienceSpotReached)
                    {
                        double relativeBearing = Rover.Heading - Rover.BearingToScienceSpot;

                        GUILayout.BeginVertical();
                        GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
                        if (!Rover.AnomalyPresent)
                        {
                            GUILayout.Label(SetRichColor(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Potential"), "yellow")); // "[POTENTIAL SCIENCE SPOT]"
                        } else
                        {
                            GUILayout.Label(SetRichColor(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Anomaly"), "yellow")); // "[ANOMALY DETECTED]"
                        }

                        GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();

                        GUILayout.Label(Localizer.Format("#LOC_RoverScience_GUI_DistToSpot", Math.Round(Rover.DistanceFromScienceSpot, 1))); // "> Distance to spot (m): <<1>>" + );
                        //GUILayout.Label("Bearing of Site (degrees): " + Math.Round(rover.bearingToScienceSpot, 1));
                        //GUILayout.Label("Rover Bearing (degrees): " + Math.Round(rover.heading, 1));
                        //GUILayout.Label("Rel. Bearing (degrees): " + Math.Round(relativeBearing, 1));

                        if (!Rover.AnomalyPresent)
                        {
                            //GUILayout.Label("> Science Potential: " + rover.scienceSpot.predictedSpot + " (" + roverScience.currentPredictionAccuracy + "% confident)");
                            GUILayout.Label(Localizer.Format("#LOC_RoverScience_GUI_Prediction", PotentialFontColor(Rover.scienceSpot.predictedSpot))); // > Science Prediction: <<1>>"
                            GUILayout.Label(Localizer.Format("#LOC_RoverScience_GUI_Confidence", PredictionFontColor(roverScience.CurrentPredictionAccuracy))); // > Prediction is <<1>> confident");
                        }
                        else
                        {
                            GUILayout.Label(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Anomaly2")); //"> ANOMALY DETECTED. Something interesting is nearby . . . the science potential should be very significant")
                        }
                        //GUIBreakline();
                        //GUIBreakline();

                        //This block handles writing getDriveDirection
                        //GUILayout.BeginHorizontal ();
                        //GUILayout.FlexibleSpace ();
                        //GUILayout.Label(getDriveDirection(rover.bearingToScienceSpot, rover.heading));
                        //GUILayout.FlexibleSpace ();
                        //GUILayout.EndHorizontal ();
                        GUILayout.EndVertical();
                    }
                    else
                    {

                        GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
                        GUILayout.Label(SetRichColor(Localizer.GetStringByTag("#LOC_RoverScience_GUI_SpotReached"), "green")); // "[SCIENCE SPOT REACHED]"
                        GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();

                        //GUILayout.Label("Total dist. traveled for this spot: " + Math.Round(rover.distanceTraveledTotal, 1));
                        //GUILayout.Label("Distance from landing site: " +
                        //Math.Round(rover.getDistanceBetweenTwoPoints(rover.scienceSpot.location, rover.landingSpot.location), 1));
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(Localizer.Format("#LOC_RoverScience_GUI_ActualPotential", PotentialFontColor(Rover.scienceSpot.potentialGenerated))); // "> Science Potential: <<1>> (actual)");
                        GUILayout.EndHorizontal();

                        GUIBreakline();

                        GUILayout.Label(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Note")); // "> NOTE: The more you analyze, the less you will get each time!");
                    }

                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            // ACTIVATE ROVER BUTTON
            if (Rover.ScienceSpotReached)
            {
               
                if (!analyzeButtonPressedOnce)
                {
                    if (GUILayout.Button(Localizer.GetStringByTag("#LOC_RoverScience_GUI_BtnAnalyze"), GUILayout.Height(60))) // "Analyze Science"
                    {
                        if (roverScience.container.GetStoredDataCount() == 0)
                        {
                            if (Rover.ScienceSpotReached)
                            {
                                analyzeButtonPressedOnce = true;
                                consolePrintOut.Clear();

                            }
                            else if (!Rover.ScienceSpotReached)
                            {
                                ScreenMessages.PostScreenMessage(Localizer.GetStringByTag("#LOC_RoverScience_GUI_GetToSpot"), 3, ScreenMessageStyle.UPPER_CENTER); // "Cannot analyze - Get to the science spot first!"
                            }
                        }
                        else
                        {
                            ScreenMessages.PostScreenMessage(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Full"), 3, ScreenMessageStyle.UPPER_CENTER); // "Cannot analyze - Rover Brain already contains data!"
                        }
                    }
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(Localizer.GetStringByTag("#LOC_RoverScience_GUI_BtnConfirm"))) // "Confirm"
                    {
                        analyzeButtonPressedOnce = false;
                        roverScience.AnalyzeScienceSample();
                    }
                    if (GUILayout.Button(Localizer.GetStringByTag("#LOC_RoverScience_GUI_BtnCancel"))) //"Cancel"
                    {
                        analyzeButtonPressedOnce = false;
                    }
                    GUILayout.EndHorizontal();
                }
            }

            if (Rover.scienceSpot.established)
            {
                if (GUILayout.Button(Localizer.GetStringByTag("#LOC_RoverScience_GUI_BtnResetSpot"))) // "Reset Science Spot"
                {
                    Rover.scienceSpot.established = false;
                    Rover.ResetDistanceTraveled();
                    DrawWaypoint.Instance.DestroyInterestingObject();
                    DrawWaypoint.Instance.HideMarker();
                    consolePrintOut.Clear();

                }
            }

            //if (GUILayout.Button ("Reorient from Part")) {
            //roverScience.command.MakeReference ();
            //}
            GUIBreakline();
            GUIBreakline();
            if (roverScience.ScienceDecayPercentage < 100)
            {
            GUILayout.BeginHorizontal();
            inputMaxDistance = GUILayout.TextField(inputMaxDistance, 5, new GUILayoutOption[] { GUILayout.Width(40) });

            
                if (GUILayout.Button(Localizer.Format("#LOC_RoverScience_GUI_BtnScanRange", roverScience.CurrentMaxDistance))) // "Set Scan Range [Max: <<1>>]"
                {

                    int inputMaxDistanceInt;
                    bool isNumber = int.TryParse(inputMaxDistance, out inputMaxDistanceInt);


                    if ((isNumber) && (inputMaxDistanceInt <= roverScience.CurrentMaxDistance) && (inputMaxDistanceInt >= 40))
                    {
                        Rover.maxRadius = inputMaxDistanceInt;
                        Debug.Log("Set maxRadius to input: " + Rover.maxRadius);
                        ScreenMessages.PostScreenMessage(Localizer.Format("#LOC_RoverScience_GUI_Scanning", Rover.maxRadius), 3, ScreenMessageStyle.UPPER_CENTER); // "Now scanning for science spots at range: <<1>>
                    }

                    if (inputMaxDistanceInt > roverScience.CurrentMaxDistance)
                    {
                        ScreenMessages.PostScreenMessage(Localizer.Format("#LOC_RoverScience_GUI_OverMax", roverScience.CurrentMaxDistance), 3, ScreenMessageStyle.UPPER_CENTER); // "Cannot set scan range over max distance of: <<1>>
                    } else if (inputMaxDistanceInt < 40)
                    {
                        ScreenMessages.PostScreenMessage(Localizer.GetStringByTag("#LOC_RoverScience_GUI_MinRange"), 3, ScreenMessageStyle.UPPER_CENTER); // "Minimum of 40m scan range is required"
                    }
                }
            }

            GUILayout.EndHorizontal();

			if (GUILayout.Button (Localizer.GetStringByTag("#LOC_RoverScience_GUI_BtnUpgradeMenu"))) { // "Upgrade Menu"
				upgradeGUI.Toggle ();
			}

			GUILayout.Space (5);
			if (GUILayout.Button (Localizer.GetStringByTag("#LOC_RoverScience_GUI_BtnShutdown"))) { // "Close and Shutdown"
				Rover.scienceSpot.established = false;
				Rover.ResetDistanceTraveled ();
				consolePrintOut.Clear ();

                DrawWaypoint.Instance.HideMarker();

				consoleGUI.Hide ();
				upgradeGUI.Hide ();
			}
			GUI.DragWindow ();
		}


        private string GetDriveDirection(double destination, double currentHeading)
        {
            // This will calculate the closest angle to the destination, given a current heading.
            // Everything here will be in degrees, not radians
           
            // Shift destination angle to 000 bearing. Apply this shift to the currentHeading in the same direction.
            double delDestAngle = 0;
            double shiftedCurrentHeading = 0;

            if (destination > 180) {
                // Delta will be clockwise
                delDestAngle = 360 - destination;
                shiftedCurrentHeading = currentHeading + delDestAngle;

                if (shiftedCurrentHeading > 360) shiftedCurrentHeading -= 360;
            } else {
                // Delta will be anti-clockwise
                delDestAngle = destination;
                shiftedCurrentHeading = currentHeading - delDestAngle;

                if (shiftedCurrentHeading < 0) shiftedCurrentHeading += 360;
            }

            double absShiftedCurrentHeading = Math.Abs(shiftedCurrentHeading);

			if (absShiftedCurrentHeading < 6) {
                return Localizer.GetStringByTag("#LOC_RoverScience_GUI_DriveFwd"); // "DRIVE FORWARD";
            }

			if ((absShiftedCurrentHeading > 174) && (absShiftedCurrentHeading < 186)) {
                return Localizer.GetStringByTag("#LOC_RoverScience_GUI_TurnAround"); // "TURN AROUND";
            }

			if (absShiftedCurrentHeading < 180) {
                return Localizer.GetStringByTag("#LOC_RoverScience_GUI_TurnLeft"); // "TURN LEFT";
            }

			if (absShiftedCurrentHeading > 180) {
                return Localizer.GetStringByTag("#LOC_RoverScience_GUI_TurnRight"); //  "TURN RIGHT";
            }



            return Localizer.GetStringByTag("#LOC_RoverScience_GUI_DirectionError"); // "ERROR: FAILED TO RESOLVE DRIVE DIRECTION";

        }


	}


}

