using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class RacingModeGameManager : MonoBehaviourPunCallbacks
{
    [Header("玩家预设")]
    public GameObject[] PlayerPrefabs;
    public Transform[] InstantiatePositions;

    [Header("UI 引用")]
    public Text TimeUIText;

    [Header("圈数触发器")]
    public List<GameObject> lapTriggers = new List<GameObject>();

    // Singleton
    public static RacingModeGameManager instance = null;

    private bool playersSpawned = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // 场景加载完成后再生成玩家
        if (PhotonNetwork.IsConnectedAndReady)
        {
            SpawnLocalPlayer();
        }
    }

    /// <summary>
    /// 生成本地玩家（由场景加载完成时调用）
    /// </summary>
    void SpawnLocalPlayer()
    {
        if (playersSpawned) return;
        playersSpawned = true;

        object playerSelectionNumber;
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(
            MultiplayerRacingGame.PLAYER_SELECTION_NUMBER, out playerSelectionNumber))
        {
            int selectionIndex = (int)playerSelectionNumber;
            int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

            // 根据 ActorNumber 确定生成位置
            Vector3 spawnPos;
            if (actorNumber - 1 < InstantiatePositions.Length)
            {
                spawnPos = InstantiatePositions[actorNumber - 1].position;
            }
            else
            {
                // 如果超出位置数组，循环使用
                spawnPos = InstantiatePositions[(actorNumber - 1) % InstantiatePositions.Length].position;
            }

            if (selectionIndex >= 0 && selectionIndex < PlayerPrefabs.Length)
            {
                string prefabName = PlayerPrefabs[selectionIndex].name;
                Debug.Log($"正在生成玩家: {prefabName} 于位置 {spawnPos}");
                PhotonNetwork.Instantiate(prefabName, spawnPos, Quaternion.identity, 0);
            }
            else
            {
                Debug.LogError($"玩家选择序号无效: {selectionIndex}");
            }
        }
        else
        {
            Debug.LogError("无法获取玩家选择属性，使用默认车辆");
            // 回退：使用第一个预设
            if (PlayerPrefabs.Length > 0 && InstantiatePositions.Length > 0)
            {
                int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
                Vector3 spawnPos = InstantiatePositions[(actorNumber - 1) % InstantiatePositions.Length].position;
                PhotonNetwork.Instantiate(PlayerPrefabs[0].name, spawnPos, Quaternion.identity, 0);
            }
        }
    }

    void Update()
    {
        // 如果在游戏场景中但尚未生成玩家，开始生成
        if (!playersSpawned && PhotonNetwork.IsConnectedAndReady && 
            PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerCount > 0)
        {
            SpawnLocalPlayer();
        }
    }

    /// <summary>
    /// 重新填充 lapTriggers（场景切换后调用）
    /// </summary>
    public void RefreshLapTriggers()
    {
        lapTriggers.Clear();
        GameObject[] allTriggers = GameObject.FindGameObjectsWithTag("LapTrigger");
        foreach (GameObject trigger in allTriggers)
        {
            lapTriggers.Add(trigger);
        }
        Debug.Log($"已刷新圈数触发器，共 {lapTriggers.Count} 个");
    }
}
