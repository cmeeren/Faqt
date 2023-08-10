module StringAssertions

open System
open System.Globalization
open System.Text.RegularExpressions
open Faqt
open Xunit


module HaveLength =


    [<Fact>]
    let ``Can be chained with And`` () =
        "a".Should().HaveLength(1).Id<And<string>>().And.Be("a")


    [<Fact>]
    let ``Passes if length = expected`` () = "a".Should().HaveLength(1)


    [<Fact>]
    let ``Fails if length < expected`` () =
        Assert.Throws<AssertionFailedException>(fun () -> "".Should().HaveLength(1) |> ignore)


    [<Fact>]
    let ``Fails if length > expected`` () =
        Assert.Throws<AssertionFailedException>(fun () -> "as".Should().HaveLength(1) |> ignore)


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: string = null
            x.Should().HaveLength(1)
        |> assertExnMsg
            """
x
    should have length
1
    but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if length does not match`` () =
        fun () ->
            let x = ""
            x.Should().HaveLength(1)
        |> assertExnMsg
            """
x
    should have length
1
    but length was
0

""
"""


    [<Fact>]
    let ``Fails with expected message if length does not match with because`` () =
        fun () ->
            let x = ""
            x.Should().HaveLength(1, "some reason")
        |> assertExnMsg
            """
x
    should have length
1
    because some reason, but length was
0

""
"""


module BeEmpty =


    [<Fact>]
    let ``Can be chained with And`` () =
        "".Should().BeEmpty().Id<And<string>>().And.Be("")


    [<Fact>]
    let ``Passes if string is empty`` () = "".Should().BeEmpty()


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: string = null
            x.Should().BeEmpty()
        |> assertExnMsg
            """
x
    should be empty, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: string = null
            x.Should().BeEmpty("some reason")
        |> assertExnMsg
            """
x
    should be empty because some reason, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if not empty`` () =
        fun () ->
            let x = "a"
            x.Should().BeEmpty()
        |> assertExnMsg
            """
x
    should be empty, but was
"a"
"""


    [<Fact>]
    let ``Fails with expected message if not empty with because`` () =
        fun () ->
            let x = "a"
            x.Should().BeEmpty("some reason")
        |> assertExnMsg
            """
x
    should be empty because some reason, but was
"a"
"""


module NotBeEmpty =


    [<Fact>]
    let ``Can be chained with And`` () =
        "a".Should().NotBeEmpty().Id<And<string>>().And.Be("a")


    [<Fact>]
    let ``Passes if string is not empty`` () = "a".Should().NotBeEmpty()


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: string = null
            x.Should().NotBeEmpty()
        |> assertExnMsg
            """
x
    should not be empty, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: string = null
            x.Should().NotBeEmpty("some reason")
        |> assertExnMsg
            """
x
    should not be empty because some reason, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if empty`` () =
        fun () ->
            let x = ""
            x.Should().NotBeEmpty()
        |> assertExnMsg
            """
x
    should not be empty, but was empty.
"""


    [<Fact>]
    let ``Fails with expected message if empty with because`` () =
        fun () ->
            let x = ""
            x.Should().NotBeEmpty("some reason")
        |> assertExnMsg
            """
x
    should not be empty because some reason, but was empty.
"""


module BeNullOrEmpty =


    [<Fact>]
    let ``Can be chained with And`` () =
        "".Should().BeNullOrEmpty().Id<And<string>>().And.Be("")


    [<Fact>]
    let ``Passes if string is empty`` () = "".Should().BeNullOrEmpty()


    [<Fact>]
    let ``Passes if string is null`` () = (null: string).Should().BeNullOrEmpty()


    [<Fact>]
    let ``Fails with expected message if not empty`` () =
        fun () ->
            let x = "a"
            x.Should().BeNullOrEmpty()
        |> assertExnMsg
            """
x
    should be null or empty, but was
"a"
"""


    [<Fact>]
    let ``Fails with expected message if not empty with because`` () =
        fun () ->
            let x = "a"
            x.Should().BeNullOrEmpty("some reason")
        |> assertExnMsg
            """
x
    should be null or empty because some reason, but was
"a"
"""


module ``BeUpperCase with culture`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "A"
            .Should()
            .BeUpperCase(CultureInfo.InvariantCulture)
            .Id<And<string>>()
            .And.Be("A")


    [<Fact>]
    let ``Passes if string does not contain lower-case characters`` () =
        "A 1".Should().BeUpperCase(CultureInfo.InvariantCulture)


    [<Fact>]
    let ``Fails with expected message if subject contains lower-case characters, CultureInfo.InvariantCulture`` () =
        fun () ->
            let x = "Aa"
            x.Should().BeUpperCase(CultureInfo.InvariantCulture)
        |> assertExnMsg
            """
