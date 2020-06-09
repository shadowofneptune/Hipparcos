using SimpleJSON;
using UnityEngine;
namespace hipparcos
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class CelestialSphereSpaceCentre : CelestialSphereTrackingStation { }

    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public class CelestialSphereTrackingStation : CelestialSphereFlight 
    {
        public new void LateUpdate()
        {
            celestialSphere.transform.rotation = Planetarium.Rotation;
        }
    }

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class CelestialSphereFlight : CelestialSphere
    {
        bool celestialSphereDisabled = false;
        void Awake()
        {
            HSettings.Load(false);
            if (Utilities.IsSkyboxDisabled() == true)
            {
                GalaxyCubeControl.Instance.maxGalaxyColor = Color.black;
            }
        }

        public void LateUpdate()
        {
            celestialSphere.transform.rotation = Planetarium.Rotation;
            if (celestialSphereDisabled == false && MapView.MapIsEnabled && TimeWarp.CurrentRate > 100)
            {
                if (FlightGlobals.ActiveVessel.situation == Vessel.Situations.LANDED || FlightGlobals.ActiveVessel.situation == Vessel.Situations.PRELAUNCH || FlightGlobals.ActiveVessel.situation == Vessel.Situations.SPLASHED)
                {
                    celestialSpherePartSys.GetComponent<ParticleSystemRenderer>().enabled = false;
                    celestialSphereDisabled = true;
                }
            }
            if (celestialSphereDisabled == true && MapView.MapIsEnabled && TimeWarp.CurrentRate <= 100)
            {
                celestialSpherePartSys.GetComponent<ParticleSystemRenderer>().enabled = true;
                celestialSphereDisabled = false;
            }
            if (celestialSphereDisabled == true && !MapView.MapIsEnabled)
            {
                celestialSpherePartSys.GetComponent<ParticleSystemRenderer>().enabled = true;
                celestialSphereDisabled = false;
            }
        }

        void OnDestroy()
        {
            Destroy(celestialSphere);
        }
    }

    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class CelestialSphereMainMenu : CelestialSphere 
    {
        GameObject skyboxCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Material sphereMat = new Material(Shabby.Shabby.FindShader("Unlit/InfiniteSky"));
        Quaternion menuSpherePos = Quaternion.Euler(0, 225, 0);

        void Awake()
        {
            HSettings.Load(false);
            if (Utilities.IsSkyboxDisabled() == true)
            {
                //this stupid dumb hack is because of the GalaxyCube game object in the Main Menu scene not having GalaxyCubeControl.  This is the only scene used in the plugin where it's missing.
                var skyboxRenderer = skyboxCube.GetComponent<Renderer>();
                skyboxRenderer.material = sphereMat;
                skyboxCube.transform.localScale = new Vector3(5000, 5000, 5000);
                skyboxCube.layer = 0;
            }
        }
        void LateUpdate()
        {
            celestialSphere.transform.rotation = menuSpherePos;

        }
        void OnDestroy()
        {
            Destroy(celestialSphere);
            Destroy(skyboxCube);
        }
    }

    public class CelestialSphere : MonoBehaviour
    {
        public ParticleSystem celestialSpherePartSys;
        private ParticleSystem.Particle[] stars;
        float x;
        float y;
        float z;
        Vector3 position;
        public GameObject celestialSphere;
        Material starMat = new Material(Shabby.Shabby.FindShader("Particles/Infinite Depth Unlit"));

        void Start()
        {
            HSettings.Load(false);
            CelestialSphereGeneration();
        }

        void CelestialSphereGeneration()
        {
            stars = new ParticleSystem.Particle[Utilities.GetStarCatalog()["starCatalog"].Count];
            int starsArray = 0;
            print("Initiating celestial sphere generation...");
            foreach (JSONNode o in Utilities.GetStarCatalog()["starCatalog"].Children)
            {
                float rarad = float.Parse(o["rarad"].Value);
                float decrad = float.Parse(o["decrad"].Value);
                float mag = float.Parse(o["mag"].Value);
                float ci = float.Parse(o["ci"].Value);
                AStarisBorn(starsArray, rarad, decrad, mag, ci);
                starsArray += 1;
            }
            celestialSphere = new GameObject("Celestial Sphere");
            if (HighLogic.LoadedScene == GameScenes.MAINMENU)
            {
                celestialSphere.layer = 0;
            }
            else
            {
                celestialSphere.layer = 10;
            }
            celestialSphere.AddComponent<ParticleSystem>();
            celestialSpherePartSys = celestialSphere.GetComponent<ParticleSystem>();
            celestialSpherePartSys.GetComponent<ParticleSystemRenderer>().material = starMat;
            celestialSpherePartSys.GetComponent<ParticleSystemRenderer>().material.mainTexture = Utilities.GetStarTex();
            celestialSpherePartSys.SetParticles(stars, stars.Length);
            print("Celestial sphere generation complete.  Number of stars: " + stars.Length);
            celestialSpherePartSys.Stop();
            if (HighLogic.LoadedScene == GameScenes.MAINMENU)
            {

                celestialSphere.transform.position = GameObject.Find("Landscape Camera").transform.position;
                celestialSphere.transform.SetParent(GameObject.Find("Landscape Camera").transform);
            }
            else
            {
                celestialSphere.transform.position = PlanetariumCamera.fetch.transform.position;
                celestialSphere.transform.SetParent(PlanetariumCamera.fetch.transform);
            }
        }

        void RaDecToXYZ(float rarad, float decrad, ref float x, ref float y, ref float z) //Converts right ascension and declination (both in radians) to XYZ coordinates. While it's possible to use this to get the true positions of stars using their distances in parsecs, we just want to make a simplified model: a celestial sphere.
        {
            float r = 2000.0f; //radius of celestial sphere in Unity units. Can be rather small in cosmic terms since it will be always bound to the camera and rendered behind everything else.
            z = 0 - r * Mathf.Cos(decrad) * Mathf.Cos(rarad);//forward/backward axis
            x = r * Mathf.Cos(decrad) * Mathf.Sin(rarad);//right/left axis
            y = r * Mathf.Sin(decrad); //Up/down axis.  These letters refer to different axes than the textbook examples of these equations, that is because of Unity's coordinate system.
        }

        void AStarisBorn(int starsArray, float rarad, float decrad, float mag, float ci)
        {
            RaDecToXYZ(rarad, decrad, ref x, ref y, ref z);
            //print("Star " + o["id"].Value + " " + o["proper"].Value + " has coordinates of (" + -x + ", " + y + " , " + -z + ")"); //for debug purposes
            position = new Vector3(x, y, z);
            Color starColor = ColorFinder(ci);
            stars[starsArray].position = position;
            stars[starsArray].startSize = Utilities.GetStarSize(mag);
            stars[starsArray].startColor = Utilities.GetStarSaturation(starColor);
            stars[starsArray].startLifetime = Mathf.Infinity;
        }
        Color ColorFinder(float ci)
        {
            float r;
            float g;
            float b;
            float temp = 4600 * (1 / (0.92f * ci + 1.7f) + 1 / (0.92f * ci + 0.62f)); //converting B-V color index to temperature using a formula from Ballesteros 2012: https://doi.org/10.1209/0295-5075/97/34008
            //the following is based off the algorithm found here: https://tannerhelland.com/2012/09/18/convert-temperature-rgb-algorithm-code.html
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
        }
    }
}