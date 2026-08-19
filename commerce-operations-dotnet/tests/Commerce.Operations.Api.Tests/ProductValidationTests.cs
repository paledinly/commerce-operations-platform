using Commerce.Operations.Api.Products;
using Xunit;

namespace Commerce.Operations.Api.Tests;

public sealed class ProductValidationTests
{
    [Fact]
    public void Valid_product_is_accepted()
    {
        var result = new CreateProductRequestValidator().Validate(new CreateProductRequest("SKU-001", "테스트 상품", 1000m));
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "상품", 100)]
    [InlineData("INVALID SKU", "상품", 100)]
    [InlineData("SKU-001", "", 100)]
    [InlineData("SKU-001", "상품", -1)]
    public void Invalid_product_is_rejected(string sku, string name, decimal price)
    {
        Assert.False(new CreateProductRequestValidator().Validate(new CreateProductRequest(sku, name, price)).IsValid);
    }

    [Fact]
    public void Excessive_page_size_is_rejected()
    {
        Assert.False(new ProductQueryValidator().Validate(new ProductQuery(null, null, 1, 101)).IsValid);
    }
}
