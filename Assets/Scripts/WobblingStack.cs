using System.Collections.Generic;
using System.Linq;

using DG.Tweening;

using UnityEngine;


public class WobblingStack :MonoBehaviour
{
    [SerializeField, Tooltip("The range for the wobble rate. X = base stack, Y = topmost items.")]
    private Vector2 rateRange = new Vector2(0.8f, 0.4f);

    [SerializeField, Tooltip("The factor that determines the amount of tilt for each item in the stack.")]
    private float bendFactor = 0.1f;

    public MaterialType MaterialType { get; private set; }  // The current type of the stack (Food, Trash, or Package)
    public int Count => stack.Count;

    private List<Transform> stack = new List<Transform>();

    private float stackOffset => 0.3f;

    Vector2 movement;  // Used to store player input for wobble movement

    void Update()
    {
        // If the stack is empty, there's nothing to update
        if (stack.Count == 0)
            return;

        // Get the horizontal and vertical input for wobbling movement
        movement.x = SimpleInput.GetAxis("Horizontal");
        movement.y = SimpleInput.GetAxis("Vertical");

        // Update the position and rotation of the first item in the stack (base)
        stack[0].transform.position = transform.position;
        stack[0].transform.rotation = transform.rotation;

        // Loop through the remaining items in the stack
        for (int i = 1; i < stack.Count; i++)
        {
            // Calculate the wobble rate based on the item's position in the stack
            float rate = Mathf.Lerp(rateRange.x, rateRange.y, i / (float)stack.Count);

            // Smoothly move each item towards the position of the item below it
            stack[i].position = Vector3.Lerp(stack[i].position, stack[i - 1].position + (stack[i - 1].up * stackOffset), rate);

            // Smoothly rotate each item to align with the item below it
            stack[i].rotation = Quaternion.Lerp(stack[i].rotation, stack[i - 1].rotation, rate);

            // Apply bending / tilting based on player input
            if (movement != Vector2.zero)
                stack[i].rotation *= Quaternion.Euler(-i * bendFactor * rate, 0, 0);
        }
    }

    /// <summary>
    /// Adds an item to the stack.
    /// </summary>
    /// <param name="child">The transform of the item to be added to the stack.</param>
    /// <param name="materialType">The type of the stack (Food, Trash, or Package).</param>
    public void AddToStack(Transform child, MaterialType materialType)
    {
        // If this is the first item, set the stack type and show the tray
        if (stack.Count == 0)
        {
            MaterialType = materialType;
        }

        Vector3 peakPoint = transform.position + Vector3.up * Count * stackOffset;  // Calculate the peak point for the item to jump to
        child.DOJump(peakPoint, 5f, 1, 0.3f);

        stack.Add(child);
    }

    /// <summary>
    /// Removes an item from the stack.
    /// </summary>
    /// <returns>The transform of the item that was removed.</returns>
    public Transform RemoveFromStack()
    {
        // Get the last item in the stack
        var lastChild = stack.LastOrDefault();
        if (lastChild == null)
            return null;
        lastChild.rotation = Quaternion.identity;  // Reset its rotation

        // Remove the last item from the stack and decrease the height
        stack.Remove(lastChild);

        // If the stack is empty after removal, hide the tray
        if (stack.Count == 0)
        {
            MaterialType = MaterialType.None;
        }

        return lastChild;  // Return the removed item
    }
}

public enum MaterialType
{
    None = 0,

    Wood_1 = 11,
    Rock_1 = 12,

    Wood_2 = 21,
    Flower_2 = 22,
    Rock_2 = 23,

    Wood_3 = 31,
    Rock_3 = 32,
    Snow_3 = 33,
    Tomato_3 = 34,

    Wood_4 = 41,
    Rock_4 = 42,
    Flower_4 = 43,
    Car_4 = 44,
    Pig_4 = 45,

    Wood_5 = 51,
    Snow_5 = 52,
    Rock_5 = 53,
    Juice_5 = 54,
    Skeleton_5 = 55,
    Wood2_5 = 56,

    Wood_6 = 61,
    Flower_6 = 62,
    MushRoom_6 = 63,
    Car_6 = 64,
    Rock_6 = 65,
    Wood2_6 = 66,


    Wood_7 = 71,
    Saboten_7 = 72,
    Pumpkin_7 = 73,
    Dumbbell_7 = 74,
    Food_7 = 75,
    Wood2_7 = 76,
}
