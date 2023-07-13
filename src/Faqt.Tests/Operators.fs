module Operators

open Faqt
open Faqt.Operators
open Xunit


[<Fact>]
let ``Ignore operator works`` () =
    %"a".Should().Be("a")
    %"a".Should().Be("a")
