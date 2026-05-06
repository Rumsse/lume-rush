using UnityEngine;

public class FuelConsumer : MonoBehaviour
{
    [SerializeField] float consumeInterval = 1f;
    [SerializeField] int consumeAmount = 1;

    float timer;

    void Update()
    {
        if (ResourceManager.Instance == null)
            return;

        timer += Time.deltaTime;

        if (timer < consumeInterval)
            return;

        timer = 0f;

        if (!ResourceManager.Instance.ConsumeResource("Fuel", consumeAmount))
            Debug.Log("[FuelConsumer] Out of fuel!");
    }
}