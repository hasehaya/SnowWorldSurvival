#if UNITY_EDITOR
using UnityEditor;

#endif
using UnityEngine;

#if UNITY_EDITOR
public class GridGenerator :MonoBehaviour
{
    [SerializeField] private string childBaseName = "Child";
    [SerializeField] private int rows = 1;
    [SerializeField] private int columns = 1;
    [SerializeField] private Vector3 spacing = Vector3.one;

    public void GenerateGrid()
    {
        var parent = gameObject;
        if (parent.transform.childCount == 0)
        {
            Debug.LogWarning("No child exists.");
            return;
        }

        var firstChild = parent.transform.GetChild(0);

        for (int i = parent.transform.childCount - 1; i > 0; i--)
        {
            DestroyImmediate(parent.transform.GetChild(i).gameObject);
        }

        firstChild.name = $"{childBaseName}_1";
        firstChild.localPosition = Vector3.zero;
        firstChild.localRotation = Quaternion.identity;

        int index = 1;

        var sourcePrefab = PrefabUtility.GetCorrespondingObjectFromSource(firstChild.gameObject);
        if (sourcePrefab != null)
        {
            var sourceTree = sourcePrefab.GetComponent<MaterialProducer>();
            if (sourceTree != null)
            {
                sourceTree.Row = 1;
                sourceTree.Column = 1;
            }
        }

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (row == 0 && col == 0)
                    continue;
                index++;
                GameObject clone;
                if (sourcePrefab != null)
                {
                    clone = (GameObject)PrefabUtility.InstantiatePrefab(sourcePrefab, parent.transform);
                    var tree = clone.GetComponent<MaterialProducer>();
                    if (tree != null)
                    {
                        tree.Row = row + 1;
                        tree.Column = col + 1;
                    }
                }
                else
                {
                    clone = Instantiate(firstChild.gameObject, parent.transform);
                }

                clone.name = $"{childBaseName}_{index}";
                // 例: X=col, Y=row, Z=row で配置
                clone.transform.localPosition = new Vector3(
                    col * spacing.x,
                    0,
                    row * spacing.z
                );
                clone.transform.localRotation = Quaternion.identity;
            }
        }

        Debug.Log($"Generated {rows * columns} objects in a {rows}x{columns} grid.");
    }
}


[CustomEditor(typeof(GridGenerator))]
public class GridGeneratorEditor :Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GridGenerator script = (GridGenerator)target;
        if (GUILayout.Button("Generate Grid"))
        {
            script.GenerateGrid();
        }
    }
}
#endif
