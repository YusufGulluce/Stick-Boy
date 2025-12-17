using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu main;
    [SerializeField]
    private GameObject menu;
    [SerializeField]
    private EventSystem eventSystem;
    [SerializeField]
    private KeyCode menuKey;

    [Header("Pause Text")]
    [SerializeField]
    private Text pauseText;
    [SerializeField]
    private Text cantResume;
    [SerializeField]
    private float textPeriod;

    [Header("Win Screen")]
    [SerializeField]
    private GameObject winScreen;


    public static bool isPaused = false;
    public static bool canResume = true;

    public static void SetResume(bool r)
    {
        canResume = r;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        main = this;
        Cursor.lockState = CursorLockMode.Locked;
        isPaused = false;
        canResume = true;
    }
    private void OnDestroy()
    {
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
        Cursor.lockState = CursorLockMode.None;
        isPaused = true;
        Time.timeScale = 0f;
        menu.SetActive(true);

        Camera.main.gameObject.AddComponent<PauseCamera>().Set(Vector3.zero);

        if (FoldController.pages != null)
            foreach (FoldController page in FoldController.pages)
                page.EditMode(true);
    }

    private void Unpause()
    {
        if(canResume)
        {
            Cursor.lockState = CursorLockMode.Locked;
            isPaused = false;
            Time.timeScale = 1f;

            Destroy(Camera.main.GetComponent<PauseCamera>());
            CameraFollow.main.RePosition();

            if (FoldController.pages != null)
                foreach (FoldController page in FoldController.pages)
                    page.EditMode(false);
            menu.SetActive(false);
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
        cantResume.enabled = !canResume;
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
