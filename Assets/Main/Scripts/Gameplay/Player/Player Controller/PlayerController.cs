using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerData data; // данные игрока
    
    private CharacterController controller;
    private Vector3 moveDirection = Vector3.zero;
    private float verticalSpeed = 0.0f; // Вертикальная скорость

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if(Time.timeScale != 0)
        {
            if(controller.enabled == false) controller.enabled = true;

            // Проверяем, на земле ли персонаж
            if (controller.isGrounded)
            {
                // Получаем ввод от игрока
                float horizontalInput = Input.GetAxis("Horizontal");
                float verticalInput = Input.GetAxis("Vertical");

                // Определяем скорость в зависимости от состояния бега
                float speed = Input.GetKey(KeyCode.LeftShift) ? data.runMoveSpeed : data.moveSpeed;

                // Вычисляем направление движения по горизонтали
                moveDirection = (transform.forward * verticalInput + transform.right * horizontalInput) * speed;

                // Если игрок нажал кнопку прыжка, устанавливаем вертикальную скорость для прыжка
                if (Input.GetButtonDown("Jump"))
                {
                    verticalSpeed = data.jumpForce;
                }
            }

            // Применяем гравитацию к вертикальной скорости
            verticalSpeed -= data.gravity * Time.deltaTime;

            // Добавляем вертикальную скорость к moveDirection
            moveDirection.y = verticalSpeed;

            // Двигаем персонажа
            controller.Move(moveDirection * Time.deltaTime);
        }          
        else controller.enabled = false;
    }

    void OnApplicationQuit()
    {        
        SaveSystem.SaveData(transform.position, data.nameFilePlayerPos);        
    }

    [ContextMenu("Reset Saved Player Position")]
    public void ResetSavedPosition()
    {
        SaveSystem.TryDeleteData(data.nameFilePlayerPos);        
    }
}

