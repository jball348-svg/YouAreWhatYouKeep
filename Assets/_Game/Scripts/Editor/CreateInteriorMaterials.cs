// CreateInteriorMaterials.cs
// Creates MAT_WallInterior (cream plaster) and MAT_RoofSlate (dark grey)
// then assigns them to the correct objects.
// Run via Tools → Create Interior Materials

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class CreateInteriorMaterials : MonoBehaviour
{
    [MenuItem("Tools/Create Interior Materials")]
    public static void CreateMaterials()
    {
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null)
        {
            Debug.LogError("[InteriorMats] URP Lit shader not found");
            return;
        }

        string matFolder = "Assets/_Game/Materials";

        // -------------------------------------------------------
        // MAT_WallInterior — cream/off-white plaster
        // -------------------------------------------------------
        Material wallMat = new Material(urpLit);
        wallMat.name = "MAT_WallInterior";
        // Warm off-white — the colour from your reference photos
        wallMat.SetColor("_BaseColor", new Color(0.96f, 0.94f, 0.88f, 1f));
        wallMat.SetFloat("_Smoothness", 0.1f);  // slightly matte like plaster
        wallMat.SetFloat("_Cull", 0);           // both sides
        AssetDatabase.CreateAsset(wallMat, matFolder + "/MAT_WallInterior.mat");
        Debug.Log("[InteriorMats] MAT_WallInterior created");

        // -------------------------------------------------------
        // MAT_RoofSlate — dark grey slate tile
        // -------------------------------------------------------
        Material roofMat = new Material(urpLit);
        roofMat.name = "MAT_RoofSlate";
        // Dark blue-grey — typical UK slate roof colour
        roofMat.SetColor("_BaseColor", new Color(0.22f, 0.24f, 0.27f, 1f));
        roofMat.SetFloat("_Smoothness", 0.05f); // very matte
        roofMat.SetFloat("_Cull", 0);
        AssetDatabase.CreateAsset(roofMat, matFolder + "/MAT_RoofSlate.mat");
        Debug.Log("[InteriorMats] MAT_RoofSlate created");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // -------------------------------------------------------
        // Assign MAT_RoofSlate to roof panels
        // -------------------------------------------------------
        string[] roofObjects = { "Roof_PanelLeft", "Roof_PanelRight", "Roof_GableLeft", "Roof_GableRight" };
        int roofCount = 0;
        foreach (string name in roofObjects)
        {
            GameObject go = GameObject.Find(name);
            if (go == null) continue;
            MeshRenderer r = go.GetComponent<MeshRenderer>();
            if (r == null) continue;
            r.sharedMaterial = roofMat;
            roofCount++;
        }
        Debug.Log("[InteriorMats] Roof slate applied to " + roofCount + " objects");

        // -------------------------------------------------------
        // Assign MAT_WallInterior to HOUSE_downstairs and HOUSE_Upstairs
        // -------------------------------------------------------
        string[] interiorParents = { "HOUSE_downstairs", "HOUSE_Upstairs" };
        int interiorCount = 0;
        foreach (string parentName in interiorParents)
        {
            GameObject parent = GameObject.Find(parentName);
            if (parent == null) { Debug.LogWarning("[InteriorMats] Not found: " + parentName); continue; }
            MeshRenderer[] renderers = parent.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer r in renderers)
            {
                r.sharedMaterial = wallMat;
                interiorCount++;
            }
        }
        Debug.Log("[InteriorMats] Interior wall material applied to " + interiorCount + " renderers");

        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("[InteriorMats] Done.");
    }
}
