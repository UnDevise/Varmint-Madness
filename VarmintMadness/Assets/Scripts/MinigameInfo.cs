using UnityEngine;

[CreateAssetMenu(fileName = "MinigameInfo", menuName = "Minigame/Info")]
public class MinigameInfo : ScriptableObject
{
    public string minigameName;
    [TextArea(3, 6)]
    public string controlsDescription;
    public Sprite minigameIcon;
}