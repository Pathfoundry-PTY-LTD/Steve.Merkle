namespace Steve.Merkle.Core;

public interface IMerkleTree<T>
{
    IMerkleNode<T> Root { get; }

    void AddData(T data); // Adds a new piece of data to the tree.
    void RemoveData(T data); // Removes a piece of data from the tree.
    void UpdateData(T oldData, T newData); // Updates a node's data in the tree.

    string ComputeRootHash(); // Computes the root hash of the tree.
    bool VerifyIntegrity(); // Verifies the integrity of the tree by recalculating and comparing the root hash.

    void TraverseTree(Action<IMerkleNode<T>> nodeAction); // Traverses the tree and applies an action on each node.
}