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
        EditorApplication.hierarchyWindowChanged += HierarchyChange;
    }

    // this method corrects the IDs for photonviews in the scene and in prefabs
    // make sure prefabs always use viewID 0
    // make sure instances never use a owner
    // this is a editor class that should only run if not playing
    internal static void HierarchyChange()
    {
        if (Application.isPlaying)
        {
            //Debug.Log("HierarchyChange ignored, while running.");
            CheckSceneForStuckHandlers = true;  // done once AFTER play mode.
            return;
        }

        if (CheckSceneForStuckHandlers)
        {
            CheckSceneForStuckHandlers = false;
        }
        PeerClient.Close();


        HashSet<NetworkEntity> pvInstances = new HashSet<NetworkEntity>();
        HashSet<int> usedInstanceViewNumbers = new HashSet<int>();
        bool fixedSomeId = false;

        //// the following code would be an option if we only checked scene objects (but we can check all PVs)
        //PhotonView[] pvObjects = GameObject.FindSceneObjectsOfType(typeof(PhotonView)) as PhotonView[];
        //Debug.Log("HierarchyChange. PV Count: " + pvObjects.Length);

        string levelName = SceneManagerHelper.ActiveSceneName;
        #if UNITY_EDITOR
        levelName = SceneManagerHelper.EditorActiveSceneName;
        #endif
        //int minViewIdInThisScene = GameConnect.S;
        //Debug.Log("Level '" + Application.loadedLevelName + "' has a minimum ViewId of: " + minViewIdInThisScene);

        NetworkEntity[] pvObjects = Resources.FindObjectsOfTypeAll(typeof(NetworkEntity)) as NetworkEntity[];

        foreach (NetworkEntity entity in pvObjects)
        {
            // first pass: fix prefabs to viewID 0 if they got a view number assigned (cause they should not have one!)
            if (EditorUtility.IsPersistent(entity.gameObject))
            {
                if (entity.entityId != 0 || entity.prefixBackup != -1 || entity.instantiationId != -1)
                {
                    Debug.LogWarning("NetworkEntity on persistent object being fixed (id and prefix must be 0). Was: " + entity);
                    entity.entityId = 0;
                    entity.prefixBackup = -1;
                    entity.instantiationId = -1;
                    EditorUtility.SetDirty(entity);   // even in Unity 5.3+ it's OK to SetDirty() for non-scene objects. 
                    fixedSomeId = true;
                }
            }
            else
            {
                // keep all scene-instanced PVs for later re-check
                pvInstances.Add(entity);
            }
        }

        Dictionary<GameObject, int> idPerObject = new Dictionary<GameObject, int>();

        // second pass: check all used-in-scene viewIDs for duplicate viewIDs (only checking anything non-prefab)
        // scene-PVs must have user == 0 (scene/room) and a subId != 0
        foreach (NetworkEntity entity in pvInstances)
        {
            if (entity.ownerId > 0)
            {
                Debug.Log("Re-Setting Owner ID of: " + entity);
            }
            entity.ownerId = 0;   // simply make sure no owner is set (cause room always uses 0)
            entity.prefix = -1;   // TODO: prefix could be settable via inspector per scene?!

            if (entity.entityId != 0)
            {
                if (usedInstanceViewNumbers.Contains(entity.entityId))
                {
                    entity.entityId = 0; // avoid duplicates and negative values by assigning 0 as (temporary) number to be fixed in next pass
                }
                else
                {
                    usedInstanceViewNumbers.Add(entity.entityId); // builds a list of currently used viewIDs

                    int instId = 0;
                    if (idPerObject.TryGetValue(entity.gameObject, out instId))
                    {
                        entity.instantiationId = instId;
                    }
                    else
                    {
                        entity.instantiationId = entity.entityId;
                        idPerObject[entity.gameObject] = entity.instantiationId;
                    }
                }
            }

        }

        // third pass: anything that's now 0 must get a new (not yet used) ID (starting at 0)
        int lastUsedId = 0;

        foreach (NetworkEntity entity in pvInstances)
        {
            if (entity.entityId == 0)
            {
                Undo.RecordObject(entity, "Automatic entityId change for: "+entity.gameObject.name);

                int nextViewId = EntityHandler.GetID(lastUsedId, usedInstanceViewNumbers);

                entity.entityId = nextViewId;

                int instId = 0;
                if (idPerObject.TryGetValue(entity.gameObject, out instId))
                {
                    entity.instantiationId = instId;
                }
                else
                {
                    entity.instantiationId = entity.entityId;
                    idPerObject[entity.gameObject] = nextViewId;
                }

                lastUsedId = nextViewId;
                fixedSomeId = true;

                #if !UNITY_MIN_5_3
                EditorUtility.SetDirty(view);
                #endif
            }
        }


        if (fixedSomeId)
        {
            //Debug.LogWarning("Some subId was adjusted."); // this log is only interesting for Exit Games
        }
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
            EntityHandler.HierarchyChange();//NOTE: most likely on load also triggers a hierarchy change
            EditorSceneManager.SaveOpenScenes();
        }

        Debug.Log("Corrected scene views where needed.");
    }
}
