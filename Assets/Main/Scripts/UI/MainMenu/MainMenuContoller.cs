using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public InputField seedInputField;
    [SerializeField] private SplashScreenController splash;

    public void OnStartButtonClicked()
    {
        splash.EndAnimation();        
    }

    public void LoadGameScene()
    {
        string seedText = seedInputField.text;

        // Преобразуем текст в int, если это необходимо
        int seed = 0;
        if (int.TryParse(seedText, out seed))
        {
            SaveSystem.SaveData(seed, "Seed");
        }


        // Загрузить следующую сцену
        SceneManager.LoadScene("GameScene"); // Укажите имя сцены, которую нужно загрузить
    }
}
