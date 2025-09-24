using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualBehaviour : MonoBehaviour
{
    Dictionary<string, Material> MaterialsDict = new Dictionary<string, Material>();
    [SerializeField] 
    Material[] materials;
    void Start()
    {
        foreach (Material mat in materials)
        {
            MaterialsDict.Add(mat.name, mat);
        }
    }
    /// <summary>
    /// Get the material name of the object
    /// </summary>
    /// <returns>String of the material name</returns>
    public string GetMaterialName()
    {
        return GetComponent<Renderer>().material.name;
    }
    /// <summary>
    /// Set the material of the object
    /// </summary>
    /// <param name="name">name of the material to change to</param>
    public void SetMaterial(string name)
    {
        try
        {
            GetComponent<Renderer>().material = MaterialsDict[name];
        }
        catch (KeyNotFoundException)
        {
            Debug.LogError("Material not found");
        }
    }
}
