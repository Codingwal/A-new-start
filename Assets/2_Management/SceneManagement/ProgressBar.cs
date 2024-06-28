using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Playables;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [Header("LoadingProgress slider and text")]
    public Slider progressSlider;
    public TMP_Text progressText;
    public TMP_Text currentTaskText;

    void Awake()
    {
        // Subscribe to the SceneLoading event to show the progress
        SceneSystem.Loading += OnSceneLoading;
    }
    private void OnDisable()
    {
        SceneSystem.Loading -= OnSceneLoading;
    }
    public void OnSceneLoading(float progress, string currentTask)
    {
        progressSlider.value = progress;
        progressText.text = Mathf.Round(progress * 100) + "%";
        currentTaskText.text = currentTask;
    }
}
