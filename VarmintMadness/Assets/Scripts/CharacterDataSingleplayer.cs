using UnityEngine;

namespace MyCharacterSystem
{
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "Character Selection/Character")]
    public class CharacterData : ScriptableObject
    {
        public string characterName;
        public Sprite displaySprite;
        public GameObject characterPrefab;
    }
}
