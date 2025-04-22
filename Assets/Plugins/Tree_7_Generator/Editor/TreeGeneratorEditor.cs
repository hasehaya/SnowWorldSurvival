using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.SceneManagement;


[CustomEditor(typeof(TreeGenerator))]
public class TreeGeneratorEditor : Editor
{
    Texture2D bodyTexture;
    Texture2D seasonTexture;
    Texture2D appleTexture;

    void OnEnable()
    {
        // Specify the paths of the colored images
        bodyTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Pack_Trees/Materials/Textures/treeBodyEditor.png");
        seasonTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Pack_Trees/Materials/Textures/treeSeason.png");
        appleTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Pack_Trees/Materials/Textures/treeSeason.png");

        // Set the TreeMeshList reference
#if UNITY_EDITOR
        TreeGenerator treeGenerator = (TreeGenerator)target;
        if (treeGenerator.tml == null)
        {
            treeGenerator.tml = AssetDatabase.LoadAssetAtPath<TreeMeshList>("Assets/Pack_Trees/Resources/TreeMeshList.asset");
        }
        // AssetDatabase codes go here
#endif
    }


    public override void OnInspectorGUI()
    {
        TreeGenerator script = (TreeGenerator)target;

        // GUIStyle set for centered text
        GUIStyle centeredStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter
        };

        // Tree Color
        EditorGUILayout.LabelField("Tree Color:", centeredStyle);
        DrawColorSlider(ref script.season, seasonTexture);

        // Body Color
        float bodyFloat = script.body; // Convert int value to float
        EditorGUILayout.LabelField("Body Color:", centeredStyle);
        DrawColorSlider(ref bodyFloat, bodyTexture);
        script.body = (.375f + (Mathf.RoundToInt(bodyFloat) * .75F)); // Convert back from float to int

        // Apple Toggle and Apple Color
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace(); // Leaves flexible space on the left
        EditorGUILayout.LabelField("Apple", centeredStyle, GUILayout.Width(60)); // Set label width
        script.apple = EditorGUILayout.Toggle(script.apple, GUILayout.Width(15)); // Set toggle width
        GUILayout.FlexibleSpace(); // Leaves flexible space on the right
        EditorGUILayout.EndHorizontal();

        if (script.apple)
        {
            EditorGUILayout.LabelField("Apple Color:", centeredStyle);
            DrawColorSlider(ref script.appleSlider, appleTexture);
        }


        // Smooth Toggle
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace(); // Leaves flexible space on the left
        EditorGUILayout.LabelField("Smooth", centeredStyle, GUILayout.Width(60)); // Set label width
        script.smooth = EditorGUILayout.Toggle(script.smooth, GUILayout.Width(15)); // Set toggle width
        GUILayout.FlexibleSpace(); // Leaves flexible space on the right
        EditorGUILayout.EndHorizontal();

        #region Line
        // Draw a line under the Smooth Toggle
        EditorGUILayout.Space();
        Rect rect2 = EditorGUILayout.GetControlRect(false, 1);
        rect2.height = 1;
        EditorGUI.DrawRect(rect2, Color.gray);
        EditorGUILayout.Space();
        #endregion

        // Rotation slider
        EditorGUILayout.LabelField("Rotation:", centeredStyle); // Centers the "Rotation:" title above the slider
        script.rotation = EditorGUILayout.Slider(script.rotation, 0f, 360f); // Adds the slider (from 0 to 360)
        if (GUI.changed)
        {
            // If the slider is changed, update the object's rotation
            script.transform.eulerAngles = new Vector3(script.transform.eulerAngles.x, script.rotation, script.transform.eulerAngles.z);
        }

        /*--------------------------------------------------------------------------*/

        // Change Tree: Label
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Change Tree:", centeredStyle);

        // GUI code for direction buttons
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace(); // Center the buttons

