#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Photon.Pun;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 自动扫描 Resources 目录下所有 Prefab，批量添加网络组件。
/// 使用方法：Tools → Racing Game → Setup Network Prefabs
/// </summary>
public class PrefabSetupEditor : MonoBehaviour
{
    [MenuItem("Tools/Racing Game/Setup Network Prefabs")]
    static void SetupAllPlayerPrefabs()
    {
        // 动态扫描整个 Resources 目录（包含 calss assets 等子文件夹）
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Resources" });
        List<string> prefabPaths = new List<string>();

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            // 跳过 PUN 自带的 Demo Prefab
            if (path.Contains("Photon/PhotonUnityNetworking/Demos")) continue;
            // 跳过 .meta 解析
            if (!path.EndsWith(".prefab")) continue;

            prefabPaths.Add(path);
        }

        if (prefabPaths.Count == 0)
        {
            Debug.LogWarning("⚠️ Resources 目录下未找到任何 Prefab，请先放置玩家 Prefab。");
            return;
        }

        int modifiedCount = 0;
        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null) continue;

            bool modified = SetupPrefabComponents(instance, path);
            GameObject.DestroyImmediate(instance);

            if (modified)
            {
                Debug.Log($"✅ 已更新: {path}");
                modifiedCount++;
            }
        }

        Debug.Log($"🎯 设置完成！扫描 {prefabPaths.Count} 个 Prefab，更新 {modifiedCount} 个。");
        AssetDatabase.Refresh();
    }

    static bool SetupPrefabComponents(GameObject go, string path)
    {
        bool modified = false;

        // 1. 确保 Rigidbody 存在
        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = go.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.drag = 0.5f;
            rb.angularDrag = 0.5f;
            rb.useGravity = false;
            Debug.Log($"  ➕ Rigidbody -> {path}");
            modified = true;
        }

        // 2. 确保 PhotonView 存在
        PhotonView pv = go.GetComponent<PhotonView>();
        if (pv == null)
        {
            pv = go.AddComponent<PhotonView>();
            pv.ViewID = 0;
            pv.Synchronization = ViewSynchronization.UnreliableOnChange;
            Debug.Log($"  ➕ PhotonView -> {path}");
            modified = true;
        }
        else if (pv.Synchronization != ViewSynchronization.UnreliableOnChange)
        {
            pv.Synchronization = ViewSynchronization.UnreliableOnChange;
            modified = true;
        }

        // 3. 确保 CarSync 存在
        CarSync carSync = go.GetComponent<CarSync>();
        if (carSync == null)
        {
            carSync = go.AddComponent<CarSync>();
            Debug.Log($"  ➕ CarSync -> {path}");
            modified = true;
        }

        // 4. 将 CarSync 注册到 PhotonView.ObservedComponents
        if (!pv.ObservedComponents.Contains(carSync))
        {
            pv.ObservedComponents.Add(carSync);
            Debug.Log($"  ➕ CarSync → PhotonView.ObservedComponents");
            modified = true;
        }

        // 5. Tag = "Player"
        if (!go.CompareTag("Player"))
        {
            go.tag = "Player";
            Debug.Log($"  ➕ Tag = Player -> {path}");
            modified = true;
        }

        // 6. PlayerSetup 组件
        if (go.GetComponent<PlayerSetup>() == null)
        {
            go.AddComponent<PlayerSetup>();
            Debug.Log($"  ➕ PlayerSetup -> {path}");
            modified = true;
        }

        return modified;
    }

    [MenuItem("Tools/Racing Game/Validate Prefabs (Check Only)")]
    static void ValidateAllPlayerPrefabs()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Resources" });

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith(".prefab")) continue;
            if (path.Contains("Photon/PhotonUnityNetworking/Demos")) continue;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            PhotonView pv = prefab.GetComponent<PhotonView>();
            CarSync cs = prefab.GetComponent<CarSync>();
            PlayerSetup ps = prefab.GetComponent<PlayerSetup>();
            Rigidbody rb = prefab.GetComponent<Rigidbody>();

            bool hasTag = prefab.CompareTag("Player");

            string obsStatus = (pv != null && cs != null && pv.ObservedComponents.Contains(cs)) ? "✅" : "❌";
            Debug.Log($"📋 {path}: PV={(pv != null)} CS={(cs != null)} PS={(ps != null)} RB={(rb != null)} Tag={hasTag} Observed={obsStatus}");
        }
    }

    [MenuItem("Tools/Racing Game/Move Flame Effect to Resources")]
    static void MoveFlameEffectToResources()
    {
        // 如果 ExplosionBomb 的火焰特效不在 Resources 里，
        // 可以通过此工具原位生成引用副本。
        // 实际使用时只需在 ExplosionBomb Inspector 中拖入正确的 Prefab 引用。
        Debug.Log("提示：请确保 ExplosionBomb.flameEffectPrefab 引用的特效 Prefab 已在场景中实例化。");
    }
}
#endif
