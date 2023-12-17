using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using LethalExpansion.Utils;
using UnityEngine.Events;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;

namespace LethalExpansion.Utils
{
    public class VersionChecker
    {
        public static async Task CheckVersion()
        {
            const string url = "https://raw.githubusercontent.com/HolographicWings/LethalExpansion/main/last.txt";

            using (var httpClient = new HttpClient())
            {
                try
                {
                    string onlineVersion = await httpClient.GetStringAsync(url);
                    CompareVersions(onlineVersion);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("Erreur lors de la récupération de la version : " + e.Message);
                }
            }
        }

        private static void CompareVersions(string onlineVersion)
        {
            if (LethalExpansion.ModVersion < Version.Parse(onlineVersion))
            {
                PopupManager.Instance.InstantiatePopup(SceneManager.GetSceneByName("MainMenu"), "Update", "Lethal Expansion is not up to date " + onlineVersion, "Update", "Ignore", new UnityAction(() => { Application.OpenURL("https://thunderstore.io/c/lethal-company/p/HolographicWings/LethalExpansion/"); }));
            }
        }
    }
}