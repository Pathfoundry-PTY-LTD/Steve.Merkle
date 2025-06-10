using System.Security.Cryptography;
using System.Text;

namespace Steve.Merkle.Core;

/// <summary>
/// Represents a node in the Merkle Tree, either a leaf or an internal node.
/// </summary>
/// <typeparam name="T">The type of data stored in the node.</typeparam>
public class MerkleNode<T> : IMerkleNode<T>
{
    /// <summary>
    /// The data stored in the node. For leaf nodes, this is the actual data.
    /// For internal nodes, this is typically null or default.
    /// </summary>
    public T Data { get; set; }

    /// <summary>
    /// The SHA-256 hash of the node.
    /// For leaf nodes, this is the hash of the data.
    /// For internal nodes, this is the hash of the concatenated child hashes.
    /// </summary>
    public string Hash { get; private set; }

    /// <summary>
    /// Indicates whether the node is a leaf node.
    /// </summary>
    public bool IsLeaf { get; private set; }

    /// <summary>
    /// Reference to the left child node. Null for leaf nodes.
    /// </summary>
    public IMerkleNode<T> Left { get; private set; }

    /// <summary>
    /// Reference to the right child node. Null for leaf nodes.
    /// </summary>
    public IMerkleNode<T> Right { get; private set; }

    /// <summary>
    /// Reference to the parent node. Null for the root node.
    /// </summary>
    public IMerkleNode<T> Parent { get; set; }

    private readonly Func<T, string> _hashFunction;

    /// <summary>
    /// Constructor for creating a leaf node.
    /// </summary>
    /// <param name="data">The data to store in the leaf node.</param>
    /// <param name="hashFunction">The function to compute the hash of the data.</param>
    public MerkleNode(T data, Func<T, string> hashFunction)
    {
        Data = data;
        _hashFunction = hashFunction;
        IsLeaf = true;
        ComputeHash();
    }

    /// <summary>
    /// Constructor for creating an internal node with two children.
    /// </summary>
    /// <param name="left">The left child node.</param>
    /// <param name="right">The right child node.</param>
    public MerkleNode(IMerkleNode<T> left, IMerkleNode<T> right)
    {
        Left = left;
        Right = right;
        IsLeaf = false;

        Left.Parent = this;
        Right.Parent = this;

        ComputeHash();
    }

    /// <summary>
    /// Computes the hash of the node.
    /// For leaf nodes, it's the hash of the data.
    /// For internal nodes, it's the hash of the concatenated child hashes.
    /// </summary>
    public void ComputeHash()
    {
        if (IsLeaf)
        {
            Hash = _hashFunction(Data);
        }
        else
        {
            Hash = ComputeCombinedHash(Left.Hash, Right.Hash);
        }
    }

    /// <summary>
    /// Computes the SHA-256 hash of the concatenated left and right child hashes.
    /// </summary>
    /// <param name="leftHash">Hash of the left child.</param>
    /// <param name="rightHash">Hash of the right child.</param>
    /// <returns>The combined hash as a hexadecimal string.</returns>
    private static string ComputeCombinedHash(string leftHash, string rightHash)
    {
        using (var sha256 = SHA256.Create())
        {
            var combinedHash = leftHash + rightHash;
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedHash));
            return BitConverter.ToString(hashBytes).Replace("-", "");
        }
    }
}