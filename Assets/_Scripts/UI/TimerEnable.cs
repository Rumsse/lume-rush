using UnityEngine;

public class TimerEnable : MonoBehaviour
{
    [SerializeField] GameObject timerObject;

    void Start()
    {
        if (!timerObject)
            return;

        timerObject.SetActive(SupernovaManager.Instance != null);
    }
}