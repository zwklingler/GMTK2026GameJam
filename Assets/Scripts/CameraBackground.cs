using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraBackground : MonoBehaviour
{
    [SerializeField] private Transform rocket;
    [SerializeField] private Color skyColor   = new Color(0.3f, 0.6f, 1f);
    [SerializeField] private Color spaceColor  = Color.black;
    [SerializeField] private float spaceHeight = 200f;

    [Header("Stars")]
    [Tooltip("Sprite for the starry sky")]
    [SerializeField] private SpriteRenderer stars;

    [SerializeField] private float starsDistance = 150f;

    [Tooltip("Height where stars begin to appear")]
    [SerializeField] private float starsStartHeight = 60f;

    [Tooltip("Height where stars are fully visible")]
    [SerializeField] private float starsFullHeight = 250f;

    [Tooltip("Constant star movement")]
    [SerializeField] private float starsScrollSpeed = 0.4f;

    [Tooltip("Extra movement in relation to the rocket's height")]
    [SerializeField] private float starsParallax = 0.08f;

    [Tooltip("Snap scrolling to pixels")]
    [SerializeField] private bool snapStarsToPixels = true;

    private Camera cam;

    private float lastAspect = -1f;
    private float lastOrthoSize = -1f;
    private float lastDistance = -1f;

    private float starsScroll;

    private float starsWorldHeight;

    void Awake()
    {
        cam = GetComponent<Camera>();

        if (stars != null)
        {
            stars.drawMode = SpriteDrawMode.Tiled;
            stars.tileMode = SpriteTileMode.Continuous;
            stars.transform.localScale = Vector3.one;
        }
    }

    void Update()
    {
        float t = Mathf.Clamp01(rocket.position.y / spaceHeight);
        cam.backgroundColor = Color.Lerp(skyColor, spaceColor, t);

        UpdateStars();
    }

    void UpdateStars()
    {
        if (stars == null)
            return;

        FitToCamera();

        float alpha = Mathf.InverseLerp(starsStartHeight, starsFullHeight, rocket.position.y);

        Color color = stars.color;
        color.a = alpha;
        stars.color = color;

        ScrollStars();
    }

    void ScrollStars()
    {
        if (stars.sprite == null)
            return;

        starsScroll += starsScrollSpeed * Time.deltaTime;

        float totalScroll = starsScroll + rocket.position.y * starsParallax;

        float tileHeight = stars.sprite.bounds.size.y;
        float offsetY = -Mathf.Repeat(totalScroll, tileHeight) + tileHeight * 0.5f;

        if (snapStarsToPixels && starsWorldHeight > 0f && Screen.height > 0)
        {
            float unitsPerPixel = starsWorldHeight / Screen.height;
            offsetY = Mathf.Round(offsetY / unitsPerPixel) * unitsPerPixel;
        }

        stars.transform.localPosition = new Vector3(0f, offsetY, starsDistance);
    }

    void FitToCamera()
    {
        if (Mathf.Approximately(cam.aspect, lastAspect) && Mathf.Approximately(cam.orthographicSize, lastOrthoSize) && Mathf.Approximately(starsDistance, lastDistance))
            return;

        lastAspect = cam.aspect;
        lastOrthoSize = cam.orthographicSize;
        lastDistance = starsDistance;

        if (stars.sprite == null)
            return;

        float worldHeight = cam.orthographic ? cam.orthographicSize * 2f : 2f * starsDistance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

        float worldWidth = worldHeight * cam.aspect;

        starsWorldHeight = worldHeight;

        float tileHeight = stars.sprite.bounds.size.y;

        stars.transform.localRotation = Quaternion.identity;
        stars.transform.localScale = Vector3.one;
        stars.size = new Vector2(worldWidth, worldHeight + tileHeight);
    }
}