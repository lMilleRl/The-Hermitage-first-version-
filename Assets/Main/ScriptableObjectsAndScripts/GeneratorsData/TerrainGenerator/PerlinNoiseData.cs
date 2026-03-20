using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using static PerlinNoiseData;
using Unity.VisualScripting;

[CreateAssetMenu(menuName = "Terrain Generator/Perlin Noise Data")]
public class PerlinNoiseData : ScriptableObject
{
    [HideInInspector] public int SizeMap;

    [Range(256f, float.MaxValue)][SerializeField] private float MaxHeight;
    public float maxHeight => MaxHeight;

    [SerializeField][HideInInspector] public float[] links;
    [Range(2, ushort.MaxValue)] public ushort SideSize;    

    [HideInInspector] public uint sqrSideSize;

    [HideInInspector] public byte[][] settingsOctaves;
    [HideInInspector] public ushort settingsSize;
    [Range(1, 254)][SerializeField] private byte SettingSideProportion;
    public byte settingSideProportion => SettingSideProportion;


    [SerializeField][HideInInspector] public float oneSetting;

    public ushort[] dividers;
    [SerializeField][HideInInspector] public float[] oneWithDivider;
    [SerializeField] private bool IsAutoSelectionDividers;
    public bool isAutoSelectionDividers => IsAutoSelectionDividers;
    [Range(1, 255)] public byte LengthRowOctaves;
        
    [SerializeField] private byte SizeChunk;
    public byte sizeChunk => SizeChunk;

    [SerializeField] private ushort CountChunks;
    public ushort countChunks => CountChunks;

    [HideInInspector] public float onePerlinChunk;

    
    [SerializeField][HideInInspector] public float center;
    [SerializeField][HideInInspector] public float[] centersWithDivider;

    [SerializeField][HideInInspector] public Dictionary<Vector2Int, List<float>> rivers;
    [SerializeField][HideInInspector] public float waterLevel;
    [Range(0f, float.MaxValue)] public float heightWaterLevel;    


    [Range(1f, float.MaxValue)] public float maxDepthBigRiver;
    
    [Range(1f, float.MaxValue)][SerializeField] private float MaxRadiusBigRiverValley;
    public float maxRadiusBigRiverValley => MaxRadiusBigRiverValley;


    [Range(0f, float.MaxValue)][SerializeField] private float MaxRadiusBigRiver;
    public float maxRadiusBigRiver => MaxRadiusBigRiver;
    [Range(0f, float.MaxValue)][SerializeField] private float MinRadiusBigRiver;
    public float minRadiusBigRiver => MinRadiusBigRiver;

    [Range(0f, float.MaxValue)][SerializeField] private float MaxDRadiusBigRiver;
    public float maxDRadiusBigRiver => MaxDRadiusBigRiver;
    [Range(0f, float.MaxValue)][SerializeField] private float MinDRadiusBigRiver;
    public float minDRadiusBigRiver => MinDRadiusBigRiver;

    [SerializeField] private int MinCountRotationBRBend;
    public int minCountRotationBRBend => MinCountRotationBRBend;
    [SerializeField] private int MaxCountRotationBRBend;
    public int maxCountRotationBRBend => MaxCountRotationBRBend;

    [SerializeField] private int CountBigRivers;
    public int countBigRivers => CountBigRivers;
    [SerializeField] private int CountBigRiverBend;
    public int countBigRiverBend => CountBigRiverBend;


    [Range(0f, float.MaxValue)][SerializeField] private float MaxRadiusMidRiver;
    public float maxRadiusMidRiver => MaxRadiusMidRiver;
    [Range(0f, float.MaxValue)][SerializeField] private float MinRadiusMidRiver;
    public float minRadiusMidRiver => MinRadiusMidRiver;

    [Range(0f, float.MaxValue)][SerializeField] private float MaxDRadiusMidRiver;
    public float maxDRadiusMidRiver => MaxDRadiusMidRiver;
    [Range(0f, float.MaxValue)][SerializeField] private float MinDRadiusMidRiver;
    public float minDRadiusMidRiver => MinDRadiusMidRiver;

    [SerializeField] private int MinCountRotationMRBend;
    public int minCountRotationMRBend => MinCountRotationMRBend;
    [SerializeField] private int MaxCountRotationMRBend;
    public int maxCountRotationMRBend => MaxCountRotationMRBend;

    [SerializeField] private int CountMidRivers;
    public int countMidRivers => CountMidRivers;
    [SerializeField] private int CountMidRiverBend;
    public int countMidRiverBend => CountMidRiverBend;


