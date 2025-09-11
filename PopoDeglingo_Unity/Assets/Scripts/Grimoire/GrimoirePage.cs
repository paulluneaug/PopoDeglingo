using UnityEngine;

using UnityUtility.CustomAttributes;

public class GrimoirePage : MonoBehaviour
{
    [Button(nameof(ToggleVisibility))]
    [SerializeField] private AudioSource m_audioSource;
    [SerializeField] private CanvasGroup m_canvasGroup;

    public void Show()
    {
        ShowCanvas();
        StartAudio();
    }

    public void Hide()
    {
        HideCanvas();
        StopAudio();
    }

    public void Repeat()
    {
        StopAudio();
        StartAudio();
    }

    private void ShowCanvas()
    {
        m_canvasGroup.alpha = 1.0f;
        m_canvasGroup.blocksRaycasts = true;
    }

    private void HideCanvas()
    {
        m_canvasGroup.alpha = 0.0f;
        m_canvasGroup.blocksRaycasts = false;
    }

    private void StartAudio()
    {
        if (m_audioSource.isPlaying)
        {
            return;
        }
        m_audioSource.Play();
    }

    private void StopAudio()
    {
        if (!m_audioSource.isPlaying)
        {
            return;
        }

        m_audioSource.Stop();
    }

    private void ToggleVisibility()
    {
        if (m_canvasGroup.alpha == 0.0f)
        {
            ShowCanvas();
            return;
        }
        HideCanvas();
    }
}
