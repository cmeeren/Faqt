module StringAssertions

open System
open System.Globalization
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
    let ``Fails with expected message`` () =
        fun () ->
            let x = ""
            x.Should().HaveLength(1)
        |> assertExnMsg
            """
x
    should have length
1
    but was
""
    with length
0
"""


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
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = ""
            x.Should().HaveLength(1, "some reason")
        |> assertExnMsg
            """
x
    should have length
1
    because some reason, but was
""
    with length
0
"""


module BeEmpty =


    [<Fact>]
    let ``Can be chained with And`` () =
        "".Should().BeEmpty().Id<And<string>>().And.Be("")


    [<Fact>]
    let ``Passes if string is empty`` () = "".Should().BeEmpty()


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
    let ``Fails with expected message if empty with because`` () =
        fun () ->
            let x = "a"
            x.Should().BeEmpty("some reason")
        |> assertExnMsg
            """
x
    should be empty because some reason, but was
"a"
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


module NotBeEmpty =


    [<Fact>]
    let ``Can be chained with And`` () =
        "a".Should().NotBeEmpty().Id<And<string>>().And.Be("a")


    [<Fact>]
    let ``Passes if string is not empty`` () = "a".Should().NotBeEmpty()


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
    let ``Fails with expected message if empty with because`` () =
        fun () ->
            let x = ""
            x.Should().NotBeEmpty("some reason")
        |> assertExnMsg
            """
x
    should not be empty because some reason, but was empty.
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
    let ``Fails with expected message if empty with because`` () =
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


module ``BeUpperCase without culture`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "A".Should().BeUpperCase().Id<And<string>>().And.Be("A")


    [<Fact>]
    let ``Passes if string does not contain lower-case characters`` () = "A 1".Should().BeUpperCase()


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


module ``BeLowerCase without culture`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "a".Should().BeLowerCase().Id<And<string>>().And.Be("a")


    [<Fact>]
    let ``Passes if string does not contain upper-case characters`` () = "a 1".Should().BeLowerCase()


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
