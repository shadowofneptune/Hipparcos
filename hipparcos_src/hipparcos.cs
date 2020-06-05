using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using SimpleJSON;
using UnityEngine.Rendering;
using Shabby;
using System.Xml.Schema;
using Smooth.Collections;

namespace hipparcos
{

    
    [KSPAddon(KSPAddon.Startup.FlightAndKSC, false)]
    public class CelestialSphere : MonoBehaviour//generates the celestial sphere
    {
        private ParticleSystem celestialSpherePartSys;
        private ParticleSystem.Particle[] stars;
        int numEntries;
        float x;
        float y;
        float z;
        Vector3 position;
        GameObject celestialSphere;
     
        Material starMat = new Material(Shabby.Shabby.FindShader("Particles/Infinite Depth Unlit"));
        
    void Start()
        {
            var JSONString = StarCatalog();
            var starCatalog = JSON.Parse(JSONString);
            for (int i = 0; i < starCatalog["starCatalog"].Count; ++i)
            {
                numEntries += 1;
            }
            print("Number of stars is: " + numEntries);
            stars = new ParticleSystem.Particle[numEntries];
            int starsArray = 0;
            print("Initiating celestial sphere generation...");
            foreach (JSONNode o in starCatalog["starCatalog"].Children)
            {
                float rarad = float.Parse(o["rarad"].Value);
                float decrad = float.Parse(o["decrad"].Value);
                float mag = float.Parse(o["mag"].Value);
                float ci = float.Parse(o["ci"].Value);
                AStarisBorn(starsArray, rarad, decrad, mag, ci);
                starsArray += 1;
            }
            Texture2D starTex = null;
            if (GameDatabase.Instance.ExistsTexture("Hipparcos/Data/starTex"))
            {
                starTex = GameDatabase.Instance.GetTexture("Hipparcos/Data/starTex", false);
            }

            
            celestialSphere = new GameObject("Celestial Sphere");
            celestialSphere.layer = 10;
            celestialSphere.AddComponent<ParticleSystem>();
            celestialSpherePartSys = celestialSphere.GetComponent<ParticleSystem>();
            celestialSpherePartSys.GetComponent<ParticleSystemRenderer>().material = starMat;
            celestialSpherePartSys.GetComponent<ParticleSystemRenderer>().material.mainTexture = starTex;
            celestialSpherePartSys.SetParticles(stars, stars.Length);
            print("Celestial sphere generation complete");
            celestialSpherePartSys.Stop();
            celestialSphere.transform.position = PlanetariumCamera.fetch.transform.position;
            celestialSphere.transform.rotation = Quaternion.Euler(0, 0, 0);
            celestialSphere.transform.SetParent(PlanetariumCamera.fetch.transform);


        }

        void LateUpdate()
        {
            celestialSphere.transform.rotation = Planetarium.Rotation;
            if ((MapView.MapIsEnabled) & (TimeWarp.CurrentRate > 100))
            {
                celestialSpherePartSys.GetComponent<ParticleSystemRenderer>().enabled = false;//TODO: Fix that stupid issue of timewarp breaking how the stars look in map view so it doesn't have to be disabled at high time warp.

            }
            else
            {
                celestialSpherePartSys.GetComponent<ParticleSystemRenderer>().enabled = true;
            }
            
        }
        

        static string StarCatalog()//imports stellar data from database at start of game
        {
            string starCatalogPath = "/GameData/Hipparcos/Data/hygdata_v3_mag66.json"; //defaults to standard catalog TODO: add error handling if settings.cfg doesn't exist
            ConfigNode settings = ConfigNode.Load(KSPUtil.ApplicationRootPath + "/GameData/Hipparcos/settings.cfg");//loading user-configured path for star catalog
            foreach (ConfigNode node in settings.GetNodes("Settings"))
            {
                starCatalogPath = node.GetValue("starCatalogPath");
            }

            string dataPath = KSPUtil.ApplicationRootPath + starCatalogPath;
            string returnvalue = File.ReadAllText(dataPath);
            //id: HYG ID
            //hip: Hipparcos catalog ID
            //hd: Henry Draper catalog ID
            //hr: Harvard Revised catalog ID
            //gl: Gliese catalog ID
            //bf: Bayer/Flamsteed ID
            //proper: The common name for a star. Most don't have one.
            //ra: right ascension (J2000). Important for determining position on celestial sphere.
            //dec: declination (J2000). Important for determining position on celestial sphere.
            //dist: Distance from Earth in parsecs. ly dist = pc dist * 3.262
            //pmra: proper motion in ra
            //pmdec: proper motion in dec
            //rv: radial velocity
            //mag: apparent magnitude. Important for brightness of star.
            //absmag: absolute magnitude (apparent mag from 10 pcs away)
            //spect: spectral type
            //ci: color index. Important for determining color of star.
            //rarad: ra in radians
            //decrad: dec in radians
            //pmrarad: proper motion in ra in radians
            //pmdecrad: proper motion in dec in radians
            //bayer: Bayer designation
            //flam: Flamsteed number
            //con: constellation abbreviation
            //comp: ID of companion star. Gliese stars only.
            //comp_primary: ID of primary star. Gliese stars only.
            //base: ID of the multi-star system. Gliese stars only.
            //lum: luminosity as multiple of Sun's luminosity
            //var: variable star designation
            //var_min: lowest magnitude if variable
            //var_max: lowest highest if variable
            //we'll never use all of these, but it's nice to have around just in case
            //See https://github.com/astronexus/HYG-Database for more details. Some variable names changed slightly from source.
            return returnvalue;
        }

        void RaDecToXYZ(float rarad, float decrad, ref float x, ref float y, ref float z) //Converts right ascension and declination (both in radians) to XYZ coordinates.  While it's possible to use this to get the true positions of stars using their distances in parsecs, we just want to make a simplified model: a celestial sphere.
        {
            float r = 2000.0f; //radius of celestial sphere in Unity units. Can be rather small since it will be always bound to the camera and rendered behind everything else.
            z = 0f-(r * Mathf.Cos(decrad) * Mathf.Cos(rarad));//forward/backward axis
            x = r * Mathf.Cos(decrad) * Mathf.Sin(rarad);//right/left axis
            y = r * Mathf.Sin(decrad); //Up/down axis.  These letters refer to different axes than the textbook examples of these equations, that is because of Unity's coordinate system.
        }
        void AStarisBorn(int starsArray, float rarad, float decrad, float mag, float ci) //makes a star!
        {
            RaDecToXYZ(rarad, decrad, ref x, ref y, ref z);
            //print("Star " + o["id"].Value + " " + o["proper"].Value + " has coordinates of (" + -x + ", " + y + " , " + -z + ")"); //for debug purposes
            position = new Vector3 (x, y, z);
            Color starColor = new Color(1,1,1,1);
            stars[starsArray].position = position;
            stars[starsArray].startSize = 2.0f * (8.0f - mag);
            ColorFinder(ci, ref starColor);
            stars[starsArray].startColor = starColor;
            stars[starsArray].startLifetime = Mathf.Infinity;
        }
        void ColorFinder(float ci, ref Color returnColor)
        {
            
            float r;
            float g;
            float b;
            float temp = 4600 * ((1 / (0.92f * ci + 1.7f)) + (1 / (0.92f * ci + 0.62f))); //converting B-V color index to temperature using a formula from Ballesteros 2012: https://doi.org/10.1209/0295-5075/97/34008
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
                returnColor = new Color(r, g, b, 1);
                print(returnColor);
        }
       

        void OnDestroy()
        {
            Destroy(celestialSphere);
        }
    }
}


