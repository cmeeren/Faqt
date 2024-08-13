module HttpResponseMessageAssertions

#nowarn "760"

open System
open System.Net
open System.Net.Http
open Faqt
open Xunit


let resp statusCode =
    let version = Version.Parse("0.5")
    let response = HttpResponseMessage(enum statusCode)
    response.Version <- version
    let request = HttpRequestMessage(HttpMethod.Get, "/")
    request.Version <- version
    response.RequestMessage <- request
    response


let respContent statusCode content =
    let response = resp statusCode
    response.Content <- new StringContent(content)
    response


let respHeader statusCode (headerAndValues: (string * string) list) =
    let response = resp statusCode

    for h, v in headerAndValues do
        if not (response.Headers.TryAddWithoutValidation(h, v)) then
            response.Content <- StringContent("ignored")

            if not (response.Content.Headers.TryAddWithoutValidation(h, v)) then
                failwith $"Unable to add header '%s{h}' with value '%s{v}'"

    response


module HaveStatusCode =


    [<Fact>]
    let ``Can be chained with And`` () =
        (resp 200)
            .Should()
            .HaveStatusCode(HttpStatusCode.OK)
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.OK)


    [<Theory>]
    [<InlineData(201, 201)>]
    [<InlineData(404, 404)>]
    let ``Passes if the status code is the same`` actual expected =
        (resp actual).Should().HaveStatusCode(expected)


    [<Theory>]
    [<InlineData(200, 201)>]
    [<InlineData(400, 404)>]
    let ``Fails if the status code is different`` actual expected =
        assertFails (fun () -> (resp actual).Should().HaveStatusCode(expected))


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = resp 201
            x.Should().HaveStatusCode(HttpStatusCode.Accepted)
        |> assertExnMsg
            """
Subject: x
Should: HaveStatusCode
Expected: 202 Accepted
But was: 201 Created
Response: HTTP/0.5 201 Created
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = resp 201
            x.Should().HaveStatusCode(HttpStatusCode.Accepted, "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: HaveStatusCode
Expected: 202 Accepted
But was: 201 Created
Response: HTTP/0.5 201 Created
Request: GET / HTTP/0.5
"""


module Be1XXInformational =


    [<Fact>]
    let ``Can be chained with And`` () =
        (resp 100)
            .Should()
            .Be1XXInformational()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.Continue)


    [<Theory>]
    [<InlineData(100)>]
    [<InlineData(199)>]
    let ``Passes if the status code is 1XX`` statusCode =
        (resp statusCode).Should().Be1XXInformational()


    [<Theory>]
    [<InlineData(200)>]
    [<InlineData(300)>]
    [<InlineData(400)>]
    [<InlineData(500)>]
    [<InlineData(900)>]
    let ``Fails if the status code is not 1XX`` statusCode =
        assertFails (fun () -> (resp statusCode).Should().Be1XXInformational())


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = (resp 201)
            x.Should().Be1XXInformational()
        |> assertExnMsg
            """
Subject: x
Should: Be1XXInformational
But was: 201 Created
Response: HTTP/0.5 201 Created
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = (resp 201)
            x.Should().Be1XXInformational("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be1XXInformational
But was: 201 Created
Response: HTTP/0.5 201 Created
Request: GET / HTTP/0.5
"""


module Be2XXSuccessful =


    [<Fact>]
    let ``Can be chained with And`` () =
        (resp 200)
            .Should()
            .Be2XXSuccessful()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.OK)


    [<Theory>]
    [<InlineData(200)>]
    [<InlineData(299)>]
    let ``Passes if the status code is 2XX`` statusCode =
        (resp statusCode).Should().Be2XXSuccessful()


    [<Theory>]
    [<InlineData(100)>]
    [<InlineData(300)>]
    [<InlineData(400)>]
    [<InlineData(500)>]
    [<InlineData(900)>]
    let ``Fails if the status code is not 2XX`` statusCode =
        assertFails (fun () -> (resp statusCode).Should().Be2XXSuccessful())


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = resp 100
            x.Should().Be2XXSuccessful()
        |> assertExnMsg
            """
Subject: x
Should: Be2XXSuccessful
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = resp 100
            x.Should().Be2XXSuccessful("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be2XXSuccessful
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be3XXRedirection =


    [<Fact>]
    let ``Can be chained with And`` () =
        (resp 300)
            .Should()
            .Be3XXRedirection()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.MultipleChoices)


    [<Theory>]
    [<InlineData(300)>]
    [<InlineData(399)>]
    let ``Passes if the status code is 3XX`` statusCode =
        (resp statusCode).Should().Be3XXRedirection()


    [<Theory>]
    [<InlineData(100)>]
    [<InlineData(200)>]
    [<InlineData(400)>]
    [<InlineData(500)>]
    [<InlineData(900)>]
    let ``Fails if the status code is not 3XX`` statusCode =
        assertFails (fun () -> (resp statusCode).Should().Be3XXRedirection())


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = resp 100
            x.Should().Be3XXRedirection()
        |> assertExnMsg
            """
