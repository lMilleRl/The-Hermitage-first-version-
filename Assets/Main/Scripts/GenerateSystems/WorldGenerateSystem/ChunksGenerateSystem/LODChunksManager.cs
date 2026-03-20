using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class LODChunksManager : MonoBehaviour
{
    [Range(2, 1000)][SerializeField] public int InitRadiusLodBatches;
    [HideInInspector] public int RadiusLodBatches;
    [HideInInspector] public int RadiusToDontDrawBatches;

    private int sqrViewRadiusBatches;
    private int SqrDistToDontDraw;
    private int sqrViewRadiusBatchesInequality;

    [HideInInspector] public int CountOriginalChunksInLengthBatch;

    private int CountBatches;

    [HideInInspector] public float polygonScale;

    private List<LodChunkData> LodChunks;    
    private List<int> ChunksToMove;
    private HashSet<Vector2Int> ChunksPos;

    [HideInInspector] public Queue<JobHandle> ChunkGenerateQueue;
    [HideInInspector] public Queue<MeshChunkBuilder.GenerateLodMeshParallel> ChunkGenerateJobQueue;

    [HideInInspector] public int seed;

    public class LodChunkData
    {
        public Vector2Int LodChunkPos;
        public Vector3 WorldPos;
        public LodChunkState state;
        public Mesh mesh;
        public int index;
        public int LodLevelIndex;

        public LodChunkData(Vector2Int ChunkPos, LodChunkState state, Mesh mesh, int index, int LodLevelIndex)
        {
            this.index = index;
            LodChunkPos = ChunkPos;
            this.state = state;
            this.mesh = mesh;
            this.LodLevelIndex = LodLevelIndex;
        }
    }

    public enum LodChunkState
    {
        NotToMove,
        ToMove
    }

    [Range(1, 16)]
    [SerializeField] public int LodDensity;

    private uint[] NormalsLodTriangles;
    private uint[] LodTriangles;

    public void InitLodTrianlges()
    {
        LodTriangles = new uint[CountOriginalChunksInLengthBatch * CountOriginalChunksInLengthBatch * 6 * LodDensity * LodDensity];

        int index = 0;
        for (uint y = 0; y < CountOriginalChunksInLengthBatch * LodDensity; y++)
        {
            for (uint x = 0; x < CountOriginalChunksInLengthBatch * LodDensity; x++)
            {
                uint topLeft = (uint)(x + y * (CountOriginalChunksInLengthBatch * LodDensity + 1));
                uint bottomLeft = (uint)(x + (y + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1));
                uint topRight = topLeft + 1;
                uint bottomRight = bottomLeft + 1;

                // Первый треугольник
                LodTriangles[index++] = topLeft;
                LodTriangles[index++] = bottomLeft;
                LodTriangles[index++] = topRight;

                // Второй треугольник
                LodTriangles[index++] = topRight;
                LodTriangles[index++] = bottomLeft;
                LodTriangles[index++] = bottomRight;
            }
        }
        NormalsLodTriangles = new uint[(CountOriginalChunksInLengthBatch * LodDensity + 2) * (CountOriginalChunksInLengthBatch * LodDensity + 2) * 6];

        index = 0;
        for (uint y = 0; y < CountOriginalChunksInLengthBatch * LodDensity + 2; y++)
        {
            for (uint x = 0; x < CountOriginalChunksInLengthBatch * LodDensity + 2; x++)
            {
                uint topLeft = (uint)(x + y * (CountOriginalChunksInLengthBatch * LodDensity + 3));
                uint bottomLeft = (uint)(x + (y + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 3));
                uint topRight = topLeft + 1;
                uint bottomRight = bottomLeft + 1;

                // Первый треугольник
                NormalsLodTriangles[index++] = topLeft;
                NormalsLodTriangles[index++] = bottomLeft;
                NormalsLodTriangles[index++] = topRight;

                // Второй треугольник
                NormalsLodTriangles[index++] = topRight;
                NormalsLodTriangles[index++] = bottomLeft;
                NormalsLodTriangles[index++] = bottomRight;
            }
        }
    }

    private PerlinNoiseData perlinData;
    private PerlinNoise perlin;

    [HideInInspector] public Vector2Int currentPlayerChunk;

    public float DistToChangePosView { private set; get; }
    public void CreateViewRangeChunks()
    {
        StopCoroutine(ChunkGenerate());
        StopCoroutine(AddChunksToMove());
        StopCoroutine(SetBatchesMesh());

        InitLodTrianlges();

        perlin = GetComponent<PerlinNoise>();
        perlinData = GetComponent<ChunksGenerator>().perlinData;

        ConvertBatchPosInWorldPos = CountOriginalChunksInLengthBatch * (perlinData.sizeChunk - 1) * polygonScale;
        sqrViewRadiusBatches = RadiusLodBatches * RadiusLodBatches;
        sqrViewRadiusBatchesInequality = (RadiusLodBatches) * (RadiusLodBatches);

        DistToChangePosView = 0.5f + (float)(perlinData.sizeChunk - 1) / ConvertBatchPosInWorldPos;

        SqrDistToDontDraw = (RadiusToDontDrawBatches) * (RadiusToDontDrawBatches);
        CountBatches = 4 * sqrViewRadiusBatches;

        ChunkGenerateQueue = new Queue<JobHandle>(CountBatches);
        ChunkGenerateJobQueue = new Queue<MeshChunkBuilder.GenerateLodMeshParallel>(CountBatches);
        
        LodChunks = new List<LodChunkData>(CountBatches);
        ChunksToMove = new List<int>(CountBatches);
        ChunksPos = new HashSet<Vector2Int>(CountBatches);        
                
        int X;
        int Z;
        int index = 0;

        var chunkPos = new List<Vector2Int>(CountBatches);
        var chunkIndex = new List<int>(CountBatches);

        int dist;
        Vector2Int localPos;
        for (int i = 0; i < 4 * sqrViewRadiusBatches; i++)
        {
            X = i % (2 * RadiusLodBatches) + currentPlayerChunk.x - RadiusLodBatches;
            Z = i / (2 * RadiusLodBatches) + currentPlayerChunk.y - RadiusLodBatches;

            localPos = new Vector2Int(X, Z) - currentPlayerChunk;
            if ((localPos.x == 0 && Mathf.Abs(localPos.y) == RadiusLodBatches) || (localPos.y == 0 && Mathf.Abs(localPos.x) == RadiusLodBatches)) continue;

            dist = localPos.sqrMagnitude;
            if (dist > sqrViewRadiusBatchesInequality) continue;

            if (ChunksPos.Contains(new Vector2Int(X, Z))) continue;

            LodChunkData chunkData = new LodChunkData(new Vector2Int(X, Z), LodChunkState.ToMove, new Mesh(), index, 0);
                        
            chunkIndex.Add(index);

            SetDataChunkForMeshGenerate(InitChunk(chunkData), chunkPos);

            index++;
        }

        var parallelJob = new MeshChunkBuilder.GenerateLodMeshParallel()
        {
            seed = seed,

            OriginalSizeChunk = perlinData.sizeChunk,
            CountOriginalChunksInLengthBatch = CountOriginalChunksInLengthBatch,

            maxHeight = perlinData.maxHeight,

            PerlinCountChunks = perlinData.countChunks,
            settingSideProportion = perlinData.settingSideProportion,
            lengthRowOct = perlinData.LengthRowOctaves,
            oneSetting = perlinData.oneSetting,
            SettingsSize = perlinData.settingsSize,

            CountVerticiesInLodChunk = CountOriginalChunksInLengthBatch * LodDensity + 1,
            SqrCountVerticiesInLodChunk = (CountOriginalChunksInLengthBatch * LodDensity + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1),
            LodDensity = LodDensity,

            SideSize = perlinData.SideSize,
            waterLevel = perlinData.waterLevel,
            countElementsRiver = perlinData.countElementsRiver,
            startCoordHeight = perlin.startCoordHeight,

            polygonScale = polygonScale,

            oneWithDivider = new NativeArray<float>(perlinData.oneWithDivider, Allocator.Persistent),
            centersWithDivider = new NativeArray<float>(perlinData.centersWithDivider, Allocator.Persistent),
            dividers = new NativeArray<ushort>(perlinData.dividers, Allocator.Persistent),

            TrianglesForNormals = new NativeArray<uint>(NormalsLodTriangles, Allocator.Persistent),
            triangles = new NativeArray<uint>(LodTriangles, Allocator.Persistent),

            LodChunkPos = new NativeArray<Vector2Int>(chunkPos.ToArray(), Allocator.Persistent),
            IndexChunks = new NativeArray<int>(chunkIndex.ToArray(), Allocator.Persistent),

            verticies = new NativeArray<float3>(chunkIndex.Count * (CountOriginalChunksInLengthBatch * LodDensity + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1), Allocator.Persistent),
            normals = new NativeArray<float3>(chunkIndex.Count * (CountOriginalChunksInLengthBatch * LodDensity + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1), Allocator.Persistent),
            uvs = new NativeArray<float2>(chunkIndex.Count * (CountOriginalChunksInLengthBatch * LodDensity + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1), Allocator.Persistent),

            Links = perlinData.NativeLinks,
            SettingsOctaves = perlinData.NativeSettingsOctaves,
            SettingsOctavesSlices = perlinData.NativeSettingsOctaveSlices,
            RiversData = perlinData.NativeRiversData,
            RiversSlices = perlinData.NativeRiversSlices,
            ReliefTypes = perlinData.NativeReliefTypes,            
        };
        ChunkGenerateJobQueue.Enqueue(parallelJob);
        var jobHandle = parallelJob.ScheduleParallel(chunkPos.Count, 8, default);
        
        ChunkGenerateQueue.Enqueue(jobHandle);
    }

    private LodChunkData InitChunk(LodChunkData chunk)
    {
        LodChunks.Add(chunk);
        ChunksToMove.Add(chunk.index);

        int CountVerticies = (CountOriginalChunksInLengthBatch * LodDensity + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1);

        chunk.mesh.SetVertexBufferParams(CountVerticies,
        new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, stream: 0),
        new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, stream: 1),
        new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, stream: 2));

        chunk.mesh.SetIndexBufferParams(LodTriangles.Length, IndexFormat.UInt32);

        var size = new Vector3(ConvertBatchPosInWorldPos, perlinData.maxHeight * 1.5f, ConvertBatchPosInWorldPos);
        chunk.mesh.bounds = new Bounds(new Vector3(0, size.y * 0.5f, 0), size);

        chunk.mesh.subMeshCount = 1;
        chunk.mesh.SetSubMesh(0, new SubMeshDescriptor(0, LodTriangles.Length)
        {
            vertexCount = CountVerticies,
        }, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);

        return chunk;
    }

    private IEnumerator ChunkGenerate()
    {
        var chunkPos = new List<Vector2Int>(CountBatches);
        var chunkIndex = new List<int>(CountBatches);

        int X;
        int Z;
        int dist;
        Vector2Int localPos;
        for (int i = 0; i < 4 * sqrViewRadiusBatches; i++)
        {
            X = i % (2 * RadiusLodBatches) + currentPlayerChunk.x - RadiusLodBatches;
            Z = i / (2 * RadiusLodBatches) + currentPlayerChunk.y - RadiusLodBatches;

            localPos = new Vector2Int(X, Z) - currentPlayerChunk;
            if ((localPos.x == 0 && Mathf.Abs(localPos.y) == RadiusLodBatches) || (localPos.y == 0 && Mathf.Abs(localPos.x) == RadiusLodBatches)) continue;

            dist = localPos.sqrMagnitude;
            if (dist > sqrViewRadiusBatchesInequality) continue;

            if (ChunksPos.Contains(new Vector2Int(X, Z)) || ChunksToMove.Count == 0) continue;

            LodChunkData chunkData = LodChunks[ChunksToMove[ChunksToMove.Count - 1]];
            ChunksPos.Remove(LodChunks[chunkData.index].LodChunkPos);
            chunkData.LodChunkPos = new Vector2Int(X, Z);
            
            chunkIndex.Add(chunkData.index);

            SetDataChunkForMeshGenerate(chunkData, chunkPos);
            if (CoroutineTimeManager.GetSuspensionMoment()) yield return null;
        }

        var parallelJob = new MeshChunkBuilder.GenerateLodMeshParallel()
        {
            seed = seed,

            OriginalSizeChunk = perlinData.sizeChunk,
            CountOriginalChunksInLengthBatch = CountOriginalChunksInLengthBatch,

            maxHeight = perlinData.maxHeight,

            PerlinCountChunks = perlinData.countChunks,
            settingSideProportion = perlinData.settingSideProportion,
            lengthRowOct = perlinData.LengthRowOctaves,
            oneSetting = perlinData.oneSetting,
            SettingsSize = perlinData.settingsSize,

            CountVerticiesInLodChunk = CountOriginalChunksInLengthBatch * LodDensity + 1,
            SqrCountVerticiesInLodChunk = (CountOriginalChunksInLengthBatch * LodDensity + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1),
            LodDensity = LodDensity,

            SideSize = perlinData.SideSize,
            waterLevel = perlinData.waterLevel,
            countElementsRiver = perlinData.countElementsRiver,
            startCoordHeight = perlin.startCoordHeight,

            polygonScale = polygonScale,

            oneWithDivider = new NativeArray<float>(perlinData.oneWithDivider, Allocator.Persistent),
            centersWithDivider = new NativeArray<float>(perlinData.centersWithDivider, Allocator.Persistent),
            dividers = new NativeArray<ushort>(perlinData.dividers, Allocator.Persistent),

            TrianglesForNormals = new NativeArray<uint>(NormalsLodTriangles, Allocator.Persistent),
            triangles = new NativeArray<uint>(LodTriangles, Allocator.Persistent),

            LodChunkPos = new NativeArray<Vector2Int>(chunkPos.ToArray(), Allocator.Persistent),
            IndexChunks = new NativeArray<int>(chunkIndex.ToArray(), Allocator.Persistent),

            verticies = new NativeArray<float3>(chunkIndex.Count * (CountOriginalChunksInLengthBatch * LodDensity + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1), Allocator.Persistent),
            normals = new NativeArray<float3>(chunkIndex.Count * (CountOriginalChunksInLengthBatch * LodDensity + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1), Allocator.Persistent),
            uvs = new NativeArray<float2>(chunkIndex.Count * (CountOriginalChunksInLengthBatch * LodDensity + 1) * (CountOriginalChunksInLengthBatch * LodDensity + 1), Allocator.Persistent),

            Links = perlinData.NativeLinks,
            SettingsOctaves = perlinData.NativeSettingsOctaves,
            SettingsOctavesSlices = perlinData.NativeSettingsOctaveSlices,
            RiversData = perlinData.NativeRiversData,
            RiversSlices = perlinData.NativeRiversSlices,
            ReliefTypes = perlinData.NativeReliefTypes,            
        };
        ChunkGenerateJobQueue.Enqueue(parallelJob); 
        var jobHandle = parallelJob.ScheduleParallel(chunkPos.Count, 8, default);
        
        ChunkGenerateQueue.Enqueue(jobHandle);
    }

    [HideInInspector] public MeshChunkBuilder.GenerateLodMeshParallel CurrentJob;

    private void SetDataChunkForMeshGenerate(LodChunkData chunkData, List<Vector2Int> chunkPos)
    {
        var pos = chunkData.LodChunkPos;

        ChunksPos.Add(pos);
        chunkPos.Add(pos);

        chunkData.state = LodChunkState.NotToMove;        
        ChunksToMove.RemoveAt(ChunksToMove.Count - 1);
    }
       
    [SerializeField] private GameObject player;
    private Vector2 playerChunk;
    [HideInInspector] public bool isCheckSetGenerate;    

    public float ConvertBatchPosInWorldPos { private set; get; }
    [SerializeField] private Material ChunkMaterial;
    [SerializeField] private int LodChunkLayer;

    private float SqrDistToDraw;
    private void LateUpdate()
    {
        if (isCheckSetGenerate)
        {           
            if (LodChunks != null && LodChunks.Count > 0)
            {
                for (int i = 0; i < LodChunks.Count; i++)
                {
                    SqrDistToDraw = (LodChunks[i].LodChunkPos - currentPlayerChunk).sqrMagnitude;
                    if (LodChunks[i].mesh == null || SqrDistToDraw < SqrDistToDontDraw) continue;
                    Graphics.DrawMesh(LodChunks[i].mesh, Matrix4x4.TRS(LodChunks[i].WorldPos, Quaternion.identity, Vector3.one), ChunkMaterial, LodChunkLayer);
                }
            }            
        }
    }
    
    [HideInInspector] public bool IsBatchesGenerated;
    public IEnumerator SetBatchesMesh()
    {
        IsBatchesGenerated = false;
        var handle = ChunkGenerateQueue.Dequeue();

        handle.Complete();

        var parallelJob = ChunkGenerateJobQueue.Dequeue();
        parallelJob.DisposeTempData();
        CurrentJob = parallelJob;
        int indexChunk;
        for (int i = 0; i < parallelJob.LodChunkPos.Length; i++)
        {
            indexChunk = parallelJob.IndexChunks[i];
            LodChunks[indexChunk].WorldPos = new Vector3(parallelJob.LodChunkPos[i].x * ConvertBatchPosInWorldPos, 0, parallelJob.LodChunkPos[i].y * ConvertBatchPosInWorldPos);
            LodChunks[indexChunk].state = LodChunkState.NotToMove;

            LodChunks[indexChunk].mesh.SetVertexBufferData<float3>(parallelJob.verticies, i * parallelJob.SqrCountVerticiesInLodChunk, 0, parallelJob.SqrCountVerticiesInLodChunk, 0, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);
            LodChunks[indexChunk].mesh.SetVertexBufferData<float3>(parallelJob.normals, i * parallelJob.SqrCountVerticiesInLodChunk, 0, parallelJob.SqrCountVerticiesInLodChunk, 1, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);
            LodChunks[indexChunk].mesh.SetVertexBufferData<float2>(parallelJob.uvs, i * parallelJob.SqrCountVerticiesInLodChunk, 0, parallelJob.SqrCountVerticiesInLodChunk, 2, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);
            
            LodChunks[indexChunk].mesh.SetIndexBufferData<uint>(LodTriangles, 0, 0, LodTriangles.Length, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);

            if (CoroutineTimeManager.GetSuspensionMoment()) yield return null;
        }
        parallelJob.DisposeMainData();
        CurrentJob = parallelJob;

        IsBatchesGenerated = true;
    }

    public IEnumerator AddChunksToMove()
    {
        Vector2Int localPos;
        int dist;
        for (int i = 0; i < LodChunks.Count; i++)
        {
            var chunk = LodChunks[i];

            localPos = chunk.LodChunkPos - currentPlayerChunk;            

            dist = localPos.sqrMagnitude;
            // Проверяем квадрат расстояния для удаления чанкa
            if (chunk.state != LodChunkState.ToMove && (dist > sqrViewRadiusBatchesInequality || (localPos.x == 0 && Mathf.Abs(localPos.y) == RadiusLodBatches) || (localPos.y == 0 && Mathf.Abs(localPos.x) == RadiusLodBatches)))
            {
                ChunksToMove.Add(chunk.index);                
                chunk.state = LodChunkState.ToMove;                
            }
            if (CoroutineTimeManager.GetSuspensionMoment()) yield return null;
        }
        
        StartCoroutine(ChunkGenerate());
    }

    void OnApplicationQuit()
    {
        if (ChunkGenerateQueue != null)
            while (ChunkGenerateQueue.Count > 0)
            {
                ChunkGenerateQueue.Dequeue().Complete();
            }

        if (ChunkGenerateJobQueue != null)
            while (ChunkGenerateJobQueue.Count > 0)
            {
                var job = ChunkGenerateJobQueue.Dequeue();
                job.DisposeTempData();
                job.DisposeMainData();                
            }
    }
}
