using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarHog : MonoBehaviour
{
    [SerializeField] private int direction;
    public Direction directionType;

    void Start()
    {
        Animator animator = GetComponentInChildren<Animator>();
        animator.SetInteger("Direction", direction);
    }
}