Subject: x
Should: Be3XXRedirection
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = resp 100
            x.Should().Be3XXRedirection("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be3XXRedirection
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be4XXClientError =


    [<Fact>]
    let ``Can be chained with And`` () =
        (resp 400)
            .Should()
            .Be4XXClientError()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.BadRequest)


    [<Theory>]
    [<InlineData(400)>]
    [<InlineData(499)>]
    let ``Passes if the status code is 4XX`` statusCode =
        (resp statusCode).Should().Be4XXClientError()


    [<Theory>]
    [<InlineData(100)>]
    [<InlineData(200)>]
    [<InlineData(300)>]
    [<InlineData(500)>]
    [<InlineData(900)>]
    let ``Fails if the status code is not 4XX`` statusCode =
        assertFails (fun () -> (resp statusCode).Should().Be4XXClientError())


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = resp 100
            x.Should().Be4XXClientError()
        |> assertExnMsg
            """
Subject: x
Should: Be4XXClientError
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = resp 100
            x.Should().Be4XXClientError("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be4XXClientError
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be5XXServerError =


    [<Fact>]
    let ``Can be chained with And`` () =
        (resp 500)
            .Should()
            .Be5XXServerError()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.InternalServerError)


    [<Theory>]
    [<InlineData(500)>]
    [<InlineData(599)>]
    let ``Passes if the status code is 5XX`` statusCode =
        (resp statusCode).Should().Be5XXServerError()


    [<Theory>]
    [<InlineData(100)>]
    [<InlineData(200)>]
    [<InlineData(300)>]
    [<InlineData(400)>]
    [<InlineData(900)>]
    let ``Fails if the status code is not 5XX`` statusCode =
        assertFails (fun () -> (resp statusCode).Should().Be5XXServerError())


    [<Fact>]
    let ``Fails with expected message`` () =
        fun () ->
            let x = resp 100
            x.Should().Be5XXServerError()
        |> assertExnMsg
            """
Subject: x
Should: Be5XXServerError
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = resp 100
            x.Should().Be5XXServerError("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be5XXServerError
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be100Continue =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 100)
            .Should()
            .Be100Continue()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.Continue)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = (resp 201)
            x.Should().Be100Continue()
        |> assertExnMsg
            """
Subject: x
Should: Be100Continue
But was: 201 Created
Response: HTTP/0.5 201 Created
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = (resp 201)
            x.Should().Be100Continue("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be100Continue
But was: 201 Created
Response: HTTP/0.5 201 Created
Request: GET / HTTP/0.5
"""


module Be101SwitchingProtocols =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 101)
            .Should()
            .Be101SwitchingProtocols()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.SwitchingProtocols)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be101SwitchingProtocols()
        |> assertExnMsg
            """
Subject: x
Should: Be101SwitchingProtocols
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be101SwitchingProtocols("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be101SwitchingProtocols
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be200OK =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 200)
            .Should()
            .Be200Ok()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.OK)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be200Ok()
        |> assertExnMsg
            """
Subject: x
Should: Be200Ok
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be200Ok("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be200Ok
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be201Created =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 201)
            .Should()
            .Be201Created()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.Created)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be201Created()
        |> assertExnMsg
            """
Subject: x
Should: Be201Created
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be201Created("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be201Created
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be202Accepted =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 202)
            .Should()
            .Be202Accepted()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.Accepted)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be202Accepted()
        |> assertExnMsg
            """
Subject: x
Should: Be202Accepted
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be202Accepted("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be202Accepted
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be203NonAuthoritativeInformation =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 203)
            .Should()
            .Be203NonAuthoritativeInformation()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.NonAuthoritativeInformation)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be203NonAuthoritativeInformation()
        |> assertExnMsg
            """
Subject: x
Should: Be203NonAuthoritativeInformation
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be203NonAuthoritativeInformation("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be203NonAuthoritativeInformation
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be204NoContent =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 204)
            .Should()
            .Be204NoContent()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.NoContent)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be204NoContent()
        |> assertExnMsg
            """
