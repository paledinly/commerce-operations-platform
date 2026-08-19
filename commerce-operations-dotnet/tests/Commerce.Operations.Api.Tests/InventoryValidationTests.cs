using Commerce.Operations.Api.Inventories;
using Xunit;

namespace Commerce.Operations.Api.Tests;

public sealed class InventoryValidationTests
{
    [Fact]
    public void Valid_inventory_creation_is_accepted()
    {
        var result = new CreateInventoryRequestValidator().Validate(new CreateInventoryRequest(1, 0));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0, "입고")]
    [InlineData(1, "")]
    public void Invalid_adjustment_is_rejected(long quantityDelta, string reason)
    {
        var result = new AdjustInventoryRequestValidator().Validate(new AdjustInventoryRequest(quantityDelta, reason));

        Assert.False(result.IsValid);
    }
}
