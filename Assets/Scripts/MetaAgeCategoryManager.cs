using System;
using UnityEngine;
using Oculus.Platform;
using Oculus.Platform.Models;

[DefaultExecutionOrder(-10000)]
public class MetaAgeCategoryManager : MonoBehaviour
{
    public static bool IsAgeCategoryResolved { get; private set; }
    public static bool IsChildUser { get; private set; }

    private static bool _initialized;

    private void Awake()
    {
        if (_initialized)
        {
            Destroy(gameObject);
            return;
        }

        _initialized = true;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[MetaAgeCategoryManager] Initialized. Starting Meta age category request...");
        InitializeMetaAgeCheck();
    }

    private void Update()
    {
        // Make sure Oculus Platform async callbacks are pumped on device.
        Request.RunCallbacks();
    }

    private void InitializeMetaAgeCheck()
    {
        try
        {
            Debug.Log("[MetaAgeCategoryManager] PlatformSettings.AppID: " + PlatformSettings.AppID);
            Debug.Log("[MetaAgeCategoryManager] PlatformSettings.MobileAppID: " + PlatformSettings.MobileAppID);
            Debug.Log("[MetaAgeCategoryManager] PlatformSettings.UseMobileAppIDInEditor: " + PlatformSettings.UseMobileAppIDInEditor);

            string appIdForInit = PlatformSettings.AppID;
            if (string.IsNullOrEmpty(appIdForInit))
                appIdForInit = PlatformSettings.MobileAppID;

            Core.AsyncInitialize(appIdForInit).OnComplete(OnPlatformInitialized);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[MetaAgeCategoryManager] Core.AsyncInitialize failed: " + ex.Message);
            IsAgeCategoryResolved = true;
        }
    }

    private void OnPlatformInitialized(Message<PlatformInitialize> message)
    {
        if (message == null || message.IsError)
        {
            string err = message != null && message.GetError() != null ? message.GetError().Message : "unknown error";
            Debug.LogWarning("[MetaAgeCategoryManager] Meta Platform initialization unsuccessful: " + err);
            IsAgeCategoryResolved = true;
            return;
        }

        Users.GetUserProof().OnComplete(OnUserProofReceived);
    }

    private void OnUserProofReceived(Message<UserProof> message)
    {
        if (message == null || message.IsError)
        {
            string err = message != null && message.GetError() != null ? message.GetError().Message : "unknown error";
            Debug.LogWarning("[MetaAgeCategoryManager] Failed to get user proof: " + err);
            IsAgeCategoryResolved = true;
            return;
        }

        UserAgeCategory.Get().OnComplete(OnUserAgeCategoryReceived);
    }

    private void OnUserAgeCategoryReceived(Message<UserAccountAgeCategory> messageAge)
    {
        if (messageAge == null || messageAge.IsError)
        {
            string err = messageAge != null && messageAge.GetError() != null ? messageAge.GetError().Message : "unknown error";
            Debug.LogWarning("[MetaAgeCategoryManager] Failed to retrieve user age category: " + err);
            IsAgeCategoryResolved = true;
            return;
        }

        string ageCategoryText = messageAge.Data != null ? messageAge.Data.AgeCategory.ToString() : "UNKNOWN";
        Debug.Log("<color=green>[MetaAgeCategoryManager] User age category: " + ageCategoryText + "</color>");

        IsChildUser = string.Equals(ageCategoryText, "Ch", StringComparison.OrdinalIgnoreCase);
        IsAgeCategoryResolved = true;
    }
}
