Faqt
====

**Fantastic fluent assertions for your F# tests and domain pre-/post-conditions/invariants.**

<img src="https://raw.githubusercontent.com/cmeeren/Faqt/main/logo/faqt-logo-docs.png" width="300" align="right" />

Faqt improves on the best of [FluentAssertions](https://github.com/fluentassertions/fluentassertions)
and [Shouldly](https://github.com/shouldly/shouldly) and serves it steaming hot on a silver platter to the discerning F#
developer.

**It aims to the best assertion library for F#.**

If you don't agree, I consider that a bug - please raise an issue. 😉

### Work in progress, 1.0 to be released September 2023

Faqt is currently a work in progress. All "infrastructure" and supporting features are in place; some actual assertions
remain to be implemented. A feature complete 1.0 version will hopefully be released by the end of September 2023.

### Versioning and breaking changes

Faqt follows [SemVer v2.0.0](https://semver.org/) and aims to preserve **source and binary** compatibility between
releases, except when the major version is incremented. Note that any change to the assertion message format is
considered a non-breaking change.

## Table of contents

<!-- TOC -->

* [A motivating example](#a-motivating-example)
* [Installation and requirements](#installation-and-requirements)
* [Faqt in a nutshell](#faqt-in-a-nutshell)
* [Writing your own assertions](#writing-your-own-assertions)
* [Multiple assertion chains without `|> ignore`](#multiple-assertion-chains-without--ignore)
* [Customizing the format](#customizing-the-format)
* [Security considerations](#security-considerations)
* [FAQ](#faq)
  * [Which testing frameworks does Faqt work with?](#which-testing-frameworks-does-faqt-work-with)
  * [Why is the subject name not correct in my specific example?](#why-is-the-subject-name-not-correct-in-my-specific-example)
  * [Why do I have to use `Should(())` inside an assertion chain?](#why-do-i-have-to-use-should-inside-an-assertion-chain)
  * [Why does this assertion pass/fail for null?](#why-does-this-assertion-passfail-for-null)
  * [Why not FluentAssertions?](#why-not-fluentassertions)
  * [Why not Shouldly?](#why-not-shouldly)
  * [Why not Unquote?](#why-not-unquote)
  * [Can I use Faqt from C#?](#can-i-use-faqt-from-c)

<!-- TOC -->

## A motivating example

Here is an example of what you can do with Faqt. Simply use `Should()` to start asserting, whether in a unit test or for
validating preconditions etc. in domain code (the latter is demonstrated below). For subsequent calls to `Should` in the
same chain, use `Should(())` (double parentheses - this is required for subject names to work properly). Like
FluentAssertions, all assertions support an optional "because" parameter that will be used in the output.

```f#
type Customer =
    | Internal of {| ContactInfo: {| Name: {| LastName: string |} |} option |}
    | External of {| Id: int |}

let calculateFreeShipping customer =
    customer
        .Should()
        .BeOfCase(Internal, "This function should only be called with internal customers")
        .Whose.ContactInfo.Should(())
        .BeSome()
        .Whose.Name.LastName.Should(())
        .Be("Armstrong", "Only customers named Armstrong get free shipping")
```

(The example is formatted using [Fantomas](https://fsprojects.github.io/fantomas/), which line-breaks fluent chains at
method calls. While the readability of Faqt assertion chains could be slightly improved by manual formatting, entirely
foregoing automatic formatting is not worth the slight benefit to readability.)

Depending on the input, a `Faqt.AssertionFailedException` may be raised with one of these messages:

If customer is `External`:

```
Subject: customer
Because: This function should only be called with internal customers
Should: BeOfCase
Expected: Internal
But was:
  External:
    Id: 1
```

If `ContactInfo` is `None`:

```
Subject:
- customer
- ContactInfo
Should: BeSome
But was: null
```

If `LastName` is not `Armstrong`:

```
Subject:
- customer
- ContactInfo
- Name.LastName
Because: Only customers named Armstrong get free shipping
Should: Be
Expected: Armstrong
But was: Aldrin
```

As you can see, the output is YAML-based (because this is both human readable and works well for arbitrary structured
values). The top-level `Subject` key tells you which part of the code fails, and an array of values is used when using
derived state from an assertion, so you can track the transformations on the original subject.

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
- **Composable:** As far as possible, assertions are orthogonal (they check one thing only). For example, an empty
  collection will pass an assertion verifying that the collection only contains items that match a predicate. You can
  chain assertions with `And`, `Whose`, `WhoseValue`, `That`, and `Subject`, assert on derived values like with
  `BeSome()`, and compose assertions with higher-order assertions like `Satisfy` and `SatisfyAll`.
- **Configurable:** You can configure, either globally or for a specific scope, how assertion failure messages are
  rendered. You can easily tweak the defaults or completely replace the formatter.
- **Production-ready:** Faqt is very well tested and is highly unlikely to break your code, whether test or production.

## Writing your own assertions

Writing your own assertions is easy! Custom assertions are implemented exactly like Faqt’s built-in assertions, so you
can always look at those for inspiration (see all files ending with `Assertions` in [this
folder](https://github.com/cmeeren/Faqt/tree/main/src/Faqt)).

All the details are further below, but first, we'll get a long way just by looking at some examples.

Here is Faqt’s simplest assertion, `Be`:

```f#
open System.Runtime.CompilerServices
open Faqt
open AssertionHelpers

[<Extension>]
type Assertions =

    /// Asserts that the subject is equal to the specified value.
    [<Extension>]
    static member Be(t: Testable<'a>, expected: 'a, ?because) : And<'a> =
        use _ = t.Assert()

        if t.Subject <> expected then
            t.With("Expected", expected).With("But was", t.Subject).Fail(because)

        And(t)
```

Simple, right? Now let's look at an assertion that's just as simple, but uses derived state, where you return
`AndDerived` instead of `And`:

```f#
open System
open System.Runtime.CompilerServices
open Faqt
open AssertionHelpers

[<Extension>]
type Assertions =

    /// Asserts that the subject has a value.
    [<Extension>]
    static member HaveValue(t: Testable<Nullable<'a>>, ?because) : AndDerived<Nullable<'a>, 'a> =
        use _ = t.Assert()

        if not t.Subject.HasValue then
            t.With("But was", t.Subject).Fail(because)

        AndDerived(t, t.Subject.Value)
```

This allows users to continue asserting on the derived state (the inner value, in this case).

Finally, let's look at a more complex assertion - a higher-order assertion that calls user assertions and which also
asserts for every item in a sequence:

```f#
open System
open System.Runtime.CompilerServices
open Faqt
open AssertionHelpers
open Formatting

type private SatisfyReportItem = { Index: int; Failure: FailureData }

[<Extension>]
type Assertions =

    /// Asserts that all items in the collection satisfy the supplied assertion.
    [<Extension>]
    static member AllSatisfy(t: Testable<#seq<'a>>, assertion: 'a -> 'ignored, ?because) : And<_> =
        use _ = t.Assert(true, true)

        if isNull (box t.Subject) then
            t.With("But was", t.Subject).Fail(because)

        let exceptions =
            t.Subject
            |> Seq.indexed
            |> Seq.choose (fun (i, x) ->
                try
                    use _ = t.AssertItem()
                    assertion x |> ignore
                    None
                with :? AssertionFailedException as ex ->
                    Some(i, ex)
            )
            |> Seq.toArray

        if exceptions.Length > 0 then
            t
                .With("Failures", exceptions |> Array.map (fun (i, ex) -> { Index = i; Failure = ex.FailureData }))
                .With("Value", t.Subject)
                .Fail(because)

        And(t)
```

Note that in this case we use `t.Assert(true, true)` at the top (use `t.Assert(true)` for higher-order assertions that
do not run the same assertions on items in a sequence), and we call `use _ = t.AssertItem()` before the assertion of
each item.

The most significant thing _not_ demonstrated in the examples above is that if your assertion calls `Should`, make sure
to
use the `Should(t)` overload instead of `Should()`.

If you want all the details, here they are:

* Implement the assertion as an [extension
  method](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/type-extensions#extension-methods) for
  `Testable` (the first argument), with whatever constraints you need. The constraints could be implicitly imposed by
  F#, as with `Be` where it requires `equality` on `'a` due to the use of the inequality operator (`<>`), or they could
  be explicitly specified, for example by specifying more concrete types (such as `Testable<'a option>` in order to have
  your extension only work for `option`-wrapped types).

* Accept whichever arguments you need for your assertion, and end with an optional `?because` parameter.

* First in your method, call `use _ = t.Assert()`. This is needed to track important state necessary for subject
  names to work. If your assertion is a higher-order assertion (like `Satisfy`) that calls user code that is expected to
  call other assertions, call `t.Assert(true)` instead. If your assertion calls the same assertions for each item in a
  sequence, call `t.Assert(true, true)` instead, and additionally call `use _ = t.AssertItem()` before the assertion of
  each item.

* If your condition is not met and the assertion should fail, call `t.Fail(because)`, optionally with any number
  of `With(key, value)` or `With(condition, key, value)` before `Fail`:

   ```f#
   t.With("Key 1", value1).With("Key 2", value2).Fail(because)
   ```

  Note that the keys `"Subject"`, `"Because"`, and `"Should"` are reserved by Faqt.

  Which key-value pairs, if any, to add to the message is up to you. Most assertion failure messages are more helpful if
  they display value being tested (`t.Subject`). Faqt mostly places this as the last value, so that if its rendering is
  very large, it does not push other important details far down.

  If you add values where you wrap user-supplied data (e.g., in a record, list or similar), then you should wrap the
  user values in the single-case DU `TryFormat`. This will ensure that if the value fails serialization, a fallback
  formatter will be used for that value. If `TryFormat` is not used, only the top-level items added using `With` will
  have this behavior, which may cause a (less useful) fallback formatter to be used for your top-level values instead of
  its (user-supplied) constituent parts.

  If you use anonymous type values in your assertion, note that anonymous type members seems to appear in alphabetical
  order, not declaration order. If this is not desired, use a normal record.

* If your assertion extracts derived state that can be used for further assertions,
  return `AndDerived(t, derivedState)`. Otherwise return `And(t)`. Prefer `AndDerived` over `And` if at all relevant,
  since it strictly expands what the user can do.

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
See [this StackOverflow answer](https://stackoverflow.com/a/34188952/2978652) for valid prefix operators. However, your
custom operator will then be shown in the subject name (whereas `%` is automatically removed).

## Customizing the format

Faqt's formatter is implemented as a simple function with signature `FailureData -> string`.

```f#
open Faqt
open Formatting

let myFormatter : FailureData -> string =
    // You can implement your own formatter from scratch, or modify the default one as shown here
    YamlFormatterBuilder.Default
        // Override System.Text.Json options
        .ConfigureJsonSerializerOptions(fun opts -> opts.MaxDepth <- 5)
        // Override FSharp.SystemTextJson options
        .ConfigureJsonFSharpOptions(fun opts -> opts.WithUnionAdjacentTag())
        // Add custom System.Text.Json converters
        .AddConverter(MyJsonConverter())
        // Easily transform values before serializing without needing a custom converter
        .SerializeAs(fun (t: System.Type) -> t.FullName)
        // Same as above, but does not apply to subtypes
        .SerializeExactAs(...)
        // Set how values wrapped in TryFormat are formatted when serialization fails
        .TryFormatFallback(fun ex obj -> {|
            Exception = ex
            ValueToString = obj.ToString()
        |})
        // Set the YamlDotNet visitor (inheriting from YamlVisitorBase) that is used after loading
        // the serialized JSON into a YAML document
        .SetYamlVisitor(MyYamlVisitor)
        // Build the formatter
        .Build()

// Set the default formatter
Formatter.Set(myFormatter)

// Set the formatter for a certain scope (until the returned value is disposed)
use _ = Formatter.With(myFormatter)
```

## Security considerations

**Treat assertion exception messages (and therefore test failure messages) as securely as you treat your source code.**

Faqt derives subject names from your source code. Known existing limitations (see below) as well as bugs can cause Faqt
to use a lot more of your code in the subject name than intended (up to entire source files). Therefore, do not give
anyone access to Faqt assertion failure messages that should not have access to your source code.

## FAQ

### Which testing frameworks does Faqt work with?

All of them. XUnit, NUnit, MSTest, NSpec, MSpec, Expecto, you name it. Faqt is agnostic to the test framework (and can
also be used in non-test production code, as previously described); it simply throws a custom exception when an
assertion fails.

### Why is the subject name not correct in my specific example?

The automatic subject name (the first part of the assertion message) is correct in most situations, but there are edge
cases where it may produce unexpected results:

* Multi-line strings literals will be concatenated.
* Lines starting with `//` in multi-line string literals will be removed.
* Subject names will be truncated if they are too long (currently 1000 characters, though that may change without
  notice), since it is then likely that a limitation or a bug is causing Faqt to use too large parts of the source code
  as the subject name.
* The subject name may be incorrect under the following conditions:
  * Assertion chains not starting on a new line or at the start of a lambda (`fun ... ->`)
  * Nested `Satisfy`, `AllSatisfy` or other higher-order assertions
  * `SatisfyAny` or similar with multiple assertion chains all on the same line containing the same assertion
  * Assertion chains not fully completing on a single thread
  * Assertion chains containing non-assertion methods with the same name as an assertion
  * Situations where assertions are not invoked in source order, such as for assertions chained after `AllSatisfy` if
    the sequence is empty

If you have encountered a case not covered above, please raise an issue. If I can't or won't fix it, I can at the very
least document it as a known limitation.

These limitations are due to the implementation of automatic subject names. The implementation is based on clever use of
caller info attributes, parsing source code from either local files or embedded resources, thread-local state, and
simple regex-based processing/replacement of the call chain based on which assertions have been encountered so far.

If you would like to help make the automatic subject name functionality more robust, please raise an issue. You can find
the relevant code in [SubjectName.fs](https://github.com/cmeeren/Faqt/blob/main/src/Faqt/SubjectName.fs).

### Why do I have to use `Should(())` inside an assertion chain?

This is due to how subject names are implemented, and the solution was chosen as the lesser of several evils. The
details are probably boring, but in short, when an assertion fails, Faqt needs to know the chain of assertions
encountered in the source code in order to derive the correct subject name. This chain is stored in thread-local state,
and has to be reset when a new assertion chain starts. This is done in `Should()`. However, that would ruin the subject
name for assertions after subsequent `Should()` calls in the chain.

Alternative solutions would either require making the assertion syntax more verbose (e.g. by enclosing entire assertion
chains in some method call, or wrapping them in a `use` statement in order to reset the thread-local state or avoid it
entirely), or make the subject name incorrect in many more cases (e.g. by removing the tracking of the encountered
assertion history altogether, thereby only giving correct subject names up to the first assertion of any given name in a
chain).

### Why does this assertion pass/fail for null?

Note: I recognize that the below is not the only way to look at the issue. If you fundamentally disagree with this
policy, I am open to discussing it. Please raise an issue.

It really boils down to assumptions about Faqt users would expect and find useful. For example, I assume that
making `HaveLength(0)` pass for `null` values would be a surprise for many users, and therefore be a bad idea. On the
other hand, allowing null values in assertions makes the assertions more composable, since it is trivial to
add `.NotBeNull()` to the start of your assertion chain if you want to require a non-`null` value for an assertion that
allows it, and somewhat harder to allow a `null` in an assertion that requires it non-`null` value (you'd have to use
something like `SatisfyAny`).

That being said, in order to find some guiding principles, the general policy on allowing or disallowing `null` subject
values is based on the following:

* `null` is separate from "empty". Values that are `null` do not have properties like "length" and "contents", whereas
  empty values do.
* Negative assertions (like `NotBeEmpty` or `NotContain`) essentially assert the _lack_ of a property, e.g., the lack of
  a specific length.

With that in mind, `null` subject values are generally allowed in negative assertions and disallowed in positive
assertions. For example, `HaveLength(0)` will fail for `null`, because a `null` value does not have any length (zero or
otherwise). Contrariwise, `NotHaveLength(0)` asserts the lack of having the length `0`, and will pass for `null` values
since they, indeed, do not possess the property of having that specific length.

Another way to look at it is that negative assertions could be thought of conceptually as e.g. `not (HaveLength(0))`,
i.e., just an inversion of the corresponding positive assertion. In this light, anything that fails the positive
assertion (including `null`) should pass the negative assertion.

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

### Why not Unquote?

[Unquote](https://github.com/SwensenSoftware/unquote) is a great library built on a great idea: Use code quotations with
arbitrary `bool`-returning F# expressions as your assertions, and Unquote will display step-by-step evaluations if the
assertion fails. This allows you to assert whatever you want without needing custom-made assertions.

Unfortunately, I stopped using it because of several issues:

* Its assertion messages are not very helpful for non-trivial objects or expressions. The rendering is confusing, and it
  can be very hard to see what the actual error is. Often, I resorted to debugging instead of reading the assertion
  message, because that turned out to be quicker.
* It is based around F# quotations, which have several limitations, for example regarding generic functions and mutable
  values. Simply put, not all F# code can be used in a quotation.
* It can not be used to extract values for further testing or similar (which is supported by Faqt's `BeSome` and similar
  assertions).
* I need assertions that can work in production code, too, and I assume that evaluating quotations has a significant
  performance impact (I have admittedly not measured this, since I stopped using it for the reasons above anyway).

### Can I use Faqt from C#?

Faqt is designed only for F#. The subject names only work correctly for F#, and the API design and assertion choices are
based on F# idioms and expected usage. Any support for C# is incidental, and improving or even preserving C# support is
out of scope for Faqt. You are likely better off with FluentAssertions for C#.
