using UnityEngine;

[CreateAssetMenu(menuName = "PlayerData/MainData")]
public class PlayerData : ScriptableObject
{
    public string nameFilePlayerPos = "PlayerPos"; // Название файла сохранения позиции

    public float moveSpeed = 5.0f; // Скорость движения персонажа
    public float runMoveSpeed = 30.0f; // Скорость бега
    public float jumpForce = 8.0f; // Сила прыжка
    public float gravity = 20.0f; // Сила гравитации
}
