using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private GameObject objectToFollow;
    [SerializeField] private float timeOffset = 0.1f;
    [SerializeField] private int granularity = 10;

    [SerializeField] private float interpolationSpeed = 20;

    [SerializeField] private GameObject poofSmoke;

    private readonly Queue<Vector3> path = new();

    private Vector3 currentPathPosition;
    private LineRenderer lineRenderer;
    private float queueTimer;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        lineRenderer = objectToFollow.GetComponentInChildren<LineRenderer>();
    }

    // Update is called once per frame
    private void Update()
    {
        // If the objectToFollow has moved, this moves this GO towards the new position with speed = interpolationSpeed
        if (currentPathPosition != Vector3.zero)
            transform.position += (currentPathPosition - transform.position) * (Time.deltaTime * interpolationSpeed);

        // sets Knitby's sprite renderer to be visible depending on whether the yarn string is on screen or not
        if (spriteRenderer.enabled != !lineRenderer.isVisible)
        {
            spriteRenderer.enabled = !lineRenderer.isVisible;
            // make poof smoke only on knitby disappearance, not on reapparance 
            if (!spriteRenderer.enabled)
                Instantiate(poofSmoke, transform);
        }

        // flip Knitby sprite depending on location relative to next path
        spriteRenderer.flipX = (currentPathPosition - transform.position).x > 0;
        transform.Rotate(Vector3.back, Time.deltaTime * 400f);
    }

    private void FixedUpdate()
    {
        // granularity controls how many path positions can be stored in the queue before it is dequeued
        // this controls how far back we want this GO to lag behind the objectToFollow
        if (objectToFollow is null)
            return;
        queueTimer -= Time.fixedDeltaTime;

        if (!(queueTimer <= 0)) return;
        queueTimer = timeOffset / granularity;
        if (path.Count == granularity)
            currentPathPosition = path.Dequeue();
        path.Enqueue(objectToFollow.transform.position);
    }
}