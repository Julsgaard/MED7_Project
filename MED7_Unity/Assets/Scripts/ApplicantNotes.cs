using UnityEngine;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class ApplicantNotes : MonoBehaviour
{
    [SerializeField] private int applicantAmount = 3;
    [SerializeField] private int currentApplicantNumber = 1;
    [SerializeField] private string[] applicantNotes;
    [SerializeField] private Color[] applicantColours;
    
    // UI
    [SerializeField] private TextMeshProUGUI currentApplicantText;
    [SerializeField] private Image currentApplicantColorImage;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button doneButton;
    [SerializeField] private TextMeshProUGUI applicantNotesText;
    //[SerializeField] private TMP_InputField[] applicantNotesInputFields;
    
    private GameManager _gameManager;
    
    // Start is called before the first frame update
    private void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>().GetComponent<GameManager>();
        
        // Create new array of applicant notes
        applicantNotes = new string[applicantAmount];
        
        // Create new array of applicant input fields
        //applicantNotesInputFields = new TMP_InputField[applicantAmount];
        
        // Add listeners to the UI (buttons and input fields)
        AddListenersToUI();

        // If the length of the applicant notes array is not equal to the amount of applicants, create a new array
        // The array can manually be set in the inspector for testing purposes
        if (applicantNotes.Length != applicantAmount)
        {
            applicantNotes = new string[applicantAmount];
            
            Debug.Log("New applicant notes array created");
        }
        
        // Update the UI
        UpdateApplicantUI();
    }

    private void AddListenersToUI()
    {
        // Add listener to the next button
        nextButton.onClick.AddListener(() =>
        {
            NextApplicant();
        });
        
        // Add listener to the previous button
        previousButton.onClick.AddListener(() =>
        {
            PreviousApplicant();
        });
        
        // Add listener to the done button
        doneButton.onClick.AddListener(() =>
        {
            CreateApplicantsDoneButton();
        });
        
        // Add listener to the input fields
        foreach (var inputField in applicantNotesInputFields)
        {
            inputField.onValueChanged.AddListener((value) =>
            {
                WhenInputFieldChanged(inputField);
            });
        }
    }
    
    private void WhenInputFieldChanged(TMP_InputField inputField)
    {
        // Check if the last character is a new line character
        if (inputField.text.Length > 0 && inputField.text[inputField.text.Length - 1] == '\n')
        {
            // Append the bullet point at the end of the text
            inputField.text += "* ";
        
            // Set the caret position at the end of the text after adding '* '
            inputField.caretPosition = inputField.text.Length;
        }
    }

    
    private void NextApplicant()
    {
        currentApplicantNumber++;

        
        if (currentApplicantNumber > applicantAmount)
        {
            currentApplicantNumber = 1;
        }    
        
        Debug.Log($"Next applicant, current number: {currentApplicantNumber}");
        
        // Update UI
        UpdateApplicantUI();
    }
    
    private void PreviousApplicant()
    {
        currentApplicantNumber--;
        
        if (currentApplicantNumber < 1)
        {
            currentApplicantNumber = applicantAmount;
        }
        
        Debug.Log($"Previous applicant, current number: {currentApplicantNumber}");
        
        // Update UI
        UpdateApplicantUI();
    }
    
    // Method for when the user presses the done button. The system will go to the next step.
    private void CreateApplicantsDoneButton()
    {
        Debug.Log("Done button pressed");
        
        // Disable the applicant notes UI
        gameObject.SetActive(false);
        
        // Show the connect to server UI
        _gameManager.ShowConnectUI();
    }
    
    private void UpdateApplicantUI()
    {
        // Update the text for the current applicant
        currentApplicantText.text = $"Applicant {currentApplicantNumber}";
        
        // Update the colour for the current applicant
        currentApplicantColorImage.color = applicantColours[currentApplicantNumber - 1];
        
        // Update the notes for the current applicant
        applicantNotesText.text = applicantNotes[currentApplicantNumber - 1];
    }
    
}
