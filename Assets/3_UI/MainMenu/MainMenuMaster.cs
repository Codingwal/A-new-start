using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class MainMenuMaster : MonoBehaviour
{
    // Create world options
    public GameObject worldNameObject;

    // Worldsaves dropdown
    public TMP_Dropdown worldsavesDropdown;

    // LoadingProgress slider and text
    public GameObject loadingScreen;
    public Slider progressSlider;
    public TMP_Text progressText; 

    private void Awake()
    {
        MainSystem.gameState = GameState.MainMenu;

        // Setup the menu so that the player starts at the MainMenu

        // Deactivate all sub menus
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        // Activate the MainMenu wich is the first child
        transform.GetChild(0).gameObject.SetActive(true);

        // Subscribe to the SceneLoading event to show the progress
        SceneSystem.Loading += OnSceneLoading;
    }
    public void Play()
    {
        // Activate the loading screen
        loadingScreen.SetActive(true);

        // Load the Singleplayer scene
        SceneSystem.SwitchScene(SceneSystem.Scenes.Singleplayer, this);
    }
    public void OnSceneLoading(float progress)
    {
        progressSlider.value = progress;
        progressText.text = progress * 100 + "%";
    }
    public void Quit()
    {
        // Close the app
        Application.Quit();
    }
    public void CreateWorld()
    {
        // Get the world name
        string worldName = worldNameObject.GetComponent<TMP_InputField>().text;
        if (worldName == "")
        {
            Debug.Log("Canceled CreateWorld: The worldName is empty");
        }

        // Create a new world
        DataManager.NewWorld(worldName);

        // Start the game
        Play();
    }
    public void LoadWorldMenu()
    {
        // Update the world selection dropdown

        // Clear dropdown
        worldsavesDropdown.ClearOptions();

        // Load all worldSaves names
        List<string> worldSaves = new(DataManager.GetAllWorlds().Keys);

        // Add all worldSave names as options
        foreach (string saveName in worldSaves)
        {
            worldsavesDropdown.options.Add(new(saveName));
        }
        // Reset the dropdown value
        worldsavesDropdown.value = 0;

        // Refresh the dropdown
        worldsavesDropdown.RefreshShownValue();
    }
    public void LoadWorld()
    {
        // Cancel LoadWorld if there are no saved worlds
        if (worldsavesDropdown.options.Count == 0)
        {
            return;
        }

        // Set the currentWorldName to the selected world that should be loaded
        DataManager.currentWorldName = worldsavesDropdown.options[worldsavesDropdown.value].text;

        // Start the game
        Play();
    }

    private void OnApplicationQuit()
    {
        MainSystem.ChangeGameState(GameState.Quitting);
    }
}
