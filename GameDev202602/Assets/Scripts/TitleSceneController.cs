using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneController : MonoBehaviour
{
    [SerializeField]
    private string nextSceneName;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(nextSceneName);//æ“¾‚µ‚½ƒV[ƒ“‚É‘JˆÚ
        }
    }
}
