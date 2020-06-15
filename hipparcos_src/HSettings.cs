using System.IO;
using UnityEngine;

namespace hipparcos
{
    public class HSettings : MonoBehaviour
    {
        public static float starSaturation, starBaseSize, starSizeMultiplier;
        public static bool skyboxDisabled;

        public static void Load(bool reset)//use reset to restore current settings to hardcoded defaults
        {
            if (GameDatabase.Instance.ExistsConfigNode("Hipparcos/settings/Settings") && reset == false)
            //any time you see this kind of logic in the plugin, it's to prevent any issues if the settings file is missing 
            {

                starSaturation = Functions.GetFloat("starSaturation");
                starBaseSize = Functions.GetFloat("starBaseSize");
                starSizeMultiplier = Functions.GetFloat("starSizeMultiplier");
                skyboxDisabled = Functions.GetBool("skyboxDisabled");
            }
            else
            {
                starSaturation = 1;
                starBaseSize = 2.7f;
                starSizeMultiplier = 3;
                skyboxDisabled = true;
                //default settings
            }
            if (!GameDatabase.Instance.ExistsConfigNode("Hipparcos/settings/Settings"))
            {
                //add save method here
            }
        }
        public static void Open()
        {
        
        }
        public static void Close()
        {
        
        }
    }
}