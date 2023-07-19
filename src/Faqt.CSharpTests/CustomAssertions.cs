using Faqt;
using Faqt.AssertionHelpers;
using Xunit;
using static Faqt.Formatting;
using static TestUtils;

#pragma warning disable CS1591


public static class Extensions
{
    public static And<string> NotInvade(this Testable<string> t, string target, string because = "")
    {
        var _ = t.Assert();

        if (t.Subject == "Russia" && target == "Ukraine")
            t.Fail<string, And<string>>(
                "\tExpected\n{subject}\n\tto not invade\n{0}\n\t{because}but an invasion was found to be taking place by\n{actual}",
                because,
                format(target)
            );

        return new And<string>(t);
    }
}


public class CustomAssertions
{
    [Fact]
    public void Be()
    {
        AssertExnMsg(() =>
        {
            var country = "Russia";
            country.Should().NotInvade("Ukraine");
        }, """
    Expected
country
    to not invade
"Ukraine"
    but an invasion was found to be taking place by
"Russia"
""");
    }
}
