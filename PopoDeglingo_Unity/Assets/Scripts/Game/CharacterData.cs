using UnityEngine;

/// <summary>
/// Represents a character
/// </summary>
[CreateAssetMenu(fileName = "CharacterData", menuName = "Potion/CharacterData")]
public class CharacterData : ScriptableObject
{
	public int ID;
	public string CharacterName;
	public Sprite CharacterSprite;
	public bool retryIfNotGood;
	public bool resetTimerOnLeave;
	public DialogData[] introDialogs;
	public DialogData potionDialog;
	public DialogData[] notAPotionDialog;
	public DialogData[] wrongPotionDialog;
	public DialogData[] goodPotionDialog;
	public PotionRecipe linkedRecipe;

	/// <summary>
	/// Represents a dialog's data
	/// </summary>
	[System.Serializable]
	public class DialogData
	{
		public string subtitles;
		public AudioClip clip;
	}
}
