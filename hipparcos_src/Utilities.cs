using SimpleJSON;
using System.IO;
using UnityEngine;

namespace hipparcos
{
    public class Utilities : MonoBehaviour
    {
        static ConfigNode settings = ConfigNode.Load(KSPUtil.ApplicationRootPath + "GameData/Hipparcos/settings.cfg");
        public static bool IsSkyboxDisabled()
        {
            bool returnValue = false;
            if (HSettings.skyboxDisabled == true && HSettings.distantObjectCompatibility == true)
            {
                returnValue = true;
            }
            return returnValue;
        }

        public static Color GetStarSaturation(Color baseColor)
        {
            Color returnValue = Color.Lerp(Color.white, baseColor, HSettings.starSaturation);
            return returnValue;
        }

        public static float GetStarSize(float mag)
        {
            float returnValue;
            returnValue = HSettings.starBaseSize + HSettings.starSizeMultiplier * (6.5f - mag);
            return returnValue;
        }

        public static Texture2D GetStarTex()
        {
            Texture2D starTex = null;
            if (GameDatabase.Instance.ExistsConfigNode("Hipparcos/settings/Settings"))
            { 
                string dataPath = GetPath("starTex");
                if (GameDatabase.Instance.ExistsTexture(dataPath))
                {
                    starTex = GameDatabase.Instance.GetTexture(dataPath, false);
                }
            }
            else
            {
                if (GameDatabase.Instance.ExistsTexture("Hipparcos/Data/starTex"))
                {
                    starTex = GameDatabase.Instance.GetTexture("Hipparcos/Data/starTex", false);
                }
            }
            return starTex;
        }

        public static JSONNode GetStarCatalog()//imports stellar data from database at start of game
        {
            string dataPath;
            if (GameDatabase.Instance.ExistsConfigNode("Hipparcos/settings/Settings"))
            {
                dataPath = GetPath("starCatalog");
            }
            else
            {
                dataPath = "GameData/Hipparcos/Data/hygdata_v3_mag66.json";
            }
            dataPath = KSPUtil.ApplicationRootPath + dataPath;
            dataPath = File.ReadAllText(dataPath);
            JSONNode starCatalog = JSON.Parse(dataPath);
            return starCatalog;
            //id: HYG ID
            //hip: Hipparcos catalog ID
            //hd: Henry Draper catalog ID
            //hr: Harvard Revised catalog ID
            //gl: Gliese catalog ID
            //bf: Bayer/Flamsteed ID
            //proper: The common name for a star. Most don't have one.
            //rarad: ra in radians
            //decrad: dec in radians
            //mag: apparent magnitude. Important for brightness of star.
            //ci: color index. Important for determining color of star.
            //See https://github.com/astronexus/HYG-Database for more details. Some variable names changed slightly from source.
        }
        public static bool GetBool(string inputString)//the flag in the settings file.  Whether the Skybox is actually disabled depends upon if Distant Object Enhancement compatibility is turned on.
        {
            bool returnValue = false;
            int storedValue = 0;
            foreach (ConfigNode node in settings.GetNodes("Settings"))
            {
                storedValue = int.Parse(node.GetValue(inputString));
            }
            if (storedValue == 1)
            {
                returnValue = true;
            }
            return returnValue;
        }

        public static float GetFloat(string inputString)//the flag in the settings file.  Whether the Skybox is actually disabled depends upon if Distant Object Enhancement compatibility is turned on.
        {
            float returnValue = 0;
            foreach (ConfigNode node in settings.GetNodes("Settings"))
            {
                returnValue = float.Parse(node.GetValue(inputString));
            }
            return returnValue;
        }

        static string GetPath(string inputString)
        {
            string returnValue = ""; 
            foreach (ConfigNode node in settings.GetNodes("Paths"))
            {
                returnValue = node.GetValue(inputString);
            }
            return returnValue;
        }
    }
}