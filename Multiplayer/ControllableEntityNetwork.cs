using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine;
using Unity.Netcode;
using Unity.Mathematics;

[RequireComponent(typeof(CharacterController))]
public class ControllableEntityNetwork : NetworkBehaviour
{
    [Header("Entity setup")]
    [SerializeField] protected Transform head;
    [SerializeField] protected Transform Body;

    [Header("Key bindings")]
    [SerializeField] KeyCode Sprint = KeyCode.LeftShift;

    [Header("Walking")]
    [SerializeField] protected float Acceleration = 0.25f;
    [SerializeField] protected float DeAcceleration = 0.5f;
    [SerializeField] protected float MaxSpeed = 1.5f;

    [Header("Sprinting")]
    [SerializeField] protected float S_Acceleration = 0.5f;
    [SerializeField] protected float S_DeAcceleration = 1f;
    [SerializeField] protected float S_MaxSpeed = 3f;

    [Header("Animator setup (animator needs a parameter _Speed) *This component is optional*")]
    [SerializeField] protected Animator animator;

    [Header("Camera Settings")]
    [SerializeField] protected float Smoothing = 25;
    [SerializeField] protected float Sensitivity = 80f;
    [SerializeField] protected float minRot = -90f;
    [SerializeField] protected float maxRot = 90f;

    [Header("Head bobbing")]
    //How high?
    [SerializeField] protected float BobAmplitude = 0.04f;
    //How fast?
    [SerializeField] protected float BobSpeed = 8.5f;
    //Reset how fast to reset upon stopping?
    [SerializeField] protected float BobResetDuration = 5f;
    //How low before trigger step?
    [SerializeField] protected float StepTriggerValue = 0.075f;
    //Base delay before trigger step?
    [SerializeField] float Delay = 0.1f;

    [Header("Sounds")]
    [SerializeField] AudioSource FeetAudioSource;
    [SerializeField] AudioClip AudioClipRightStep;
    [SerializeField] AudioClip AudioClipLeftStep;
    [SerializeField] float StartTime = 0;

    //Private variables
    CharacterController cc;
    private float xRotation; 
    private float yRotation;
    private float timer;
    private float timeElapsed;
    private float HeadStartPos;
    private Vector3 velocity;
    private float BobspeedSprintFactor;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    ///<summary>
    ///Setups the characters variables. Holder -> Body -> Head.
    /// </summary>
    protected void Setup()
    {
        cc = this.gameObject.GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        HeadStartPos = head.transform.localPosition.y;
        head.GetComponent<AudioListener>().enabled = true;
        head.GetComponent<Camera>().enabled = true;
    }
    ///<summary>
    ///Moves the character controller in the direction of a body gameobject.
    /// </summary>
    protected void Move()
    {
        float Horizontal = Input.GetAxis("Horizontal");
        float Vertical = Input.GetAxis("Vertical");
        Vector3 movement = Body.forward * Vertical + Body.right * Horizontal;
        Vector3 velocityVector = GetVelocity(movement, Input.GetKey(Sprint));
        cc.Move(velocityVector * Time.deltaTime);
        Animate(velocityVector);
    }
    ///<summary>
    ///Gradually accelerates a movement vector until max speed has been reached, also deaccelerates.
    /// </summary>
    private Vector3 GetVelocity(Vector3 movement, bool IsSprinting)
    {
        if(!IsSprinting)
        {
            velocity += Acceleration * movement;
            if (movement.magnitude == 0) { velocity -= DeAcceleration * velocity.normalized; if (velocity.magnitude < 0.125f) velocity = Vector3.zero; }
            velocity = Vector3.ClampMagnitude(velocity, MaxSpeed);
            BobspeedSprintFactor = 1;
            return velocity;
        }
        velocity += S_Acceleration * movement;
        if (movement.magnitude == 0) { velocity -= S_DeAcceleration * velocity.normalized; if (velocity.magnitude < 0.125f) velocity = Vector3.zero; }
        velocity = Vector3.ClampMagnitude(velocity, S_MaxSpeed);
        BobspeedSprintFactor = S_MaxSpeed / MaxSpeed;
        return velocity;
    }
    ///<summary>
    ///Set attribute values for an animation controller based on velocity vector.
    /// </summary>
    private void Animate(Vector3 velocityVector)
    {
        if (!animator) return;
        float velMagnitude = velocityVector.magnitude;
        animator.SetFloat("_Speed", velMagnitude);
    }
    ///<summary>
    ///Adds gravity to a character controller.
    /// </summary>
    protected void AddGravity()
    {
        cc.Move(-1 * 9.82f * Body.up);
    }
    ///<summary>
    ///Calculates the rotation based on input axis and applys that rotation with ApplyRotation()
    /// </summary>
    protected void CalcRotation()
    {
        yRotation = yRotation + Input.GetAxis("Mouse X") * Sensitivity * Time.deltaTime;
        xRotation = xRotation + Input.GetAxis("Mouse Y") * Sensitivity * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, minRot, maxRot);
        ApplyRotation(-1 * xRotation, yRotation);
    }
    ///<summary>
    ///Applies rotation based on two float values, x (Body) and y (Head).
    /// </summary>
    protected void ApplyRotation(float x, float y)
    {
        Quaternion x_targetRotation = Quaternion.Euler(x, 0, 0);
        Quaternion y_targetRotation = Quaternion.Euler(0, y, 0);

        head.localRotation = Quaternion.Lerp(head.localRotation, x_targetRotation, Smoothing * Time.deltaTime);
        Body.localRotation = Quaternion.Lerp(Body.localRotation, y_targetRotation, Smoothing * Time.deltaTime);
    }
    ///<summary>
    ///Enables bobbing.
    /// </summary>
    protected void HeadBob()
    {
        if (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0)
        {
            float currentSinValue = Mathf.Sin(timer * BobSpeed);
            head.localPosition = Vector3.Lerp(head.localPosition, new Vector3(head.localPosition.x, HeadStartPos + BobAmplitude * currentSinValue, head.localPosition.z), Time.time);

            // Check if the sinus value at low point*
            if (currentSinValue <= StepTriggerValue && timer > Delay)
            {
                PlayStepSound();
                timer = 0;
            }
            timeElapsed = 0;
            timer += Time.deltaTime * BobspeedSprintFactor;
        }
        else
        {
            if (timeElapsed < BobResetDuration)
            {
                head.localPosition = new Vector3(head.localPosition.x, Mathf.Lerp(head.localPosition.y, HeadStartPos, timeElapsed / BobResetDuration), head.localPosition.z);
                timeElapsed += Time.deltaTime;
            }
            timer = 0;
        }
    }
    ///<summary>
    ///Produces step sound.
    /// </summary>
    private void PlayStepSound()
    {
        if (FeetAudioSource.clip == AudioClipRightStep) FeetAudioSource.clip = AudioClipLeftStep;
        else FeetAudioSource.clip = AudioClipRightStep;
        FeetAudioSource.time = StartTime;
        FeetAudioSource.Play();
    }
}
