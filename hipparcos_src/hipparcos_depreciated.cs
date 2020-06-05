////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////
//Depreciated method of making the starfield using meshes.//
////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using SimpleJSON;
using JetBrains.Annotations;
using UnityEngine.Rendering;
namespace hipparcos
{

    [RequireComponent(typeof(MeshFilter))]
    [KSPAddon(KSPAddon.Startup.Flight, true)]
    public class CelestialSphere : MonoBehaviour//generates the celestial sphere
    {
        Mesh celestialSphereMesh;
        Vector3[] starVerts;
        int[] starTris;
        int numEntries = 0;
        GameObject celestialSphere;
        void Start()
        {
            celestialSphereMesh = new Mesh();
            GetComponent<MeshFilter>().mesh = celestialSphereMesh;
            var JSONString = StarCatalog();
            var starCatalog = JSON.Parse(JSONString);
            for (int i = 0; i < starCatalog["starCatalog"].Count; ++i)
            {
                numEntries += 1;
            }
            print("Number of stars is: " + numEntries);
            int v = 0;
            int t = 0;//counters
            starVerts = new Vector3[4 * numEntries];
            starTris = new int[6 * numEntries];//as many triangles and vertices as we need for all the stars
            foreach (JSONNode o in starCatalog["starCatalog"].Children)
            {
                float rarad = float.Parse(o["rarad"].Value);
                float decrad = float.Parse(o["decrad"].Value);
                //print("Star " + o["id"].Value + " " + o["proper"].Value + " has coordinates of (" + x + ", " + y + " , " + z + ")"); //for debug purposes
                AStarisBorn(rarad, decrad, v, t, ref v, ref t);
                //print("Star " + o["id"].Value + " " + o["proper"].Value + " complete"); //also for debug purposes 
            }
            celestialSphereMesh.vertices = starVerts;
            celestialSphereMesh.triangles = starTris;
            celestialSphereMesh.RecalculateNormals();
            celestialSphere = new GameObject("Celestial Sphere");
            celestialSphere.AddComponent<MeshFilter>();
            celestialSphere.AddComponent<MeshRenderer>();
            celestialSphere.GetComponent<MeshFilter>().mesh = celestialSphereMesh;
            celestialSphere.transform.SetParent(FlightCamera.fetch.mainCamera.transform);
        }
        void LateUpdate()
        {
            celestialSphere.transform.rotation = Quaternion.Euler(0,0,0);//locking the sphere in place
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
            float r = 30.0f; //radius of celestial sphere in Unity units. Can be rather small since it will be always bound to the camera and rendered behind everything else.
            z = r * Mathf.Cos(decrad) * Mathf.Cos(rarad);//forward/backward axis
            x = r * Mathf.Cos(decrad) * Mathf.Sin(rarad);//right/left axis
            y = r * Mathf.Sin(decrad); //Up/down axis.  These letters refer to different axes than the textbook examples of these equations, that is because of Unity's coordinate system.
        }
        void AStarisBorn(float rarad, float decrad, int v, int t, ref int v2, ref int t2) //makes a star!
        {
            float vert1x = 0;
            float vert1y = 0;
            float vert1z = 0;
            float vert2x = 0;
            float vert2y = 0;
            float vert2z = 0;
            float vert3x = 0;
            float vert3y = 0;
            float vert3z = 0;
            float vert4x = 0;
            float vert4y = 0;
            float vert4z = 0;
            float vert1rad = rarad + 0.001f;
            float vert2rad = rarad + 0.001f;
            float vert3rad = rarad - 0.001f;
            float vert4rad = rarad - 0.001f;
            float vert1dec = decrad + 0.001f;
            float vert2dec = decrad - 0.001f;
            float vert3dec = decrad + 0.001f;
            float vert4dec = decrad - 0.001f;
            RaDecToXYZ(vert1rad, vert1dec, ref vert1x, ref vert1y, ref vert1z);
            RaDecToXYZ(vert2rad, vert2dec, ref vert2x, ref vert2y, ref vert2z);
            RaDecToXYZ(vert3rad, vert3dec, ref vert3x, ref vert3y, ref vert3z);
            RaDecToXYZ(vert4rad, vert4dec, ref vert4x, ref vert4y, ref vert4z);//by waiting until now to convert equatorial coordinates into unity coordinates, we get a very easy way of producing squares that face the camera.


                starVerts[    v] = new Vector3(vert1x, vert1y, vert1z);
                starVerts[v + 1] = new Vector3(vert2x, vert2y, vert2z);
                starVerts[v + 2] = new Vector3(vert3x, vert3y, vert3z);
                starVerts[v + 3] = new Vector3(vert4x, vert4y, vert4z);

                starTris[    t] =     v;
                starTris[t + 1] = v + 1;
                starTris[t + 2] = v + 2;
                starTris[t + 3] = v + 2;
                starTris[t + 4] = v + 1;
                starTris[t + 5] = v + 3; // making the two triangles for each star


                v2 = v + 4;
                t2 = t + 6;

        }

    }
}
