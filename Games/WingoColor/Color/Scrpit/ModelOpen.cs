using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ModelOpen : MonoBehaviour{
  
    public void OpenModel(Image image)
    {
        if (image != null)
        {
            image.gameObject.SetActive(true); // Corrected the property access
        }
    }

    public void CloseModel(Image image)
    {
        if (image != null)
        {
            image.gameObject.SetActive(false); // Corrected the property access
        }
    }

    public void Lobby()
    {
        SceneManager.LoadScene(1);
    }

   

}
