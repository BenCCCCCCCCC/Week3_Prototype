using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PatchRollback
{
    private const string InteractionStatsPath = "Assets/Configs/InteractionStats_Default.asset";
    private const string Map2ScenePath = "Assets/Scenes/Map2_W7.unity";
    private const string ResultPanelPath = "Assets/Scripts/ResultPanelUI.cs";
    private const string CoverObjectName = "Cover1";
    private const string VersionCurrentText = "BuildVersionLabel = \"v0.2\"";
    private const string VersionRollbackText = "BuildVersionLabel = \"v0.1\"";
    private const string VersionCurrentValue = "v0.2";
    private const string VersionRollbackValue = "v0.1";

    private const float RepairCurrentValue = 10f;
    private const float RepairRollbackValue = 2f;
    private const float RescueCurrentValue = 2.8f;
    private const float RescueRollbackValue = 3.5f;

    private static readonly Vector3 CoverCurrentPosition = new Vector3(6.2f, 0.51f, -4.2f);
    private static readonly Vector3 CoverRollbackPosition = new Vector3(6.99f, 0.51f, -4.97f);

    [MenuItem("Tools/Patch/Rollback v0.2 -> v0.1")]
    public static void RollbackV02ToV01()
    {
        InteractionStatsSO interactionStats = AssetDatabase.LoadAssetAtPath<InteractionStatsSO>(InteractionStatsPath);
        if (interactionStats == null)
        {
            Debug.LogError($"Patch rollback failed: missing interaction stats asset at {InteractionStatsPath}.");
            return;
        }

        bool openedSceneForRollback = false;
        Scene mapScene = GetLoadedScene(Map2ScenePath);
        if (!mapScene.IsValid() || !mapScene.isLoaded)
        {
            mapScene = EditorSceneManager.OpenScene(Map2ScenePath, OpenSceneMode.Additive);
            openedSceneForRollback = true;
        }

        GameObject cover = FindSingleGameObject(mapScene, CoverObjectName);
        if (cover == null)
        {
            if (openedSceneForRollback)
            {
                EditorSceneManager.CloseScene(mapScene, true);
            }

            return;
        }

        Undo.RecordObject(interactionStats, "Rollback PATCH-012 interaction values");
        interactionStats.repairHoldSeconds = RepairRollbackValue;
        interactionStats.rescueHoldSeconds = RescueRollbackValue;
        EditorUtility.SetDirty(interactionStats);
        AssetDatabase.SaveAssets();

        Undo.RecordObject(cover.transform, "Rollback PATCH-012 Cover1 position");
        cover.transform.localPosition = CoverRollbackPosition;
        EditorUtility.SetDirty(cover.transform);
        EditorSceneManager.MarkSceneDirty(mapScene);
        EditorSceneManager.SaveScene(mapScene);

        if (!RollbackVersionLabel())
        {
            if (openedSceneForRollback)
            {
                EditorSceneManager.CloseScene(mapScene, true);
            }

            return;
        }

        if (openedSceneForRollback)
        {
            EditorSceneManager.CloseScene(mapScene, true);
        }
    }

    private static Scene GetLoadedScene(string scenePath)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.path == scenePath)
            {
                return scene;
            }
        }

        return default(Scene);
    }

    private static GameObject FindSingleGameObject(Scene scene, string objectName)
    {
        GameObject match = null;
        int matchCount = 0;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            FindByName(root.transform, objectName, ref match, ref matchCount);
        }

        if (matchCount == 1)
        {
            return match;
        }

        Debug.LogError($"Patch rollback failed: expected exactly one {objectName} in {scene.path}, found {matchCount}.");
        return null;
    }

    private static void FindByName(Transform current, string objectName, ref GameObject match, ref int matchCount)
    {
        if (current.name == objectName)
        {
            match = current.gameObject;
            matchCount++;
        }

        for (int i = 0; i < current.childCount; i++)
        {
            FindByName(current.GetChild(i), objectName, ref match, ref matchCount);
        }
    }

    private static bool RollbackVersionLabel()
    {
        if (!File.Exists(ResultPanelPath))
        {
            Debug.LogError($"Patch rollback failed: missing result panel script at {ResultPanelPath}.");
            return false;
        }

        string source = File.ReadAllText(ResultPanelPath);
        if (!source.Contains(VersionCurrentText))
        {
            Debug.LogError($"Patch rollback failed: expected {VersionCurrentText} in {ResultPanelPath}.");
            return false;
        }

        EditorUtility.DisplayDialog("Week 12 Rollback", BuildRollbackSummary(), "OK");
        LogRollbackSummary();

        File.WriteAllText(ResultPanelPath, source.Replace(VersionCurrentText, VersionRollbackText));
        AssetDatabase.ImportAsset(ResultPanelPath);
        return true;
    }

    private static string BuildRollbackSummary()
    {
        return
            "Week 12 PATCH-012 rollback applied:\n" +
            $"repairHoldSeconds: {FormatNumber(RepairCurrentValue)} -> {FormatNumber(RepairRollbackValue)}\n" +
            $"rescueHoldSeconds: {FormatNumber(RescueCurrentValue)} -> {FormatNumber(RescueRollbackValue)}\n" +
            $"Cover1 localPosition: {FormatVector(CoverCurrentPosition)} -> {FormatVector(CoverRollbackPosition)}\n" +
            $"BuildVersionLabel: {VersionCurrentValue} -> {VersionRollbackValue}";
    }

    private static void LogRollbackSummary()
    {
        Debug.Log("Week 12 PATCH-012 rollback applied.");
        Debug.Log($"repairHoldSeconds: {FormatNumber(RepairCurrentValue)} -> {FormatNumber(RepairRollbackValue)}");
        Debug.Log($"rescueHoldSeconds: {FormatNumber(RescueCurrentValue)} -> {FormatNumber(RescueRollbackValue)}");
        Debug.Log($"Cover1 localPosition: {FormatVector(CoverCurrentPosition)} -> {FormatVector(CoverRollbackPosition)}");
        Debug.Log($"BuildVersionLabel: {VersionCurrentValue} -> {VersionRollbackValue}");
    }

    private static string FormatNumber(float value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static string FormatVector(Vector3 value)
    {
        return $"{{{FormatNumber(value.x)}, {FormatNumber(value.y)}, {FormatNumber(value.z)}}}";
    }
}
