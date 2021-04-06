using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Preface to this code:
 * 
 * This is code I wrote at the beginning of my Unity and C# learning process while under a short deadline
 * to finish a playtest of Phaser for my Game Design 2 course. I have included this code in my showcase
 * to explain how this code operates and how I would do this differently if I were write this today.
 * 
 * This script is attached to each and every colored object in Phaser except for Spike Shooters (it is instead attached to the spikes they shoot).
 * That includes spikes, blocks, and moving platforms. This causes relatively poor performance for a game which should run very smoothly, because
 * every single frame (and for every colored object), the Update() function performs a bunch of comparisons to check if the player pressed a button
 * to switch colors and how that button applies to the color of the colored object. On smaller levels this isn't much of an issue, but on larger
 * and more complex issues, this can cause noticeable lag on lower end machines such as non-gaming laptops (which Phaser realistically should still be able to run well on).
 * 
 * If I were to write this program now, I would opt for grouping colored objects together by their color in the Unity scene hierarchy, and attaching this
 * script to our game manager in the scene. Instead of every object individually checking which button was pressed and how it affects their state, the script
 * would check a single time each frame which button was pressed, and then activate/deactivate whole groups of same-colored objects in one fell swoop. This
 * method would not work for spike shooters, as the block that shoots spikes does not disappear when the player switches colors. This would be a simple fix
 * however by simply making the instantiated spikes a child of the game object containing their similarly colored objects when they are shot, and then checking
 * if that color is currently active.
 * 
 * This script ended up the way it is because of my inexperience with Unity at the time (this was one of the very first scripts I had to write for the game), a lack
 * of proper planning, and the very short deadline we had. I had considered making changes to this script throughout our time finishing up Phaser, but ultimately decided
 * against it for time reasons (every level would have had to be changed among other things). We decided as a group that it was best to finish Phaser and move on to
 * new projects where we could apply our learnings. My learnings from Phaser have helped me create more thoughtful code for our current project at the time
 * of writing this message, "Gaming for High Value Care". GHVC is a project we are working on for the UW Madison VA Hospital to teach Resident doctors 
 * high value care principles. Overall we have found developing for GHVC to be much easier because of our planning process and new code practices.
 * 
 * Thank you for reading this!
 * 
 */



public class colorSwitcher : MonoBehaviour
{

    public bool red;
    public bool blue;
    public bool green;
    public bool yellow;
    public bool isSpike;
    public Sprite onSprite;
    public Sprite offSprite;
    private SpriteRenderer spriteRen;
    private EdgeCollider2D[] edgeCol;
    private BoxCollider2D[] boxCol;
    private colorController colorCon;
    private int lastPressed;

    // Start is called before the first frame update
    void Start()
    {
        colorCon = GameObject.Find("Player").GetComponent<colorController>();
        lastPressed = colorCon.getLastPressed();

        // Setup the object if its a spike, while accounting for which color is activated
        //  in the case of the spike being spawned by a spike shooter
        if(isSpike)
        {
            spriteRen = GetComponent<SpriteRenderer>();
            spriteRen.sprite = onSprite;

            boxCol = GetComponents<BoxCollider2D>();
            int boxColSize = boxCol.Length;
            for (int x = 0; x < boxColSize; x++)
            {
                boxCol[x].enabled = true;
            }

            edgeCol = GetComponents<EdgeCollider2D>();
            int edgeColSize = edgeCol.Length;
            for (int x = 0; x < edgeColSize; x++)
            {
                edgeCol[x].enabled = true;
            }

            if (red && lastPressed == 1)
            {
                ToggleBlock(boxCol, edgeCol, false, offSprite);
            }
            else if (blue && lastPressed == 2)
            {
                ToggleBlock(boxCol, edgeCol, false, offSprite);
            }
            else if (green && lastPressed == 3)
            {
                ToggleBlock(boxCol, edgeCol, false, offSprite);
            }
            else if (yellow && lastPressed == 4)
            {
                ToggleBlock(boxCol, edgeCol, false, offSprite);
            }
        }
        else  // Set up the block's sprite and colliders. Nothing complicated here, as all blocks are in the scene at the first frame.
        {
            spriteRen = GetComponent<SpriteRenderer>();
            spriteRen.sprite = offSprite;

            boxCol = GetComponents<BoxCollider2D>();
            int boxColSize = boxCol.Length;
            for (int x = 0; x < boxColSize; x++)
            {
                boxCol[x].enabled = false;
            }

            edgeCol = GetComponents<EdgeCollider2D>();
            int edgeColSize = edgeCol.Length;
            for (int x = 0; x < edgeColSize; x++)
            {
                edgeCol[x].enabled = false;
            }
        }
    }

