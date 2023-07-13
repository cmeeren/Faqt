module AssertionHelpers

open Faqt
open AssertionHelpers
open Xunit


module bc =


    [<Fact>]
    let ``Prefixes with "because " if not already present`` () =
        Assert.Equal("because some reason", bc (Some "some reason"))


    [<Fact>]
    let ``Does not prefix with "because " if already present`` () =
        Assert.Equal("because some reason", bc (Some "because some reason"))


    [<Fact>]
    let ``Returns empty string for None input`` () = Assert.Empty(bc None)


module bcc =


    [<Fact>]
    let ``Prefixes with "because " if not already present, and suffixes with ", "`` () =
        Assert.Equal("because some reason, ", bcc (Some "some reason"))


    [<Fact>]
    let ``Does not prefix with "because " if already present, and suffixes with ", "`` () =
        Assert.Equal("because some reason, ", bcc (Some "because some reason"))


    [<Fact>]
    let ``Returns empty string for None input`` () = Assert.Empty(bcc None)


module sbc =


    [<Fact>]
    let ``Prefixes with " because " if "because " is not already present`` () =
        Assert.Equal(" because some reason", sbc (Some "some reason"))


    [<Fact>]
    let ``Prefixes with " " if "because " is already present`` () =
        Assert.Equal(" because some reason", sbc (Some "because some reason"))


    [<Fact>]
    let ``Returns empty string for None input`` () = Assert.Empty(sbc None)


module sbcc =


    [<Fact>]
    let ``Prefixes with " because " if "because " is not already present, and suffixes with ", "`` () =
        Assert.Equal(" because some reason, ", sbcc (Some "some reason"))


    [<Fact>]
    let ``Prefixes with " " if "because " is already present, and suffixes with ", "`` () =
        Assert.Equal(" because some reason, ", sbcc (Some "because some reason"))


    [<Fact>]
    let ``Returns empty string for None input`` () = Assert.Empty(sbcc None)
