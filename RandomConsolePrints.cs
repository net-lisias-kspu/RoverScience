using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RoverScience
{

	partial class RoverScienceGUI
	{
		public class RandomConsolePrintOuts
		{
			List<string> strings = new List<string>();

			public string getRandomPrint()
			{
				System.Random rand = new System.Random();
				int randIndex = rand.Next (0, strings.Count);

				if (strings.Count > 0) {
					return strings [randIndex];
				}

                return Localizer.GetStringByTag("#LOC_RoverScience_GUI_Random1"); // "Nothing seems to be here";
			}

			public RandomConsolePrintOuts()
			{
                strings.Add(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Random1")); // "Nothing seems to be here");
				strings.Add(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Random2")); // "Running scans, checking");
				strings.Add(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Random3")); // "Weak signals of interest");
				strings.Add(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Random4")); // "Defragmenting - maybe that will help");
				strings.Add(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Random5")); // "Doing science-checks");
				strings.Add(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Random6")); // "No science here");
				strings.Add(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Random7")); // "Curiousity still increasing");
                strings.Add(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Random8")); // "Rocks, rocks, rocks, where are the interesting rocks?");
                strings.Add(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Random9")); // "Daisy . . . *cough* checking for science!");
                strings.Add(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Random10")); // "Nothing interesting here yet");
                strings.Add(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Random11")); // "Science, science!");
            }

		}

	}

}