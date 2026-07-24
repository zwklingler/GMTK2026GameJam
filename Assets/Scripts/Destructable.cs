using UnityEngine;

public class Destructable : MonoBehaviour
{
    [Tooltip("Played when this is destroyed")]
    [SerializeField] AudioClip destroySound;
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] float explosionLifetime = 2f;
    [Range(0f, 1f)]
    [SerializeField] float destroyVolume = 1f;
    
    public void DestroyObject()
    {
        if(destroySound != null && AudioManager.instance != null)
            AudioManager.instance.PlaySFX(destroySound, destroyVolume);

        if(explosionPrefab != null)
        {
            GameObject effect = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(effect, explosionLifetime);
        }

        Destroy(gameObject);
    }
    
}
