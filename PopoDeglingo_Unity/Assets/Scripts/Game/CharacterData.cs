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
	public DialogData[] introDialogs;
	public DialogData[] notAPotionDialogs;
	public DialogData[] wrongPotionDialogs;
	public DialogData[] goodPotionDialogs;

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
