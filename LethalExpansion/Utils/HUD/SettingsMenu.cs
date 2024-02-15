using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using static UnityEngine.EventSystems.EventTrigger;

namespace LethalExpansion.Utils.HUD
{
    public class SettingsMenu
    {
        private static SettingsMenu _instance;
        private SettingsMenu() { }
        public static SettingsMenu Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SettingsMenu();
                }
                return _instance;
            }
        }
        private bool initialized = false;
        private List<HUDSettingEntry> entries = new List<HUDSettingEntry> ();
        private GameObject ModSettingsPanel;
        public void InitSettingsMenu()
        {
            entries = new List<HUDSettingEntry>();

            GameObject menuContainer = GameObject.Find("Canvas/MenuContainer");
            if (menuContainer == null)
            {
                LethalExpansion.Log.LogError("MenuContainer not found in the scene!");
                return;
            }
            GameObject ModSettings = null;
            if (!ConfigManager.Instance.FindItemValue<bool>("HideModSettingsMenu"))
            {
                menuContainer.transform.Find("MainButtons/HostButton").GetComponent<RectTransform>().anchoredPosition += new Vector2(0, 38.5f);
                menuContainer.transform.Find("MainButtons/JoinACrew").GetComponent<RectTransform>().anchoredPosition += new Vector2(0, 38.5f);
                menuContainer.transform.Find("MainButtons/StartLAN").GetComponent<RectTransform>().anchoredPosition += new Vector2(0, 38.5f);
                GameObject SettingsButton = menuContainer.transform.Find("MainButtons/SettingsButton").gameObject;
                SettingsButton.GetComponent<RectTransform>().anchoredPosition += new Vector2(0, 38.5f);
                ModSettings = GameObject.Instantiate(SettingsButton, menuContainer.transform);
                ModSettings.transform.SetParent(menuContainer.transform.Find("MainButtons"));
                ModSettings.name = "ModSettingsButton";
                ModSettings.transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = "> Mod Settings";
                ModSettings.GetComponent<RectTransform>().anchoredPosition = SettingsButton.GetComponent<RectTransform>().anchoredPosition - new Vector2(0, 38.5f);
            }

            ModSettingsPanel = GameObject.Instantiate(AssetBundlesManager.Instance.mainAssetBundle.LoadAsset<GameObject>("Assets/Mods/LethalExpansion/Prefabs/HUD/Settings/ModSettings.prefab"));
            GameObject ModSettingsEntry = AssetBundlesManager.Instance.mainAssetBundle.LoadAsset<GameObject>("Assets/Mods/LethalExpansion/Prefabs/HUD/Settings/SettingEntry.prefab");
            GameObject ModSettingsCategory = AssetBundlesManager.Instance.mainAssetBundle.LoadAsset<GameObject>("Assets/Mods/LethalExpansion/Prefabs/HUD/Settings/SettingCategory.prefab");
            ModSettingsPanel.transform.SetParent(menuContainer.transform);
            ModSettingsPanel.transform.localPosition = Vector3.zero;
            ModSettingsPanel.transform.localScale = Vector3.one;

            if (!ConfigManager.Instance.FindItemValue<bool>("HideModSettingsMenu") && ModSettings != null)
            {
                Button ModSettingsButton = ModSettings.GetComponent<Button>();
                ModSettingsButton.onClick = new Button.ButtonClickedEvent();
                ModSettingsButton.onClick.AddListener(() => {
                    if (!GetSettingsMenuActive())
                    {
                        ShowSettingsMenu();
                    }
                    else
                    {
                        HideSettingsMenu();
                    }
                });
            }


            SettingMenu_DragAndDrop ModSettingsDragAndDropSurface = ModSettingsPanel.transform.Find("DragAndDropSurface").gameObject.AddComponent<SettingMenu_DragAndDrop>();
            ModSettingsDragAndDropSurface.rectTransform = ModSettingsPanel.GetComponent<RectTransform>();
            Button ModSettingsCloseButton = ModSettingsPanel.transform.Find("CloseButton").GetComponent<Button>();
            Button ModSettingsApplyButton = ModSettingsPanel.transform.Find("ApplyButton").GetComponent<Button>();
            Button ModSettingsCancelButton = ModSettingsPanel.transform.Find("CancelButton").GetComponent<Button>();
            ModSettingsCloseButton.onClick.AddListener(() => { HideSettingsMenu(); });
            ModSettingsApplyButton.onClick.AddListener(() => { ApplySettings(); HideSettingsMenu(); });
            ModSettingsCancelButton.onClick.AddListener(() => { HideSettingsMenu(); });
            GameObject ModSettingsContent = ModSettingsPanel.transform.Find("Scroll View/Viewport/ModSettingsContent").gameObject;
            Button ModSettingsAllDefaultButton = ModSettingsContent.transform.Find("AllDefaultButton").GetComponent<Button>();
            ModSettingsAllDefaultButton.onClick.AddListener(() => { ResetAllSettings(); });
            RectTransform ModSettingsContentRectTransform = ModSettingsContent.GetComponent<RectTransform>();

            GameObject ModSettingsToolTipPanel = ModSettingsContent.transform.Find("TooltipPanel").gameObject;
            TooltipHandler.ModSettingsToolTipPanel = ModSettingsToolTipPanel.GetComponent<RectTransform>();
            TooltipHandler.ModSettingsToolTipPanelDescription = ModSettingsToolTipPanel.transform.Find("Description").GetComponent<TMP_Text>();
            TooltipHandler.ModSettingsToolTipPanelNetInfo = ModSettingsToolTipPanel.transform.Find("NetInfo").GetComponent<TMP_Text>();

            int y = -15;
            string lastCategory = string.Empty;

            foreach (var item in ConfigManager.Instance.GetAll())
            {
                if(item.Tab != lastCategory)
                {
                    GameObject category = GameObject.Instantiate(ModSettingsCategory, ModSettingsContent.transform);
                    category.transform.Find("Text").GetComponent<TMP_Text>().text = item.Tab + ':';
                    category.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100, y);
                    lastCategory = item.Tab;
                    y -= 20;
                }
                HUDSettingEntry entry = new HUDSettingEntry();

                entry.SettingsEntry = GameObject.Instantiate(ModSettingsEntry, ModSettingsContent.transform);
                entry.SettingsEntryKey = entry.SettingsEntry.transform.Find("Key").gameObject;
                entry.SettingsEntryKeyTextComponent = entry.SettingsEntryKey.GetComponent<TMP_Text>();
                entry.SettingsEntryKeyTextComponent.text = item.Key;
                entry.SettingsEntry.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100, y);

                entry.ValueObject = null;
                entry.ValueTypeName = Regex.Replace(item.type.Name, "([a-z])([A-Z])", "$1 $2");
                switch (entry.ValueTypeName)
                {
                    case "Int32":
                        entry.ValueObject = entry.SettingsEntry.transform.Find("Value/Slider").gameObject;
                        entry.ValueObjectSelectable1 = entry.ValueObject.GetComponent<Slider>();
                        entry.ValueObjectSelectable1.wholeNumbers = true;
                        entry.ValueObjectSelectable1.minValue = (int)item.MinValue;
                        entry.ValueObjectSelectable1.maxValue = (int)item.MaxValue;
                        entry.ValueObjectSelectable1Text = entry.ValueObjectSelectable1.transform.Find("Text").GetComponent<TMP_Text>();
                        entry.ValueObjectSelectable1.onValueChanged.AddListener((value) => { value = RoundToNearest((int)value, 1); entry.ValueObjectSelectable1Text.text = value.ToString(); });
                        entry.SettingsEntry.transform.Find("DefaultButton").GetComponent<Button>().onClick.AddListener(() => { entry.ValueObjectSelectable1.value = (int)item.DefaultValue; });
                        break;
                    case "Single":
                        entry.ValueObject = entry.SettingsEntry.transform.Find("Value/Slider").gameObject;
                        entry.ValueObjectSelectable2 = entry.ValueObject.GetComponent<Slider>();
                        entry.ValueObjectSelectable2.minValue = (float)item.MinValue;
                        entry.ValueObjectSelectable2.maxValue = (float)item.MaxValue;
                        entry.ValueObjectSelectable2Text = entry.ValueObjectSelectable2.transform.Find("Text").GetComponent<TMP_Text>();
                        entry.ValueObjectSelectable2.onValueChanged.AddListener((value) => { value = (float)Math.Round(RoundToNearest(value, 0.05f), 2); entry.ValueObjectSelectable2Text.text = value.ToString(); });
                        entry.SettingsEntry.transform.Find("DefaultButton").GetComponent<Button>().onClick.AddListener(() => { entry.ValueObjectSelectable2.value = (float)item.DefaultValue; });
                        break;
                    case "Boolean":
                        entry.ValueObject = entry.SettingsEntry.transform.Find("Value/Toggle").gameObject;
                        entry.ValueObjectSelectable3 = entry.ValueObject.GetComponent<Toggle>();
                        entry.SettingsEntry.transform.Find("DefaultButton").GetComponent<Button>().onClick.AddListener(() => { entry.ValueObjectSelectable3.isOn = (bool)item.DefaultValue; });
                        break;
                    case "String":
                        entry.ValueObject = entry.SettingsEntry.transform.Find("Value/InputField").gameObject;
                        entry.ValueObjectSelectable4 = entry.ValueObject.GetComponent<TMP_InputField>();
                        entry.SettingsEntry.transform.Find("DefaultButton").GetComponent<Button>().onClick.AddListener(() => { entry.ValueObjectSelectable4.text = (string)item.DefaultValue; });
                        break;
                    default:
                        break;
                }
                if (entry.ValueObject != null)
                {
                    entry.ValueObject.SetActive(true);
                }
                entries.Add(entry);

                y -= 20;
            }
            ModSettingsAllDefaultButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, y-5);
            //ModSettingsContentRectTransform.sizeDelta = new Vector2(380, Mathf.Abs(y-25));
            ModSettingsContentRectTransform.sizeDelta = new Vector2(380, Mathf.Abs(y-25-110));

            ModSettingsToolTipPanel.transform.SetAsLastSibling();
            initialized = true;
        }
        public void GetSettings()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].SettingsEntry.AddComponent<TooltipHandler>().index = i;
                switch (entries[i].ValueTypeName)
                {
                    case "Int32":
                        entries[i].ValueObjectSelectable1.value = (int)ConfigManager.Instance.FindEntryValue(i);
                        break;
                    case "Single":
                        entries[i].ValueObjectSelectable2.value = (float)ConfigManager.Instance.FindEntryValue(i);
                        break;
                    case "Boolean":
                        entries[i].ValueObjectSelectable3.isOn = (bool)ConfigManager.Instance.FindEntryValue(i);
                        break;
                    case "String":
                        entries[i].ValueObjectSelectable4.text = (string)ConfigManager.Instance.FindEntryValue(i);
                        break;
                    default:
                        break;
                }
            }
        }
        public void ApplySettings()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                switch (entries[i].ValueTypeName)
                {
                    case "Int32":
                        ConfigManager.Instance.SetEntryValue<int>(i, RoundToNearest((int)entries[i].ValueObjectSelectable1.value, 1));
                        break;
                    case "Single":
                        ConfigManager.Instance.SetEntryValue<float>(i, (float)Math.Round(RoundToNearest(entries[i].ValueObjectSelectable2.value, 0.05f), 2));
                        break;
                    case "Boolean":
                        ConfigManager.Instance.SetEntryValue<bool>(i, entries[i].ValueObjectSelectable3.isOn);
                        break;
                    case "String":
                        ConfigManager.Instance.SetEntryValue<string>(i, entries[i].ValueObjectSelectable4.text);
                        break;
                    default:
                        break;
                }
                if (!ConfigManager.Instance.MustBeSync(i))
                {
                    switch (entries[i].ValueTypeName)
                    {
                        case "Int32":
                            ConfigManager.Instance.SetItemValue<int>(i, RoundToNearest((int)entries[i].ValueObjectSelectable1.value, 1));
                            break;
                        case "Single":
                            ConfigManager.Instance.SetItemValue<float>(i, (float)Math.Round(RoundToNearest(entries[i].ValueObjectSelectable2.value, 0.05f), 2));
                            break;
                        case "Boolean":
                            ConfigManager.Instance.SetItemValue<bool>(i, entries[i].ValueObjectSelectable3.isOn);
                            break;
                        case "String":
                            ConfigManager.Instance.SetItemValue<string>(i, entries[i].ValueObjectSelectable4.text);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        public void ResetAllSettings()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                switch (entries[i].ValueTypeName)
                {
                    case "Int32":
                        entries[i].ValueObjectSelectable1.value = (int)ConfigManager.Instance.FindDefaultValue(i);
                        break;
                    case "Single":
                        entries[i].ValueObjectSelectable2.value = (float)ConfigManager.Instance.FindDefaultValue(i);
                        break;
                    case "Boolean":
                        entries[i].ValueObjectSelectable3.isOn = (bool)ConfigManager.Instance.FindDefaultValue(i);
                        break;
                    case "String":
                        entries[i].ValueObjectSelectable4.text = (string)ConfigManager.Instance.FindDefaultValue(i);
                        break;
                    default:
                        break;
                }
            }
        }
        private int DetermineIncrement(int min, int max)
        {
            int range = max - min;

            if (range <= 5000) return 1;
            else if (range <= 1000) return 50;
            else if (range <= 10000) return 500;
            else return 1000;
        }
        private float DetermineIncrement(float min, float max)
        {
            float range = max - min;

            if (range <= 50) return 1;
            else if (range <= 1000) return 50;
            else if (range <= 10000) return 500;
            else return 1000;
        }
        private int RoundToNearest(int number, int min, int max)
        {
            int increment = DetermineIncrement(min, max);
            return (int)(Math.Round((double)number / increment) * increment);
        }
        private float RoundToNearest(float number, float min, float max)
        {
            float increment = DetermineIncrement(min, max);
            return (float)(Math.Round(number / increment) * increment);
        }
        private int RoundToNearest(int number, int increment)
        {
            return (int)(Math.Round((double)number / increment) * increment);
        }
        private float RoundToNearest(float number, float increment)
        {
            return (float)(Math.Round(number / increment) * increment);
        }
        public void ShowSettingsMenu()
        {
            if (!ModSettingsPanel.activeSelf)
            {
                GetSettings();
                ModSettingsPanel.SetActive(true);
            }
        }
        public void HideSettingsMenu()
        {
            ModSettingsPanel.SetActive(false);
        }
        public bool GetSettingsMenuActive()
        {
            return ModSettingsPanel.activeSelf;
        }
    }
    class HUDSettingEntry
    {
        public GameObject SettingsEntry;
        public GameObject SettingsEntryKey;
        public TMP_Text SettingsEntryKeyTextComponent;
        public GameObject ValueObject;
        public String ValueTypeName;
        public Slider ValueObjectSelectable1;
        public TMP_Text ValueObjectSelectable1Text;
        public Slider ValueObjectSelectable2;
        public TMP_Text ValueObjectSelectable2Text;
        public Toggle ValueObjectSelectable3;
        public TMP_InputField ValueObjectSelectable4;
        public TooltipHandler TooltipHandler;
    }
}
