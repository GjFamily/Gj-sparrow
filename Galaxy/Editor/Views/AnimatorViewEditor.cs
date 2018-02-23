#if UNITY_5 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2 || UNITY_5_4_OR_NEWER
#define UNITY_MIN_5_3
#endif


using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Animations;
using Gj.Galaxy.Logic;
using Gj.Galaxy.Scripts;

[CustomEditor(typeof (AnimatorView))]
public class AnimatorViewEditor : Editor
{
    private Animator m_Animator;
    private AnimatorView m_Target;

	private AnimatorController m_Controller;

    public override void OnInspectorGUI()
    {
        if (this.m_Animator == null)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("GameObject doesn't have an Animator component to synchronize");
            GUILayout.EndVertical();
            return;
        }

        DrawWeightInspector();
       
		if (GetLayerCount() == 0)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Animator doesn't have any layers setup to synchronize");
            GUILayout.EndVertical();
        }

        DrawParameterInspector();

        if (GetParameterCount() == 0)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Animator doesn't have any parameters setup to synchronize");
            GUILayout.EndVertical();
        }

        serializedObject.ApplyModifiedProperties();

    }

	 
    private int GetLayerCount()
    {
		return (this.m_Controller == null) ? 0 : this.m_Controller.layers.Length;
    }

    private RuntimeAnimatorController GetEffectiveController(Animator animator)
    {
        RuntimeAnimatorController controller = animator.runtimeAnimatorController;

        AnimatorOverrideController overrideController = controller as AnimatorOverrideController;
        while (overrideController != null)
        {
            controller = overrideController.runtimeAnimatorController;
            overrideController = controller as AnimatorOverrideController;
        }

        return controller;
    }


    private void OnEnable()
    {
        this.m_Target = (AnimatorView) target;
        this.m_Animator = this.m_Target.GetComponent<Animator>();

        this.m_Controller = this.GetEffectiveController(this.m_Animator) as AnimatorController;

        CheckIfStoredParametersExist();
    }

    private void DrawWeightInspector()
    {
        SerializedProperty foldoutProperty = serializedObject.FindProperty("ShowLayerWeightsInspector");
        foldoutProperty.boolValue = EditGUI.ContainerHeaderFoldout("Synchronize Layer Weights", foldoutProperty.boolValue);

        if (foldoutProperty.boolValue == false)
        {
            return;
        }

        float lineHeight = 20;
        Rect containerRect = EditGUI.ContainerBody(this.GetLayerCount()*lineHeight);

        for (int i = 0; i < this.GetLayerCount(); ++i)
        {
            if (this.m_Target.DoesLayerSynchronizeTypeExist(i) == false)
            {
                this.m_Target.SetLayerSynchronized(i, AnimatorView.SynchronizeType.Disabled);

                #if !UNITY_MIN_5_3
                EditorUtility.SetDirty(this.m_Target);
                #endif
            }

            AnimatorView.SynchronizeType syncType = this.m_Target.GetLayerSynchronizeType(i);

            Rect elementRect = new Rect(containerRect.xMin, containerRect.yMin + i*lineHeight, containerRect.width, lineHeight);

            Rect labelRect = new Rect(elementRect.xMin + 5, elementRect.yMin + 2, EditorGUIUtility.labelWidth - 5, elementRect.height);
            GUI.Label(labelRect, "Layer " + i);

            Rect popupRect = new Rect(elementRect.xMin + EditorGUIUtility.labelWidth, elementRect.yMin + 2, elementRect.width - EditorGUIUtility.labelWidth - 5, EditorGUIUtility.singleLineHeight);
            syncType = (AnimatorView.SynchronizeType) EditorGUI.EnumPopup(popupRect, syncType);

            if (i < this.GetLayerCount() - 1)
            {
                Rect splitterRect = new Rect(elementRect.xMin + 2, elementRect.yMax, elementRect.width - 4, 1);
                EditGUI.DrawSplitter(splitterRect);
            }

            if (syncType != this.m_Target.GetLayerSynchronizeType(i))
            {
                Undo.RecordObject(target, "Modify Synchronize Layer Weights");
                this.m_Target.SetLayerSynchronized(i, syncType);

                #if !UNITY_MIN_5_3
                EditorUtility.SetDirty(this.m_Target);
                #endif
            }
        }
    }

    private int GetParameterCount()
    {
        return (this.m_Controller == null) ? 0 : this.m_Controller.parameters.Length;
    }

    private AnimatorControllerParameter GetAnimatorControllerParameter(int i)
    {
        return this.m_Controller.parameters[i];
    }

    private bool DoesParameterExist(string name)
    {
        for (int i = 0; i < this.GetParameterCount(); ++i)
        {
            if (GetAnimatorControllerParameter(i).name == name)
            {
                return true;
            }
        }

        return false;
    }

    private void CheckIfStoredParametersExist()
    {
        var syncedParams = this.m_Target.GetSynchronizedParameters();
        List<string> paramsToRemove = new List<string>();

        for (int i = 0; i < syncedParams.Count; ++i)
        {
            string parameterName = syncedParams[i].Name;
            if (DoesParameterExist(parameterName) == false)
            {
                Debug.LogWarning("Parameter '" + this.m_Target.GetSynchronizedParameters()[i].Name + "' doesn't exist anymore. Removing it from the list of synchronized parameters");
                paramsToRemove.Add(parameterName);
            }
        }
        if (paramsToRemove.Count > 0)
        {
            foreach (string param in paramsToRemove)
            {
                this.m_Target.GetSynchronizedParameters().RemoveAll(item => item.Name == param);
            }

            #if !UNITY_MIN_5_3
            EditorUtility.SetDirty(this.m_Target);
            #endif
        }
    }
	

    private void DrawParameterInspector()
    {
		// flag to expose a note in Interface if one or more trigger(s) are synchronized
		bool isUsingTriggers = false;

        SerializedProperty foldoutProperty = serializedObject.FindProperty("ShowParameterInspector");
        foldoutProperty.boolValue = EditGUI.ContainerHeaderFoldout("Synchronize Parameters", foldoutProperty.boolValue);

        if (foldoutProperty.boolValue == false)
        {
            return;
        }

        float lineHeight = 20;
        Rect containerRect = EditGUI.ContainerBody(GetParameterCount()*lineHeight);

        for (int i = 0; i < GetParameterCount(); i++)
        {
            AnimatorControllerParameter parameter = null;
            parameter = GetAnimatorControllerParameter(i);

            string defaultValue = "";

            if (parameter.type == AnimatorControllerParameterType.Bool)
            {
				if (Application.isPlaying && m_Animator.gameObject.activeInHierarchy)
				{
					defaultValue += m_Animator.GetBool(parameter.name);
				}else{
                	defaultValue += parameter.defaultBool.ToString();
				}
            }
            else if (parameter.type == AnimatorControllerParameterType.Float)
            {
				if (Application.isPlaying && m_Animator.gameObject.activeInHierarchy)
				{
					defaultValue += m_Animator.GetFloat(parameter.name).ToString("0.00");
				}else{
               	 defaultValue += parameter.defaultFloat.ToString();
				}
            }
            else if (parameter.type == AnimatorControllerParameterType.Int)
            {
				if (Application.isPlaying && m_Animator.gameObject.activeInHierarchy)
				{
					defaultValue += m_Animator.GetInteger(parameter.name);
				}else{
                	defaultValue += parameter.defaultInt.ToString();
				}
            }
			else if (parameter.type == AnimatorControllerParameterType.Trigger)
			{
				if (Application.isPlaying && m_Animator.gameObject.activeInHierarchy)
				{
					defaultValue += m_Animator.GetBool(parameter.name);
				}else{
					defaultValue += parameter.defaultBool.ToString();
				}
			}

            if (this.m_Target.DoesParameterSynchronizeTypeExist(parameter.name) == false)
            {
                this.m_Target.SetParameterSynchronized(parameter.name, (AnimatorView.ParameterType) parameter.type, AnimatorView.SynchronizeType.Disabled);

                #if !UNITY_MIN_5_3
                EditorUtility.SetDirty(this.m_Target);
                #endif
            }

            AnimatorView.SynchronizeType value = this.m_Target.GetParameterSynchronizeType(parameter.name);

			// check if using trigger and actually synchronizing it
			if (value!=AnimatorView.SynchronizeType.Disabled &&parameter.type == AnimatorControllerParameterType.Trigger)
			{
				isUsingTriggers = true;
			}

            Rect elementRect = new Rect(containerRect.xMin, containerRect.yMin + i*lineHeight, containerRect.width, lineHeight);

            Rect labelRect = new Rect(elementRect.xMin + 5, elementRect.yMin + 2, EditorGUIUtility.labelWidth - 5, elementRect.height);
            GUI.Label(labelRect, parameter.name + " (" + defaultValue + ")");

            Rect popupRect = new Rect(elementRect.xMin + EditorGUIUtility.labelWidth, elementRect.yMin + 2, elementRect.width - EditorGUIUtility.labelWidth - 5, EditorGUIUtility.singleLineHeight);
            value = (AnimatorView.SynchronizeType) EditorGUI.EnumPopup(popupRect, value);

            if (i < GetParameterCount() - 1)
            {
                Rect splitterRect = new Rect(elementRect.xMin + 2, elementRect.yMax, elementRect.width - 4, 1);
                EditGUI.DrawSplitter(splitterRect);
            }



            if (value != this.m_Target.GetParameterSynchronizeType(parameter.name))
            {
                Undo.RecordObject(target, "Modify Synchronize Parameter " + parameter.name);
                this.m_Target.SetParameterSynchronized(parameter.name, (AnimatorView.ParameterType) parameter.type, value);

                #if !UNITY_MIN_5_3
                EditorUtility.SetDirty(this.m_Target);
                #endif
            }
        }

		// display note when synchronized triggers are detected.
		if (isUsingTriggers)
		{
			GUILayout.BeginHorizontal(GUI.skin.box);
			GUILayout.Label("When using triggers, make sure this component is last in the stack");
			GUILayout.EndHorizontal();
		}

    }
}