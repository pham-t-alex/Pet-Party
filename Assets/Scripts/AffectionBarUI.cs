using UnityEngine;
using UnityEngine.UI;

public class AffectionBarUI : MonoBehaviour
{
    [SerializeField] private RectTransform affectionBar;

    private float maxAffection;
    private float baseWidth;
    private float baseHeight;

    void Awake()
    {
        if (affectionBar == null)
            affectionBar = GetComponent<RectTransform>(); 

        baseWidth  = affectionBar.sizeDelta.x;
        baseHeight = affectionBar.sizeDelta.y;
    }

    public void SetMaxAffection(float maxAffection)
    {
        this.maxAffection = Mathf.Max(0.0001f, maxAffection);
        SetAffection(this.maxAffection);
    }

    public void SetAffection(float affection)
    {
        if (maxAffection <= 0f) return;

        float ratio = Mathf.Clamp01(affection / maxAffection);
        float newWidth = ratio * baseWidth;
        affectionBar.sizeDelta = new Vector2(newWidth, baseHeight);
    }
}
