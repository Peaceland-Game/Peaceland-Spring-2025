using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Script for a cuttable flower within the trimming minigame
/// </summary>
public class CuttableFlower : MonoBehaviour
{
    List<Transform> masks;
    Transform hitbox;

    // Fields for random hitbox generation
    float minY;
    float maxY;
    float halfHeight;
    const float DEVIATION = 2.0f; // Determines how far apart min y and max y can be
    const float BOTTOM_BUFFER = 0.5f; // Determines the minimum distance from the bottom the hitbox will be
    const float ANGLE_RANGE = 40.0f;

    void Awake()
    {
        // There should only ever be two masks
        masks = new List<Transform>();

        // TO-DO implement dynamic sprite setting once enum is in
        //SetChildSprites();

        // Set proper mask scale
        GetMasksAndHitbox();
        SetMaskScale();

        // Moves hitbox to a random position
        RandomHitboxPos();

        // Assign CuttableFlower's OnCut method to be triggered by a cut
        CutLogic.onCut += OnCut;
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
            if (child.GetComponent<BoxCollider2D>() != null)
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
        Rect rect = this.GetComponentInChildren<RectTransform>().rect;

        // Set positions of random hitbox generation for later
        halfHeight = rect.height / 2.5f * this.transform.localScale.y;
        minY = this.transform.position.y - halfHeight + BOTTOM_BUFFER;
        maxY = minY + DEVIATION;

        // Loop through and set scale of each mask
        foreach(Transform t in masks)
        {
            if (t.gameObject.layer == LayerMask.NameToLayer("FlowerBottom"))
            {
                // Bottom should be a bit narrower
                Vector3 scale = t.localScale;
                scale.Set(rect.height / 1.5f, rect.height, 1.0f);
                t.localScale = scale;
            }
            else
            {
                Vector3 scale = t.localScale;
                scale.Set(rect.width * 1.5f, rect.height, 1.0f);
                t.localScale = scale;
            }
        }
    }

    void RandomHitboxPos()
    {
        // Generate a random position in the bottom range of the stem
        Vector3 newPos = this.transform.position;
        newPos.y = UnityEngine.Random.Range(minY, maxY);

        // Generate a random angle for the hitbox
        float angle = UnityEngine.Random.Range(-ANGLE_RANGE, ANGLE_RANGE);

        // Set hitbox position and angle
        hitbox.position = newPos;

        // *** ANGLE ADJUSTMENTS *** //
        hitbox.rotation = Quaternion.Euler(0, 0, angle);
    }

    void OnCut(Vector2 cutStart, Vector2 cutEnd)
    {
        // Calculate midpoint of the the line
        Vector2 midpoint = CalculateMidpoint(cutStart, cutEnd);

        // Adjust vertical positions of the masks
        // to match y value of the midpoint cut
        foreach (Transform t in masks)
        {
            if (t.gameObject.layer == LayerMask.NameToLayer("FlowerTop"))
            {
                t.Translate(t.up * ((t.localScale.y / 4) + t.parent.gameObject.transform.position.y), Space.World);
            }
            else
            {
                t.Translate(-1 * t.up * ((t.localScale.y / 4) + t.parent.gameObject.transform.position.y), Space.World);
            }
        }


        //** Adjust angle of masks to match the angle of the cut itself **//
        // Calculate angle between horizon and the cutLine
        Vector2 cutLine = cutEnd - cutStart;
        float angle = CalculateVectorAngleFromHorizon(cutLine);

        for (int i = 0; i < masks.Count; i++)
        {
            masks[i].rotation = Quaternion.Euler(0, 0, angle);
        }
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
        return -1 * Mathf.Rad2Deg * Mathf.Acos(Vector2.Dot(v, Vector2.right) / (v.magnitude * Vector2.right.magnitude));
    }
}
