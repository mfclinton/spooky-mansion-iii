using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Fade to black game objects")]
    [SerializeField] private Image _blackScreenImage;

    [Header("Fade to black settings")]
    [SerializeField] private float _fadeTime = 3f;

    private void Awake()
    {
        _blackScreenImage.gameObject.SetActive(true);

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        StartCoroutine(FadeFromBlack());
    }

    public void LoadLevel(int levelIndex, bool useFade = true)
    {
        if (useFade)
        {
            StartCoroutine(LoadLevelWithFade(levelIndex));
        }
        else
        {
            SceneManager.LoadScene(levelIndex);
        }
    }

    public void RestartLevel(bool useFade = true)
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        LoadLevel(currentSceneIndex, useFade);
    }

    private IEnumerator LoadLevelWithFade(int levelIndex)
    {
        yield return FadeToBlack();
        SceneManager.LoadScene(levelIndex);
        yield return FadeFromBlack();
    }

    private IEnumerator FadeToBlack()
    {
        _blackScreenImage.color = new Color(0f, 0f, 0f, 0f);

        float elapsedTime = 0f;

        while (elapsedTime < _fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / _fadeTime);
            _blackScreenImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
    }

    private IEnumerator FadeFromBlack()
    {
        _blackScreenImage.color = new Color(0f, 0f, 0f, 1f);

        float elapsedTime = 0f;

        while (elapsedTime < _fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsedTime / _fadeTime);
            _blackScreenImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

    }
}
