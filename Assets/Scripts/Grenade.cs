using System;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public GameObject grenadeParticle;
    public float grenadeSpeed = 10f;
    public float explosionTimer = 5f;
    public float timer = 0f;
    bool nowGrenade = true;
    Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if(timer >= explosionTimer)
        {
            GameObject particle = Instantiate(grenadeParticle);
            particle.transform.position = transform.position;
            Destroy(gameObject);
        }

        if (Input.GetKeyUp(KeyCode.G))
        {
            if (nowGrenade)
            {
                rb.useGravity = true;
                rb.linearVelocity = transform.forward * grenadeSpeed;
                nowGrenade = false;
            }
            
            
        }
    }
}
