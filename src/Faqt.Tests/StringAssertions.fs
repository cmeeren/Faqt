module StringAssertions

open System
open System.Globalization
open System.Text.RegularExpressions
open Faqt
open Xunit


module HaveLength =


    [<Fact>]
    let ``Can use seq assertion with expected success`` () = "a".Should().HaveLength(1)


    [<Fact>]
    let ``Can use seq assertion with expected error`` () =
        fun () ->
            let x = "a"
            x.Should().HaveLength(2)
        |> assertExnMsg
            """
Subject: x
Should: HaveLength
Expected: 2
But was: 1
Subject value: a
"""


module BeEmpty =


    [<Fact>]
    let ``Can use seq assertion with expected success`` () = "".Should().BeEmpty()


    [<Fact>]
    let ``Can use seq assertion with expected error`` () =
        fun () ->
            let x = "a"
            x.Should().BeEmpty()
        |> assertExnMsg
            """
Subject: x
Should: BeEmpty
But was: a
"""


module NotBeEmpty =


    [<Fact>]
    let ``Can use seq assertion with expected success`` () = "a".Should().NotBeEmpty()


    [<Fact>]
    let ``Can use seq assertion with expected error`` () =
        fun () ->
            let x = ""
            x.Should().NotBeEmpty()
        |> assertExnMsg
            """
Subject: x
Should: NotBeEmpty
But was: ''
"""


module BeNullOrEmpty =


    [<Fact>]
    let ``Can use seq assertion with expected success`` () = "".Should().BeNullOrEmpty()


    [<Fact>]
    let ``Can use seq assertion with expected error`` () =
        fun () ->
            let x = "a"
            x.Should().BeNullOrEmpty()
        |> assertExnMsg
            """
Subject: x
Should: BeNullOrEmpty
But was: a
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
Subject: x
Should: BeUpperCase
In culture: invariant
But was: Aa
"""


    [<Fact>]
    let ``Fails with expected message if subject contains lower-case characters, CultureInfo("")`` () =
        fun () ->
            let x = "Aa"
            x.Should().BeUpperCase(CultureInfo(""))
        |> assertExnMsg
            """
Subject: x
Should: BeUpperCase
In culture: invariant
But was: Aa
"""


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: string = null
            x.Should().BeUpperCase(CultureInfo.InvariantCulture)
        |> assertExnMsg
            """
Subject: x
Should: BeUpperCase
In culture: invariant
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: string = null
            x.Should().BeUpperCase(CultureInfo.InvariantCulture, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeUpperCase
In culture: invariant
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if subject contains lower-case characters, CultureInfo("nb-NO")`` () =
        fun () ->
            let x = "Aa"
            x.Should().BeUpperCase(CultureInfo("nb-NO"))
        |> assertExnMsg
            """
