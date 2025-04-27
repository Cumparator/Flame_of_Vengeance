using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    // ��������� ����� (������ � ������)
    public Vector2 size = new Vector2(5, 1);

    void Start()
    {
        // ������ ��������� ��� ������������
        BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
        collider.size = size;

        // ��������� ���������� ����������� �����
        SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.color = Color.gray;  // ������ �������� ��� �� ���� ������ �����
    }
}
