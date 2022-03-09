using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;

public class InputHandler : MonoBehaviour {
    public InputField _inputField;
    public Button _button;
    public Text _outputField;
    private FixedString128Bytes _message;
    public bool _IsClicked { get; set; }

    // Start is called before the first frame update
    void Start() {
        _button.onClick.AddListener(SetMessage);
    }

    public void SetTitle(string findByName, string title) {
        GameObject.Find(findByName).GetComponent<Text>().text = title;
    }

    public void SetMessage() {
        _message = _inputField.text;
        _IsClicked = true;
    }

    public FixedString128Bytes GetMessage() {
        return _message;
    }

    public void SetButtonLabel(string buttonLabel) {
        _button.GetComponentInChildren<Text>().text = buttonLabel;
    }

    public void SetOutputField(FixedString128Bytes message) {
        string strMessage = message.ToString();
        _outputField.text = strMessage;
    }
}

