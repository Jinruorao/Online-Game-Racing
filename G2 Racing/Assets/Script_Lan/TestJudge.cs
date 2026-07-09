using System.Collections;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// 测试裁判：玩家碰到触发器 → 显示目标物体 → 10 秒后退出房间回到大厅。
/// 挂载在任何带有 Collider (IsTrigger) 的 GameObject 上即可。
/// </summary>
[RequireComponent(typeof(Collider))]
public class TestJudge : MonoBehaviour
{
    [Header("触发后显示的物体（如完赛面板）")]
    public GameObject targetObject;

    [Header("等待秒数")]
    public float waitSeconds = 10f;

    [Header("返回的场景名")]
    public string lobbySceneName = "LobbyScene";

    private bool hasTriggered = false;
    private Collider triggerCollider;

    void Start()
    {
        triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;

        if (targetObject != null)
            targetObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.GetComponentInParent<MovementController>() == null &&
            other.GetComponentInParent<PhotonView>() == null)
            return;

        hasTriggered = true;
        Debug.Log("[TestJudge] 玩家 " + other.name + " 触碰触发器");

        if (targetObject != null)
            targetObject.SetActive(true);

        StartCoroutine(ExitCountdown());
    }

    IEnumerator ExitCountdown()
    {
        float remaining = waitSeconds;
        while (remaining > 0)
        {
            remaining -= Time.deltaTime;
            yield return null;
        }

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(lobbySceneName);
        }
    }

    void OnLeftRoom()
    {
        if (!string.IsNullOrEmpty(lobbySceneName))
            PhotonNetwork.LoadLevel(lobbySceneName);
    }
}
