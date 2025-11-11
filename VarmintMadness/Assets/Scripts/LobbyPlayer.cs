using Unity.Netcode;
using UnityEngine;

public class LobbyPlayer : NetworkBehaviour
{
    public NetworkVariable<int> CharacterIndex = new NetworkVariable<int>(-1);
    public NetworkVariable<bool> IsReady = new NetworkVariable<bool>(false);

    private void Start()
    {
        // Update character selection UI when value changes
        CharacterIndex.OnValueChanged += OnCharacterChanged;
    }

    private void OnCharacterChanged(int oldIndex, int newIndex)
    {
        // Update the UI to reflect the character change
        // All clients will call this when the variable changes
    }

    [ServerRpc]
    public void SelectCharacterServerRpc(int characterIndex)
    {
        // Check if character is already taken
        if (!IsCharacterTaken(characterIndex))
        {
            CharacterIndex.Value = characterIndex;
        }
    }

    private bool IsCharacterTaken(int characterIndex)
    {
        // Logic to check if another player already has this character index
        // Use a list of players and their selected characters
        return false;
    }
}
