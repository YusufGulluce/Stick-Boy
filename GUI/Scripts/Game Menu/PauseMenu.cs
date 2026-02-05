using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PauseMenu : MonoBehaviour
{
    [Header("General")]
    public static PauseMenu main;

    [SerializeField]
    private Camera cam;
    [SerializeField]
    private GameObject menu;
    [SerializeField]
    private EventSystem eventSystem;
    [SerializeField]
    private KeyCode menuKey;

    [Space]

    [Header("Pause Parameters")]
    [SerializeField]
    private Vector2 pauseCameraOffset;
    [SerializeField]
    private float pauseSize;
    [SerializeField]
    private float pauseZoomSpeed = .1f;

    [Space]

    [Header("Pause Text")]
    [SerializeField]
    private Text pauseText;
    [SerializeField]
    private Text cantResume;
    [SerializeField]
    private float textPeriod;

    [Space]

    [Header("Win Screen")]
    [SerializeField]
    private GameObject winScreen;

    [Space]

    [Header("Restart Screen")]
    [SerializeField]
    private Text restartingText;
    [SerializeField]
    private Text shouldRestartText;

    private static Vector3 lastCamPos = Vector3.zero;


    public static bool isPaused = false;
    public static bool canResume = true;

    public static void SetResume(bool r)
    {
        canResume = r;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        //Debug.Log("hey");
        main = this;
        Cursor.lockState = CursorLockMode.Locked;
        isPaused = false;
        canResume = true;

        restartingText.enabled = false;
        shouldRestartText.gameObject.SetActive(false);

        foreach (List<Folded.IFoldEffected> onFold in Folded.IFoldEffected.onFolds)
            onFold.Clear();
        Folded.IPauseEffected.all.Clear();
        Folded.Core.IInteractable.lastInteractable.Clear();

    }
    private void OnDestroy()
    {
        if(main == this)
            main = null;
    }
    private void Update()
    {
        if (Input.GetKeyDown(menuKey) && !isPaused)
            Pause();
        else if (Input.GetKeyDown(menuKey) && isPaused)
            Unpause();
        PauseText();
    }

    private void Pause()
    {
        Player.main.sr.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
        Cursor.lockState = CursorLockMode.None;
        isPaused = true;
        Time.timeScale = 0f;
        menu.SetActive(true);

        cam.gameObject.AddComponent<PauseCamera>().Set(pauseCameraOffset, pauseSize, pauseZoomSpeed);
        cam.GetComponent<Folded.GUI.DeskCam>().Enable();
        if (FoldController.pages != null)
            foreach (FoldController page in FoldController.pages)
                page.EditMode(true);

        foreach (Folded.IPauseEffected item in Folded.IPauseEffected.all)
            item.Pause();
    }

    private void Unpause()
    {
        if(canResume)
        {
            cam.GetComponent<Folded.GUI.DeskCam>().AdjustTo(Vector3.forward, .2f);
            Player.main.sr.maskInteraction = SpriteMaskInteraction.None;

            Cursor.lockState = CursorLockMode.Locked;
            isPaused = false;
            Time.timeScale = 1f;

            Destroy(cam.GetComponent<PauseCamera>());
            CameraFollow.main.RePosition();

            if (FoldController.pages != null)
                foreach (FoldController page in FoldController.pages)
                    page.EditMode(false);
            menu.SetActive(false);

            foreach (Folded.IPauseEffected item in Folded.IPauseEffected.all)
                item.Resume();
        }
    }

    public void ActResume()
    {
        Unpause();
    }

    public void ActSettings()
    {
        SettingsController.AddScene(eventSystem, menu);
    }
    public void ActQuit()
    {
        //Debug.Log("Quiting..");
        SceneManager.LoadScene(0);
    }


    private void PauseText()
    {
        pauseText.enabled = Time.unscaledTime % textPeriod > textPeriod * .5f;

        shouldRestartText.enabled = pauseText.enabled;

        cantResume.enabled = !canResume;
    }

    public void ShouldResetText(bool status)
    {
        shouldRestartText.gameObject.SetActive(status);
    }

    public void ResetingText(bool status, float time)
    {
        if (status)
        {
            restartingText.enabled = true;
            StartCoroutine(ResetingTextChange(time));
        }
        else
            restartingText.enabled = false;
    }

    IEnumerator ResetingTextChange(float time)
    {
        restartingText.text = "Restarting   ";
        yield return new WaitForSeconds(time * .25f);
        restartingText.text = "Restarting.  ";
        yield return new WaitForSeconds(time * .25f);
        restartingText.text = "Restarting.. ";
        yield return new WaitForSeconds(time * .25f);
        restartingText.text = "Restarting...";
    }

    public void Reset()
    {
        lastCamPos = cam.transform.position;

        LoadSceneMode mode = LoadSceneMode.Single;
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex, new LoadSceneParameters(mode));
        SceneManager.sceneLoaded += AdjustCamera;
    }

    private void AdjustCamera(Scene scene, LoadSceneMode mode)
    {
        foreach(GameObject gameObject in scene.GetRootGameObjects())
            if(gameObject.CompareTag("MainCamera"))
            {
                gameObject.transform.position = lastCamPos;
                return;
            }
    }

    // Win Screen

    public void WinScreenPop()
    {
        Cursor.lockState = CursorLockMode.None;
        isPaused = true;
        Time.timeScale = 0f;
        winScreen.SetActive(true);
    }

    public void ActRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}


namespace Folded
{
    public interface IPauseEffected
    {
        public static List<IPauseEffected> all = new();

        public void Login()
        {
            all.Add(this);
        }
        public void Logout()
        {
            all.Remove(this);
        }

        public void Pause();
        public void Resume();
    }
}