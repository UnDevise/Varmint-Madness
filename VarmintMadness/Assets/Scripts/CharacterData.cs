using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Character Selection/Character Data")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public Sprite characterSprite;
    [TextArea(3, 10)]
    public string characterBio;
    public Color backgroundColor;
}
