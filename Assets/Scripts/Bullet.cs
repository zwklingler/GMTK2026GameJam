using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;
    public float duration;
    float t;

    void Update()
    {
        transform.position += transform.up * speed * Time.deltaTime;

        t += Time.deltaTime;

        if(t > duration)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        Destructable target = collision.gameObject.GetComponentInParent<Destructable>();
        if(target != null)
        {
            Destroy(target.gameObject);
            Destroy(gameObject);
        }
    }
}
