using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class Week10TaskDefinitionValidator
{
    private class ExpectedTask
    {
        public string id;
        public string layer;
        public string perspective;
        public string pair;
        public string type;
        public int targetValue;
        public string rewardTiming;
        public bool canReset;

        public ExpectedTask(
            string id,
            string layer,
            string perspective,
            string pair,
            string type,
            int targetValue,
            string rewardTiming,
            bool canReset)
        {
            this.id = id;
            this.layer = layer;
            this.perspective = perspective;
            this.pair = pair;
            this.type = type;
            this.targetValue = targetValue;
            this.rewardTiming = rewardTiming;
            this.canReset = canReset;
        }
    }

    private static Dictionary<string, ExpectedTask> expectedTasks = new Dictionary<string, ExpectedTask>()
    {
        // Daily
        { "D-S-01", new ExpectedTask("D-S-01", "Daily", "Survivor", "D-H-01", "RepairProgressReach", 50, "OnMatchSettlement", true) },
        { "D-H-01", new ExpectedTask("D-H-01", "Daily", "Hunter", "D-S-01", "DownCountReach", 2, "OnMatchSettlement", true) },
        { "D-S-02", new ExpectedTask("D-S-02", "Daily", "Survivor", "D-H-02", "RescueCountReach", 1, "OnMatchSettlement", true) },
        { "D-H-02", new ExpectedTask("D-H-02", "Daily", "Hunter", "D-S-02", "PreventRescueReach", 1, "OnMatchSettlement", true) },
        { "D-X-01", new ExpectedTask("D-X-01", "Daily", "Shared", "-", "MatchCompleteCountReach", 1, "OnMatchSettlement", true) },
        { "D-X-02", new ExpectedTask("D-X-02", "Daily", "Shared", "-", "SkillUseCountReach", 3, "OnMatchSettlement", true) },

        // Weekly
        { "W-S-01", new ExpectedTask("W-S-01", "Weekly", "Survivor", "W-H-01", "RescueCountReach", 5, "OnWeeklyClaim", true) },
        { "W-H-01", new ExpectedTask("W-H-01", "Weekly", "Hunter", "W-S-01", "PreventRescueReach", 5, "OnWeeklyClaim", true) },
        { "W-S-02", new ExpectedTask("W-S-02", "Weekly", "Survivor", "W-H-02", "RepairProgressReach", 500, "OnWeeklyClaim", true) },
        { "W-H-02", new ExpectedTask("W-H-02", "Weekly", "Hunter", "W-S-02", "PatrolSignalReach", 15, "OnWeeklyClaim", true) },
        { "W-X-01", new ExpectedTask("W-X-01", "Weekly", "Shared", "-", "MatchCompleteCountReach", 10, "OnWeeklyClaim", true) },
        { "W-X-02", new ExpectedTask("W-X-02", "Weekly", "Shared", "-", "SkillUseCountReach", 30, "OnWeeklyClaim", true) },

        // Season
        { "S-S-01", new ExpectedTask("S-S-01", "Season", "Survivor", "S-H-01", "RepairProgressReach", 2500, "OnSeasonClaim", false) },
        { "S-H-01", new ExpectedTask("S-H-01", "Season", "Hunter", "S-S-01", "DownCountReach", 40, "OnSeasonClaim", false) },
        { "S-S-02", new ExpectedTask("S-S-02", "Season", "Survivor", "S-H-02", "RescueCountReach", 30, "OnSeasonClaim", false) },
        { "S-H-02", new ExpectedTask("S-H-02", "Season", "Hunter", "S-S-02", "PreventRescueReach", 30, "OnSeasonClaim", false) },
        { "S-S-03", new ExpectedTask("S-S-03", "Season", "Survivor", "S-H-03", "EscapeOnce", 20, "OnSeasonClaim", false) },
        { "S-H-03", new ExpectedTask("S-H-03", "Season", "Hunter", "S-S-03", "SeasonWinCountReach", 20, "OnSeasonClaim", false) },
        { "S-S-04", new ExpectedTask("S-S-04", "Season", "Survivor", "S-H-04", "GateOpenCountReach", 15, "OnSeasonClaim", false) },
        { "S-H-04", new ExpectedTask("S-H-04", "Season", "Hunter", "S-S-04", "CustomDesignOnly", 15, "OnSeasonClaim", false) },
        { "S-X-01", new ExpectedTask("S-X-01", "Season", "Shared", "-", "MatchCompleteCountReach", 50, "OnSeasonClaim", false) },
        { "S-X-02", new ExpectedTask("S-X-02", "Season", "Shared", "-", "SkillUseCountReach", 150, "OnSeasonClaim", false) },
    };

    [MenuItem("Nightfile/Week10/Validate Task Definitions")]
    public static void ValidateTaskDefinitions()
    {
        string[] guids = AssetDatabase.FindAssets("t:TaskDefinition");

        int checkedCount = 0;
        int warningCount = 0;
        HashSet<string> seenIds = new HashSet<string>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);

            if (asset == null)
            {
                continue;
            }

            SerializedObject so = new SerializedObject(asset);

            SerializedProperty taskIdProp = so.FindProperty("taskId");
            if (taskIdProp == null)
            {
                Debug.LogWarning("[Week10Check] 找到一个 TaskDefinition，但没有 taskId 字段: " + path, asset);
                warningCount++;
                continue;
            }

            checkedCount++;

            string rawId = taskIdProp.stringValue;
            string id = SafeTrim(rawId);

            if (rawId != id)
            {
                Debug.LogWarning("[Week10Check] taskId 前后有空格: [" + rawId + "] -> 应改为 [" + id + "] 路径: " + path, asset);
                warningCount++;
            }

            CheckTrim(so, "taskName", path, asset, ref warningCount);
            CheckTrim(so, "counterpartTaskId", path, asset, ref warningCount);
            CheckTrim(so, "triggerCondition", path, asset, ref warningCount);
            CheckTrim(so, "completionRule", path, asset, ref warningCount);
            CheckTrim(so, "progressReportFrequency", path, asset, ref warningCount);
            CheckTrim(so, "implementationNote", path, asset, ref warningCount);

            if (seenIds.Contains(id))
            {
                Debug.LogWarning("[Week10Check] 重复 taskId: " + id + " 路径: " + path, asset);
                warningCount++;
            }
            else
            {
                seenIds.Add(id);
            }

            if (!expectedTasks.ContainsKey(id))
            {
                Debug.LogWarning("[Week10Check] 文档表格中没有这个 taskId: [" + id + "] 路径: " + path, asset);
                warningCount++;
                continue;
            }

            ExpectedTask expected = expectedTasks[id];

            CheckEnum(so, "taskLayer", expected.layer, id, path, asset, ref warningCount);
            CheckEnum(so, "perspective", expected.perspective, id, path, asset, ref warningCount);
            CheckString(so, "counterpartTaskId", expected.pair, id, path, asset, ref warningCount);
            CheckEnum(so, "taskType", expected.type, id, path, asset, ref warningCount);
            CheckInt(so, "targetValue", expected.targetValue, id, path, asset, ref warningCount);
            CheckEnum(so, "rewardTiming", expected.rewardTiming, id, path, asset, ref warningCount);
            CheckBool(so, "canReset", expected.canReset, id, path, asset, ref warningCount);
        }

        foreach (string expectedId in expectedTasks.Keys)
        {
            if (!seenIds.Contains(expectedId))
            {
                Debug.LogWarning("[Week10Check] 缺少文档中应该存在的任务卡 asset: " + expectedId);
                warningCount++;
            }
        }

        Debug.Log("[Week10Check] 检查完成。TaskDefinition 数量 = " + checkedCount + "，警告数量 = " + warningCount);
    }

    [MenuItem("Nightfile/Week10/Auto Trim Task Text Fields")]
    public static void AutoTrimTaskTextFields()
    {
        string[] guids = AssetDatabase.FindAssets("t:TaskDefinition");
        int fixedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);

            if (asset == null)
            {
                continue;
            }

            SerializedObject so = new SerializedObject(asset);

            bool changed = false;
            changed |= TrimProperty(so, "taskId");
            changed |= TrimProperty(so, "taskName");
            changed |= TrimProperty(so, "counterpartTaskId");
            changed |= TrimProperty(so, "triggerCondition");
            changed |= TrimProperty(so, "completionRule");
            changed |= TrimProperty(so, "progressReportFrequency");
            changed |= TrimProperty(so, "implementationNote");

            if (changed)
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(asset);
                fixedCount++;
                Debug.Log("[Week10Fix] 已清理前后空格: " + path, asset);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[Week10Fix] 清理完成。修改 asset 数量 = " + fixedCount);
    }

    private static void CheckTrim(SerializedObject so, string propertyName, string path, Object asset, ref int warningCount)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop == null || prop.propertyType != SerializedPropertyType.String)
        {
            return;
        }

        string raw = prop.stringValue;
        string trimmed = SafeTrim(raw);

        if (raw != trimmed)
        {
            Debug.LogWarning("[Week10Check] 字段 " + propertyName + " 前后有空格: [" + raw + "] -> [" + trimmed + "] 路径: " + path, asset);
            warningCount++;
        }
    }

    private static bool TrimProperty(SerializedObject so, string propertyName)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop == null || prop.propertyType != SerializedPropertyType.String)
        {
            return false;
        }

        string raw = prop.stringValue;
        string trimmed = SafeTrim(raw);

        if (raw != trimmed)
        {
            prop.stringValue = trimmed;
            return true;
        }

        return false;
    }

    private static void CheckString(SerializedObject so, string propertyName, string expected, string id, string path, Object asset, ref int warningCount)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop == null)
        {
            Debug.LogWarning("[Week10Check] " + id + " 缺少字段: " + propertyName + " 路径: " + path, asset);
            warningCount++;
            return;
        }

        string actual = SafeTrim(prop.stringValue);

        if (actual != expected)
        {
            Debug.LogWarning("[Week10Check] " + id + " 字段不一致: " + propertyName + " 实际=[" + actual + "] 文档=[" + expected + "] 路径: " + path, asset);
            warningCount++;
        }
    }

    private static void CheckInt(SerializedObject so, string propertyName, int expected, string id, string path, Object asset, ref int warningCount)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop == null)
        {
            Debug.LogWarning("[Week10Check] " + id + " 缺少字段: " + propertyName + " 路径: " + path, asset);
            warningCount++;
            return;
        }

        if (prop.intValue != expected)
        {
            Debug.LogWarning("[Week10Check] " + id + " 数值不一致: " + propertyName + " 实际=" + prop.intValue + " 文档=" + expected + " 路径: " + path, asset);
            warningCount++;
        }
    }

    private static void CheckBool(SerializedObject so, string propertyName, bool expected, string id, string path, Object asset, ref int warningCount)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop == null)
        {
            Debug.LogWarning("[Week10Check] " + id + " 缺少字段: " + propertyName + " 路径: " + path, asset);
            warningCount++;
            return;
        }

        if (prop.boolValue != expected)
        {
            Debug.LogWarning("[Week10Check] " + id + " 布尔值不一致: " + propertyName + " 实际=" + prop.boolValue + " 文档=" + expected + " 路径: " + path, asset);
            warningCount++;
        }
    }

    private static void CheckEnum(SerializedObject so, string propertyName, string expected, string id, string path, Object asset, ref int warningCount)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop == null)
        {
            Debug.LogWarning("[Week10Check] " + id + " 缺少字段: " + propertyName + " 路径: " + path, asset);
            warningCount++;
            return;
        }

        if (prop.propertyType != SerializedPropertyType.Enum)
        {
            Debug.LogWarning("[Week10Check] " + id + " 字段不是 enum: " + propertyName + " 路径: " + path, asset);
            warningCount++;
            return;
        }

        string actual = prop.enumNames[prop.enumValueIndex];

        if (actual != expected)
        {
            Debug.LogWarning("[Week10Check] " + id + " 枚举不一致: " + propertyName + " 实际=" + actual + " 文档=" + expected + " 路径: " + path, asset);
            warningCount++;
        }
    }

    private static string SafeTrim(string value)
    {
        if (value == null)
        {
            return "";
        }

        return value.Trim();
    }
}