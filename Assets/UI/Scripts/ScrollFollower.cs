using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ScrollFollower : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;
    [SerializeField] private Selectable firstSelected;
    [SerializeField] private float scrollSpeed = 10f;
    [SerializeField] private float mouseScrollBlockDuration = 0.2f;

    private float lastMouseScrollTime = -999f;

    private void OnEnable()
    {
        StartCoroutine(SelectFirstNextFrame());
    }

    private IEnumerator SelectFirstNextFrame()
    {
        yield return null;

        if (EventSystem.current != null && firstSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);
        }
    }

    private void Update()
    {
        TrackMouseScroll();

        if (Time.unscaledTime - lastMouseScrollTime < mouseScrollBlockDuration)
        {
            return;
        }

        if (EventSystem.current == null || scrollRect == null || viewport == null || content == null)
        {
            return;
        }

        GameObject selected = EventSystem.current.currentSelectedGameObject;

        if (selected == null)
        {
            return;
        }

        RectTransform selectedRect = selected.GetComponent<RectTransform>();

        if (selectedRect == null)
        {
            return;
        }

        if (!selectedRect.IsChildOf(content))
        {
            return;
        }

        ScrollToVisible(selectedRect);
    }

    private void TrackMouseScroll()
    {
        if (Mouse.current == null)
        {
            return;
        }

        Vector2 scroll = Mouse.current.scroll.ReadValue();

        if (Mathf.Abs(scroll.x) > 0.01f || Mathf.Abs(scroll.y) > 0.01f)
        {
            lastMouseScrollTime = Time.unscaledTime;
        }
    }

    private void ScrollToVisible(RectTransform target)
    {
        Canvas.ForceUpdateCanvases();

        Vector3[] viewportCorners = new Vector3[4];
        Vector3[] targetCorners = new Vector3[4];

        viewport.GetWorldCorners(viewportCorners);
        target.GetWorldCorners(targetCorners);

        if (scrollRect.horizontal)
        {
            HandleHorizontalScroll(viewportCorners, targetCorners);
        }

        if (scrollRect.vertical)
        {
            HandleVerticalScroll(viewportCorners, targetCorners);
        }
    }

    private void HandleHorizontalScroll(Vector3[] viewportCorners, Vector3[] targetCorners)
    {
        float viewportLeft = viewportCorners[0].x;
        float viewportRight = viewportCorners[3].x;

        float targetLeft = targetCorners[0].x;
        float targetRight = targetCorners[3].x;

        float offset = 0f;

        if (targetLeft < viewportLeft)
        {
            offset = targetLeft - viewportLeft;
        }
        else if (targetRight > viewportRight)
        {
            offset = targetRight - viewportRight;
        }

        if (Mathf.Abs(offset) < 0.01f)
        {
            return;
        }

        float scrollableWidth = content.rect.width - viewport.rect.width;

        if (scrollableWidth <= 0f)
        {
            return;
        }

        float normalizedOffset = offset / scrollableWidth;
        float targetScroll = scrollRect.horizontalNormalizedPosition + normalizedOffset;

        scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(Mathf.Lerp(scrollRect.horizontalNormalizedPosition, targetScroll, Time.unscaledDeltaTime * scrollSpeed));
    }

    private void HandleVerticalScroll(Vector3[] viewportCorners, Vector3[] targetCorners)
    {
        float viewportTop = viewportCorners[1].y;
        float viewportBottom = viewportCorners[0].y;

        float targetTop = targetCorners[1].y;
        float targetBottom = targetCorners[0].y;

        float offset = 0f;

        if (targetTop > viewportTop)
        {
            offset = targetTop - viewportTop;
        }
        else if (targetBottom < viewportBottom)
        {
            offset = targetBottom - viewportBottom;
        }

        if (Mathf.Abs(offset) < 0.01f)
        {
            return;
        }

        float scrollableHeight = content.rect.height - viewport.rect.height;

        if (scrollableHeight <= 0f)
        {
            return;
        }

        float normalizedOffset = offset / scrollableHeight;
        float targetScroll = scrollRect.verticalNormalizedPosition + normalizedOffset;

        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(Mathf.Lerp(scrollRect.verticalNormalizedPosition, targetScroll, Time.unscaledDeltaTime * scrollSpeed));
    }
}