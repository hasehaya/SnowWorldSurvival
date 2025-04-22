using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.SceneManagement;

[ExecuteInEditMode]
public class TreeGenerator : MonoBehaviour
{
    [Range(0f, 3f)]
    public float season = 0f;
    [Range(0f, 3f)]
    public float body = 0;
    [Range(0f, 2f)]
    public float appleSlider = 0f;

    public float rotation;
    public int currentMeshIndex = 0;

    public bool apple = false;
    public bool smooth = false;
    public Material applematerial;
    private bool meshChanged = false;
    private Renderer rendererComponent;

#if UNITY_EDITOR
    public TreeMeshList tml;
#endif

    void OnEnable()
    {
        applematerial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Pack_Trees/Materials/TreeApple.mat");
        rendererComponent = GetComponent<Renderer>();
        UpdateMaterials();
    }

    void Update()
    {
        UpdateMaterials();
    }

    public void UpdateMaterials()
    {
        if (rendererComponent == null)
        {
            rendererComponent = GetComponent<Renderer>();
        }

        var materials = rendererComponent.sharedMaterials.ToList(); // Diziyi listeye dönüştür

        // MeshFilter bileşenini al
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null && tml != null)
        {
            // Smooth toggle değiştirildiğinde ve mesh henüz değiştirilmediyse mesh'i değiştir
            if (smooth && !meshChanged)
            {
                int meshIndex = FindMeshIndex(meshFilter.sharedMesh, tml.flatMeshes, tml.smoothMeshes);
                if (meshIndex != -1)
                {
                    meshFilter.mesh = tml.smoothMeshes[meshIndex];
                    meshChanged = true;
                }
            }
            else if (!smooth && meshChanged)
            {
                int meshIndex = FindMeshIndex(meshFilter.sharedMesh, tml.smoothMeshes, tml.flatMeshes);
                if (meshIndex != -1)
                {
                    meshFilter.mesh = tml.flatMeshes[meshIndex];
                    meshChanged = false;
                }
            }
        }

        // Apple materyalini eklemek veya çıkarmak için materyalleri güncelle
        if (apple)
        {
            if (!materials.Contains(applematerial))
            {
                materials.Add(applematerial);
                rendererComponent.sharedMaterials = materials.ToArray();
            }
        }
        else
        {
            if (materials.Contains(applematerial))
            {
                materials.Remove(applematerial);
                rendererComponent.sharedMaterials = materials.ToArray();
            }
        }

        // Diğer materyalleri güncelle
        if (materials.Count > 0 && materials[0] != null)
            materials[0].mainTextureOffset = new Vector2((body - .375f) * 0.334f, 0);

        if (materials.Count > 1 && materials[1] != null)
            materials[1].mainTextureOffset = new Vector2(season / 3.2f, 0);

        if (apple && materials.Count > 2 && materials[2] != null)
            materials[2].mainTextureOffset = new Vector2(appleSlider * 0.33f, 0);
    }

    private int FindMeshIndex(Mesh currentMesh, Mesh[] arrayA, Mesh[] arrayB)
    {
        for (int i = 0; i < arrayA.Length; i++)
        {
            if (arrayA[i] == currentMesh)
            {
                return i; // Eğer mevcut mesh arrayA'daysa, arrayB'deki karşılığını döndür
            }
        }
        for (int i = 0; i < arrayB.Length; i++)
        {
            if (arrayB[i] == currentMesh)
            {
                return i; // Eğer mevcut mesh arrayB'daysa, arrayA'deki karşılığını döndür
            }
        }
        return -1; // Eğer mesh hiçbir dizide bulunamazsa -1 döndür
    }
}