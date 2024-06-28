using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class MainMenuMaster : MonoBehaviour
{
    [Header("Create world options")]
    public GameObject worldNameObject;
    public GameObject emptyWorldNameObject;
    public GameObject worldSeedObject;

    [Header("WorldSaves dropdown")]
    public TMP_Dropdown worldsavesDropdown;


    [Header("Worldsettings scriptable objects")]
    [SerializeField] TerrainSettingsObject terrainSettingsObj;
    [SerializeField] PlayerSettingsObject playerSettingsObj;

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
    }
    public void Play()
    {
        // Load the Singleplayer scene
        // SceneSystem.SwitchScene(SceneSystem.Scenes.Singleplayer, this);
        SceneSystem.LoadSingleplayer();
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
            emptyWorldNameObject.SetActive(true);
            return;
        }

        string worldSeedString = worldSeedObject.GetComponent<TMP_InputField>().text;

        // If the seedString is empty, set the seed to 0 (this will result in a randomly generated seed)
        int worldSeed = worldSeedString != "" ? int.Parse(worldSeedString) : 0;

        Debug.Log($"Seed: {worldSeed}");

        // Create a new world
        DataManager.NewWorld(worldName, terrainSettingsObj, playerSettingsObj, worldSeed);

        // Start the game
        Play();
    }
    public void LoadWorldMenu()
    {
        // Update the world selection dropdown

        // Clear dropdown
        worldsavesDropdown.ClearOptions();

        // Load all worldSaves names
        List<string> worldNames = DataManager.GetAllWorldNames();

        // Add all worldSave names as options
        foreach (string worldName in worldNames)
        {
            worldsavesDropdown.options.Add(new(worldName));
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
