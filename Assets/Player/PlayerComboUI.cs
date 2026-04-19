using UnityEngine;

public class PlayerComboBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AttributesController attributes;
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2f, 0f);

    [Header("Bar")]
    [SerializeField] private RectTransform root;
    [SerializeField] private RectTransform fillArea;
    [SerializeField] private RectTransform redLeft;
    [SerializeField] private RectTransform yellowLeft;
    [SerializeField] private RectTransform greenCenter;
    [SerializeField] private RectTransform yellowRight;
    [SerializeField] private RectTransform redRight;

    [Header("Overlay")]
    [SerializeField] private RectTransform marker;
    [SerializeField] private float markerPadding = 0f;

    [Header("World Space")]
    [SerializeField] private Canvas worldCanvas;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Start()
    {
        if (attributes != null)
        {
            RebuildFromComboBar(attributes.comboBar);
        }

        HideBar();
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + worldOffset;
        }

        if (worldCanvas != null && worldCanvas.renderMode == RenderMode.WorldSpace && mainCamera != null)
        {
            transform.forward = mainCamera.transform.forward;
        }
    }

    public void ResetBar(int[] comboBar)
    {
        RebuildFromComboBar(comboBar);
        SetMarkerPosition(0f);
        ShowBar();
    }

    public void RebuildFromComboBar(int[] comboBar)
    {
        if (comboBar == null || comboBar.Length < 3 || fillArea == null)
        {
            return;
        }

        float redValue = comboBar[0];
        float yellowValue = comboBar[1];
        float greenValue = comboBar[2];

        float totalUnits = redValue * 2f + yellowValue * 2f + greenValue;
        if (totalUnits <= 0f)
        {
            return;
        }

        float totalWidth = fillArea.rect.width;

        float redWidth = totalWidth * (redValue / totalUnits);
        float yellowWidth = totalWidth * (yellowValue / totalUnits);
        float greenWidth = totalWidth * (greenValue / totalUnits);

        float leftEdge = -totalWidth * 0.5f;
        float x = leftEdge;

        SetSection(redLeft, x, redWidth);
        x += redWidth;

        SetSection(yellowLeft, x, yellowWidth);
        x += yellowWidth;

        SetSection(greenCenter, x, greenWidth);
        x += greenWidth;

        SetSection(yellowRight, x, yellowWidth);
        x += yellowWidth;

        SetSection(redRight, x, redWidth);
    }

    public void SetMarkerPosition(float normalized)
    {
        if (fillArea == null || marker == null)
        {
            return;
        }

        normalized = Mathf.Clamp01(normalized);

        float width = fillArea.rect.width;
        float left = -width * 0.5f + markerPadding;
        float right = width * 0.5f - markerPadding;

        float x = Mathf.Lerp(left, right, normalized);

        Vector2 pos = marker.anchoredPosition;
        pos.x = x;
        marker.anchoredPosition = pos;
    }

    public void ShowBar()
    {
        if (root != null)
        {
            root.gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }

    }

    public void HideBar()
    {
        if (root != null)
        {
            root.gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void SetSection(RectTransform rect, float leftX, float width)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);

        rect.anchoredPosition = new Vector2(leftX, 0f);
        rect.sizeDelta = new Vector2(width, rect.sizeDelta.y);
    }
}