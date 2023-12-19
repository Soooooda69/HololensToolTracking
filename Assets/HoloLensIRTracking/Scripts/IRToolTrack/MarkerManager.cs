using IRToolTrack;
using System;
using System.IO;
using System.Linq;
//using UnityEditor.Search;
using UnityEngine;

namespace IRToolTrack
{

    public class MarkerManager : MonoBehaviour
    {
        public string Config_Resources_Path;
        public float indicator_y_offset = 0.05f;
        public bool use_indicator;

        void Start()
        {

            TextAsset[] jsonFiles = Resources.LoadAll<TextAsset>(Config_Resources_Path);
            LoadAndCreateMarkers(jsonFiles);
        }

        private void LoadAndCreateMarkers(TextAsset[] jsonFiles)
        {
            int tool_count = jsonFiles.Length;
            Debug.LogWarning($"Count: {jsonFiles.Length}");
            GameObject[] tool_list = new GameObject[tool_count];
            string[] topic_list = new string[tool_count];
            int tool_idx = 0;
            foreach (var jsonFile in jsonFiles)
            {
                int fid_idx = 0;
                float x_mean = 0.0f;
                float y_mean = 0.0f;
                float z_mean = 0.0f;
                string jsonText = jsonFile.text;
                Debug.Log($"File name: {jsonFile.name}");
                MarkerConfig config = JsonUtility.FromJson<MarkerConfig>(jsonText);

                foreach (var fiducial in config.fiducials)
                {
                    x_mean += fiducial.x / config.count;
                    y_mean += fiducial.y / config.count;
                    z_mean += fiducial.z / config.count;
                }
                Debug.Log($"{config.path}");
                GameObject prefab = Resources.Load<GameObject>(config.path);
                if (prefab == null)
                {
                    Debug.LogError("Prefab could not be loaded. Check the path.");
                }
                GameObject markerInstance = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);

                IRToolController IRToolController_Instance = markerInstance.AddComponent<IRToolController>();
                IRToolController_Instance.identifier = config.identifier;

                Transform originalSphere = markerInstance.transform.Find("Sphere0"); 
                GameObject[] createdSpheres = new GameObject[config.count];

                if (originalSphere != null)
                {
                    for (int i = 1; i < config.count; i++)
                    {
                        GameObject duplicatedSphere = Instantiate(originalSphere.gameObject, markerInstance.transform);
                        duplicatedSphere.name = "Sphere" + i; 
                    }
                }

                markerInstance.name = config.identifier;

                Transform pivotTransform = markerInstance.transform.Find("model");
                pivotTransform.localPosition = new Vector3(config.model.x, config.model.y, config.model.z);
                pivotTransform.localRotation = Quaternion.Euler(config.model.rx, config.model.ry, config.model.rz);
                if (use_indicator)
                {
                    GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    indicator.name = "indicator";
                    indicator.transform.SetParent(markerInstance.transform);
                    indicator.transform.localPosition = new Vector3(0.0f, indicator_y_offset, 0.0f);
                    indicator.transform.localScale = new Vector3(config.scale / 2, config.scale / 2, config.scale / 2);
                    // indicator.transform.SetParent(markerInstance.transform, true);
                    Renderer sphereRenderer = indicator.GetComponent<Renderer>();

                    if (sphereRenderer != null)
                    {
                        sphereRenderer.material.color = Color.red;
                    }
                }

                foreach (var fiducial in config.fiducials)
                {
                    string fiducialName = "Sphere" + fid_idx.ToString();
                    Transform fiducialTransform = markerInstance.transform.Find(fiducialName);
                    /*                GameObject fiducialObject = new GameObject("Fiducial");
                                    fiducialObject.transform.parent = markerInstance.transform;
                                    fiducialObject.transform.localPosition = new Vector3(fiducial.x, fiducial.y, fiducial.z);*/

                    if (fiducialTransform != null)
                    {
                        fiducialTransform.localPosition = new Vector3(fiducial.x, fiducial.y, fiducial.z);
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
                tool_list[tool_idx] = markerInstance;
                topic_list[tool_idx] = config.identifier;
                tool_idx++;
            }
            SampleProcess Sample_Process_instance = GameObject.FindObjectOfType<SampleProcess>();
            if (Sample_Process_instance != null)
            {
                Sample_Process_instance.tools = tool_list;
                Sample_Process_instance.topics = topic_list;
            }


        }
    }
}