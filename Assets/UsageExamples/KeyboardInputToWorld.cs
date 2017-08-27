using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardInputToWorld : MonoBehaviour {

    protected TextToWorld _textToWorld;

    private List<SpriteRenderer> textToRender;
    private float    currBackspaceDelay;
    private bool bIsBackspaceDown;



    public void Start() {
        _textToWorld = GetComponent<TextToWorld>();
    }//Start


    public void Update() {
        HandleKeyboardInput();
    }//Update



    public void HandleKeyboardInput() {
        string dynamicText = _textToWorld.TextToRender;

        foreach (KeyCode kcode in System.Enum.GetValues(typeof(KeyCode))) {
            if (!Input.GetKeyDown(kcode) || Input.GetKeyDown(KeyCode.Backspace))
                continue;

            char letter = kcode.ToString()[0];
            if (kcode.ToString() == "Space")
                letter = ' ';

            if (kcode.ToString().Length > 1 && kcode != KeyCode.Space) {
                if (kcode.ToString().Contains("Alpha"))
                    letter = kcode.ToString()[5];
                else
                    continue;
            }//if

            bool isCharAdded = _textToWorld.AddCharacter(letter);
            if (!isCharAdded)
                continue;

            dynamicText += letter; //FOR DEBUGGING
            _textToWorld.RenderText();
        }//foreach

        if (Input.GetKeyDown(KeyCode.Backspace))
            bIsBackspaceDown = true;
        if (Input.GetKeyUp(KeyCode.Backspace)) {
            bIsBackspaceDown = false;
            currBackspaceDelay = 0;
        }

        if (bIsBackspaceDown && currBackspaceDelay == 0) {
            bool wasCharRemoved = _textToWorld.RemoveLetter();
            if (wasCharRemoved) {
                _textToWorld.RenderText();
                dynamicText = dynamicText.Remove(dynamicText.Length - 1);
            }//if
        }//if

        if (bIsBackspaceDown) {
            currBackspaceDelay = (currBackspaceDelay >= 0.05f) ? 0 : currBackspaceDelay + Time.deltaTime;
        }

        _textToWorld.TextToRender = dynamicText;
    }//HandleKeyboardInput

}//class
