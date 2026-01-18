using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SettingsController : MonoBehaviour
{
    public static SettingsController main;
    public static List<SoundController> soundControllers = new();

    public static ChangeableValue generalVolume = new(.5f);
    public static ChangeableValue musicVolume = new(.5f);
    public static ChangeableValue SFXvolume = new(.5f);

    //public static Action onDestroy;
    [SerializeField]
    private SettingsSlider[] settingsSliders;

    private static EventSystem otherEvent;
    private static Scene thisScene;
    private static GameObject otherObject;
    private static string settingsSceneName = "SettingsMenu";

    public static void AddScene(EventSystem eventSystem, GameObject disableObject)
    {
        eventSystem.enabled = false;
        otherEvent = eventSystem;
        disableObject.SetActive(false);
        otherObject = disableObject;

        LoadSceneParameters param = new(LoadSceneMode.Additive, LocalPhysicsMode.None);
        thisScene = SceneManager.LoadScene(settingsSceneName, param);

    }

    private void Start()
    {
        main = this;

        settingsSliders[0].Set(generalVolume);
        settingsSliders[1].Set(musicVolume);
        settingsSliders[2].Set(SFXvolume);
    }
    public void Apply()
    {
        foreach (SoundController sc in soundControllers)
            sc.SetVolumes(generalVolume.value, SFXvolume.value, musicVolume.value);
    }


    public void Return()
    {
        Apply();

        main = null;

        otherEvent.enabled = true;
        otherObject.SetActive(true);

        otherEvent = null;
        otherObject = null;

        SceneManager.UnloadSceneAsync(thisScene);
    }
}
[Serializable]
public class ChangeableValue
{
    [Range(0f, 1f)] public float value = .5f;
    public ChangeableValue(float v)
    {
        value = v;
    }
}
