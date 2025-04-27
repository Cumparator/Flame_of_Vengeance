using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    // Параметры стены (ширина и высота)
    public Vector2 size = new Vector2(5, 1);

    void Start()
    {
        // Создаём коллайдер для столкновений
        BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
        collider.size = size;

        // Добавляем визуальное отображение стены
        SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.color = Color.gray;  // Можешь заменить это на свой спрайт стены
    }
}