Subject: x
Should: Be204NoContent
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be204NoContent("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be204NoContent
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be205ResetContent =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 205)
            .Should()
            .Be205ResetContent()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.ResetContent)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be205ResetContent()
        |> assertExnMsg
            """
Subject: x
Should: Be205ResetContent
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be205ResetContent("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be205ResetContent
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be206PartialContent =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 206)
            .Should()
            .Be206PartialContent()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.PartialContent)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be206PartialContent()
        |> assertExnMsg
            """
Subject: x
Should: Be206PartialContent
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be206PartialContent("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be206PartialContent
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be300MultipleChoices =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 300)
            .Should()
            .Be300MultipleChoices()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.MultipleChoices)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be300MultipleChoices()
        |> assertExnMsg
            """
Subject: x
Should: Be300MultipleChoices
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be300MultipleChoices("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be300MultipleChoices
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be301MovedPermanently =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 301)
            .Should()
            .Be301MovedPermanently()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.MovedPermanently)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be301MovedPermanently()
        |> assertExnMsg
            """
Subject: x
Should: Be301MovedPermanently
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be301MovedPermanently("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be301MovedPermanently
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be302Found =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 302)
            .Should()
            .Be302Found()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.Found)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be302Found()
        |> assertExnMsg
            """
Subject: x
Should: Be302Found
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be302Found("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be302Found
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be303SeeOther =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 303)
            .Should()
            .Be303SeeOther()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.SeeOther)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be303SeeOther()
        |> assertExnMsg
            """
Subject: x
Should: Be303SeeOther
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be303SeeOther("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be303SeeOther
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be304NotModified =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 304)
            .Should()
            .Be304NotModified()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.NotModified)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be304NotModified()
        |> assertExnMsg
            """
Subject: x
Should: Be304NotModified
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be304NotModified("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be304NotModified
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be307TemporaryRedirect =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 307)
            .Should()
            .Be307TemporaryRedirect()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.TemporaryRedirect)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be307TemporaryRedirect()
        |> assertExnMsg
            """
Subject: x
Should: Be307TemporaryRedirect
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be307TemporaryRedirect("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be307TemporaryRedirect
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be308PermanentRedirect =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 308)
            .Should()
            .Be308PermanentRedirect()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.PermanentRedirect)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be308PermanentRedirect()
        |> assertExnMsg
            """
Subject: x
Should: Be308PermanentRedirect
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be308PermanentRedirect("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be308PermanentRedirect
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be400BadRequest =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 400)
            .Should()
            .Be400BadRequest()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.BadRequest)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be400BadRequest()
        |> assertExnMsg
            """
Subject: x
Should: Be400BadRequest
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be400BadRequest("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be400BadRequest
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be401Unauthorized =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 401)
            .Should()
            .Be401Unauthorized()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.Unauthorized)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be401Unauthorized()
        |> assertExnMsg
            """
Subject: x
Should: Be401Unauthorized
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be401Unauthorized("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be401Unauthorized
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be403Forbidden =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 403)
            .Should()
            .Be403Forbidden()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.Forbidden)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be403Forbidden()
        |> assertExnMsg
            """
Subject: x
Should: Be403Forbidden
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be403Forbidden("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be403Forbidden
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be404NotFound =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 404)
            .Should()
            .Be404NotFound()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.NotFound)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be404NotFound()
        |> assertExnMsg
            """
Subject: x
Should: Be404NotFound
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be404NotFound("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be404NotFound
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be405MethodNotAllowed =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 405)
            .Should()
            .Be405MethodNotAllowed()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.MethodNotAllowed)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be405MethodNotAllowed()
        |> assertExnMsg
            """
Subject: x
Should: Be405MethodNotAllowed
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be405MethodNotAllowed("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be405MethodNotAllowed
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be406NotAcceptable =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 406)
            .Should()
            .Be406NotAcceptable()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.NotAcceptable)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be406NotAcceptable()
        |> assertExnMsg
            """
Subject: x
Should: Be406NotAcceptable
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be406NotAcceptable("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be406NotAcceptable
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be407ProxyAuthenticationRequired =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 407)
            .Should()
            .Be407ProxyAuthenticationRequired()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.ProxyAuthenticationRequired)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be407ProxyAuthenticationRequired()
        |> assertExnMsg
            """
