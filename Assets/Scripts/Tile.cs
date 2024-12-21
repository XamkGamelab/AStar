using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private Renderer renderer;
    public bool traversable = true;
    public int iCoord; // horizontal coordinate
    public int jCoord; // vertical coordinate
    public int gCost;
    public int hCost;
    public int fCost { get { return gCost + hCost; } }
    public Tile previous;

    [SerializeField] TMP_Text fText;
    [SerializeField] TMP_Text gText;
    [SerializeField] TMP_Text hText;
    private void Awake()
    {
        renderer=GetComponent<Renderer>();
    }
    
    /// <summary>
    /// Sets colour based on traversability
    /// </summary>
    public void SetColour()
    {
        if (traversable) { renderer.material.color = Color.gray; }
        else { renderer.material.color = Color.black; }
    }
    /// <summary>
    /// Overload sets colour to <paramref name="colour"/>
    /// </summary>
    /// <param name="color"></param>
    public void SetColour(Color colour)
    {
        traversable = true;
        renderer.material.color = colour;
    }
    /// <summary>
    /// Updates costs visually
    /// </summary>
    public void UpdateCosts()
    {
        fText.text = fCost.ToString();
        gText.text = gCost.ToString();
        hText.text = hCost.ToString();
    }

}
