using UnityEngine;

public partial class ParallaxController : MonoBehaviour
{
    [SerializeField] private Transform cinemaCam;

    private Vector3 lastCamPos; // Track the camera's position last frame
    private Material[] mats;
    private float[] backSpeed;
    private Vector2[] currentOffsets; // Store cumulative offset per layer

    [Range(0.001f, 0.5f)]
    public float parallaxSpeed = 0.02f;

    void Start()
    {
        if (cinemaCam == null) cinemaCam = Camera.main.transform;

        lastCamPos = cinemaCam.position;

        int backCount = transform.childCount;
        mats = new Material[backCount];
        backSpeed = new float[backCount];
        currentOffsets = new Vector2[backCount]; // Track cumulative movement

        float farthestBack = 0;

        for (int i = 0; i < backCount; i++)
        {
            float distance = transform.GetChild(i).position.z - cinemaCam.position.z;
            if (distance > farthestBack) farthestBack = distance;
        }

        for (int i = 0; i < backCount; i++)
        {
            Transform child = transform.GetChild(i);
            mats[i] = child.GetComponent<Renderer>().material;

            float distance = child.position.z - cinemaCam.position.z;
            // Farther = lower multiplier, Closer = higher
            backSpeed[i] = 1 - (distance / (farthestBack + 0.1f));
        }
    }

    private void LateUpdate()
    {
        Vector3 camMovement = cinemaCam.position - lastCamPos;

        transform.position = new Vector3(cinemaCam.position.x, cinemaCam.position.y, transform.position.z);

        for (int i = 0; i < mats.Length; i++)
        {
            float speed = backSpeed[i] * parallaxSpeed;

            // We add to the offset rather than recalculating from a start position
            currentOffsets[i].x += camMovement.x * speed;
            currentOffsets[i].y += camMovement.y * speed;

            mats[i].SetTextureOffset("_MainTex", currentOffsets[i]);
        }

        lastCamPos = cinemaCam.position;
    }
}