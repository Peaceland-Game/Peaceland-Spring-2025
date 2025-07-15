using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Script for a cuttable flower within the trimming minigame
/// </summary>
public class CuttableFlower : MonoBehaviour
{
    /// <summary>
    /// List to store all SpriteMask transforms within the CuttableFlower prefab
    /// </summary>
    List<Transform> masks;

    /// <summary>
    /// List to store all SpriteRenderers within the CuttableFlower prefab
    /// </summary>
    List<SpriteRenderer> renderers;

    /// <summary>
    /// Guideline for the guideLine object within the prefab
    /// </summary>
    Transform guideLine;

    /// <summary>
    /// Guideline for the hitbox object within the prefab
    /// </summary>
    Transform hitbox;

    // Fields for random hitbox generation
    float minY;
    float maxY;
    float halfHeight;
    const float DEVIATION = 2.0f; // Determines how far apart min y and max y can be
    const float BOTTOM_BUFFER = 1.0f; // Determines the minimum distance from the bottom the hitbox will be
    const float ANGLE_RANGE = 15.0f;

    Rect spriteRect;

    Vector2 cutLine;
    Vector2 cutStart;
    Vector2 cutEnd;

    void Start()
    {
        // There should only ever be two masks
        masks = new List<Transform>();
        renderers = new List<SpriteRenderer>();

        cutLine = Vector2.zero;
        cutStart = Vector2.zero;
        cutEnd = Vector2.zero;

        SetChildSprites();

        // Set proper mask scale
        GetMasksAndHitbox();
        SetMaskScale();

        // Moves hitbox to a random position
        SetHitboxPos();

        // Assign CuttableFlower's OnCut method to be triggered by a cut
        CutLogic.onCut += OnCut;
    }

    /// <summary>
    /// Uses FlowerShop manager to dynamically select the sprite of the
    /// flower to be cut
    /// </summary>
    private void SetChildSprites()
    {
        foreach(Transform child in transform)
        {
            if(child.GetComponent<SpriteRenderer>() != null)
            {
                renderers.Add(child.GetComponent<SpriteRenderer>());
            }
        }

        /*
        foreach(SpriteRenderer renderer in renderers)
        {
            renderer.sprite = FlowerShopManager.GetFlowerSprite(
                FlowerShopManager.GetCurrentFlower().flowerType);
        }
        */

        //Sets the sprites for the flower tops and bottoms seperately
        for(int i = 0; i < renderers.Count; i+= 2)
        {
            renderers[i].sprite = FlowerShopManager.GetFlowerTopSprite(FlowerShopManager.GetCurrentFlower().flowerType);
            renderers[i+1].sprite = FlowerShopManager.GetFlowerBottomSprite(FlowerShopManager.GetCurrentFlower().flowerType);
            renderers[i + 1].sortingOrder = 1;
        }
    }

    private void OnDestroy()
    {
        // Unassign event to avoid errors
        CutLogic.onCut -= OnCut;
    }


    /// <summary>
    /// Gets the masks and hitboxes of the flower
    /// </summary>
    void GetMasksAndHitbox()
    {
        foreach(Transform child in transform)
        {
            foreach(Transform grandChild in child)
            {
                if(grandChild.GetComponent<SpriteMask>() != null)
                {
                    masks.Add(grandChild);
                }
            }
            
            if (child.GetComponent<LineRenderer>() != null)
            {
                guideLine = child;
                Debug.Log(child.name);
            }
            else if (child.GetComponent<BoxCollider2D>() != null)
            {
                hitbox = child;
            }
        }
    }

    /// <summary>
    /// Sets the flower's sprite masks to dynamically size to the rect
    /// of the sprite that is currently being used
    /// </summary>
    void SetMaskScale()
    {
        // Get the bounding rectangle of the sprite
        spriteRect = this.GetComponentInChildren<RectTransform>().rect;

        // Set positions of random hitbox generation for later
        halfHeight = spriteRect.height / 2f * this.transform.localScale.y;
        minY = this.transform.position.y - halfHeight + BOTTOM_BUFFER;
        maxY = minY + DEVIATION;

        // Loop through and set scale of each mask
        foreach(Transform t in masks)
        {
            Vector3 scale = t.localScale;
            scale.Set(spriteRect.height, spriteRect.height, 1.0f);
            t.localScale = scale;
        }
    }

