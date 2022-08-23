#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpellSO))]
public class SpellSOEditor : Editor
{


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(GUILayout.Button("Assign Inv Code"))
        {
            var obj = target as SpellSO;
            obj.CreateInvCode();
            SerializedObject so = new SerializedObject(obj);
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets(); 
        }
    }
}

[CustomEditor(typeof(UnitSO))]
public class UnitSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(GUILayout.Button("Assign Inv Code"))
        {
            var obj = target as UnitSO;
            obj.CreateInvCode();
            SerializedObject so = new SerializedObject(obj);
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
}

#endif