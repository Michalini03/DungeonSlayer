using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class DeathScreen : MonoBehaviour
{
    [SerializeField] private Button defaultSelectedButton;

    private void OnEnable()
    {
        StartCoroutine(SelectDefaultNextFrame());
    }

    private IEnumerator SelectDefaultNextFrame()
    {
        yield return null;

        if (EventSystem.current == null || defaultSelectedButton == null)
            yield break;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(defaultSelectedButton.gameObject);
    }

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