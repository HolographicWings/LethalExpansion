using LethalExpansion.Utils.HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LethalExpansion.Utils
{
    internal class PopupManager
    {
        private static PopupManager _instance;
        private PopupManager() { }
        public static PopupManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PopupManager();
                }
                return _instance;
            }
        }
        List<PopupObject> popups = new List<PopupObject>();
        public void InstantiatePopup(string title = "Popup", string content = "", string button1 = "Ok", string button2 = "Cancel", UnityAction button1Action = null, UnityAction button2Action = null, bool button1Destroy = true, bool button2Destroy = true, bool dontDestroyOnLoad = false)
        {
            var canvas = GameObject.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasObject = new GameObject();
                canvasObject.name = "Canvas";
                canvas = canvasObject.AddComponent<Canvas>();
            }
            var _tmp = GameObject.Instantiate(AssetBundlesManager.Instance.mainAssetBundle.LoadAsset<GameObject>("Assets/Mods/LethalExpansion/Prefabs/HUD/Popup.prefab"), canvas.transform);
            if(_tmp != null)
            {
                _tmp.transform.Find("DragAndDropSurface").gameObject.AddComponent<SettingMenu_DragAndDrop>().rectTransform = _tmp.GetComponent<RectTransform>();
                _tmp.transform.Find("CloseButton").gameObject.GetComponent<Button>().onClick.AddListener(() => { GameObject.DestroyImmediate(_tmp); });
                if(button1Action != null)
                {
                    _tmp.transform.Find("Button1").gameObject.GetComponent<Button>().onClick.AddListener(button1Action);
                }
                if (button2Action != null)
                {
                    _tmp.transform.Find("Button2").gameObject.GetComponent<Button>().onClick.AddListener(button2Action);
                }
                if (button1Destroy)
                {
                    _tmp.transform.Find("Button1").gameObject.GetComponent<Button>().onClick.AddListener(() => { GameObject.DestroyImmediate(_tmp); });
                }
                if(button2Destroy)
                {
                    _tmp.transform.Find("Button2").gameObject.GetComponent<Button>().onClick.AddListener(() => { GameObject.DestroyImmediate(_tmp); });
                }
                if (dontDestroyOnLoad)
                {
                    GameObject.DontDestroyOnLoad(_tmp);
                }
                PopupObject _instance = new PopupObject(_tmp, _tmp.transform.Find("Title").GetComponent<TMP_Text>(), _tmp.transform.Find("Panel/MainContent").GetComponent<TMP_Text>(), _tmp.transform.Find("Button1/Text").GetComponent<TMP_Text>(), _tmp.transform.Find("Button2/Text").GetComponent<TMP_Text>());
                _instance.title.text = title;
                _instance.content.text = content;
                _instance.button1.text = button1;
                _instance.button2.text = button2;
                popups.Add(_instance);
            }
        }
    }
    internal class PopupObject
    {
        public GameObject baseObject;
        public TMP_Text title;
        public TMP_Text content;
        public TMP_Text button1;
        public TMP_Text button2;
        public PopupObject(GameObject baseObject, TMP_Text title, TMP_Text content, TMP_Text button1, TMP_Text button2)
        {
            this.baseObject = baseObject;
            this.title = title;
            this.content = content;
            this.button1 = button1;
            this.button2 = button2;
        }
    }

}
