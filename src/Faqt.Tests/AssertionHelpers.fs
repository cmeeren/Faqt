module AssertionHelpers

open Faqt
open AssertionHelpers
open Xunit


module bc =


    [<Fact>]
    let ``Prefixes with "because " if not already present`` () =
        Assert.Equal("because some reason", bc "some reason")


    [<Fact>]
    let ``Does not prefix with "because " if already present`` () =
        Assert.Equal("because some reason", bc "because some reason")


    [<Fact>]
    let ``Returns empty string for empty input`` () = Assert.Empty(bc "")


module bcc =


    [<Fact>]
    let ``Prefixes with "because " if not already present, and suffixes with ", "`` () =
        Assert.Equal("because some reason, ", bcc "some reason")


    [<Fact>]
    let ``Does not prefix with "because " if already present, and suffixes with ", "`` () =
        Assert.Equal("because some reason, ", bcc "because some reason")


    [<Fact>]
    let ``Returns empty string for empty input`` () = Assert.Empty(bcc "")


module sbc =


    [<Fact>]
    let ``Prefixes with " because " if "because " is not already present`` () =
        Assert.Equal(" because some reason", sbc "some reason")


    [<Fact>]
    let ``Prefixes with " " if "because " is already present`` () =
        Assert.Equal(" because some reason", sbc "because some reason")


    [<Fact>]
    let ``Returns empty string for empty input`` () = Assert.Empty(sbc "")


module sbcc =


    [<Fact>]
    let ``Prefixes with " because " if "because " is not already present, and suffixes with ", "`` () =
        Assert.Equal(" because some reason, ", sbcc "some reason")


    [<Fact>]
    let ``Prefixes with " " if "because " is already present, and suffixes with ", "`` () =
        Assert.Equal(" because some reason, ", sbcc "because some reason")


    [<Fact>]
    let ``Returns empty string for empty input`` () = Assert.Empty(sbcc "")