Subject: x
Should: Be407ProxyAuthenticationRequired
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be407ProxyAuthenticationRequired("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be407ProxyAuthenticationRequired
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be408RequestTimeout =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 408)
            .Should()
            .Be408RequestTimeout()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.RequestTimeout)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be408RequestTimeout()
        |> assertExnMsg
            """
Subject: x
Should: Be408RequestTimeout
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be408RequestTimeout("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be408RequestTimeout
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be409Conflict =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 409)
            .Should()
            .Be409Conflict()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.Conflict)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be409Conflict()
        |> assertExnMsg
            """
Subject: x
Should: Be409Conflict
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be409Conflict("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be409Conflict
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be410Gone =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 410)
            .Should()
            .Be410Gone()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.Gone)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be410Gone()
        |> assertExnMsg
            """
Subject: x
Should: Be410Gone
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be410Gone("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be410Gone
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be411LengthRequired =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 411)
            .Should()
            .Be411LengthRequired()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.LengthRequired)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be411LengthRequired()
        |> assertExnMsg
            """
Subject: x
Should: Be411LengthRequired
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be411LengthRequired("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be411LengthRequired
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be412PreconditionFailed =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 412)
            .Should()
            .Be412PreconditionFailed()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.PreconditionFailed)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be412PreconditionFailed()
        |> assertExnMsg
            """
Subject: x
Should: Be412PreconditionFailed
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be412PreconditionFailed("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be412PreconditionFailed
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be413ContentTooLarge =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 413)
            .Should()
            .Be413ContentTooLarge()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.RequestEntityTooLarge)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be413ContentTooLarge()
        |> assertExnMsg
            """
Subject: x
Should: Be413ContentTooLarge
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be413ContentTooLarge("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be413ContentTooLarge
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be414UriTooLong =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 414)
            .Should()
            .Be414UriTooLong()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.RequestUriTooLong)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be414UriTooLong()
        |> assertExnMsg
            """
Subject: x
Should: Be414UriTooLong
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be414UriTooLong("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be414UriTooLong
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be415UnsupportedMediaType =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 415)
            .Should()
            .Be415UnsupportedMediaType()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.UnsupportedMediaType)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be415UnsupportedMediaType()
        |> assertExnMsg
            """
Subject: x
Should: Be415UnsupportedMediaType
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be415UnsupportedMediaType("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be415UnsupportedMediaType
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be416RangeNotSatisfiable =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 416)
            .Should()
            .Be416RangeNotSatisfiable()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.RequestedRangeNotSatisfiable)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be416RangeNotSatisfiable()
        |> assertExnMsg
            """
Subject: x
Should: Be416RangeNotSatisfiable
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be416RangeNotSatisfiable("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be416RangeNotSatisfiable
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be417ExpectationFailed =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 417)
            .Should()
            .Be417ExpectationFailed()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.ExpectationFailed)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be417ExpectationFailed()
        |> assertExnMsg
            """
Subject: x
Should: Be417ExpectationFailed
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be417ExpectationFailed("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be417ExpectationFailed
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be421MisdirectedRequest =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 421)
            .Should()
            .Be421MisdirectedRequest()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.MisdirectedRequest)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be421MisdirectedRequest()
        |> assertExnMsg
            """
Subject: x
Should: Be421MisdirectedRequest
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be421MisdirectedRequest("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be421MisdirectedRequest
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be426UpgradeRequired =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 426)
            .Should()
            .Be426UpgradeRequired()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.UpgradeRequired)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be426UpgradeRequired()
        |> assertExnMsg
            """
Subject: x
Should: Be426UpgradeRequired
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be426UpgradeRequired("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be426UpgradeRequired
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be428PreconditionRequired =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 428)
            .Should()
            .Be428PreconditionRequired()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.PreconditionRequired)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be428PreconditionRequired()
        |> assertExnMsg
            """
Subject: x
Should: Be428PreconditionRequired
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be428PreconditionRequired("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be428PreconditionRequired
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be429TooManyRequests =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 429)
            .Should()
            .Be429TooManyRequests()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.TooManyRequests)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be429TooManyRequests()
        |> assertExnMsg
            """
