using LethalExpansion;
using LethalExpansion.Utils;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private string description;
    private string netinfo;
    public static RectTransform ModSettingsToolTipPanel;
    public static TMP_Text ModSettingsToolTipPanelDescription;
    public static TMP_Text ModSettingsToolTipPanelNetInfo;
    public int index;
    private float delay = 0.5f;
    private bool isPointerOver = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        StartCoroutine(ShowTooltipAfterDelay());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        ModSettingsToolTipPanel.gameObject.SetActive(false);
    }
    private IEnumerator ShowTooltipAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        if (isPointerOver)
        {
            ModSettingsToolTipPanel.anchoredPosition = new Vector2(-30, this.GetComponent<RectTransform>().anchoredPosition.y + 180);

            description = ConfigManager.Instance.FindDescription(index);
            (bool, bool) info = ConfigManager.Instance.FindNetInfo(index);
            netinfo = "Network synchronization: " + (info.Item1 ? "Yes" : "No") + "\nMod required by clients: " + (info.Item2 ? "No" : "Yes");

            ModSettingsToolTipPanelDescription.text = description;
            ModSettingsToolTipPanelNetInfo.text = netinfo;
            ModSettingsToolTipPanel.gameObject.SetActive(true);
        }
    }
}