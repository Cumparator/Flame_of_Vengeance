using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movePlayer : MonoBehaviour
{
    private Rigidbody2D rb;
    public float speed = 40.0f;
    private Vector2 moveVector;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        moveVector.x = Input.GetAxis("Horizontal");
        moveVector.y = Input.GetAxis("Vertical");

        // Проверяем столкновение перед движением
        if (!Physics2D.OverlapCircle(transform.position + (Vector3)moveVector * speed * Time.deltaTime, 0.1f, LayerMask.GetMask("Wall")))
        {
            rb.MovePosition(rb.position + moveVector * speed * Time.deltaTime);  // Двигаем игрока
        }
    }
}