module ``Custom assertions``

open System.Runtime.CompilerServices
open Faqt
open AssertionHelpers
open Xunit


[<FaqtAssertion>]
let private failHelperWithAttribute (t: Testable<'a>) = t.Subject.Should().Fail()


let private failHelperWithoutAttribute (t: Testable<'a>) = t.Subject.Should().Fail()


[<ContainsFaqtAssertions>]
module private FailHelperModuleWithAttribute =


    let helper (t: Testable<'a>) = t.Subject.Should().Fail()


[<Extension; ContainsFaqtAssertions>]
type private AssertionsWithFaqtAssertions =


    [<Extension>]
    static member CustomFailWithClassAttribute(t: Testable<'a>) : And<'a> = t.Subject.Should().Fail()


    [<Extension>]
    static member CustomFailWithClassAttributeAndHelperWithModuleAttribute(t: Testable<'a>) : And<'a> =
        FailHelperModuleWithAttribute.helper t


    [<Extension>]
    static member NotInvade(t: Testable<string>, target: string, ?because) : And<string> =
        if t.Subject = "Russia" && target = "Ukraine" then
            fail
                $"\tExpected\n{sub ()}\n\tto not invade\n{fmt target}\n\t{bcc because}but an invasion was found to be taking place by\n{fmt t.Subject}"

        And(t)


[<Extension>]
type private AssertionsWithIndividualFaqtAssertion =


    [<Extension; FaqtAssertion>]
    static member CustomFailWithMethodAttribute(t: Testable<'a>) : And<'a> = t.Subject.Should().Fail()


    [<Extension>]
    static member CustomFailWithoutAttributes(t: Testable<'a>) : And<'a> = t.Subject.Should().Fail()


    [<Extension; FaqtAssertion>]
    static member CustomFailWithHelperWithFunctionAttributeAndMethodAttribute(t: Testable<'a>) : And<'a> =
        failHelperWithAttribute t


    [<Extension; FaqtAssertion>]
    static member CustomFailWithHelperWithoutFunctionAttributeWithMethodAttribute(t: Testable<'a>) : And<'a> =
        failHelperWithoutAttribute t


    [<Extension>]
    static member CustomFailWithHelperWithFunctionAttributeWithoutMethodAttribute(t: Testable<'a>) : And<'a> =
        failHelperWithAttribute t


    [<Extension>]
    static member CustomFailWithHelperWithoutFunctionAttributeOrMethodAttribute(t: Testable<'a>) : And<'a> =
        failHelperWithoutAttribute t


[<Fact>]
let ``CustomFailWithClassAttribute gives expected subject name`` () =
    fun () -> "asd".Should().CustomFailWithClassAttribute()
    |> assertExnMsg "\"asd\""


[<Fact>]
let ``CustomFailWithClassAttributeAndHelperWithModuleAttribute gives expected subject name`` () =
    fun () -> "asd".Should().CustomFailWithClassAttributeAndHelperWithModuleAttribute()
    |> assertExnMsg "\"asd\""


[<Fact>]
let ``CustomFailWithMethodAttribute gives expected subject name`` () =
    fun () -> "asd".Should().CustomFailWithMethodAttribute()
    |> assertExnMsg "\"asd\""


// Note: This test serves to document "undefined" behavior, not enforce a requirement. Update as needed if it fails.
[<Fact>]
let ``CustomFailWithoutAttributes gives expected subject name`` () =
    fun () -> "asd".Should().CustomFailWithoutAttributes()
    |> assertExnMsg "t"


[<Fact>]
let ``CustomFailWithHelperWithFunctionAttributeAndMethodAttribute gives expected subject name`` () =
    fun () -> "asd".Should().CustomFailWithHelperWithFunctionAttributeAndMethodAttribute()
    |> assertExnMsg "\"asd\""


// Note: This test serves to document "undefined" behavior, not enforce a requirement. Update as needed if it fails.
[<Fact>]
let ``CustomFailWithHelperWithoutFunctionAttributeWithMethodAttribute gives expected subject name`` () =
    fun () -> "asd".Should().CustomFailWithHelperWithoutFunctionAttributeWithMethodAttribute()
    |> assertExnMsg "t"


// Note: This test serves to document "undefined" behavior, not enforce a requirement. Update as needed if it fails.
[<Fact>]
let ``CustomFailWithHelperWithFunctionAttributeWithoutMethodAttribute gives expected subject name`` () =
    fun () -> "asd".Should().CustomFailWithHelperWithFunctionAttributeWithoutMethodAttribute()
    |> assertExnMsg "failHelperWithAttribute t"


// Note: This test serves to document "undefined" behavior, not enforce a requirement. Update as needed if it fails.
[<Fact>]
let ``CustomFailWithHelperWithoutFunctionAttributeOrMethodAttribute gives expected subject name`` () =
    fun () -> "asd".Should().CustomFailWithHelperWithoutFunctionAttributeOrMethodAttribute()
    |> assertExnMsg "t"


[<Fact>]
let ``Custom assertion gives expected message`` () =
    fun () ->
        let country = "Russia"
        country.Should().NotInvade("Ukraine")
    |> assertExnMsg
        """
    Expected
country
    to not invade
"Ukraine"
    but an invasion was found to be taking place by
"Russia"
"""
