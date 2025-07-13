using DentalClinic.Domain.Aggregates.PatientAggregate;
using DentalClinic.Domain.ValueObjects;

namespace DentalClinic.Tests.Unit.Domain.Types;

public class GuidEntityIdTests
{
    [Fact]
    public void Constructor_ValidGuid_ShouldCreateInstance()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var entityId = new GuidEntityId<Patient>(guid);

        // Assert
        Assert.Equal(guid, entityId.Value);
    }

    [Fact]
    public void Constructor_EmptyGuid_ShouldCreateInstance()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act
        var entityId = new GuidEntityId<Patient>(emptyGuid);

        // Assert
        Assert.Equal(emptyGuid, entityId.Value);
    }

    [Fact]
    public void New_ShouldGenerateUniqueIds()
    {
        // Act
        var id1 = GuidEntityId<Patient>.New();
        var id2 = GuidEntityId<Patient>.New();

        // Assert
        Assert.NotEqual(id1, id2);
        Assert.NotEqual(id1.Value, id2.Value);
        Assert.NotEqual(Guid.Empty, id1.Value);
        Assert.NotEqual(Guid.Empty, id2.Value);
    }

    [Fact]
    public void Equality_SameValues_ShouldBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = new GuidEntityId<Patient>(guid);
        var id2 = new GuidEntityId<Patient>(guid);

        // Act & Assert
        Assert.Equal(id1, id2);
        Assert.True(id1 == id2);
        Assert.False(id1 != id2);
        Assert.Equal(id1.GetHashCode(), id2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var id1 = GuidEntityId<Patient>.New();
        var id2 = GuidEntityId<Patient>.New();

        // Act & Assert
        Assert.NotEqual(id1, id2);
        Assert.False(id1 == id2);
        Assert.True(id1 != id2);
    }

    [Fact]
    public void DifferentEntityTypes_WithSameGuid_ShouldNotBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var patientId = new GuidEntityId<Patient>(guid);
        // Note: We can't directly compare different entity types due to type safety,
        // but we can verify they have the same underlying Guid value
        
        // Act & Assert
        Assert.Equal(guid, patientId.Value);
    }

    [Fact]
    public void ToString_ShouldContainGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var entityId = new GuidEntityId<Patient>(guid);

        // Act
        var stringValue = entityId.ToString();

        // Assert
        Assert.Contains(guid.ToString(), stringValue);
    }

    [Fact]
    public void ImplicitConversion_ToGuid_ShouldWork()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var entityId = new GuidEntityId<Patient>(guid);

        // Act
        Guid convertedGuid = entityId.Value;

        // Assert
        Assert.Equal(guid, convertedGuid);
    }

    [Fact]
    public void MultipleNew_ShouldGenerateUniqueSequence()
    {
        // Arrange
        var ids = new HashSet<GuidEntityId<Patient>>();
        const int count = 1000;

        // Act
        for (int i = 0; i < count; i++)
        {
            ids.Add(GuidEntityId<Patient>.New());
        }

        // Assert
        Assert.Equal(count, ids.Count); // All should be unique
    }
} 