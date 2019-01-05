using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class MoveRandomly : MonoBehaviour
{
    public Transform Bounds;
    public bool X = true;
    public bool Y = false;
    public bool Z = true;

    protected Bounds bounds;

    protected void Awake()
    {
        if (Bounds == null)
            return;
		
        bounds = Bounds.GetBounds();
        Move();
    }

    protected void Move()
    {
        Vector3 random = bounds.Random();
        Vector3 position = transform.position;
        if (!X)
            random.x = position.x;
        if (!Y)
            random.y = position.y;
        if (!Z)
            random.z = position.z;
        transform.DOMove(random, 5.0f).SetSpeedBased().OnComplete(Move);
    }
}