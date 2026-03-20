using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class MeshChunkBuilder
{    
    [BurstCompile]
    public struct GenerateLodMeshParallel : IJobFor
    {
        public int seed;

        public float maxHeight;
        
        public int CountOriginalChunksInLengthBatch;
        public byte OriginalSizeChunk;
        public int LodDensity;

        public int CountVerticiesInLodChunk;
        public int SqrCountVerticiesInLodChunk;

        public ushort SideSize;
        public int SettingsSize;
        public ushort PerlinCountChunks;

        public byte settingSideProportion;
        public float oneSetting;
        public int lengthRowOct;

        public float waterLevel;
        public int countElementsRiver;

        public float startCoordHeight;

        public float polygonScale;

        [NativeDisableParallelForRestriction][ReadOnly] public NativeArray<float> oneWithDivider;
        [NativeDisableParallelForRestriction][ReadOnly] public NativeArray<float> centersWithDivider;
        [NativeDisableParallelForRestriction][ReadOnly] public NativeArray<ushort> dividers;

        [NativeDisableParallelForRestriction][ReadOnly] public NativeArray<float> Links;

        [NativeDisableParallelForRestriction][ReadOnly] public NativeArray<byte> SettingsOctaves;
        [NativeDisableParallelForRestriction][ReadOnly] public NativeArray<int2> SettingsOctavesSlices;

        [NativeDisableParallelForRestriction][ReadOnly] public NativeArray<float> RiversData;
        [NativeDisableParallelForRestriction][ReadOnly] public NativeParallelHashMap<int2, int2> RiversSlices;
        [NativeDisableParallelForRestriction][ReadOnly] public NativeArray<int> ReliefTypes;

        [NativeDisableParallelForRestriction][ReadOnly] public NativeArray<uint> triangles;
        [NativeDisableParallelForRestriction][ReadOnly] public NativeArray<uint> TrianglesForNormals;        

        [NativeDisableParallelForRestriction][ReadOnly] public NativeArray<Vector2Int> LodChunkPos;
        [NativeDisableParallelForRestriction][ReadOnly] public NativeArray<int> IndexChunks;

        [NativeDisableParallelForRestriction][WriteOnly] public NativeArray<float3> verticies;
        [NativeDisableParallelForRestriction][WriteOnly] public NativeArray<float3> normals;
        [NativeDisableParallelForRestriction][WriteOnly] public NativeArray<float2> uvs;
                
        public void Execute(int startIndex)
        {
            GenerateMesh(startIndex);
        }

        private void GenerateMesh(int chunkIndex)
        {
            int CountVetriciesNow = CountVerticiesInLodChunk + 2;
            int SqrCountVerticiesNow = CountVetriciesNow * CountVetriciesNow;

            var verticies = new NativeArray<float3>(SqrCountVerticiesNow, Allocator.Temp);
            float x; float z; float y; float ChunkX = (LodChunkPos[chunkIndex].x - 0.5f) * CountOriginalChunksInLengthBatch - 1 / (float)LodDensity; float ChunkY = (LodChunkPos[chunkIndex].y - 0.5f) * CountOriginalChunksInLengthBatch - 1 / (float)LodDensity;
            float step = (OriginalSizeChunk - 1) / (float)LodDensity;
            float offset = CountOriginalChunksInLengthBatch * (OriginalSizeChunk - 1) * 0.5f;

            float WorldX, WorldY;
            var Rand = new Unity.Mathematics.Random();
            var UVs = new NativeArray<float2>(SqrCountVerticiesNow, Allocator.Temp);
            for (int i = 0; i < SqrCountVerticiesNow; i++)
            {
                WorldX = ChunkX * (OriginalSizeChunk - 1) + i % CountVetriciesNow * step;
                WorldY = ChunkY * (OriginalSizeChunk - 1) + i / CountVetriciesNow * step;
                y = PerlinNoise.GetPointHeightNative(WorldX, WorldY, settingSideProportion, oneSetting, OriginalSizeChunk, PerlinCountChunks, SideSize, waterLevel,
                countElementsRiver, oneWithDivider, centersWithDivider, dividers, Links, SettingsOctaves, SettingsOctavesSlices, SettingsSize, RiversData, RiversSlices, ReliefTypes, seed, maxHeight, startCoordHeight);
                x = i % CountVetriciesNow * step - step - offset;
                z = i / CountVetriciesNow * step - step - offset;
                verticies[i] = new float3(x * polygonScale, y, z * polygonScale);

                Rand.state = math.hash(new float3(seed, WorldX, WorldY));
                float RandEdge = Rand.NextFloat(-10, 10) * math.sin(y);
                if (y <= waterLevel * maxHeight + 10) UVs[i] = new float2(0.25f, 0.75f);
                else if (y >= waterLevel * maxHeight + 10 && y <= 600 + RandEdge) UVs[i] = new float2(0.25f, 0.25f);
                else if (y >= 600 + RandEdge && y <= 1550 + RandEdge) UVs[i] = new float2(0.75f, 0.75f);
                else if (y >= 1550 + RandEdge) UVs[i] = new float2(0.75f, 0.25f);
            }
            
            var normals = CalculateNormal(verticies);            
           
            int index = 0;
            int startIndex = chunkIndex * SqrCountVerticiesInLodChunk;
            for (int i = 0; i < SqrCountVerticiesNow; i++)
            {
                if (i % CountVetriciesNow == 0 || i % CountVetriciesNow == (CountVetriciesNow - 1) || i / CountVetriciesNow == 0 || i / CountVetriciesNow == (CountVetriciesNow - 1)) continue;

                this.verticies[startIndex + index] = verticies[i];
                this.normals[startIndex + index] = normals[i];
                this.uvs[startIndex + index] = UVs[i];
                index++;
            }            
        }

        private NativeArray<float3> CalculateNormal(NativeArray<float3> verticies)
        {
            int triangleCount = TrianglesForNormals.Length / 3;

            NativeArray<float3> CurrentNormals = new NativeArray<float3>(verticies.Length, Allocator.Temp);

            for (int i = 0; i < triangleCount; i++)
            {
                int index0 = (int)TrianglesForNormals[i * 3];
                int index1 = (int)TrianglesForNormals[i * 3 + 1];
                int index2 = (int)TrianglesForNormals[i * 3 + 2];

                float3 v0 = verticies[index0];
                float3 v1 = verticies[index1];
                float3 v2 = verticies[index2];

                float3 normal = math.normalize(math.cross(v1 - v0, v2 - v0));

                CurrentNormals[index0] += normal;
                CurrentNormals[index1] += normal;
                CurrentNormals[index2] += normal;
            }

            for (int i = 0; i < CurrentNormals.Length; i++)
            {
                CurrentNormals[i] = math.normalize(CurrentNormals[i]);
            }

            return CurrentNormals;
        }

        public void DisposeTempData()
        {
            if (oneWithDivider.IsCreated) oneWithDivider.Dispose(); if (centersWithDivider.IsCreated) centersWithDivider.Dispose(); if (dividers.IsCreated) dividers.Dispose();
            if (triangles.IsCreated) triangles.Dispose(); if (TrianglesForNormals.IsCreated) TrianglesForNormals.Dispose();
        }
                
        public void DisposeMainData()
        {
            if (LodChunkPos.IsCreated) LodChunkPos.Dispose(); if (IndexChunks.IsCreated) IndexChunks.Dispose();
            if (verticies.IsCreated) verticies.Dispose(); if (normals.IsCreated) normals.Dispose();
            if (uvs.IsCreated) uvs.Dispose();            
        }
    }
}
