using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;

// Applicant class to store variables for each applicant (number, colour, notes)
[System.Serializable]
public class Applicant
{
    public int applicantNumber;
    public Color applicantColour;
    public List<string> notes;

    // Constructor for the Applicant class
    public Applicant(int number, Color color)
    {
        applicantNumber = number;
        applicantColour = color;
        notes = new List<string>();
    }
}

public class ApplicantNotes : MonoBehaviour
{
    [SerializeField] private int applicantAmount;
    [SerializeField] private int currentApplicantIndex;
    [SerializeField] public List<Applicant> applicants;
    [SerializeField] private Color[] possibleApplicantColours; // Possible colours for the applicants - #FEFF9C, #FF7EB9, #7AFCFF, #7AFF7A, #FFA87A

    // UI
    [SerializeField] private GameObject areYouSureUI, createApplicantUIObject;
    [SerializeField] private TextMeshProUGUI currentApplicantNumberText;
    [SerializeField] private Image currentApplicantColorImage;
    [SerializeField] private Button nextButton, previousButton, doneButton, addNoteButton, removeNoteButton;
    [SerializeField] private Transform notesParent;
    [SerializeField] private GameObject addAndRemoveNotesObject;
    [SerializeField] private GameObject applicantInputFieldPrefab;
    [SerializeField] private Button yesButton, noButton;
    private GameManager _gameManager;
    
    private void Awake()
    {
        // Find the GameManager object in the scene
        _gameManager = FindObjectOfType<GameManager>();
        
        // Initialize the applicants
        InitializeApplicants(applicantAmount);

        // Add listeners to the UI buttons
        AddListenersToUI();
        
        // Update the UI to show the first applicant
        UpdateApplicantUI();
    }

    private void InitializeApplicants(int amount)
    {
        //Initialize the applicants list 
        applicants = new List<Applicant>();

        // Loop through the amount of applicants and create a new Applicant object for each one
        for (int i = 0; i < amount; i++)
        {
            Color applicantColor = possibleApplicantColours[i];
            Applicant newApplicant = new Applicant(i + 1, applicantColor);
            applicants.Add(newApplicant);
        }
    }

    private void AddListenersToUI()
    {
        // Add listener to the buttons
        nextButton.onClick.AddListener(NextApplicant);
        previousButton.onClick.AddListener(PreviousApplicant);
        doneButton.onClick.AddListener(() =>
        {
            areYouSureUI.SetActive(true);
        });
        addNoteButton.onClick.AddListener(AddNoteForApplicant);
        removeNoteButton.onClick.AddListener(RemoveNoteForApplicant);
        yesButton.onClick.AddListener(() =>
        {
            areYouSureUI.SetActive(false);
            createApplicantUIObject.SetActive(false);
            _gameManager.ShowConnectUI();
        });
        noButton.onClick.AddListener(() =>
        {
            areYouSureUI.SetActive(false);
        });
    }
    
    private void NextApplicant()
    {
        // Move to the next applicant
        currentApplicantIndex = (currentApplicantIndex + 1) % applicantAmount;
        
        // Debug.Log($"Next applicant, current number: {currentApplicantIndex}");
        
        // Update UI
        UpdateApplicantUI();
    }
    
    private void PreviousApplicant()
    {
        // Move to the previous applicant
        currentApplicantIndex = (currentApplicantIndex - 1 + applicantAmount) % applicantAmount;
        
        // Debug.Log($"Previous applicant, current number: {currentApplicantIndex}");
        
        // Update UI
        UpdateApplicantUI();
    }
    
    private void AddNoteForApplicant()
    {
        // Add an empty note to the applicant's notes list
        applicants[currentApplicantIndex].notes.Add("");

        // Update the UI to reflect the new note
        UpdateApplicantUI();
    }
    
    private void RemoveNoteForApplicant()
    {
        // If there is only one note, do not remove it
        if (applicants[currentApplicantIndex].notes.Count <= 1)
            return;
        
        // Remove the last note text from the applicant's notes
        applicants[currentApplicantIndex].notes.RemoveAt(applicants[currentApplicantIndex].notes.Count - 1);
        
        // Update the UI to reflect the removed note
        UpdateApplicantUI();
    }
    
    private void UpdateApplicantUI()
    {
        // Clear the current notes from the UI except the add and remove notes object
        foreach (Transform child in notesParent)
        {
            if (child.gameObject != addAndRemoveNotesObject) 
                Destroy(child.gameObject);
        } 
        
        // Get the current applicant
        Applicant currentApplicant = applicants[currentApplicantIndex];
        
        // If there are no notes add an empty note to the list
        if (currentApplicant.notes.Count == 0)
        {
            currentApplicant.notes.Add("");
        }
        
        // Update UI text and colour
        currentApplicantNumberText.text = $"{currentApplicant.applicantNumber}";
        currentApplicantColorImage.color = currentApplicant.applicantColour;

        // Create the notes input fields again
        for (int i = 0; i < currentApplicant.notes.Count; i++)
        {
            // Get the note text
            string noteText = currentApplicant.notes[i];
            
            // Instantiate the note input field as a child of notesParent
            GameObject newInputField = Instantiate(applicantInputFieldPrefab, notesParent);
            
            // Ensure the addAndRemoveNotesObject is the last child
            addAndRemoveNotesObject.transform.SetAsLastSibling();
            
            // Get the input field component
            TMP_InputField inputFieldComponent = newInputField.GetComponent<TMP_InputField>();
            
            // Set the text
            inputFieldComponent.text = noteText;
            
            // Get the index of the note
            int index = i;
            
            // Add a listener to update the note when the input field changes.
            // This is to prevent the notes from being lost when switching between applicants.
            inputFieldComponent.onValueChanged.AddListener((text) =>
            {
                currentApplicant.notes[index] = text;
                
                // Update the size of the input field
                LayoutRebuilder.ForceRebuildLayoutImmediate(notesParent.GetComponent<RectTransform>());
            });
        }
    }
}
