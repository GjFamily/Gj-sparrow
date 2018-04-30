#if UNITY_5 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2 || UNITY_5_4_OR_NEWER
#define UNITY_MIN_5_3
#endif

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections;
using Debug = UnityEngine.Debug;
using UnityEditor.SceneManagement;
using Gj.Galaxy.Logic;

[InitializeOnLoad]
public class EntityHandler : EditorWindow
{
    private static bool CheckSceneForStuckHandlers = true;

    static EntityHandler()
    {
        // hierarchyWindowChanged is called on hierarchy changed and on save. It's even called when hierarchy-window is closed and if a prefab with instances is changed.
        // this is not called when you edit a instance's value but: on save
    }


    // TODO fail if no ID was available anymore
    // TODO look up lower numbers if offset hits max?!
    public static int GetID(int idOffset, HashSet<int> usedInstanceViewNumbers)
    {
        while (true)
        {
            idOffset++;
            if (!usedInstanceViewNumbers.Contains(idOffset))
            {
                break;
            }
        }

        return idOffset;
    }

    //TODO: check if this can be internal protected (as source in editor AND as dll)
    public static void LoadAllScenesToFix()
    {
        string[] scenes = System.IO.Directory.GetFiles(".", "*.unity", SearchOption.AllDirectories);

        foreach (string scene in scenes)
        {
            EditorSceneManager.OpenScene(scene);
            EditorSceneManager.SaveOpenScenes();
        }

        Debug.Log("Corrected scene views where needed.");
    }
}
