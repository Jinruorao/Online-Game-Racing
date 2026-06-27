using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LapController : MonoBehaviour
{
    private List<GameObject> LapTriggers = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        foreach(GameObject lapTrigger in RacingModeGameManager.instance.lapTriggers)
        {
            LapTriggers.Add(lapTrigger);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(LapTriggers.Contains(other.gameObject))
        {
            int indexOfTrigger = LapTriggers.IndexOf(other.gameObject);
            LapTriggers[indexOfTrigger].SetActive(false);
            if(other.name == "FinishTrigger")
            {
                //game is finished
                GameFinished();
            }
        }
    }
    void GameFinished()
    {
        GetComponent<PlayerSetup>().PlayerCamera.transform.parent = null;
        GetComponent<CarMovement>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
