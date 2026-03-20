using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;


public class PerlinNoise : MonoBehaviour
{
    [SerializeField] private PerlinNoiseData data;

    private int SizeMap
    {
        get { return data.SizeMap; }
        set { data.SizeMap = value; }
    }

    private float maxHeight
    {
        get { return data.maxHeight; }        
    }

    private float[] heights
    {
        get { return data.links; }
        set { data.links = value; }
    }

    private ushort SideSize
    {
        get { return data.SideSize; }
        set { data.SideSize = value; }
    }

    private uint sqrSideSize
    {
        get { return data.sqrSideSize; }
        set { data.sqrSideSize = value; }
    }

    private byte[][] settingsOctaves
    {
        get { return data.settingsOctaves; }
        set { data.settingsOctaves = value; }
    }

    private ushort settingsSize
    {
        get { return data.settingsSize; }
        set { data.settingsSize = value; }
    }

    private byte settingSideProportion
    {
        get { return data.settingSideProportion; }        
    }

    private float oneSetting
    {
        get { return data.oneSetting; }
        set { data.oneSetting = value; }
    }

    private ushort[] dividers
    {
        get { return data.dividers; }
        set { data.dividers = value; }
    }

    private float[] oneWithDivider
    {
        get { return data.oneWithDivider; }
        set { data.oneWithDivider = value; }
    }

    private byte LengthRowOctaves
    {
        get { return data.LengthRowOctaves; }
        set { data.LengthRowOctaves = value; }
    }

    private byte sizeChunk
    {
        get { return data.sizeChunk; }        
    }

    private ushort countChunks
    {
        get { return data.countChunks; }        
    }

    private float onePerlinChunk
    {
        get { return data.onePerlinChunk; }
        set { data.onePerlinChunk = value; }
    }    

    private float center
    {
        get { return data.center; }
        set { data.center = value; }
    }

    private float[] centersWithDivider
    {
        get { return data.centersWithDivider; }
        set { data.centersWithDivider = value; }
    }

    private Dictionary<Vector2Int, List<float>> rivers
    {
        get { return data.rivers; }
        set { data.rivers = value; }
    }

    private float waterLevel
    {
        get { return data.waterLevel; }
        set { data.waterLevel = value; }
    }

    private float heightWaterLevel
    {
        get { return data.heightWaterLevel; }
        set { data.heightWaterLevel = value; }
    }


    private float maxDepthBigRiver
    {
        get { return data.maxDepthBigRiver; }        
        set { data.maxDepthBigRiver = value; }
    }

    private float maxRadiusBigRiverValley
    {
        get { return data.maxRadiusBigRiverValley; }        
    }

    private float maxRadiusBigRiver
    {
        get { return data.maxRadiusBigRiver; }        
    }

    private float minRadiusBigRiver
    {
        get { return data.minRadiusBigRiver; }        
    }

    private float maxDRadiusBigRiver
    {
        get { return data.maxDRadiusBigRiver; }        
    }

    private float minDRadiusBigRiver
    {
        get { return data.minDRadiusBigRiver; }        
    }

    private int minCountRotationBRBend
    {
        get { return data.minCountRotationBRBend; }
    }

    private int maxCountRotationBRBend
    {
        get { return data.maxCountRotationBRBend; }
    }

    private int countBigRivers
    {
        get { return data.countBigRivers; }
    }

    private int countBigRiverBend
    {
        get { return data.countBigRiverBend; }
    }    


    private float maxRadiusMidRiver
    {
        get { return data.maxRadiusMidRiver; }        
    }

    private float minRadiusMidRiver
    {
        get { return data.minRadiusMidRiver; }        
    }

    private float maxDRadiusMidRiver
    {
        get { return data.maxDRadiusMidRiver; }        
    }

    private float minDRadiusMidRiver
    {
        get { return data.minDRadiusMidRiver; }        
    }

    private int minCountRotationMRBend
    {
        get { return data.minCountRotationMRBend; }        
    }

    private int maxCountRotationMRBend
    {
        get { return data.maxCountRotationMRBend; }        
    }

    private int countMidRivers
    {
        get { return data.countMidRivers; }        
    }

    private int countMidRiverBend
    {
        get { return data.countMidRiverBend; }        
    }

    private float maxRadiusLittleRiver
    {
        get { return data.maxRadiusLittleRiver; }        
    }

    private float minRadiusLittleRiver
    {
        get { return data.minRadiusLittleRiver; }        
    }

    private float maxDRadiusLittleRiver
    {
        get { return data.maxDRadiusLittleRiver; }        
    }

    private float minDRadiusLittleRiver
    {
        get { return data.minDRadiusLittleRiver; }        
    }

    private int minCountRotationLRBend
    {
        get { return data.minCountRotationLRBend; }        
    }

    private int maxCountRotationLRBend
    {
        get { return data.maxCountRotationLRBend; }        
    }

    private int countLittleRivers
    {
        get { return data.countLittleRivers; }        
    }

    private int countLittleRiverBend
    {
        get { return data.countLittleRiverBend; }        
    }


    private int seed;

    public float startCoordHeight { private set; get; }
    public void SetStartCoordHeight()
    {
        startCoordHeight = GetPointHeight(0, 0, 0, 0);
        if (startCoordHeight < data.waterLevel + 2f / maxHeight)
            startCoordHeight = data.waterLevel + 2f / maxHeight;
    }

// инициализация вместе с валидацией данных, установка данных для октав шума Перлина
    public void SetStartingData(int seed)
    {
        this.seed = seed;

        if (SideSize > settingSideProportion + 1)
            SideSize = (ushort)(Mathf.FloorToInt((float)SideSize / settingSideProportion) * settingSideProportion + 1);
        else
            SideSize = (ushort)(settingSideProportion + 1);

        sqrSideSize = (uint)(SideSize * SideSize);
        settingsSize = (ushort)(Mathf.FloorToInt((float)(SideSize - 1) / settingSideProportion) + 1);

        center = (float)(SideSize - 1) / 2;        

        onePerlinChunk = 1f / (countChunks * (sizeChunk - 1));

        oneSetting = onePerlinChunk / settingSideProportion;

        SizeMap = countChunks * (sizeChunk - 1) * (SideSize - 1);

        if (data.isAutoSelectionDividers)
        {
            if (LengthRowOctaves == 0) LengthRowOctaves++;

            dividers = new ushort[LengthRowOctaves];
            dividers[0] = 1;
        
            byte normalizeLenghtRowOctaves = 1;
            if (LengthRowOctaves > 1)
            {
                if ((SideSize - 1) % 2 == 0) dividers[1] = 2;
                else for (byte j = 3; j <= 7; j += 2)
                    {
                        if ((SideSize - 1) % j == 0)
                        {
                            dividers[1] = j;
                            break;
                        }
                        else dividers[1] = 1;
                    }
                normalizeLenghtRowOctaves++;
            }

            if (LengthRowOctaves > 1 && dividers[1] != (SideSize - 1))
                for (byte i = 2; i < LengthRowOctaves; i++)
                {
                    if ((SideSize - 1) / dividers[i - 1] % 2 == 0) dividers[i] = (ushort)(2 * dividers[i - 1]);
                    else for (byte j = 3; j <= 7; j += 2)
                        {
                            if ((SideSize - 1) / dividers[i - 1] % j == 0)
                            {
                                dividers[i] = (ushort)(j * dividers[i - 1]);
                                break;
                            }
                            else dividers[i] = 1;
                        }
                    normalizeLenghtRowOctaves++;
                    if (dividers[i] == (SideSize - 1)) break;
                }
            LengthRowOctaves = normalizeLenghtRowOctaves;
            ushort[] normalizeDividers = new ushort[LengthRowOctaves];
            Array.Copy(dividers, normalizeDividers, LengthRowOctaves);
            dividers = new ushort[LengthRowOctaves];
            Array.Copy(normalizeDividers, dividers, LengthRowOctaves);
        }        

        oneWithDivider = new float[LengthRowOctaves];
        centersWithDivider = new float[LengthRowOctaves];
        for (byte i = 0; i < LengthRowOctaves; i++)
        {
            oneWithDivider[i] = onePerlinChunk / dividers[i];
            centersWithDivider[i] = (float)center / dividers[i];
        }

        if (heightWaterLevel > maxHeight)
            heightWaterLevel = maxHeight;
        else if (maxDepthBigRiver > maxHeight - 1)
            maxDepthBigRiver = maxHeight;
        else if (heightWaterLevel - maxDepthBigRiver < 0)
            heightWaterLevel = maxDepthBigRiver;

        waterLevel = heightWaterLevel / maxHeight;
        countElementsRiver = data.countElementsRiver;
    } 

    // инициализация массивов данных для рек на основе кривых Безье
    public (float[], byte[][], Dictionary<Vector2Int, List<float>>) InitBigBaseData(int seed)
    {        
        SetStartingData(seed);        
        var random = new Unity.Mathematics.Random((uint)seed);        
        return (CreateRandomLinks(random), CreateRandomSettings(random), CreateRandomRivers(random));     
    }

    // инициализация данных для вычислениях значениев высот в Unity job system
    public void InitNativeBigBaseData()
    {
        data.NativeLinks = new NativeArray<float>(data.links, Allocator.Persistent);
        data.NativeReliefTypes = new NativeArray<int>(data.reliefTypes, Allocator.Persistent);

        data.NativeSettingsOctaveSlices = new NativeArray<int2>(data.settingsOctaves.Length, Allocator.Persistent);
        List<byte> TempForNativeSO = new List<byte>(data.settingsOctaves.Length * data.LengthRowOctaves);
        int startIndex = 0;
        for (int i = 0; i < data.settingsOctaves.Length; i++)
        {
            TempForNativeSO.AddRange(data.settingsOctaves[i]);
            data.NativeSettingsOctaveSlices[i] = new int2(startIndex, data.settingsOctaves[i].Length);
            startIndex += data.settingsOctaves[i].Length;
        }
        data.NativeSettingsOctaves = new NativeArray<byte>(TempForNativeSO.ToArray(), Allocator.Persistent);

        data.NativeRiversSlices = new NativeParallelHashMap<int2, int2>(data.rivers.Count, Allocator.Persistent);
        List<float> TempForNativeRD = new List<float>(data.rivers.Count * data.countElementsRiver * 2);
        startIndex = 0;
        foreach (KeyValuePair<Vector2Int, List<float>> kv in data.rivers)
        {
            TempForNativeRD.AddRange(kv.Value);
            data.NativeRiversSlices.Add(new int2(kv.Key.x, kv.Key.y), new int2(startIndex, kv.Value.Count));
            startIndex += kv.Value.Count;
        }
        data.NativeRiversData = new NativeArray<float>(TempForNativeRD.ToArray(), Allocator.Persistent);
    }

