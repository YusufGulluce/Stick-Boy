using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField]
    private GameObject menuPanel;
    [SerializeField]
    private string settingsSceneName;
    [SerializeField]
    private string playSceneName;
    [SerializeField]
    private EventSystem eventSystem;

    private Scene settingsScene;

    private void Start()
    {
        
    }

    public void PlayButton()
    {
        //SceneManager.LoadScene(playSceneName, LoadSceneMode.Single);
        Folded.GUI.FoldAnimation.FoldToScene(Vector2.zero, Vector2.one, Vector2.right, 2f, playSceneName);
    }
    public void SettingsButton()
    {
        //menuPanel.SetActive(false);

        //LoadSceneParameters param = new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.None);
        //settingsScene = SceneManager.LoadScene(settingsSceneName, param);
        //SettingsController.onDestroy = ReturnFromSettings;
        //eventSystem.enabled = false;
        SettingsController.AddScene(eventSystem, menuPanel);

    }
    public void ExitButton()
    {

    }

    //public void ReturnFromSettings()
    //{
    //    SceneManager.UnloadSceneAsync(settingsScene);
    //    menuPanel.SetActive(true);
    //    eventSystem.enabled = true;
    //}



}
