using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] GameObject respawnButton;

    InputAction pauseAction;
    bool paused;

    void Awake()
    {
        pauseAction = InputSystem.actions.FindAction("Pause");

        if(panel != null)
            panel.SetActive(false);
    }

    void Update()
    {
        if(pauseAction != null && pauseAction.WasPressedThisFrame())
            Toggle();
    }

    public void Toggle()
    {
        if(paused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        paused = true;

        Time.timeScale = 0f;

        if(panel != null)
            panel.SetActive(true);

        if(respawnButton != null)
            respawnButton.SetActive(GameManager.instance != null && GameManager.instance.IsRunActive);
    }

    public void Resume()
    {
        paused = false;
        Time.timeScale = 1f;

        if(panel != null)
            panel.SetActive(false);
    }


    public void OnResumePressed()
    {
        Resume();
    }

    public void OnRespawnPressed()
    {
        Resume();

        if(GameManager.instance != null)
            GameManager.instance.Respawn();
    }

    public void OnRestartPressed()
    {
        if(GameManager.instance != null)
            GameManager.instance.RestartGame();
    }
}