Subject: x
Should: Be429TooManyRequests
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be429TooManyRequests("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be429TooManyRequests
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be431RequestHeaderFieldsTooLarge =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 431)
            .Should()
            .Be431RequestHeaderFieldsTooLarge()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.RequestHeaderFieldsTooLarge)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be431RequestHeaderFieldsTooLarge()
        |> assertExnMsg
            """
Subject: x
Should: Be431RequestHeaderFieldsTooLarge
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be431RequestHeaderFieldsTooLarge("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be431RequestHeaderFieldsTooLarge
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be451UnavailableForLegalReasons =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 451)
            .Should()
            .Be451UnavailableForLegalReasons()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.UnavailableForLegalReasons)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be451UnavailableForLegalReasons()
        |> assertExnMsg
            """
Subject: x
Should: Be451UnavailableForLegalReasons
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be451UnavailableForLegalReasons("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be451UnavailableForLegalReasons
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be500InternalServerError =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 500)
            .Should()
            .Be500InternalServerError()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.InternalServerError)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be500InternalServerError()
        |> assertExnMsg
            """
Subject: x
Should: Be500InternalServerError
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be500InternalServerError("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be500InternalServerError
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be501NotImplemented =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 501)
            .Should()
            .Be501NotImplemented()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.NotImplemented)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be501NotImplemented()
        |> assertExnMsg
            """
Subject: x
Should: Be501NotImplemented
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be501NotImplemented("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be501NotImplemented
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be502BadGateway =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 502)
            .Should()
            .Be502BadGateway()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.BadGateway)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be502BadGateway()
        |> assertExnMsg
            """
Subject: x
Should: Be502BadGateway
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be502BadGateway("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be502BadGateway
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be503ServiceUnavailable =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 503)
            .Should()
            .Be503ServiceUnavailable()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.ServiceUnavailable)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be503ServiceUnavailable()
        |> assertExnMsg
            """
Subject: x
Should: Be503ServiceUnavailable
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be503ServiceUnavailable("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be503ServiceUnavailable
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be504GatewayTimeout =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 504)
            .Should()
            .Be504GatewayTimeout()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.GatewayTimeout)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be504GatewayTimeout()
        |> assertExnMsg
            """
Subject: x
Should: Be504GatewayTimeout
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be504GatewayTimeout("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be504GatewayTimeout
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be505HttpVersionNotSupported =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 505)
            .Should()
            .Be505HttpVersionNotSupported()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.HttpVersionNotSupported)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be505HttpVersionNotSupported()
        |> assertExnMsg
            """
Subject: x
Should: Be505HttpVersionNotSupported
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be505HttpVersionNotSupported("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be505HttpVersionNotSupported
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be506VariantAlsoNegotiates =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 506)
            .Should()
            .Be506VariantAlsoNegotiates()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.VariantAlsoNegotiates)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be506VariantAlsoNegotiates()
        |> assertExnMsg
            """
Subject: x
Should: Be506VariantAlsoNegotiates
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be506VariantAlsoNegotiates("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be506VariantAlsoNegotiates
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be510NotExtended =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 510)
            .Should()
            .Be510NotExtended()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.NotExtended)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be510NotExtended()
        |> assertExnMsg
            """
Subject: x
Should: Be510NotExtended
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be510NotExtended("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be510NotExtended
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module Be511NetworkAuthenticationRequired =


    [<Fact>]
    let ``Passes for the correct status code and can be chained with And`` () =
        (resp 511)
            .Should()
            .Be511NetworkAuthenticationRequired()
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.NetworkAuthenticationRequired)


    [<Fact>]
    let ``Fails with expected message for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be511NetworkAuthenticationRequired()
        |> assertExnMsg
            """
Subject: x
Should: Be511NetworkAuthenticationRequired
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because for incorrect status codes`` () =
        fun () ->
            let x = resp 100
            x.Should().Be511NetworkAuthenticationRequired("Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: Be511NetworkAuthenticationRequired
But was: 100 Continue
Response: HTTP/0.5 100 Continue
Request: GET / HTTP/0.5
"""


