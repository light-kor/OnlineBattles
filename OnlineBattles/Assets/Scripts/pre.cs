using UnityEngine;
using UnityEngine.SceneManagement;

public class pre : MonoBehaviour
{
    public void First()
    {
        SceneManager.LoadScene("mainMenu");
    }

    public void Second()
    {
        DataHolder.KeyID = "321";
        SceneManager.LoadScene("mainMenu");
    }
}
