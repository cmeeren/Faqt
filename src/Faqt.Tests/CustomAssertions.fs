module ``Custom assertions``

open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open Faqt
open Faqt.AssertionHelpers
open Formatting
open Xunit


[<Extension>]
type private Assertions =


    [<Extension>]
    static member DelegatingFail(t: Testable<'a>) : And<'a> =
        use _ = t.Assert()
        t.Subject.Should(t).Fail()


    [<Extension>]
    static member DelegatingFailSatisfy(t: Testable<int>) : And<int> =
        use _ = t.Assert()
        t.TestSatisfy(fun x -> x.Should().Fail())


    [<Extension>]
    static member NotInvade
        (
            t: Testable<string>,
            target: string,
            [<Optional; DefaultParameterValue("")>] because: string
        ) : And<string> =
        use _ = t.Assert()

        if t.Subject = "Russia" && target = "Ukraine" then
            t.Fail(
                "\tExpected\n{subject}\n\tto not invade\n{0}\n\t{because}but an invasion was found to be taking place by\n{actual}",
                because,
                format target
            )

        And(t)


[<Fact>]
let ``DelegatingFail gives expected subject name`` () =
    fun () -> "asd".Should().DelegatingFail()
    |> assertExnMsg "\"asd\""


[<Fact>]
let ``DelegatingFailSatisfy gives expected subject name`` () =
    fun () -> (1).Should().DelegatingFailSatisfy()
    |> assertExnMsg
        """
(1)
x
"""


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
