using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CrosswordSpace : MonoBehaviour
{
    public enum Type
    {
        Unoccupied,
        Letter,
        Blocked,
    }

    RectTransform _Rect;
    public Image _Image;

    public Text _Text;

    public Type _Type = Type.Unoccupied;
    
    public char _Letter;

    public WordQuestionPair _AcrossWord;
    public WordQuestionPair _DownWord;
    

    public bool _Highlight = false;

    // Use this for initialization
    public void Init(float size)
    {
        _Rect = GetComponent<RectTransform>();
        _Rect.sizeDelta = new Vector2(size, size);
        _Type = Type.Unoccupied;
        _Letter = ')';
    }

    public void PlaceWord(WordQuestionPair word, int letterIndex, Crossword.Alignment alignment)
    {
        // Assign word to across or down so it can access the question when highlighted
        if (alignment == Crossword.Alignment.Across) _AcrossWord = word;
        else _DownWord = word;


        _Letter = word._Word[letterIndex];

        _Type = Type.Letter;
        _Image.color = Color.white;
        _Text.text = _Letter.ToString();
    }

    public void Block()
    {
        _Type = Type.Blocked;
        _Image.color = Color.black;
    }

    public void Highlight(bool highlight)
    {
        _Highlight = highlight;

        if (_Highlight) _Image.color = Color.red;
        else
        {
            if(_Type == Type.Blocked)
                _Image.color = Color.black;
            else if (_Type == Type.Unoccupied)
                _Image.color = Color.gray;
            else if (_Type == Type.Letter)
                _Image.color = Color.white;
        }
    }
}
