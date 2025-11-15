using TMPro;
using UnityEngine;

public class PointDisplay : MonoBehaviour
{
    public ulong PlayerId { get; private set; }
    public int Points { get; private set; }
    [SerializeField] private TMP_Text displayText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializePointDisplay(ulong playerId)
    {
        PlayerId = playerId;
        SetPoints(0);
    }

    public void SetPoints(int points)
    {
        Points = points;
        displayText.text = $"Id: {PlayerId} | {points}";
    }
}
