using UnityEngine;
using UnityEngine.UI;

public class EnergyBarUI : MonoBehaviour
{
    [SerializeField] private RectTransform energyBar;

    private float maxEnergy;
    private float width;
    private float height;

    void Awake()
    {
        if (energyBar == null)
            energyBar = GetComponent<RectTransform>();

        width = energyBar.sizeDelta.x;
        height = energyBar.sizeDelta.y;
    }

    public void SetMaxEnergy(float maxEnergy)
    {
        this.maxEnergy = maxEnergy;
    }

    public void SetEnergy(float energy)
    {
        if (maxEnergy <= 0) return;

        float newWidth = (energy / maxEnergy) * width;
        energyBar.sizeDelta = new Vector2(newWidth, height);
    }
}
