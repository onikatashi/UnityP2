using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject bulletParticle;
    public float bulletSpeed = 15f;
    float lifeDistance = 100f;
    float currentDistance = 0f;
    Vector3 previousPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        previousPos = transform.position;
    }

    private void FixedUpdate()
    {
        currentDistance += Vector3.Distance(transform.position, previousPos);
        if (currentDistance >= lifeDistance)
        {
            Destroy(gameObject);
        }
        previousPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(transform.forward * bulletSpeed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject particle = Instantiate(bulletParticle);
        particle.transform.position = transform.position;
        Destroy(gameObject);
    }
}
