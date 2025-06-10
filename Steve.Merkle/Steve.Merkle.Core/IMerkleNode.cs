using System.Buffers;

namespace Steve.Merkle.Core;

public interface IMerkleNode<T>
{
    T Data { get; }
    string Hash { get; }
    bool IsLeaf { get; }
    IMerkleNode<T> Left { get; }
    IMerkleNode<T> Right { get;}
    IMerkleNode<T> Parent { get; set; }
    void ComputeHash(); 
}