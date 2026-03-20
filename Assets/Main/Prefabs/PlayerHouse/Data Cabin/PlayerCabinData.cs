using UnityEngine;


[CreateAssetMenu(menuName = "ObjectsData/CabinData")]

public class PlayerCabinData : ScriptableObject
{
    [SerializeField] private GameObject Cabin;

    public string nameFilePos = "CabinPos"; // Название файла сохранения позиции

    public string nameFileRot = "CabinRot"; // Название файла сохранения позиции

    void OnApplicationQuit()
    {
        SaveSystem.SaveData(Cabin.transform.position, nameFilePos);
        SaveSystem.SaveData(Cabin.transform.rotation, nameFileRot);
    }
}
