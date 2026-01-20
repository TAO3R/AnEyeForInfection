using UnityEngine;

[CreateAssetMenu(fileName = "MaterialLibrary", menuName = "Scriptable Objects/MaterialLibrary")]
public class MaterialLibrary : ScriptableObject
{
    [SerializeField] private Material[] materials;

    public Material[] Materials => materials;
}
