using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class item : MonoBehaviour
{
    [Header("Item SwayRot settings")]
    [SerializeField] protected float SwayMultiplier = 1;
    [SerializeField] protected float SwaySmooth = 1;

    [Header("Rot offset")]
    [SerializeField] protected float OffsetX, OffsetY, OffsetZ;
    [Header("Item SwayPos settings")]
    [SerializeField] protected Transform Hand;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        ItemSwayRot();
    }
    virtual protected void ItemSwayRot()
    {
        float MouseX = Input.GetAxisRaw("Mouse X") * SwayMultiplier;
        float MouseY = Input.GetAxisRaw("Mouse Y") * SwayMultiplier;

        // Define offset rotation
        Quaternion offsetRotation = Quaternion.Euler(OffsetX, OffsetY, OffsetZ);

        // Calculate sway rotations
        Quaternion quaternionX = Quaternion.AngleAxis(MouseY, Vector3.left);
        Quaternion quaternionY = Quaternion.AngleAxis(MouseX, Vector3.up);

        // Combine the offset rotation with sway rotations
        Quaternion targetRot = offsetRotation * quaternionX * quaternionY;

        // Smoothly interpolate towards the target rotation
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, SwaySmooth * Time.deltaTime);
    }
    protected void ItemSwayPos()
    {
        Vector3 targetVector = Hand.transform.position;

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetVector, SwaySmooth * Time.deltaTime); 

    }
    virtual protected void HandleInput()
    {

    }
}
