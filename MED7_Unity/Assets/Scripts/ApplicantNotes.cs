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
    
    // Start is called before the first frame update
    private void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>().GetComponent<GameManager>();
        
        // Initialize the applicants list
        //applicants = new List<Applicant>();
        
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
        // Save the notes for the current applicant
        SaveCurrentApplicantNotes();
        
        // Add one to the current applicant index
        currentApplicantIndex++;

        // If the current applicant index is greater than the amount of applicants - 1, set it back to 0
        if (currentApplicantIndex > applicantAmount - 1)
            currentApplicantIndex = 0;
        
        Debug.Log($"Next applicant, current number: {currentApplicantIndex}");
        
        // Update UI
        UpdateApplicantUI();
    }
    
    private void PreviousApplicant()
    {
        // Save the notes for the current applicant
        SaveCurrentApplicantNotes();
        
        // Subtract one from the current applicant index
        currentApplicantIndex--;
        
        // If the current applicant index is less than 1, set it to the amount of applicants - 1
        if (currentApplicantIndex < 0)
            currentApplicantIndex = applicantAmount - 1;
        
        Debug.Log($"Previous applicant, current number: {currentApplicantIndex}");
        
        // Update UI
        UpdateApplicantUI();
    }
    
    // Method for when the user presses the done button. The system will go to the next step.
    private void DoneButton()
    {
        // Save the notes for the current applicant
        SaveCurrentApplicantNotes();
        
        Debug.Log("Done button pressed");
        
        // Disable the applicant notes UI
        gameObject.SetActive(false);
        
        // Show the connect to server UI
        _gameManager.ShowConnectUI();
    }
    
    private void AddNoteForApplicant()
    {
        // Instantiate a new input field and set it as the second last child of the notesParent
        GameObject newInputField = Instantiate(applicantInputFieldPrefab, notesParent);
        newInputField.transform.SetSiblingIndex(notesParent.childCount - 1);
        
        // Get the input field component
        TMP_InputField inputFieldComponent = newInputField.GetComponent<TMP_InputField>();
        
        // Add an empty note to the applicant's notes list
        applicants[currentApplicantIndex].notes.Add(inputFieldComponent.text);
    }
    
    private void RemoveNoteForApplicant()
    {
        // if there is only one note, do not remove it
        if (applicants[currentApplicantIndex].notes.Count <= 1)
            return;
        
        // Get the current applicant
        Applicant currentApplicant = applicants[currentApplicantIndex];

        // Remove the last note text from the applicants notes. RemoveAt removes the specified index.
        currentApplicant.notes.RemoveAt(currentApplicant.notes.Count - 1);
        
        // Remove the last input field from the notesParent
        Destroy(notesParent.GetChild(notesParent.childCount - 2).gameObject);
        
        // Update the UI
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
        
        // If there are no notes, create a default note
        if (currentApplicant.notes.Count == 0)
        {
            AddNoteForApplicant();
        }

        // Update UI text and colour
        currentApplicantNumberText.text = $"Applicant {currentApplicant.applicantNumber}";
        currentApplicantColorImage.color = currentApplicant.applicantColour;

        // Create the notes input fields again
        foreach (string noteText in currentApplicant.notes)
        {
            // Instantiate the note input field and set it as the second last sibling of the notesParent
            GameObject newInputField = Instantiate(applicantInputFieldPrefab, notesParent);
            newInputField.transform.SetSiblingIndex(notesParent.childCount - 1);

            // Get the input field component
            TMP_InputField inputFieldComponent = newInputField.GetComponent<TMP_InputField>();

            // Set the text
            inputFieldComponent.text = noteText;
        }
    }

    
    private void SaveCurrentApplicantNotes()
    {
        // Get the current applicant
        Applicant currentApplicant = applicants[currentApplicantIndex];

        // Clear the current notes
        currentApplicant.notes.Clear();

        // Save the current notes from the UI
        foreach (Transform child in notesParent)
        {
            if (child.gameObject != addAndRemoveNotesObject)
            {
                TMP_InputField inputField = child.GetComponent<TMP_InputField>();
                if (inputField != null)
                {
                    currentApplicant.notes.Add(inputField.text);
                }
            }
        }
    }
}