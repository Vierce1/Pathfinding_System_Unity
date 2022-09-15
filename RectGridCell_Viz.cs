using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectGridCell_Viz : MonoBehaviour
{
    //attach the inner sprite gameobject to
    // this variable in inspector
    public SpriteRenderer innerSprite;
    //attach the outer sprite gameobject to
    // this varibale in inspector
    [SerializeField] SpriteRenderer outerSprite;

    //this is a ref to the actual grid cell that this 
    // RectGridCell_Viz represents
    public RectGridCell rgc;    

    //can set these colors to alpha = 0 if you
    // don't want to see the visual cell
    // Good to leave in for testing though
    public Color startCol;
    public Color starterOuterCol;
    public Color nonWalkableCol;

    private void Start()
    {
        startCol = innerSprite.color;
        starterOuterCol = outerSprite.color;
        nonWalkableCol = Color.red;
    }
    
    //comment out Update if you don't
    // want the cells to change colors
    private void Update()
    {        
        if (!rgc.isWalkable)
        {
            SetInnerColor(Color.red);
        }
        else
        {
            ResetColor();
        }
    }

    public void SetInnerColor(Color col)
    {
        innerSprite.color = col;
    }
    public void SetOuterColor(Color col)
    {
        outerSprite.color = col;
    }
    public void ResetColor()
    {
        innerSprite.color = startCol;
        outerSprite.color = starterOuterCol;
    }
}
