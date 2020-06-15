using SimpleJSON;
using System.IO;
using UnityEngine;

namespace hipparcos
{
    public class Functions : MonoBehaviour //listed in alphabetical order
    {
        readonly static ConfigNode settings = ConfigNode.Load(KSPUtil.ApplicationRootPath + "GameData/Hipparcos/settings.cfg");
        
        public static Color ColorGenerator(float ci)
        {
            float r,g,b;
            float temp = 4600 * (1 / (0.92f * ci + 1.7f) + 1 / (0.92f * ci + 0.62f)); 
            /*converting B-V color index to temperature using a formula from Ballesteros 2012: 
            https://doi.org/10.1209/0295-5075/97/34008 */
            temp /= 100;
            //red
            if (temp <= 66)
            {
                r = 1;
            }
            else
            {
                r = temp - 60;
                r = 329.698727446f * Mathf.Pow(r, -0.1332047592f);
                r /= 255;
                if (r < 0)
                {
                    r = 0;
                }
                if (r > 1)
                {
                    r = 1;
                }
            }
            //green
            if (temp <= 66)
            {
                g = temp;
                g = 99.4708025861f * Mathf.Log(g) - 161.1195681661f;
                g /= 255;
                if (g < 0)
                {
                    g = 0;
                }
                if (g > 1)
                {
                    g = 1;
                }
            }
            else
            {
                g = temp - 60;
                g = 288.1221695283f * Mathf.Pow(g, -0.0755148492f);
                g /= 255;
                if (g < 0)
                {
                    g = 0;
                }
                if (g > 1)
                {
                    g = 1;
                }
            }
            //blue
            if (temp >= 66)
            {
                b = 1;
            }
            else
            {
                if (temp <= 19)
                {
                    b = 0;
                }
                else
                {
                    b = temp - 10;
                    b = 138.5177312231f * Mathf.Log(b) - 305.0447927307f;
                    b /= 255;
                    if (b < 0)
                    {
                        b = 0;
                    }
                    if (b > 1)
                    {
                        b = 1;
                    }
                }
            }
            Color returnColor = new Color(r, g, b, 1);
            return returnColor;
            /*the above is an implementation of the algorithm found here: 
            https://tannerhelland.com/2012/09/18/convert-temperature-rgb-algorithm-code.html */
        }

        public static bool GetBool(string inputString)
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

        public static float GetFloat(string inputString)
        {
            float returnValue = 0;
            foreach (ConfigNode node in settings.GetNodes("Settings"))
            {
                returnValue = float.Parse(node.GetValue(inputString));
            }
            return returnValue;
        }

        static string GetPath(string inputString)//inputString is the flag in the settings file.  
        {
            string returnValue = "";
            foreach (ConfigNode node in settings.GetNodes("Paths"))
            {
                returnValue = node.GetValue(inputString);
            }
            return returnValue;
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
            //    id: HYG ID
            //   hip: Hipparcos catalog ID
            //    hd: Henry Draper catalog ID
            //    hr: Harvard Revised catalog ID
            //    gl: Gliese catalog ID
            //    bf: Bayer/Flamsteed ID
            //proper: The common name for a star. Most don't have one.
            // rarad: ra in radians
            //decrad: dec in radians
            //   mag: apparent magnitude. Important for brightness of star.
            //    ci: color index. Important for determining color of star.
            //See https://github.com/astronexus/HYG-Database for more details.
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

        public static Vector3 RaDecToXYZ(float rarad, float decrad) 
        //right ascension and declination must be in radians
        {
            float x, y, z;
            float radius = 2000.0f; 
            z = 0 - radius * Mathf.Cos(decrad) * Mathf.Cos(rarad);
            x = radius * Mathf.Cos(decrad) * Mathf.Sin(rarad);
            y = radius * Mathf.Sin(decrad);
            Vector3 returnValue = new Vector3(x, y, z);
            return returnValue;
        }
    }
}