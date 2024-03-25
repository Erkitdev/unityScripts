using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerMovement : ControllableEntityNetwork
{
    [Header("Models")]
    [SerializeField] GameObject ModelHolder;
    // Start is called before the first frame update
    void Start()
    {
        if (!IsOwner) return;
        foreach (var item in ModelHolder.GetComponentsInChildren<Transform>())
        item.gameObject.layer = 3;
        Setup();
        this.transform.position = new Vector3(5, 1, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        Move();
        AddGravity();
        CalcRotation();
        HeadBob();
    }
}
