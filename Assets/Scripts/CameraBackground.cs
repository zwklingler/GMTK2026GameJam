using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraBackground : MonoBehaviour
{
    [SerializeField] private Transform rocket;
    [SerializeField] private Color skyColor   = new Color(0.3f, 0.6f, 1f);
    [SerializeField] private Color spaceColor  = Color.black;
    [SerializeField] private float spaceHeight = 200f;

    private Camera cam;

    void Awake() => cam = GetComponent<Camera>();

    void Update()
    {
        float t = Mathf.Clamp01(rocket.position.y / spaceHeight);
        cam.backgroundColor = Color.Lerp(skyColor, spaceColor, t);
    }
}