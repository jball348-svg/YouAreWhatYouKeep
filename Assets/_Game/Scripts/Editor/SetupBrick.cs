using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SetupBrick
{
    [MenuItem("Tools/YAWYK/Setup And Assign Brick Material")]
public static void Run()
    {
        // 1 — Mark normal map correctly
        string normalPath = "Assets/_Game/Art/Environment/Textures/Brick/Bricks059_1K-JPG_NormalGL.jpg";
        TextureImporter ni = AssetImporter.GetAtPath(normalPath) as TextureImporter;
        if (ni != null) { ni.textureType = TextureImporterType.NormalMap; ni.SaveAndReimport(); }

        // 2 — Load maps
        Texture2D col = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Art/Environment/Textures/Brick/Bricks059_1K-JPG_Color.jpg");
        Texture2D nrm = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
        Texture2D rgh = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Art/Environment/Textures/Brick/Bricks059_1K-JPG_Roughness.jpg");
        Texture2D ao  = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Art/Environment/Textures/Brick/Bricks059_1K-JPG_AmbientOcclusion.jpg");

        if (col == null) { Debug.LogError("[Brick] Color map not found"); return; }

        // 3 — Create material (overwrite if exists)
        if (!AssetDatabase.IsValidFolder("Assets/_Game/Materials"))
            AssetDatabase.CreateFolder("Assets/_Game", "Materials");

        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetTexture("_BaseMap", col);
        if (nrm != null) { mat.SetTexture("_BumpMap", nrm); mat.EnableKeyword("_NORMALMAP"); }
        if (ao  != null) { mat.SetTexture("_OcclusionMap", ao); mat.SetFloat("_OcclusionStrength", 0.8f); }
        mat.SetFloat("_Smoothness", 0.1f);
        mat.SetFloat("_Cull", 0f);
        mat.SetTextureScale("_BaseMap", new Vector2(2f, 2f));
        mat.SetTextureScale("_BumpMap", new Vector2(2f, 2f));

        string matPath = "Assets/_Game/Materials/MAT_BrickExterior.mat";
        // Delete old one if it exists so we get a clean asset
        AssetDatabase.DeleteAsset(matPath);
        AssetDatabase.CreateAsset(mat, matPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

        // 4 — Assign to all renderers in the three house groups
        int total = 0;
        foreach (string groupName in new[] { "HOUSE_downstairs", "HOUSE_Upstairs", "HOUSE_Roof" })
        {
            GameObject go = GameObject.Find(groupName);
            if (go == null) { Debug.LogWarning("[Brick] Not found: " + groupName); continue; }
            foreach (MeshRenderer r in go.GetComponentsInChildren<MeshRenderer>())
            {
                r.sharedMaterial = mat;
                total++;
            }
            Debug.Log("[Brick] Assigned to " + groupName);
        }

        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("[Brick] Done — " + total + " renderers. Stopping here.");
    }
}
