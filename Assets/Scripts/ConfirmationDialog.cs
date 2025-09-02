using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmationDialog : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject root;      // The panel container to show/hide (defaults to this.gameObject if null)
    [SerializeField] private TMP_Text messageText; // The prompt text
    [SerializeField] private Button confirmButton; // “Yes” / “Confirm”
    [SerializeField] private Button cancelButton;  // “No” / “Cancel”

    private Action _onConfirm;
    private Action _onCancel;

    private void Awake()
    {
        if (root == null) root = gameObject;
        Hide();
    }

    public void Show(string message, Action onConfirm, Action onCancel = null)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;

        if (messageText) messageText.text = message;

        if (root != null) root.SetActive(true);

        // Clean old listeners to avoid stacking
        if (confirmButton)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() =>
            {
                Hide();
                _onConfirm?.Invoke();
            });
        }

        if (cancelButton)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(() =>
            {
                Hide();
                _onCancel?.Invoke();
            });
        }

        root.SetActive(true);
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
    }
}