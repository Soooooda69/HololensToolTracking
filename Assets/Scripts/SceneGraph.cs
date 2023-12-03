using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

class SceneGraph
{
    public static Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform found = FindDeepChild(child, childName);
            if (found != null)
                return found;
        }
        return null;
    }
}
