using TMPro;  // Required to work with TextMeshPro
using UnityEngine;

public class LiveInputHandler : MonoBehaviour
{
    // Reference to the TextMeshPro Input Field
    public TMP_InputField userInputField;

    // Reference to a TextMeshProUGUI text to display live input (optional)
    public TextMeshProUGUI displayLiveText;

    void Start()
    {
        // Add listener to the onValueChanged event to capture live input
        userInputField.onValueChanged.AddListener(OnUserInputChanged);
    }

    // Method to handle the live input changes
    void OnUserInputChanged(string input)
    {
        // Do something with the live input (e.g., log or display)
        Debug.Log("Live User Input: " + input);

        // Optionally display the input in a TextMeshProUGUI text
        if (displayLiveText != null)
        {
            displayLiveText.text =  input;
            OpenBett app = FindObjectOfType<OpenBett>();
            int intValue = int.Parse(input);
            app.QuantityAddLive(intValue);
        }
    }

    void OnDestroy()
    {
        // Remove listener when the object is destroyed
        userInputField.onValueChanged.RemoveListener(OnUserInputChanged);
    }
}
