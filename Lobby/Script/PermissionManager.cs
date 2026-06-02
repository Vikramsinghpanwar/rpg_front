using UnityEngine;
using UnityEngine.Android;

public class PermissionManager : MonoBehaviour
{
    // Function to request permission
    public void RequestStoragePermission()
    {
        // Check if the permission is already granted
        if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Debug.Log("Permission already granted.");
            // Permission is already granted, you can proceed to open the gallery or access storage.
        }
        else
        {
            // Permission is not granted yet, request it
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }

        // Optional: You may also want to check and request for write permission.
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }
    }

    // This function will be called after the permission dialog is closed.
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                Debug.Log("Permission granted.");
                // Permission is granted, proceed with accessing the gallery or external storage
            }
            else
            {
                Debug.Log("Permission denied.");
                // Permission was denied, show an appropriate message to the user
            }
        }
    }
}
