using System.Collections;
using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;

public class GrabAndSwipe : MonoBehaviour
{
    /// <summary>
    /// the speed the shears need to be moving (units/sec) in order to slice
    /// </summary>
    [SerializeField] float sliceSpeed;

    /// <summary>
    /// the trail that spawns when the player touches the screen
    /// </summary>
    [SerializeField] GameObject shearTrail;
    GameObject currentTrail;

    /// <summary>
    /// Used to add difficulty to the minigame. 0 is normal, 1 is shaky hands, and 2 is blurred vision.
    /// </summary>
    public int difficulty;

    bool isMouseDown;
    bool isSlicing;
    Vector2 previousMousePos;
    private bool shearMouseDown; // Makes sure multiple shear trails can't be made in one mouse down

    /// <summary>
    /// Used to keep track of the shaking hands and change their frequency.
    /// </summary>
    private float xOffset = 0;
    private float yOffset = 0;
    private float shakeTimer;

    public bool IsSlicing { get { return isSlicing; } }

    private void Start()
    {
        shearMouseDown = false;
        isSlicing = false;
        previousMousePos = Vector2.zero;

        if(difficulty > 1)
        {
            PostProcessVolume ppVolume = Camera.main.gameObject.GetComponent<PostProcessVolume>();
            ppVolume.weight = 1;
            ppVolume.enabled = true;
            if(difficulty >= 2)
            {
                ppVolume.weight = 0.45f + (difficulty * 0.05f);
            }
        }
    }

    private void Update()
    {
        if (isMouseDown)
        {
            // Snap to mouse position
            transform.position = InputHelper.GetPointerWorldPosition();

            //Randomly moves the slicer position to simulate shaky hands
            if (difficulty > 0)
            {
                shakeTimer += Time.deltaTime;
                if (shakeTimer > 0.2)
                {
                    xOffset = UnityEngine.Random.Range(-0.4f, 0.4f);
                    yOffset = UnityEngine.Random.Range(-0.4f, 0.4f);
                    shakeTimer = 0;
                }
                xOffset *= 0.99f;
                yOffset *= 0.99f;
                transform.position = new(transform.position.x + xOffset, transform.position.y + yOffset);
            }

            // Determine if the blade is slicing
            isSlicing = Vector2.Distance(previousMousePos, transform.position) >= sliceSpeed * Time.deltaTime;

            // Update prev mouse pos
            previousMousePos = new(transform.position.x + xOffset, transform.position.y + yOffset);
        }
        //separate the trail from the shears
        else
        {
            if (currentTrail != null)
            {
                currentTrail.GetComponent<TrailRenderer>().autodestruct = true;
                currentTrail.transform.parent = null;
                currentTrail = null;
            }
            shearMouseDown = false;
        }
    }

    /// <summary>
    /// Called when user taps down on the screen
    /// </summary>
    /// <param name="context"></param>
    public void OnTap(InputAction.CallbackContext context)
    {
        //only effect the shear in the active minigame
        if (isActiveAndEnabled)
        {
            isMouseDown = !context.canceled;
            //spawn a trail when the mouse is pressed, not released
            if (isMouseDown)
            {
                if (!shearMouseDown)
                {
                    currentTrail = Instantiate(shearTrail, transform.position, Quaternion.identity);

                    currentTrail.transform.parent = transform;
                    //turn on the trail once the shears move to the player's finger
                    StartCoroutine(TurnOnTrail());
                    shearMouseDown = true;
                }
            }
        }
    }

    private IEnumerator TurnOnTrail()
    {
        yield return new WaitForSeconds(0.01f);
        currentTrail.GetComponent<TrailRenderer>().emitting = true;
    }
}
