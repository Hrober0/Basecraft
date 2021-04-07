using System.Collections.Generic;
using UnityEngine;

public class GameEventTrigger : MonoBehaviour
{
    [SerializeField] private GameObject tutorialPrefab = null;
    private GameObject tutorialOnScene = null;

    public static GameEventTrigger instance;
    private void Awake()
    {
        if (instance != null) return;
        instance = this;
    }

    public void OpenTutorial()
    {
        if (tutorialOnScene != null) { tutorialOnScene.SetActive(true); }
        else { tutorialOnScene = Instantiate(tutorialPrefab); }
    }
}