x
    should be upper-case according to the invariant culture, but was
"Aa"
"""


    [<Fact>]
    let ``Fails with expected message if subject contains lower-case characters, CultureInfo("")`` () =
        fun () ->
            let x = "Aa"
            x.Should().BeUpperCase(CultureInfo(""))
        |> assertExnMsg
            """
x
    should be upper-case according to the invariant culture, but was
"Aa"
"""


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: string = null
            x.Should().BeUpperCase(CultureInfo.InvariantCulture)
        |> assertExnMsg
            """
x
    should be upper-case according to the invariant culture, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: string = null
            x.Should().BeUpperCase(CultureInfo.InvariantCulture, "some reason")
        |> assertExnMsg
            """
x
    should be upper-case according to the invariant culture because some reason, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if subject contains lower-case characters, CultureInfo("nb-NO")`` () =
        fun () ->
            let x = "Aa"
            x.Should().BeUpperCase(CultureInfo("nb-NO"))
        |> assertExnMsg
            """
x
    should be upper-case according to culture nb-NO, but was
"Aa"
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "Aa"
            x.Should().BeUpperCase(CultureInfo.InvariantCulture, "some reason")
        |> assertExnMsg
            """
x
    should be upper-case according to the invariant culture because some reason, but was
"Aa"
"""


module ``BeUpperCase without culture`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "A".Should().BeUpperCase().Id<And<string>>().And.Be("A")


    [<Fact>]
    let ``Passes if string does not contain lower-case characters`` () = "A 1".Should().BeUpperCase()


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: string = null
            x.Should().BeUpperCase()
        |> assertExnMsg
            """
x
    should be upper-case according to the invariant culture, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: string = null
            x.Should().BeUpperCase("some reason")
        |> assertExnMsg
            """
x
    should be upper-case according to the invariant culture because some reason, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if subject contains lower-case characters`` () =
        fun () ->
            let x = "Aa"
            x.Should().BeUpperCase()
        |> assertExnMsg
            """
x
    should be upper-case according to the invariant culture, but was
"Aa"
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "Aa"
            x.Should().BeUpperCase("some reason")
        |> assertExnMsg
            """
x
    should be upper-case according to the invariant culture because some reason, but was
"Aa"
"""


module ``BeLowerCase with culture`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "a"
            .Should()
            .BeLowerCase(CultureInfo.InvariantCulture)
            .Id<And<string>>()
            .And.Be("a")


    [<Fact>]
    let ``Passes if string does not contain upper-case characters`` () =
        "a 1".Should().BeLowerCase(CultureInfo.InvariantCulture)


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: string = null
            x.Should().BeLowerCase(CultureInfo.InvariantCulture)
        |> assertExnMsg
            """
x
    should be lower-case according to the invariant culture, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: string = null
            x.Should().BeLowerCase(CultureInfo.InvariantCulture, "some reason")
        |> assertExnMsg
            """
x
    should be lower-case according to the invariant culture because some reason, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if subject contains upper-case characters, CultureInfo.InvariantCulture`` () =
        fun () ->
            let x = "Aa"
            x.Should().BeLowerCase(CultureInfo.InvariantCulture)
        |> assertExnMsg
            """
x
    should be lower-case according to the invariant culture, but was
"Aa"
"""


    [<Fact>]
    let ``Fails with expected message if subject contains upper-case characters, CultureInfo("")`` () =
        fun () ->
            let x = "Aa"
            x.Should().BeLowerCase(CultureInfo(""))
        |> assertExnMsg
            """
x
    should be lower-case according to the invariant culture, but was
"Aa"
"""


    [<Fact>]
    let ``Fails with expected message if subject contains upper-case characters, CultureInfo("nb-NO")`` () =
        fun () ->
            let x = "Aa"
            x.Should().BeLowerCase(CultureInfo("nb-NO"))
        |> assertExnMsg
            """
x
    should be lower-case according to culture nb-NO, but was
"Aa"
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "Aa"
            x.Should().BeLowerCase(CultureInfo.InvariantCulture, "some reason")
        |> assertExnMsg
            """
x
    should be lower-case according to the invariant culture because some reason, but was
"Aa"
"""


module ``BeLowerCase without culture`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "a".Should().BeLowerCase().Id<And<string>>().And.Be("a")


    [<Fact>]
    let ``Passes if string does not contain upper-case characters`` () = "a 1".Should().BeLowerCase()


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: string = null
            x.Should().BeLowerCase()
        |> assertExnMsg
            """