    // Update is called once per frame
    // If the game isn't paused, it calls the correct color processing function based on if the object is a spike or not.
    void Update()
    {
        if (Time.timeScale != 0)
        {
            lastPressed = colorCon.getLastPressed();
            if (isSpike)
            {
                ColoredSpikes();
            }
            else
            {
                ColoredBlocks();
            }
        }
    }

    // Takes in the box and edge colliders of the object (spike or block), as well as what state the object should be in
    //  and which sprite to switch the object to.
    // Applies isActive to the colliders of the object and then switches the objects sprite to switchSprite
    void ToggleBlock(BoxCollider2D[] boxColliders, EdgeCollider2D[] edgeColliders, bool isActive, Sprite switchSprite)
    {
        if (boxColliders != null)
        {
            int numBoxColliders = boxColliders.Length;
            for (int x = 0; x < numBoxColliders; x++)
            {
                boxColliders[x].enabled = isActive;
            }
        }

        if (edgeColliders != null)
        {
            int numEdgeColliders = edgeColliders.Length;
            for (int x = 0; x < numEdgeColliders; x++)
            {
                edgeColliders[x].enabled = isActive;
            }
        }
        spriteRen.sprite = switchSprite;
    }

    // Determines which buttons were pressed on the frame, and then activates/deactivates the spike
    // according to the button. If only the color of the spike was pressed, deactivates the spike,
    // otherwise activates the spike.
    void ColoredSpikes()
    {
        bool redPress = Input.GetButtonDown("ColorRed");
        bool bluePress = Input.GetButtonDown("ColorBlue");
        bool greenPress = Input.GetButtonDown("ColorGreen");
        bool yellowPress = Input.GetButtonDown("ColorYellow");
        if (red)
        {
            if (redPress && !bluePress && !greenPress && !yellowPress)
            {
                ToggleBlock(boxCol, edgeCol, false, offSprite);
                colorCon.setLastPressed(1);
            }
            else if (bluePress)
            {
                ToggleBlock(boxCol, edgeCol, true, onSprite);
                colorCon.setLastPressed(2);
            }
            else if (greenPress)
            {
                ToggleBlock(boxCol, edgeCol, true, onSprite);
                colorCon.setLastPressed(3);
            }
            else if (yellowPress)
            {
                ToggleBlock(boxCol, edgeCol, true, onSprite);
                colorCon.setLastPressed(4);
            }
        }
        else if (blue)
        {
            if (bluePress && !redPress && !greenPress && !yellowPress)
            {
                ToggleBlock(boxCol, edgeCol, false, offSprite);
                colorCon.setLastPressed(2);
            }
            else if (redPress)
            {
                ToggleBlock(boxCol, edgeCol, true, onSprite);
                colorCon.setLastPressed(1);
            }
            else if (greenPress)
            {
                ToggleBlock(boxCol, edgeCol, true, onSprite);
                colorCon.setLastPressed(3);
            }
            else if (yellowPress)
            {
                ToggleBlock(boxCol, edgeCol, true, onSprite);
                colorCon.setLastPressed(4);
            }
        }
        else if (green)
        {
            if (greenPress && !bluePress && !redPress && !yellowPress)
            {
                ToggleBlock(boxCol, edgeCol, false, offSprite);
                colorCon.setLastPressed(3);

            }
            else if (bluePress)
            {
                ToggleBlock(boxCol, edgeCol, true, onSprite);
                colorCon.setLastPressed(2);
            }
            else if (redPress)
            {
                ToggleBlock(boxCol, edgeCol, true, onSprite);
                colorCon.setLastPressed(1);
            }
            else if (yellowPress)
            {
                ToggleBlock(boxCol, edgeCol, true, onSprite);
                colorCon.setLastPressed(4);
            }
        }
        else if (yellow)
        {
            if (yellowPress && !bluePress && !redPress && !greenPress)
            {
                ToggleBlock(boxCol, edgeCol, false, offSprite);
                colorCon.setLastPressed(4);
            }
            else if (bluePress)
            {
                ToggleBlock(boxCol, edgeCol, true, onSprite);
                colorCon.setLastPressed(2);
            }
            else if (greenPress)
            {
                ToggleBlock(boxCol, edgeCol, true, onSprite);
                colorCon.setLastPressed(3);
            }
            else if (redPress)
            {
                ToggleBlock(boxCol, edgeCol, true, onSprite);
                colorCon.setLastPressed(1);
            }
        }
    }

