using UnityEngine;
using UnityEngine.UI;

public class ProfileImgPopulator : MonoBehaviour
{
    public static ProfileImgPopulator Instance;
    public GameObject avatarPrefab;
    public Sprite[] Profiles;
    public Transform container;
    public int selectedIndex;
    // Start is called before the first frame update
    void Start()
    {
        Populate();
        Debug.Log("Profile image index: " + UserDetail.profileImageIndex);
        UpdateProfile(UserDetail.profileImageIndex);
    }

    void Awake()
    {
        if (Instance != this)
        {
            Instance = this;
        }
    }

    public void Populate()
    {
        
        Object[] loadedSprites = Resources.LoadAll("Avatar", typeof(Sprite));
        int i =0;
        Profiles = new Sprite[loadedSprites.Length];
        foreach (Object obj in loadedSprites)
        {
            Profiles[i] = obj as Sprite;
            GameObject a = Instantiate(avatarPrefab, container);
            a.transform.GetChild(0).GetComponent<Image>().sprite = obj as Sprite;
            int idx = i;
            a.GetComponent<Button>().onClick.AddListener(() => SelectAvatar(idx));
            i++;
        }

    }

    public void SelectAvatar(int val)
    {
        selectedIndex = val;
        PlayerPrefs.DeleteKey("myProfile");
        UserData.Instance.ProfileImage(Profiles[val]);
    }

    public void UpdateProfile(int index)
    {
        Debug.Log("Updating image at index: " + index);
        UserData.Instance.ProfileImage(Profiles[index]);
    }
}
