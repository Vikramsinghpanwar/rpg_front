using UnityEngine;

public class ShareManager : MonoBehaviour
{
    public static ShareManager instance;
    void Awake()
    {
               instance=this;
    }
    public void ShareText(string message)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent"))
        using (AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent"))
        {
            intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
            intentObject.Call<AndroidJavaObject>("setType", "text/plain");
            intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), message);

            AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");

            AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, "Share via");
            currentActivity.Call("startActivity", chooser);
        }
#endif
    }
}
