using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    public InputField lobbyNameInput;
    public InputField passwordInput;
    public Toggle privateToggle;

    private Lobby currentLobby;

    private async void Awake()
    {
        Instance = this;
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void CreateLobby()
    {
        // Set lobby name and visibility
        string lobbyName = lobbyNameInput.text;
        bool isPrivate = privateToggle.isOn;

        CreateLobbyOptions options = new CreateLobbyOptions
        {
            IsPrivate = isPrivate,
            Password = isPrivate ? passwordInput.text : null,
            Player = new Player()
        };

        currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 4, options);
        Debug.Log("Lobby created with ID: " + currentLobby.Id);

        // Transition to in-lobby scene and UI
    }

    public async void JoinLobbyByCode(string lobbyCode)
    {
        JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
        {
            Password = passwordInput.text,
            Player = new Player()
        };

        try
        {
            currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
            Debug.Log("Lobby joined with ID: " + currentLobby.Id);

            // Transition to in-lobby scene and UI
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to join lobby: " + e);
        }
    }
}