x
    should be lower-case according to the invariant culture, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: string = null
            x.Should().BeLowerCase("some reason")
        |> assertExnMsg
            """
x
    should be lower-case according to the invariant culture because some reason, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if subject contains upper-case characters`` () =
        fun () ->
            let x = "Aa"
            x.Should().BeLowerCase()
        |> assertExnMsg
            """
x
    should be lower-case according to the invariant culture, but was
"Aa"
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "Aa"
            x.Should().BeLowerCase("some reason")
        |> assertExnMsg
            """
x
    should be lower-case according to the invariant culture because some reason, but was
"Aa"
"""


module ``Contain with StringComparison`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "asd"
            .Should()
            .Contain("s", StringComparison.Ordinal)
            .Id<And<string>>()
            .And.Be("asd")


    [<Fact>]
    let ``Passes if string contains substring`` () =
        "asd".Should().Contain("s", StringComparison.Ordinal)


    [<Fact>]
    let ``Passes if string contains substring using StringComparison.OrdinalIgnoreCase`` () =
        "asd".Should().Contain("S", StringComparison.OrdinalIgnoreCase)


    [<Fact>]
    let ``Throws ArgumentNullException if substring is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> "".Should().Contain(null, StringComparison.Ordinal) |> ignore)


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: string = null
            x.Should().Contain("f", StringComparison.Ordinal)
        |> assertExnMsg
            """
x
    should contain
"f"
    using StringComparison.Ordinal, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: string = null
            x.Should().Contain("f", StringComparison.Ordinal, "some reason")
        |> assertExnMsg
            """
x
    should contain
"f"
    using StringComparison.Ordinal because some reason, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if string does not contain substring`` () =
        fun () ->
            let x = "asd"
            x.Should().Contain("S", StringComparison.Ordinal)
        |> assertExnMsg
            """
x
    should contain
"S"
    using StringComparison.Ordinal, but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if string does not contain substring using StringComparison.CurrentCulture with nb-NO``
        ()
        =
        use _ = CultureInfo.withCurrentCulture "nb-NO"

        fun () ->
            let x = "asd"
            x.Should().Contain("f", StringComparison.CurrentCulture)
        |> assertExnMsg
            """
x
    should contain
"f"
    using StringComparison.CurrentCulture (culture nb-NO), but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if string does not contain substring using StringComparison.CurrentCulture with invariant culture``
        ()
        =
        use _ = CultureInfo.withCurrentCulture ""

        fun () ->
            let x = "asd"
            x.Should().Contain("f", StringComparison.CurrentCulture)
        |> assertExnMsg
            """
x
    should contain
"f"
    using StringComparison.CurrentCulture (invariant culture), but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().Contain("f", StringComparison.Ordinal, "some reason")
        |> assertExnMsg
            """
x
    should contain
"f"
    using StringComparison.Ordinal because some reason, but was
"asd"
"""


module ``Contain without StringComparison`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "asd".Should().Contain("s").Id<And<string>>().And.Be("asd")


    [<Fact>]
    let ``Passes if string contains substring`` () = "asd".Should().Contain("s")


    [<Fact>]
    let ``Throws ArgumentNullException if substring is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> "".Should().Contain(null) |> ignore)


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: string = null
            x.Should().Contain("f")
        |> assertExnMsg
            """
x
    should contain
"f"
    using StringComparison.Ordinal, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: string = null
            x.Should().Contain("f", "some reason")
        |> assertExnMsg
            """
x
    should contain
"f"
    using StringComparison.Ordinal because some reason, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if string does not contain substring`` () =
        fun () ->
            let x = "asd"
            x.Should().Contain("S")
        |> assertExnMsg
            """
x
    should contain
"S"
    using StringComparison.Ordinal, but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().Contain("f", "some reason")
        |> assertExnMsg
            """
x
    should contain
"f"
    using StringComparison.Ordinal because some reason, but was
"asd"
"""


module ``NotContain with StringComparison`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "asd"
            .Should()
            .NotContain("f", StringComparison.Ordinal)
            .Id<And<string>>()
            .And.Be("asd")


    [<Fact>]
    let ``Passes if string does not contain substring`` () =
        "asd".Should().NotContain("S", StringComparison.Ordinal)


    [<Fact>]
    let ``Passes if string does not contain substring using StringComparison.OrdinalIgnoreCase`` () =
        "asd".Should().NotContain("f", StringComparison.OrdinalIgnoreCase)


    [<Fact>]
    let ``Passes if subject is null`` () =
        (null: string).Should().NotContain("d", StringComparison.Ordinal)


    [<Fact>]
    let ``Throws ArgumentNullException if substring is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> "".Should().NotContain(null, StringComparison.Ordinal) |> ignore)


    [<Fact>]
    let ``Fails with expected message if string contains substring`` () =
        fun () ->
            let x = "asd"
            x.Should().NotContain("s", StringComparison.Ordinal)
        |> assertExnMsg
            """
