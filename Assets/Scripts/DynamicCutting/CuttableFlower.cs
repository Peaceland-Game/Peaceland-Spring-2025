using NUnit.Framework;
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
    const float DEVIATION = 3.0f; // Determines how far apart min y and max y can be
    const float BOTTOM_BUFFER = 1.0f; // Determines the minimum distance from the bottom the hitbox will be
    const float ANGLE_RANGE = 55.0f;
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
        halfHeight = rect.height / 2 * this.transform.localScale.y;
        minY = this.transform.position.y - halfHeight + BOTTOM_BUFFER;
        maxY = this.transform.position.y - halfHeight + BOTTOM_BUFFER + DEVIATION;

        // Loop through and set scale of each mask
        for(int i = 0; i < masks.Count; i++)
        {
            Vector3 scale = masks[i].localScale;
            scale.Set(rect.height, rect.height, 1.0f);
            masks[i].localScale = scale;
        }
    }

    void RandomHitboxPos()
    {
        // Generate a random position in the bottom range of the stem
        Vector3 newPos = this.transform.position;
        newPos.y = Random.Range(minY, maxY);

        // Generate a random angle for the hitbox
        float angle = Random.Range(-ANGLE_RANGE, ANGLE_RANGE);

        // Set hitbox position and angle
        hitbox.position = newPos;
        

        // Adjust mask positions
        for(int i = 0; i < masks.Count; i++)
        {
            // Set new mask pos equal to hitbox position,
            // then adjust the y position so that it is just above the line
            Vector3 newMaskPos = hitbox.position;
            if (masks[i].gameObject.layer == LayerMask.NameToLayer("FlowerTop")) {
                // Move the mask up if its part of the top flower
                newMaskPos.y += halfHeight;
            }
            else
            {
                // Move the mask down if its part of the bottom flower
                newMaskPos.y -= halfHeight;
            }

           
        }

        // *** ANGLE ADJUSTMENTS *** //
        hitbox.rotation = Quaternion.Euler(0, 0, angle);
        //for(int i = 0; i < masks.Count; i++)
        //{
        //    masks[i].rotation = hitbox.rotation;
        //}
    }
}
