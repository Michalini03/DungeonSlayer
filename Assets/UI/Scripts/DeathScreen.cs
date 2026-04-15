using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreen : MonoBehaviour
{
    public void GoToMainMenu()
    {
        if (RunController.Instance != null)
        {
            RunController.Instance.EndRun();
        }

        else
        {
            SaveSystem.DeleteRun();
        }

        SceneManager.LoadScene("MainMenu");
    }
}