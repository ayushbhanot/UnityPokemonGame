using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandMoveNPC : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float changeDirectionInterval = 2f;
    public Vector2 movementRangeMin;
    public Vector2 movementRangeMax;

    private Vector3 currentDirection;
    private float timer;

    void Start()
    {
        // Start with a random direction
        currentDirection = GetRandomDirection();
        timer = changeDirectionInterval;
    }

    void Update()
    {
        // Move the NPC
        transform.Translate(currentDirection * moveSpeed * Time.deltaTime);

        // Clamp NPC position within movement range
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, movementRangeMin.x, movementRangeMax.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, movementRangeMin.y, movementRangeMax.y);
        transform.position = clampedPosition;

        // Update timer
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            // Change direction after a certain interval
            currentDirection = GetRandomDirection();
            timer = changeDirectionInterval;
        }
    }

    Vector3 GetRandomDirection()
    {
        // Generate a random integer to select a direction
        int randomIndex = Random.Range(0, 4); // 0: up, 1: down, 2: left, 3: right

        // Convert the random integer to a direction vector
        switch (randomIndex)
        {
            case 0:
                return Vector3.up;
            case 1:
                return Vector3.down;
            case 2:
                return Vector3.left;
            case 3:
                return Vector3.right;
            default:
                return Vector3.zero; // Should not happen
        }
    }
}
