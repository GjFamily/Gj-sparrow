using Gj.Galaxy.Logic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof (RigidbodyView))]
public class RigidbodyViewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditGUI.ContainerHeader("Options");

        Rect containerRect = EditGUI.ContainerBody(EditorGUIUtility.singleLineHeight*2 + 10);

        Rect propertyRect = new Rect(containerRect.xMin + 5, containerRect.yMin + 5, containerRect.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(propertyRect, serializedObject.FindProperty("m_SynchronizeVelocity"), new GUIContent("Synchronize Velocity"));

        propertyRect.y += EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(propertyRect, serializedObject.FindProperty("m_SynchronizeAngularVelocity"), new GUIContent("Synchronize Angular Velocity"));

        serializedObject.ApplyModifiedProperties();
    }
}