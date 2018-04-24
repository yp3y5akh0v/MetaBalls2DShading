﻿using UnityEngine;

public class BlobController : MonoBehaviour {

    public Vector2 velocity;
    public float radius;
    public float mass;
    public bool isAlive;

    [Range(0f, 0.01f)]
    public float massAbsorbingRate;
    [Range(0f, 0.01f)]
    public float massDesorbingRate;

    [Range(0f, 0.01f)]
    public float radiusExpansionRate;
    [Range(0f, 0.01f)]
    public float radiusCompressionRate;

    public void AdjustVelocity(Rect rect)
    {
        if (isAlive)
        {
            if (transform.localPosition.x < -rect.width / 2 || transform.localPosition.x > rect.width / 2)
            {
                velocity.x = -velocity.x;
            }
            if (transform.localPosition.y < -rect.height / 2 || transform.localPosition.y > rect.height / 2)
            {
                velocity.y = -velocity.y;
            }
        }
    }

    public void AdjustPosition(Rect rect)
    {
        if (isAlive)
        {
            if (transform.localPosition.x < -rect.width / 2)
            {
                transform.localPosition = new Vector3(-rect.width / 2, transform.localPosition.y, transform.localPosition.z);
            }

            if (transform.localPosition.x > rect.width / 2)
            {
                transform.localPosition = new Vector3(rect.width / 2, transform.localPosition.y, transform.localPosition.z);
            }

            if (transform.localPosition.y < -rect.height / 2)
            {
                transform.localPosition = new Vector3(transform.localPosition.x, -rect.height / 2, transform.localPosition.z);
            }

            if (transform.localPosition.y > rect.height / 2)
            {
                transform.localPosition = new Vector3(transform.localPosition.x, rect.height / 2, transform.localPosition.z);
            }
        }
    }

    public void UpdateVelocity(Vector2 acceleration)
    {
        if (isAlive)
        {
            velocity += acceleration * Time.deltaTime;
        } 
    }

    public void UpdatePosition()
    {
        if (isAlive)
        {
            transform.localPosition = new Vector3(transform.localPosition.x + velocity.x * Time.deltaTime, transform.localPosition.y + velocity.y * Time.deltaTime, transform.localPosition.z);
        }
    }

    public void DesorbeMass()
    {
        if (isAlive)
        {
            mass *= (1 - massDesorbingRate * Time.deltaTime);
            if (mass < 0.1)
            {
                mass = 0;
                isAlive = false;
            }
        }
    }

    public void AbsorbeMass()
    {
        if (isAlive)
        {
            mass *= (1 + massAbsorbingRate * Time.deltaTime);
            if (mass > 500)
            {
                isAlive = false;
            }
        }
    }

    public void CompressRadius()
    {
        if (isAlive)
        {
            radius *= (1 - radiusCompressionRate * Time.deltaTime);
            if (radius < 0.1)
            {
                radius = 0;
                isAlive = false;
            }
        } 
    }

    public void ExpanseRadius()
    {
        if (isAlive)
        {
            radius *= (1 + radiusExpansionRate * Time.deltaTime);
            if (radius > 500)
            {
                isAlive = false;
            }
        }
    }

    public void applyFriction(float fric)
    {
        if (isAlive)
        {
            velocity = velocity.magnitude * (1 - fric * Time.deltaTime) * velocity.normalized;
            if (velocity.magnitude < 0.1)
            {
                velocity = Vector2.zero;
            }
        }
    }

    void FixedUpdate()
    {
        UpdatePosition();
    }
}