        // Left button
        GUI.enabled = script.currentMeshIndex > 0;
        if (GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(30)))
        {
            script.currentMeshIndex--;
            UpdateMesh(script);
        }

        // Right button
        GUI.enabled = script.currentMeshIndex < script.tml.flatMeshes.Length - 1;
        if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(30)))
        {
            script.currentMeshIndex++;
            UpdateMesh(script);
        }

        GUILayout.FlexibleSpace(); // Center the buttons
        EditorGUILayout.EndHorizontal();
        GUI.enabled = true; // Reset button enablement

        #region Line
        // Draw a line under the Smooth Toggle
        EditorGUILayout.Space();
        Rect rect3 = EditorGUILayout.GetControlRect(false, 1);
        rect2.height = 1;
        EditorGUI.DrawRect(rect3, Color.gray);
        EditorGUILayout.Space();
        #endregion

        // Add Generate Tree button here
        EditorGUILayout.Space(); // Space between buttons
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace(); // Center the buttons
        GUI.enabled = script.currentMeshIndex >= 0 && script.currentMeshIndex < script.tml.flatMeshes.Length; // Enable Generate Tree button
        if (GUILayout.Button("Generate Tree", GUILayout.MinWidth(100), GUILayout.Height(30)))
        {
            GenerateTreePrefab(script);
        }
        GUILayout.FlexibleSpace(); // Center the buttons
        EditorGUILayout.EndHorizontal();
        GUI.enabled = true; // Reset button enablement

        // Save changes
        if (GUI.changed)
        {
            EditorUtility.SetDirty(script.gameObject);
        }
    }


    private void DrawColorSlider(ref float value, Texture2D texture)
    {
        // Texture and Slider
        Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(20), GUILayout.ExpandWidth(true));
        GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill);
        value = GUI.HorizontalSlider(rect, value, 0f, 3f, GUIStyle.none, GUI.skin.horizontalSliderThumb);
    }

    private void UpdateMesh(TreeGenerator script)
    {
        // Get the MeshFilter component and assign the new mesh
        MeshFilter meshFilter = script.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            meshFilter.sharedMesh = script.smooth ? script.tml.smoothMeshes[script.currentMeshIndex] : script.tml.flatMeshes[script.currentMeshIndex];
            // Update the scene after changing the mesh
            EditorUtility.SetDirty(meshFilter.gameObject);
        }
    }

    private void GenerateTreePrefab(TreeGenerator script)
    {
        // Create a clone of the GameObject before saving
        GameObject clone = Instantiate(script.gameObject);
        MeshFilter cloneMeshFilter = clone.GetComponent<MeshFilter>();
        clone.name = script.gameObject.GetComponent<MeshFilter>().sharedMesh.name; // Name the clone the same as the original object

        // Create and save new materials for the clone
        string materialFolderPath = "Assets/Pack_Trees/Materials/TreeMaterials";
        if (!AssetDatabase.IsValidFolder(materialFolderPath))
        {
            AssetDatabase.CreateFolder("Assets/Pack_Trees/Materials", "TreeMaterials");
        }
        Renderer cloneRenderer = clone.GetComponent<Renderer>();
        Material[] newMaterials = new Material[cloneRenderer.sharedMaterials.Length];
        for (int i = 0; i < newMaterials.Length; i++)
        {
            Material newMaterial = new Material(cloneRenderer.sharedMaterials[i]);
            string materialName = script.gameObject.name + "_Mat_" + (i + 1).ToString();
            string materialPath = AssetDatabase.GenerateUniqueAssetPath(System.IO.Path.Combine(materialFolderPath, materialName + ".mat"));
            AssetDatabase.CreateAsset(newMaterial, materialPath);
            newMaterials[i] = newMaterial;
        }
        cloneRenderer.sharedMaterials = newMaterials;

        // Reset the clone's transform
        clone.transform.position = Vector3.zero;
        clone.transform.rotation = new Quaternion(0, 0, 0, 0);
        clone.transform.localScale = Vector3.one;

        // Remove the TreeGenerator component from the clone
        TreeGenerator cloneTreeGenerator = clone.GetComponent<TreeGenerator>();
        if (cloneTreeGenerator != null)
        {
            DestroyImmediate(cloneTreeGenerator, true);
        }

        // Save the prefab to the specified path and destroy the clone
        string localPath = AssetDatabase.GenerateUniqueAssetPath("Assets/Pack_Trees/Prefabs/" + clone.name + ".prefab");
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(clone, localPath);

        // Delete the clone from the scene, we're done with it
        DestroyImmediate(clone);

        // Refresh the AssetDatabase
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