x
    should not contain
"s"
    using StringComparison.Ordinal, but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if string contains substring using StringComparison.CurrentCulture with nb-NO``
        ()
        =
        use _ = CultureInfo.withCurrentCulture "nb-NO"

        fun () ->
            let x = "asd"
            x.Should().NotContain("s", StringComparison.CurrentCulture)
        |> assertExnMsg
            """
x
    should not contain
"s"
    using StringComparison.CurrentCulture (culture nb-NO), but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if string does not contain substring using StringComparison.CurrentCulture with invariant culture``
        ()
        =
        use _ = CultureInfo.withCurrentCulture ""

        fun () ->
            let x = "asd"
            x.Should().NotContain("s", StringComparison.CurrentCulture)
        |> assertExnMsg
            """
x
    should not contain
"s"
    using StringComparison.CurrentCulture (invariant culture), but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().NotContain("s", StringComparison.Ordinal, "some reason")
        |> assertExnMsg
            """
x
    should not contain
"s"
    using StringComparison.Ordinal because some reason, but was
"asd"
"""


module ``NotContain without StringComparison`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "asd".Should().NotContain("f").Id<And<string>>().And.Be("asd")


    [<Fact>]
    let ``Passes if string does not contain substring`` () = "asd".Should().NotContain("f")


    [<Fact>]
    let ``Passes if subject is null`` () = (null: string).Should().NotContain("f")


    [<Fact>]
    let ``Throws ArgumentNullException if substring is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> "".Should().NotContain(null) |> ignore)


    [<Fact>]
    let ``Fails with expected message if string contains substring`` () =
        fun () ->
            let x = "asd"
            x.Should().NotContain("s")
        |> assertExnMsg
            """
x
    should not contain
"s"
    using StringComparison.Ordinal, but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().NotContain("s", "some reason")
        |> assertExnMsg
            """
x
    should not contain
"s"
    using StringComparison.Ordinal because some reason, but was
"asd"
"""


module ``StartWith with StringComparison`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "asd"
            .Should()
            .StartWith("a", StringComparison.Ordinal)
            .Id<And<string>>()
            .And.Be("asd")


    [<Fact>]
    let ``Passes if string starts with substring`` () =
        "asd".Should().StartWith("a", StringComparison.Ordinal)


    [<Fact>]
    let ``Passes if string starts with substring using StringComparison.OrdinalIgnoreCase`` () =
        "asd".Should().StartWith("A", StringComparison.OrdinalIgnoreCase)


    [<Fact>]
    let ``Throws ArgumentNullException if substring is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> "".Should().StartWith(null, StringComparison.Ordinal) |> ignore)


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: string = null
            x.Should().StartWith("A", StringComparison.Ordinal)
        |> assertExnMsg
            """
x
    should start with
"A"
    using StringComparison.Ordinal, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: string = null
            x.Should().StartWith("A", StringComparison.Ordinal, "some reason")
        |> assertExnMsg
            """
x
    should start with
"A"
    using StringComparison.Ordinal because some reason, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message if string does not start with substring`` () =
        fun () ->
            let x = "asd"
            x.Should().StartWith("A", StringComparison.Ordinal)
        |> assertExnMsg
            """
x
    should start with
"A"
    using StringComparison.Ordinal, but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if string does not start with substring using StringComparison.CurrentCulture with nb-NO``
        ()
        =
        use _ = CultureInfo.withCurrentCulture "nb-NO"

        fun () ->
            let x = "asd"
            x.Should().StartWith("A", StringComparison.CurrentCulture)
        |> assertExnMsg
            """
x
    should start with
"A"
    using StringComparison.CurrentCulture (culture nb-NO), but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if string does not start with substring using StringComparison.CurrentCulture with invariant culture``
        ()
        =
        use _ = CultureInfo.withCurrentCulture ""

        fun () ->
            let x = "asd"
            x.Should().StartWith("A", StringComparison.CurrentCulture)
        |> assertExnMsg
            """
x
    should start with
"A"
    using StringComparison.CurrentCulture (invariant culture), but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().StartWith("A", StringComparison.Ordinal, "some reason")
        |> assertExnMsg
            """
