using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            Restart();
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


}
