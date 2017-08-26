using System.Collections.Generic;
using UnityEngine;


public class NumberSprites: MonoBehaviour {

    protected Vector2 AlphabetUCaseRange   = new Vector2(65, 90);
    protected Vector2 SpecialCharRange     = new Vector2(32, 47);

    [Tooltip("Element in the array represents the a number sprite.")]
    public Sprite[] Numbers;
    [Tooltip("Upper Case alphabet. Element 0 is A, which is = (65(hex val) - 65). B = 66 - 65.")]
    public Sprite[] AlphabetU;
    public Sprite[] SpecialChars;
    public float    LetterSpacing = 0.1f;
    public string   SortingLayerName;
    public int      OrderInLayer;
    [Tooltip("'Pixel Per Unit' value used on the sprite itself.")]
    public int      PixelPerUnit = 100;
    public string   TextToRender = "1";
    public Vector3  Scale = Vector3.one;
    [Tooltip("Allow runtime text rendering by listening to the keyboard inputs.")]

    public bool     AllowKeyboardInput = false;
    public float    currBackspaceDelay;


    private List<SpriteRenderer> textToRender;
    private bool bIsBackspaceDown;


    public void Start() {
        textToRender = new List<SpriteRenderer>();
        foreach (char letter in TextToRender)
            AddCharacter(letter);
        RenderText();
    }//Start


    public void Update() {
        if(AllowKeyboardInput)
            HandleKeyboardInput();
    }//Update


    public void HandleKeyboardInput() {
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

            bool isCharAdded = AddCharacter(letter);
            if (!isCharAdded)
                continue;

            TextToRender += letter; //FOR DEBUGGING
            RenderText();
        }//foreach

        if (Input.GetKeyDown(KeyCode.Backspace))
            bIsBackspaceDown = true;
        if (Input.GetKeyUp(KeyCode.Backspace)) {
            bIsBackspaceDown = false;
            currBackspaceDelay = 0;
        }

        if (bIsBackspaceDown && currBackspaceDelay == 0) {
            bool wasCharRemoved = RemoveLetter();
            if (wasCharRemoved) {
                RenderText();
                TextToRender = TextToRender.Remove(TextToRender.Length - 1);
            }//if
        }//if

        if (bIsBackspaceDown) {
            currBackspaceDelay = (currBackspaceDelay >= 0.05f) ? 0 : currBackspaceDelay + Time.deltaTime;
        }
    }//HandleKeyboardInput


    public void RenderText() {
        Vector2 prevLetterPos = Vector3.zero;
        Vector2 prevLetterSize = Vector3.zero;
        for (int i = 0; i < textToRender.Count; i++) {
            SpriteRenderer letter = textToRender[i];
            Vector2 letterNewPos = Vector2.zero;
            if (i == 0)
                letterNewPos = Vector2.zero;
            if (i > 0) {
                var offset = prevLetterPos.x + prevLetterSize.x + LetterSpacing;
                letterNewPos = new Vector2(offset, 0);
            }

            letter.transform.localPosition = letterNewPos;
            prevLetterSize = letter.sprite.rect.size / PixelPerUnit;
            prevLetterPos = letter.transform.localPosition;
            letter.gameObject.SetActive(true);
        }//for
    }//RenderText


    public bool AddCharacter(char letter) {
        int number = -1;
        SpriteRenderer createdLetter = null;
        //Parse Number character
        if (int.TryParse(letter.ToString(), out number)) {
            var createdNumber = CreateLetter(number, ref Numbers);
            if (createdNumber == null)
                return false;

            textToRender.Add(createdNumber);
            return true;
        }
        //Parse Alphabetic characters

        if (IsSpecialChar(letter)) {
            int hexCode = letter;
            if (hexCode != 32) //hex code for Space
                return false;

            var arrayIndex = hexCode - Mathf.FloorToInt(SpecialCharRange.x);
            createdLetter = this.CreateLetter(arrayIndex, ref SpecialChars);
            if (createdLetter == null)
                return false;

            textToRender.Add(createdLetter);
            return true;
        }//if special char

        char upperLetter = letter.ToString().ToUpper()[0]; //make it upper, cause no lowercase support here.

        bool isLetter = IsUpperLetter(upperLetter);
        if (isLetter) {
            var arrayIndex = upperLetter - Mathf.FloorToInt(AlphabetUCaseRange.x);
            createdLetter = this.CreateLetter(arrayIndex, ref AlphabetU);
            if (createdLetter == null)
                return false;

            textToRender.Add(createdLetter);
        }//if letter

        return true;
    }//BuildText


    /// <summary>
    ///  Check if symbol is between hex values of 32 and 47 (both inclusive).
    /// This is a special character range, based of: http://www.asciitable.com/
    /// </summary>
    public bool IsSpecialChar(char symbol) {
        int hexIndex = symbol;
        return hexIndex >= SpecialCharRange.x && hexIndex <= SpecialCharRange.y;
    }//IsSpecialChar


    /// <summary>
    ///  Check if between hex value of 65 and 90 (both inclusive) which
    ///  represent the Upper Case alphabetic characters: http://www.asciitable.com/
    /// </summary>
    public bool IsUpperLetter(char symbol) {
        int hexIndex = symbol;
        return hexIndex >= AlphabetUCaseRange.x && hexIndex <= AlphabetUCaseRange.y;
    }//IsUpperLetter


    protected SpriteRenderer CreateLetter(int index, ref Sprite[] letterSprites) {
        bool isValidNumber = this.validateArrayIndex(index, ref letterSprites);
        if (!isValidNumber)
            return null;

        Sprite spriteRepr = letterSprites[index];
        SpriteRenderer spriteRenderer = CreateFontObject();
        spriteRenderer.sprite = spriteRepr;
        spriteRenderer.sortingLayerName = SortingLayerName;
        spriteRenderer.sortingOrder = OrderInLayer;

        return spriteRenderer;
    }//CreateNumber


    protected SpriteRenderer CreateFontObject() {
        GameObject numberGO = new GameObject();
        numberGO.AddComponent<SpriteRenderer>();

        SpriteRenderer spriteRenderer = numberGO.GetComponent<SpriteRenderer>();
        spriteRenderer.sortingLayerName = SortingLayerName;
        spriteRenderer.sortingOrder = OrderInLayer;

        numberGO.transform.parent = this.transform;
        numberGO.transform.localScale = Scale;
        numberGO.SetActive(false);
        numberGO.transform.localPosition = Vector3.zero;

        return spriteRenderer;
    }//CreateWorldFont


    public bool RemoveLetter(int letterPosition=-1) {
        if (letterPosition == -1)
            letterPosition = textToRender.Count - 1;

        if (letterPosition >= textToRender.Count)
            return false;

        if (textToRender.Count == 0)
            return false;

        SpriteRenderer letter = textToRender[letterPosition];
        textToRender.RemoveAt(letterPosition);
        Destroy(letter.gameObject);
        return true;
    }//RemoveLetter


    protected bool validateArrayIndex(int index, ref Sprite[] target) {
        if(target.Length == 0) {
            #if UNITY_EDITOR
                Debug.LogWarning("Array has no sprites set!");
            #endif
            return false;
        }
        if (index >= target.Length || index < 0) {
            #if UNITY_EDITOR
                Debug.LogWarning("Out Of range! Passed " + index + " for the array  " + target.Length);
            #endif
            return false;
        }//if length

        if (target[index] == null) {
            #if UNITY_EDITOR
                if (target[index] == null)
                    Debug.LogWarning("Array with index '" + index + "' has no sprite representation!");
            #endif
            return false;
        }//if null

        return true;
    }//validateArrayIndex

}//class
