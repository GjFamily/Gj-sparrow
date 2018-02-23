using Gj.Galaxy.Scripts;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(CullMap))]
public class CullMapEditor : Editor
{
    private bool alignEditorCamera, showHelpEntries;

    private CullMap cullMap;
    
    private enum UP_AXIS_OPTIONS
    {
        SideScrollerMode = 0,
        TopDownOr3DMode = 1
    }

    private UP_AXIS_OPTIONS upAxisOptions;

    public void OnEnable()
    {
        cullMap = (CullMap) target;

        // Destroying the newly created cull area if there is already one existing
        if (FindObjectsOfType<CullMap>().Length > 1)
        {
            Debug.LogWarning("Destroying newly created cull area because there is already one existing in the scene.");

            DestroyImmediate(cullMap);

            return;
        }

        // Prevents the dropdown from resetting
        if (cullMap != null)
        {
            upAxisOptions = cullMap.YIsUpAxis ? UP_AXIS_OPTIONS.SideScrollerMode : UP_AXIS_OPTIONS.TopDownOr3DMode;
        }
    }
    
    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginVertical();
        
        if (Application.isEditor && !Application.isPlaying)
        {
            OnInspectorGUIEditMode();
        }
        else
        {
            OnInspectorGUIPlayMode();
        }

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    ///     Represents the inspector GUI when edit mode is active.
    /// </summary>
    private void OnInspectorGUIEditMode()
    {
        EditorGUI.BeginChangeCheck();

        #region DEFINE_UP_AXIS

        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Select game type", EditorStyles.boldLabel);
            upAxisOptions = (UP_AXIS_OPTIONS) EditorGUILayout.EnumPopup("Game type", upAxisOptions);
            cullMap.YIsUpAxis = (upAxisOptions == UP_AXIS_OPTIONS.SideScrollerMode);
            EditorGUILayout.EndVertical();
        }

        #endregion

        EditorGUILayout.Space();

        #region SUBDIVISION

        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Set the number of subdivisions", EditorStyles.boldLabel);
            cullMap.NumberOfSubdivisions = EditorGUILayout.IntSlider("Number of subdivisions", cullMap.NumberOfSubdivisions, 0, CullMap.MAX_NUMBER_OF_SUBDIVISIONS);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            if (cullMap.NumberOfSubdivisions != 0)
            {
                for (int index = 0; index < cullMap.Subdivisions.Length; ++index)
                {
                    if ((index + 1) <= cullMap.NumberOfSubdivisions)
                    {
                        string countMessage = (index + 1) + ". Subdivision: row / column count";

                        EditorGUILayout.BeginVertical();
                        cullMap.Subdivisions[index] = EditorGUILayout.Vector2Field(countMessage, cullMap.Subdivisions[index]);
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.Space();
                    }
                    else
                    {
                        cullMap.Subdivisions[index] = new Vector2(1, 1);
                    }
                }
            }
        }

        #endregion

        EditorGUILayout.Space();

        #region UPDATING_MAIN_CAMERA

        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("View and camera options", EditorStyles.boldLabel);
            alignEditorCamera = EditorGUILayout.Toggle("Automatically align editor view with grid", alignEditorCamera);

            if (Camera.main != null)
            {
                if (GUILayout.Button("Align main camera with grid"))
                {
                    Undo.RecordObject(Camera.main.transform, "Align main camera with grid.");

                    float yCoord = cullMap.YIsUpAxis ? cullMap.Center.y : Mathf.Max(cullMap.Size.x, cullMap.Size.y);
                    float zCoord = cullMap.YIsUpAxis ? -Mathf.Max(cullMap.Size.x, cullMap.Size.y) : cullMap.Center.y;

                    Camera.main.transform.position = new Vector3(cullMap.Center.x, yCoord, zCoord);
                    Camera.main.transform.LookAt(cullMap.transform.position);
                }

                EditorGUILayout.LabelField("Current main camera position is " + Camera.main.transform.position.ToString());
            }

            EditorGUILayout.EndVertical();
        }

        #endregion

        if (EditorGUI.EndChangeCheck())
        {
            cullMap.RecreateCellHierarchy = true;

            AlignEditorView();
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        showHelpEntries = EditorGUILayout.Foldout(showHelpEntries, "Need help with this component?");
        if (showHelpEntries)
        {
            EditorGUILayout.HelpBox("To find help you can either follow the tutorial or have a look at the forums by clicking on the buttons below.", MessageType.Info);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open the tutorial"))
            {
                Application.OpenURL("https://doc.photonengine.com/en-us/pun/current/manuals-and-demos/culling-demo");
            }
            if (GUILayout.Button("Take me to the forums"))
            {
                Application.OpenURL("http://forum.photonengine.com/categories/unity-networking-plugin-pun");
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    /// <summary>
    ///     Represents the inspector GUI when play mode is active.
    /// </summary>
    private void OnInspectorGUIPlayMode()
    {
        EditorGUILayout.LabelField("No changes allowed when game is running. Please exit play mode first.", EditorStyles.boldLabel);
    }

    public void OnSceneGUI()
    {
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(Screen.width - 110, Screen.height - 90, 100, 60));

        if (GUILayout.Button("Reset position"))
        {
            cullMap.transform.position = Vector3.zero;
        }

        if (GUILayout.Button("Reset scaling"))
        {
            cullMap.transform.localScale = new Vector3(25.0f, 25.0f, 25.0f);
        }

        GUILayout.EndArea();
        Handles.EndGUI();

        // Checking for changes of the transform
        if (cullMap.transform.hasChanged)
        {
            // Resetting position
            float posX = cullMap.transform.position.x;
            float posY = cullMap.YIsUpAxis ? cullMap.transform.position.y : 0.0f;
            float posZ = !cullMap.YIsUpAxis ? cullMap.transform.position.z : 0.0f;

            cullMap.transform.position = new Vector3(posX, posY, posZ);

            // Resetting scaling
            if (cullMap.Size.x < 1.0f || cullMap.Size.y < 1.0f)
            {
                float scaleX = (cullMap.transform.localScale.x < 1.0f) ? 1.0f : cullMap.transform.localScale.x;
                float scaleY = (cullMap.transform.localScale.y < 1.0f) ? 1.0f : cullMap.transform.localScale.y;
                float scaleZ = (cullMap.transform.localScale.z < 1.0f) ? 1.0f : cullMap.transform.localScale.z;
                
                cullMap.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);

                Debug.LogWarning("Scaling on a single axis can not be lower than 1. Resetting...");
            }

            cullMap.RecreateCellHierarchy = true;

            AlignEditorView();
        }
    }
    
    /// <summary>
    ///     Aligns the editor view with the created grid.
    /// </summary>
    private void AlignEditorView()
    {
        if (!alignEditorCamera)
        {
            return;
        }

        // This creates a temporary game object in order to align the editor view.
        // The created game object is destroyed afterwards.
        GameObject tmpGo = new GameObject();

        float yCoord = cullMap.YIsUpAxis ? cullMap.Center.y : Mathf.Max(cullMap.Size.x, cullMap.Size.y);
        float zCoord = cullMap.YIsUpAxis ? -Mathf.Max(cullMap.Size.x, cullMap.Size.y) : cullMap.Center.y;

        tmpGo.transform.position = new Vector3(cullMap.Center.x, yCoord, zCoord);
        tmpGo.transform.LookAt(cullMap.transform.position);

        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.AlignViewToObject(tmpGo.transform);
        }

        DestroyImmediate(tmpGo);
    }
}