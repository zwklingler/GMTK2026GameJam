using UnityEngine;

public class PowerupGlow : MonoBehaviour
{
    [Tooltip("Soft sprite for the glow")]
    [SerializeField] private Sprite glowSprite;

    [Tooltip("Glow color")]
    [SerializeField] private Color glowColor = new Color(1f, 0.85f, 0.3f);

    [Tooltip("World size of the glow")]
    [SerializeField] private float glowSize = 2.5f;

    [Tooltip("Dimmest brightness")]
    [SerializeField] private float minIntensity = 1f;

    [Tooltip("Brightest brightness")]
    [SerializeField] private float maxIntensity = 3f;

    [Tooltip("Pulse speed of the glow")]
    [SerializeField] private float pulseSpeed = 1.5f;

    [Tooltip("How far behind the powerup the glow sits")]
    [SerializeField] private float glowDepth = 0.3f;

    private SpriteRenderer glowRenderer;
    private float phaseOffset;

    void Awake()
    {
        GameObject glow = new GameObject("Glow");
        glow.transform.SetParent(transform, false);
        glow.transform.localPosition = new Vector3(0f, 0f, glowDepth);
        glow.transform.localScale = Vector3.one * glowSize;

        glowRenderer = glow.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = glowSprite != null ? glowSprite : BuildFallbackSprite();

        Material additive = new Material(Shader.Find("Sprites/Default"));
        glowRenderer.material = additive;

        glowRenderer.sortingOrder = 100;

        phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        if(glowRenderer == null)
            return;

        float t = (Mathf.Sin(Time.time * pulseSpeed + phaseOffset) + 1f) * 0.5f;
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);

        glowRenderer.color = glowColor * intensity;
    }

    Sprite BuildFallbackSprite()
    {
        const int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;

        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.5f;

        for(int y = 0; y < size; y++)
        {
            for(int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center) / radius;

                float alpha = Mathf.Clamp01(1f - dist);
                alpha = alpha * alpha;

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}