    public void DisposeNativeBigBaseData()
    {
        if (data.NativeLinks.IsCreated) data.NativeLinks.Dispose();
        if (data.NativeReliefTypes.IsCreated) data.NativeReliefTypes.Dispose();

        if (data.NativeSettingsOctaves.IsCreated) data.NativeSettingsOctaves.Dispose();
        if (data.NativeSettingsOctaveSlices.IsCreated) data.NativeSettingsOctaveSlices.Dispose();

        if (data.NativeRiversData.IsCreated) data.NativeRiversData.Dispose();
        if (data.NativeRiversSlices.IsCreated) data.NativeRiversSlices.Dispose();
    }

    private float[] CreateRandomLinks(Unity.Mathematics.Random random)
    {     
        var heights = new float[sqrSideSize * 2];
        sbyte[] weights = new sbyte[] { -1, 1 };

        for (int i = 0; i < sqrSideSize; i++)
        {            
            float x = random.NextFloat();
            heights[i] = x * weights[random.NextUInt(0, 2)];
            heights[i + sqrSideSize] = Mathf.Sqrt(1 - x * x) * weights[random.NextUInt(0, 2)];                        
        }
        
        return heights;
    }

    private byte[][] CreateRandomSettings(Unity.Mathematics.Random random)
    {        
        var settingsOctaves = new byte[settingsSize * settingsSize][];
        data.reliefTypes = new int[settingsSize * settingsSize];

        uint length;
        PerlinNoiseData.ReliefType type;
        uint randomNumber;
        byte startOctave, endOctave, chanceOctave, startSpecialOctave, endSpecialOctave, chanceSpecialOctave, countSpecialOctave, maxCountSpecialOctave;
        byte[] octaves = new byte[LengthRowOctaves];


        for (int y = 0; y < settingsSize; y++)
        {
            for (int x = 0; x < settingsSize; x++)
            {
                data.reliefTypes[x + y * settingsSize] = (int)GetBiomeWithNoise(x, y, seed);
            }
        }        

        for (int y = 0; y < settingsSize; y++)
        {
            for (int x = 0; x < settingsSize; x++)
            {
                length = 0;                

                type = (PerlinNoiseData.ReliefType)data.reliefTypes[x + y * settingsSize];

                if(type == PerlinNoiseData.ReliefType.Forest)
                {
                    startOctave = 1; endOctave = 5; chanceOctave = 60; startSpecialOctave = 5; endSpecialOctave = 8; chanceSpecialOctave = 50; maxCountSpecialOctave = 3;
                    octaves[0] = 0;
                    length++;
                }
                else if(type == PerlinNoiseData.ReliefType.See)
                {
                    startOctave = 5; endOctave = 8; chanceOctave = 60; startSpecialOctave = 2; endSpecialOctave = 5; chanceSpecialOctave = 30; maxCountSpecialOctave = 3;
                    octaves[0] = 1;
                    length++;
                }                
                else
                {
                    startOctave = 1; endOctave = 5; chanceOctave = 70; startSpecialOctave = 5; endSpecialOctave = 8; chanceSpecialOctave = 60; maxCountSpecialOctave = 3;
                    octaves[0] = 0;
                    length++;
                }
                
                for (byte i = startOctave; i < endOctave; i++)
                {
                        randomNumber = random.NextUInt(1, 101);
                    if (randomNumber <= chanceOctave)
                    {
                        if(octaves[0] != i)
                        {
                            octaves[length] = i;
                            length++;
                        }
                    }
                }
                if(length == 1)
                {
                    octaves[length] = (byte)random.NextUInt(startOctave, endOctave);
                    length++;
                }

                countSpecialOctave = 0;
                for (byte i = startSpecialOctave; i < endSpecialOctave; i++)
                {
                    if (countSpecialOctave == maxCountSpecialOctave) break;
                    randomNumber = random.NextUInt(1, 101);
                    if (randomNumber <= chanceSpecialOctave)
                    {
                        if (octaves[0] != i)
                        {
                            octaves[length] = i;
                            length++;
                            countSpecialOctave++;
                        }
                    }                    
                }

                if (length > 0)
                {
                    settingsOctaves[x + y * settingsSize] = new byte[length];
                    Array.Copy(octaves, settingsOctaves[x + y * settingsSize], length);
                }                                             
            }
        }
        
        return settingsOctaves;
    }

    private PerlinNoiseData.ReliefType GetBiomeWithNoise(int x, int y, int seed)
    {
        var pos = new float2(x, y);
        var weight = PerlinLite(pos / 2f, seed);

        if (weight > 0.55f) return PerlinNoiseData.ReliefType.Mountains;
        else if (weight < 0.33f) return PerlinNoiseData.ReliefType.See;
        else return PerlinNoiseData.ReliefType.Forest;
    }


    private int countElementsRiver;   

