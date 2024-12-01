using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

// Applicant class to store variables for each applicant (number, colour, notes)
[System.Serializable]
public class Applicant
{
    public int applicantNumber;
    public Color applicantColour;
    public List<string> notes;

    public Applicant(int number, Color color)
    {
        applicantNumber = number;
        applicantColour = color;
        notes = new List<string>();
    }
}

public class ApplicantNotes : MonoBehaviour
{
    [SerializeField] private int applicantAmount = 3;
    [SerializeField] private int currentApplicantIndex = 0;
    [SerializeField] private List<Applicant> applicants;
    [SerializeField] private Color[] possibleApplicantColours; // Possible colours for the applicants - #FEFF9C, #FF7EB9, #7AFCFF, #7AFF7A, #FFA87A

    // UI
    [SerializeField] private TextMeshProUGUI currentApplicantNumberText;
    [SerializeField] private Image currentApplicantColorImage;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button doneButton;
    [SerializeField] private Button addNoteButton;
    [SerializeField] private Button removeNoteButton;
    [SerializeField] private Transform notesParent;
    [SerializeField] private GameObject addAndRemoveNotesObject;
    [SerializeField] private GameObject applicantInputFieldPrefab;
    
    private GameManager _gameManager;
    
    private void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>();

        // Initialize the applicants list
        applicants = new List<Applicant>();
        
        // Loop through the amount of applicants and create a new Applicant object for each one
        for (int i = 0; i < applicantAmount; i++)
        {
            Color applicantColor = possibleApplicantColours[i];
            Applicant newApplicant = new Applicant(i + 1, applicantColor);
            applicants.Add(newApplicant);
        }
        
        // Add listeners to the UI (buttons and input fields)
        AddListenersToUI();
        
        // Update the UI
        UpdateApplicantUI();
    }

    private void AddListenersToUI()
    {
        // Add listener to the buttons
        nextButton.onClick.AddListener(NextApplicant);
        previousButton.onClick.AddListener(PreviousApplicant);
        doneButton.onClick.AddListener(DoneButton);
        addNoteButton.onClick.AddListener(AddNoteForApplicant);
        removeNoteButton.onClick.AddListener(RemoveNoteForApplicant);
    }
    
    private void NextApplicant()
    {
        // Move to the next applicant
        currentApplicantIndex = (currentApplicantIndex + 1) % applicantAmount;
        
        Debug.Log($"Next applicant, current number: {currentApplicantIndex}");
        
        // Update UI
        UpdateApplicantUI();
    }
    
    private void PreviousApplicant()
    {
        // Move to the previous applicant
        currentApplicantIndex = (currentApplicantIndex - 1 + applicantAmount) % applicantAmount;
        
        Debug.Log($"Previous applicant, current number: {currentApplicantIndex}");
        
        // Update UI
        UpdateApplicantUI();
    }
    
    private void DoneButton()
    {
        Debug.Log("Done button pressed");
        
        // Disable the applicant notes UI
        gameObject.SetActive(false);
        
        // Show the connect to server UI
        _gameManager.ShowConnectUI();
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
        // Clear the current notes from the UI, except the add and remove notes object
        foreach (Transform child in notesParent)
        {
            if (child.gameObject != addAndRemoveNotesObject)
            {
                Destroy(child.gameObject);
            }
        }

        // Get the current applicant
        Applicant currentApplicant = applicants[currentApplicantIndex];
        
        // If there are no notes, add an empty note to the list
        if (currentApplicant.notes.Count == 0)
        {
            currentApplicant.notes.Add("");
        }

        // Update UI text and colour
        currentApplicantNumberText.text = $"Applicant {currentApplicant.applicantNumber}";
        currentApplicantColorImage.color = currentApplicant.applicantColour;

        // Create the notes input fields again
        for (int i = 0; i < currentApplicant.notes.Count; i++)
        {
            string noteText = currentApplicant.notes[i];

            // Instantiate the note input field and set it as the second last sibling of the notesParent
            GameObject newInputField = Instantiate(applicantInputFieldPrefab, notesParent);
            newInputField.transform.SetSiblingIndex(notesParent.childCount - 1);

            // Get the input field component
            TMP_InputField inputFieldComponent = newInputField.GetComponent<TMP_InputField>();

            // Set the text
            inputFieldComponent.text = noteText;

            // Capture the index for the closure
            int index = i;

            // Add a listener to update the note when the input field changes
            inputFieldComponent.onValueChanged.AddListener((text) =>
            {
                currentApplicant.notes[index] = text;
            });
        }
    }
}
