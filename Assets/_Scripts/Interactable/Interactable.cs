using UnityEngine;

public class Interactable : MonoBehaviour
{
    public ResourceData resourceData;

    [SerializeField] string thicknessProperty = "_OutlineThickness";
    [SerializeField] string colorProperty = "_OutlineColor";
    [SerializeField] float highlightThickness = 1.15f;
    [SerializeField] int targetMaterialIndex = -1;

    Renderer[] renderers;
    MaterialPropertyBlock mpb;
    int thicknessId;
    int colorId;

    public string ResourceName => resourceData?.resourceName;
    public Sprite ResourceIcon => resourceData?.resourceIcon;
    public bool CanBePickedUp => gameObject.activeInHierarchy;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        mpb = new MaterialPropertyBlock();
        thicknessId = Shader.PropertyToID(thicknessProperty);
        colorId = Shader.PropertyToID(colorProperty);
        SetHighlighted(false);
    }

    public void SetHighlighted(bool value)
    {
        mpb.SetFloat(thicknessId, value ? highlightThickness : 0f);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (targetMaterialIndex < 0) renderers[i].SetPropertyBlock(mpb);
            else renderers[i].SetPropertyBlock(mpb, targetMaterialIndex);
        }
    }
}