    // Checks which buttons were pressed on the frame to change colors. If only the color
    // of the block is pressed, activates the block. If any other color is pressed,
    // deactivates the block.
    void ColoredBlocks()
    {
        bool redPress = Input.GetButtonDown("ColorRed");
        bool bluePress = Input.GetButtonDown("ColorBlue");
        bool greenPress = Input.GetButtonDown("ColorGreen");
        bool yellowPress = Input.GetButtonDown("ColorYellow");
        if (red)
        {
            if (redPress && !bluePress && !greenPress && !yellowPress)
            {
                ToggleBlock(boxCol, edgeCol, true, onSprite);
                colorCon.setLastPressed(1);
            }
            else if (bluePress)
            {
                ToggleBlock(boxCol, edgeCol, false, offSprite);
                colorCon.setLastPressed(2);
            }
            else if(greenPress)
            {
                ToggleBlock(boxCol, edgeCol, false, offSprite);
                colorCon.setLastPressed(3);
            }
            else if (yellowPress)
            {
                ToggleBlock(boxCol, edgeCol, false, offSprite);
                colorCon.setLastPressed(4);
            }
        }
        else if (blue)
        {
            if (bluePress && !redPress && !greenPress && !yellowPress)
            {
                ToggleBlock(boxCol, edgeCol, true, onSprite);
                colorCon.setLastPressed(2);
            }
            else if (redPress)
            {
                ToggleBlock(boxCol, edgeCol, false, offSprite);
                colorCon.setLastPressed(1);
            }
            else if (greenPress)
            {
                ToggleBlock(boxCol, edgeCol, false, offSprite);
                colorCon.setLastPressed(3);
            }
            else if (yellowPress)
            {
                ToggleBlock(boxCol, edgeCol, false, offSprite);
                colorCon.setLastPressed(4);
            }
        }
        else if (green)
        {
            if (greenPress && !bluePress && !redPress && !yellowPress)
            {
                ToggleBlock(boxCol, edgeCol, true, onSprite);
                colorCon.setLastPressed(3);
            }
            else if (bluePress)
            {
                ToggleBlock(boxCol, edgeCol, false, offSprite);
                colorCon.setLastPressed(2);
            }
            else if (redPress)
            {
                ToggleBlock(boxCol, edgeCol, false, offSprite);
                colorCon.setLastPressed(1);
            }
            else if (yellowPress)
            {
                ToggleBlock(boxCol, edgeCol, false, offSprite);
                colorCon.setLastPressed(4);
            }
        }
        else if (yellow)
        {
            if (yellowPress && !bluePress && !redPress && !greenPress)
            {
                ToggleBlock(boxCol, edgeCol, true, onSprite);
                colorCon.setLastPressed(4);
            }
            else if (bluePress)
            {
                ToggleBlock(boxCol, edgeCol, false, offSprite);
                colorCon.setLastPressed(2);
            }
            else if (greenPress)
            {
                ToggleBlock(boxCol, edgeCol, false, offSprite);
                colorCon.setLastPressed(3);
            }
            else if (redPress)
            {
                ToggleBlock(boxCol, edgeCol, false, offSprite);
                colorCon.setLastPressed(1);
            }
        }
    }

    public int getLastPressed() { return lastPressed; }
}
