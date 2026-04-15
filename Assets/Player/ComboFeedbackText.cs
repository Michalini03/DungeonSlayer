using UnityEngine;
using TMPro; // Required for TextMeshPro

public class ComboFeedbackText : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float fadeSpeed = 2f;
    public float lifetime = 1f;

    private TextMeshPro textMesh;
    private Color textColor;

    // We will call this from PlayerCombat to set the word and color
    public void Setup(string text, Color color)
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh != null)
        {
            textMesh.text = text;
            textMesh.color = color;
            textColor = color;
        }

        // Automatically destroy this GameObject after 'lifetime' seconds
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Float upwards
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // Fade out by reducing the alpha channel
        textColor.a -= fadeSpeed * Time.deltaTime;

        if (textMesh != null)
        {
            textMesh.color = textColor;
        }
    }
}