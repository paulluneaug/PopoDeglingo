using System;

using UnityEngine;
using UnityEngine.InputSystem;

using UnityUtility.CustomAttributes;
using UnityUtility.Extensions;

public class GrimoireManager : MonoBehaviour
{
    [Title("Inputs")]
    [SerializeField] private InputActionReference m_nextPageInput;
    [SerializeField] private InputActionReference m_previousPageInput;
    [SerializeField] private InputActionReference m_repeatPageInput;

    [Title("Audio sources")]
    [SerializeField] private AudioSource m_cantFlipPageAudioSource;

    [Title("Settings")]
    [Button(nameof(GetAllPages))]
    [SerializeField] private int m_startPage = 0;
    [SerializeField] private GrimoirePage[] m_pages;

    // Cache
    [NonSerialized] private int m_currentPage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        m_nextPageInput.action.performed += OnNextPagePerformed;
        m_previousPageInput.action.performed += OnPreviousPagePerformed;
        m_repeatPageInput.action.performed += OnReapeatPagePerformed;

        m_pages.ForEach(page => page.Hide());

        m_currentPage = m_startPage;
        m_pages[m_currentPage].Show();
    }

    private void OnDestroy()
    {
        m_nextPageInput.action.performed -= OnNextPagePerformed;
        m_previousPageInput.action.performed -= OnPreviousPagePerformed;
        m_repeatPageInput.action.performed -= OnReapeatPagePerformed;
    }

    private void ChangePage(int offset)
    {
        int newPage = m_currentPage + offset;

        if (!(0 <= newPage && newPage < m_pages.Length))
        {
            m_cantFlipPageAudioSource.Play();
            return;
        }

        m_pages[m_currentPage].Hide();
        m_currentPage = newPage;
        m_pages[m_currentPage].Show();

    }

    private void OnPreviousPagePerformed(InputAction.CallbackContext context)
    {
        ChangePage(-1);
    }

    private void OnNextPagePerformed(InputAction.CallbackContext context)
    {
        ChangePage(1);
    }

    private void OnReapeatPagePerformed(InputAction.CallbackContext context)
    {
        m_pages[m_currentPage].Repeat();
    }

    private void GetAllPages()
    {
        m_pages = FindObjectsByType<GrimoirePage>(FindObjectsSortMode.InstanceID);
    }
}
