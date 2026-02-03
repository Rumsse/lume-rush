using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlanetConfirmationUI : MonoBehaviour
{
    public TextMeshProUGUI questionText;
    public Button confirmButton;
    public Button cancelButton;

    public void Setup(string planetName, System.Action onConfirm, System.Action onCancel)
    {
        questionText.text = $"Do you want to explore {planetName}?";

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() => onConfirm?.Invoke());

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() =>
        {
            onCancel?.Invoke();
            gameObject.SetActive(false);
        });
    }
}