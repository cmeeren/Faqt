module StringAssertions

open System
open System.Globalization
open System.Text.RegularExpressions
open Faqt
open Xunit


[<AutoOpen>]
module private Helpers =


    let asciiExceptLetters =
        [| yield! [| 0..64 |]; yield! [| 91..96 |]; yield! [| 123..126 |] |]
        |> Array.map char
        |> String


    let asciiUppercaseLetters = [| 65..90 |] |> Array.map char |> String


    let asciiLowercaseLetters = [| 97..122 |] |> Array.map char |> String


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


    let passData = [
        [| box asciiUppercaseLetters; CultureInfo.InvariantCulture |]
        [| asciiExceptLetters; CultureInfo.InvariantCulture |]
        [| "Å"; CultureInfo.InvariantCulture |]
        [| "ı"; CultureInfo.InvariantCulture |] // No casing in invariant culture, lower in Turkish
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if containing lower-case characters in the specified culture``
        (subject: string)
        (culture: CultureInfo)
        =
        subject.Should().BeUpperCase(culture)


    let failData = [
        [| box null; CultureInfo.InvariantCulture |]
        [| "a"; CultureInfo.InvariantCulture |]
        [| "Aa"; CultureInfo.InvariantCulture |]
        [| "å"; CultureInfo.InvariantCulture |]
        [| "ı"; CultureInfo("tr-TR") |] // No casing in invariant culture, lower in Turkish
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if null or containing lower-case characters in the specified culture``
        (subject: string)
        (culture: CultureInfo)
        =
        assertFails (fun () -> subject.Should().BeUpperCase(culture))


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = "a"
            x.Should().BeUpperCase(CultureInfo("nb-NO"))
        |> assertExnMsg
            """
Subject: x
Should: BeUpperCase
In culture: nb-NO
But was: a
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "a"
            x.Should().BeUpperCase(CultureInfo("nb-NO"), "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeUpperCase
In culture: nb-NO
But was: a
"""


module ``BeUpperCase without culture`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "A".Should().BeUpperCase().Id<And<string>>().And.Be("A")


    let passData = [ [| box asciiUppercaseLetters |]; [| asciiExceptLetters |]; [| "Å" |] ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if containing lower-case characters in the specified culture`` (subject: string) =
        subject.Should().BeUpperCase()


    let failData = [ [| null |]; [| "a" |]; [| "Aa" |]; [| "å" |] ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if null or containing lower-case characters in the specified culture`` (subject: string) =
        assertFails (fun () -> subject.Should().BeUpperCase())


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = "a"
            x.Should().BeUpperCase()
        |> assertExnMsg
            """
Subject: x
Should: BeUpperCase
In culture: invariant
But was: a
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "a"
            x.Should().BeUpperCase("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeUpperCase
In culture: invariant
But was: a
"""


module ``BeLowerCase with culture`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "a"
            .Should()
            .BeLowerCase(CultureInfo.InvariantCulture)
            .Id<And<string>>()
            .And.Be("a")


    let passData = [
        [| box asciiLowercaseLetters; CultureInfo.InvariantCulture |]
        [| asciiExceptLetters; CultureInfo.InvariantCulture |]
        [| "å"; CultureInfo.InvariantCulture |]
        [| "ı"; CultureInfo.InvariantCulture |] // No casing in invariant culture, lower in Turkish
        [| "ı"; CultureInfo("tr-TR") |]
    ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if containing lower-case characters in the specified culture``
        (subject: string)
        (culture: CultureInfo)
        =
        subject.Should().BeLowerCase(culture)


    let failData = [
        [| box null; CultureInfo.InvariantCulture |]
        [| "A"; CultureInfo.InvariantCulture |]
        [| "Aa"; CultureInfo.InvariantCulture |]
        [| "Å"; CultureInfo.InvariantCulture |]
    ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if null or containing lower-case characters in the specified culture``
        (subject: string)
        (culture: CultureInfo)
        =
        assertFails (fun () -> subject.Should().BeLowerCase(culture))


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = "A"
            x.Should().BeLowerCase(CultureInfo("nb-NO"))
        |> assertExnMsg
            """
Subject: x
Should: BeLowerCase
In culture: nb-NO
But was: A
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "A"
            x.Should().BeLowerCase(CultureInfo("nb-NO"), "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeLowerCase
In culture: nb-NO
But was: A
"""


module ``BeLowerCase without culture`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "a"
            .Should()
            .BeLowerCase(CultureInfo.InvariantCulture)
            .Id<And<string>>()
            .And.Be("a")


    let passData = [ [| box asciiLowercaseLetters |]; [| asciiExceptLetters |]; [| "å" |] ]


    [<Theory>]
    [<MemberData(nameof passData)>]
    let ``Passes if containing lower-case characters in the specified culture`` (subject: string) =
        subject.Should().BeLowerCase()


    let failData = [ [| null |]; [| "A" |]; [| "Aa" |]; [| "Å" |] ]


    [<Theory>]
    [<MemberData(nameof failData)>]
    let ``Fails if null or containing lower-case characters in the specified culture`` (subject: string) =
        assertFails (fun () -> subject.Should().BeLowerCase())


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = "A"
            x.Should().BeLowerCase()
        |> assertExnMsg
            """
Subject: x
Should: BeLowerCase
In culture: invariant
But was: A
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "A"
            x.Should().BeLowerCase("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: BeLowerCase
In culture: invariant
But was: A
"""


module ``Be with StringComparison`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "a".Should().Be("a", StringComparison.Ordinal).Id<And<string>>().And.Be("a")


    [<Theory>]
    [<InlineData(null, null, StringComparison.Ordinal, "")>]
    [<InlineData("", "", StringComparison.Ordinal, "")>]
    [<InlineData("a", "a", StringComparison.Ordinal, "")>]
    [<InlineData("asd", "asd", StringComparison.Ordinal, "")>]
    [<InlineData("ASD", "asd", StringComparison.OrdinalIgnoreCase, "")>]
    [<InlineData("i", "İ", StringComparison.CurrentCultureIgnoreCase, "tr-TR")>] // Different casings of same letter in Turkish, but not invariant
    let ``Passes if string equals expected using the specified StringComparison``
        (subject: string)
        (substring: string)
        (comparison: StringComparison)
        (culture: string)
        =
        use _ = CultureInfo.withCurrentCulture culture
        subject.Should().Be(substring, comparison)


    [<Theory>]
    [<InlineData(null, "", StringComparison.Ordinal, "")>]
    [<InlineData("", null, StringComparison.Ordinal, "")>]
    [<InlineData("", "a", StringComparison.Ordinal, "")>]
    [<InlineData("a", "", StringComparison.Ordinal, "")>]
    [<InlineData("a", "b", StringComparison.Ordinal, "")>]
    [<InlineData("a", "A", StringComparison.Ordinal, "")>]
    [<InlineData("i", "İ", StringComparison.InvariantCultureIgnoreCase, "")>] // Different casings of same letter in Turkish, but not invariant
    let ``Fails if not equals expected using the specified StringComparison``
        (subject: string)
        (substring: string)
        (comparison: StringComparison)
        (culture: string)
        =
        use _ = CultureInfo.withCurrentCulture culture
        assertFails (fun () -> subject.Should().Be(substring, comparison))


    [<Fact>]
    let ``Fails with expected message using StringComparison.Ordinal`` () =
        fun () ->
            let x = "asd"
            x.Should().Be("a", StringComparison.Ordinal)
        |> assertExnMsg
            """
Subject: x
Should: Be
Expected: a
StringComparison: Ordinal
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message using StringComparison.CurrentCulture`` () =
        use _ = CultureInfo.withCurrentCulture "nb-NO"

        fun () ->
            let x = "asd"
            x.Should().Be("a", StringComparison.CurrentCulture)
        |> assertExnMsg
            """
Subject: x
Should: Be
Expected: a
StringComparison: CurrentCulture
CurrentCulture: nb-NO
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().Be("a", StringComparison.Ordinal, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be
Expected: a
StringComparison: Ordinal
But was: asd
"""


module ``NotBe with StringComparison`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "a".Should().NotBe("b", StringComparison.Ordinal).Id<And<string>>().And.Be("a")


    [<Theory>]
    [<InlineData(null, "", StringComparison.Ordinal, "")>]
    [<InlineData("", null, StringComparison.Ordinal, "")>]
    [<InlineData("", "a", StringComparison.Ordinal, "")>]
    [<InlineData("a", "", StringComparison.Ordinal, "")>]
    [<InlineData("a", "b", StringComparison.Ordinal, "")>]
    [<InlineData("a", "A", StringComparison.Ordinal, "")>]
    [<InlineData("i", "İ", StringComparison.InvariantCultureIgnoreCase, "")>] // Different casings of same letter in Turkish, but not invariant
    let ``Passes if not equals expected using the specified StringComparison``
        (subject: string)
        (substring: string)
        (comparison: StringComparison)
        (culture: string)
        =
        use _ = CultureInfo.withCurrentCulture culture
        subject.Should().NotBe(substring, comparison)


    [<Theory>]
    [<InlineData(null, null, StringComparison.Ordinal, "")>]
    [<InlineData("", "", StringComparison.Ordinal, "")>]
    [<InlineData("a", "a", StringComparison.Ordinal, "")>]
    [<InlineData("asd", "asd", StringComparison.Ordinal, "")>]
    [<InlineData("ASD", "asd", StringComparison.OrdinalIgnoreCase, "")>]
    [<InlineData("i", "İ", StringComparison.CurrentCultureIgnoreCase, "tr-TR")>] // Different casings of same letter in Turkish, but not invariant
    let ``Fails if string equals expected using the specified StringComparison``
        (subject: string)
        (substring: string)
        (comparison: StringComparison)
        (culture: string)
        =
        use _ = CultureInfo.withCurrentCulture culture
        assertFails (fun () -> subject.Should().NotBe(substring, comparison))


    [<Fact>]
    let ``Fails with expected message using StringComparison.Ordinal`` () =
        fun () ->
            let x = "asd"
            x.Should().NotBe("asd", StringComparison.Ordinal)
        |> assertExnMsg
            """
Subject: x
Should: NotBe
Other: asd
StringComparison: Ordinal
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message using StringComparison.CurrentCulture`` () =
        use _ = CultureInfo.withCurrentCulture "nb-NO"

        fun () ->
            let x = "asd"
            x.Should().NotBe("asd", StringComparison.CurrentCulture)
        |> assertExnMsg
            """
Subject: x
Should: NotBe
Other: asd
StringComparison: CurrentCulture
CurrentCulture: nb-NO
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().NotBe("asd", StringComparison.Ordinal, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotBe
Other: asd
StringComparison: Ordinal
But was: asd
"""


module ``Contain with StringComparison`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "a"
            .Should()
            .Contain("a", StringComparison.Ordinal)
            .Id<And<string>>()
            .And.Be("a")


    [<Theory>]
    [<InlineData("a", "", StringComparison.Ordinal, "")>]
    [<InlineData("a", "a", StringComparison.Ordinal, "")>]
    [<InlineData("asd", "s", StringComparison.Ordinal, "")>]
    [<InlineData("asd", "S", StringComparison.OrdinalIgnoreCase, "")>]
    [<InlineData("ASD", "s", StringComparison.OrdinalIgnoreCase, "")>]
    [<InlineData("i", "İ", StringComparison.CurrentCultureIgnoreCase, "tr-TR")>] // Different casings of same letter in Turkish, but not invariant
    let ``Passes if substring is empty or string contains substring using the specified StringComparison``
        (subject: string)
        (substring: string)
        (comparison: StringComparison)
        (culture: string)
        =
        use _ = CultureInfo.withCurrentCulture culture
        subject.Should().Contain(substring, comparison)


    [<Theory>]
    [<InlineData(null, "", StringComparison.Ordinal, "")>]
    [<InlineData("a", "A", StringComparison.Ordinal, "")>]
    [<InlineData("asd", "f", StringComparison.Ordinal, "")>]
    [<InlineData("i", "İ", StringComparison.InvariantCultureIgnoreCase, "")>] // Different casings of same letter in Turkish, but not invariant
    let ``Fails if null or not containing substring using the specified StringComparison``
        (subject: string)
        (substring: string)
        (comparison: StringComparison)
        (culture: string)
        =
        use _ = CultureInfo.withCurrentCulture culture
        assertFails (fun () -> subject.Should().Contain(substring, comparison))


    [<Fact>]
    let ``Throws ArgumentNullException if substring is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> "".Should().Contain(null, StringComparison.Ordinal) |> ignore)


    [<Fact>]
    let ``Fails with expected message using StringComparison.Ordinal`` () =
        fun () ->
            let x = "asd"
            x.Should().Contain("f", StringComparison.Ordinal)
        |> assertExnMsg
            """
Subject: x
Should: Contain
Substring: f
StringComparison: Ordinal
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message using StringComparison.CurrentCulture`` () =
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
        "a".Should().Contain("a").Id<And<string>>().And.Be("a")


    [<Theory>]
    [<InlineData("a", "")>]
    [<InlineData("a", "a")>]
    [<InlineData("asd", "s")>]
    let ``Passes if substring is empty or string contains substring`` (subject: string) (substring: string) =
        subject.Should().Contain(substring)


    [<Theory>]
    [<InlineData(null, "")>]
    [<InlineData("a", "A")>]
    [<InlineData("asd", "f")>]
    let ``Fails if null or not containing substring`` (subject: string) (substring: string) =
        assertFails (fun () -> subject.Should().Contain(substring))


    [<Fact>]
    let ``Throws ArgumentNullException if substring is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> "".Should().Contain(null) |> ignore)


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = "asd"
            x.Should().Contain("f")
        |> assertExnMsg
            """
Subject: x
Should: Contain
Substring: f
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
        "a"
            .Should()
            .NotContain("b", StringComparison.Ordinal)
            .Id<And<string>>()
            .And.Be("a")


    [<Theory>]
    [<InlineData(null, "", StringComparison.Ordinal, "")>]
    [<InlineData("a", "A", StringComparison.Ordinal, "")>]
    [<InlineData("asd", "f", StringComparison.Ordinal, "")>]
    [<InlineData("i", "İ", StringComparison.InvariantCultureIgnoreCase, "")>] // Different casings of same letter in Turkish, but not invariant
    let ``Passes if null or not containing substring using the specified StringComparison``
        (subject: string)
        (substring: string)
        (comparison: StringComparison)
        (culture: string)
        =
        use _ = CultureInfo.withCurrentCulture culture
        subject.Should().NotContain(substring, comparison)


    [<Theory>]
    [<InlineData("a", "", StringComparison.Ordinal, "")>]
    [<InlineData("a", "a", StringComparison.Ordinal, "")>]
    [<InlineData("asd", "s", StringComparison.Ordinal, "")>]
    [<InlineData("asd", "S", StringComparison.OrdinalIgnoreCase, "")>]
    [<InlineData("ASD", "s", StringComparison.OrdinalIgnoreCase, "")>]
    [<InlineData("i", "İ", StringComparison.CurrentCultureIgnoreCase, "tr-TR")>] // Different casings of same letter in Turkish, but not invariant
    let ``Fails if substring is empty or string contains substring using the specified StringComparison``
        (subject: string)
        (substring: string)
        (comparison: StringComparison)
        (culture: string)
        =
        use _ = CultureInfo.withCurrentCulture culture
        assertFails (fun () -> subject.Should().NotContain(substring, comparison))


    [<Fact>]
    let ``Throws ArgumentNullException if substring is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> "".Should().NotContain(null, StringComparison.Ordinal) |> ignore)


    [<Fact>]
    let ``Fails with expected message using StringComparison.Ordinal`` () =
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
    let ``Fails with expected message using StringComparison.CurrentCulture`` () =
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
        "a".Should().NotContain("b").Id<And<string>>().And.Be("a")


    [<Theory>]
    [<InlineData(null, "")>]
    [<InlineData("a", "A")>]
    [<InlineData("asd", "f")>]
    let ``Passes if null or not containing substring`` (subject: string) (substring: string) =
        subject.Should().NotContain(substring)


    [<Theory>]
    [<InlineData("a", "")>]
    [<InlineData("a", "a")>]
    [<InlineData("asd", "s")>]
    let ``Fails if substring is empty or string contains substring`` (subject: string) (substring: string) =
        assertFails (fun () -> subject.Should().NotContain(substring))


    [<Fact>]
    let ``Throws ArgumentNullException if substring is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> "".Should().NotContain(null) |> ignore)


    [<Fact>]
    let ``Fails with expected message`` () =
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
        "a"
            .Should()
            .StartWith("a", StringComparison.Ordinal)
            .Id<And<string>>()
            .And.Be("a")


    [<Theory>]
    [<InlineData("a", "", StringComparison.Ordinal, "")>]
    [<InlineData("a", "a", StringComparison.Ordinal, "")>]
    [<InlineData("asd", "a", StringComparison.Ordinal, "")>]
    [<InlineData("asd", "A", StringComparison.OrdinalIgnoreCase, "")>]
    [<InlineData("ASD", "a", StringComparison.OrdinalIgnoreCase, "")>]
    [<InlineData("i", "İ", StringComparison.CurrentCultureIgnoreCase, "tr-TR")>] // Different casings of same letter in Turkish, but not invariant
    let ``Passes if substring is empty or string starts with substring using the specified StringComparison``
        (subject: string)
        (substring: string)
        (comparison: StringComparison)
        (culture: string)
        =
        use _ = CultureInfo.withCurrentCulture culture
        subject.Should().StartWith(substring, comparison)


    [<Theory>]
    [<InlineData(null, "", StringComparison.Ordinal, "")>]
    [<InlineData("a", "A", StringComparison.Ordinal, "")>]
    [<InlineData("asd", "s", StringComparison.Ordinal, "")>]
    [<InlineData("i", "İ", StringComparison.InvariantCultureIgnoreCase, "")>] // Different casings of same letter in Turkish, but not invariant
    let ``Fails if null or not starting with substring using the specified StringComparison``
        (subject: string)
        (substring: string)
        (comparison: StringComparison)
        (culture: string)
        =
        use _ = CultureInfo.withCurrentCulture culture
        assertFails (fun () -> subject.Should().StartWith(substring, comparison))


    [<Fact>]
    let ``Throws ArgumentNullException if substring is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> "".Should().StartWith(null, StringComparison.Ordinal) |> ignore)


    [<Fact>]
    let ``Fails with expected message using StringComparison.Ordinal`` () =
        fun () ->
            let x = "asd"
            x.Should().StartWith("f", StringComparison.Ordinal)
        |> assertExnMsg
            """
Subject: x
Should: StartWith
Substring: f
StringComparison: Ordinal
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message using StringComparison.CurrentCulture`` () =
        use _ = CultureInfo.withCurrentCulture "nb-NO"

        fun () ->
            let x = "asd"
            x.Should().StartWith("f", StringComparison.CurrentCulture)
        |> assertExnMsg
            """
Subject: x
Should: StartWith
Substring: f
StringComparison: CurrentCulture
CurrentCulture: nb-NO
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().StartWith("f", StringComparison.Ordinal, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: StartWith
Substring: f
StringComparison: Ordinal
But was: asd
"""


module ``StartWith without StringComparison`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "a".Should().StartWith("a").Id<And<string>>().And.Be("a")


    [<Theory>]
    [<InlineData("a", "")>]
    [<InlineData("a", "a")>]
    [<InlineData("asd", "a")>]
    let ``Passes if substring is empty or string starts with substring`` (subject: string) (substring: string) =
        subject.Should().StartWith(substring)


    [<Theory>]
    [<InlineData(null, "")>]
    [<InlineData("a", "A")>]
    [<InlineData("asd", "s")>]
    let ``Fails if null or not starting with substring`` (subject: string) (substring: string) =
        assertFails (fun () -> subject.Should().StartWith(substring))


    [<Fact>]
    let ``Throws ArgumentNullException if substring is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> "".Should().StartWith(null) |> ignore)


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = "asd"
            x.Should().StartWith("f")
        |> assertExnMsg
            """
Subject: x
Should: StartWith
Substring: f
StringComparison: Ordinal
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().StartWith("f", "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: StartWith
Substring: f
StringComparison: Ordinal
But was: asd
"""


module ``NotStartWith with StringComparison`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "a"
            .Should()
            .NotStartWith("b", StringComparison.Ordinal)
            .Id<And<string>>()
            .And.Be("a")


    [<Theory>]
    [<InlineData(null, "", StringComparison.Ordinal, "")>]
    [<InlineData("a", "A", StringComparison.Ordinal, "")>]
    [<InlineData("asd", "s", StringComparison.Ordinal, "")>]
    [<InlineData("i", "İ", StringComparison.InvariantCultureIgnoreCase, "")>] // Different casings of same letter in Turkish, but not invariant
    let ``Passes if null or not starting with substring using the specified StringComparison``
        (subject: string)
        (substring: string)
        (comparison: StringComparison)
        (culture: string)
        =
        use _ = CultureInfo.withCurrentCulture culture
        subject.Should().NotStartWith(substring, comparison)


    [<Theory>]
    [<InlineData("a", "", StringComparison.Ordinal, "")>]
    [<InlineData("a", "a", StringComparison.Ordinal, "")>]
    [<InlineData("asd", "a", StringComparison.Ordinal, "")>]
    [<InlineData("asd", "A", StringComparison.OrdinalIgnoreCase, "")>]
    [<InlineData("ASD", "a", StringComparison.OrdinalIgnoreCase, "")>]
    [<InlineData("i", "İ", StringComparison.CurrentCultureIgnoreCase, "tr-TR")>] // Different casings of same letter in Turkish, but not invariant
    let ``Fails if substring is empty or string starts with substring using the specified StringComparison``
        (subject: string)
        (substring: string)
        (comparison: StringComparison)
        (culture: string)
        =
        use _ = CultureInfo.withCurrentCulture culture
        assertFails (fun () -> subject.Should().NotStartWith(substring, comparison))


    [<Fact>]
    let ``Throws ArgumentNullException if substring is null`` () =
        Assert.Throws<ArgumentNullException>(fun () ->
            "".Should().NotStartWith(null, StringComparison.Ordinal) |> ignore
        )


    [<Fact>]
    let ``Fails with expected message using StringComparison.Ordinal`` () =
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
    let ``Fails with expected message using StringComparison.CurrentCulture`` () =
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
        "a".Should().NotStartWith("b").Id<And<string>>().And.Be("a")


    [<Theory>]
    [<InlineData(null, "")>]
    [<InlineData("a", "A")>]
    [<InlineData("asd", "s")>]
    let ``Passes if null or not starting with substring`` (subject: string) (substring: string) =
        subject.Should().NotStartWith(substring)


    [<Theory>]
    [<InlineData("a", "")>]
    [<InlineData("a", "a")>]
    [<InlineData("asd", "a")>]
    let ``Fails if substring is empty or string starts with substring`` (subject: string) (substring: string) =
        assertFails (fun () -> subject.Should().NotStartWith(substring))


    [<Fact>]
    let ``Throws ArgumentNullException if substring is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> "".Should().NotStartWith(null) |> ignore)


    [<Fact>]
    let ``Fails with expected message`` () =
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
        "a"
            .Should()
            .EndWith("a", StringComparison.Ordinal)
            .Id<And<string>>()
            .And.Be("a")


    [<Theory>]
    [<InlineData("a", "", StringComparison.Ordinal, "")>]
    [<InlineData("a", "a", StringComparison.Ordinal, "")>]
    [<InlineData("asd", "d", StringComparison.Ordinal, "")>]
    [<InlineData("asd", "D", StringComparison.OrdinalIgnoreCase, "")>]
    [<InlineData("ASD", "d", StringComparison.OrdinalIgnoreCase, "")>]
    [<InlineData("i", "İ", StringComparison.CurrentCultureIgnoreCase, "tr-TR")>] // Different casings of same letter in Turkish, but not invariant
    let ``Passes if substring is empty or string ends with substring using the specified StringComparison``
        (subject: string)
        (substring: string)
        (comparison: StringComparison)
        (culture: string)
        =
        use _ = CultureInfo.withCurrentCulture culture
        subject.Should().EndWith(substring, comparison)


    [<Theory>]
    [<InlineData(null, "", StringComparison.Ordinal, "")>]
    [<InlineData("a", "A", StringComparison.Ordinal, "")>]
    [<InlineData("asd", "s", StringComparison.Ordinal, "")>]
    [<InlineData("i", "İ", StringComparison.InvariantCultureIgnoreCase, "")>] // Different casings of same letter in Turkish, but not invariant
    let ``Fails if null or not ending with substring using the specified StringComparison``
        (subject: string)
        (substring: string)
        (comparison: StringComparison)
        (culture: string)
        =
        use _ = CultureInfo.withCurrentCulture culture
        assertFails (fun () -> subject.Should().EndWith(substring, comparison))


    [<Fact>]
    let ``Throws ArgumentNullException if substring is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> "".Should().EndWith(null, StringComparison.Ordinal) |> ignore)


    [<Fact>]
    let ``Fails with expected message using StringComparison.Ordinal`` () =
        fun () ->
            let x = "asd"
            x.Should().EndWith("f", StringComparison.Ordinal)
        |> assertExnMsg
            """
Subject: x
Should: EndWith
Substring: f
StringComparison: Ordinal
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message using StringComparison.CurrentCulture`` () =
        use _ = CultureInfo.withCurrentCulture "nb-NO"

        fun () ->
            let x = "asd"
            x.Should().EndWith("f", StringComparison.CurrentCulture)
        |> assertExnMsg
            """
Subject: x
Should: EndWith
Substring: f
StringComparison: CurrentCulture
CurrentCulture: nb-NO
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().EndWith("f", StringComparison.Ordinal, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: EndWith
Substring: f
StringComparison: Ordinal
But was: asd
"""


module ``EndWith without StringComparison`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "a".Should().EndWith("a").Id<And<string>>().And.Be("a")


    [<Theory>]
    [<InlineData("a", "")>]
    [<InlineData("a", "a")>]
    [<InlineData("asd", "d")>]
    let ``Passes if substring is empty or string ends with substring`` (subject: string) (substring: string) =
        subject.Should().EndWith(substring)


    [<Theory>]
    [<InlineData(null, "")>]
    [<InlineData("a", "A")>]
    [<InlineData("asd", "s")>]
    let ``Fails if null or not ending with substring`` (subject: string) (substring: string) =
        assertFails (fun () -> subject.Should().EndWith(substring))


    [<Fact>]
    let ``Throws ArgumentNullException if substring is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> "".Should().EndWith(null) |> ignore)


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = "asd"
            x.Should().EndWith("f")
        |> assertExnMsg
            """
Subject: x
Should: EndWith
Substring: f
StringComparison: Ordinal
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().EndWith("f", "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: EndWith
Substring: f
StringComparison: Ordinal
But was: asd
"""


module ``NotEndWith with StringComparison`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "a"
            .Should()
            .NotEndWith("b", StringComparison.Ordinal)
            .Id<And<string>>()
            .And.Be("a")


    [<Theory>]
    [<InlineData(null, "", StringComparison.Ordinal, "")>]
    [<InlineData("a", "A", StringComparison.Ordinal, "")>]
    [<InlineData("asd", "s", StringComparison.Ordinal, "")>]
    [<InlineData("i", "İ", StringComparison.InvariantCultureIgnoreCase, "")>] // Different casings of same letter in Turkish, but not invariant
    let ``Passes if null or not ending with substring using the specified StringComparison``
        (subject: string)
        (substring: string)
        (comparison: StringComparison)
        (culture: string)
        =
        use _ = CultureInfo.withCurrentCulture culture
        subject.Should().NotEndWith(substring, comparison)


    [<Theory>]
    [<InlineData("a", "", StringComparison.Ordinal, "")>]
    [<InlineData("a", "a", StringComparison.Ordinal, "")>]
    [<InlineData("asd", "d", StringComparison.Ordinal, "")>]
    [<InlineData("asd", "D", StringComparison.OrdinalIgnoreCase, "")>]
    [<InlineData("ASD", "d", StringComparison.OrdinalIgnoreCase, "")>]
    [<InlineData("i", "İ", StringComparison.CurrentCultureIgnoreCase, "tr-TR")>] // Different casings of same letter in Turkish, but not invariant
    let ``Fails if substring is empty or string ends with substring using the specified StringComparison``
        (subject: string)
        (substring: string)
        (comparison: StringComparison)
        (culture: string)
        =
        use _ = CultureInfo.withCurrentCulture culture
        assertFails (fun () -> subject.Should().NotEndWith(substring, comparison))


    [<Fact>]
    let ``Throws ArgumentNullException if substring is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> "".Should().NotEndWith(null, StringComparison.Ordinal) |> ignore)


    [<Fact>]
    let ``Fails with expected message using StringComparison.Ordinal`` () =
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
    let ``Fails with expected message using StringComparison.CurrentCulture`` () =
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
        "a".Should().NotEndWith("b").Id<And<string>>().And.Be("a")


    [<Theory>]
    [<InlineData(null, "")>]
    [<InlineData("a", "A")>]
    [<InlineData("asd", "s")>]
    let ``Passes if null or not ending with substring`` (subject: string) (substring: string) =
        subject.Should().NotEndWith(substring)


    [<Theory>]
    [<InlineData("a", "")>]
    [<InlineData("a", "a")>]
    [<InlineData("asd", "d")>]
    let ``Fails if substring is empty or string ends with substring`` (subject: string) (substring: string) =
        assertFails (fun () -> subject.Should().NotEndWith(substring))


    [<Fact>]
    let ``Throws ArgumentNullException if substring is null`` () =
        Assert.Throws<ArgumentNullException>(fun () -> "".Should().NotEndWith(null) |> ignore)


    [<Fact>]
    let ``Fails with expected message`` () =
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
        "a".Should().MatchRegex(Regex(".*")).Id<And<string>>().And.Be("a")


    [<Theory>]
    [<InlineData("", ".*", RegexOptions.None)>]
    [<InlineData("asd", ".*", RegexOptions.None)>]
    [<InlineData("asd", "^asd$", RegexOptions.None)>]
    [<InlineData("asd", "^ASD$", RegexOptions.IgnoreCase)>]
    let ``Passes if string matches regex`` (subject: string) (pattern: string) (options: RegexOptions) =
        subject.Should().MatchRegex(Regex(pattern, options))


    [<Theory>]
    [<InlineData(null, ".*", RegexOptions.None)>]
    [<InlineData("", ".+", RegexOptions.None)>]
    [<InlineData("asd", "^as$", RegexOptions.None)>]
    [<InlineData("asd", "^ASD$", RegexOptions.None)>]
    let ``Fails if null or not matching regex`` (subject: string) (pattern: string) (options: RegexOptions) =
        assertFails (fun () -> subject.Should().MatchRegex(Regex(pattern, options)))


    [<Fact>]
    let ``Fails with expected message`` () =
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
    let ``Fails with expected message using custom RegexOptions`` () =
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
        "a".Should().MatchRegex(".*", RegexOptions.None).Id<And<string>>().And.Be("a")


    [<Theory>]
    [<InlineData("", ".*", RegexOptions.None)>]
    [<InlineData("asd", ".*", RegexOptions.None)>]
    [<InlineData("asd", "^asd$", RegexOptions.None)>]
    [<InlineData("asd", "^ASD$", RegexOptions.IgnoreCase)>]
    let ``Passes if string matches pattern with options`` (subject: string) (pattern: string) (options: RegexOptions) =
        subject.Should().MatchRegex(pattern, options)


    [<Theory>]
    [<InlineData(null, ".*", RegexOptions.None)>]
    [<InlineData("", ".+", RegexOptions.None)>]
    [<InlineData("asd", "^as$", RegexOptions.None)>]
    [<InlineData("asd", "^ASD$", RegexOptions.None)>]
    let ``Fails if null or not matching pattern with options``
        (subject: string)
        (pattern: string)
        (options: RegexOptions)
        =
        assertFails (fun () -> subject.Should().MatchRegex(pattern, options))


    [<Fact>]
    let ``Fails with expected message using default RegexOptions`` () =
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
    let ``Fails with expected message using custom RegexOptions`` () =
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
        "a".Should().MatchRegex(".*").Id<And<string>>().And.Be("a")


    [<Theory>]
    [<InlineData("", ".*")>]
    [<InlineData("asd", ".*")>]
    [<InlineData("asd", "^asd$")>]
    let ``Passes if string matches pattern`` (subject: string) (pattern: string) = subject.Should().MatchRegex(pattern)


    [<Theory>]
    [<InlineData(null, ".*")>]
    [<InlineData("", ".+")>]
    [<InlineData("asd", "^as$")>]
    [<InlineData("asd", "^ASD$")>]
    let ``Fails if null or not matching pattern with options`` (subject: string) (pattern: string) =
        assertFails (fun () -> subject.Should().MatchRegex(pattern))


    [<Fact>]
    let ``Fails with expected message`` () =
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
        "a".Should().NotMatchRegex(Regex("b.*")).Id<And<string>>().And.Be("a")


    [<Theory>]
    [<InlineData(null, ".*", RegexOptions.None)>]
    [<InlineData("", ".+", RegexOptions.None)>]
    [<InlineData("asd", "^as$", RegexOptions.None)>]
    [<InlineData("asd", "^ASD$", RegexOptions.None)>]
    let ``Passes if null or not matching regex`` (subject: string) (pattern: string) (options: RegexOptions) =
        subject.Should().NotMatchRegex(Regex(pattern, options))


    [<Theory>]
    [<InlineData("", ".*", RegexOptions.None)>]
    [<InlineData("asd", ".*", RegexOptions.None)>]
    [<InlineData("asd", "^asd$", RegexOptions.None)>]
    [<InlineData("asd", "^ASD$", RegexOptions.IgnoreCase)>]
    let ``Fails if string matches regex`` (subject: string) (pattern: string) (options: RegexOptions) =
        assertFails (fun () -> subject.Should().NotMatchRegex(Regex(pattern, options)))


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = "asd"
            x.Should().NotMatchRegex(Regex("a.*"))
        |> assertExnMsg
            """
Subject: x
Should: NotMatchRegex
Pattern: a.*
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message using custom RegexOptions`` () =
        fun () ->
            let x = "asd"

            x
                .Should()
                .NotMatchRegex(Regex("a.*", RegexOptions.IgnoreCase ||| RegexOptions.Multiline))
        |> assertExnMsg
            """
Subject: x
Should: NotMatchRegex
Pattern: a.*
RegexOptions: IgnoreCase, Multiline
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().NotMatchRegex(Regex("a.*"), "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotMatchRegex
Pattern: a.*
But was: asd
"""


module ``NotMatchRegex with string and options`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "a"
            .Should()
            .NotMatchRegex("b.*", RegexOptions.None)
            .Id<And<string>>()
            .And.Be("a")


    [<Theory>]
    [<InlineData(null, ".*", RegexOptions.None)>]
    [<InlineData("", ".+", RegexOptions.None)>]
    [<InlineData("asd", "^as$", RegexOptions.None)>]
    [<InlineData("asd", "^ASD$", RegexOptions.None)>]
    let ``Passes if null or not matching pattern with options``
        (subject: string)
        (pattern: string)
        (options: RegexOptions)
        =
        subject.Should().NotMatchRegex(pattern, options)


    [<Theory>]
    [<InlineData("", ".*", RegexOptions.None)>]
    [<InlineData("asd", ".*", RegexOptions.None)>]
    [<InlineData("asd", "^asd$", RegexOptions.None)>]
    [<InlineData("asd", "^ASD$", RegexOptions.IgnoreCase)>]
    let ``Fails if string matches pattern with options`` (subject: string) (pattern: string) (options: RegexOptions) =
        assertFails (fun () -> subject.Should().NotMatchRegex(pattern, options))


    [<Fact>]
    let ``Fails with expected message using default RegexOptions`` () =
        fun () ->
            let x = "asd"
            x.Should().NotMatchRegex("a.*", RegexOptions.None)
        |> assertExnMsg
            """
Subject: x
Should: NotMatchRegex
Pattern: a.*
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message using custom RegexOptions`` () =
        fun () ->
            let x = "asd"

            x
                .Should()
                .NotMatchRegex("a.*", RegexOptions.IgnoreCase ||| RegexOptions.Multiline)
        |> assertExnMsg
            """
Subject: x
Should: NotMatchRegex
Pattern: a.*
RegexOptions: IgnoreCase, Multiline
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().NotMatchRegex("a.*", RegexOptions.None, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotMatchRegex
Pattern: a.*
But was: asd
"""


module ``NotMatchRegex with string`` =


    [<Fact>]
    let ``Can be chained with And`` () =
        "a".Should().NotMatchRegex("b.*").Id<And<string>>().And.Be("a")


    [<Theory>]
    [<InlineData(null, ".*")>]
    [<InlineData("", ".+")>]
    [<InlineData("asd", "^as$")>]
    [<InlineData("asd", "^ASD$")>]
    let ``Passes if null or not matching pattern`` (subject: string) (pattern: string) =
        subject.Should().NotMatchRegex(pattern)


    [<Theory>]
    [<InlineData("", ".*")>]
    [<InlineData("asd", ".*")>]
    [<InlineData("asd", "^asd$")>]
    let ``Fails if string matches pattern`` (subject: string) (pattern: string) =
        assertFails (fun () -> subject.Should().NotMatchRegex(pattern))


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = "asd"
            x.Should().NotMatchRegex("a.*")
        |> assertExnMsg
            """
Subject: x
Should: NotMatchRegex
Pattern: a.*
But was: asd
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().NotMatchRegex("a.*", "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: NotMatchRegex
Pattern: a.*
But was: asd
"""
