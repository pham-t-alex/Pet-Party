using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskDisplayBox : MonoBehaviour
{
    public int TaskId { get; private set; }
    [SerializeField] private TMP_Text taskNameText;
    [SerializeField] private Slider taskProgressSlider;
    [SerializeField] private TMP_Text taskPointsText;
    private string taskName;
    private float maxValue;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializeTaskInfo(int id, string name, float value, float maxValue, int points)
    {
        TaskId = id;
        taskName = name;
        this.maxValue = maxValue;
        taskProgressSlider.maxValue = maxValue;
        taskProgressSlider.value = value;
        taskPointsText.text = $"+{points}";
        taskNameText.text = $"{taskName} ({value.ToString("0.##")}/{maxValue})";
    }

    public void SetTaskValue(float value)
    {
        taskProgressSlider.value = value;
        taskNameText.text = $"{taskName} ({value.ToString("0.##")}/{maxValue})";
    }
}
