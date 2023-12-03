using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class KeyboardInput : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public TMP_InputField inputField;
    private TouchScreenKeyboard keyboard;

    void Start()
    {
        if (inputField == null)
            inputField = GetComponent<TMP_InputField>();

        if (inputField == null)
        {
            Debug.LogError("KeyboardInput: TMP_InputField component missing from the GameObject.", this);
            return;
        }

        inputField.onEndEdit.AddListener(HandleEndEdit);
        keyboard.text = inputField.text;
    }

    void Update()
    {
        if (keyboard != null && keyboard.active && !keyboard.done)
        {
            inputField.text = keyboard.text;
        }

        if (keyboard != null && keyboard.done && !keyboard.wasCanceled)
        {
            HandleEndEdit(keyboard.text);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        keyboard = TouchScreenKeyboard.Open(inputField.text, TouchScreenKeyboardType.Default);
    }

    private void HandleEndEdit(string text)
    {

    }

    public void OnDeselect(BaseEventData eventData)
    {

    }
}