using UnityEngine.Rendering;
using UnityEngine;

public class ChunkRenderer : MonoBehaviour
{
    [SerializeField] private MeshCollider meshColl;
    [HideInInspector] public Mesh chunkMesh;    
    public void SetActiveMesh(bool isActive)
    {
        meshColl.enabled = isActive;        
    }

    public void UpdateMesh()
    {
        meshColl.sharedMesh = chunkMesh;        
    }

    public Mesh SetStartMeshData(int VerticiesLength, int TrianglesLength, Vector3 bounds)
    {
        chunkMesh = new Mesh();                

        chunkMesh.SetVertexBufferParams(VerticiesLength, new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, stream: 0),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, stream: 1),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, stream: 2));

        chunkMesh.SetIndexBufferParams(TrianglesLength, IndexFormat.UInt32);

        chunkMesh.bounds = new Bounds(new Vector3(0, bounds.y * 0.5f, 0), bounds);

        chunkMesh.subMeshCount = 1;
        chunkMesh.SetSubMesh(0, new SubMeshDescriptor(0, TrianglesLength)
        {
            vertexCount = VerticiesLength,           
        }, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);        

        return chunkMesh;
    }    
}