    [Range(0f, float.MaxValue)][SerializeField] private float MaxRadiusLittleRiver;
    public float maxRadiusLittleRiver => MaxRadiusLittleRiver;
    [Range(0f, float.MaxValue)][SerializeField] private float MinRadiusLittleRiver;
    public float minRadiusLittleRiver => MinRadiusLittleRiver;

    [Range(0f, float.MaxValue)][SerializeField] private float MaxDRadiusLittleRiver;
    public float maxDRadiusLittleRiver => MaxDRadiusLittleRiver;
    [Range(0f, float.MaxValue)][SerializeField] private float MinDRadiusLittleRiver;
    public float minDRadiusLittleRiver => MinDRadiusLittleRiver;

    [SerializeField] private int MinCountRotationLRBend;
    public int minCountRotationLRBend => MinCountRotationLRBend;
    [SerializeField] private int MaxCountRotationLRBend;
    public int maxCountRotationLRBend => MaxCountRotationLRBend;

    [SerializeField] private int CountLittleRivers;
    public int countLittleRivers => CountLittleRivers;
    [SerializeField] private int CountLittleRiverBend;
    public int countLittleRiverBend => CountLittleRiverBend;


    [SerializeField] private int CountElementsRiver = 10;
    public int countElementsRiver => CountElementsRiver;


    [HideInInspector] public int[] reliefTypes;    

    public enum ReliefType
    {
        Forest,
        See,
        Mountains,       
    }

    [HideInInspector] public NativeArray<float> NativeLinks;

    [HideInInspector] public NativeArray<byte> NativeSettingsOctaves;
    [HideInInspector] public NativeArray<int2> NativeSettingsOctaveSlices;

    [HideInInspector] public NativeArray<float> NativeRiversData;
    [HideInInspector] public NativeParallelHashMap<int2, int2> NativeRiversSlices;
    [HideInInspector] public NativeArray<int> NativeReliefTypes;

    public string nameFileLinks = "Links";
    public string nameFileSettings = "SettingsOctaves";
    public string nameFileReliefTypes = "ReliefTypes";
    public string nameFileRiversKeys = "RiversKeys";
    public string nameFileRiversValues = "RiversValues";

    [ContextMenu("Delete Base Constant Data")]
    public void DeleteBaseConstantData()
    {
        SaveSystem.TryDeleteData(nameFileLinks);
        SaveSystem.TryDeleteData(nameFileSettings);
        SaveSystem.TryDeleteData(nameFileReliefTypes);
        SaveSystem.TryDeleteData(nameFileRiversKeys);
        SaveSystem.TryDeleteData(nameFileRiversValues);
    }

    public void SaveBaseConstantData()
    {
        SaveSystem.SaveData(links, nameFileLinks);
        SaveSystem.SaveData(settingsOctaves, nameFileSettings);
        SaveSystem.SaveData(reliefTypes, nameFileReliefTypes);

        if (rivers != null && rivers.Keys != null && rivers.Values != null)
        {
            var keys = new Vector2Int[rivers.Keys.Count];
            rivers.Keys.CopyTo(keys, 0);
            var values = new List<float>[rivers.Values.Count];
            rivers.Values.CopyTo(values, 0);
            SaveSystem.SaveData(keys, nameFileRiversKeys);
            SaveSystem.SaveData(values, nameFileRiversValues);
        }
    }

    public void LoadBaseConstantData()
    {
        SaveSystem.SavedData<float[]> dataLinks = SaveSystem.LoadData<float[]>(nameFileLinks);
        if(dataLinks != null)
            links = dataLinks.data;

        SaveSystem.SavedData<byte[][]> dataSettings = SaveSystem.LoadData<byte[][]>(nameFileSettings);
        if (dataSettings != null)
            settingsOctaves = dataSettings.data;

        SaveSystem.SavedData<int[]> dataReliefTypes = SaveSystem.LoadData<int[]>(nameFileReliefTypes);
        if (dataReliefTypes != null)
            reliefTypes = dataReliefTypes.data;

        SaveSystem.SavedData<Vector2Int[]> dataRiversKeys = SaveSystem.LoadData<Vector2Int[]>(nameFileRiversKeys);
        if (dataRiversKeys != null)
        {
            rivers = new Dictionary<Vector2Int, List<float>>(dataRiversKeys.data.Length);
            SaveSystem.SavedData<List<float>[]> dataRiversValues = SaveSystem.LoadData<List<float>[]>(nameFileRiversValues);
            
            for (int i = 0; i <  dataRiversKeys.data.Length; i++)
            {
                rivers.Add(dataRiversKeys.data[i], dataRiversValues.data[i]);
            }
        }
    }    
}