    private Dictionary<Vector2Int, List<float>> CreateRandomRivers(Unity.Mathematics.Random random)
    {
        var rivers = new Dictionary<Vector2Int, List<float>>((SideSize - 1) * (SideSize - 1));        
        
        CreateRiversInfo(data.countElementsRiver, random, rivers, minRadiusBigRiver, maxRadiusBigRiver, minDRadiusBigRiver, maxDRadiusBigRiver, minCountRotationBRBend, maxCountRotationBRBend, countBigRivers, countBigRiverBend, 0);        
        
        CreateRiversInfo(data.countElementsRiver, random, rivers, minRadiusMidRiver, maxRadiusMidRiver, minDRadiusMidRiver, maxDRadiusMidRiver, minCountRotationMRBend, maxCountRotationMRBend, countBigRivers + countMidRivers, countMidRiverBend, countBigRivers);        
        
        CreateRiversInfo(data.countElementsRiver, random, rivers, minRadiusLittleRiver, maxRadiusLittleRiver, minDRadiusLittleRiver, maxDRadiusLittleRiver, minCountRotationLRBend, maxCountRotationLRBend, countBigRivers + countMidRivers + countLittleRivers, countLittleRiverBend, countBigRivers + countMidRivers);

        return rivers;
    }

    
    private void CreateRiversInfo(int countRiversElements, Unity.Mathematics.Random random, Dictionary<Vector2Int, List<float>> rivers, float minRiverRadius, float maxRiverRadius, float minDirectionRadius, float maxDirectionRadius, int minCountRotationBend, int maxCountRotationBend, int countRivers, int countRiverBend, int numberStart)
    {
        Queue<Vector2Int> chunks = new Queue<Vector2Int>(100);
        
        Vector2 p0, p1, p2;
        float radius, originalAngle, rotationAngle, angle, t, currentRiverRadius, bottomDepth, radiusRiverValley;
        int countRotationBend;
        sbyte[] signs = new sbyte[] { -1, 1 };
        bool isJoiningRiver = false;

        currentRiverRadius = maxRiverRadius;
        for (int i = numberStart; i < countRivers; i++)
        {              
            if (random.NextUInt(0, 2) == 0)
            {
                if (random.NextUInt(0, 2) == 0)
                {
                    p0.x = 0;
                    originalAngle = random.NextFloat(285, 435);
                }
                else
                {
                    p0.x = SizeMap - 1;
                    originalAngle = random.NextFloat(105, 255);
                }
                p0.y = random.NextFloat(0, SizeMap - 1);
            }
            else
            {
                if (random.NextUInt(0, 2) == 0)
                {
                    p0.y = 0;
                    originalAngle = random.NextFloat(15, 165);
                }
                else
                {
                    p0.y = SizeMap - 1;
                    originalAngle = random.NextFloat(195, 345);
                }
                p0.x = random.NextFloat(0, SizeMap - 1);
            }

            

            currentRiverRadius = random.NextFloat(minRiverRadius, currentRiverRadius);

            bottomDepth = waterLevel - Mathf.Sqrt(currentRiverRadius / maxRadiusBigRiver) * maxDepthBigRiver / maxHeight;

            radiusRiverValley = currentRiverRadius + Mathf.Sqrt(currentRiverRadius / maxRadiusBigRiver) * maxRadiusBigRiverValley;

            radius = random.NextFloat(minDirectionRadius, maxDirectionRadius);

            p2 = p0 + new Vector2(radius * Mathf.Cos(originalAngle * Mathf.Deg2Rad), radius * Mathf.Sin(originalAngle * Mathf.Deg2Rad));            

            countRotationBend = random.NextInt(minCountRotationBend, maxCountRotationBend + 1);

            t = random.NextFloat(0f, 0.2f);

            p1 = GetLinearInterpolation(t, p0, p2);

            sbyte sign = signs[random.NextUInt(0, 2)];

            t = random.NextFloat(0.4f, 1.2f);

            p1.x += -sign * (p2.y - p0.y) * t;

            p1.y += sign * (p2.x - p0.x) * t;

            var P0P2 = p2 - p0;
            var P0P1 = p1 - p0;
            if (P0P2.x * P0P1.y - P0P2.y * P0P1.x > 0)
                rotationAngle = random.NextFloat(originalAngle - 75, originalAngle - 5);
            else
                rotationAngle = random.NextFloat(originalAngle + 5, originalAngle + 75);

            Vector2Int centralChunkPos = new Vector2Int(Mathf.FloorToInt(GetQuadraticBezier(0.5f, p0, p1, p2).x / ((sizeChunk - 1) * countChunks))
                , Mathf.FloorToInt(GetQuadraticBezier(0.5f, p0, p1, p2).y / ((sizeChunk - 1) * countChunks)));

            chunks.Enqueue(centralChunkPos);

            centralChunkPos = new Vector2Int(Mathf.FloorToInt(p0.x / ((sizeChunk - 1) * countChunks))
            , Mathf.FloorToInt(p0.y / ((sizeChunk - 1) * countChunks)));

            chunks.Enqueue(centralChunkPos);

            ProcessRiverChunks(countRiversElements, rivers, chunks, p0, p1, ref p2, ref isJoiningRiver, radiusRiverValley, bottomDepth, currentRiverRadius, i);

            if(isJoiningRiver)
            {
                centralChunkPos = new Vector2Int(Mathf.FloorToInt(GetQuadraticBezier(0.5f, p0, p1, p2).x / ((sizeChunk - 1) * countChunks))
                , Mathf.FloorToInt(GetQuadraticBezier(0.5f, p0, p1, p2).y / ((sizeChunk - 1) * countChunks)));

                chunks.Enqueue(centralChunkPos);

                centralChunkPos = new Vector2Int(Mathf.FloorToInt(p0.x / ((sizeChunk - 1) * countChunks))
                , Mathf.FloorToInt(p0.y / ((sizeChunk - 1) * countChunks)));

                chunks.Enqueue(centralChunkPos);

                ProcessRiverChunks(countRiversElements, rivers, chunks, p0, p1, ref p2, ref isJoiningRiver, radiusRiverValley, bottomDepth, currentRiverRadius, i);
            }

            if (p2.x > SizeMap - 1 || p2.x < 0 || p2.y > SizeMap - 1 || p2.y < 0 || isJoiningRiver)
            {
                isJoiningRiver = false;
                continue;
            }


            int counter = 1;
            for (int j = 1; j < countRiverBend; j++)
            {                
                p1 = 2 * p2 - p1;

                p0 = p2;

                radius = random.NextFloat(minDirectionRadius, maxDirectionRadius);

                angle = GetInterpolationLinks(originalAngle, rotationAngle, (float)counter / countRotationBend) * Mathf.Deg2Rad;

                p2 = p0 + new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));                

                centralChunkPos = new Vector2Int(Mathf.FloorToInt(GetQuadraticBezier(0.5f, p0, p1, p2).x / ((sizeChunk - 1) * countChunks))
                    , Mathf.FloorToInt(GetQuadraticBezier(0.5f, p0, p1, p2).y / ((sizeChunk - 1) * countChunks)));

                chunks.Enqueue(centralChunkPos);

                centralChunkPos = new Vector2Int(Mathf.FloorToInt(p0.x / ((sizeChunk - 1) * countChunks))
                     , Mathf.FloorToInt(p0.y / ((sizeChunk - 1) * countChunks)));

                chunks.Enqueue(centralChunkPos);

                ProcessRiverChunks(countRiversElements, rivers, chunks, p0, p1, ref p2, ref isJoiningRiver, radiusRiverValley, bottomDepth, currentRiverRadius, i);

                if (isJoiningRiver)
                {
                    centralChunkPos = new Vector2Int(Mathf.FloorToInt(GetQuadraticBezier(0.5f, p0, p1, p2).x / ((sizeChunk - 1) * countChunks))
                    , Mathf.FloorToInt(GetQuadraticBezier(0.5f, p0, p1, p2).y / ((sizeChunk - 1) * countChunks)));

                    chunks.Enqueue(centralChunkPos);

                    centralChunkPos = new Vector2Int(Mathf.FloorToInt(p0.x / ((sizeChunk - 1) * countChunks))
                    , Mathf.FloorToInt(p0.y / ((sizeChunk - 1) * countChunks)));

                    chunks.Enqueue(centralChunkPos);

                    ProcessRiverChunks(countRiversElements, rivers, chunks, p0, p1, ref p2, ref isJoiningRiver, radiusRiverValley, bottomDepth, currentRiverRadius, i);
                }

                if (p2.x > SizeMap - 1 || p2.x < 0 || p2.y > SizeMap - 1 || p2.y < 0 || isJoiningRiver)
                {
                    isJoiningRiver = false;
                    break;
                }                

                if ((float)counter / countRotationBend >= 1)
                {
                    counter = -1;
                    countRotationBend = random.NextInt(minCountRotationBend, maxCountRotationBend + 1);

                    originalAngle = rotationAngle;
                    P0P2 = p2 - p0;
                    P0P1 = p1 - p0;
                                        
                    if (P0P2.x * P0P1.y - P0P2.y * P0P1.x > 0)
                        rotationAngle = random.NextFloat(originalAngle - 75, originalAngle - 5);
                    else
                        rotationAngle = random.NextFloat(originalAngle + 5, originalAngle + 75);
                }

                counter++;                  
            }           
        }     
    }

    private void ProcessRiverChunks(int countRiversElements, Dictionary<Vector2Int, List<float>> rivers, Queue<Vector2Int> chunks, Vector2 p0, Vector2 p1, ref Vector2 p2, ref bool isJoiningRiver, float radiusRiverValley, float bottomDepth, float currentRiverRadius, int i)
    {
        var PosAddedChunks = new Queue<Vector2Int>();
        Vector2Int CurrentPos;
        while (chunks.Count > 0)
        {
            CurrentPos = chunks.Dequeue();
            if (CurrentPos.x < SideSize - 1 && CurrentPos.y < SideSize - 1 && CurrentPos.x >= 0 && CurrentPos.y >= 0 && IsRiverChunk(CurrentPos, p0, p1, p2, radiusRiverValley))
            {
                if (!rivers.ContainsKey(CurrentPos))
                {
                    rivers.Add(CurrentPos, new List<float>(countRiversElements) { radiusRiverValley, bottomDepth, currentRiverRadius, i, p0.x, p0.y, p1.x, p1.y, p2.x, p2.y });
                    AddNeighbourToQueue(chunks, CurrentPos);
                    PosAddedChunks.Enqueue(CurrentPos);
                }
                else if ((p0.x != rivers[CurrentPos][rivers[CurrentPos].Count - 6] || p0.y != rivers[CurrentPos][rivers[CurrentPos].Count - 5]) && i != rivers[CurrentPos][rivers[CurrentPos].Count - 7] && isJoiningRiver == false && IsRiverChunk(CurrentPos, p0, p1, p2, currentRiverRadius))
                {
                    Vector2 otherP0 = new Vector2(rivers[CurrentPos][rivers[CurrentPos].Count - 6], rivers[CurrentPos][rivers[CurrentPos].Count - 5]);
                    Vector2 otherP1 = new Vector2(rivers[CurrentPos][rivers[CurrentPos].Count - 4], rivers[CurrentPos][rivers[CurrentPos].Count - 3]);
                    Vector2 otherP2 = new Vector2(rivers[CurrentPos][rivers[CurrentPos].Count - 2], rivers[CurrentPos][rivers[CurrentPos].Count - 1]);
                    float minDistance = GetMinDistanceBezier(otherP0, otherP1, otherP2, p0);
                    float MinT = GetMinDistanceBezierT(otherP0, otherP1, otherP2, p0);
                    Vector2 connectedP2 = GetQuadraticBezier(MinT, otherP0, otherP1, otherP2);

                    float distance;
                    float otherT;

                    for (int g = countRiversElements; g < rivers[CurrentPos].Count; g += countRiversElements)
                    {
                        otherP0 = new Vector2(rivers[CurrentPos][g + countRiversElements - 6], rivers[CurrentPos][g + countRiversElements - 5]);
                        otherP1 = new Vector2(rivers[CurrentPos][g + countRiversElements - 4], rivers[CurrentPos][g + countRiversElements - 3]);
                        otherP2 = new Vector2(rivers[CurrentPos][g + countRiversElements - 2], rivers[CurrentPos][g + countRiversElements - 1]);

                        distance = GetMinDistanceBezier(otherP0, otherP1, otherP2, p0);
                        otherT = GetMinDistanceBezierT(otherP0, otherP1, otherP2, p0);
                        if (minDistance > distance)
                        {
                            minDistance = distance;
                            MinT = otherT;
                            connectedP2 = GetQuadraticBezier(MinT, otherP0, otherP1, otherP2);
                        }
                    }

                    p2 = connectedP2;

                    for (int j = 0; j < PosAddedChunks.Count; j++)
                    {
                        var pos = PosAddedChunks.Dequeue();
                        rivers[pos].RemoveRange(rivers[pos].Count - countRiversElements, countRiversElements);
                        if (rivers[pos].Count == 0) rivers.Remove(pos);
                    }

                    isJoiningRiver = true;
                    chunks.Clear();
                    break;
                }
                else if (p0.x != rivers[CurrentPos][rivers[CurrentPos].Count - 6] || p0.y != rivers[CurrentPos][rivers[CurrentPos].Count - 5])
                {
                    AddRiversElements(rivers, CurrentPos, p0, p1, p2, radiusRiverValley, bottomDepth, currentRiverRadius, i);
                    AddNeighbourToQueue(chunks, CurrentPos);
                    PosAddedChunks.Enqueue(CurrentPos);
                }
            }            
        }
    }

    private void AddRiversElements(Dictionary<Vector2Int, List<float>> rivers, Vector2Int temp, Vector2 p0, Vector2 p1, Vector2 p2, float radiusRiverValley, float bottomDepth, float currentRiverRadius, int i)
    {
        rivers[temp].Add(radiusRiverValley);
        rivers[temp].Add(bottomDepth);
        rivers[temp].Add(currentRiverRadius);
        rivers[temp].Add(i);
        rivers[temp].Add(p0.x);
        rivers[temp].Add(p0.y);
        rivers[temp].Add(p1.x);
        rivers[temp].Add(p1.y);
        rivers[temp].Add(p2.x);
        rivers[temp].Add(p2.y);
    }


    private void AddNeighbourToQueue(Queue<Vector2Int> chunks, Vector2Int chunkPos)
    {
        chunks.Enqueue(chunkPos + new Vector2Int(0, 1));
        chunks.Enqueue(chunkPos + new Vector2Int(0, -1));
        chunks.Enqueue(chunkPos + new Vector2Int(1, 0));
        chunks.Enqueue(chunkPos + new Vector2Int(-1, 0));
        chunks.Enqueue(chunkPos + new Vector2Int(0, 1) + new Vector2Int(1, 0));
        chunks.Enqueue(chunkPos + new Vector2Int(0, -1) + new Vector2Int(1, 0));
        chunks.Enqueue(chunkPos + new Vector2Int(0, 1) + new Vector2Int(-1, 0));
        chunks.Enqueue(chunkPos + new Vector2Int(0, -1) + new Vector2Int(-1, 0));
    }


    public static (int PosC00X, int PosC10X, int PosC01X, int PosC11X) SetPosX(float x, float centerLinks, int divider)
    {
        int PosC00X; int PosC01X; int PosC10X; int PosC11X;
        PosC00X = Mathf.FloorToInt(centerLinks + x / divider) * divider;
        PosC01X = PosC00X;
        PosC10X = PosC00X + divider;
        PosC11X = PosC10X;
        return (PosC00X, PosC10X, PosC01X, PosC11X);
    }

    public static (int PosC00Y, int PosC10Y, int PosC01Y, int PosC11Y) SetPosY(float y, float centerLinks, int divider)
    {
        int PosC00Y; int PosC01Y; int PosC10Y; int PosC11Y;
        PosC00Y = Mathf.FloorToInt(centerLinks + y / divider) * divider;
        PosC10Y = PosC00Y;
        PosC01Y = PosC00Y + divider;
        PosC11Y = PosC01Y;
        return (PosC00Y, PosC10Y, PosC01Y, PosC11Y);
    }    

    private static int SetSettingPosX(int x, byte settingSideProportion)
    {
        return Mathf.FloorToInt((float)x / settingSideProportion);
    }

    private static int SetSettingPosY(int y, byte settingSideProportion)
    {
        return Mathf.FloorToInt((float)y / settingSideProportion);
    }

    public float GetPointHeight(int CoordX, int CoordY, float InChunkX, float InChunkY)
    {
        float MapX = (float)CoordX / countChunks;
        float MapY = (float)CoordY / countChunks;

        int PosC00X; int PosC00Y; int PosC10X; int PosC10Y; int PosC01X; int PosC01Y; int PosC11X; int PosC11Y;

        (PosC00X, PosC10X, PosC01X, PosC11X) = SetPosX(MapX, center, 1);
        (PosC00Y, PosC10Y, PosC01Y, PosC11Y) = SetPosY(MapY, center, 1);

        if (PosC00X < 0 || PosC00X >= SideSize - 1 || PosC00Y < 0 || PosC00Y >= SideSize - 1) return 0f;

        float height = 0f;

        var heightsC = new float[4];

        int settingPosC00X; int settingPosC00Y;
        settingPosC00X = SetSettingPosX(PosC00X, settingSideProportion);
        settingPosC00Y = SetSettingPosY(PosC00Y, settingSideProportion);

        byte sidePos = 0;
        byte indexDivider = 0;
        uint posSettings;
        float localX;
        float localY;
        for (byte settingY = 0; settingY < 2; settingY++)
        {
            for (byte settingX = 0; settingX < 2; settingX++)
            {
                sidePos = (byte)(settingX + settingY * 2);
                posSettings = (uint)(settingPosC00X + settingX + (settingPosC00Y + settingY) * settingsSize);

                indexDivider = settingsOctaves[posSettings][0];

                (PosC00X, PosC10X, PosC01X, PosC11X) = SetPosX(MapX, centersWithDivider[indexDivider], dividers[indexDivider]);
                (PosC00Y, PosC10Y, PosC01Y, PosC11Y) = SetPosY(MapY, centersWithDivider[indexDivider], dividers[indexDivider]);

                localX = (MapX - PosC00X) / dividers[indexDivider] + centersWithDivider[indexDivider] + oneWithDivider[indexDivider] * InChunkX;
                localY = (MapY - PosC00Y) / dividers[indexDivider] + centersWithDivider[indexDivider] + oneWithDivider[indexDivider] * InChunkY;

                heightsC[sidePos] = MainGetHeight(PosC00X, PosC00Y, PosC10X, PosC10Y, PosC01X, PosC01Y, PosC11X, PosC11Y, localX, localY);

                for (int i = 1; i < settingsOctaves[posSettings].Length; i++)
                {
                    indexDivider = settingsOctaves[posSettings][i];

                    (PosC00X, PosC10X, PosC01X, PosC11X) = SetPosX(MapX, centersWithDivider[indexDivider], dividers[indexDivider]);
                    (PosC00Y, PosC10Y, PosC01Y, PosC11Y) = SetPosY(MapY, centersWithDivider[indexDivider], dividers[indexDivider]);

                    localX = (MapX - PosC00X) / dividers[indexDivider] + centersWithDivider[indexDivider] + oneWithDivider[indexDivider] * InChunkX;
                    localY = (MapY - PosC00Y) / dividers[indexDivider] + centersWithDivider[indexDivider] + oneWithDivider[indexDivider] * InChunkY;

                    heightsC[sidePos] += MainGetHeight(PosC00X, PosC00Y, PosC10X, PosC10Y, PosC01X, PosC01Y, PosC11X, PosC11Y, localX, localY);
                    heightsC[sidePos] /= 2;
                }
            }
        }

        localX = (MapX + center) / settingSideProportion - settingPosC00X + oneSetting * InChunkX;
        localY = (MapY + center) / settingSideProportion - settingPosC00Y + oneSetting * InChunkY;

        float posX = CoordX * (sizeChunk - 1) + InChunkX; float posY = CoordY * (sizeChunk - 1) + InChunkY;
        heightsC[0] = GetRelief(data.reliefTypes[settingPosC00X + settingPosC00Y * settingsSize], waterLevel, heightsC[0], maxHeight, sizeChunk, posX, posY, seed, startCoordHeight);
        heightsC[1] = GetRelief(data.reliefTypes[settingPosC00X + 1 + settingPosC00Y * settingsSize], waterLevel, heightsC[1], maxHeight, sizeChunk, posX, posY, seed, startCoordHeight);
        heightsC[2] = GetRelief(data.reliefTypes[settingPosC00X + (settingPosC00Y + 1) * settingsSize], waterLevel, heightsC[2], maxHeight, sizeChunk, posX, posY, seed, startCoordHeight);
        heightsC[3] = GetRelief(data.reliefTypes[settingPosC00X + 1 + (settingPosC00Y + 1) * settingsSize], waterLevel, heightsC[3], maxHeight, sizeChunk, posX, posY, seed, startCoordHeight);

        height = GetInterpolationSettings(heightsC[0], heightsC[1], heightsC[2], heightsC[3], localX, localY);        

        (PosC00X, PosC10X, PosC01X, PosC11X) = SetPosX(MapX, center, 1);
        (PosC00Y, PosC10Y, PosC01Y, PosC11Y) = SetPosY(MapY, center, 1);
        if (rivers.ContainsKey(new Vector2Int(PosC00X, PosC00Y)))
        {
            Vector2Int RiverDataPos = new Vector2Int(PosC00X, PosC00Y);
            float currentHeightBottomRiver = 0;
            float riverHeight; float distance;

            CoordX *= (sizeChunk - 1);
            CoordX += (int)math.floor(center) * countChunks * (sizeChunk - 1);
            CoordY *= (sizeChunk - 1);
            CoordY += (int)math.floor(center) * countChunks * (sizeChunk - 1);

            Vector2 p = new Vector2(CoordX + InChunkX, CoordY + InChunkY);
            float t;

            for (int i = 0; i < rivers[RiverDataPos].Count; i+=countElementsRiver)
            {
                (riverHeight, distance) = GetRiverHeight(p, ref currentHeightBottomRiver, i, RiverDataPos);
                if (height > riverHeight)
                {
                    riverHeight = ModifyHeightRiver(p.x, p.y, riverHeight, distance, RiverDataPos, i);
                    if (height > riverHeight)
                    {
                        t = math.saturate(1 - (distance - rivers[RiverDataPos][i + countElementsRiver - 8]) / (rivers[RiverDataPos][i + countElementsRiver - 10] - rivers[RiverDataPos][i + countElementsRiver - 8]));
                        height = GetInterpolationLinks(height, riverHeight, fade(t));
                    }
                }
            }            
        }
        float sqrMagnitude = posX * posX + posY * posY;
        if ((posX == 0 && posY == 0) == false && sqrMagnitude <= 25600f)
        {
            float t = math.saturate(1 - math.sqrt(sqrMagnitude) / 160f);
            height = GetInterpolationLinks(height, startCoordHeight, fade(t));
        }

        return height;
    }

    public static float GetPointHeightNative(float WorldX, float WorldY, byte settingSideProportion, float oneSetting, byte CurrentSizeChunk, ushort countChunks, ushort SideSize, float waterLevel, int countElementsRiver, NativeArray<float> oneWithDivider
        , NativeArray<float> centersWithDivider, NativeArray<ushort> dividers, NativeArray<float> Links, NativeArray<byte> SettingsOctaves, NativeArray<int2> SettingsOctavesSlices, int SettingsSize, NativeArray<float> RiversData, NativeParallelHashMap<int2, int2> RiversSlices
        , NativeArray<int> ReliefTypes, int seed, float maxHeight, float startCoordHeight)
    {
        float ChunkX = math.floor(WorldX / (CurrentSizeChunk - 1));
        float ChunkY = math.floor(WorldY / (CurrentSizeChunk - 1));

        float CoordXInChunk = WorldX - ChunkX * (CurrentSizeChunk - 1);
        float CoordYInChunk = WorldY - ChunkY * (CurrentSizeChunk - 1);

        float PerlinMapX = ChunkX / countChunks;
        float PerlinMapY = ChunkY / countChunks;

        int PosC00X; int PosC00Y; int PosC10X; int PosC10Y; int PosC01X; int PosC01Y; int PosC11X; int PosC11Y;

        (PosC00X, PosC10X, PosC01X, PosC11X) = SetPosX(PerlinMapX, centersWithDivider[0], 1);
        (PosC00Y, PosC10Y, PosC01Y, PosC11Y) = SetPosY(PerlinMapY, centersWithDivider[0], 1);

        if (PosC00X < 0 || PosC00X >= SideSize - 1 || PosC00Y < 0 || PosC00Y >= SideSize - 1) return 0f;

        float height = 0f;

        var heightsC = new NativeArray<float>(4, Allocator.Temp);

        int settingPosC00X; int settingPosC00Y;
        settingPosC00X = SetSettingPosX(PosC00X, settingSideProportion);
        settingPosC00Y = SetSettingPosY(PosC00Y, settingSideProportion);

        byte sidePos = 0;
        byte indexDivider = 0;
        int posSettings;
        float localX;
        float localY;
        int SqrSideSize = SideSize * SideSize;
        for (byte settingY = 0; settingY < 2; settingY++)
        {
            for (byte settingX = 0; settingX < 2; settingX++)
            {
                sidePos = (byte)(settingX + settingY * 2);
                posSettings = settingPosC00X + settingX + (settingPosC00Y + settingY) * SettingsSize;

                indexDivider = SettingsOctaves[SettingsOctavesSlices[posSettings].x];

                (PosC00X, PosC10X, PosC01X, PosC11X) = SetPosX(PerlinMapX, centersWithDivider[indexDivider], dividers[indexDivider]);
                (PosC00Y, PosC10Y, PosC01Y, PosC11Y) = SetPosY(PerlinMapY, centersWithDivider[indexDivider], dividers[indexDivider]);

                localX = (PerlinMapX - PosC00X) / dividers[indexDivider] + centersWithDivider[indexDivider] + oneWithDivider[indexDivider] * CoordXInChunk;
                localY = (PerlinMapY - PosC00Y) / dividers[indexDivider] + centersWithDivider[indexDivider] + oneWithDivider[indexDivider] * CoordYInChunk;

                heightsC[sidePos] = MainGetHeight(localX, localY, Links, PosC00X, PosC00Y, PosC10X, PosC10Y, PosC01X, PosC01Y, PosC11X, PosC11Y, SideSize, SqrSideSize);

                for (int i = SettingsOctavesSlices[posSettings].x + 1; i < SettingsOctavesSlices[posSettings].x + SettingsOctavesSlices[posSettings].y; i++)
                {
                    indexDivider = SettingsOctaves[i];

                    (PosC00X, PosC10X, PosC01X, PosC11X) = SetPosX(PerlinMapX, centersWithDivider[indexDivider], dividers[indexDivider]);
                    (PosC00Y, PosC10Y, PosC01Y, PosC11Y) = SetPosY(PerlinMapY, centersWithDivider[indexDivider], dividers[indexDivider]);

                    localX = (PerlinMapX - PosC00X) / dividers[indexDivider] + centersWithDivider[indexDivider] + oneWithDivider[indexDivider] * CoordXInChunk;
                    localY = (PerlinMapY - PosC00Y) / dividers[indexDivider] + centersWithDivider[indexDivider] + oneWithDivider[indexDivider] * CoordYInChunk;

                    heightsC[sidePos] += MainGetHeight(localX, localY, Links, PosC00X, PosC00Y, PosC10X, PosC10Y, PosC01X, PosC01Y, PosC11X, PosC11Y, SideSize, SqrSideSize);
                    heightsC[sidePos] /= 2;
                }
            }
        }

        localX = (PerlinMapX + centersWithDivider[0]) / settingSideProportion - settingPosC00X + oneSetting * CoordXInChunk;
        localY = (PerlinMapY + centersWithDivider[0]) / settingSideProportion - settingPosC00Y + oneSetting * CoordYInChunk;
        
        heightsC[0] = GetRelief(ReliefTypes[settingPosC00X + settingPosC00Y * SettingsSize], waterLevel, heightsC[0], maxHeight, CurrentSizeChunk, WorldX, WorldY, seed, startCoordHeight);
        heightsC[1] = GetRelief(ReliefTypes[settingPosC00X + 1 + settingPosC00Y * SettingsSize], waterLevel, heightsC[1], maxHeight, CurrentSizeChunk, WorldX, WorldY, seed, startCoordHeight);
        heightsC[2] = GetRelief(ReliefTypes[settingPosC00X + (settingPosC00Y + 1) * SettingsSize], waterLevel, heightsC[2], maxHeight, CurrentSizeChunk, WorldX, WorldY, seed, startCoordHeight);
        heightsC[3] = GetRelief(ReliefTypes[settingPosC00X + 1 + (settingPosC00Y + 1) * SettingsSize], waterLevel, heightsC[3], maxHeight, CurrentSizeChunk, WorldX, WorldY, seed, startCoordHeight);

        height = GetInterpolationSettings(heightsC[0], heightsC[1], heightsC[2], heightsC[3], localX, localY);        

        (PosC00X, PosC10X, PosC01X, PosC11X) = SetPosX(PerlinMapX, centersWithDivider[0], 1);
        (PosC00Y, PosC10Y, PosC01Y, PosC11Y) = SetPosY(PerlinMapY, centersWithDivider[0], 1);
        if (RiversSlices.ContainsKey(new int2(PosC00X, PosC00Y)))
        {
            int2 RiverDataPos = new int2(PosC00X, PosC00Y);
            float currentHeightBottomRiver;
            float riverHeight; float distance;

            ChunkX *= (CurrentSizeChunk - 1);
            ChunkX += math.floor(centersWithDivider[0]) * countChunks * (CurrentSizeChunk - 1);
            ChunkY *= (CurrentSizeChunk - 1);
            ChunkY += math.floor(centersWithDivider[0]) * countChunks * (CurrentSizeChunk - 1);

            float2 p = new float2(ChunkX + CoordXInChunk, ChunkY + CoordYInChunk);
            float t;

            for (int i = RiversSlices[RiverDataPos].x; i < RiversSlices[RiverDataPos].x + RiversSlices[RiverDataPos].y; i += countElementsRiver)
            {
                (riverHeight, currentHeightBottomRiver, distance) = GetRiverHeightPoint(p, RiversData, waterLevel, countElementsRiver, i);
                if (height > riverHeight)
                {
                    riverHeight = ModifyHeightWithRiver(p.x, p.y, riverHeight, distance, RiversData, i, countElementsRiver, waterLevel, currentHeightBottomRiver, seed);
                    if (height > riverHeight)
                    {
                        t = math.saturate(1 - (distance - RiversData[i + countElementsRiver - 8]) / (RiversData[i + countElementsRiver - 10] - RiversData[i + countElementsRiver - 8]));
                        height = GetInterpolationLinks(height, riverHeight, fade(t));
                    }
                }
            }
        }
        float sqrMagnitude = WorldX * WorldX + WorldY * WorldY;
        if ((WorldX == 0 && WorldY == 0) == false && sqrMagnitude <= 25600f)
        {            
            float t = math.saturate(1 - math.sqrt(sqrMagnitude) / 160f);
            height = GetInterpolationLinks(height, startCoordHeight, fade(t));
        }

        return height * maxHeight;
    }

    public static void GetReliefNative(int reliefType, float waterLevel, NativeArray<float> initialArray, float maxHeight, byte sizeChunk, byte offset, float CoordX, float CoordY, int seed, float startCoordHeight)
    {        
        if (reliefType == 3)
        {
            float baseHeight = waterLevel + 50f / maxHeight;

            float4 temp = new float4();
            float4 t = new float4();
            if (initialArray.Length >= 4)
                for (int i = 0; i < initialArray.Length - (initialArray.Length % 4); i += 4)
                {
                    temp.x = initialArray[i]; temp.y = initialArray[i + 1]; temp.z = initialArray[i + 2]; temp.w = initialArray[i + 3];
                    t = math.saturate(temp - baseHeight) / (1 - baseHeight) + math.saturate(baseHeight - temp) / baseHeight;
                    t = math.saturate(t);
                    temp = GetInterpolationLinksSIMD(temp, baseHeight, math.sqrt(1 - (t - 1) * (t - 1)));
                    initialArray[i] = temp.x; initialArray[i + 1] = temp.y; initialArray[i + 2] = temp.z; initialArray[i + 3] = temp.w;


                }

            if (initialArray.Length % 4 != 0)
            {
                int startI = initialArray.Length - (initialArray.Length % 4);
                if (initialArray.Length - (initialArray.Length % 4) < 0)
                    startI = 0;

                float temp1 = 0f;
                float t1 = 0;

                for (int i = startI; i < initialArray.Length; i++)
                {
                    temp1 = initialArray[i];
                    t1 = math.saturate(temp1 - baseHeight) / (1 - baseHeight) + math.saturate(baseHeight - temp1) / baseHeight;
                    t1 = math.saturate(t1);
                    temp1 = GetInterpolationLinks(temp1, baseHeight, math.sqrt(1 - (t1 - 1) * (t1 - 1)));
                    initialArray[i] = temp1;
                }
            }

            if (math.abs(CoordX) <= 2 && math.abs(CoordY) <= 2)
            {
                float sqrMagnitude;
                CoordX *= (sizeChunk - 1) * offset; CoordY *= (sizeChunk - 1) * offset;
                int pos;
                float t1;
                for (int y = 0; y < sizeChunk; y++)
                {
                    for (int x = 0; x < sizeChunk; x++)
                    {
                        sqrMagnitude = (CoordX + x * offset) * (CoordX + x * offset) + (CoordY + y * offset) * (CoordY + y * offset);
                        t1 = math.saturate(1 - math.sqrt(sqrMagnitude) / ((sizeChunk - 1) * offset * 2));

                        pos = x + y * sizeChunk;
                        initialArray[pos] = GetInterpolationLinks(initialArray[pos], startCoordHeight, fade(t1));
                    }
                }
            }
        }
    }   

    // модификация рельефа в зависимости от типа рельефа (0 - лесной, 1 - озёрный, 2 - гористый)
    public static float GetRelief(int reliefType, float waterLevel, float height, float maxHeight, int sizeChunk, float CoordX, float CoordY, int seed, float startCoordHeight)
    {
        var pos = new float2(CoordX, CoordY);
        if (reliefType == 0)
        {
            height = (height + PerlinLite(pos / 4096f, seed) + PerlinLite(pos / 2048f, seed)) * 0.33333f;
            height = height * 0.3f + 0.2f;
        }
        if (reliefType == 1)
        {
            float baseHeight = waterLevel + 100f / maxHeight;     

            var PHeights = new NativeArray<float>(3, Allocator.Temp);

            float smallHeightOffset = PerlinLite(pos / 60f, seed) * 5f / maxHeight + PerlinLite(pos / 8f, seed + 5) * 1f / maxHeight;            
            PHeights[0] = height * baseHeight;
            PHeights[1] = (PerlinLite(pos / 1024f, seed) + PerlinLite(pos / 512f, seed + 1)) * 0.5f * waterLevel * 2f + smallHeightOffset;
            PHeights[2] = (PerlinLite(pos / 1024f, seed + 4) + PerlinLite(pos / 512f, seed + 2) + PerlinLite(pos / 512f, seed + 3) + PerlinLite(pos / 2048f, seed + 1)) * 0.25f * waterLevel * 2f + smallHeightOffset;

            height = FusionPlots(pos / 560f, seed, PHeights);
        }
        if (reliefType == 2)
        {            
            height = fade((PerlinLite(pos / 4096f, seed) + PerlinLite(pos / 1024f, seed) + PerlinLite(pos / 640f, seed + 1) + height) * 0.25f * 0.48f + 0.52f);           
        }        

        return height;
    }

    private static (float x, float y, float z, float w) ComputeHeightVector(float4 localXVec, float localY, NativeArray<float> Links, int PosC00X, int PosC00Y, int PosC10X, int PosC10Y, int PosC01X, int PosC01Y, int PosC11X, int PosC11Y, int SideSize, int sqrSideSize)
    {    
        float4 C00 = VectorScalarSumSIMD(
            Links[PosC00X + PosC00Y * SideSize],
            Links[PosC00X + PosC00Y * SideSize + sqrSideSize],
            localXVec, localY
        );

        float4 C10 = VectorScalarSumSIMD(
            Links[PosC10X + PosC10Y * SideSize],
            Links[PosC10X + PosC10Y * SideSize + sqrSideSize],
            localXVec - 1, localY
        );

        float4 C01 = VectorScalarSumSIMD(
            Links[PosC01X + PosC01Y * SideSize],
            Links[PosC01X + PosC01Y * SideSize + sqrSideSize],
            localXVec, localY - 1
        );

        float4 C11 = VectorScalarSumSIMD(
            Links[PosC11X + PosC11Y * SideSize],
            Links[PosC11X + PosC11Y * SideSize + sqrSideSize],
            localXVec - 1, localY - 1
        );

        float4 alpha = GetIncreaseCurvatureSIMD(localXVec);
        float beta = GetIncreaseCurvature(localY);

        float4 C00C10 = GetInterpolationLinksSIMD(C00, C10, alpha);
        float4 C01C11 = GetInterpolationLinksSIMD(C01, C11, alpha);

        float4 result = 0.5f + GetInterpolationLinksSIMD(C00C10, C01C11, beta) / 2;

        return (result.x, result.y, result.z, result.w);
    }

    private static float4 VectorScalarSumSIMD(float linkX, float linkY, float4 localX, float localY)
    {        
        return linkX * localX + linkY * localY;
    }

    private static float4 GetIncreaseCurvatureSIMD(float4 vec)
    {
        return vec * vec * vec * ((6 * vec - 15) * vec + 10);
    }

    private static float4 GetInterpolationLinksSIMD(float4 a, float4 b, float4 t)
    {
        return a + (b - a) * t;
    }

    private static float4 GetInterpolationLinksSIMD(float4 a, float4 b, float t)
    {
        return a + (b - a) * t;
    }

    private float MainGetHeight(int PosC00X, int PosC00Y, int PosC10X, int PosC10Y, int PosC01X, int PosC01Y, int PosC11X, int PosC11Y, float localX, float localY)
    {
        float C00 = VectorScalarSum(heights[PosC00X + PosC00Y * SideSize], heights[PosC00X + PosC00Y * SideSize + sqrSideSize], localX, localY);
        float C10 = VectorScalarSum(heights[PosC10X + PosC10Y * SideSize], heights[PosC10X + PosC10Y * SideSize + sqrSideSize], localX - 1, localY);
        float C01 = VectorScalarSum(heights[PosC01X + PosC01Y * SideSize], heights[PosC01X + PosC01Y * SideSize + sqrSideSize], localX, localY - 1);
        float C11 = VectorScalarSum(heights[PosC11X + PosC11Y * SideSize], heights[PosC11X + PosC11Y * SideSize + sqrSideSize], localX - 1, localY - 1);

        float alpha = GetIncreaseCurvature(localX);
        float beta = GetIncreaseCurvature(localY);

        float C00C10 = GetInterpolationLinks(C00, C10, alpha);
        float C01C11 = GetInterpolationLinks(C01, C11, alpha);

        return 0.5f + GetInterpolationLinks(C00C10, C01C11, beta) / 2;
    }

    private static float MainGetHeight(float localX, float localY, NativeArray<float> Links, int PosC00X, int PosC00Y, int PosC10X, int PosC10Y, int PosC01X, int PosC01Y, int PosC11X, int PosC11Y, int SideSize, int sqrSideSize)
    {
        float C00 = VectorScalarSum(Links[PosC00X + PosC00Y * SideSize], Links[PosC00X + PosC00Y * SideSize + sqrSideSize], localX, localY);
        float C10 = VectorScalarSum(Links[PosC10X + PosC10Y * SideSize], Links[PosC10X + PosC10Y * SideSize + sqrSideSize], localX - 1, localY);
        float C01 = VectorScalarSum(Links[PosC01X + PosC01Y * SideSize], Links[PosC01X + PosC01Y * SideSize + sqrSideSize], localX, localY - 1);
        float C11 = VectorScalarSum(Links[PosC11X + PosC11Y * SideSize], Links[PosC11X + PosC11Y * SideSize + sqrSideSize], localX - 1, localY - 1);

        float alpha = GetIncreaseCurvature(localX);
        float beta = GetIncreaseCurvature(localY);

        float C00C10 = GetInterpolationLinks(C00, C10, alpha);
        float C01C11 = GetInterpolationLinks(C01, C11, alpha);

        return 0.5f + GetInterpolationLinks(C00C10, C01C11, beta) / 2;
    }

    private static float GetInterpolationSettings(float C00, float C10, float C01, float C11, float localSettingX, float localSettingY)
    {
        float alpha = GetIncreaseCurvature(localSettingX);
        float beta = GetIncreaseCurvature(localSettingY);

        float C00C10 = GetInterpolationLinks(C00, C10, alpha);
        float C01C11 = GetInterpolationLinks(C01, C11, alpha);

        return GetInterpolationLinks(C00C10, C01C11, beta);
    }

    private (float, float) GetRiverHeight(Vector2 p, ref float currentHeightBottomRiver, int i, Vector2Int RiverDataPos)
    {
        float result = 1; float distance = 0;    

        if (rivers.ContainsKey(RiverDataPos))
        {
            currentHeightBottomRiver = rivers[RiverDataPos][i + countElementsRiver - 9];

            distance = GetMinDistanceBezier(new Vector2(rivers[RiverDataPos][i + countElementsRiver - 6], rivers[RiverDataPos][i + countElementsRiver - 5]), new Vector2(rivers[RiverDataPos][i + countElementsRiver - 4]
            , rivers[RiverDataPos][i + countElementsRiver - 3]), new Vector2(rivers[RiverDataPos][i + countElementsRiver - 2], rivers[RiverDataPos][i + countElementsRiver - 1]), p);

            result = distance / rivers[RiverDataPos][i + countElementsRiver - 8];

            if (result > 1)
            {
                result = 1 - (rivers[RiverDataPos][i + countElementsRiver - 10] - result * rivers[RiverDataPos][i + countElementsRiver - 8]) / (rivers[RiverDataPos][i + countElementsRiver - 10] - rivers[RiverDataPos][i + countElementsRiver - 8]);
                if (result > 1)
                    result = 1;
                
                result = GetInterpolationLinks(waterLevel, 1, GetIncreaseCurvature(result));
            }    
            else
            result = GetInterpolationLinks(rivers[RiverDataPos][i + countElementsRiver - 9], waterLevel, GetIncreaseCurvature(result));            
        }
        
        if (result > 1)
            result = 1;

        return (result, distance);
    }    

    private static (float, float, float) GetRiverHeightPoint(float2 Q, NativeArray<float> dataRivers, float waterLevel, int countElementsRiver, int i)
    {
        float result = 1;

        var currentHeightBottomRiver = dataRivers[i + countElementsRiver - 9];

        float distance = GetMinDistanceBezier(new float2(dataRivers[i + countElementsRiver - 6], dataRivers[i + countElementsRiver - 5]), new float2(dataRivers[i + countElementsRiver - 4]
        , dataRivers[i + countElementsRiver - 3]), new float2(dataRivers[i + countElementsRiver - 2], dataRivers[i + countElementsRiver - 1]), Q);

        result = distance / dataRivers[i + countElementsRiver - 8];

        if (result > 1)
        {
            result = 1 - (dataRivers[i + countElementsRiver - 10] - result * dataRivers[i + countElementsRiver - 8]) / (dataRivers[i + countElementsRiver - 10] - dataRivers[i + countElementsRiver - 8]);
            if (result > 1)
                result = 1;

            result = GetInterpolationLinks(waterLevel, 1, GetIncreaseCurvature(result));
        }
        else
            result = GetInterpolationLinks(currentHeightBottomRiver, waterLevel, GetIncreaseCurvature(result));
        

        if (result > 1)
            result = 1;     

        return (result, currentHeightBottomRiver, distance);
    }

    private static float fade(float t) => t * t * t * (t * (t * 6 - 15) + 10);

    private static float2 GetGradient(int x, int y, int seed)
    {
        int hash = (x * 73856093) ^ (y * 19349663) ^ (seed * 83492791);
        hash &= 255; // ������������ �� 0..255

        // ����������� ��� � ����
        float angle = hash * (2 * math.PI / 255); // 256 �����������

        return new float2(math.cos(angle), math.sin(angle));
    }

    private static float PerlinLite(float2 pos, int seed)
    {
        int x0 = (int)math.floor(pos.x);
        int y0 = (int)math.floor(pos.y);
        float2 rel = pos - new float2(x0, y0);

        float2 g00 = GetGradient(x0, y0, seed);
        float2 g10 = GetGradient(x0 + 1, y0, seed);
        float2 g01 = GetGradient(x0, y0 + 1, seed);
        float2 g11 = GetGradient(x0 + 1, y0 + 1, seed);

        float n00 = math.dot(g00, rel);
        float n10 = math.dot(g10, rel - new float2(1, 0));
        float n01 = math.dot(g01, rel - new float2(0, 1));
        float n11 = math.dot(g11, rel - new float2(1, 1));

        float u = fade(rel.x);
        float v = fade(rel.y);

        float ix0 = math.lerp(n00, n10, u);
        float ix1 = math.lerp(n01, n11, u);

        float value = math.lerp(ix0, ix1, v);

        return value * 0.5f + 0.5f;
    }

    private static float FusionPlots(float2 pos, int seed, NativeArray<float> PerlinOctavesHeights)
    {
        int x0 = (int)math.floor(pos.x);
        int y0 = (int)math.floor(pos.y);
        float2 rel = pos - new float2(x0, y0);

        float n00; float n10; float n01; float n11;
        
        int hash = math.abs((x0 * 73856093) ^ (y0 * 19349663) ^ (seed * 83492791));
        int idx0 = hash % PerlinOctavesHeights.Length;
        int idx1 = (idx0 + 1) % PerlinOctavesHeights.Length;

        float blend = (hash & 63) / 63f; // ��� ��������� (0..1)
        n00 = math.lerp(PerlinOctavesHeights[idx0], PerlinOctavesHeights[idx1], blend);

        hash = math.abs(((x0 + 1) * 73856093) ^ (y0 * 19349663) ^ (seed * 83492791));
        idx0 = hash % PerlinOctavesHeights.Length;
        idx1 = (idx0 + 1) % PerlinOctavesHeights.Length;

        blend = (hash & 63) / 63f; // ��� ��������� (0..1)
        n10 = math.lerp(PerlinOctavesHeights[idx0], PerlinOctavesHeights[idx1], blend);

        hash = math.abs((x0 * 73856093) ^ ((y0 + 1) * 19349663) ^ (seed * 83492791));
        idx0 = hash % PerlinOctavesHeights.Length;
        idx1 = (idx0 + 1) % PerlinOctavesHeights.Length;

        blend = (hash & 63) / 63f; // ��� ��������� (0..1)
        n01 = math.lerp(PerlinOctavesHeights[idx0], PerlinOctavesHeights[idx1], blend);

        hash = math.abs(((x0 + 1) * 73856093) ^ ((y0 + 1) * 19349663) ^ (seed * 83492791));
        idx0 = hash % PerlinOctavesHeights.Length;
        idx1 = (idx0 + 1) % PerlinOctavesHeights.Length;

        blend = (hash & 63) / 63f; // ��� ��������� (0..1)
        n11 = math.lerp(PerlinOctavesHeights[idx0], PerlinOctavesHeights[idx1], blend);

        float u = fade(rel.x);
        float v = fade(rel.y);

        float ix0 = math.lerp(n00, n10, u);
        float ix1 = math.lerp(n01, n11, u);

        float value = math.lerp(ix0, ix1, v);

        return value;
    }

    private static float FusionPlots(float2 pos, int seed, float[] PerlinOctavesHeights)
    {
        int x0 = (int)math.floor(pos.x);
        int y0 = (int)math.floor(pos.y);
        float2 rel = pos - new float2(x0, y0);

        float n00; float n10; float n01; float n11;

        int hash = math.abs((x0 * 73856093) ^ (y0 * 19349663) ^ (seed * 83492791));
        int idx0 = hash % PerlinOctavesHeights.Length;
        int idx1 = (idx0 + 1) % PerlinOctavesHeights.Length;

        float blend = (hash & 63) / 63f; // ��� ��������� (0..1)
        n00 = math.lerp(PerlinOctavesHeights[idx0], PerlinOctavesHeights[idx1], blend);

        hash = math.abs(((x0 + 1) * 73856093) ^ (y0 * 19349663) ^ (seed * 83492791));
        idx0 = hash % PerlinOctavesHeights.Length;
        idx1 = (idx0 + 1) % PerlinOctavesHeights.Length;

        blend = (hash & 63) / 63f; // ��� ��������� (0..1)
        n10 = math.lerp(PerlinOctavesHeights[idx0], PerlinOctavesHeights[idx1], blend);

        hash = math.abs((x0 * 73856093) ^ ((y0 + 1) * 19349663) ^ (seed * 83492791));
        idx0 = hash % PerlinOctavesHeights.Length;
        idx1 = (idx0 + 1) % PerlinOctavesHeights.Length;

        blend = (hash & 63) / 63f; // ��� ��������� (0..1)
        n01 = math.lerp(PerlinOctavesHeights[idx0], PerlinOctavesHeights[idx1], blend);

        hash = math.abs(((x0 + 1) * 73856093) ^ ((y0 + 1) * 19349663) ^ (seed * 83492791));
        idx0 = hash % PerlinOctavesHeights.Length;
        idx1 = (idx0 + 1) % PerlinOctavesHeights.Length;

        blend = (hash & 63) / 63f; // ��� ��������� (0..1)
        n11 = math.lerp(PerlinOctavesHeights[idx0], PerlinOctavesHeights[idx1], blend);

        float u = fade(rel.x);
        float v = fade(rel.y);

        float ix0 = math.lerp(n00, n10, u);
        float ix1 = math.lerp(n01, n11, u);

        float value = math.lerp(ix0, ix1, v);

        return value;
    }

    // функции, связанные с генерацией высот рек
    
    private static float ModifyHeightWithRiver(float x, float z, float height, float distToRiver, NativeArray<float> dataRivers, int i, int countElementsRiver, float WaterLevel, float bottomDepth, int seed)
    {
        float2 pos = new float2(x, z);
        
        var PerlinHeights = new NativeArray<float>(4, Allocator.Temp);
        PerlinHeights[0] = -PerlinLite(pos / 64f, seed); // �������� ����� ������ � ������� ������
        PerlinHeights[1] = -(PerlinLite(pos / 32f, seed) * 0.75f); // ������� ���������� ������
        PerlinHeights[2] = (PerlinLite(pos / 16f, seed) - 0.5f) * 0.5f; // ���������� � �������
        PerlinHeights[3] = -(PerlinLite(pos / 128f, seed + 1) * 0.6f); // ��������� ���������� �� ����� ������� ��������


        float perlinHeight = FusionPlots(pos / 64f, seed, PerlinHeights);

        float erosionDepth = (WaterLevel - bottomDepth) * 0.5f * perlinHeight;

        float t = math.saturate(1f - distToRiver / dataRivers[i + countElementsRiver - 10]);
        return height - erosionDepth * t;
    }

    private float ModifyHeightRiver(float x, float z, float height, float distToRiver, Vector2Int RiverDataPos, int i)
    {
        float2 pos = new float2(x, z);

        var PerlinHeights = new float[4];
        PerlinHeights[0] = -PerlinLite(pos / 64f, seed); // �������� ����� ������ � ������� ������
        PerlinHeights[1] = -(PerlinLite(pos / 32f, seed) * 0.75f); // ������� ���������� ������
        PerlinHeights[2] = (PerlinLite(pos / 16f, seed) - 0.5f) * 0.5f; // ���������� � �������
        PerlinHeights[3] = -(PerlinLite(pos / 128f, seed + 1) * 0.6f); // ��������� ���������� �� ����� ������� ��������


        float perlinHeight = FusionPlots(pos / 64f, seed, PerlinHeights);

        float erosionDepth = (waterLevel - rivers[RiverDataPos][i + countElementsRiver - 9]) * 0.5f * perlinHeight;

        float t = math.saturate(1f - distToRiver / rivers[RiverDataPos][i + countElementsRiver - 10]);
        return height - erosionDepth * t;
    }

    private static float GetIncreaseCurvature(float t)
    {
        return t * t * t * ((6 * t - 15) * t + 10);
    }

    private static float GetInterpolationLinks(float value1, float value2, float t)
    {
        return value1 + (value2 - value1) * t;
    }

    private static float VectorScalarSum(float linkX, float linkY, float pointX, float pointY)
    {
        return linkX * pointX + linkY * pointY;
    }    

    
    private static float2 GetQuadraticBezier(float t, float2 P0, float2 P1, float2 P2)
    {        
        return (1 - t) * (1 - t) * P0 + 2 * (1 - t) * P1 * t + P2 * t * t;
    }

    private static float GetQuadraticBezier(float t, float P0, float P1, float P2)
    {
        return (1 - t) * (1 - t) * P0 + 2 * (1 - t) * P1 * t + P2 * t * t;
    }

    private static Vector2 GetLinearInterpolation(float t, Vector2 P0, Vector2 P1)
    {
        return P0 + (P1 - P0) * t;
    }    

    private static bool GetIsBezierInRadiusBorder(Vector2 P0, Vector2 P1, Vector2 P2, Vector2 Q1, Vector2 Q2, float q, float borderQ1, float borderQ2, bool isX, float radius)
    {        
        float p0; float p1; float p2;
        float bordP0; float bordP1; float bordP2;
        if (isX)
        {
            p0 = P0.x; p1 = P1.x; p2 = P2.x; 
            bordP0 = P0.y; bordP1 = P1.y; bordP2 = P2.y;
        }
        else
        {
            p0 = P0.y; p1 = P1.y; p2 = P2.y; 
            bordP0 = P0.x; bordP1 = P1.x; bordP2 = P2.x; 
        }

        (float T1, float T2) = FindRoots(p0, p1, p2, q);       
        
        
        float checkBord = GetQuadraticBezier(T1, bordP0, bordP1, bordP2);
        if (borderQ1 <= checkBord && checkBord <= borderQ2)
        {
            if (math.abs(q - GetQuadraticBezier(T1, p0, p1, p2)) <= radius) return true;
        }

        checkBord = GetQuadraticBezier(T2, bordP0, bordP1, bordP2);
        if (borderQ1 <= checkBord && checkBord <= borderQ2)
        {
            if (math.abs(q - GetQuadraticBezier(T2, p0, p1, p2)) <= radius) return true;
        }
        
        T1 = 0; T2 = 1;

        checkBord = GetQuadraticBezier(T1, bordP0, bordP1, bordP2);
        if (borderQ1 <= checkBord && checkBord <= borderQ2)
        {
            if (math.abs(q - GetQuadraticBezier(T1, p0, p1, p2)) <= radius) return true;
        }

        checkBord = GetQuadraticBezier(T2, bordP0, bordP1, bordP2);
        if (borderQ1 <= checkBord && checkBord <= borderQ2)
        {
            if (math.abs(q - GetQuadraticBezier(T2, p0, p1, p2)) <= radius) return true;
        }
        

        if (GetMinDistanceBezier(P0, P1, P2, Q1) <= radius) return true;

        if (GetMinDistanceBezier(P0, P1, P2, Q2) <= radius) return true;

        return false;
    }  

    private static (float, float) FindRoots(float P0, float P1, float P2, float Q)
    {
        return SolveCubic(CalculateA(P0, P1, P2), CalculateB(P0, P1, P2), CalculateC(P0, P1, P2, Q), CalculateD(P0, P1, Q), P0, P1, P2, Q);
    }

    private static float CalculateA(float P0, float P1, float P2)
    {
        float A = P0 * P0 - 2 * P0 * P1 + P0 * P2 - 2 * P1 * P0 + 4 * P1 * P1 - 2 * P1 * P2 + P2 * P0 - 2 * P2 * P1 + P2 * P2;        
        return A;
    }

    private static float CalculateB(float P0, float P1, float P2)
    {
        float B = -3 * P0 * P0 + 5 * P0 * P1 - 2 * P0 * P2 + 4 * P1 * P0 - 6 * P1 * P1 + 2 * P1 * P2 - P2 * P0 + P2 * P1;        
        return B;
    }

    private static float CalculateC(float P0, float P1, float P2, float Q)
    {
        float C = 3 * P0 * P0 - 4 * P0 * P1 + P0 * P2 - 2 * P1 * P0 + 2 * P1 * P1 - Q * P0 + 2 * Q * P1 - Q * P2;        
        return C;
    }

    private static float CalculateD(float P0, float P1, float Q)
    {
        float D = -P0 * P0 + P0 * P1 + Q * P0 - Q * P1;        
        return D;
    }


    private bool IsRiverChunk(Vector2Int posChunk, Vector2 P0, Vector2 P1, Vector2 P2, float riverRadius)
    {
        int globalSize = (sizeChunk - 1) * countChunks;
        posChunk *= globalSize;        
        
        Vector2 Q1 = posChunk; Vector2 Q2 = posChunk + Vector2.up * globalSize;
        if (GetIsBezierInRadiusBorder(P0, P1, P2, Q1, Q2, Q1.x, Q1.y, Q2.y, true, riverRadius)) return true;

        Q1 = posChunk + Vector2.right * globalSize; Q2 = posChunk + (Vector2.up + Vector2.right) * globalSize;
        if (GetIsBezierInRadiusBorder(P0, P1, P2, Q1, Q2, Q1.x, Q1.y, Q2.y, true, riverRadius)) return true;

        Q1 = posChunk; Q2 = posChunk + Vector2.right * globalSize;
        if (GetIsBezierInRadiusBorder(P0, P1, P2, Q1, Q2, Q1.y, Q1.x, Q2.x, false, riverRadius)) return true;

        Q1 = posChunk + Vector2.up * globalSize; Q2 = posChunk + (Vector2.up + Vector2.right) * globalSize;
        if (GetIsBezierInRadiusBorder(P0, P1, P2, Q1, Q2, Q1.y, Q1.x, Q2.x, false, riverRadius)) return true;

        return false;
    }

    private static (float, float) GetMinBezier(float2 p0, float2 p1, float2 p2, float2 Q)
    {
        float T = FindRoots(p0, p1, p2, Q);        

        var minDistance = float.MaxValue;
        if (T >= 0 && T <= 1)
        minDistance = GetDistanceBezier(T, p0, p1, p2, Q);

        // ����� ��������� ���������� �� �������� t = 0 � t = 1
        float distanceAt0 = GetDistanceBezier(0, p0, p1, p2, Q);
        float distanceAt1 = GetDistanceBezier(1, p0, p1, p2, Q);

        if (distanceAt0 < minDistance)
        {
            T = 0f;
            minDistance = distanceAt0;
        }

        if (distanceAt1 < minDistance)
        {
            T = 1f;
            minDistance = distanceAt1;
        }

        return (T, minDistance);
    }    

    private static float GetMinDistanceBezier(float2 p0, float2 p1, float2 p2, float2 Q)
    {
        (float T, float distance) = GetMinBezier(p0, p1, p2, Q);
        return distance;
    }   

    private static float GetMinDistanceBezierT(float2 p0, float2 p1, float2 p2, float2 Q)
    {
        (float T, float distance) = GetMinBezier(p0, p1, p2, Q);
        return T;
    }


    private static float FindRoots(float2 P0, float2 P1, float2 P2, float2 Q)
    {
        return SolveCubic(CalculateA(P0, P1, P2), CalculateB(P0, P1, P2), CalculateC(P0, P1, P2, Q), CalculateD(P0, P1, Q), P0, P1, P2, Q);        
    }       


    private static float CalculateA(float2 P0, float2 P1, float2 P2)
    {
        float Ax = P0.x * P0.x - 2 * P0.x * P1.x + P0.x * P2.x - 2 * P1.x * P0.x + 4 * P1.x * P1.x - 2 * P1.x * P2.x + P2.x * P0.x - 2 * P2.x * P1.x + P2.x * P2.x;
        float Ay = P0.y * P0.y - 2 * P0.y * P1.y + P0.y * P2.y - 2 * P1.y * P0.y + 4 * P1.y * P1.y - 2 * P1.y * P2.y + P2.y * P0.y - 2 * P2.y * P1.y + P2.y * P2.y;
        return Ax + Ay;
    }

    private static float CalculateB(float2 P0, float2 P1, float2 P2)
    {
        float Bx = -3 * P0.x * P0.x + 5 * P0.x * P1.x - 2 * P0.x * P2.x + 4 * P1.x * P0.x - 6 * P1.x * P1.x + 2 * P1.x * P2.x - P2.x * P0.x + P2.x * P1.x;
        float By = -3 * P0.y * P0.y + 5 * P0.y * P1.y - 2 * P0.y * P2.y + 4 * P1.y * P0.y - 6 * P1.y * P1.y + 2 * P1.y * P2.y - P2.y * P0.y + P2.y * P1.y;
        return Bx + By;
    }

    private static float CalculateC(float2 P0, float2 P1, float2 P2, float2 Q)
    {
        float Cx = 3 * P0.x * P0.x - 4 * P0.x * P1.x + P0.x * P2.x - 2 * P1.x * P0.x + 2 * P1.x * P1.x - Q.x * P0.x + 2 * Q.x * P1.x - Q.x * P2.x;
        float Cy = 3 * P0.y * P0.y - 4 * P0.y * P1.y + P0.y * P2.y - 2 * P1.y * P0.y + 2 * P1.y * P1.y - Q.y * P0.y + 2 * Q.y * P1.y - Q.y * P2.y;
        return Cx + Cy;
    }

    private static float CalculateD(float2 P0, float2 P1, float2 Q)
    {
        float Dx = -P0.x * P0.x + P0.x * P1.x + Q.x * P0.x - Q.x * P1.x;
        float Dy = -P0.y * P0.y + P0.y * P1.y + Q.y * P0.y - Q.y * P1.y;
        return Dx + Dy;
    }

    private static float SolveCubic(float A, float B, float C, float D, float2 P0, float2 P1, float2 P2, float2 Q)
    {        
        // ��������� � ������������� ���� t^3 + pt^2 + qt + r = 0
        float p = B / A;
        float q = C / A;
        float r = D / A;

        // ��������������� ����������
        float a = q - p * p / 3.0f;
        float b = 2.0f * p * p * p / 27.0f - p * q / 3.0f + r;

        float discriminant = b * b / 4.0f + a * a * a / 27.0f;
        
        if (discriminant > 0)
        {
            // ���� �������������� ������
            float sqrtDiscriminant = math.sqrt(discriminant);            
            float u = Cbrt(-b / 2.0f + sqrtDiscriminant);
            float v = Cbrt(-b / 2.0f - sqrtDiscriminant);            
            float t1 = u + v - p / 3.0f;            

            return t1;
        }
        else if (discriminant == 0)
        {
            // ��� �������������� �����, ��� �� ������� ���������
            float u = Cbrt(-b / 2.0f);
            float t1 = 2 * u - p / 3.0f;
            float t2 = -u - p / 3.0f;
                        
            float minT = 0;
            float minDistance = float.MaxValue;
            float distance;

            if (t1 >= 0 && t1 <= 1)
            {
                distance = GetDistanceBezierSqr(t1, P0, P1, P2, Q);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    minT = t1;
                }
            }
            if (t2 >= 0 && t2 <= 1)
            {
                distance = GetDistanceBezierSqr(t2, P0, P1, P2, Q);
                if (distance < minDistance)
                {                    
                    minT = t2;
                }
            }

            return minT;
        }
        else
        {
            // ��� ��������� �������������� �����            
            float theta = math.acos(-b / 2.0f / math.sqrt(-a * a * a / 27.0f));
            
            float sqrtNegA = 2.0f * math.sqrt(-a / 3.0f);
            
            float t1 = sqrtNegA * math.cos(theta / 3.0f) - p / 3.0f;
            float t2 = sqrtNegA * math.cos((theta + 2.0f * math.PI) / 3.0f) - p / 3.0f;
            float t3 = sqrtNegA * math.cos((theta + 4.0f * math.PI) / 3.0f) - p / 3.0f;

            float minT = 0;            
            float minDistance = float.MaxValue;
            float distance;            

            if (t1 >= 0 && t1 <= 1)
            {
                distance = GetDistanceBezierSqr(t1, P0, P1, P2, Q);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    minT = t1;
                }
            }
            if (t2 >= 0 && t2 <= 1)
            {
                distance = GetDistanceBezierSqr(t2, P0, P1, P2, Q);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    minT = t2;
                }
            }
            if (t3 >= 0 && t3 <= 1)
            {
                distance = GetDistanceBezierSqr(t3, P0, P1, P2, Q);
                if (distance < minDistance)
                {                    
                    minT = t3;
                }
            }

            return minT;
        }                
    }

    private static (float, float) SolveCubic(float A, float B, float C, float D, float P0, float P1, float P2, float Q)
    {
        // ��������� � ������������� ���� t^3 + pt^2 + qt + r = 0
        float p = B / A;
        float q = C / A;
        float r = D / A;

        // ��������������� ����������
        float a = q - p * p / 3.0f;
        float b = 2.0f * p * p * p / 27.0f - p * q / 3.0f + r;

        float discriminant = b * b / 4.0f + a * a * a / 27.0f;

        if (discriminant > 0)
        {
            // ���� �������������� ������
            float sqrtDiscriminant = math.sqrt(discriminant);
            float u = Cbrt(-b / 2.0f + sqrtDiscriminant);
            float v = Cbrt(-b / 2.0f - sqrtDiscriminant);
            float t1 = u + v - p / 3.0f;

            return (t1, 0);
        }
        else if (discriminant == 0)
        {
            // ��� �������������� �����, ��� �� ������� ���������
            float u = Cbrt(-b / 2.0f);
            float t1 = 2 * u - p / 3.0f;
            float t2 = -u - p / 3.0f;

            float minT1 = 0;
            float minT2 = 0;
            float minDistance = float.MaxValue;
            float distance;

            if (t1 >= 0 && t1 <= 1)
            {
                distance = math.abs(Q - GetQuadraticBezier(t1, P0, P1, P2));                
                    minDistance = distance;
                    minT1 = t1;                
            }
            if (t2 >= 0 && t2 <= 1)
            {
                distance = math.abs(Q - GetQuadraticBezier(t2, P0, P1, P2));
                if (distance < minDistance)
                {
                    minT1 = t2;
                }
                else if(distance == minDistance)
                {
                    minT2 = t2;
                }
            }

            return (minT1, minT2);
        }
        else
        {
            // ��� ��������� �������������� �����            
            float theta = math.acos(-b / 2.0f / math.sqrt(-a * a * a / 27.0f));

            float sqrtNegA = 2.0f * math.sqrt(-a / 3.0f);

            float t1 = sqrtNegA * math.cos(theta / 3.0f) - p / 3.0f;
            float t2 = sqrtNegA * math.cos((theta + 2.0f * math.PI) / 3.0f) - p / 3.0f;
            float t3 = sqrtNegA * math.cos((theta + 4.0f * math.PI) / 3.0f) - p / 3.0f;

            float minT1 = 0;
            float minT2 = 0;
            float minDistance = float.MaxValue;
            float distance;

            if (t1 >= 0 && t1 <= 1)
            {
                distance = math.abs(Q - GetQuadraticBezier(t1, P0, P1, P2));                
                    minDistance = distance;
                    minT1 = t1;                
            }
            if (t2 >= 0 && t2 <= 1)
            {
                distance = math.abs(Q - GetQuadraticBezier(t2, P0, P1, P2));
                if (distance < minDistance)
                {
                    minDistance = distance;
                    minT1 = t2;
                }
                else if (distance == minDistance)
                {
                    minT2 = t2;
                }
            }
            if (t3 >= 0 && t3 <= 1)
            {
                distance = math.abs(Q - GetQuadraticBezier(t3, P0, P1, P2));
                if (distance < minDistance)
                {
                    minT1 = t3;
                }
                else if (distance == minDistance)
                {
                    minT2 = t3;
                }
            }

            return (minT1, minT2);
        }
    }


    private static float Cbrt(float value)
    {
        return value < 0 ? -math.pow(math.abs(value), 1.0f / 3.0f) : math.pow(math.abs(value), 1.0f / 3.0f);
    }    

    private static float GetDistanceBezierSqr(float t, float2 P0, float2 P1, float2 P2, float2 Q)
    {
        var PointBezier = GetQuadraticBezier(t, P0, P1, P2);
        return (PointBezier.x - Q.x) * (PointBezier.x - Q.x) + (PointBezier.y - Q.y) * (PointBezier.y - Q.y);
    }

    private static float GetDistanceBezier(float t, float2 P0, float2 P1, float2 P2, float2 Q)
    {
        var PointBezier = GetQuadraticBezier(t, P0, P1, P2);
        return math.sqrt((PointBezier.x - Q.x) * (PointBezier.x - Q.x) + (PointBezier.y - Q.y) * (PointBezier.y - Q.y));
    }  
}