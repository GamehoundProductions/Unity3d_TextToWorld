using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  This script lets you render text\font in a form of Sprite to the World space
/// based on the input string. It is particularly useful when you need to display
/// text in the world space that changes during the runtime.
/// </summary>
public class TextToWorld: MonoBehaviour {

    protected Vector2 AlphabetUCaseRange   = new Vector2(65, 90);
    protected Vector2 SpecialCharRange     = new Vector2(32, 47);

    [Tooltip("Element in the array represents the a number sprite.")]
    public Sprite[] Numbers;
    [Tooltip("Upper Case alphabet. Hex range from 65 to 90. where 65 is element 0 of the array.")]
    public Sprite[] AlphabetU;
    [Tooltip("Special Characters sprites. Hex range from 32 to 47. where 32 is element 0 of the array.")]
    public Sprite[] SpecialChars;
    [Tooltip("Space between letters.")]
    public float    LetterSpacing = 0.1f;
    [Tooltip("SpriteRenderer 'Sorting layer'.")]
    public string   SortingLayerName;
    [Tooltip("SpriteRenderer 'Order in layer'.")]
    public int      OrderInLayer;
    [Tooltip("'Pixel Per Unit' value used on the sprite itself.")]
    public int      PixelPerUnit = 100;
    [Tooltip("Text to be rendered in the world space using Sprite arrays.")]
    public string   TextToRender = "Hello World";
    [Tooltip("Scale for each letter.")]
    public Vector3  Scale = Vector3.one;

    private List<SpriteRenderer> textToRender;


    public void Start() {
        textToRender = new List<SpriteRenderer>();
        foreach (char letter in TextToRender)
            AddCharacter(letter);
        RenderText();
    }//Start


    /// <summary>
    ///  
    /// </summary>
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


    /// <summary>
    ///  Validates array for out of bounds and for null sprite
    /// at the index's position. I just want a slightly more
    /// readable message to be displayed with less panic when
    /// things go wrong...
    /// </summary>
    /// <param name="index">Index of the array to validate.</param>
    /// <param name="target">Array to validate on.</param>
    /// <returns>True if everything is good. 
    ///          False - something went wrong.
    /// </returns>
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


}//class
