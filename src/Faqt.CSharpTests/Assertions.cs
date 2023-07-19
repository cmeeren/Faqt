using Faqt;
using Xunit;
using static TestUtils;

#pragma warning disable CS1591


public class Assertions
{
    [Fact]
    public void Be()
    {
        AssertExnMsg(() =>
        {
            var x = 1;
            x.Should().Be(2);
        }, """
x
    should be
2
    but was
1
""");
    }


    [Fact]
    public void Be_Because()
    {
        AssertExnMsg(() =>
        {
            var x = 1;
            x.Should().Be(2, "some reason");
        }, """
x
    should be
2
    because some reason, but was
1
""");
    }


    [Fact]
    public void NotBe()
    {
        AssertExnMsg(() =>
        {
            var x = 1;
            x.Should().NotBe(x);
        }, """
x
    should not be
1
    but the values were equal.
""");
    }


    [Fact]
    public void NotBe_Because()
    {
        AssertExnMsg(() =>
        {
            var x = 1;
            x.Should().NotBe(x, "some reason");
        }, """
x
    should not be
1
    because some reason, but the values were equal.
""");
    }


    [Fact]
    public void BeNull()
    {
        AssertExnMsg(() =>
        {
            var x = "asd";
            x.Should().BeNull();
        }, """
x
    should be null, but was
"asd"
""");
    }


    [Fact]
    public void BeNull_Reason()
    {
        AssertExnMsg(() =>
        {
            var x = "asd";
            x.Should().BeNull("some reason");
        }, """
x
    should be null because some reason, but was
"asd"
""");
    }


    [Fact]
    public void NotBeNull()
    {
        AssertExnMsg(() =>
        {
            string? x = null;
            x.Should().NotBeNull();
        }, """
x
    should not be null, but was null.
""");
    }


    [Fact]
    public void NotBeNull_Reason()
    {
        AssertExnMsg(() =>
        {
            string? x = null;
            x.Should().NotBeNull("some reason");
        }, """
x
    should not be null because some reason, but was null.
""");
    }
}
