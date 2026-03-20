using UnityEngine;

public class LoadingScreenController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private void Start()
    {
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
    }
}
