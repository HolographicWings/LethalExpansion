using UnityEngine;
using TMPro;
using System.Collections;
using LethalExpansion;

public class AutoScrollText : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public float scrollSpeed = 15f;
    private Vector2 startPosition;
    private float textHeight;
    private bool startScrolling = false;
    private bool isWaitingToReset = false;
    private float displayHeight;
    private float fontSize;

    void Start()
    {
        textMeshPro = this.GetComponent<TextMeshProUGUI>();
        InitializeScrolling();
    }
    private void InitializeScrolling()
    {
        if (textMeshPro != null)
        {
            startPosition = textMeshPro.rectTransform.anchoredPosition;
            textHeight = textMeshPro.preferredHeight;
            displayHeight = textMeshPro.rectTransform.sizeDelta.y;
            fontSize = textMeshPro.fontSize;
        }

        StartCoroutine(WaitBeforeScrolling(5f));
    }

    IEnumerator WaitBeforeScrolling(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        startScrolling = true;
    }

    IEnumerator WaitBeforeResetting(float waitTime)
    {
        isWaitingToReset = true;
        yield return new WaitForSeconds(waitTime);
        textMeshPro.rectTransform.anchoredPosition = startPosition;
        isWaitingToReset = false;
        StartCoroutine(WaitBeforeScrolling(5f));
    }

    void Update()
    {
        if (textMeshPro != null && startScrolling && !isWaitingToReset)
        {
            textMeshPro.rectTransform.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);

            if (textMeshPro.rectTransform.anchoredPosition.y >= startPosition.y + textHeight - displayHeight - fontSize)
            {
                startScrolling = false;
                StartCoroutine(WaitBeforeResetting(5f));
            }
        }
    }
    public void ResetScrolling()
    {
        StopAllCoroutines();
        if (textMeshPro != null)
        {
            textMeshPro.rectTransform.anchoredPosition = startPosition;
        }
        isWaitingToReset = false;
        InitializeScrolling();
    }
}
