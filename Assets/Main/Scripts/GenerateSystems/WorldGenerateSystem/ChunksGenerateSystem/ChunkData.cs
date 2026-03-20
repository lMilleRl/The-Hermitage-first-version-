using UnityEngine;

public class ChunkData
{
    public Vector2Int pos;
    public ChunkDataState state;

    public GameObject chunk;
    public ChunkRenderer rend;
    public int index;

    public ChunkData(Vector2Int pos, ChunkDataState state, GameObject chunk, int index)
    {
        this.index = index;
        this.pos = pos;
        this.state = state;
        this.chunk = chunk;        
        if (chunk != null) rend = chunk.GetComponent<ChunkRenderer>();
        else rend = null;
    }
}

public enum ChunkDataState
{
    StartingLoading,
    Loaded,
    Unload,
    ToMove,
    SpawnedInWorld    
}
