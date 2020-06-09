using System.IO;
using UnityEngine;

namespace hipparcos
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class HSettingsMainMenu : HSettings { }

    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public class HSettingsTrackingStation : HSettings { }

    [KSPAddon(KSPAddon.Startup.FlightAndKSC, false)]
    public class HSettings : MonoBehaviour
    {
        public static float starSaturation;
        public static float starBaseSize;
        public static float starSizeMultiplier;
        public static bool skyboxDisabled;
        public static bool distantObjectCompatibility;
        public static void Load(bool reset)
        {
            if (GameDatabase.Instance.ExistsConfigNode("Hipparcos/settings/Settings") && reset == false)
            {
                starSaturation = Utilities.GetFloat("starSaturation");
                starBaseSize = Utilities.GetFloat("starBaseSize");
                starSizeMultiplier = Utilities.GetFloat("starSizeMultiplier");
                skyboxDisabled = Utilities.GetBool("skyboxDisabled");
                distantObjectCompatibility = Utilities.GetBool("distantObjectCompatibility");
            }
            else
            {
                starSaturation = 1;
                starBaseSize = 2.7f;
                starSizeMultiplier = 3;
                skyboxDisabled = true;
                distantObjectCompatibility = true;
                //default settings
            }
        }
    }
}