Faqt
====

**Fantastic fluent assertions for your F# tests and domain pre-/post-conditions/invariants.**

<img src="https://raw.githubusercontent.com/cmeeren/Faqt/main/logo/faqt-logo-docs.png" width="300" align="right" />

Faqt improves on the best of [FluentAssertions](https://github.com/fluentassertions/fluentassertions)
and [Shouldly](https://github.com/shouldly/shouldly) and serves it steaming hot on a silver platter to the discerning F#
developer.

**It aims to the best assertion library for F#.**

If you don't agree, I consider that a bug - please raise an issue. 😉

### Work in progress, 1.0 to be released late August 2023

Faqt is currently a work in progress. All "infrastructure" and supporting features are in place; most of the actual
assertions remain to be implemented. A feature complete 1.0 version will hopefully be released during August 2023.

## Table of contents

<!-- TOC -->

* [A motivating example](#a-motivating-example)
* [Installation and requirements](#installation-and-requirements)
* [Faqt in a nutshell](#faqt-in-a-nutshell)
* [Writing your own assertions](#writing-your-own-assertions)
* [Multiple assertion chains without `|> ignore`](#multiple-assertion-chains-without--ignore)
* [FAQ](#faq)
  * [Which testing frameworks does Faqt work with?](#which-testing-frameworks-does-faqt-work-with)
  * [Why is the subject name not correct in my specific example?](#why-is-the-subject-name-not-correct-in-my-specific-example)
  * [Why not FluentAssertions?](#why-not-fluentassertions)
  * [Why not Shouldly?](#why-not-shouldly)
  * [Can I use Faqt from C#?](#can-i-use-faqt-from-c)

<!-- TOC -->

## A motivating example

Here is an example of what you can do with Faqt. Simply use `Should()` to start asserting, whether in a unit test or for
validating preconditions etc. in domain code (the latter is demonstrated below). Like FluentAssertions, all assertions
support an optional "because" parameter that will be used in the output.

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
        .Be("Armstrong", "only customers named Armstrong get free shipping")
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
    because only customers named Armstrong get free shipping, but was
"Aldrin"
```

As you can see, the first line tells you which part of the code fails (and `...` is inserted when using derived state
from an assertion).

**Yes, this works even in Release mode or when source files are not available!** See the very simple requirements below.

## Installation and requirements

1. Install Faqt [from NuGet](https://www.nuget.org/packages/Faqt). Faqt supports .NET 5.0 and higher.
2. If you use path mapping (e.g., CI builds with `DeterministicSourcePaths` enabled) or want to execute assertions where
   source files are not available (e.g. in production), enable the following settings on all projects that call
   assertions (either in the `.fsproj` files or in `Directory.Build.props`):

   ```xml
   <DebugType>embedded</DebugType>
   <EmbedAllSources>true</EmbedAllSources>
   ```

   Alternatively, enable them by passing the following parameters to your `dotnet build`/`test`/`publish` commands:

   ```
   -p:DebugType=embedded -p:EmbedAllSources=true
   ```

   Note that `DebugType=embeded` is automatically set
   by [DotNet.ReproducibleBuilds](https://github.com/dotnet/reproducible-builds) if you use that.

## Faqt in a nutshell

As expected by the discerning F# developer, Faqt is:

- **Readable:** Assertions read like natural language and clearly reveal their intention.
- **Concise:** Assertion syntax verbosity is kept to an absolute minimum.
- **Usable:** Faqt comes with batteries included, and contains many useful assertions, including aliases
  (like `BeTrue()` for `Be(true)` on booleans, and `BeSome` for `BeOfCase(Some)` on `option` values).
- **Safe:** Assertions are as type-safe as F# allows.
- **Extensible:** No assertion? No problem! Writing your own assertions is very simple (details below).
- **Informative:** The assertion failure messages are designed to give you all the information you need in a consistent
  and easy-to-read format.
- **Discoverable:** The fluent syntax means you can just type a dot to discover all possible assertions and actions on
  the current value.
- **Composable:** As far as possible, assertions are orthogonal (they check one thing only). For example, an assertion
  for verifying that a collection only contains items that match a predicate does not fail if the collection is empty.
  You can chain assertions with `And`, `Whose`, `WhoseValue`, `That`, and `Subject`, assert on derived values like
  with `BeSome()`, and compose assertions with higher-order assertions like `Satisfy`, `SatisfyAll`, and `SatisfyAny`.
- **Configurable:** You can configure how values are formatted in the assertion message on a type-by-type basis, and
  specify a default formatter (e.g. for displaying all values as serialized JSON by default).
- **Production-ready:** Faqt is very well tested and is highly unlikely to break your code, whether test or production.

## Writing your own assertions

Writing your own assertions is easy! They are implemented exactly like Faqt’s built-in assertions, so you can always
look at those for inspiration (see all files ending with `Assertions`
in [this folder](https://github.com/cmeeren/Faqt/tree/main/src/Faqt)).

Let’s look at the implementation for Faqt’s simplest assertion, `Be`:

```f#
open Faqt
open AssertionHelpers
open Formatting

[<Extension>]
type Assertions =

    /// Asserts that the subject is the specified value, using the default equality comparison (=).
    [<Extension>]
    static member Be(t: Testable<'a>, expected: 'a, ?because) : And<'a> =
        use _ = t.Assert()

        if t.Subject <> expected then
            t.Fail("{subject}\n\tshould be\n{0}\n\t{because}but was\n{actual}", because, format expected)

        And(t)
```

Simple, right? Here are additional quick remarks that are not shown above:

* If it's a higher-order assertion that calls user code that calls other assertions, use `t.Assert(true)` instead
  of `t.Assert()`.
* Return `AndDerived(t, derivedState)` instead of `And(t)` if your assertion extracts derived state.
* If your assertion calls `Should`, make sure to the `Should(t)` overload.

If you want all the details, here they are:

* Implement the assertion as
  an [extension method](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/type-extensions#extension-methods)
  for `Testable` (the first argument), with whatever constraints you need. The constraints could be implicitly imposed
  by F#, as above where it requires `equality` on `'a` due to the use of `<>`, or they could be explicitly specified,
  for example by specifying more concrete types (such as `Testable<'a option>` in order to have your extension only work
  for `option`-wrapped types).

* Accept whichever arguments you need for your assertion, and end with `?because`.

* First in your method, call `use _ = t.Assert()`. This is needed to track important state necessary for subject
  names to work. If your assertion is a higher-order assertion (like `Satisfy`) that calls user code that is expected to
  call other assertions, call `t.Assert(true)` instead.

* If your condition is not met, call

   ```f#
   t.Fail("<message template>", because, param1, param2, ...)
   ```

* The message template is up you, but for consistency it should ideally adhere to the following conventions:

  * The general structure should be something like “{subject} should … {because}, but …”.
  * Use `{subject}`, `{because}`, and `{actual}` as placeholders for the subject name, the user-supplied reason, and the
    current value being tested (`t.Subject`), respectively. (Not all assertions need `{actual}`.)
  * Use `{0}`, `{1}`, etc. as needed for any values passed as parameters after the template. These parameters must
    be of type `string`; use the `format` function (in the opened `Formatting` module) to format values for display.
  * Don’t use string interpolation to insert values you don’t have control over (for example, values that could contain
    the placeholders mentioned above).
  * Place `{subject}`, `{actual}`, and other important values on separate lines. All other text should be indented
    using `\t`.
  * Ensure that your message is rendered correctly if `{because}` is replaced with an empty string. If needed, Faqt will
    automatically insert a space before `{because}` and/or a comma + space after `{because}`.

* If your assertion extracts derived state that can be used for further assertions,
  return `AndDerived(t, derivedState)`. Otherwise return `And(t)`. Prefer `AndDerived` over `And` if at all relevant.
  For example, if you implement an assertion called `ContainsElementsMatching(predicate)`, return the matched elements
  as the derived state, so that the user has the option to continue asserting on them.

* If your assertion calls `Should` at any point, make sure you use the overload that takes the original `Testable` as an
  argument (`.Should(t)`), since it contains important state relating to the end user’s original assertion call.

## Multiple assertion chains without `|> ignore`

Since assertions return `And` or `AndDerived`, F# will warn you if an assertion chain is not the last line of an
expression. You have to `|> ignore` all lines (except the last) in order to remove this warning.

For convenience, you can `open Faqt.Operators` and use the `%` prefix operator:

```f#
%x.Should().Be("a")
%y.Should().Be("b")
```

Note that the `%` operator is simply an alias for `ignore` and is defined like this:

```f#
let inline (~%) x = ignore x
```

If you want to use another operator, you can define your own just as easily.
See [this StackOverflow answer](https://stackoverflow.com/a/34188952/2978652) for valid prefix operators.

## FAQ

### Which testing frameworks does Faqt work with?

All of them. XUnit, NUnit, MSTest, NSpec, MSpec, Expecto, you name it. Faqt is agnostic to the test framework; it simply
throws a custom exception when an assertion fails.

### Why is the subject name not correct in my specific example?

The automatic subject name (the first part of the assertion message) is correct in most situations, but there are edge
cases where it may produce unexpected results:

* The subject name is incorrect if the assertion chain does not start on a new line or at the start of a
  lambda (`fun ... ->`).
* Multi-line strings literals will be concatenated.
* Lines starting with `//` in multi-line string literals will be removed.
* Nested `Satisfy` or other higher-order assertions may give incorrect subject names.

If you have encountered a case not listed above, please raise an issue. If I can't or won't fix it, I can at the very
least document it as a known limitation.

These limitations are due to the implementation of automatic subject names. It is based on clever use of caller info
attributes, parsing source code from either local files or embedded resources, thread-local state, and simple
regex-based processing/replacement of the call chain based on which assertions have been encountered so far.

If you would like to help make the automatic subject name functionality more robust, please raise an issue. You can find
the relevant code in [SubjectName.fs](https://github.com/cmeeren/Faqt/blob/main/src/Faqt/SubjectName.fs).

### Why not FluentAssertions?

FluentAssertions is a fantastic library, and very much the inspiration for Faqt. Unfortunately, its API design causes
trouble for F#. Here are the reasons I decided to make Faqt instead of just using FluentAssertions:

* The `because` parameter cannot be omitted when used from
  F# ([#2225](https://github.com/fluentassertions/fluentassertions/issues/2225)).
* Several assertions (specifically, those that accept an `Action<_>`) require `ignore` when used from
  F# ([#2226](https://github.com/fluentassertions/fluentassertions/issues/2226)).
* The subject name does not consider transformations in the assertion
  chain ([#2223](https://github.com/fluentassertions/fluentassertions/issues/2223)).
* Improving F# usage issues (particularly the point about the `because` parameter)
  was [deemed out of scope](https://github.com/fluentassertions/fluentassertions/issues/2225#issuecomment-1636733116)
  for FluentAssertions.
* The one-line assertion messages are harder to parse than more structured output, especially for complex objects and
  collections.
* Some assertions run contrary to expectations of F# (or even C#)
  developers ([discussion](https://github.com/fluentassertions/fluentassertions/discussions/2143#discussioncomment-5525582)).

Note that Faqt does not aim for feature parity with FluentAssertions. For example, Faqt does not execute and report on
multiple assertions simultaneously; like almost all assertion libraries, it stops at the first failure ("monadic"
instead of "applicative" behavior).

### Why not Shouldly?

I will admit I have not used Shouldly myself, but its feature set (ignoring the actual assertions) seem to be a subset
of that of FluentAssertions. For example, it does not support chaining assertions. However, I like its easy-to-read
assertion failure messages, and have used those as inspiration for Faqt's assertion messages.

### Can I use Faqt from C#?

Feel free, but know that Faqt is designed only for F#. The subject names only work correctly for F#, and the API design
and assertion choices are based on F# idioms and expected usage. Any support for C# is incidental, and improving or even
preserving C# support is out of scope for Faqt.
