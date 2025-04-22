using UnityEngine;

[CreateAssetMenu(fileName = "NewTreeMeshList", menuName = "Tree Generator/Mesh List", order = 0)]
public class TreeMeshList : ScriptableObject
{
    public Mesh[] flatMeshes;
    public Mesh[] smoothMeshes;
}