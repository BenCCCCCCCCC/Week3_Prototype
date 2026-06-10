using UnityEditor;
using UnityEngine;

public static class PatchRollback
{
    [MenuItem("Tools/Patch/Rollback v0.2 → v0.1")]
    public static void RollbackV02ToV01()
    {
        // TODO: Restore PATCH-012-01 after the numeric patch field and old value are approved.
        // TODO: Restore PATCH-012-02 after the mechanism patch field and old value are approved.
        // TODO: Restore PATCH-012-03 after the map patch Transform and old value are approved.
        Debug.LogWarning("Patch rollback skeleton only. Week 12 patch values are not approved yet.");
    }
}
