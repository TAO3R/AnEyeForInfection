using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MaterialSelectorDropdown))]
public class MaterialSelectorDropdownEditor : PropertyDrawer
{
    private static readonly float lineHeight = EditorGUIUtility.singleLineHeight;
    const float verticalSpacing = 2f;
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var libraryProp = property.FindPropertyRelative("matLib");
        var selectedIndexProp = property.FindPropertyRelative("index");
        var selectedMaterialProp = property.FindPropertyRelative("selectedMat");

        if (libraryProp == null)
        {
            EditorGUI.LabelField(position, "Property setup error (check field names).");
            EditorGUI.EndProperty();
            return;
        }

        // Draw Material Library field
        Rect lineRect = new Rect(position.x, position.y, position.width, lineHeight);
        EditorGUI.PropertyField(lineRect, libraryProp);

        // Draw popup if library assigned
        var library = libraryProp.objectReferenceValue as MaterialLibrary;
        if (library != null && library.Materials != null && library.Materials.Length > 0)
        {
            string[] names = new string[library.Materials.Length];
            for (int i = 0; i < names.Length; i++)
                names[i] = library.Materials[i] != null ? library.Materials[i].name : "(Missing)";

            int newIndex = EditorGUI.Popup(position, "Material", selectedIndexProp.intValue, names);
            if (newIndex != selectedIndexProp.intValue)
            {
                selectedIndexProp.intValue = newIndex;
                selectedMaterialProp.objectReferenceValue = library.Materials[newIndex];
            }
        }

        EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float totalHeight = lineHeight + verticalSpacing; // always one line for library

        var libraryProp = property.FindPropertyRelative("materialLibrary");
        var library = libraryProp?.objectReferenceValue as MaterialLibrary;
        if (library != null && library.Materials != null && library.Materials.Length > 0)
        {
            totalHeight += lineHeight + verticalSpacing; // extra line for popup
        }

        return totalHeight;
    }
}   // End of class
