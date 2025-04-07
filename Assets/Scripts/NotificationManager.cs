using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class NotificationManager : MonoBehaviour
{
    // Singleton instance
    public static NotificationManager Instance { get; private set; }
    
    [Header("UI References")]
    [Tooltip("Text component to display notifications")]
    public TextMeshProUGUI notificationText;
    
    [Tooltip("Panel that contains the notification text")]
    public GameObject notificationPanel;
    
    [Header("Settings")]
    [Tooltip("Default duration for notifications (in seconds)")]
    public float defaultDuration = 3f;
    
    [Tooltip("Fade out duration (in seconds)")]
    public float fadeOutDuration = 0.5f;
    
    // Current coroutine for displaying notifications
    private Coroutine currentNotificationCoroutine;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // Hide notification panel by default
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }
    
    // Show a notification with the default duration
    public void ShowNotification(string message)
    {
        ShowNotification(message, defaultDuration);
    }
    
    // Show a notification with a custom duration
    public void ShowNotification(string message, float duration)
    {
        // Stop any existing notification
        if (currentNotificationCoroutine != null)
        {
            StopCoroutine(currentNotificationCoroutine);
        }
        
        // Start a new notification
        currentNotificationCoroutine = StartCoroutine(ShowNotificationCoroutine(message, duration));
    }
    
    // Coroutine to display the notification
    private IEnumerator ShowNotificationCoroutine(string message, float duration)
    {
        // Set the message
        if (notificationText != null)
        {
            notificationText.text = message;
        }
        
        // Show the panel
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(true);
            
            // Make sure the panel is fully visible
            CanvasGroup canvasGroup = notificationPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }
        
        // Wait for the duration
        yield return new WaitForSeconds(duration);
        
        // Fade out
        if (notificationPanel != null)
        {
            CanvasGroup canvasGroup = notificationPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                // Fade out over time
                float startTime = Time.time;
                while (Time.time < startTime + fadeOutDuration)
                {
                    float alpha = Mathf.Lerp(1f, 0f, (Time.time - startTime) / fadeOutDuration);
                    canvasGroup.alpha = alpha;
                    yield return null;
                }
                
                // Hide the panel
                notificationPanel.SetActive(false);
            }
            else
            {
                // If no CanvasGroup, just hide the panel
                notificationPanel.SetActive(false);
            }
        }
        
        // Clear the coroutine reference
        currentNotificationCoroutine = null;
    }
} 