using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Cutscenecontroller : MonoBehaviour
{
    [Header("UI")]
    public Image cutsceneImage;
    public TMP_Text cutsceneText;

    [Header("Content")]
    public Sprite[] images;
    [TextArea(2, 4)]
    public string[] texts;

    [Header("Flow")]
    public string nextSceneName;

    private int index = 0;

    void Start()
    {
        ShowCurrent();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            Next();
        }
    }

    void ShowCurrent()
    {
        cutsceneImage.sprite = images[index];
        cutsceneText.text = texts[index];
    }

    void Next()
    {
        index++;

        if (index >= images.Length)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            return;
        }

        ShowCurrent();
    }
}
