using UnityEngine;
using System.Collections;

public class SoundController : MonoBehaviour
{
    [SerializeField]
    private AudioSource[] musicSources;
    [SerializeField]
    private AudioSource[] sfxSources;

    private void Start()
    {
        SettingsController.soundControllers.Add(this);
        SetVolumes
            (
                SettingsController.generalVolume.value,
                SettingsController.SFXvolume.value,
                SettingsController.musicVolume.value
            );
    }
    private void OnDestroy()
    {
        SettingsController.soundControllers.Remove(this);
    }

    public void SetVolumes(float generalVolume, float sfxVolume, float musicVolume)
    {
        SetSFX(generalVolume, sfxVolume);
        SetMusic(generalVolume, musicVolume);
    }

    public void SetSFX(float generalVolume, float sfxVolume)
    {
        foreach (AudioSource audioSource in sfxSources)
            audioSource.volume *= generalVolume * sfxVolume;
    }
    public void SetMusic(float generalVolume, float musicVolume)
    {
        foreach (AudioSource audioSource in musicSources)
            audioSource.volume *= generalVolume * musicVolume;
    }
}
