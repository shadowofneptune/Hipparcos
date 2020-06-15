using System.IO;
using System.Collections.Generic;
using UnityEngine;
using KSP.UI.Screens;
using System.Text.RegularExpressions;
using ClickThroughFix;


namespace hipparcos
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class ToolbarGUI : MonoBehaviour
	{
        void Awake()
        {
            GameEvents.onGUIEditorToolbarReady.Add(AddIcon);
            GameEvents.onGUIApplicationLauncherReady.Add(AddButton);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(RemoveButton);
            GameEvents.onGameSceneSwitchRequested.Add(SetButtonVisibility);
        }

        void AddIcon()
        {

        }

        void AddButton()
        {

        }
        void RemoveButton()
        {

        }
        void SetButtonVisibility(GameEvents.FromToAction<GameScenes, GameScenes> data)
        {

        }

    }
}