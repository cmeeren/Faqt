Faqt
====

**Fantastic fluent assertions your F# tests and domain pre-/post-conditions/invariants.**

<img src="https://raw.githubusercontent.com/cmeeren/Faqt/main/logo/faqt-logo-docs.png" width="300" align="right" />

Faqt improves on the best of [FluentAssertions](https://github.com/fluentassertions/fluentassertions)
and [Shouldly](https://github.com/shouldly/shouldly) and serves it steaming hot on a silver platter
to the discerning F# developer. **It aims to the best assertion library for F#.** If you don't agree, I consider that a
bug - please raise an issue.

### Work in progress, 1.0 to be released late August 2023

Faqt is currently a work in progress. All "infrastructure" and supporting features are in place; most of the actual
assertions remain to be implemented. A feature complete 1.0 version will hopefully be released during August 2023.

## A motivating example

Here is an example of what you can do with Faqt. Simply use `Should()` to start asserting, whether in a unit test or for
validating preconditions etc. in domain code (demonstrated below). Like FluentAssertions, all assertions support an
optional "because" parameter that will be used in the output.

```f#
type Customer =
    | Internal of {| ContactInfo: {| Name: {| LastName: string |} |} option |}
    | External of {| Id: int |}

let calculateFreeShipping customer =
    customer
        .Should()
        .BeOfCase(Internal, "this function should only be called with internal customers")
        .Whose.ContactInfo.Should()
        .BeSome()
        .Whose.Name.LastName.Should()
        .Be("Armstrong", "only customers named Armstrong gets free shipping")
```

(The example is formatted using [Fantomas](https://fsprojects.github.io/fantomas/), which line-breaks fluent chains at
method calls. While the readability of Faqt assertion chains could be slightly improved by manual formatting, entirely
foregoing automatic formatting is not worth the slight benefit to readability.)

Depending on the input, a `Faqt.AssertionFailedException` may be raised with one of these messages:

If customer is `External`:

```
customer
    should be of case
Internal
    because this function should only be called with internal customers, but was
External { Id = 1 }
```

If `ContactInfo` is `None`:

```
customer...ContactInfo
    should be of case
Some
    but was
None
```

If `LastName` is not `Armstrong`:

```
customer...ContactInfo...Name.LastName
    should be
"Armstrong"
    because only customers named Armstrong gets free shipping, but was
"Aldrin"
```

As you can see, the first line tells you which part of the code fails (and `...` is used when using derived state from
an assertion). **Yes, this works even in production, on CI with `DeterministicSourcePaths`, and otherwise when your
source files are not available, as long as you use `<DebugType>embedded</DebugType>`
and `<EmbedAllSources>true</EmbedAllSources>`. It's magic!**

## Faqt in a nutshell

As expected by the discerning F# developer, Faqt is:

- **Readable:** Assertions read like natural language.
- **Concise:** Verbosity in the syntax is kept to an absolute minimum.
- **Usable:** Faqt comes with batteries included - many useful assertions, including aliases (like `BeTrue()`
  for `Be(true)` on booleans, and `BeSome` for `BeOfCase(Some)` on `option` values).
- **Safe:** Assertions are as type-safe as F# allows.
- **Extendable:** No assertion? No problem! Writing your own assertions is very simple.
- **Informative:** The assertion failure messages are designed to give you all the information you need in an
  easy-to-parse format.
- **Discoverable:** The fluent syntax means you can just type a dot to discover all possible assertions.
- **Composable:** You can chain assertions with `And`, `Whose`, `Which`, and `Subject`, assert on derived values like
  with `BeSome()`, split out assertion chains with `Satisfy`, and require one of several sub-assertions
  with `SatisfyAny`.
- **Configurable:**: You can configure how values are formatted in the assertion message on a type-by-type basis, and
  specify a default formatter (e.g. for outputting as JSON).
- **Production-ready:** Faqt is very well tested and will not break your code, whether test or production.

## Writing your own assertions

Writing your own assertions is easy! They are implemented exactly like Faqt’s built-in assertions, so you can always
look at those for inspiration.

Let’s look at the implementation for Faqt’s simplest assertion, `Be`. Don’t be discouraged by how detailed the
explanation below is; it’s better to explain it thoroughly once than piecewise here and there.

```f#
open Faqt
open AssertionHelpers
open Formatting

[<Extension>]
type Assertions =

    [<Extension>]
    static member Be(t: Testable<'a>, expected: 'a, ?because, ?methodNameOverride) : And<'a> =
        if t.Subject <> expected then
            Fail(t, because, methodNameOverride)
                .Throw("{subject}\n\tshould be\n{0}\n\t{because}but was\n{actual}", format expected)

        And(t)
```

Here are the important points:

* Implement the assertion as
  an [extension method](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/type-extensions#extension-methods)
  for `Testable` (the first argument), with whatever constraints you need. The constraints could be implicitly imposed
  by F#, as above where it requires `equality` on `a` due to the use of `<>`, or they could be explicitly specified, for
  example by specifying more concrete types (such as `Testable<'a option>` in order to have your extension only work
  for `option`-wrapped types).

* Accept whichever arguments you need for your assertion, and end with `?because` and `?methodNameOverride`. The latter
  is used when other assertions call your assertions in their implementation, and is required in order for the automatic
  subject name (the first part of the assertion message) to work in those cases. If you write ad-hoc assertions that
  only you will use, and you know no other assertions will call your assertion, feel free to drop `methodNameOverride` (
  and pass `None` in its position in the call to `Fail`).

* If your condition is not met, call

   ```f#
   Fail(t, because, methodNameOverride)
   	.Throw("<message template>", param1, param2, ...)
   ```

* The message template is up you, but for consistency it should ideally adhere to the following conventions:

  * The general structure should be something like “{subject} should … {because}, but …”.
  * Use `{subject}`, `{because}`, and `{actual}` as placeholders for the subject name, the user-supplied reason, and the
    current value being tested (`t.Suject`), respectively. (Not all assertions need `{actual}`.)
  * Use `{0}`, `{1}`, etc. as needed for any values passed as parameters after the template. These parameters must
    be `string`; use the `format` function (in the opened `Formatting` module) to format values for display.
  * Don’t use string interpolation to insert values you don’t have control over (for example, values that could contain
    the placeholders mentioned above).
  * Place `{subject}`, `{actual}`, and all other important values on separate lines. All other text should be indented
    using `\t`.
  * Ensure that your message is rendered correctly if `{because}` is replaced with an empty string (regarding whitespace
    before and/or comma + space after `{because}`). Faqt will insert a space before and space + comma after `{because}`
    if needed.

* If your assertion extracts derived state that can be used for further assertions,
  return `AndDerived(t, derivedState)`. Otherwise return `And(t)`.

* If your assertions calls another assertion, you must pass `methodNameOverride` explicitly. For example, here is
  the `BeSome` assertion, which is an alias of `BeOfCase(Some)`:

    ```f#
    [<Extension>]
    static member BeSome(t: Testable<'a option>, ?because, ?methodNameOverride) =
        t.BeOfCase(
            Some,
            ?because = because,
            methodNameOverride = defaultArg methodNameOverride (nameof Assertions.BeSome)
        )
    ```

* If your assertion calls `Should` at any point, make sure you pass the original `Testable` as an argument, since it
  contains important state relating to the end user’s original assertion call. For example, the above `BeSome`
  implementation could (somewhat artificially) be implemented like this:

  ```f#
  [<Extension>]
  static member BeSome(t: Testable<'a option>, ?because, ?methodNameOverride) =
      t.Subject.Should(t).BeOfCase((* same as previous example *))
  ```

Subject name and limitations
----------------------------

(This is not likely to interest most users.)

The automatic subject name (the first part of the assertion message) is based on clever use of caller info attributes,
parsing sources from either local files or embedded resources, and simple regex-based processing/replacement of the call
chain. It has a few limitations.

Fundamental limitations that there probably isn’t a way around:

* If an assertion chain has multiple assertions with the same method name, only the first one will be considered when
  getting the subject name. This is because the caller information attributes (and stack traces, which were also
  attempted and abandoned early on) only refer to the start of the chain, so there is no way for Faqt to know which of
  them failed.

Assumptions that could be ad-hoc improved (raise an issue) or theoretically solved entirely by parsing the F# source
code using FSharp.Compiler.Service instead of simple regex-based processing (though note that this has its own
drawbacks; it was initially tried and abandoned in the early stages of Faqt):

* Assertion chains must start on a new line, or right after `fun ... ->`.
* Chains do not contain string literals containing `//` (an exception is made for `://` which is used in URLs).
* Chains do not contain multi-line strings.
* Chains do not contain multi-line parenthesized/bracketed expressions.

If these assumptions are broken, the worst that happens is that the subject name is incorrect (or in the worst case is
replaced by the generic string `"subject"` if something catastrophic occurs).
