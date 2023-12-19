using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MarkerConfig
{
    public string identifier;
    public string path;
    public int count;
    public float scale;
    public Fiducial[] fiducials;
    public Model model;
}

[Serializable]
public class Fiducial
{
    public float x;
    public float y;
    public float z;
}

[Serializable]
public class Model
{
    public float x;
    public float y;
    public float z;
    public float rx;
    public float ry;
    public float rz;
}
