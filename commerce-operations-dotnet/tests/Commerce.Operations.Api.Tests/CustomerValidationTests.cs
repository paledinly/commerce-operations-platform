using Commerce.Operations.Api.Customers;
using Xunit;

namespace Commerce.Operations.Api.Tests;

public sealed class CustomerValidationTests
{
    [Fact]
    public void Valid_customer_is_accepted() => Assert.True(new CreateCustomerRequestValidator().Validate(new CreateCustomerRequest("User@Example.com", "테스트 회원", "010-1234-5678")).IsValid);

    [Theory]
    [InlineData("invalid", "회원", "01012345678")]
    [InlineData("user@example.com", "", "01012345678")]
    [InlineData("user@example.com", "회원", "123")]
    public void Invalid_customer_is_rejected(string email, string name, string phone) => Assert.False(new CreateCustomerRequestValidator().Validate(new CreateCustomerRequest(email, name, phone)).IsValid);

    [Fact]
    public void Email_and_phone_are_normalized()
    {
        Assert.Equal("user@example.com", CustomerNormalization.Email(" User@Example.COM "));
        Assert.Equal("+821012345678", CustomerNormalization.Phone("+82 10-1234-5678"));
    }
}
