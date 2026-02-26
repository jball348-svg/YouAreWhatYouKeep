// AssignBrickMaterial.cs
// Editor utility — assigns MAT_BrickExterior to all renderers
// in HOUSE_downstairs, HOUSE_Upstairs, and HOUSE_Roof.
// Run via Tools → Assign Brick Material, then delete this script.

using UnityEngine;
using UnityEditor;

public class AssignBrickMaterial : MonoBehaviour
{
    [MenuItem("Tools/Assign Brick Material")]
    public static void AssignBrick()
    {
        Material brickMat = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/_Game/Materials/MAT_BrickExterior.mat");

        if (brickMat == null)
        {
            Debug.LogError("[AssignBrick] MAT_BrickExterior not found — run Setup Brick Material first");
            return;
        }

        string[] targets = { "HOUSE_downstairs", "HOUSE_Upstairs", "HOUSE_Roof" };
        int totalAssigned = 0;

        foreach (string targetName in targets)
        {
            GameObject go = GameObject.Find(targetName);
            if (go == null)
            {
                Debug.LogWarning("[AssignBrick] Could not find: " + targetName);
                continue;
            }

            // Get all renderers including children
            MeshRenderer[] renderers = go.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer r in renderers)
            {
                // Apply to all material slots
                Material[] mats = new Material[r.sharedMaterials.Length];
                for (int i = 0; i < mats.Length; i++)
                    mats[i] = brickMat;
                r.sharedMaterials = mats;
                totalAssigned++;
            }

            Debug.Log("[AssignBrick] Applied to " + renderers.Length + " renderers in " + targetName);
        }

        // Mark scene dirty so it saves
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("[AssignBrick] Done — " + totalAssigned + " renderers updated");
    }
}
