using IRToolTrack;
using System;
using System.IO;
using System.Linq;
using UnityEditor.Search;
using UnityEngine;

namespace IRToolTrack
{

    public class MarkerManager : MonoBehaviour
    {
        public string Config_Path;

        void Start()
        {
            // Ensure the directory exists
            if (!Directory.Exists(Config_Path))
            {
                Debug.LogError("Directory does not exist: " + Config_Path);
                return;
            }
            //irToolController = FindObjectOfType<IRToolController>();
            string[] jsonFiles = Directory.GetFiles(Config_Path, "*.json");
            LoadAndCreateMarkers(jsonFiles);
        }

        private void LoadAndCreateMarkers(string[] jsonFiles)
        {
            int fid_idx = 0;
            float x_mean = 0.0f;
            float y_mean = 0.0f;
            float z_mean = 0.0f;

            foreach (var jsonFile in jsonFiles)
            {
                string jsonText = File.ReadAllText(jsonFile);
                Debug.LogWarning($"File name: {jsonFile}");
                MarkerConfig config = JsonUtility.FromJson<MarkerConfig>(jsonText);

                foreach (var fiducial in config.fiducials)
                {
                    x_mean += fiducial.x / config.count;
                    y_mean += fiducial.y / config.count;
                    z_mean += fiducial.z / config.count;
                }
                Debug.LogWarning($"{config.path}");
                GameObject prefab = Resources.Load<GameObject>(config.path);
                if (prefab == null)
                {
                    Debug.LogError("Prefab could not be loaded. Check the path.");
                }
                GameObject markerInstance = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
                IRToolController IRToolController_Instance = markerInstance.AddComponent<IRToolController>();
                IRToolController_Instance.identifier = config.identifier;

                Transform originalSphere = markerInstance.transform.Find("Sphere0"); // Replace "sphere_0" with the name of your original sphere object
                GameObject[] createdSpheres = new GameObject[config.count];

                if (originalSphere != null)
                {
                    for (int i = 1; i < config.count; i++)
                    {
                        GameObject duplicatedSphere = Instantiate(originalSphere.gameObject, markerInstance.transform);
                        duplicatedSphere.name = "Sphere" + i; // Naming the duplicated spheres as sphere_1, sphere_2, etc.

                        // Optional: Adjust the position or other properties of the duplicated sphere here
                    }
                }

                markerInstance.name = config.identifier;

                // Set the position of the pivot
                Transform pivotTransform = markerInstance.transform.Find("model");
                pivotTransform.localPosition = new Vector3(config.pivot.x - x_mean, config.pivot.y - y_mean, config.pivot.z - z_mean);
                pivotTransform.localRotation = Quaternion.Euler(config.pivot.rx, config.pivot.ry, config.pivot.rz);
                fid_idx = 0;
                foreach (var fiducial in config.fiducials)
                {
                    string fiducialName = "Sphere" + fid_idx.ToString();
                    Transform fiducialTransform = markerInstance.transform.Find(fiducialName);
                    /*                GameObject fiducialObject = new GameObject("Fiducial");
                                    fiducialObject.transform.parent = markerInstance.transform;
                                    fiducialObject.transform.localPosition = new Vector3(fiducial.x, fiducial.y, fiducial.z);*/

                    if (fiducialTransform != null)
                    {
                        fiducialTransform.localPosition = new Vector3(fiducial.x - x_mean, fiducial.y - y_mean, fiducial.z - z_mean);
                        fiducialTransform.localScale = new Vector3(config.scale, config.scale, config.scale);
                        createdSpheres[fid_idx] = fiducialTransform.gameObject;
                    }
                    else
                    {
                        Debug.LogWarning($"Fiducial object with name {fiducialName} not found.");
                    }
                    fid_idx++;
                }

                IRToolController_Instance.spheres = createdSpheres;
            }


        }
    }
}