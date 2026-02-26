// BrickMaterialSetup.cs
// Editor utility — sets up the brick texture imports and creates
// a correctly configured URP/Lit material with all maps wired up.
// Run once via Tools → Setup Brick Material, then delete this script.

using UnityEngine;
using UnityEditor;

public class BrickMaterialSetup : MonoBehaviour
{
    [MenuItem("Tools/Setup Brick Material")]
    public static void SetupBrickMaterial()
    {
        // -------------------------------------------------------
        // STEP 1 — Mark the normal map correctly
        // -------------------------------------------------------
        string normalPath = "Assets/_Game/Art/Environment/Textures/Brick/Bricks059_8K-PNG_NormalGL.png";
        TextureImporter normalImporter = AssetImporter.GetAtPath(normalPath) as TextureImporter;
        if (normalImporter != null)
        {
            normalImporter.textureType = TextureImporterType.NormalMap;
            normalImporter.SaveAndReimport();
            Debug.Log("[BrickSetup] Normal map type set correctly");
        }
        else
        {
            Debug.LogWarning("[BrickSetup] Could not find normal map at: " + normalPath);
        }

        // -------------------------------------------------------
        // STEP 2 — Load all texture maps
        // -------------------------------------------------------
        Texture2D colorMap = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/_Game/Art/Environment/Textures/Brick/Bricks059_8K-PNG_Color.png");
        Texture2D normalMap = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/_Game/Art/Environment/Textures/Brick/Bricks059_8K-PNG_NormalGL.png");
        Texture2D roughnessMap = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/_Game/Art/Environment/Textures/Brick/Bricks059_8K-PNG_Roughness.png");
        Texture2D aoMap = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/_Game/Art/Environment/Textures/Brick/Bricks059_8K-PNG_AmbientOcclusion.png");

        if (colorMap == null) { Debug.LogError("[BrickSetup] Color map not found"); return; }
        if (normalMap == null) { Debug.LogError("[BrickSetup] Normal map not found"); return; }

        // -------------------------------------------------------
        // STEP 3 — Create the material
        // -------------------------------------------------------
        string matFolder = "Assets/_Game/Materials";
        if (!AssetDatabase.IsValidFolder(matFolder))
            AssetDatabase.CreateFolder("Assets/_Game", "Materials");

        string matPath = matFolder + "/MAT_BrickExterior.mat";

        // Use URP Lit shader
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null)
        {
            Debug.LogError("[BrickSetup] URP Lit shader not found — make sure URP is installed");
            return;
        }

        Material brickMat = new Material(urpLit);
        brickMat.name = "MAT_BrickExterior";

        // -------------------------------------------------------
        // STEP 4 — Wire up all texture maps
        // -------------------------------------------------------

        // Albedo (colour)
        brickMat.SetTexture("_BaseMap", colorMap);

        // Normal map — gives depth to mortar lines
        brickMat.SetTexture("_BumpMap", normalMap);
        brickMat.SetFloat("_BumpScale", 1.0f);
        brickMat.EnableKeyword("_NORMALMAP");

        // Roughness — brick is matte, not shiny
        // URP uses a Smoothness workflow — roughness needs to be inverted
        // We set smoothness low directly since we can't auto-invert in script
        brickMat.SetFloat("_Smoothness", 0.15f); // low = rough = correct for brick

        // Occlusion (darkens mortar lines)
        if (aoMap != null)
        {
            brickMat.SetTexture("_OcclusionMap", aoMap);
            brickMat.SetFloat("_OcclusionStrength", 0.8f);
        }

        // -------------------------------------------------------
        // STEP 5 — Tiling
        // This controls how many times the texture repeats across
        // a surface. At real-world scale (1 unit = 1m) tiling of
        // 2x2 means the 8K texture covers 0.5m per repeat —
        // roughly 4-5 brick courses visible per metre. Adjust if
        // bricks look too large or too small on your walls.
        // -------------------------------------------------------
        brickMat.SetTextureScale("_BaseMap", new Vector2(2f, 2f));
        brickMat.SetTextureScale("_BumpMap", new Vector2(2f, 2f));

        // Render both sides (so exterior visible from inside and out)
        brickMat.SetFloat("_Cull", 0); // 0 = Off (both sides)

        // -------------------------------------------------------
        // STEP 6 — Save the material asset
        // -------------------------------------------------------
        AssetDatabase.CreateAsset(brickMat, matPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[BrickSetup] MAT_BrickExterior created at: " + matPath);
        Debug.Log("[BrickSetup] Done! Now assign MAT_BrickExterior to your house exterior walls.");
        Debug.Log("[BrickSetup] Tip: Select all exterior wall objects, drag the material onto them in the Inspector.");
    }
}