Subject: x
Should: BeUpperCase
In culture: nb-NO
But was: Aa
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "Aa"
            x.Should().BeUpperCase(CultureInfo.InvariantCulture, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeUpperCase
In culture: invariant
But was: Aa
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
Subject: x
Should: BeUpperCase
In culture: invariant
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: string = null
            x.Should().BeUpperCase("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeUpperCase
In culture: invariant
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if subject contains lower-case characters`` () =
        fun () ->
            let x = "Aa"
            x.Should().BeUpperCase()
        |> assertExnMsg
            """
Subject: x
Should: BeUpperCase
In culture: invariant
But was: Aa
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "Aa"
            x.Should().BeUpperCase("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeUpperCase
In culture: invariant
But was: Aa
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
Subject: x
Should: BeLowerCase
In culture: invariant
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: string = null
            x.Should().BeLowerCase(CultureInfo.InvariantCulture, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeLowerCase
In culture: invariant
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if subject contains upper-case characters, CultureInfo.InvariantCulture`` () =
        fun () ->
            let x = "Aa"
            x.Should().BeLowerCase(CultureInfo.InvariantCulture)
        |> assertExnMsg
            """
Subject: x
Should: BeLowerCase
In culture: invariant
But was: Aa
"""


    [<Fact>]
    let ``Fails with expected message if subject contains upper-case characters, CultureInfo("")`` () =
        fun () ->
            let x = "Aa"
            x.Should().BeLowerCase(CultureInfo(""))
        |> assertExnMsg
            """
Subject: x
Should: BeLowerCase
In culture: invariant
But was: Aa
"""


    [<Fact>]
    let ``Fails with expected message if subject contains upper-case characters, CultureInfo("nb-NO")`` () =
        fun () ->
            let x = "Aa"
            x.Should().BeLowerCase(CultureInfo("nb-NO"))
        |> assertExnMsg
            """
Subject: x
Should: BeLowerCase
In culture: nb-NO
But was: Aa
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "Aa"
            x.Should().BeLowerCase(CultureInfo.InvariantCulture, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeLowerCase
In culture: invariant
But was: Aa
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
Subject: x
Should: BeLowerCase
In culture: invariant
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: string = null
            x.Should().BeLowerCase("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeLowerCase
In culture: invariant
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if subject contains upper-case characters`` () =
        fun () ->
            let x = "Aa"
            x.Should().BeLowerCase()
        |> assertExnMsg
            """
Subject: x
Should: BeLowerCase
In culture: invariant
But was: Aa
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "Aa"
            x.Should().BeLowerCase("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeLowerCase
In culture: invariant
But was: Aa
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
    let ``Passes if substring is empty`` () =
        "asd".Should().Contain("", StringComparison.OrdinalIgnoreCase)


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
Subject: x
Should: Contain
Substring: f
StringComparison: Ordinal
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: string = null
            x.Should().Contain("f", StringComparison.Ordinal, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Contain
Substring: f
StringComparison: Ordinal
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if string does not contain substring`` () =
        fun () ->
            let x = "asd"
            x.Should().Contain("S", StringComparison.Ordinal)
        |> assertExnMsg
            """
Subject: x
Should: Contain
Substring: S
StringComparison: Ordinal
But was: asd
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
Subject: x
Should: Contain
Substring: f
StringComparison: CurrentCulture
CurrentCulture: nb-NO
But was: asd
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
Subject: x
Should: Contain
Substring: f
StringComparison: CurrentCulture
CurrentCulture: invariant
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().Contain("f", StringComparison.Ordinal, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Contain
Substring: f
StringComparison: Ordinal
But was: asd
"""


module ``Contain without StringComparison`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "asd".Should().Contain("s").Id<And<string>>().And.Be("asd")


    [<Fact>]
    let ``Passes if string contains substring`` () = "asd".Should().Contain("s")


    [<Fact>]
    let ``Passes if substring is empty`` () = "asd".Should().Contain("")


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
Subject: x
Should: Contain
Substring: f
StringComparison: Ordinal
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: string = null
            x.Should().Contain("f", "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Contain
Substring: f
StringComparison: Ordinal
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if string does not contain substring`` () =
        fun () ->
            let x = "asd"
            x.Should().Contain("S")
        |> assertExnMsg
            """
Subject: x
Should: Contain
Substring: S
StringComparison: Ordinal
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().Contain("f", "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Contain
Substring: f
StringComparison: Ordinal
But was: asd
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
    let ``Fails if substring is empty`` () =
        assertFails (fun () -> "".Should().NotContain("", StringComparison.Ordinal) |> ignore)


    [<Fact>]
    let ``Fails with expected message if string contains substring`` () =
        fun () ->
            let x = "asd"
            x.Should().NotContain("s", StringComparison.Ordinal)
        |> assertExnMsg
            """
Subject: x
Should: NotContain
Substring: s
StringComparison: Ordinal
But was: asd
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
Subject: x
Should: NotContain
Substring: s
StringComparison: CurrentCulture
CurrentCulture: nb-NO
But was: asd
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
Subject: x
Should: NotContain
Substring: s
StringComparison: CurrentCulture
CurrentCulture: invariant
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().NotContain("s", StringComparison.Ordinal, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotContain
Substring: s
StringComparison: Ordinal
But was: asd
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
    let ``Fails if substring is empty`` () =
        assertFails (fun () -> "".Should().NotContain(""))


    [<Fact>]
    let ``Fails with expected message if string contains substring`` () =
        fun () ->
            let x = "asd"
            x.Should().NotContain("s")
        |> assertExnMsg
            """
Subject: x
Should: NotContain
Substring: s
StringComparison: Ordinal
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().NotContain("s", "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotContain
Substring: s
StringComparison: Ordinal
But was: asd
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
Subject: x
Should: StartWith
Substring: A
StringComparison: Ordinal
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: string = null
            x.Should().StartWith("A", StringComparison.Ordinal, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: StartWith
Substring: A
StringComparison: Ordinal
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if string does not start with substring`` () =
        fun () ->
            let x = "asd"
            x.Should().StartWith("A", StringComparison.Ordinal)
        |> assertExnMsg
            """
Subject: x
Should: StartWith
Substring: A
StringComparison: Ordinal
But was: asd
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
Subject: x
Should: StartWith
Substring: A
StringComparison: CurrentCulture
CurrentCulture: nb-NO
But was: asd
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
Subject: x
Should: StartWith
Substring: A
StringComparison: CurrentCulture
CurrentCulture: invariant
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().StartWith("A", StringComparison.Ordinal, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: StartWith
Substring: A
StringComparison: Ordinal
But was: asd
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
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: string = null
            x.Should().StartWith("A")
        |> assertExnMsg
            """
Subject: x
Should: StartWith
Substring: A
StringComparison: Ordinal
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: string = null
            x.Should().StartWith("A", "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: StartWith
Substring: A
StringComparison: Ordinal
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if string does not start with substring`` () =
        fun () ->
            let x = "asd"
            x.Should().StartWith("A")
        |> assertExnMsg
            """
Subject: x
Should: StartWith
Substring: A
StringComparison: Ordinal
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().StartWith("A", "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: StartWith
Substring: A
StringComparison: Ordinal
But was: asd
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
Subject: x
Should: NotStartWith
Substring: a
StringComparison: Ordinal
But was: asd
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
Subject: x
Should: NotStartWith
Substring: a
StringComparison: CurrentCulture
CurrentCulture: nb-NO
But was: asd
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
Subject: x
Should: NotStartWith
Substring: a
StringComparison: CurrentCulture
CurrentCulture: invariant
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().NotStartWith("a", StringComparison.Ordinal, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotStartWith
Substring: a
StringComparison: Ordinal
But was: asd
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
Subject: x
Should: NotStartWith
Substring: a
StringComparison: Ordinal
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().NotStartWith("a", "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotStartWith
Substring: a
StringComparison: Ordinal
But was: asd
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
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: string = null
            x.Should().EndWith("D", StringComparison.Ordinal)
        |> assertExnMsg
            """
Subject: x
Should: EndWith
Substring: D
StringComparison: Ordinal
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: string = null
            x.Should().EndWith("D", StringComparison.Ordinal, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: EndWith
Substring: D
StringComparison: Ordinal
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if string does not end with substring`` () =
        fun () ->
            let x = "asd"
            x.Should().EndWith("D", StringComparison.Ordinal)
        |> assertExnMsg
            """
Subject: x
Should: EndWith
Substring: D
StringComparison: Ordinal
But was: asd
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
Subject: x
Should: EndWith
Substring: D
StringComparison: CurrentCulture
CurrentCulture: nb-NO
But was: asd
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
Subject: x
Should: EndWith
Substring: D
StringComparison: CurrentCulture
CurrentCulture: invariant
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().EndWith("D", StringComparison.Ordinal, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: EndWith
Substring: D
StringComparison: Ordinal
But was: asd
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
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: string = null
            x.Should().EndWith("D")
        |> assertExnMsg
            """
Subject: x
Should: EndWith
Substring: D
StringComparison: Ordinal
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: string = null
            x.Should().EndWith("D", "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: EndWith
Substring: D
StringComparison: Ordinal
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if string does not end with substring`` () =
        fun () ->
            let x = "asd"
            x.Should().EndWith("D")
        |> assertExnMsg
            """
Subject: x
Should: EndWith
Substring: D
StringComparison: Ordinal
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().EndWith("D", "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: EndWith
Substring: D
StringComparison: Ordinal
But was: asd
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
Subject: x
Should: NotEndWith
Substring: d
StringComparison: Ordinal
But was: asd
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
Subject: x
Should: NotEndWith
Substring: d
StringComparison: CurrentCulture
CurrentCulture: nb-NO
But was: asd
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
Subject: x
Should: NotEndWith
Substring: d
StringComparison: CurrentCulture
CurrentCulture: invariant
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().NotEndWith("d", StringComparison.Ordinal, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotEndWith
Substring: d
StringComparison: Ordinal
But was: asd
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
Subject: x
Should: NotEndWith
Substring: d
StringComparison: Ordinal
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().NotEndWith("d", "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotEndWith
Substring: d
StringComparison: Ordinal
But was: asd
"""


module ``MatchRegex with Regex`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "asd".Should().MatchRegex(Regex(".*")).Id<And<string>>().And.Be("asd")


    [<Fact>]
    let ``Passes if string matches regex`` () =
        "asd".Should().MatchRegex(Regex("as.*"))


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: string = null
            x.Should().MatchRegex(Regex("b.*"))
        |> assertExnMsg
            """
Subject: x
Should: MatchRegex
Pattern: b.*
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: string = null
            x.Should().MatchRegex(Regex("b.*"), "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: MatchRegex
Pattern: b.*
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if string does not match regex`` () =
        fun () ->
            let x = "asd"
            x.Should().MatchRegex(Regex("b.*"))
        |> assertExnMsg
            """
Subject: x
Should: MatchRegex
Pattern: b.*
But was: asd
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
Subject: x
Should: MatchRegex
Pattern: b.*
RegexOptions: IgnoreCase, Multiline
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().MatchRegex(Regex("b.*"), "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: MatchRegex
Pattern: b.*
But was: asd
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
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: string = null
            x.Should().MatchRegex("b.*", RegexOptions.None)
        |> assertExnMsg
            """
Subject: x
Should: MatchRegex
Pattern: b.*
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: string = null
            x.Should().MatchRegex("b.*", RegexOptions.None, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: MatchRegex
Pattern: b.*
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if string does not match regex`` () =
        fun () ->
            let x = "asd"
            x.Should().MatchRegex("b.*", RegexOptions.None)
        |> assertExnMsg
            """
Subject: x
Should: MatchRegex
Pattern: b.*
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message if string does not match regex using multiple RegexOptions`` () =
        use _ = CultureInfo.withCurrentCulture "nb-NO"

        fun () ->
            let x = "asd"

            x.Should().MatchRegex("b.*", RegexOptions.IgnoreCase ||| RegexOptions.Multiline)
        |> assertExnMsg
            """
Subject: x
Should: MatchRegex
Pattern: b.*
RegexOptions: IgnoreCase, Multiline
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().MatchRegex("b.*", RegexOptions.None, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: MatchRegex
Pattern: b.*
But was: asd
"""


module ``MatchRegex with string`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "asd".Should().MatchRegex(".*").Id<And<string>>().And.Be("asd")


    [<Fact>]
    let ``Passes if string matches regex`` () = "asd".Should().MatchRegex("as.*")


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let x: string = null
            x.Should().MatchRegex("b.*")
        |> assertExnMsg
            """
Subject: x
Should: MatchRegex
Pattern: b.*
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if null with because`` () =
        fun () ->
            let x: string = null
            x.Should().MatchRegex("b.*", "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: MatchRegex
Pattern: b.*
But was: null
"""


    [<Fact>]
    let ``Fails with expected message if string does not match regex`` () =
        fun () ->
            let x = "asd"
            x.Should().MatchRegex("b.*")
        |> assertExnMsg
            """
Subject: x
Should: MatchRegex
Pattern: b.*
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().MatchRegex("b.*", "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: MatchRegex
Pattern: b.*
But was: asd
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
Subject: x
Should: NotMatchRegex
Pattern: .*
But was: asd
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
Subject: x
Should: NotMatchRegex
Pattern: .*
RegexOptions: IgnoreCase, Multiline
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().NotMatchRegex(Regex(".*"), "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotMatchRegex
Pattern: .*
But was: asd
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
Subject: x
Should: NotMatchRegex
Pattern: .*
But was: asd
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
Subject: x
Should: NotMatchRegex
Pattern: .*
RegexOptions: IgnoreCase, Multiline
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().NotMatchRegex(".*", RegexOptions.None, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotMatchRegex
Pattern: .*
But was: asd
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
Subject: x
Should: NotMatchRegex
Pattern: .*
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().NotMatchRegex(".*", "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotMatchRegex
Pattern: .*
But was: asd
"""
