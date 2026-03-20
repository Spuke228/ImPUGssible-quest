using UnityEngine;

public class Анимациямопса : MonoBehaviour
{
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        bool isMoving = Input.GetKey(KeyCode.W) ||
                        Input.GetKey(KeyCode.A) ||
                        Input.GetKey(KeyCode.S) ||
                        Input.GetKey(KeyCode.D);

        animator.SetBool("isRunning", isMoving);
    }

    // Запуск анимации сна
    public void PlaySleepAnimation()
    {
        animator.SetTrigger("SleepTrigger");
    }
}
