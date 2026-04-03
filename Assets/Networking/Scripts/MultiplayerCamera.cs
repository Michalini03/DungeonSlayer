using UnityEngine;

/// <summary>
/// Camera controller that follows the midpoint between all registered players.
/// In single-player, follows the single player. In multiplayer, zooms out to fit both.
/// Attach to the main camera or a Cinemachine virtual camera.
/// </summary>
public class MultiplayerCamera : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -10f);

    [Header("Zoom Settings")]
    [SerializeField] private float minOrthoSize = 5f;
    [SerializeField] private float maxOrthoSize = 12f;
    [SerializeField] private float zoomPadding = 3f;

    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        var players = PlayerRegistry.Players;
        if (players.Count == 0) return;

        if (players.Count == 1)
        {
            if (players[0] == null) return;
            Vector3 targetPos = players[0].transform.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
            return;
        }

        // Multiple players: find midpoint and zoom to fit
        Vector3 centerPoint = GetCenterPoint();
        Vector3 newPos = centerPoint + offset;
        transform.position = Vector3.Lerp(transform.position, newPos, smoothSpeed * Time.deltaTime);

        if (cam != null && cam.orthographic)
        {
            float targetSize = GetRequiredOrthoSize();
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, smoothSpeed * Time.deltaTime);
        }
    }

    private Vector3 GetCenterPoint()
    {
        var players = PlayerRegistry.Players;
        if (players.Count == 1 && players[0] != null)
            return players[0].transform.position;

        var bounds = new Bounds(players[0].transform.position, Vector3.zero);
        for (int i = 1; i < players.Count; i++)
        {
            if (players[i] != null)
                bounds.Encapsulate(players[i].transform.position);
        }

        return bounds.center;
    }

    private float GetRequiredOrthoSize()
    {
        var players = PlayerRegistry.Players;
        if (players.Count < 2) return minOrthoSize;

        var bounds = new Bounds(players[0].transform.position, Vector3.zero);
        for (int i = 1; i < players.Count; i++)
        {
            if (players[i] != null)
                bounds.Encapsulate(players[i].transform.position);
        }

        float sizeX = bounds.size.x / cam.aspect / 2f + zoomPadding;
        float sizeY = bounds.size.y / 2f + zoomPadding;

        return Mathf.Clamp(Mathf.Max(sizeX, sizeY), minOrthoSize, maxOrthoSize);
    }
}
