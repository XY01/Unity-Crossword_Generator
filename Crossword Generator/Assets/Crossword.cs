using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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
    List<WordQuestionPair> _PlacedWords = new List<WordQuestionPair>();

    int _WordsPlaced = 0;

    public float _RoutineWaitTime = .01f;

    // Use this for initialization
    void Start ()
    {
        PopualteGrid();
        CreateDebugList();
        // PopulateWords();

        _WordQuestionPairs = _WordQuestionPairs.OrderByDescending(n => n._Word.Length).ToList();

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

    /*
    void PopulateWords()
    {
        for (int i = 0; i < _WordQuestionPairs.Count; i++)
        {
            SearchForPlacement(_WordQuestionPairs[i]);
        }
    }
    */

    IEnumerator PopulateWordsRoutine()
    {
        TryPlaceWordDown(_WordQuestionPairs[0], (_GridAxisCount / 2) - (_WordQuestionPairs[0]._Word.Length/2), (_GridAxisCount / 2));

        for (int i = 1; i < _WordQuestionPairs.Count; i++)
        {
            yield return StartCoroutine(SearchForPlacementRoutine(_WordQuestionPairs[i]));
            yield return new WaitForSeconds(_RoutineWaitTime);
        }

        for (int i = 0; i < _GridAxisCount; i++)
        {
            for (int j = 0; j < _GridAxisCount; j++)
            {
                if (_CrosswordSpaceGrid[i, j]._Type == CrosswordSpace.Type.Unoccupied)
                    _CrosswordSpaceGrid[i, j].Block();
            }
        }
    }

    IEnumerator SearchForPlacementRoutine(WordQuestionPair word)
    {
        bool wordPlaced = false;        

        CrosswordSpace currentSpace = _CrosswordSpaceGrid[0,0];

        #region Search along previouis words

        //_PlacedWords = _PlacedWords.OrderByDescending(n => n._Word.Length).ToList();

        for (int i = 0; i < _PlacedWords.Count; i++)
        {
            WordQuestionPair placedWord = _PlacedWords[i];
            for (int j = 0; j < placedWord._Word.Length; j++)
            {
                if(placedWord._Alignment == Alignment.Across)
                {
                    // Search for placements across
                    currentSpace = _CrosswordSpaceGrid[placedWord._XPos + j, placedWord._YPos];
                    currentSpace.Highlight(true);

                    wordPlaced = TryPlaceWordDown(word, placedWord._XPos + j, placedWord._YPos);

                    yield return new WaitForSeconds(_RoutineWaitTime);
                    currentSpace.Highlight(false);
                }
                else
                {
                    // Search for placements down
                    currentSpace = _CrosswordSpaceGrid[placedWord._XPos, placedWord._YPos + j];
                    currentSpace.Highlight(true);

                    wordPlaced = TryPlaceWordAcross(word, placedWord._XPos, placedWord._YPos + j);

                    yield return new WaitForSeconds(_RoutineWaitTime);
                    currentSpace.Highlight(false);
                }

                if (wordPlaced) break;
            }

            if (wordPlaced) break;
        }

        #endregion

        /*
        #region Search left/top edges of previouis words

        //_PlacedWords = _PlacedWords.OrderByDescending(n => n._Word.Length).ToList();

        for (int i = 0; i < _PlacedWords.Count; i++)
        {
            WordQuestionPair placedWord = _PlacedWords[i];
            for (int j = 0; j < placedWord._Word.Length; j++)
            {
                if (placedWord._Alignment == Alignment.Across && placedWord._YPos > 0)
                {
                    // Search for placements across
                    currentSpace = _CrosswordSpaceGrid[placedWord._XPos + j, placedWord._YPos-1];
                    currentSpace.Highlight(true);

                    wordPlaced = TryPlaceWordDown(word, placedWord._XPos + j, placedWord._YPos-1);

                    yield return new WaitForSeconds(_RoutineWaitTime);
                    currentSpace.Highlight(false);
                    if (wordPlaced) break;      
                }
                else if (placedWord._Alignment == Alignment.Down && placedWord._XPos > 0)
                {
                    // Search for placements downward
                    currentSpace = _CrosswordSpaceGrid[placedWord._XPos - 1, placedWord._YPos + j];
                    currentSpace.Highlight(true);

                    wordPlaced = TryPlaceWordAcross(word, placedWord._XPos - 1, placedWord._YPos + j);

                    yield return new WaitForSeconds(_RoutineWaitTime);
                    currentSpace.Highlight(false);
                }

                if (wordPlaced) break;
            }

            if (wordPlaced) break;
        }

        #endregion

        #region Search right/bottom edges of previouis words

        for (int i = 0; i < _PlacedWords.Count; i++)
        {
            WordQuestionPair placedWord = _PlacedWords[i];
            for (int j = 0; j < placedWord._Word.Length; j++)
            {
                if (placedWord._Alignment == Alignment.Across && placedWord._YPos < _GridAxisCount - 2)
                {
                    // Search for placements across
                    currentSpace = _CrosswordSpaceGrid[placedWord._XPos + j, placedWord._YPos + 1];
                    currentSpace.Highlight(true);

                    wordPlaced = TryPlaceWordDown(word, placedWord._XPos + j, placedWord._YPos + 1);

                    yield return new WaitForSeconds(_RoutineWaitTime);
                    currentSpace.Highlight(false);
                    if (wordPlaced) break;
                }
                else if(placedWord._Alignment == Alignment.Down && placedWord._XPos < _GridAxisCount - 2)
                {
                    // Search for placements downward
                    currentSpace = _CrosswordSpaceGrid[placedWord._XPos + 1, placedWord._YPos + j];
                    currentSpace.Highlight(true);

                    wordPlaced = TryPlaceWordAcross(word, placedWord._XPos + 1, placedWord._YPos + j);

                    yield return new WaitForSeconds(_RoutineWaitTime);
                    currentSpace.Highlight(false);
                }

                if (wordPlaced) break;
            }

            if (wordPlaced) break;
        }

        #endregion
    */
        int randXOffset = Random.Range(0, _GridAxisCount);
        int randYOffset = Random.Range(0, _GridAxisCount);

        if (!wordPlaced)
        {
            // Search within the space that can fit the word
            for (int y = 0; y < _GridAxisCount; y++)
            {
                for (int x = 0; x < _GridAxisCount; x++)
                {
                    int xOffset = x + randXOffset;
                    if (xOffset >= _GridAxisCount) xOffset -= _GridAxisCount;

                    int yOffset = y + randYOffset;
                    if (yOffset >= _GridAxisCount) yOffset -= _GridAxisCount;

                    // Highlight current space we are searching
                    currentSpace = _CrosswordSpaceGrid[xOffset, yOffset];
                    currentSpace.Highlight(true);

                    wordPlaced = TryPlaceWordAcross(word, xOffset, yOffset);
                    if (!wordPlaced) wordPlaced = TryPlaceWordDown(word, x, y);

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
    }
    
    // Trys to place a word as a specific position by searching the adjascent grid spaces
    bool TryPlaceWordAcross(WordQuestionPair word, int xPos, int yPos)
    {
        //Debug.Log("Trying to place ACROSS: " + word._Word + " at: " + xPos + " - " + yPos);

        int letterIndex = 0;
        CrosswordSpace currentSpace;

        CrosswordSpace aboveSpace;
        CrosswordSpace belowSpace;
        CrosswordSpace rightSpace;

        WordQuestionPair existingWord = null;

        currentSpace = _CrosswordSpaceGrid[xPos, yPos];

        if (_PlacedWords.Count > 0)
        {
            if (currentSpace._AcrossWord != null) return false;
            if (currentSpace._DownWord != null) existingWord = currentSpace._DownWord;

            if (existingWord != null && currentSpace._Letter != word._Word[letterIndex]) return false;
        }

        // if on a blocker return
        if (currentSpace._Type == CrosswordSpace.Type.Blocked) return false; 

        // Word too long from this position - FALSE
        if (xPos + word._Word.Length >= _GridAxisCount) return false;

        // On a word that is across - FALSE
        if (existingWord != null && existingWord._Alignment == Alignment.Across) return false;

        // Not edge column of grid and the space to the LEFT has a letter - FALSE
        if(xPos > 0 && _CrosswordSpaceGrid[xPos - 1, yPos]._Type == CrosswordSpace.Type.Letter) return false;
        
        // Not edge row of grid and space ABOVE has an across word - FALSE
        if (yPos > 0 && _CrosswordSpaceGrid[xPos, yPos - 1]._AcrossWord != null) return false;

        // Not edge row of grid and the space BELOW has an across word - FALSE
        if (yPos < _GridAxisCount - 1 && _CrosswordSpaceGrid[xPos, yPos + 1]._AcrossWord != null) return false;
        
        //print("Passed initial conditions");

        letterIndex++;

        for (int x = xPos + 1; x < xPos + word._Word.Length; x++)
        {
            currentSpace = _CrosswordSpaceGrid[x, yPos];

            // if the current space is occupied and letter doesn't match
            // or if current space is blocked, break
            if (currentSpace._Type == CrosswordSpace.Type.Letter && currentSpace._Letter != word._Word[letterIndex] ||
                currentSpace._Type == CrosswordSpace.Type.Blocked)
            {
               // print("Failed on current space blocked or letter");
                return false;
            }

            // Test space ABOVE
            if (yPos > 0)
            {
                aboveSpace = _CrosswordSpaceGrid[x, yPos - 1];
                if (aboveSpace._Type == CrosswordSpace.Type.Letter && aboveSpace._AcrossWord != null)
                {
                 //   print("Failed on above");
                    return false;
                }
            }

            // Test space BELOW
            if (yPos < _GridAxisCount-1)
            {
                belowSpace = _CrosswordSpaceGrid[x, yPos + 1];
                if (belowSpace._Type == CrosswordSpace.Type.Letter && belowSpace._AcrossWord != null)
                {
                  //  print("Failed on below");
                    return false;
                }
            }

            // if we are on the last letter then word can be placed
            if (letterIndex == word._Word.Length - 1)
            {
                // Test RIGHT space to make sure there are no letters
                if(xPos + word._Word.Length < _GridAxisCount)
                {
                    rightSpace = _CrosswordSpaceGrid[x + 1, yPos];
                    if (rightSpace._Type == CrosswordSpace.Type.Letter)
                    {
                        //print("Failed on end clearence");
                        return false;
                    }
                }


                PlaceWord(word, xPos, yPos, Alignment.Across);
                return true;
            }

            letterIndex++;
        }                

        return false;
    }

    // Trys to place a word as a specific position by searching the adjascent grid spaces
    bool TryPlaceWordDown(WordQuestionPair word, int xPos, int yPos)
    {
        //Debug.Log("Trying to place DOWN: " + word._Word + " at: " + xPos + " - " + yPos);

        int letterIndex = 0;
        CrosswordSpace currentSpace;
        
        CrosswordSpace belowSpace;
        CrosswordSpace leftSpace;
        CrosswordSpace rightSpace;

        WordQuestionPair existingWord = null;


        currentSpace = _CrosswordSpaceGrid[xPos, yPos];

        if (_PlacedWords.Count > 0)
        {
            if (currentSpace._AcrossWord != null) existingWord = currentSpace._AcrossWord;
            if (currentSpace._DownWord != null) return false;
            
            if (currentSpace._Letter != word._Word[letterIndex]) return false;
        }

        // if on a blocker return
        if (currentSpace._Type == CrosswordSpace.Type.Blocked) return false;

        // Word too long from this position - FALSE
        if (yPos + word._Word.Length >= _GridAxisCount) return false;

        // On a word that is down - FALSE
        if (existingWord != null && existingWord._Alignment == Alignment.Down) return false;

        // Not edge row of grid and the space to the ABOVE has a letter - FALSE
        if (yPos > 0 && _CrosswordSpaceGrid[xPos, yPos - 1]._Type == CrosswordSpace.Type.Letter) return false;

        // Not edge col of grid and space LEFT has an down word - FALSE
        if (xPos > 0 && _CrosswordSpaceGrid[xPos - 1, yPos]._DownWord != null) return false;

        // Not edge col of grid and the space BELOW has an down word - FALSE
        if (xPos < _GridAxisCount - 1 && _CrosswordSpaceGrid[xPos + 1, yPos]._DownWord != null) return false;

        print("Passed initial conditions");

        letterIndex++;

        for (int y = yPos + 1; y < yPos + word._Word.Length; y++)
        {
            currentSpace = _CrosswordSpaceGrid[xPos, y];

            // if the current space is occupied and letter doesn't match
            // or if current space is blocked, break
            if (currentSpace._Type == CrosswordSpace.Type.Letter && currentSpace._Letter != word._Word[letterIndex] ||
                currentSpace._Type == CrosswordSpace.Type.Blocked)
            {
                print("Failed on current space blocked or letter");
                return false;
            }

            // Test space LEFT
            if (xPos > 0)
            {
                leftSpace = _CrosswordSpaceGrid[xPos-1, y];
                if (leftSpace._Type == CrosswordSpace.Type.Letter && leftSpace._DownWord != null)
                {
                    print("Failed on left");
                    return false;
                }
            }

            // Test space RIGHT
            if (xPos < _GridAxisCount-1)
            {
                rightSpace = _CrosswordSpaceGrid[xPos+1, y];
                if (rightSpace._Type == CrosswordSpace.Type.Letter && rightSpace._DownWord != null)
                {
                    print("Failed on right");
                    return false;
                }
            }

            // if we are on the last letter then word can be placed
            if (letterIndex == word._Word.Length - 1)
            {
                Debug.Log("Testing clearance");

                // Test BELOW space to make sure there are no letters
                if (yPos + word._Word.Length < _GridAxisCount)
                {
                    belowSpace = _CrosswordSpaceGrid[xPos, y + 1];
                    if (belowSpace._Type == CrosswordSpace.Type.Letter)
                    {
                        Debug.Log("Failed on end clearence");
                        return false;
                    }
                }

                PlaceWord(word, xPos, yPos, Alignment.Down);
                return true;
            }

            letterIndex++;
        }

        print("Failed at end: " + letterIndex);
        return false;
    }
    
    // Places a word at a specific position and assigns the grid space references
    void PlaceWord(WordQuestionPair word, int posX, int posY, Alignment align)
    {
        CrosswordSpace currentSpace;

        word.Place(posX, posY, align);

        _PlacedWords.Add(word);

        _WordsPlaced++;

        print("Word placed: " + _WordsPlaced);

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
        _WordQuestionPairs.Add(new WordQuestionPair("PHANTOM", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("EUCALYPT", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("EMU", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("TAYLOR", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("EN", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("PARIS", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("LEAR", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("TRUSTWORTHY", "lorem ipsum blah blah lol"));     
        _WordQuestionPairs.Add(new WordQuestionPair("CHRISTOPHER", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("TROUT", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("OPTICAL", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("TOP", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("SILVER", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("INSECTS", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("CLEOPATRA", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("TEA", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("BEG", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("PLUS", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("TWICE", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("CUTLERY", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("KID", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("WOBBLY", "lorem ipsum blah blah lol"));

        _WordQuestionPairs.Add(new WordQuestionPair("WOBBLY", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("SUNFLOWER", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("LABRADOR", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("HABIT", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("FRY", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("SCHEDULE", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("BETHLEHEM", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("MORTAR", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("ICE", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("DRYER", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("ALTAR", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("EROS", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("RINGS", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("BRUNETTE", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("MOUSE", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("AGGRESSIVE", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("BLAST", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("NURTURE", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("EXPAND", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("BIKE", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("CAMEMBERT", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("INSERT", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("CRUEL", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("ERASE", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("MENU", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("WHEAT", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("CALCULATOR", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("INEPT", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("CONTRACT", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("EDIT", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("ALIBI", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("GLORY", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("CHASE", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("ASSISTED", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("DROVE", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("STORM", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("HOUR", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("DIRTY", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("BLEACH", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("BOWLS", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("JOKE", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("NEW", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("CRIMINAL", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("CASTLE", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("PUNT", "lorem ipsum blah blah lol"));
        _WordQuestionPairs.Add(new WordQuestionPair("CARPET", "lorem ipsum blah blah lol"));
    }

}