x
    should start with
"A"
    using StringComparison.Ordinal because some reason, but was
"asd"
"""


module ``StartWith without StringComparison`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "asd".Should().StartWith("a").Id<And<string>>().And.Be("asd")


    [<Fact>]
    let ``Passes if string starts with substring`` () = "asd".Should().StartWith("a")


    [<Fact>]
    let ``Throws ArgumentNullException if substring is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> "".Should().StartWith(null) |> ignore)


    [<Fact>]
    let ``Fails with expected message if string does not start with substring`` () =
        fun () ->
            let x = "asd"
            x.Should().StartWith("A")
        |> assertExnMsg
            """
x
    should start with
"A"
    using StringComparison.Ordinal, but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: string = null
            x.Should().StartWith("A")
        |> assertExnMsg
            """
x
    should start with
"A"
    using StringComparison.Ordinal, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().StartWith("A", "some reason")
        |> assertExnMsg
            """
x
    should start with
"A"
    using StringComparison.Ordinal because some reason, but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: string = null
            x.Should().StartWith("A", "some reason")
        |> assertExnMsg
            """
x
    should start with
"A"
    using StringComparison.Ordinal because some reason, but was
<null>
"""


module ``NotStartWith with StringComparison`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "asd"
            .Should()
            .NotStartWith("A", StringComparison.Ordinal)
            .Id<And<string>>()
            .And.Be("asd")


    [<Fact>]
    let ``Passes if string does not start with substring`` () =
        "asd".Should().NotStartWith("A", StringComparison.Ordinal)


    [<Fact>]
    let ``Passes if string does not start with substring using StringComparison.OrdinalIgnoreCase`` () =
        "asd".Should().NotStartWith("f", StringComparison.OrdinalIgnoreCase)


    [<Fact>]
    let ``Passes if subject is null`` () =
        (null: string).Should().NotStartWith("A", StringComparison.Ordinal)


    [<Fact>]
    let ``Throws ArgumentNullException if substring is null`` () =
        Assert.Throws<ArgumentNullException>(fun () ->
            "".Should().NotStartWith(null, StringComparison.Ordinal) |> ignore
        )


    [<Fact>]
    let ``Fails with expected message if string starts with substring`` () =
        fun () ->
            let x = "asd"
            x.Should().NotStartWith("a", StringComparison.Ordinal)
        |> assertExnMsg
            """
x
    should not start with
"a"
    using StringComparison.Ordinal, but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if string starts with substring using StringComparison.CurrentCulture with nb-NO``
        ()
        =
        use _ = CultureInfo.withCurrentCulture "nb-NO"

        fun () ->
            let x = "asd"
            x.Should().NotStartWith("a", StringComparison.CurrentCulture)
        |> assertExnMsg
            """
x
    should not start with
"a"
    using StringComparison.CurrentCulture (culture nb-NO), but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if string does not start with substring using StringComparison.CurrentCulture with invariant culture``
        ()
        =
        use _ = CultureInfo.withCurrentCulture ""

        fun () ->
            let x = "asd"
            x.Should().NotStartWith("a", StringComparison.CurrentCulture)
        |> assertExnMsg
            """
x
    should not start with
"a"
    using StringComparison.CurrentCulture (invariant culture), but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().NotStartWith("a", StringComparison.Ordinal, "some reason")
        |> assertExnMsg
            """
x
    should not start with
"a"
    using StringComparison.Ordinal because some reason, but was
"asd"
"""


module ``NotStartWith without StringComparison`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "asd".Should().NotStartWith("A").Id<And<string>>().And.Be("asd")


    [<Fact>]
    let ``Passes if string does not start with substring`` () = "asd".Should().NotStartWith("A")


    [<Fact>]
    let ``Passes if subject is null`` () =
        (null: string).Should().NotStartWith("A")


    [<Fact>]
    let ``Throws ArgumentNullException if substring is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> "".Should().NotStartWith(null) |> ignore)


    [<Fact>]
    let ``Fails with expected message if string starts with substring`` () =
        fun () ->
            let x = "asd"
            x.Should().NotStartWith("a")
        |> assertExnMsg
            """
x
    should not start with
"a"
    using StringComparison.Ordinal, but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().NotStartWith("a", "some reason")
        |> assertExnMsg
            """
x
    should not start with
"a"
    using StringComparison.Ordinal because some reason, but was
