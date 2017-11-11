using UnityEngine;
using System.Collections;
using AssetBundles;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;

public class GamePatch : MonoBehaviour
{
    public string APP_VERSION;
    public string SERVER_VERSION;

    public string SERVER_VERSION_URL = "";
    public string SERVER_PATCH_URL = "";

    public Dictionary<string, AssetBundle> dictAssetBundleRefs = new Dictionary<string, AssetBundle>();

    public string[] assets = new string[]
    {
        "scenes",
        "sprites",
        "textures",
        "models"
    };

    public GameObject PatchProgress;
    public GameObject PatchDone;

    [Header("Progress Bar")]
    public UnityEngine.UI.Text progressBar;

    void Start()
    {
        TogglePatchStatus(true, false);
        CompareVersion();
        StartCoroutine(GetPatch(OnPactched));
    }

    private void TogglePatchStatus(bool isProgress, bool isDone)
    {
        PatchProgress.SetActive(isProgress);
        PatchDone.SetActive(isDone);
    }

    public void CompareVersion()
    {
        APP_VERSION = Application.version;
        SERVER_VERSION = "1.0";
    }

    private IEnumerator GetPatch(Action callback)
    {
        //Caching.ClearCache();

        int size = assets.Length;
        int currentResouces = 1;

        foreach (var asset in assets)
        {
            string url = Path.Combine(SERVER_PATCH_URL, asset);

            while (!Caching.ready)
                yield return null;

            using (WWW downloading = WWW.LoadFromCacheOrDownload(url, 0))
            {
                while (!downloading.isDone)
                {
                    progressBar.text = "Loading " + currentResouces + "/" + size + " " + String.Format("{0:F2}", downloading.progress * 100f) + "%";
                    yield return null;
                }

                progressBar.text = "Loading " + currentResouces + "/" +  size + " " + String.Format("{0:F2}", downloading.progress * 100f) + "%";

                yield return downloading;

                if (!string.IsNullOrEmpty(downloading.error))
                {
                    Debug.LogWarning(downloading.error);
                    continue;
                }

                AssetBundle assetBundle = downloading.assetBundle;

                dictAssetBundleRefs.Add(url, assetBundle);
            }

            yield return new WaitForSeconds(1f);

            currentResouces++;
        }

        callback();
    }

    private void OnPactched()
    {
        TogglePatchStatus(false, true);
    }

    public void OnClickStartGameButton()
    {
        string url1 = Path.Combine(SERVER_PATCH_URL, assets[0]);
        string[] scenes = dictAssetBundleRefs[url1].GetAllScenePaths();

        SceneManager.LoadScene(scenes[0]);
    }
}