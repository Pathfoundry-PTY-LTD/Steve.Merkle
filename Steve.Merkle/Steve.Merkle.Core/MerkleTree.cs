namespace Steve.Merkle.Core;

/// <summary>
/// Represents the Merkle Tree, providing functionality to manage the tree and compute hashes.
/// </summary>
/// <typeparam name="T">The type of data stored in the tree's leaf nodes.</typeparam>
public class MerkleTree<T> : IMerkleTree<T>
{
    /// <summary>
    /// The root node of the Merkle Tree. The root hash represents the entire state of the tree.
    /// </summary>
    public IMerkleNode<T> Root { get; private set; }

    private readonly List<MerkleNode<T>> _leafNodes;
    private readonly Func<T, string> _hashFunction;
    private readonly object _lock = new object();
    private string _rootHash;

    /// <summary>
    /// Initializes a new instance of the MerkleTree class with the specified hash function.
    /// </summary>
    /// <param name="hashFunction">A function that computes the SHA-256 hash of the data.</param>
    public MerkleTree(Func<T, string> hashFunction)
    {
        _leafNodes = new List<MerkleNode<T>>();
        _hashFunction = hashFunction ?? throw new ArgumentNullException(nameof(hashFunction));
    }

    /// <summary>
    /// Adds a new piece of data to the Merkle Tree as a leaf node.
    /// </summary>
    /// <param name="data">The data to add.</param>
    public void AddData(T data)
    {
        lock (_lock)
        {
            var leafNode = new MerkleNode<T>(data, _hashFunction);
            _leafNodes.Add(leafNode);
            RebuildTree();
        }
    }

    /// <summary>
    /// Removes a piece of data from the Merkle Tree.
    /// </summary>
    /// <param name="data">The data to remove.</param>
    public void RemoveData(T data)
    {
        lock (_lock)
        {
            var leafNode = _leafNodes.FirstOrDefault(node => EqualityComparer<T>.Default.Equals(node.Data, data));
            if (leafNode != null)
            {
                _leafNodes.Remove(leafNode);
                RebuildTree();
            }
        }
    }

    /// <summary>
    /// Updates an existing piece of data in the Merkle Tree.
    /// </summary>
    /// <param name="oldData">The current data to replace.</param>
    /// <param name="newData">The new data to insert.</param>
    public void UpdateData(T oldData, T newData)
    {
        lock (_lock)
        {
            var leafNode = _leafNodes.FirstOrDefault(node => EqualityComparer<T>.Default.Equals(node.Data, oldData));
            if (leafNode != null)
            {
                leafNode.Data = newData;
                leafNode.ComputeHash();

                var currentNode = leafNode.Parent;
                while (currentNode != null)
                {
                    currentNode.ComputeHash();
                    currentNode = currentNode.Parent;
                }

                _rootHash = Root?.Hash;
            }
        }
    }

    /// <summary>
    /// Computes and returns the root hash of the Merkle Tree.
    /// </summary>
    /// <returns>The root hash as a hexadecimal string.</returns>
    public string ComputeRootHash()
    {
        lock (_lock)
        {
            if (Root != null)
            {
                return Root.Hash;
            }
            else
            {
                return string.Empty;
            }
        }
    }

    /// <summary>
    /// Verifies the integrity of the Merkle Tree by comparing the current root hash to the stored root hash.
    /// </summary>
    /// <returns>True if the tree is intact; otherwise, false.</returns>
    public bool VerifyIntegrity()
    {
        lock (_lock)
        {
            var currentRootHash = ComputeRootHash();
            return currentRootHash == _rootHash;
        }
    }

    /// <summary>
    /// Traverses the Merkle Tree and applies the specified action to each node.
    /// </summary>
    /// <param name="nodeAction">The action to apply to each node.</param>
    public void TraverseTree(Action<IMerkleNode<T>> nodeAction)
    {
        if (nodeAction == null)
            throw new ArgumentNullException(nameof(nodeAction));

        lock (_lock)
        {
            TraverseNode(Root, nodeAction);
        }
    }

    /// <summary>
    /// Recursively traverses the tree in a depth-first manner.
    /// </summary>
    /// <param name="node">The current node being traversed.</param>
    /// <param name="nodeAction">The action to apply to each node.</param>
    private void TraverseNode(IMerkleNode<T> node, Action<IMerkleNode<T>> nodeAction)
    {
        if (node == null)
            return;

        nodeAction(node);

        if (!node.IsLeaf)
        {
            TraverseNode(node.Left, nodeAction);
            TraverseNode(node.Right, nodeAction);
        }
    }

    /// <summary>
    /// Rebuilds the Merkle Tree from the current list of leaf nodes.
    /// </summary>
    private void RebuildTree()
    {
        if (_leafNodes.Count == 0)
        {
            Root = null;
            _rootHash = null;
            return;
        }

        var nodes = new List<MerkleNode<T>>(_leafNodes);

        while (nodes.Count > 1)
        {
            var parentNodes = new List<MerkleNode<T>>();

            for (int i = 0; i < nodes.Count; i += 2)
            {
                MerkleNode<T> left = nodes[i];
                MerkleNode<T> right = (i + 1 < nodes.Count) ? nodes[i + 1] : nodes[i]; // Duplicate last node if odd number

                var parentNode = new MerkleNode<T>(left, right);
                parentNodes.Add(parentNode);
            }

            nodes = parentNodes;
        }

        Root = nodes[0];
        _rootHash = Root.Hash;
    }
}