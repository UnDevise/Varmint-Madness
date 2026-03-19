using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Character Data")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public Sprite characterSprite;
    public GameObject characterPrefab; // <--- MAKE SURE THIS LINE EXISTS
    public Color backgroundColor;
    public string characterBio;
}
