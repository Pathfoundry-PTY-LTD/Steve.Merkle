namespace Steve.Merkle.Core;

public interface IMerkleTree<T>
{
    /// <summary>
    /// The root node of the Merkle Tree. The root hash represents the entire state of the tree.
    /// </summary>
    IMerkleNode<T> Root { get; }

    /// <summary>
    /// Adds a new piece of data to the Merkle Tree as a leaf node.
    /// </summary>
    /// <param name="data">The data to add.</param>
    void AddData(T data);

    /// <summary>
    /// Removes a piece of data from the Merkle Tree.
    /// </summary>
    /// <param name="data">The data to remove.</param>
    void RemoveData(T data);

    /// <summary>
    /// Updates an existing piece of data in the Merkle Tree.
    /// </summary>
    /// <param name="oldData">The current data to replace.</param>
    /// <param name="newData">The new data to insert.</param>
    void UpdateData(T oldData, T newData);

    /// <summary>
    /// Computes and returns the root hash of the Merkle Tree.
    /// </summary>
    /// <returns>The root hash as a hexadecimal string.</returns>
    string ComputeRootHash();

    /// <summary>
    /// Verifies the integrity of the Merkle Tree by comparing the current root hash to the stored root hash.
    /// </summary>
    /// <returns>True if the tree is intact; otherwise, false.</returns>
    bool VerifyIntegrity();

    /// <summary>
    /// Traverses the Merkle Tree and applies the specified action to each node.
    /// </summary>
    /// <param name="nodeAction">The action to apply to each node.</param>
    void TraverseTree(Action<IMerkleNode<T>> nodeAction);
}