using UnityEngine;
using UnityEngine.UI;

public class UITextBlinkColor : MonoBehaviour
{
    [SerializeField] private Text targetText;
    [SerializeField] private float cycleTime = 1.0f;
    [SerializeField] private float darkRate = 0.3f;

    private Color originalColor;

    void Start()
    {
        if (targetText == null)
            targetText = GetComponent<Text>();

        originalColor = targetText.color;
    }

    void Update()
    {
        float t = Mathf.PingPong(Time.time / (cycleTime / 2f), 1f);
        float brightness = Mathf.Lerp(darkRate, 1f, t);
        targetText.color = originalColor * brightness;//テキストの明度をループで変化させる
    }
}
