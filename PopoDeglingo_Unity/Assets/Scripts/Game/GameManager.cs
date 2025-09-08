using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Handles the game logic
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField] private float m_timer = 300;
    [SerializeField] private SpriteRenderer m_characterSprite;
    [SerializeField] private Animator m_characterAnimator;
    [SerializeField] private TextMeshProUGUI m_dialogText;

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
    private CharacterData[] m_allCharacters;


    private void Start()
    {
        m_running = false;
        m_awaitingCleaning = false;
        m_canCheckPotions = false;
        m_currentIngredients = new IngredientType[3];
        m_allRecipes = Resources.LoadAll<PotionRecipe>("Recipes/");
        m_allCharacters = Resources.LoadAll<CharacterData>("Characters/");
        m_gameTimer = new GameTimer(0, null);
    }

    /// <summary>
    /// Starts the game
    /// </summary>
    public void StartGame()
    {
        m_gameTimer.Reset(m_timer, EndGame);
        m_running = true;
        m_canCheckPotions = false;
        m_awaitingCleaning = false;
        BringNextClient();
    }

    /// <summary>
    /// Ends the game
    /// </summary>
    public void EndGame()
    {
        m_running = false;
        m_canCheckPotions = false;
    }

    public bool CurrentIngredientsAreAllEmpty()
    {
        return m_currentIngredients[0] == IngredientType.NOTHING &&
                m_currentIngredients[1] == IngredientType.NOTHING && m_currentIngredients[2] == IngredientType.NOTHING;
    }

    public void BringNextClient()
    {
        if (m_cutscene != null)
        {
            StopCoroutine(m_cutscene);
        }

        m_canCheckPotions = false;
        m_currentCharacter = m_allCharacters[Random.Range(0, m_allCharacters.Length)];
        m_currentRecipe = m_allRecipes[Random.Range(0, m_allRecipes.Length)];
        m_cutscene = StartCoroutine(Routine_BringInClient());
    }

    /// <summary>
    /// Plays a dialog and caches it
    /// </summary>
    /// <param name="data">The dialog's data</param>
    public void PlayDialog(CharacterData.DialogData data)
    {
        m_cachedDialog = data;
        if (data == null)
        {
            m_dialogText.text = "";
        }
        else
        {
            m_dialogText.text = data.subtitles;
        }
    }

    /// <summary>
    /// Routine for the client arriving
    /// </summary>
    IEnumerator Routine_BringInClient()
    {
        m_canCheckPotions = false;
        m_characterSprite.sprite = m_currentCharacter.CharacterSprite;
        m_characterAnimator.SetTrigger("In");
        PlayDialog(m_currentCharacter.introDialogs[Random.Range(0, m_currentCharacter.introDialogs.Length)]);
        yield return new WaitForSeconds(2); // Change for clip length later


        CharacterData.DialogData data = new CharacterData.DialogData();
        data.subtitles = "Je veux: " + m_currentRecipe.potionName +
        " (" + m_currentRecipe.ingredients[0] + ", " + m_currentRecipe.ingredients[1] + ", " + m_currentRecipe.ingredients[2] + ")";
        PlayDialog(data);
        yield return new WaitForSeconds(2); // Change for clip length later

        m_canCheckPotions = true;

        m_cutscene = null;
    }

    /// <summary>
    /// Routine for the client leaving
    /// </summary>
    /// <param name="data">The dialog data to play</param>
    IEnumerator Routine_ClientLeaves(CharacterData.DialogData data)
    {
        m_characterAnimator.SetTrigger("Out");
        PlayDialog(data);
        yield return new WaitForSeconds(2); // Change for clip length later

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

        m_canCheckPotions = false;
        m_awaitingCleaning = false;

        if (m_cutscene != null)
        {
            StopCoroutine(m_cutscene);
        }

        if (!recipeCreated)
        {
            // Player did not create a potion
            m_cutscene = StartCoroutine(Routine_ClientLeaves(m_currentCharacter.notAPotionDialogs[Random.Range(0, m_currentCharacter.notAPotionDialogs.Length)]));
        }
        else if (recipeCreated.ID != m_currentRecipe.ID)
        {
            // Player created the wrong potion
            m_cutscene = StartCoroutine(Routine_ClientLeaves(m_currentCharacter.wrongPotionDialogs[Random.Range(0, m_currentCharacter.wrongPotionDialogs.Length)]));
        }
        else
        {
            // Player created the correct potion
            m_cutscene = StartCoroutine(Routine_ClientLeaves(m_currentCharacter.goodPotionDialogs[Random.Range(0, m_currentCharacter.goodPotionDialogs.Length)]));
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

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.KeypadMinus)) StartGame();
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            SetCurrentIngredient(0, IngredientType.NOTHING);
            SetCurrentIngredient(1, IngredientType.NOTHING);
            SetCurrentIngredient(2, IngredientType.NOTHING);
        }
        if (Input.GetKeyDown(KeyCode.Keypad1)) SetCurrentIngredient(0, IngredientType.TEST1);
        if (Input.GetKeyDown(KeyCode.Keypad2)) SetCurrentIngredient(1, IngredientType.TEST1);
        if (Input.GetKeyDown(KeyCode.Keypad3)) SetCurrentIngredient(2, IngredientType.TEST1);

        if (Input.GetKeyDown(KeyCode.Keypad4)) SetCurrentIngredient(0, IngredientType.TEST2);
        if (Input.GetKeyDown(KeyCode.Keypad5)) SetCurrentIngredient(1, IngredientType.TEST2);
        if (Input.GetKeyDown(KeyCode.Keypad6)) SetCurrentIngredient(2, IngredientType.TEST2);

        if (Input.GetKeyDown(KeyCode.Keypad7)) SetCurrentIngredient(0, IngredientType.TEST3);
        if (Input.GetKeyDown(KeyCode.Keypad8)) SetCurrentIngredient(1, IngredientType.TEST3);
        if (Input.GetKeyDown(KeyCode.Keypad9)) SetCurrentIngredient(2, IngredientType.TEST3);

        if (Input.GetKeyDown(KeyCode.W)) SetCurrentIngredient(0, IngredientType.TEST4);
        if (Input.GetKeyDown(KeyCode.X)) SetCurrentIngredient(1, IngredientType.TEST4);
        if (Input.GetKeyDown(KeyCode.V)) SetCurrentIngredient(2, IngredientType.TEST4);

        if (Input.GetKeyDown(KeyCode.Q)) SetCurrentIngredient(0, IngredientType.TEST5);
        if (Input.GetKeyDown(KeyCode.S)) SetCurrentIngredient(1, IngredientType.TEST5);
        if (Input.GetKeyDown(KeyCode.D)) SetCurrentIngredient(2, IngredientType.TEST5);

        if (Input.GetKeyDown(KeyCode.A)) SetCurrentIngredient(0, IngredientType.TEST6);
        if (Input.GetKeyDown(KeyCode.Z)) SetCurrentIngredient(1, IngredientType.TEST6);
        if (Input.GetKeyDown(KeyCode.E)) SetCurrentIngredient(2, IngredientType.TEST6);
#endif

        if (!m_running) { return; }

    }
}
