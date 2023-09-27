# Faqt documentation

<img src="https://raw.githubusercontent.com/cmeeren/Faqt/main/logo/faqt-logo-docs.png" width="300" align="right" />

## Table of contents

<!-- TOC -->

* [Security considerations](#security-considerations)
* [Installation and requirements](#installation-and-requirements)
* [Avoiding `|> ignore` after assertion chains](#avoiding--ignore-after-assertion-chains)
* [Writing your own assertions](#writing-your-own-assertions)
  * [A basic assertion](#a-basic-assertion)
  * [Derived state](#derived-state)
  * [Higher-order assertions](#higher-order-assertions)
  * [Full guide](#full-guide)
* [Customizing the format](#customizing-the-format)
* [Configuring options](#configuring-options)
* [Assertion list](#assertion-list)
  * [Higher-order assertions](#higher-order-assertions-1)
  * [Basic assertions](#basic-assertions)
  * [Comparison assertions](#comparison-assertions)
  * [Union assertions](#union-assertions)
  * [`bool` assertions](#bool-assertions)
  * [`Nullable<_>` assertions](#nullable-assertions)
  * [`string` assertions](#string-assertions)
  * [`IDictionary<_, _>` assertions](#idictionary--assertions)
  * [`seq<_>` assertions](#seq-assertions)
  * [`Guid` assertions](#guid-assertions)
  * [`Enum` assertions](#enum-assertions)
  * [Function assertions](#function-assertions)
  * [`HttpResponseMessage` assertions](#httpresponsemessage-assertions)
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

## Security considerations

**Treat assertion exception messages (and therefore test failure messages) as securely as you treat your source code.**

Faqt derives subject names from your source code. Known existing limitations (described later in this document) as well
as bugs can cause Faqt to use a lot more of your code in the subject name than intended (up to entire source files).
Therefore, do not give anyone access to Faqt assertion failure messages if they should not have access to your source
code.

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

   Note that `DebugType=embedded` is automatically set
   by [DotNet.ReproducibleBuilds](https://github.com/dotnet/reproducible-builds) if you use that.

## Avoiding `|> ignore` after assertion chains

Since assertions return `And` or `AndDerived`, F# may warn you in some cases if an assertion chain is not ignored
using `|> ignore`.

For convenience, you can `open Faqt.Operators` and use the `%` prefix operator instead:

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

## Writing your own assertions

Writing your own assertions is easy! Custom assertions are implemented exactly like Faqt’s built-in assertions, so you
can always look at those for inspiration (see all files ending with `Assertions`
in [this folder](https://github.com/cmeeren/Faqt/tree/main/src/Faqt)).

All the details are further below, but first, we'll get a long way just by looking at some examples.

### A basic assertion

Here is Faqt’s simplest assertion, `Be`:

```f#
open System.Runtime.CompilerServices
open Faqt
open Faqt.AssertionHelpers

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

Simple, right? Using the default YAML-based formatter, it will result in this message:

```
Subject: <expression from user code>
Because: <as specified by user, skipped if None>
Should: Be
Expected: <expected value>
But was: <subject value>
```

As you can see, Faqt automatically adds `Subject`, `Because` (if supplied by the user), and `Should` (the name of the
method where you call `t.Assert()`). After that, any additional key-value pairs you specify are displayed in order.

### Derived state

Now let's look at an assertion that's just as simple, but uses derived state, where you return
`AndDerived` instead of `And`:

```f#
open System
open System.Runtime.CompilerServices
open Faqt
open Faqt.AssertionHelpers

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

This allows users to continue asserting on the derived state (the inner value, in this case), for example like
this: `nullableInt.Should().HaveValue().That.Should(()).Be(2)`.

### Higher-order assertions

Finally, let's look at a more complex assertion - a higher-order assertion that calls user assertions and which also
asserts for every item in a sequence:

```f#
open System
open System.Runtime.CompilerServices
open Faqt
open Faqt.AssertionHelpers
open Faqt.Formatting

type private SatisfyReportItem = { Index: int; Failure: FailureData }

[<Extension>]
type Assertions =

    /// Asserts that all items in the collection satisfy the supplied assertion.
    [<Extension>]
    static member AllSatisfy(t: Testable<#seq<'a>>, assertion: 'a -> 'ignored, ?because) : And<_> =
        use _ = t.Assert(true, true)

        if isNull (box t.Subject) then
            t.With("But was", t.Subject).Fail(because)

        let failures =
            t.Subject
            |> Seq.indexed
            |> Seq.choose (fun (i, x) ->
                try
                    use _ = t.AssertItem()
                    assertion x |> ignore
                    None
                with :? AssertionFailedException as ex ->
                    Some { Index = i; Failure = ex.FailureData }
            )
            |> Seq.toArray

        if failures.Length > 0 then
            t.With("Failures", failures).With("Subject value", t.Subject).Fail(because)

        And(t)
```

Note that in this case we use `t.Assert(true, true)` at the top. Both parameters are optional. The first `true`
indicates that this is a higher-order assertion, and the second `true` indicates that the assertions are run for each
item in a sequence. Note also that we call `use _ = t.AssertItem()` before the assertion of each item.

### Full guide

The most significant thing _not_ demonstrated in the examples above is that if your assertion calls `Should`, make sure
to use the `Should(t)` overload instead of `Should()`.

If you want all the details, here they are:

* Open `Faqt.AssertionHelpers`.

* If needed, open `Faqt.Formatting` to get access to `TryFormat` (described below) and `FailureData` (mostly useful for
  higher-order assertions as demonstrated above).

* Implement the assertion as
  an [extension method](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/type-extensions#extension-methods)
  for `Testable` (the first argument), with whatever constraints you need. The constraints could be implicitly imposed
  by F#, as with `Be` where it requires `equality` on `'a` due to the use of the inequality operator (`<>`), or they
  could be explicitly specified, for example by specifying more concrete types (such as `Testable<'a option>` in order
  to have your extension only work for `option`-wrapped types).

* Accept whichever arguments you need for your assertion, and end with an optional `?because` parameter.

* First in your method, call `use _ = t.Assert()`. This is needed to track important state necessary for subject names
  to work. The method has two optional boolean parameters. If your assertion is a higher-order assertion that calls user
  code that is expected to call other assertions (like `Satisfy`), use `t.Assert(true)`. If your assertion additionally
  calls the same user assertion(s) for each item in a sequence (like `AllSatisfy`), call `t.Assert(true, true)`, and
  additionally call `use _ = t.AssertItem()` before the assertion of each item.

* If your condition is not met and the assertion should fail, call `t.Fail(because)`, optionally with any number
  of `With(key, value)` or `With(condition, key, value)` before `Fail`:

   ```f#
   t.With("Key 1", value1).With("Key 2", value2).Fail(because)
   ```

  Note that the keys `"Subject"`, `"Because"`, and `"Should"` are reserved by Faqt.

  Which key-value pairs, if any, to add to the message is up to you. Most assertion failure messages are more helpful if
  they display value being tested (`t.Subject`). Faqt mostly places this as the last value, so that if its rendering is
  very large, it does not push other important details far down. If you do render `t.Subject` and there is no logical
  key that fits (such as `"But was"` in the assertion `Be`), the recommended key is `"Subject value"`.

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

* If your assertion calls other assertions, consider how your assertion method name will read when used with the
  assertion message from the called assertion. For example, `Be` has a message like:

  ```
  Subject: x
  Should: Be
  Expected: true
  But was: false
  ```

  If the `bool` assertion `BeTrue` just called `Be(true)` internally, it would read like:

  ```
  Subject: x
  Should: BeTrue
  Expected: true
  But was: false
  ```

  The `Expected: true` line is superfluous given the name of the assertion. Therefore, it's better with a separate
  assertion message for `BeTrue` that does not include `Expected`, thereby producing this improved message:

  ```
  Subject: x
  Should: BeTrue
  But was: false
  ```

## Customizing the format

Faqt's formatter is implemented as a simple function with signature `FailureData -> string`. You can implement your own
formatter from scratch (the `FailureData` members correspond to the keys in the default assertion messages and are
hopefully self-explanatory), or easily configure the built-in YAML-based formatter to your liking as shown below:

```f#
open Faqt
open Faqt.Formatting

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
        .TryFormatFallback(fun _ex obj -> obj.ToString())
        // Set the YamlDotNet visitor (inheriting from YamlDotNet.RepresentationModel.YamlVisitorBase)
        // that is used after loading the serialized JSON into a YAML document
        .SetYamlVisitor(MyYamlVisitor)
        // Build the formatter
        .Build()

// Set the default formatter
Formatter.Set(myFormatter)

// Set the config for a certain scope (until the returned value is disposed)
use _ = Formatter.With(myConfig)
```

## Configuring options

Faqt contains some configurable options, which are adjusted similarly to the formatter:

```f#
open Faqt
open Faqt.Configuration

// Create a configuration
let myConfig =
    FaqtConfig.Default
        // Set the maximum length of rendered HttpContent in assertion failure output
        .SetHttpContentMaxLength(1024 * 1024)
        // Disable formatting of HttpContent (such as indenting JSON for readability)
        .SetFormatHttpContent(false)

// Set the default config
Config.Set(myConfig)

// Set the config for a certain scope (until the returned value is disposed)
use _ = Config.With(myConfig)

// Config.Current is available globally and can be used in your own converters/formatters.
myFormatter Config.Current
```

## Assertion list

### Higher-order assertions

* `Satisfy`
* `NotSatisfy`
* `SatisfyAny`
* `SatisfyAll`

### Basic assertions

* `Be`: Structural or custom equality
* `NotBe`: Structural or custom equality
* `BeOneOf`: Structural or custom equality with multiple candidates
* `NotBeOneOf`: Structural or custom equality with multiple candidates
* `BeSameAs`: Reference equality
* `NotBeSameAs`: Reference equality
* `BeNull`
* `NotBeNull`
* `Transform`: Parsing or other transformations that can throw
* `TryTransform`: Same as `Transform`, but for functions returning `Option`, `Result`, or `bool * 'a` (
  like `Int32.TryParse`)
* `BeOfType`: Exact type check
* `BeAssignableTo`: Polymorphic type check

### Comparison assertions

* `BeCloseTo`: Same as `Be`, but with a tolerance
* `NotBeCloseTo`: Same as `NotBe`, but with a tolerance
* `BeGreaterThan`
* `BeGreaterThanOrEqualTo`
* `BeLessThan`
* `BeLessThanOrEqualTo`
* `BePositive`
* `BeNegative`
* `BeNonNegative`
* `BeNonPositive`
* `BeInRange`: Inclusive range

### Union assertions

* `BeOfCase`: Assert DU case and continue asserting on the inner value
* `BeSome`
* `BeNone`
* `BeOk`
* `BeError`

### `bool` assertions

* `BeTrue`
* `BeFalse`
* `Imply`: If the subject is true, the specified value must also be true
* `BeImpliedBy`: If the specified value is true, the subject must also be true

### `Nullable<_>` assertions

* `HaveValue`
* `NotHaveValue`
* `BeNull`
* `NotBeNull`

### `string` assertions

* `BeUpperCase`: Case check with invariant or specified culture
* `BeLowerCase`: Case check with invariant or specified culture
* `Be`: Equality with specified comparison type (the normal `Be` uses ordinal comparison)
* `NotBe`: Equality with specified comparison type (the normal `NotBe` uses ordinal comparison)
* `Contain`: Substring check with ordinal or specified comparison type
* `NotContain`: Substring check with ordinal or specified comparison type
* `StartWith`: Prefix check with ordinal or specified comparison type
* `NotStartWith`: Prefix check with ordinal or specified comparison type
* `EndWith`: Suffix check with ordinal or specified comparison type
* `NotEndWith`: Suffix check with ordinal or specified comparison type
* `MatchRegex`
* `NotMatchRegex`
* `MatchWildcard`: Simplified wildcard check with `*` (zero or more characters) and `?` (one character)
* `NotMatchWildcard`: Simplified wildcard check with `*` (zero or more characters) and `?` (one character)
* All `seq<_>` assertions, including:
  * `HaveLength`
  * `BeEmpty`
  * `NotBeEmpty`
  * `BeNullOrEmpty`

### `IDictionary<_, _>` assertions

* `Contain`: Member check with key and value
* `NotContain`: Member check with key and value
* `HaveSameItemsAs`
* `ContainKey`
* `NotContainKey`
* `ContainValue`
* `NotContainValue`
* All `seq<_>` assertions, including:
  * `AllSatisfy`
  * `SatisfyRespectively`
  * `HaveLength`
  * `BeEmpty`
  * `NotBeEmpty`
  * `BeNullOrEmpty`
  * `Contain`: Member check with `KeyValuePair<_, _>`
  * `NotContain`: Member check with `KeyValuePair<_, _>`
  * `ContainExactlyOneItem`
  * `ContainExactlyOneItemMatching`
  * `ContainAtLeastOneItem`
  * `ContainAtLeastOneItemMatching`
  * `ContainItemsMatching`
  * `BeSupersetOf`
  * `BeProperSupersetOf`
  * `BeSubsetOf`
  * `BeProperSubsetOf`
  * `IntersectWith`
  * `NotIntersectWith`

### `seq<_>` assertions

* `AllSatisfy`: Higher-order assertion where all values must satisfy the supplied assertion
* `SatisfyRespectively`: Higher-order assertion where each value must satisfy the respective supplied assertion
* `HaveLength`
* `BeEmpty`
* `NotBeEmpty`
* `BeNullOrEmpty`
* `Contain`: Member check with structural equality
* `NotContain`: Member check with structural equality
* `AllBe`: Identical items check with structural equality
* `AllBeMappedTo`: Identical mapped items check with structural equality
* `AllBeEqual`
* `AllBeEqualBy`
* `SequenceEqual`: Item-wise check with structural equality
* `HaveSameItemsAs`: Order-ignoring items check with structural equality
* `ContainExactlyOneItem`
* `ContainExactlyOneItemMatching`
* `ContainAtLeastOneItem`
* `ContainAtLeastOneItemMatching`
* `ContainItemsMatching`
* `BeDistinct`
* `BeDistinctBy`
* `BeAscending`
* `BeAscendingBy`
* `BeDescending`
* `BeDescendingBy`
* `BeStrictlyAscending`
* `BeStrictlyAscendingBy`
* `BeStrictlyDescending`
* `BeStrictlyDescendingBy`
* `BeSupersetOf`
* `BeProperSupersetOf`
* `BeSubsetOf`
* `BeProperSubsetOf`
* `IntersectWith`
* `NotIntersectWith`

### `Guid` assertions

* `Be`: Equality check against `string`
* `NotBe`: Equality check against `string`
* `BeEmpty`
* `NotBeEmpty`

### `Enum` assertions

* `HaveFlag`
* `NotHaveFlag`

### Function assertions

* `Throw`: Polymorphic exception check for top-level exception
* `ThrowInner`: Polymorphic exception check for top-level or inner exception on any level (including any exception in
  an `AggregateException`)
* `ThrowExactly`: Exact exception check for top-level exception
* `NotThrow`
* `Roundtrip`: Check that a (potentially `Option` or `Result`-returning) function returns the input value. The function
  is typically a composition of parsing and extracting a value, e.g., `(fromX >> toX).Should().Roundtrip(value)`.

### `HttpResponseMessage` assertions

All assertion failure messages contain the full response and the original request.

* `HaveStatusCode`
* `Be1XXInformational`
* `Be2XXSuccessful`
* `Be3XXRedirection`
* `Be4XXClientError`
* `Be5XXServerError`
* `Be100Continue`
* `Be101SwitchingProtocols`
* `Be200Ok`
* (etc. for other status codes)
* `HaveHeader`: Check for the existence of a header (and continue asserting on the header value(s))
* `HaveHeaderValue`: Check for the existence of a header with a specific value

## FAQ

### Which testing frameworks does Faqt work with?

All of them. XUnit, NUnit, MSTest, NSpec, MSpec, Expecto, you name it. Faqt is agnostic to the test framework (and can
also be used in non-test production code); it simply throws a custom exception when an assertion fails.

### Why is the subject name not correct in my specific example?

The automatic subject name (the first part of the assertion message) is correct in most situations, but there are edge
cases where it may produce unexpected results:

* Multi-line strings literals will be concatenated.
* Lines starting with `//` in multi-line string literals will be removed.
* Subject names will be truncated if they are too long (currently 1000 characters, though that may change without
  notice). This is because it is then likely that a limitation or a bug is causing Faqt to use too large parts of the
  source code as the subject name.
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
other hand, _allowing_ null values in assertions makes the assertions more composable, since it is trivial to
add `.NotBeNull()` to the start of your assertion chain if you want to require a non-`null` value for an assertion that
allows it (and somewhat harder to allow a `null` in an assertion that requires a non-`null` value, where you'd have to
use something like `SatisfyAny`).

That being said, in order to find some guiding principles, the general policy on allowing or disallowing `null` subject
values is based on the following:

* `null` is separate from "empty". Values that are `null` do not have properties like "length" and "contents", whereas
  empty values do.
* Negative assertions (like `NotBeEmpty` or `NotContain`) essentially assert the _lack_ of a property, e.g., the lack of
  a specific length.

With that in mind, `null` subject values are generally allowed in negative assertions and disallowed in positive
assertions. For example, `HaveLength(0)` will fail for `null`, because a `null` value does not have any length (zero or
otherwise). Contrariwise, `NotHaveLength(0)` (if it existed) would assert the lack of having the length `0`, and will
pass for `null` values since they, indeed, do not possess the property of having that specific length.

Another way to look at it is that negative assertions could be thought of conceptually as e.g. `not (HaveLength(0))`,
i.e., just an inversion of the corresponding positive assertion. In this light, anything that fails the positive
assertion (including `null`) should pass the negative assertion.

The only exceptions are for assertions that check equality, such as `Be` or `BeSameAs`. Here, `null` is considered equal
to `null` (which is consistent with the default F# implementations of structural and reference equality). This also
extends to `SequenceEqual` and `HaveSameItemsAs`, which will pass if both sequences are `null`.

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
* The relatively free-form assertion messages of FluentAssertions are harder to parse than more structured output,
  especially for complex objects and collections.
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
* I need assertions that can work in production code, too. I assume that evaluating quotations has a significant
  performance impact. (I have admittedly not measured this, since I stopped using it for the reasons above anyway.)

### Can I use Faqt from C#?

Faqt is designed only for F#. The subject names only work correctly for F#, and the API design and assertion choices are
based on F# idioms and expected usage. Any support for C# is incidental, and improving or even preserving C# support is
out of scope for Faqt. You are likely better off with FluentAssertions for C#.
