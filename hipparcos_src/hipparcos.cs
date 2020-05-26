using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using SimpleJSON;

namespace hipparcos
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class StarStats : MonoBehaviour 
    { 
    void Start()
        {
            string starCatalogPath = "/GameData/Hipparcos/Data/hygdata_v3_mag66.json"; //defaults to standard catalog TODO: add error handling if settings.cfg doesn't exist
            ConfigNode settings = ConfigNode.Load(KSPUtil.ApplicationRootPath + "/GameData/Hipparcos/settings.cfg");//loading user-configured path for star catalog
            foreach (ConfigNode node in settings.GetNodes("Settings"))
            {
                starCatalogPath = node.GetValue("starCatalogPath");
            }

            string dataPath = KSPUtil.ApplicationRootPath + starCatalogPath;
            string JSONString = File.ReadAllText(dataPath);
            var starCatalog = JSON.Parse(JSONString);
            //Importing stellar data from truncated HYG stellar database:
            //id: HYG ID
            //hip: Hipparcos catalog ID
            //hd: Henry Draper catalog ID
            //hr: Harvard Revised catalog ID
            //gl: Gliese catalog ID
            //bf: Bayer/Flamsteed ID
            //proper: The common name for a star. Most don't have one.
            //ra: right ascension (J2000)
            //dec: declination (J2000)
            //dist: Distance from Earth in parsecs. ly dist = pc dist * 3.262
            //pmra: proper motion in ra
            //pmdec: proper motion in dec
            //rv: radial velocity
            //mag: apparent magnitude
            //absmag: absolute magnitude (apparent mag from 10 pcs away)
            //spect: spectral type
            //ci: color index
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
            //See https://github.com/astronexus/HYG-Database for more details. Some variable names changed slightly from source
            foreach (JSONNode o in starCatalog["starCatalog"].Children)
            {
                Debug.Log(o["id"].Value);
            }
        }
    }
}