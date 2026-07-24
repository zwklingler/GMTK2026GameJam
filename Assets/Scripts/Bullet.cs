using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;
    public float duration;
    float t;

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;

        t += Time.deltaTime;

        if(t > duration)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        if(collision.gameObject.GetComponentInParent<Destructable>() != null)
        {
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
    }
}
