using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reference
/// https://www.codeproject.com/Articles/530853/Creating-a-crossword-generator
/// </summary>

public class WordQuestionPair
{
    public Crossword.Alignment _Alignment = Crossword.Alignment.Across;
    public int _XPos;
    public int _YPos;

    public string _Word;
    public string _Questions;

  

    public WordQuestionPair(string word, string question)
    {
        _Word = word;
        _Questions = question;
    }

    public void Place(int xPos, int yPos, Crossword.Alignment align)
    {
        _XPos = xPos;
        _YPos = yPos;
        _Alignment = align;

        Debug.Log(_Word + "  placed at: " + xPos + " - " + yPos + "  alignment: " +  align);
    }
}

public class Crossword : MonoBehaviour
{
    public enum Alignment
    {
        Across,
        Down,
    }

    public GridLayoutGroup _GridLayout;

    public int _GridAxisCount = 10;

    public CrosswordSpace _CrosswordSpace;

    CrosswordSpace[,] _CrosswordSpaceGrid;

    List<WordQuestionPair> _WordQuestionPairs = new List<WordQuestionPair>();

    int _WordsPlaced = 0;

    float _RoutineWaitTime = .03f;

    // Use this for initialization
    void Start ()
    {
        PopualteGrid();
        CreateDebugList();
        CreateDebugList();
        CreateDebugList();
        // PopulateWords();

        StartCoroutine(PopulateWordsRoutine());
    }

    void PopualteGrid()
    {
        _CrosswordSpaceGrid = new CrosswordSpace[_GridAxisCount, _GridAxisCount];

        float spaceSize = _GridLayout.GetComponent<RectTransform>().sizeDelta.x / (float)_GridAxisCount;

        _GridLayout.cellSize = new Vector2(spaceSize, spaceSize);

        for (int i = 0; i < _GridAxisCount; i++)
        {
            for (int j = 0; j < _GridAxisCount; j++)
            {
                CrosswordSpace space = Instantiate(_CrosswordSpace) as CrosswordSpace;
                space.name = "Space " + j + " - " + i;
                space.transform.SetParent(_GridLayout.transform);
                space.Init(spaceSize);
                _CrosswordSpaceGrid[j,i] = space;
            }
        }
    }

    void PopulateWords()
    {
        for (int i = 0; i < _WordQuestionPairs.Count; i++)
        {
            SearchForPlacement(_WordQuestionPairs[i]);
        }
    }

    IEnumerator PopulateWordsRoutine()
    {
        for (int i = 0; i < _WordQuestionPairs.Count; i++)
        {
            yield return StartCoroutine(SearchForPlacementRoutine(_WordQuestionPairs[i]));
            yield return new WaitForSeconds(_RoutineWaitTime);
        }
    }

    IEnumerator SearchForPlacementRoutine(WordQuestionPair word)
    {
        bool wordPlaced = false;
        CrosswordSpace currentSpace = _CrosswordSpaceGrid[0,0];

        // Search within the space that can fit the word
        for (int y = 0; y < _GridAxisCount; y++)
        {
            for(int x = 0; x < _GridAxisCount; x++)
            {
                // Highlight current space we are searching
                currentSpace = _CrosswordSpaceGrid[x, y];
                currentSpace.Highlight(true);

                // If there are some words placed already...
                if (_WordsPlaced > 0)
                {
                  
                    // First search for all spaces with the same first letter
                    if (currentSpace._Type == CrosswordSpace.Type.Letter &&
                        currentSpace._Letter == word._Word[0])
                    {
                        print("Matched first letter");
                        wordPlaced = TryPlaceWordAt(word, x, y);
                    }
                }

                yield return new WaitForSeconds(_RoutineWaitTime);

                // If the word hasn't been placed, then search for an unoccupied space
                if (!wordPlaced && currentSpace._Type == CrosswordSpace.Type.Unoccupied)
                {
                   wordPlaced = TryPlaceWordAt(word, x, y);
                }

                yield return new WaitForSeconds(_RoutineWaitTime);

                currentSpace.Highlight(false);
                // If the word is placed then return and stop searching
                if (wordPlaced) break;
            }

            currentSpace.Highlight(false);

            // If the word is placed then return and stop searching
            if (wordPlaced) break;            
        }
    }

