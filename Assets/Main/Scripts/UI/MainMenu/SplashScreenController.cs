using UnityEngine;

public class SplashScreenController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private MainMenuController MenuController;

    private void Start()
    {
        StartAnimation();
    }


    private void DisableSplashScreen()
    {
        gameObject.SetActive(false);
    }


    public void StartAnimation()
    {
        gameObject.SetActive(true);
        animator.SetTrigger("Start");
    }

    public void EndAnimation()
    {
        gameObject.SetActive(true);
        animator.SetTrigger("End");
    }

    public void LoadGameScene()
    {
        MenuController.LoadGameScene();
    }
}
