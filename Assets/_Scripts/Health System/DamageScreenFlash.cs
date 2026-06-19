using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DamageScreenFlash : MonoBehaviour
{
    #region Serialized

    [SerializeField] float flashDuration = 0.5f;
    [SerializeField][Range(0f, 1f)] float maxIntensity = 0.5f;

    #endregion

    #region State

    Image vignetteImage;
    Color vignetteColor;
    float currentIntensity;

    #endregion

    #region Lifecycle

    void Awake()
    {
        vignetteImage = GetComponent<Image>();
        vignetteColor = vignetteImage.color;

        ChangeAlpha(0f);
    }

    void OnEnable() => PlayerHealthSystem.OnPlayerDamage += Flash;

    void OnDisable() => PlayerHealthSystem.OnPlayerDamage -= Flash;

    void Update()
    {
        if (currentIntensity <= 0f)
            return;

        currentIntensity = Mathf.MoveTowards(currentIntensity, 0f, (maxIntensity / flashDuration) * Time.deltaTime);
        ChangeAlpha(currentIntensity);
    }

    #endregion

    #region Logic

    void Flash() => currentIntensity = maxIntensity;

    void ChangeAlpha(float alpha)
    {
        vignetteColor.a = alpha;
        vignetteImage.color = vignetteColor;
    }

    #endregion
}