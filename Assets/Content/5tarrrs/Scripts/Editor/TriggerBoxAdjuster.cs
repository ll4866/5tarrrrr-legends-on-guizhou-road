using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.Linq;

public class TriggerBoxAdjuster : OdinEditorWindow
{
    [MenuItem("Tools/TriggerBox Adjuster")]
    private static void OpenWindow() => GetWindow<TriggerBoxAdjuster>().Show();

    [Title("TriggerBox Adjuster")]
    [InfoBox("Finds all GameObjects named \"TriggerBox\" (including inactive) with a Box Collider (IsTrigger = true), " +
             "multiplies their collider's Y size, and shifts the center down so the bottom stays in place.")]

    [LabelText("Y Size Multiplier"), Range(0.01f, 1f)]
    public float yMultiplier = 0.5f;

    [ReadOnly, ListDrawerSettings(IsReadOnly = true)]
    [LabelText("Found TriggerBoxes")]
    public List<GameObject> foundObjects = new();

    private List<BoxCollider> FindAllTriggerBoxes()
    {
        var results = new List<BoxCollider>();

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            foreach (var root in scene.GetRootGameObjects())
            {
                results.AddRange(
                    root.GetComponentsInChildren<BoxCollider>(true)
                        .Where(bc => bc.gameObject.name == "TriggerBox" && bc.isTrigger)
                );
            }
        }

        return results;
    }

    [Button(ButtonSizes.Medium), GUIColor(0.4f, 0.8f, 1f)]
    private void FindTriggerBoxes()
    {
        foundObjects = FindAllTriggerBoxes().Select(bc => bc.gameObject).ToList();
        Debug.Log($"[TriggerBoxAdjuster] Found {foundObjects.Count} TriggerBox(es) with IsTrigger enabled.");
    }

    [Button(ButtonSizes.Large), GUIColor(0.4f, 1f, 0.4f)]
    private void AdjustColliders()
    {
        var colliders = FindAllTriggerBoxes();
        foundObjects = colliders.Select(bc => bc.gameObject).ToList();

        if (colliders.Count == 0)
        {
            Debug.LogWarning("[TriggerBoxAdjuster] No matching TriggerBox found.");
            return;
        }

        Undo.SetCurrentGroupName("Adjust TriggerBox Colliders");
        int undoGroup = Undo.GetCurrentGroup();

        foreach (var boxCollider in colliders)
        {
            Undo.RecordObject(boxCollider, "Adjust BoxCollider");

            float originalY = boxCollider.size.y;
            float newY = originalY * yMultiplier;
            float delta = originalY - newY;

            boxCollider.size = new Vector3(boxCollider.size.x, newY, boxCollider.size.z);
            boxCollider.center = new Vector3(boxCollider.center.x, boxCollider.center.y - delta * 0.5f, boxCollider.center.z);

            Debug.Log($"[TriggerBoxAdjuster] Adjusted \"{boxCollider.gameObject.name}\" — Y size: {originalY} → {newY} (x{yMultiplier}), center shifted down by {delta * 0.5f}", boxCollider.gameObject);
        }

        Undo.CollapseUndoOperations(undoGroup);
    }
}
