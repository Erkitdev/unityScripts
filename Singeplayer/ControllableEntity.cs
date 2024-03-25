using Gameplay;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine.Animations;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ControllableEntity : MonoBehaviour
{
    [Header("Entity setup")]
    [SerializeField] protected Transform head;
    [SerializeField] protected Transform Body;

    [SerializeField] protected float Acceleration;
    [SerializeField] protected float DeAcceleration;
    [SerializeField] protected float MaxSpeed;
    [SerializeField] protected float Sensitivity;

    [Header("Animator setup (animator needs a parameter _Speed) *This component is optional*")]
    [SerializeField] protected Animator animator;

    [Header("Head Rotation")]
    [SerializeField] protected float minRot;
    [SerializeField] protected float maxRot;

    [Header("Camera Settings")]
    [SerializeField] protected float Smoothing;

    [Header("Head bobbing")]
    [SerializeField] protected float BobAmplitude;
    [SerializeField] protected float BobSpeed;
    [SerializeField] protected float BobResetDuration;

    [Header("Sounds")]
    [SerializeField] AudioSource FeetAudioSource;
    [SerializeField] AudioClip AudioClipRightStep;
    [SerializeField] AudioClip AudioClipLeftStep;

    //Private variables
    CharacterController cc;
    private float xRotation;
    private float yRotation;
    private float timer;
    private float timeElapsed;
    private float HeadStartPos;
    Transform PlayerHolder;
    private Vector3 velocity;
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
        PlayerHolder = this.gameObject.transform;
        cc = this.gameObject.GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        HeadStartPos = head.transform.localPosition.y;
        NoiseTimer = new Timer(NoiseInterval);
        NoiseTimer.Start();
    }
    ///<summary>
    ///Moves the character controller in the direction of a body gameobject.
    /// </summary>
    protected void Move()
    {
        float Horizontal = Input.GetAxis("Horizontal");
        float Vertical = Input.GetAxis("Vertical");
        Vector3 movement = Body.forward * Vertical + Body.right * Horizontal;
        Vector3 velocityVector = GetVelocity(movement);
        cc.Move(velocityVector * Time.deltaTime);
        Animate(velocityVector);
    }
    ///<summary>
    ///Gradually accelerates a movement vector until max speed has been reached, also deaccelerates.
    /// </summary>
    private Vector3 GetVelocity(Vector3 movement) {
        velocity += Acceleration * movement;
        if(movement.magnitude == 0) { velocity -= DeAcceleration * velocity.normalized; if (velocity.magnitude < 0.075f) velocity = Vector3.zero; }
        velocity = Vector3.ClampMagnitude(velocity, MaxSpeed);
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
        ApplyRotation(xRotation, yRotation);
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
            timer += Time.deltaTime;
            timeElapsed = 0;
            // Check if the sinus value at low point
            if (currentSinValue <= 0.1f && timer > 0.3f)
            {
                PlayStepSound();
            }
        }
        else
        {
            if (timeElapsed < BobResetDuration)
            {
                head.localPosition = new Vector3(head.localPosition.x, Mathf.Lerp(head.localPosition.y, HeadStartPos, timeElapsed / BobResetDuration), head.localPosition.z);
                timeElapsed += Time.deltaTime;
                timer = 0;
            }
        }
    }
    ///<summary>
    ///Produces step sound.
    /// </summary>
    private void PlayStepSound()
    {
        if(FeetAudioSource.isPlaying) { return; }
        if (FeetAudioSource.clip == AudioClipRightStep) FeetAudioSource.clip = AudioClipLeftStep;
        else FeetAudioSource.clip = AudioClipRightStep;
        FeetAudioSource.Play();
    }
}