    // Searches for a grid space in which a word can start
    bool SearchForPlacement(WordQuestionPair word)
    {
        bool wordPlaced = false;

        int maxXPos = _GridAxisCount - word._Word.Length;
        int maxYPos = maxXPos;


        // Search within the space that can fit the word
        for (int x = 0; x < _GridAxisCount; x++)
        {
            for (int y = 0; y < _GridAxisCount; y++)
            {
                // First search for all spaces with the same first letter
                if (_CrosswordSpaceGrid[x, y]._Type == CrosswordSpace.Type.Letter && 
                    _CrosswordSpaceGrid[x, y]._Letter == word._Word[0])
                {                 
                    wordPlaced = TryPlaceWordAt(word, x, y);
                }                

                // If the word hasn't been placed, then search for an unoccupied space
                if (!wordPlaced && _CrosswordSpaceGrid[x, y]._Type == CrosswordSpace.Type.Unoccupied)
                {
                    wordPlaced = TryPlaceWordAt(word, x, y);
                }

                // If the word is placed then return and stop searching
                if(wordPlaced)
                {
                    return wordPlaced;
                }
            }
        }

        return wordPlaced;
    }

    // Trys to place a word as a specific position by searching the grid spaces to see if they are empty
    // or contain a matching letter
    bool TryPlaceWordAt(WordQuestionPair word, int xPos, int yPos, WordQuestionPair existingWord = null)
    {
        Debug.Log("Trying to place word: " + word._Word + " at: " + xPos + " - " + yPos);

        int letterIndex = 0;
        CrosswordSpace currentSpace;


        CrosswordSpace spaceAbove;
        CrosswordSpace spaceLeft;
        CrosswordSpace spaceRight;


        
        if (existingWord == null ||
            existingWord != null && existingWord._Alignment == Alignment.Down)
        {
            print("Searching across");
            // Search across
            if (xPos + word._Word.Length <= _GridAxisCount)
            {
                for (int x = xPos; x < xPos + word._Word.Length; x++)
                {
                    currentSpace = _CrosswordSpaceGrid[x, yPos];

                    // if the current space is occupied and letter doesn't match
                    // or if current space is blocked, break
                    if (currentSpace._Type == CrosswordSpace.Type.Letter && currentSpace._Letter != word._Word[letterIndex] ||
                        currentSpace._Type == CrosswordSpace.Type.Blocked)
                    {
                        print("Failed on current space blocked or letter");
                        break;
                    }

                    // test space above
                    if (yPos > 0 && letterIndex > 0)
                    {
                        spaceAbove = _CrosswordSpaceGrid[x, yPos - 1];
                        if (spaceAbove._Type == CrosswordSpace.Type.Letter && spaceAbove._AcrossWord != null)
                        {
                            print("Failed on above");
                            break;
                        }
                    }

                    // test space left
                    if (xPos > 0)
                    {
                        spaceLeft = _CrosswordSpaceGrid[xPos - 1, yPos];
                        if (spaceLeft._Type == CrosswordSpace.Type.Letter)
                        {
                            print("Failed on left");
                            break;
                        }
                    }


                    // test space right
                    if (x < _GridAxisCount - 1)
                    {
                        spaceRight = _CrosswordSpaceGrid[x+1, yPos];
                        if (spaceRight._Type == CrosswordSpace.Type.Letter)
                        {
                            print("Failed on right");
                            break;
                        }
                    }


                    // if we are on the last letter then word can be placed
                    if (letterIndex == word._Word.Length - 1)
                    {
                        PlaceWord(word, xPos, yPos, Alignment.Across);
                        return true;
                    }

                    letterIndex++;
                }
            }
        }
        
        
        // Reset letter index
        letterIndex = 0;

        print("Searching down");

        if (existingWord == null ||
          existingWord != null && existingWord._Alignment == Alignment.Across)
        {
            if (yPos + word._Word.Length <= _GridAxisCount)
            {
                // Search down
                for (int y = yPos; y < yPos + word._Word.Length; y++)
                {
                    currentSpace = _CrosswordSpaceGrid[xPos, y];

                    // if the current space is occupied and letter doesn't match
                    // or if current space is blocked, break
                    if (currentSpace._Type == CrosswordSpace.Type.Letter && currentSpace._Letter != word._Word[letterIndex] ||
                        currentSpace._Type == CrosswordSpace.Type.Blocked)
                    {
                        print("Failed on blocked or letter on current");
                        break;
                    }

                    if (yPos > 0)
                    {
                        // test space above
                        spaceAbove = _CrosswordSpaceGrid[xPos,yPos - 1];
                        if (spaceAbove._Type == CrosswordSpace.Type.Letter)
                        {
                            print("Failed on space above");
                            break;
                        }
                    }

                    // test space left
                    // If not in the first column an
                    
                    if (xPos > 0 && letterIndex > 0 ||
                        xPos > 0 && existingWord == null)
                    {
                        spaceLeft = _CrosswordSpaceGrid[xPos - 1, y];
                        if (spaceLeft._Type == CrosswordSpace.Type.Letter)
                        {
                            print("Failed on space left");
                            break;
                        }
                    }

                    // test space right
                    if (xPos < _GridAxisCount - 1 && letterIndex > 0)
                    {
                        spaceRight = _CrosswordSpaceGrid[xPos + 1, y];
                        if (spaceRight._Type == CrosswordSpace.Type.Letter)
                        {
                            print("Failed on space right");
                            break;
                        }
                    }


                    // if we are on the last letter then word can be placed
                    if (letterIndex == word._Word.Length - 1)
                    {
                        PlaceWord(word, xPos, yPos, Alignment.Down);
                        return true;
                    }

                    letterIndex++;
                }
            }
        }
        
        

        return false;
    }

