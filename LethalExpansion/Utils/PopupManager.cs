using LethalExpansion.Utils.HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LethalExpansion.Utils
{
    public class PopupManager
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
        public void InstantiatePopup(Scene sceneFocus, string title = "Popup", string content = "", string button1 = "Ok", string button2 = "Cancel", UnityAction button1Action = null, UnityAction button2Action = null, int titlesize = 24, int contentsize = 24, int button1size = 24, int button2size = 24)
        {
            if(sceneFocus != null && sceneFocus.isLoaded)
            {
                GameObject[] rootObjects = sceneFocus.GetRootGameObjects();
                Canvas canvas = null;
                foreach (GameObject obj in rootObjects)
                {
                    canvas = obj.GetComponentInChildren<Canvas>();
                    if (canvas != null)
                    {
                        break;
                    }
                }
                if (canvas == null || canvas.gameObject.scene != sceneFocus)
                {
                    var canvasObject = new GameObject();
                    canvasObject.name = "Canvas";
                    canvas = canvasObject.AddComponent<Canvas>();
                    SceneManager.MoveGameObjectToScene(canvas.gameObject, sceneFocus);
                }
                var _tmp = GameObject.Instantiate(AssetBundlesManager.Instance.mainAssetBundle.LoadAsset<GameObject>("Assets/Mods/LethalExpansion/Prefabs/HUD/Popup.prefab"), canvas.transform);
                if (_tmp != null)
                {
                    _tmp.transform.Find("DragAndDropSurface").gameObject.AddComponent<SettingMenu_DragAndDrop>().rectTransform = _tmp.GetComponent<RectTransform>();
                    _tmp.transform.Find("CloseButton").gameObject.GetComponent<Button>().onClick.AddListener(() => { GameObject.Destroy(_tmp); });
                    if (button1Action != null)
                    {
                        _tmp.transform.Find("Button1").gameObject.GetComponent<Button>().onClick.AddListener(button1Action);
                    }
                    if (button2Action != null)
                    {
                        _tmp.transform.Find("Button2").gameObject.GetComponent<Button>().onClick.AddListener(button2Action);
                    }
                    _tmp.transform.Find("Button1").gameObject.GetComponent<Button>().onClick.AddListener(() => { GameObject.Destroy(_tmp); });
                    _tmp.transform.Find("Button2").gameObject.GetComponent<Button>().onClick.AddListener(() => { GameObject.Destroy(_tmp); });
                    PopupObject _instance = new PopupObject(_tmp, _tmp.transform.Find("Title").GetComponent<TMP_Text>(), _tmp.transform.Find("Panel/MainContent").GetComponent<TMP_Text>(), _tmp.transform.Find("Button1/Text").GetComponent<TMP_Text>(), _tmp.transform.Find("Button2/Text").GetComponent<TMP_Text>());
                    _instance.title.text = title;
                    _instance.title.fontSize = titlesize;
                    _instance.content.text = content;
                    _instance.content.fontSize = contentsize;
                    _instance.button1.text = button1;
                    _instance.button1.fontSize = button1size;
                    _instance.button2.text = button2;
                    _instance.button2.fontSize = button2size;
                    popups.Add(_instance);
                }
            }
        }
    }
    public class PopupObject
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