    /// <summary>
    /// Sets the hitbox position and rotation
    /// </summary>
    void SetHitboxPos()
    {
        /*
        // Generate a random position in the bottom range of the stem
        Vector3 newPos = this.transform.position;
        newPos.y = UnityEngine.Random.Range(minY, maxY);
        newPos.x += 0.5f;

        // Generate a random angle for the hitbox
        float angle = 90 + UnityEngine.Random.Range(-ANGLE_RANGE, ANGLE_RANGE);

        // Set guideline and hitbox position/angle
        guideLine.transform.position = newPos;
        hitbox.transform.position = newPos;

        // *** ANGLE ADJUSTMENTS *** //
        guideLine.transform.rotation = Quaternion.Euler(0, 0, angle);
        hitbox.transform.rotation = Quaternion.Euler(0, 0, angle);

        //Scale down the hitbox so it matches the width of the flower stem
        hitbox.transform.localScale = new Vector3(hitbox.transform.localScale.x, 0.1f, hitbox.transform.localScale.z);

        */

        //Sets position
        Vector3 newPos = this.transform.position;
        newPos.y = maxY - DEVIATION/2;
        guideLine.transform.position = newPos;
        hitbox.transform.position = newPos;

        //Sets the angle
        guideLine.transform.rotation = Quaternion.Euler(0, 0, 90);
        hitbox.transform.rotation = Quaternion.Euler(0, 0, 90);

        //Scale down the hitbox so it matches the width of the flower stem
        hitbox.transform.localScale = new Vector3(hitbox.transform.localScale.x, 0.1f, hitbox.transform.localScale.z + 20f);
    }

    /// <summary>
    /// Function triggered by CutLogic. Initiates dynamic cutting sequence
    /// for the CuttableFlower.
    /// </summary>
    /// <param name="cutStart">Vector position of the start of the cut</param>
    /// <param name="cutEnd">Vector position of the end of the cut</param>

    void OnCut(Vector2 cutStart, Vector2 cutEnd)
    {
        Debug.Log("OnCut called");
        // Calculate midpoint of the the line
        Vector2 midpoint = CalculateMidpoint(cutStart, cutEnd);

        Debug.Log(midpoint);

        // Adjust vertical positions of the masks
        // to match y value of the midpoint cut
        /*
        foreach (Transform t in masks)
        {
            if (t.gameObject.layer == LayerMask.NameToLayer("FlowerTop"))
            {
                t.Translate(t.up * (t.localScale.y / 8), Space.World);
            }
            else
            {
                t.Translate(t.up * (t.localScale.y / 2.0f) * -1, Space.World);
            }
        }
        */


        //** Adjust angle of masks to match the angle of the cut itself **//
        // Calculate angle between horizon and the cutLine
        cutLine = cutEnd - cutStart;
        this.cutStart = cutStart;
        this.cutEnd = cutEnd;

        float angle = CalculateVectorAngleFromHorizon(cutLine);

        /*
        for (int i = 0; i < masks.Count; i++)
        {
            masks[i].rotation = Quaternion.Euler(0, 0, angle);
        }
        */
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = UnityEngine.Color.green;
        Gizmos.DrawLine(cutStart, cutEnd);
    }

    /// <summary>
    /// Calculates the midpoint between two vectors
    /// </summary>
    /// <param name="v1">Start point</param>
    /// <param name="v2">End point</param>
    /// <returns>A Vector representing the midpoint between two vectors</returns>
    Vector2 CalculateMidpoint(Vector2 v1, Vector2 v2)
    {
        return new Vector2((v2.x - v1.x) / 2, (v2.y - v1.y) / 2);
    }

    /// <summary>
    /// Calculates the angle between a given Vector V
    /// and the horizon (Vector2.right)
    /// </summary>
    /// <param name="v">Vector that is intersecting the horizon</param>
    /// <returns>The angle (in degrees) between V and the horizon</returns>
    float CalculateVectorAngleFromHorizon(Vector2 v)
    {
        float angle = Mathf.Rad2Deg * Mathf.Acos(Vector2.Dot(v, Vector2.right) / (v.magnitude * Vector2.right.magnitude));
        //if(hitbox.rotation.z < 0 && angle > 0)
        //{
        //    // angle NEEDS to be negative in this case
        //    angle *= -1;
        //}
        //if(hitbox.rotation.z > 0 && angle < 0)
        //{
        //    angle *= -1;
        //}
        return angle;
    }
}
