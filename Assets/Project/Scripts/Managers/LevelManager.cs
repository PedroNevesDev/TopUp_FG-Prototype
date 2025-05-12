using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{
    public GameObject pausePannel;
    bool isPaused=false;
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            Restart();
        }   

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
    public void Restart()
    {
        TogglePause(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void TogglePause(bool state)
    {
        Time.timeScale = state?0:1;
    }

    public void ChangeScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused?0:1;
        pausePannel.SetActive(isPaused);
    }

}
