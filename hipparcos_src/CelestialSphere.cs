using SimpleJSON;
using UnityEngine;
namespace hipparcos
{
    public class CelestialSphere : MonoBehaviour //the parent class for generation of the celestial sphere
    {
        public ParticleSystem celestialSpherePartSys;
        ParticleSystem.Particle[] stars;
        public GameObject celestialSphere;
        readonly Material starMat = new Material(Shabby.Shabby.FindShader("Particles/Infinite Depth Unlit"));

        void Start()
        {
            MakeCelestialSphere();
        }

        void MakeCelestialSphere()
        {
            stars = new ParticleSystem.Particle[Functions.GetStarCatalog()["starCatalog"].Count];
            int starsArray = 0;
            print("Initiating celestial sphere generation...");
            foreach (JSONNode o in Functions.GetStarCatalog()["starCatalog"].Children)
            {
                float rarad = float.Parse(o["rarad"].Value);
                float decrad = float.Parse(o["decrad"].Value);
                float mag = float.Parse(o["mag"].Value);
                float ci = float.Parse(o["ci"].Value);
                AStarisBorn(starsArray, rarad, decrad, mag, ci);
                starsArray += 1;
            }
            celestialSphere = new GameObject("Celestial Sphere");
            SetSphereLayer();
            celestialSphere.AddComponent<ParticleSystem>();
            celestialSpherePartSys = celestialSphere.GetComponent<ParticleSystem>();
            celestialSpherePartSys.GetComponent<ParticleSystemRenderer>().material = starMat;
            celestialSpherePartSys.GetComponent<ParticleSystemRenderer>().material.mainTexture = Functions.GetStarTex();
            celestialSpherePartSys.SetParticles(stars, stars.Length);
            print("Celestial sphere generation complete.  Number of stars: " + stars.Length);
            ParentSphereToCamera();
        }

        public void SetSphereLayer()//replaced in CelestialSphereMainMenu
        {
            celestialSphere.layer = 10;
        }

        public void ParentSphereToCamera()//replaced in CelestialSphereMainMenu
        {
            celestialSphere.transform.position = PlanetariumCamera.fetch.transform.position;
            celestialSphere.transform.SetParent(PlanetariumCamera.fetch.transform);
        }

        void AStarisBorn(int starsArray, float rarad, float decrad, float mag, float ci)
        {                    
            Vector3 position = Functions.RaDecToXYZ(rarad, decrad);
            stars[starsArray].position = position;
            stars[starsArray].startSize = HSettings.starBaseSize + HSettings.starSizeMultiplier * (6.5f - mag); 
            //magnitude 6.5 being the smallest stars visible with the naked eye.
            Color starColor = Functions.ColorGenerator(ci);
            starColor = Color.Lerp(Color.white, starColor, HSettings.starSaturation);
            stars[starsArray].startColor = starColor;
            stars[starsArray].startLifetime = Mathf.Infinity;
        }
    }

    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class CelestialSphereMainMenu : CelestialSphere
    {
        readonly GameObject skyboxCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        readonly Material sphereMat = new Material(Shabby.Shabby.FindShader("Unlit/InfiniteSky"));
        Quaternion menuSpherePos = Quaternion.Euler(0, 225, 0);

        void Awake()
        {
            HSettings.Load(false);
            if (HSettings.skyboxDisabled == true)
            {   
                /*this stupid dumb workaround is because of the GalaxyCube game object in the Main Menu scene
                 *not having GalaxyCubeControl. This is the only scene used in the plugin where it's missing.*/
                var skyboxRenderer = skyboxCube.GetComponent<Renderer>();
                skyboxRenderer.material = sphereMat;
                skyboxCube.transform.localScale = new Vector3(5000, 5000, 5000);
                skyboxCube.layer = 0;
            }
        }

        public new void SetSphereLayer()
        {
            celestialSphere.layer = 0;
        }

        public new void ParentSphereToCamera()
        {
            celestialSphere.transform.position = GameObject.Find("Landscape Camera").transform.position;
            celestialSphere.transform.SetParent(GameObject.Find("Landscape Camera").transform);
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

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class CelestialSphereFlight : CelestialSphere
    {
        bool celestialSphereDisabled = false;
        void Awake()
        {
            HSettings.Load(false);
            if (HSettings.skyboxDisabled == true)
            {
                GalaxyCubeControl.Instance.maxGalaxyColor = Color.black;
            }
        }

        public void LateUpdate()
        {
            var situation = FlightGlobals.ActiveVessel.situation;
            var landed = Vessel.Situations.LANDED;
            var prelaunch = Vessel.Situations.PRELAUNCH;
            var splashed = Vessel.Situations.SPLASHED;
            celestialSphere.transform.rotation = Planetarium.Rotation;

            if (celestialSphereDisabled == false && MapView.MapIsEnabled && TimeWarp.CurrentRate > 100)
            {
                if (situation == landed || situation == prelaunch || situation == splashed)
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

    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public class CelestialSphereTrackingStation : CelestialSphereFlight
    {
        public new void LateUpdate()
        {
            celestialSphere.transform.rotation = Planetarium.Rotation;
        }
    }

    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class CelestialSphereSpaceCentre : CelestialSphereTrackingStation { }
}