    // Places a word at a specific position and assigns the grid space references
    void PlaceWord(WordQuestionPair word, int posX, int posY, Alignment align)
    {
        CrosswordSpace currentSpace;

        word.Place(posX, posY, align);

        _WordsPlaced++;
        print("Words placed: " + _WordsPlaced);

        if (align == Alignment.Across)
        {
            for (int i = 0; i < word._Word.Length; i++)
            {
                currentSpace = _CrosswordSpaceGrid[posX + i, posY];
                currentSpace.PlaceWord(word, i, align);               
            }

            // Block spaces before and after the word
            if(posX - 1 >= 0)
                _CrosswordSpaceGrid[posX - 1, posY].Block();

            if (posX + word._Word.Length < _GridAxisCount)
                _CrosswordSpaceGrid[posX + word._Word.Length, posY].Block();
        }
        else
        {
            for (int i = 0; i < word._Word.Length; i++)
            {
                currentSpace = _CrosswordSpaceGrid[posX, posY + i];
                currentSpace.PlaceWord(word, i, align);
            }

            // Block spaces before and after the word
            if (posY - 1 >= 0)
                _CrosswordSpaceGrid[posX, posY - 1].Block();

            if (posY + word._Word.Length < _GridAxisCount)
                _CrosswordSpaceGrid[posX, posY + word._Word.Length].Block();
        }
    }

    void CreateDebugList()
    {
        _WordQuestionPairs.Add(new WordQuestionPair("TESTING", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("TOOT", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("TOOT", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("TOOT", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("TOOT", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("TRAFFIC", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("TECH", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("TOTES", "lorem ipsum blah blah lol"));     
        _WordQuestionPairs.Add(new WordQuestionPair("TRAIN", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("GREETINGS", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("FLOAT", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("BALLOON", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("FRENZY", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("LOOPING", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("RAD", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("REALISTIC", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("FAKE", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("CREAM", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("CORDIAL", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("SPLIT", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("CRACK", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("TREE", "lorem ipsum blah blah lol"));
    }

}
