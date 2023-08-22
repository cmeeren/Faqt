module ``Custom assertions``

open System
open System.Runtime.CompilerServices
open Faqt
open AssertionHelpers
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
        t.Satisfy(fun x -> x.Should().Fail())


    [<Extension>]
    static member FailWithoutAssert<'a>(t: Testable<'a>) : And<'a> =
        t.Fail(None)
        And(t)


    [<Extension>]
    static member NotInvade(t: Testable<string>, target: string, ?because) : And<string> =
        use _ = t.Assert()

        if t.Subject = "Russia" && target = "Ukraine" then
            t.With("Target", target).With("But was invaded by", t.Subject).Fail(because)

        And(t)


[<Fact>]
let ``DelegatingFail gives expected subject name`` () =
    fun () -> "asd".Should().DelegatingFail()
    |> assertExnMsg
        """
Subject: '"asd"'
Should: DelegatingFail
"""


[<Fact>]
let ``DelegatingFailSatisfy gives expected subject name`` () =
    fun () -> (1).Should().DelegatingFailSatisfy()
    |> assertExnMsg
        """
Subject: (1)
Should: DelegatingFailSatisfy
Failure:
  Subject: x
  Should: Fail
"""


[<Fact>]
let ``FailWithoutAssert throws expected exception`` () =
    let ex =
        Assert.Throws<InvalidOperationException>(fun () -> (1).Should().FailWithoutAssert() |> ignore)

    Assert.Equal(
        "Call chain assertion history is empty. Testable.Assert must be called in all assertions before calling Fail.",
        ex.Message
    )


[<Fact>]
let ``Custom assertion gives expected message`` () =
    fun () ->
        let country = "Russia"
        country.Should().NotInvade("Ukraine")
    |> assertExnMsg
        """
Subject: country
Should: NotInvade
Target: Ukraine
But was invaded by: Russia
"""
