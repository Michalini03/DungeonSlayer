using UnityEngine;

public partial class ParallaxController : MonoBehaviour
{
    [SerializeField] private Transform cam;
    private Vector2 camStartPos;

    private Material[] mats;
    private float[] backSpeed;

    [Range(0.001f, 0.1f)]
    public float parallaxSpeed = 0.02f;

    void Start()
    {
        // Fallback if cam isn't assigned in Inspector
        if (cam == null) cam = Camera.main.transform;

        camStartPos = cam.position;

        int backCount = transform.childCount;
        mats = new Material[backCount];
        backSpeed = new float[backCount];

        float farthestBack = 0;

        // First pass: Find the farthest distance
        for (int i = 0; i < backCount; i++)
        {
            float distance = transform.GetChild(i).position.z - cam.position.z;
            if (distance > farthestBack) farthestBack = distance;
        }

        // Second pass: Setup materials and relative speeds
        for (int i = 0; i < backCount; i++)
        {
            Transform child = transform.GetChild(i);
            mats[i] = child.GetComponent<Renderer>().material;

            float distance = child.position.z - cam.position.z;
            // Closer to camera = faster (approaches 1), Farther = slower (approaches 0)
            backSpeed[i] = 1 - (distance / (farthestBack + 0.1f));
        }
    }

    private void LateUpdate()
    {
        float distanceX = cam.position.x - camStartPos.x;
        float distanceY = cam.position.y - camStartPos.y; // Added Y support

        // Keep the container locked to the camera
        transform.position = new Vector3(cam.position.x, cam.position.y, transform.position.z);

        for (int i = 0; i < mats.Length; i++)
        {
            float speed = backSpeed[i] * parallaxSpeed;
            Vector2 offset = new Vector2(distanceX, distanceY) * speed;
            mats[i].SetTextureOffset("_MainTex", offset);
        }
    }
}