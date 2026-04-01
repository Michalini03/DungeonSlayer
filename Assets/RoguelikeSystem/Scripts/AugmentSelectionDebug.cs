using UnityEngine;
using UnityEngine.InputSystem;

public class AugmentSelectionDebug : MonoBehaviour
{
    [SerializeField] private AugmentSelectionUI augmentSelectionUI;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
        {
            if (augmentSelectionUI.gameObject.activeSelf)
                augmentSelectionUI.Hide();
            else
                augmentSelectionUI.ShowSelection();
        }
    }
}