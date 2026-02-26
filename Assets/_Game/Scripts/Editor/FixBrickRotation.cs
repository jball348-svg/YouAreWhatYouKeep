// FixBrickRotation.cs
// Fixes the 5 "brick wrong way" objects by correcting their rotation
// so textures map horizontally. Each wall was rotated 90 degrees on Z
// to be positioned horizontally, which rotates the UV mapping with it.
// We correct by adjusting the rotation and compensating with scale.
// Run via Tools → Fix Brick Rotation

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FixBrickRotation : MonoBehaviour
{
    [MenuItem("Tools/Fix Brick Rotation")]
    public static void FixRotation()
    {
        GameObject[] wrongWay = GameObject.FindGameObjectsWithTag("brick wrong way");

        if (wrongWay.Length == 0)
        {
            Debug.LogWarning("[FixBrickRot] No objects tagged 'brick wrong way' found");
            return;
        }

        foreach (GameObject go in wrongWay)
        {
            Transform t = go.transform;
            Vector3 rot = t.localEulerAngles;
            Vector3 scale = t.localScale;

            Debug.Log($"[FixBrickRot] Processing: {go.name} rot={rot} scale={scale}");

            // These walls were rotated 90 on Z (and some also 90 on Y) to position them.
            // The Z=90 rotation is what causes the UV to appear vertical.
            // Fix: remove the Z rotation by swapping scale axes to compensate.

            if (Mathf.Approximately(rot.z, 90f) || Mathf.Approximately(rot.z, 270f))
            {
                // Swap X and Y scale to compensate for removing Z rotation
                float newX = scale.y;
                float newY = scale.x;
                float newZ = scale.z;

                t.localEulerAngles = new Vector3(rot.x, rot.y, 0f);
                t.localScale = new Vector3(newX, newY, newZ);

                Debug.Log($"[FixBrickRot] Fixed {go.name}: rot Z removed, scale swapped to ({newX}, {newY}, {newZ})");
            }
            else
            {
                Debug.Log($"[FixBrickRot] Skipped {go.name} — no Z rotation found");
            }
        }

        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("[FixBrickRot] Done");
    }
}