"asd"
"""


module ``EndWith with StringComparison`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "asd"
            .Should()
            .EndWith("d", StringComparison.Ordinal)
            .Id<And<string>>()
            .And.Be("asd")


    [<Fact>]
    let ``Passes if string ends with substring`` () =
        "asd".Should().EndWith("d", StringComparison.Ordinal)


    [<Fact>]
    let ``Passes if string ends with substring using StringComparison.OrdinalIgnoreCase`` () =
        "asd".Should().EndWith("D", StringComparison.OrdinalIgnoreCase)


    [<Fact>]
    let ``Throws ArgumentNullException if substring is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> "".Should().EndWith(null, StringComparison.Ordinal) |> ignore)


    [<Fact>]
    let ``Fails with expected message if string does not end with substring`` () =
        fun () ->
            let x = "asd"
            x.Should().EndWith("D", StringComparison.Ordinal)
        |> assertExnMsg
            """
x
    should end with
"D"
    using StringComparison.Ordinal, but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if string does not end with substring using StringComparison.CurrentCulture with nb-NO``
        ()
        =
        use _ = CultureInfo.withCurrentCulture "nb-NO"

        fun () ->
            let x = "asd"
            x.Should().EndWith("D", StringComparison.CurrentCulture)
        |> assertExnMsg
            """
x
    should end with
"D"
    using StringComparison.CurrentCulture (culture nb-NO), but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if string does not end with substring using StringComparison.CurrentCulture with invariant culture``
        ()
        =
        use _ = CultureInfo.withCurrentCulture ""

        fun () ->
            let x = "asd"
            x.Should().EndWith("D", StringComparison.CurrentCulture)
        |> assertExnMsg
            """
x
    should end with
"D"
    using StringComparison.CurrentCulture (invariant culture), but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: string = null
            x.Should().EndWith("D", StringComparison.Ordinal)
        |> assertExnMsg
            """
x
    should end with
"D"
    using StringComparison.Ordinal, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().EndWith("D", StringComparison.Ordinal, "some reason")
        |> assertExnMsg
            """
x
    should end with
"D"
    using StringComparison.Ordinal because some reason, but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: string = null
            x.Should().EndWith("D", StringComparison.Ordinal, "some reason")
        |> assertExnMsg
            """
x
    should end with
"D"
    using StringComparison.Ordinal because some reason, but was
<null>
"""


module ``EndWith without StringComparison`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "asd".Should().EndWith("d").Id<And<string>>().And.Be("asd")


    [<Fact>]
    let ``Passes if string ends with substring`` () = "asd".Should().EndWith("d")


    [<Fact>]
    let ``Throws ArgumentNullException if substring is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> "".Should().EndWith(null) |> ignore)


    [<Fact>]
    let ``Fails with expected message if string does not end with substring`` () =
        fun () ->
            let x = "asd"
            x.Should().EndWith("D")
        |> assertExnMsg
            """
x
    should end with
"D"
    using StringComparison.Ordinal, but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: string = null
            x.Should().EndWith("D")
        |> assertExnMsg
            """
x
    should end with
"D"
    using StringComparison.Ordinal, but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().EndWith("D", "some reason")
        |> assertExnMsg
            """
x
    should end with
"D"
    using StringComparison.Ordinal because some reason, but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: string = null
            x.Should().EndWith("D", "some reason")
        |> assertExnMsg
            """
x
    should end with
"D"
    using StringComparison.Ordinal because some reason, but was
<null>
"""


module ``NotEndWith with StringComparison`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "asd"
            .Should()
            .NotEndWith("D", StringComparison.Ordinal)
            .Id<And<string>>()
            .And.Be("asd")


    [<Fact>]
    let ``Passes if string does not end with substring`` () =
        "asd".Should().NotEndWith("D", StringComparison.Ordinal)


    [<Fact>]
    let ``Passes if string does not end with substring using StringComparison.OrdinalIgnoreCase`` () =
        "asd".Should().NotEndWith("f", StringComparison.OrdinalIgnoreCase)


    [<Fact>]
    let ``Passes if subject is null`` () =
        (null: string).Should().NotEndWith("D", StringComparison.Ordinal)


    [<Fact>]
    let ``Throws ArgumentNullException if substring is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> "".Should().NotEndWith(null, StringComparison.Ordinal) |> ignore)


    [<Fact>]
    let ``Fails with expected message if string ends with substring`` () =
        fun () ->
            let x = "asd"
            x.Should().NotEndWith("d", StringComparison.Ordinal)
        |> assertExnMsg
            """
x
    should not end with
"d"
    using StringComparison.Ordinal, but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if string ends with substring using StringComparison.CurrentCulture with nb-NO``
        ()
        =
        use _ = CultureInfo.withCurrentCulture "nb-NO"

        fun () ->
            let x = "asd"
            x.Should().NotEndWith("d", StringComparison.CurrentCulture)
        |> assertExnMsg
            """
