using System.Buffers;

namespace Steve.Merkle.Core;

public interface IMerkleNode<T>
{
    T Data { get; }
    string Hash { get; }
    bool IsLeaf { get; }

    void ComputeHash(); // Computes the hash for this node, based on data or child nodes.
}