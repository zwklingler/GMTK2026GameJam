using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraBackground : MonoBehaviour
{
    [SerializeField] private Transform rocket;
    [SerializeField] private Color skyColor   = new Color(0.3f, 0.6f, 1f);
    [SerializeField] private Color spaceColor  = Color.black;
    [SerializeField] private float spaceHeight = 200f;

    [Header("Sky Pattern")]
    [Tooltip("Sprite pattern shown over the early sky")]
    [SerializeField] private SpriteRenderer skyPattern;

    [SerializeField] private float skyPatternDistance = 145f;

    [Tooltip("Height where the sky pattern starts fading out")]
    [SerializeField] private float skyPatternFadeOutStartHeight = 250f;

    [Tooltip("Height where the sky pattern is fully hidden")]
    [SerializeField] private float skyPatternFadeOutEndHeight = 350f;

    [Range(0f, 1f)]
    [SerializeField] private float skyPatternMaxAlpha = 0.75f;

    [Header("Stars")]
    [Tooltip("Sprite for the starry sky")]
    [SerializeField] private SpriteRenderer stars;

    [SerializeField] private float starsDistance = 150f;

    [Tooltip("Height where stars begin to appear")]
    [SerializeField] private float starsStartHeight = 250f;

    [Tooltip("Height where stars are fully visible")]
    [SerializeField] private float starsFullHeight = 350f;

    [Tooltip("Constant star movement")]
    [SerializeField] private float starsScrollSpeed = 0.4f;

    [Tooltip("Extra movement in relation to the rocket's height")]
    [SerializeField] private float starsParallax = 0.08f;

    [Tooltip("Snap scrolling to pixels")]
    [SerializeField] private bool snapStarsToPixels = true;

    private Camera cam;

    private float starsScroll;

    void Awake()
    {
        cam = GetComponent<Camera>();

        PrepareScreenLayer(skyPattern);
        PrepareTiledLayer(stars);
    }

    void Update()
    {
        float t = Mathf.Clamp01(rocket.position.y / spaceHeight);
        cam.backgroundColor = Color.Lerp(skyColor, spaceColor, t);

        UpdateSkyPattern();
        UpdateStars();
    }

    void PrepareScreenLayer(SpriteRenderer layer)
    {
        if (layer == null)
            return;

        layer.drawMode = SpriteDrawMode.Simple;
        layer.transform.localScale = Vector3.one;
    }

    void PrepareTiledLayer(SpriteRenderer layer)
    {
        if (layer == null)
            return;

        layer.drawMode = SpriteDrawMode.Tiled;
        layer.tileMode = SpriteTileMode.Continuous;
        layer.transform.localScale = Vector3.one;
    }

    void UpdateSkyPattern()
    {
        if (skyPattern == null)
            return;

        FitSingleSpriteToCamera(skyPattern, skyPatternDistance);

        float fadeOut = Mathf.InverseLerp(skyPatternFadeOutStartHeight, skyPatternFadeOutEndHeight, rocket.position.y);
        SetAlpha(skyPattern, (1f - fadeOut) * skyPatternMaxAlpha);
    }

    void UpdateStars()
    {
        if (stars == null)
            return;

        FitTiledSpriteToCamera(stars, starsDistance, out float worldHeight);

        float alpha = Mathf.InverseLerp(starsStartHeight, starsFullHeight, rocket.position.y);
        SetAlpha(stars, alpha);

        starsScroll += starsScrollSpeed * Time.deltaTime;
        ScrollLayer(stars, starsDistance, starsScroll, starsParallax, worldHeight);
    }

    void SetAlpha(SpriteRenderer layer, float alpha)
    {
        Color color = layer.color;
        color.a = alpha;
        layer.color = color;
    }

    void ScrollLayer(SpriteRenderer layer, float distance, float scroll, float parallax, float worldHeight)
    {
        if (layer.sprite == null)
            return;

        float totalScroll = scroll + rocket.position.y * parallax;
        float tileHeight = layer.sprite.bounds.size.y;
        float offsetY = -Mathf.Repeat(totalScroll, tileHeight) + tileHeight * 0.5f;

        if (snapStarsToPixels && worldHeight > 0f && Screen.height > 0)
        {
            float unitsPerPixel = worldHeight / Screen.height;
            offsetY = Mathf.Round(offsetY / unitsPerPixel) * unitsPerPixel;
        }

        layer.transform.localPosition = new Vector3(0f, offsetY, distance);
    }

    void FitSingleSpriteToCamera(SpriteRenderer layer, float distance)
    {
        if (layer == null || layer.sprite == null)
            return;

        float worldHeight = cam.orthographic ? cam.orthographicSize * 2f : 2f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float worldWidth = worldHeight * cam.aspect;

        Vector2 spriteSize = layer.sprite.bounds.size;

        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
            return;

        layer.transform.localRotation = Quaternion.identity;
        layer.transform.localPosition = new Vector3(0f, 0f, distance);
        layer.transform.localScale = new Vector3(worldWidth / spriteSize.x, worldHeight / spriteSize.y, 1f);
    }

    void FitTiledSpriteToCamera(SpriteRenderer layer, float distance, out float worldHeight)
    {
        worldHeight = 0f;

        if (layer == null || layer.sprite == null)
            return;

        worldHeight = cam.orthographic ? cam.orthographicSize * 2f : 2f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

        float worldWidth = worldHeight * cam.aspect;

        float tileHeight = layer.sprite.bounds.size.y;

        layer.transform.localRotation = Quaternion.identity;
        layer.transform.localScale = Vector3.one;
        layer.size = new Vector2(worldWidth, worldHeight + tileHeight);
    }
}