x
    should not end with
"d"
    using StringComparison.CurrentCulture (culture nb-NO), but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if string does not end with substring using StringComparison.CurrentCulture with invariant culture``
        ()
        =
        use _ = CultureInfo.withCurrentCulture ""

        fun () ->
            let x = "asd"
            x.Should().NotEndWith("d", StringComparison.CurrentCulture)
        |> assertExnMsg
            """
x
    should not end with
"d"
    using StringComparison.CurrentCulture (invariant culture), but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().NotEndWith("d", StringComparison.Ordinal, "some reason")
        |> assertExnMsg
            """
x
    should not end with
"d"
    using StringComparison.Ordinal because some reason, but was
"asd"
"""


module ``NotEndWith without StringComparison`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "asd".Should().NotEndWith("D").Id<And<string>>().And.Be("asd")


    [<Fact>]
    let ``Passes if string does not end with substring`` () = "asd".Should().NotEndWith("D")


    [<Fact>]
    let ``Passes if subject is null`` () = (null: string).Should().NotEndWith("D")


    [<Fact>]
    let ``Throws ArgumentNullException if substring is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> "".Should().NotEndWith(null) |> ignore)


    [<Fact>]
    let ``Fails with expected message if string ends with substring`` () =
        fun () ->
            let x = "asd"
            x.Should().NotEndWith("d")
        |> assertExnMsg
            """
x
    should not end with
"d"
    using StringComparison.Ordinal, but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().NotEndWith("d", "some reason")
        |> assertExnMsg
            """
x
    should not end with
"d"
    using StringComparison.Ordinal because some reason, but was
"asd"
"""


module ``MatchRegex with Regex`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "asd".Should().MatchRegex(Regex(".*")).Id<And<string>>().And.Be("asd")


    [<Fact>]
    let ``Passes if string matches regex`` () =
        "asd".Should().MatchRegex(Regex("as.*"))


    [<Fact>]
    let ``Fails with expected message if string does not match regex`` () =
        fun () ->
            let x = "asd"
            x.Should().MatchRegex(Regex("b.*"))
        |> assertExnMsg
            """
x
    should match the regex
b.*
    but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if string does not match regex using custom RegexOptions`` () =
        use _ = CultureInfo.withCurrentCulture "nb-NO"

        fun () ->
            let x = "asd"

            x
                .Should()
                .MatchRegex(Regex("b.*", RegexOptions.IgnoreCase ||| RegexOptions.Multiline))
        |> assertExnMsg
            """
x
    should match the regex
b.*
    using RegexOptions.IgnoreCase, Multiline, but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: string = null
            x.Should().MatchRegex(Regex("b.*"))
        |> assertExnMsg
            """
x
    should match the regex
b.*
    but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().MatchRegex(Regex("b.*"), "some reason")
        |> assertExnMsg
            """
x
    should match the regex
b.*
    because some reason, but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: string = null
            x.Should().MatchRegex(Regex("b.*"), "some reason")
        |> assertExnMsg
            """
x
    should match the regex
b.*
    because some reason, but was
<null>
"""


module ``MatchRegex with string and options`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "asd"
            .Should()
            .MatchRegex(".*", RegexOptions.None)
            .Id<And<string>>()
            .And.Be("asd")


    [<Fact>]
    let ``Passes if string matches regex`` () =
        "asd".Should().MatchRegex("as.*", RegexOptions.None)


    [<Fact>]
    let ``Fails with expected message if string does not match regex`` () =
        fun () ->
            let x = "asd"
            x.Should().MatchRegex("b.*", RegexOptions.None)
        |> assertExnMsg
            """
x
    should match the regex
b.*
    but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if string does not match regex using multiple RegexOptions`` () =
        use _ = CultureInfo.withCurrentCulture "nb-NO"

        fun () ->
            let x = "asd"

            x.Should().MatchRegex("b.*", RegexOptions.IgnoreCase ||| RegexOptions.Multiline)
        |> assertExnMsg
            """
x
    should match the regex
b.*
    using RegexOptions.IgnoreCase, Multiline, but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: string = null
            x.Should().MatchRegex("b.*", RegexOptions.None)
        |> assertExnMsg
            """
x
    should match the regex
b.*
    but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().MatchRegex("b.*", RegexOptions.None, "some reason")
        |> assertExnMsg
            """
x
    should match the regex