module HaveHeader =


    [<Fact>]
    let ``Passes for single header with single value and can be chained with AndDerived with the header value`` () =
        (respHeader 200 [ "A", "x" ])
            .Should()
            .HaveHeader("A")
            .Id<AndDerived<HttpResponseMessage, seq<string>>>()
            .WhoseValue.Should()
            .SequenceEqual([ "x" ])


    [<Fact>]
    let ``Passes for single header with comma-separated value and can be chained with AndDerived with the header value``
        ()
        =
        (respHeader 200 [ "A", "x,y" ])
            .Should()
            .HaveHeader("A")
            .Id<AndDerived<HttpResponseMessage, seq<string>>>()
            .WhoseValue.Should()
            .SequenceEqual([ "x,y" ])


    [<Fact>]
    let ``Passes for multiple headers and can be chained with AndDerived with the header value`` () =
        (respHeader 200 [ "A", "x"; "A", "y" ])
            .Should()
            .HaveHeader("A")
            .Id<AndDerived<HttpResponseMessage, seq<string>>>()
            .WhoseValue.Should()
            .SequenceEqual([ "x"; "y" ])


    [<Fact>]
    let ``Passes for content headers`` () =
        (respHeader 200 [ "Content-Type", "application/json" ])
            .Should()
            .HaveHeader("Content-Type")


    [<Fact>]
    let ``Fails with expected message if header is not found`` () =
        fun () ->
            let x = respHeader 200 [ "A", "x" ]
            x.Should().HaveHeader("B")
        |> assertExnMsg
            """
Subject: x
Should: HaveHeader
Header: B
Response: |-
  HTTP/0.5 200 OK
  A: x
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because if header is not found`` () =
        fun () ->
            let x = respHeader 200 [ "A", "x" ]
            x.Should().HaveHeader("B", "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: HaveHeader
Header: B
Response: |-
  HTTP/0.5 200 OK
  A: x
Request: GET / HTTP/0.5
"""


module HaveHeaderValue =


    [<Fact>]
    let ``Passes for single header with single value and can be chained with And`` () =
        (respHeader 200 [ "A", "x" ])
            .Should()
            .HaveHeaderValue("A", "x")
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.OK)


    [<Fact>]
    let ``Passes for single header with comma-separated value and can be chained with AndDerived with the header value``
        ()
        =
        (respHeader 200 [ "A", "x,y" ])
            .Should()
            .HaveHeaderValue("A", "x,y")
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.OK)


    [<Fact>]
    let ``Passes for multiple headers and can be chained with AndDerived with the header value`` () =
        (respHeader 200 [ "A", "x"; "A", "y" ])
            .Should()
            .HaveHeaderValue("A", "x")
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.OK)


    [<Fact>]
    let ``Passes for content headers`` () =
        (respHeader 200 [ "Content-Type", "application/json" ])
            .Should()
            .HaveHeaderValue("Content-Type", "application/json")


    [<Fact>]
    let ``Fails with expected message if header is not found`` () =
        fun () ->
            let x = respHeader 200 [ "A", "x"; "A", "y" ]
            x.Should().HaveHeaderValue("A", "z")
        |> assertExnMsg
            """
Subject: x
Should: HaveHeaderValue
Header: A
Value: z
Response: |-
  HTTP/0.5 200 OK
  A: x
  A: y
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message with because if header is not found`` () =
        fun () ->
            let x = respHeader 200 [ "A", "x"; "A", "y" ]
            x.Should().HaveHeaderValue("A", "z", "Some reason")
        |> assertExnMsg
            """
Subject: x
Because: Some reason
Should: HaveHeaderValue
Header: A
Value: z
Response: |-
  HTTP/0.5 200 OK
  A: x
  A: y
Request: GET / HTTP/0.5
"""


module NotHaveHeader =


    [<Fact>]
    let ``Passes if header is not present and can be chained with And`` () =
        (respHeader 200 [ "A", "x" ])
            .Should()
            .NotHaveHeader("B")
            .Id<And<HttpResponseMessage>>()
            .And.Subject.StatusCode.Should(())
            .Be(HttpStatusCode.OK)


    [<Fact>]
    let ``Fails with expected message if header is found with single value`` () =
        fun () ->
            let x = respHeader 200 [ "A", "x" ]
            x.Should().NotHaveHeader("A")
        |> assertExnMsg
            """
Subject: x
Should: NotHaveHeader
Header: A
But was present with value: x
Response: |-
  HTTP/0.5 200 OK
  A: x
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message if header is found with multiple values`` () =
        fun () ->
            let x = respHeader 200 [ "A", "x"; "A", "y" ]
            x.Should().NotHaveHeader("A")
        |> assertExnMsg
            """
Subject: x
Should: NotHaveHeader
Header: A
But was present with values: [x, y]
Response: |-
  HTTP/0.5 200 OK
  A: x
  A: y
Request: GET / HTTP/0.5
"""


