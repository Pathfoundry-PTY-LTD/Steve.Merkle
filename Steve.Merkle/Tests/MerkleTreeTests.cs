using System.Security.Cryptography;
using System.Text;
using Steve.Merkle.Core;

namespace Tests;

public class MerkleTreeTests
{
    [Fact(DisplayName = "Test adding a data item to the Merkle tree and ensuring that it is correctly added and that the root hash is updated.")]
    public void AddData_ShouldAddDataAndUpdateRootHash()
    {
        // Arrange
        var tree = new MerkleTree<string>(ComputeHash);

        // Act
        tree.AddData("data1");
        var rootHash = tree.ComputeRootHash();

        // Assert
        Assert.NotNull(rootHash);
        Assert.Equal(tree.Root.Hash, rootHash);
        Assert.True(tree.VerifyIntegrity());
    }

    [Fact(DisplayName = "Test removing a data item from the Merkle tree and verifying that the root hash is correctly updated.")]
    public void RemoveData_ShouldRemoveDataAndUpdateRootHash()
    {
        // Arrange
        var tree = new MerkleTree<string>(ComputeHash);
        tree.AddData("data1");
        tree.AddData("data2");
        var initialRootHash = tree.ComputeRootHash();

        // Act
        tree.RemoveData("data1");
        var newRootHash = tree.ComputeRootHash();

        // Assert
        Assert.NotEqual(initialRootHash, newRootHash);
        Assert.True(tree.VerifyIntegrity());
    }

    [Fact(DisplayName = "Test updating an existing data item and confirming that only the necessary nodes' hashes are recalculated, and the root hash is updated.")]
    public void UpdateData_ShouldUpdateDataAndRecalculateHashes()
    {
        // Arrange
        var tree = new MerkleTree<string>(ComputeHash);
        tree.AddData("data1");
        tree.AddData("data2");
        var initialRootHash = tree.ComputeRootHash();

        // Act
        tree.UpdateData("data1", "data1_updated");
        var newRootHash = tree.ComputeRootHash();

        // Assert
        Assert.NotEqual(initialRootHash, newRootHash);
        Assert.True(tree.VerifyIntegrity());
    }

    [Fact(DisplayName = "Test computing the root hash after multiple additions and deletions and ensuring it reflects the correct state of the tree.")]
    public void ComputeRootHash_ShouldReflectCorrectTreeState()
    {
        // Arrange
        var tree = new MerkleTree<string>(ComputeHash);
        tree.AddData("data1");
        tree.AddData("data2");
        var rootHash1 = tree.ComputeRootHash();

        // Act
        tree.AddData("data3");
        var rootHash2 = tree.ComputeRootHash();

        tree.RemoveData("data2");
        var rootHash3 = tree.ComputeRootHash();

        // Assert
        Assert.NotEqual(rootHash1, rootHash2);
        Assert.NotEqual(rootHash2, rootHash3);
        Assert.True(tree.VerifyIntegrity());
    }

    [Fact(DisplayName = "Test verifying the integrity of the tree by comparing the current root hash to a known correct hash.")]
    public void VerifyIntegrity_ShouldReturnTrueIfTreeIsUnchanged()
    {
        // Arrange
        var tree = new MerkleTree<string>(ComputeHash);
        tree.AddData("data1");
        tree.AddData("data2");
        var knownRootHash = tree.ComputeRootHash();

        // Act
        var integrity = tree.VerifyIntegrity();

        // Assert
        Assert.True(integrity);
        Assert.Equal(knownRootHash, tree.ComputeRootHash());
    }

    [Fact(DisplayName = "Test adding duplicate data items to the tree and ensuring that the tree handles it appropriately.")]
    public void AddData_DuplicateData_ShouldHandleDuplicates()
    {
        // Arrange
        var tree = new MerkleTree<string>(ComputeHash);
        tree.AddData("data1");
        tree.AddData("data1"); // duplicate
        var rootHash = tree.ComputeRootHash();

        // Act
        tree.AddData("data1"); // another duplicate
        var newRootHash = tree.ComputeRootHash();

        // Assert
        Assert.NotEqual(rootHash, newRootHash);
        Assert.True(tree.VerifyIntegrity());
    }

    [Fact(DisplayName = "Test removing a data item that does not exist in the tree and ensure the system handles it without crashing or affecting the tree’s integrity.")]
    public void RemoveData_NonExistentData_ShouldHandleGracefully()
    {
        // Arrange
        var tree = new MerkleTree<string>(ComputeHash);
        tree.AddData("data1");
        var initialRootHash = tree.ComputeRootHash();

        // Act
        tree.RemoveData("data2"); // data2 does not exist
        var newRootHash = tree.ComputeRootHash();

        // Assert
        Assert.Equal(initialRootHash, newRootHash);
        Assert.True(tree.VerifyIntegrity());
    }

    [Fact(DisplayName = "Test updating a data item that does not exist in the tree and verify that the system handles it gracefully.")]
    public void UpdateData_NonExistentData_ShouldHandleGracefully()
    {
        // Arrange
        var tree = new MerkleTree<string>(ComputeHash);
        tree.AddData("data1");
        var initialRootHash = tree.ComputeRootHash();

        // Act
        tree.UpdateData("data2", "data2_updated"); // data2 does not exist
        var newRootHash = tree.ComputeRootHash();

        // Assert
        Assert.Equal(initialRootHash, newRootHash);
        Assert.True(tree.VerifyIntegrity());
    }

    [Fact(DisplayName = "Test what happens if there is a failure to compute the hash (e.g., in case of an I/O exception or invalid data).")]
    public void ComputeHash_Failure_ShouldHandleException()
    {
        // Arrange
        var tree = new MerkleTree<string>(ComputeHashWithException);
        tree.AddData("data1");
        var initialRootHash = tree.ComputeRootHash();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => tree.AddData("data2"));
    }

    [Fact(DisplayName = "Test tree traversal with an empty tree and ensure it doesn’t cause any errors or unexpected behavior.")]
    public void TraverseTree_EmptyTree_ShouldNotThrowException()
    {
        // Arrange
        var tree = new MerkleTree<string>(ComputeHash);

        // Act & Assert
        var exception = Record.Exception(() => tree.TraverseTree(node => { /* do nothing */ }));
        Assert.Null(exception);
    }

    private string ComputeHash(string data)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hashBytes).Replace("-", "");
        }
    }

    private string ComputeHashWithException(string data)
    {
        if (data == "data2")
        {
            throw new InvalidOperationException("Failed to compute hash.");
        }

        return ComputeHash(data);
    }
}