b.*
    because some reason, but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: string = null
            x.Should().MatchRegex("b.*", RegexOptions.None, "some reason")
        |> assertExnMsg
            """
x
    should match the regex
b.*
    because some reason, but was
<null>
"""


module ``MatchRegex with string`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "asd".Should().MatchRegex(".*").Id<And<string>>().And.Be("asd")


    [<Fact>]
    let ``Passes if string matches regex`` () = "asd".Should().MatchRegex("as.*")


    [<Fact>]
    let ``Fails with expected message if string does not match regex`` () =
        fun () ->
            let x = "asd"
            x.Should().MatchRegex("b.*")
        |> assertExnMsg
            """
x
    should match the regex
b.*
    but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: string = null
            x.Should().MatchRegex("b.*")
        |> assertExnMsg
            """
x
    should match the regex
b.*
    but was
<null>
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().MatchRegex("b.*", "some reason")
        |> assertExnMsg
            """
x
    should match the regex
b.*
    because some reason, but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: string = null
            x.Should().MatchRegex("b.*", "some reason")
        |> assertExnMsg
            """
x
    should match the regex
b.*
    because some reason, but was
<null>
"""


module ``NotMatchRegex with Regex`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "asd".Should().NotMatchRegex(Regex("f.*")).Id<And<string>>().And.Be("asd")


    [<Fact>]
    let ``Passes if string matches regex`` () =
        "asd".Should().NotMatchRegex(Regex("f.*"))


    [<Fact>]
    let ``Passes if string is null`` () =
        (null: string).Should().NotMatchRegex(Regex(".*"))


    [<Fact>]
    let ``Fails with expected message if string matches regex`` () =
        fun () ->
            let x = "asd"
            x.Should().NotMatchRegex(Regex(".*"))
        |> assertExnMsg
            """
x
    should not match the regex
.*
    but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if string matches regex using custom RegexOptions`` () =
        use _ = CultureInfo.withCurrentCulture "nb-NO"

        fun () ->
            let x = "asd"

            x
                .Should()
                .NotMatchRegex(Regex(".*", RegexOptions.IgnoreCase ||| RegexOptions.Multiline))
        |> assertExnMsg
            """
x
    should not match the regex
.*
    using RegexOptions.IgnoreCase, Multiline, but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().NotMatchRegex(Regex(".*"), "some reason")
        |> assertExnMsg
            """
x
    should not match the regex
.*
    because some reason, but was
"asd"
"""


module ``NotMatchRegex with string and options`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "asd"
            .Should()
            .NotMatchRegex("f.*", RegexOptions.None)
            .Id<And<string>>()
            .And.Be("asd")


    [<Fact>]
    let ``Passes if string matches regex`` () =
        "asd".Should().NotMatchRegex("f.*", RegexOptions.None)


    [<Fact>]
    let ``Passes if string is null`` () =
        (null: string).Should().NotMatchRegex(".*", RegexOptions.None)


    [<Fact>]
    let ``Fails with expected message if string matches regex`` () =
        fun () ->
            let x = "asd"
            x.Should().NotMatchRegex(".*", RegexOptions.None)
        |> assertExnMsg
            """
x
    should not match the regex
.*
    but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if string matches regex using multiple RegexOptions`` () =
        use _ = CultureInfo.withCurrentCulture "nb-NO"

        fun () ->
            let x = "asd"

            x
                .Should()
                .NotMatchRegex(".*", RegexOptions.IgnoreCase ||| RegexOptions.Multiline)
        |> assertExnMsg
            """
x
    should not match the regex
.*
    using RegexOptions.IgnoreCase, Multiline, but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().NotMatchRegex(".*", RegexOptions.None, "some reason")
        |> assertExnMsg
            """
x
    should not match the regex
.*
    because some reason, but was
"asd"
"""


module ``NotMatchRegex with string`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "asd".Should().NotMatchRegex("f.*").Id<And<string>>().And.Be("asd")


    [<Fact>]
    let ``Passes if string matches regex`` () = "asd".Should().NotMatchRegex("f.*")


    [<Fact>]
    let ``Passes if string is null`` () =
        (null: string).Should().NotMatchRegex(".*")


    [<Fact>]
    let ``Fails with expected message if string matches regex`` () =
        fun () ->
            let x = "asd"
            x.Should().NotMatchRegex(".*")
        |> assertExnMsg
            """
x
    should not match the regex
.*
    but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().NotMatchRegex(".*", "some reason")
        |> assertExnMsg
            """
x
    should not match the regex
.*
    because some reason, but was
"asd"
"""
