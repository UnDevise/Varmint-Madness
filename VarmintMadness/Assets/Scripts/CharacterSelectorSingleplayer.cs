using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using MyCharacterSystem; // Add this line to match the namespace above

namespace MyCharacterSystem
{
    public class CharacterSelector : MonoBehaviour
    {
        public Image displayImage;
        public List<CharacterData> characters;
        private int currentIndex = 0;

        void Start() => UpdateUI();

        public void NextCharacter()
        {
            if (characters.Count == 0) return;
            currentIndex = (currentIndex + 1) % characters.Count;
            UpdateUI();
        }

        public void PreviousCharacter()
        {
            if (characters.Count == 0) return;
            currentIndex = (currentIndex - 1 + characters.Count) % characters.Count;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (characters.Count > 0 && displayImage != null)
            {
                displayImage.sprite = characters[currentIndex].displaySprite;
            }
        }
    }
}
