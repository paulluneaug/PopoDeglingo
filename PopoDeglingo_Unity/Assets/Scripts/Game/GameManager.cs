using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityUtility.Extensions;

/// <summary>
/// Handles the game logic
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField] private float m_timer = 300;
    [SerializeField] private SpriteRenderer m_characterSprite;
    [SerializeField] private Animator m_characterAnimator;
    [SerializeField] private TextMeshProUGUI m_dialogText;
    [SerializeField] private AudioSource customerSource;

    [SerializeField] private CharacterData oldManDataIntro;
    [SerializeField] private CharacterData oldManDataOutro;

    [SerializeField] private IngredientManager m_ingredientsManager;
    [SerializeField] private InputActionReference m_startGameInput;
    [SerializeField] private InputActionReference m_replayVoiceInput;


    private bool m_running = false;
    private IngredientType[] m_currentIngredients;
    private PotionRecipe m_currentRecipe;
    private CharacterData m_currentCharacter;

    private bool m_canCheckPotions;
    private bool m_awaitingCleaning;
    private GameTimer m_gameTimer;
    private Coroutine m_cutscene;
    private CharacterData.DialogData m_cachedDialog;

    private PotionRecipe[] m_allRecipes;
    private List<CharacterData> m_allCharacters;

    private int potionGood;
    private int potionBad;


    private void Start()
    {
        m_running = false;
        m_awaitingCleaning = false;
        m_canCheckPotions = false;
        m_currentIngredients = new IngredientType[3];
        m_allRecipes = Resources.LoadAll<PotionRecipe>("Recipes/");
        m_allCharacters = new List<CharacterData>(Resources.LoadAll<CharacterData>("Characters/"));

        m_gameTimer = new GameTimer(0, null);

        m_startGameInput.action.performed += OnStartGamePerformed;
        m_replayVoiceInput.action.performed += OnReplayVoicePerformed;

        Display.displays.ForEach(display => display.Activate());
    }

    private void OnDestroy()
    {
        m_startGameInput.action.performed -= OnStartGamePerformed;
        m_replayVoiceInput.action.performed -= OnReplayVoicePerformed;
    }

    private void OnStartGamePerformed(InputAction.CallbackContext context)
    {
        StartGame();
    }

    private void OnReplayVoicePerformed(InputAction.CallbackContext context)
    {
        RedoVoice();
    }

    /// <summary>
    /// Starts the game
    /// </summary>
    public void StartGame()
    {
        m_running = true;
        m_canCheckPotions = false;
        m_awaitingCleaning = false;
        potionBad = 0;
        potionGood = 0;
        StartIntro();
    }

    /// <summary>
    /// Ends the game
    /// </summary>
    public void EndGame()
    {
        m_running = false;
        m_canCheckPotions = false;
        StartEnd();
    }

    /// <summary>
    /// Checks if the current ingredients are all empty
    /// </summary>
    /// <returns>True if they are empty</returns>
    public bool CurrentIngredientsAreAllEmpty()
    {
        return m_currentIngredients[0] == IngredientType.NOTHING &&
                m_currentIngredients[1] == IngredientType.NOTHING && m_currentIngredients[2] == IngredientType.NOTHING;
    }

    /// <summary>
    /// Replays the current voiceline
    /// </summary>
    public void RedoVoice()
    {
        if (m_cachedDialog != null && m_canCheckPotions) PlayDialog(m_cachedDialog);
    }

    /// <summary>
    /// Brings in the Introduction Old Man
    /// </summary>
    public void StartIntro()
    {
        if (m_cutscene != null)
        {
            StopAllCoroutines();
        }

        m_canCheckPotions = false;
        m_cutscene = StartCoroutine(Routine_BringInClient(oldManDataIntro));
    }

    /// <summary>
    /// Brings in the Introduction Old Man
    /// </summary>
    public void StartEnd()
    {
        if (m_cutscene != null)
        {
            StopAllCoroutines();
        }

        m_canCheckPotions = false;
        m_cutscene = StartCoroutine(Routine_BringInEnd(oldManDataOutro));
    }

    /// <summary>
    /// Brings in the next client
    /// </summary>
    public void BringNextClient()
    {
        if (m_cutscene != null)
        {
            StopAllCoroutines();
        }

        m_canCheckPotions = false;

        if (m_allCharacters.Count != 0)
        {
            // Still got people to sell to
            int idxChosen = Random.Range(0, m_allCharacters.Count);
            CharacterData chosen = m_allCharacters[idxChosen];
            m_allCharacters.RemoveAt(idxChosen);
            m_cutscene = StartCoroutine(Routine_BringInClient(chosen));
        }
        else
        {
            // No more people
            EndGame();
        }
    }

    /// <summary>
    /// Plays a dialog and caches it
    /// </summary>
    /// <param name="data">The dialog's data</param>
    public void PlayDialog(CharacterData.DialogData data, bool cacheDialog = false)
    {
        if (cacheDialog) m_cachedDialog = data;

        if (data == null)
        {
            m_dialogText.text = "";
        }
        else
        {
            customerSource.Stop();
            customerSource.clip = data.clip;
            customerSource.Play();
            m_dialogText.text = data.subtitles;
        }
    }

    /// <summary>
    /// Routine for the old man coming back
    /// </summary>
    /// <param name="data">The character's data</param>
    IEnumerator Routine_BringInEnd(CharacterData data)
    {
        m_canCheckPotions = false;
        m_characterSprite.sprite = data.CharacterSprite;
        m_characterAnimator.SetTrigger("In");

        for (int i = 0; i < m_currentCharacter.introDialogs.Length; i++)
        {
            PlayDialog(m_currentCharacter.introDialogs[i]);
            yield return new WaitForSeconds(m_currentCharacter.introDialogs[i].clip.length); // Change for clip length later
        }

        m_cutscene = null;
    }

    /// <summary>
    /// Routine for the client arriving
    /// </summary>
    IEnumerator Routine_BringInClient(CharacterData data)
    {
        m_currentCharacter = data;
        m_currentRecipe = data.linkedRecipe;
        m_canCheckPotions = false;
        m_characterSprite.sprite = m_currentCharacter.CharacterSprite;
        m_characterAnimator.SetTrigger("In");

        for (int i = 0; i < m_currentCharacter.introDialogs.Length; i++)
        {
            PlayDialog(m_currentCharacter.introDialogs[i]);
            yield return new WaitForSeconds(m_currentCharacter.introDialogs[i].clip.length); // Change for clip length later
        }

        PlayDialog(m_currentCharacter.potionDialog, true);
        yield return new WaitForSeconds(m_currentCharacter.potionDialog.clip.length); // Change for clip length later

        m_canCheckPotions = true;

        m_cutscene = null;
    }

    /// <summary>
    /// Routine for the client leaving
    /// </summary>
    /// <param name="data">The dialog data to play</param>
    IEnumerator Routine_ClientLeaves(CharacterData.DialogData[] data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            if (i == data.Length - 1) m_characterAnimator.SetTrigger("Out");
            PlayDialog(data[i]);
            yield return new WaitForSeconds(data[i].clip.length); // Change for clip length later
        }

        if (m_currentCharacter.resetTimerOnLeave) m_gameTimer.Reset(m_timer, EndGame);

        if (CurrentIngredientsAreAllEmpty())
        {
            BringNextClient();
        }
        else
        {
            m_characterAnimator.SetTrigger("Hide");
            PlayDialog(null);
            m_awaitingCleaning = true;
        }

        m_cutscene = null;
    }

    /// <summary>
    /// Routine for the client complaining and asking you to retry
    /// </summary>
    /// <param name="data">The dialog data to play</param>
    IEnumerator Routine_ClientWines(CharacterData.DialogData[] data)
    {
        foreach (CharacterData.DialogData part in data)
        {
            PlayDialog(part);
            yield return new WaitForSeconds(part.clip.length); // Change for clip length later
        }


        PlayDialog(m_currentCharacter.potionDialog, true);
        yield return new WaitForSeconds(m_currentCharacter.potionDialog.clip.length); // Change for clip length later

        m_cutscene = null;
    }

    /// <summary>
    /// Checks the current combination
    /// </summary>
    private void CheckPotion()
    {
        for (int i = 0; i < m_currentIngredients.Length; i++)
        {
            if (m_currentIngredients[i] == IngredientType.NOTHING) return;
        }

        PotionRecipe recipeCreated = null;
        foreach (PotionRecipe recipe in m_allRecipes)
        {
            if (recipe.CanMakeWithIngredients(m_currentIngredients))
            {
                recipeCreated = recipe;
                break;
            }
        }

        if (m_cutscene != null)
        {
            StopAllCoroutines();
        }

        if (m_currentCharacter.retryIfNotGood && (!recipeCreated || recipeCreated.ID != m_currentRecipe.ID))
        {
            if (!recipeCreated)
            {
                // Player did not create a potion
                m_cutscene = StartCoroutine(Routine_ClientWines(m_currentCharacter.notAPotionDialog));
            }
            else
            {
                // Player created the wrong potion
                m_cutscene = StartCoroutine(Routine_ClientWines(m_currentCharacter.wrongPotionDialog));
            }
        }
        else
        {
            m_canCheckPotions = false;
            m_awaitingCleaning = false;

            if (!recipeCreated)
            {
                // Player did not create a potion
                potionBad++;
                m_cutscene = StartCoroutine(Routine_ClientLeaves(m_currentCharacter.notAPotionDialog));
            }
            else if (recipeCreated.ID != m_currentRecipe.ID)
            {
                // Player created the wrong potion
                potionBad++;
                m_cutscene = StartCoroutine(Routine_ClientLeaves(m_currentCharacter.wrongPotionDialog));
            }
            else
            {
                // Player created the correct potion
                potionGood++;
                m_cutscene = StartCoroutine(Routine_ClientLeaves(m_currentCharacter.goodPotionDialog));
            }
        }
    }

    /// <summary>
    /// Sets the current potion for a slot
    /// </summary>
    /// <param name="idx">The slot's idx</param>
    /// <param name="ingredient">The ingredient to add</param>
    void SetCurrentIngredient(int idx, IngredientType ingredient)
    {
        if (idx < 0 || idx > 2) { return; }
        if ((m_currentIngredients[idx] == IngredientType.NOTHING && ingredient != IngredientType.NOTHING) ||
            (m_currentIngredients[idx] != IngredientType.NOTHING && ingredient == IngredientType.NOTHING))
        {
            m_currentIngredients[idx] = ingredient;
            if (m_canCheckPotions && ingredient != IngredientType.NOTHING) { CheckPotion(); }
            if (m_awaitingCleaning && CurrentIngredientsAreAllEmpty())
            {
                m_awaitingCleaning = false;
                BringNextClient();
            }
        }
    }

    /// <summary>
    /// Toggle an ingrendient in the current selection
    /// </summary>
    /// <param name="ingredient">The ingredient</param>
    /// <param name="add">True if the ingredient should be added or removed</param>
    void ToggleIngredient(IngredientType ingredient, bool add)
    {
        int idx;
        if (add)
        {
            idx = FindFirstOccuranceOf(IngredientType.NOTHING);
            if (idx != -1) SetCurrentIngredient(idx, ingredient);
        }
        else
        {
            idx = FindFirstOccuranceOf(ingredient);
            if (idx != -1) SetCurrentIngredient(idx, IngredientType.NOTHING);
        }
    }

    /// <summary>
    /// Finds the first occurance of an ingredient type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    int FindFirstOccuranceOf(IngredientType type)
    {
        for (int i = 0; i < m_currentIngredients.Length; i++)
        {
            if (m_currentIngredients[i] == type) return i;
        }
        return -1;
    }

    private void Update()
    {
#if UNITY_EDITOR && false
        if (Input.GetKeyDown(KeyCode.KeypadMinus)) StartGame();

        if (Input.GetKeyDown(KeyCode.Keypad1)) ToggleIngredient(IngredientType.HONEY, true);
        else if (Input.GetKeyUp(KeyCode.Keypad1)) ToggleIngredient(IngredientType.HONEY, false);

        if (Input.GetKeyDown(KeyCode.Keypad2)) ToggleIngredient(IngredientType.CHOCOLATE, true);
        else if (Input.GetKeyUp(KeyCode.Keypad2)) ToggleIngredient(IngredientType.CHOCOLATE, false);

        if (Input.GetKeyDown(KeyCode.Keypad3)) ToggleIngredient(IngredientType.VINEGAR, true);
        else if (Input.GetKeyUp(KeyCode.Keypad3)) ToggleIngredient(IngredientType.VINEGAR, false);

        if (Input.GetKeyDown(KeyCode.Keypad4)) ToggleIngredient(IngredientType.COFFEE, true);
        else if (Input.GetKeyUp(KeyCode.Keypad4)) ToggleIngredient(IngredientType.COFFEE, false);

        if (Input.GetKeyDown(KeyCode.Keypad5)) ToggleIngredient(IngredientType.PEPPER, true);
        else if (Input.GetKeyUp(KeyCode.Keypad5)) ToggleIngredient(IngredientType.PEPPER, false);

        if (Input.GetKeyDown(KeyCode.Keypad6)) ToggleIngredient(IngredientType.GARLIC, true);
        else if (Input.GetKeyUp(KeyCode.Keypad6)) ToggleIngredient(IngredientType.GARLIC, false);

        if (Input.GetKeyDown(KeyCode.Keypad7)) ToggleIngredient(IngredientType.CURRY, true);
        else if (Input.GetKeyUp(KeyCode.Keypad7)) ToggleIngredient(IngredientType.CURRY, false);

        if (Input.GetKeyDown(KeyCode.Keypad8)) ToggleIngredient(IngredientType.CINNAMON, true);
        else if (Input.GetKeyUp(KeyCode.Keypad8)) ToggleIngredient(IngredientType.CINNAMON, false);

        if (Input.GetKeyDown(KeyCode.Keypad9)) ToggleIngredient(IngredientType.SOAP, true);
        else if (Input.GetKeyUp(KeyCode.Keypad9)) ToggleIngredient(IngredientType.SOAP, false);

        if (Input.GetKeyDown(KeyCode.KeypadMultiply)) ToggleIngredient(IngredientType.LAVENDER, true);
        else if (Input.GetKeyUp(KeyCode.KeypadMultiply)) ToggleIngredient(IngredientType.LAVENDER, false);

        if (Input.GetKeyDown(KeyCode.KeypadDivide)) ToggleIngredient(IngredientType.GRENADINE, true);
        else if (Input.GetKeyUp(KeyCode.KeypadDivide)) ToggleIngredient(IngredientType.GRENADINE, false);

        if (Input.GetKeyDown(KeyCode.KeypadPlus)) ToggleIngredient(IngredientType.MINT, true);
        else if (Input.GetKeyUp(KeyCode.KeypadPlus)) ToggleIngredient(IngredientType.MINT, false);
#endif

        m_currentIngredients = m_ingredientsManager.CurrentIngredients;
        if (m_canCheckPotions) { CheckPotion(); }
        if (m_awaitingCleaning && CurrentIngredientsAreAllEmpty())
        {
            m_awaitingCleaning = false;
            BringNextClient();
        }

        if (!m_running) { return; }

        m_gameTimer.Update(Time.deltaTime);
    }
}