module HaveStringContentSatisfying =


    [<Fact>]
    let ``Passes if has content and the inner assertion passes and returns the inner value`` () =
        (respContent 200 "foo")
            .Should()
            .HaveStringContentSatisfying(_.Should().Be("foo").Subject)
            .Id<Async<string>>()
        |> Async.RunSynchronously
        |> _.Should().Be("foo")


    [<Fact>]
    let ``Fails with expected message if no content`` () =
        fun () -> (resp 200).Should().HaveStringContentSatisfying(_.Should().Be("foo"))
        |> assertExnMsgAsync
            """
Subject: resp 200
Should: HaveStringContentSatisfying
Failure:
  Subject: _
  Should: Be
  Expected: foo
  But was: ''
Response: HTTP/0.5 200 OK
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message if no content with because`` () =
        fun () ->
            (resp 200)
                .Should()
                .HaveStringContentSatisfying(_.Should().Be("foo"), "Some reason")
        |> assertExnMsgAsync
            """
Subject: resp 200
Because: Some reason
Should: HaveStringContentSatisfying
Failure:
  Subject: _
  Should: Be
  Expected: foo
  But was: ''
Response: HTTP/0.5 200 OK
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message if content does not satisfy inner assertion`` () =
        fun () -> (respContent 200 "foo").Should().HaveStringContentSatisfying(_.Should().Fail())
        |> assertExnMsgAsync
            """
Subject: respContent 200 "foo"
Should: HaveStringContentSatisfying
Failure:
  Subject: _
  Should: Fail
Response: |-
  HTTP/0.5 200 OK
  Content-Type: text/plain; charset=utf-8
  Content-Length: 3

  foo
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message if content does not satisfy inner assertion with because`` () =
        fun () ->
            (respContent 200 "foo")
                .Should()
                .HaveStringContentSatisfying(_.Should().Fail(), "Some reason")
        |> assertExnMsgAsync
            """
Subject: respContent 200 "foo"
Because: Some reason
Should: HaveStringContentSatisfying
Failure:
  Subject: _
  Should: Fail
Response: |-
  HTTP/0.5 200 OK
  Content-Type: text/plain; charset=utf-8
  Content-Length: 3

  foo
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message if inner assertion throws`` () =
        fun () ->
            (respContent 200 "foo")
                .Should()
                .HaveStringContentSatisfying(fun _ -> failwith "foo")
            |> Async.RunSynchronously
        |> assertExnMsgWildcard
            """
Subject: respContent 200 "foo"
Should: HaveStringContentSatisfying
But threw: |-
  System.Exception: foo
*
Response: |-
  HTTP/0.5 200 OK
  Content-Type: text/plain; charset=utf-8
  Content-Length: 3

  foo
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message if inner assertion throws with because`` () =
        fun () ->
            (respContent 200 "foo")
                .Should()
                .HaveStringContentSatisfying((fun _ -> failwith "foo"), "Some reason")
            |> Async.RunSynchronously
        |> assertExnMsgWildcard
            """
Subject: respContent 200 "foo"
Because: Some reason
Should: HaveStringContentSatisfying
But threw: |-
  System.Exception: foo
*
Response: |-
  HTTP/0.5 200 OK
  Content-Type: text/plain; charset=utf-8
  Content-Length: 3

  foo
Request: GET / HTTP/0.5
"""


    [<Fact>]
    let ``Fails with expected message for example use-case with returned value`` () =
        fun () ->
            let respMsg = respContent 400 """{"error":"Invalid"}"""

            respMsg
                .Should()
                .HaveStringContentSatisfying(fun content ->
                    let error = content.Should().DeserializeTo<{| error: string |}>().Derived.error

                    error
                        .Should()
                        .Transform(
                            function
                            | null
                            | "" -> Ok()
                            | "Known" -> Error "KnownError"
                            | _ -> failwith "Unknown value"
                        )
                )
            |> Async.RunSynchronously
        |> assertExnMsgWildcard
            """
Subject: respMsg
Should: HaveStringContentSatisfying
Failure:
  Subject: error
  Should: Transform
  But threw: |-
    System.Exception: Unknown value*
  Subject value: Invalid
Response: |-
  HTTP/0.5 400 Bad Request
  Content-Type: text/plain; charset=utf-8
  Content-Length: 19

  [content has been formatted]
  {"error": "Invalid"}
Request: GET / HTTP/0.5
"""
