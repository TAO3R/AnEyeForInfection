using UnityEngine;

[System.Serializable]
public class MaterialSelectorDropdown
{
    public MaterialLibrary matLib;
    
    [HideInInspector] public int index;
    
    [HideInInspector] public Material selectedMat;
    
    // public void OnValidate()
    // {
    //     if (matLib == null || index <= 0 || index >= matLib.Materials.Length)
    //     {
    //         selectedMat = null;
    //         return;
    //     }
    //
    //     selectedMat = matLib.Materials[index];
    // }
    
}   // End of class
