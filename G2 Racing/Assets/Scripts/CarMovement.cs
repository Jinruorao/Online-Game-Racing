using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    Rigidbody rb;
    public Vector3 thrustForce = new Vector3(0f, 0f, 45f);
    public Vector3 rotationTorque = new Vector3(0f, 8f, 0f);
    public bool controlsEnabled;

    // === 加速倍率 ===
    private float _currentBoostMultiplier = 1f;
    private Coroutine _boostCoroutine;

    // 缓存原始推力，以便叠加加速效果后恢复
    private Vector3 _baseThrustForce;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        controlsEnabled = false;
        _baseThrustForce = thrustForce;
    }

    // Update is called once per frame
    void Update()
    {
        if (controlsEnabled)
        {
            // 应用加速倍率
            Vector3 boostedThrust = thrustForce * _currentBoostMultiplier;

            //moving forward
            if (Input.GetKey("w"))
            {
                rb.AddRelativeForce(boostedThrust);
            }
            //moving backward
            if (Input.GetKey("s"))
            {
                rb.AddRelativeForce(-boostedThrust);
            }
            //turn left
            if (Input.GetKey("a"))
            {
                rb.AddRelativeTorque(-rotationTorque);
            }
            //turn right
            if (Input.GetKey("d"))
            {
                rb.AddRelativeTorque(rotationTorque);
            }
        }
    }

    #region === 加速接口 ===

    /// <summary>
    /// 施加一个持续 duration 秒的速度倍率（由 SpeedBoost 等外部脚本调用）。
    /// </summary>
    public void ApplyBoost(float multiplier, float duration)
    {
        if (_boostCoroutine != null)
            StopCoroutine(_boostCoroutine);
        _boostCoroutine = StartCoroutine(BoostRoutine(multiplier, duration));
    }

    private IEnumerator BoostRoutine(float multiplier, float duration)
    {
        _currentBoostMultiplier = multiplier;
        yield return new WaitForSeconds(duration);
        _currentBoostMultiplier = 1f;
        _boostCoroutine = null;
    }

    /// <summary>
    /// 立即取消加速效果。
    /// </summary>
    public void ResetSpeedMultiplier()
    {
        if (_boostCoroutine != null)
            StopCoroutine(_boostCoroutine);
        _currentBoostMultiplier = 1f;
        _boostCoroutine = null;
    }

    #endregion
}
