module ``Custom assertions``

open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open Faqt
open AssertionHelpers
open Formatting
open Xunit


[<Extension>]
type private Assertions =


    [<Extension>]
    static member DelegatingFail(t: Testable<'a>, ?methodNameOverride) : And<'a> =
        t.Subject
            .Should(t)
            .Fail(defaultArg methodNameOverride (nameof Assertions.DelegatingFail))


    [<Extension>]
    static member NotInvade
        (
            t: Testable<string>,
            target: string,
            [<Optional; DefaultParameterValue("")>] because,
            ?methodNameOverride
        ) : And<string> =
        if t.Subject = "Russia" && target = "Ukraine" then
            Fail(t, because, methodNameOverride)
                .Throw(
                    "\tExpected\n{subject}\n\tto not invade\n{0}\n\t{because}but an invasion was found to be taking place by\n{actual}",
                    format target
                )

        And(t)


[<Fact>]
let ``DelegatingFail gives expected subject name`` () =
    fun () -> "asd".Should().DelegatingFail()
    |> assertExnMsg "\"asd\""


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
