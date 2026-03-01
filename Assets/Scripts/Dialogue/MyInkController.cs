using System;
using Ink.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// This is a super bare bones example of how to play and display a ink story in Unity.
public class MyInkController : MonoBehaviour
{
    public static event Action<Story> OnCreateStory;

    public GameObject buttons;
    public TextMeshProUGUI textTMP;
    public UnityEvent OnStoryFinished;

    void Awake()
    {
        StartStory();
    }

    // Creates a new Story object with the compiled story which we can then play!
    void StartStory()
    {
        story = new Story(inkJSONAsset.text);
        if (OnCreateStory != null) OnCreateStory(story);
        RefreshView();
    }

    // This is the main function called every time the story changes. It does a few things:
    // Destroys all the old content and choices.
    // Continues over all the lines of text, then displays all the choices. If there are no choices, the story is finished!
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("Refresh View.");
            RefreshView();
        }
    }
    void RefreshView()
    {
        
        if (!story.canContinue && story.currentChoices.Count == 0)
        {
            Debug.Log("Story finished.");
            OnStoryFinished.Invoke();
        }

        // Continue gets the next line of the story
        string text = story.Continue();
        // This removes any white space from the text.
        text = text.Trim();
        // Display the text on screen!
        textTMP.text = text;

        RemoveChildren(buttons);

        // Display all the choices, if there are any!
        if (story.currentChoices.Count > 0)
        {
            for (int i = 0; i < story.currentChoices.Count; i++)
            {
                
                Choice choice = story.currentChoices[i];
                Button button = CreateChoiceView(choice.text.Trim());
                // Tell the button what to do when we press it
                button.onClick.AddListener(delegate {
                    OnClickChoiceButton(choice);
                });
            }
        }
        
    }

    // When we click the choice button, tell the story to choose that choice!
    void OnClickChoiceButton(Choice choice)
    {
        story.ChooseChoiceIndex(choice.index);
        RefreshView();
    }

    // Creates a button showing the choice text
    Button CreateChoiceView(string text)
    {
        // Creates the button from a prefab
        Button choice = Instantiate(buttonPrefab) as Button;
        choice.transform.SetParent(buttons.transform, false);

        // Gets the text from the button prefab
        TextMeshProUGUI choiceText = choice.GetComponentInChildren<TextMeshProUGUI>();
        choiceText.text = text;

        // Make the button expand to fit the text
        //HorizontalLayoutGroup layoutGroup = choice.GetComponent<HorizontalLayoutGroup>();
        //layoutGroup.childForceExpandHeight = false;

        return choice;
    }

    // Destroys all the children of this gameobject (all the UI)
    void RemoveChildren(GameObject parent)
    {
        int childCount = parent.transform.childCount;
        for (int i = childCount - 1; i >= 0; --i)
        {
            Destroy(parent.transform.GetChild(i).gameObject);
        }
    }

    [SerializeField]
    private TextAsset inkJSONAsset = null;
    public Story story;

    [SerializeField]
    private Canvas canvas = null;

    // UI Prefabs
    [SerializeField]
    private Button buttonPrefab = null;
}
