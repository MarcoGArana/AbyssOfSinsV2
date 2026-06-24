using UnityEngine;

[System.Serializable]
public class StorySlide
{
    public Sprite panelImage;
    [TextArea(3, 6)]
    public string text;
    public string speaker;
    public TextAnchor textAlignment = TextAnchor.LowerCenter;
    public Color textColor = Color.white;
    public Color panelTint = Color.white;
}
