using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossSectionController : MonoBehaviour
{
    public Material crossSectionMaterial; // Assign your material here
    public Transform planeTransform;      // A transform representing the plane

    void Update()
    {
        if (crossSectionMaterial && planeTransform)
        {
            // Pass the plane's position and normal to the shader
            crossSectionMaterial.SetVector("_PlanePosition", planeTransform.position);
            crossSectionMaterial.SetVector("_PlaneNormal", planeTransform.up); // Normal is the transform's "up" vector
        }
    }
}
//need to change plane pos Y in material to 0 once resetted