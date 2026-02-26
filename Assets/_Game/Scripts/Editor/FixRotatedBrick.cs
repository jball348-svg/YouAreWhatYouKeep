// FixRotatedBrick.cs
// Creates MAT_BrickExterior_Rotated with swapped tiling
// and assigns it to all objects tagged "brick wrong way"
// Run via Tools → Fix Rotated Brick

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FixRotatedBrick : MonoBehaviour
{
    [MenuItem("Tools/Fix Rotated Brick")]
    public static void FixBrick()
    {
        // Load the original brick material to copy settings from
        Material originalMat = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/_Game/Materials/MAT_BrickExterior.mat");

        if (originalMat == null)
        {
            Debug.LogError("[FixBrick] MAT_BrickExterior not found");
            return;
        }

        // Create a copy with corrected tiling
        // Swapping X and Y tiling rotates the pattern 90 degrees
        Material rotatedMat = new Material(originalMat);
        rotatedMat.name = "MAT_BrickExterior_Rotated";

        // Swap the tiling axes to correct the 90 degree rotation
        Vector2 originalTiling = originalMat.GetTextureScale("_BaseMap");
        rotatedMat.SetTextureScale("_BaseMap", new Vector2(originalTiling.y, originalTiling.x));
        rotatedMat.SetTextureScale("_BumpMap", new Vector2(originalTiling.y, originalTiling.x));

        string matPath = "Assets/_Game/Materials/MAT_BrickExterior_Rotated.mat";
        AssetDatabase.CreateAsset(rotatedMat, matPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Reload after save so it's a proper asset reference
        rotatedMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

        // Find all objects with the tag and assign the corrected material
        GameObject[] wrongWayObjects = GameObject.FindGameObjectsWithTag("brick wrong way");

        if (wrongWayObjects.Length == 0)
        {
            Debug.LogWarning("[FixBrick] No objects found with tag 'brick wrong way'");
            return;
        }

        int fixed_count = 0;
        foreach (GameObject go in wrongWayObjects)
        {
            MeshRenderer r = go.GetComponent<MeshRenderer>();
            if (r == null) continue;
            r.sharedMaterial = rotatedMat;
            fixed_count++;
            Debug.Log("[FixBrick] Fixed: " + go.name);
        }

        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("[FixBrick] Done — fixed " + fixed_count + " objects");
    }
}
