Faqt
====

**Fantastic fluent assertions for your F# tests and domain code.**

<img src="https://raw.githubusercontent.com/cmeeren/Faqt/main/logo/faqt-logo-docs.png" width="300" align="right" />

Faqt improves on the best of [FluentAssertions](https://github.com/fluentassertions/fluentassertions)
and [Shouldly](https://github.com/shouldly/shouldly) and serves it steaming hot on a silver platter to the discerning F#
developer.

**It aims to be the best assertion library for F#.**

If you don't agree, I consider that a bug - please raise an issue. 😉

### Versioning and breaking changes

Faqt follows [SemVer v2.0.0](https://semver.org/) and aims to preserve **source and binary** compatibility between
releases, except when the major version is incremented. Note that any change to the assertion message format is
considered a non-breaking change.

## Table of contents

<!-- TOC -->

* [A motivating example](#a-motivating-example)
* [Installation and requirements](#installation-and-requirements)
* [Faqt in a nutshell](#faqt-in-a-nutshell)
* [Documentation](#documentation)
* [Contributing](#contributing)

<!-- TOC -->

## A motivating example

Here is an example of what you can do with Faqt. Simply use `Should()` to start asserting. For subsequent calls
to `Should` in the same chain, use `Should(())` (double parentheses - this is required for subject names to work
properly). Like FluentAssertions, all assertions support an optional "because" parameter that will be used in the
output.

```f#
// Example type definition for clarity
type Customer =
    | Internal of {| ContactInfo: {| Name: {| LastName: string |} |} option |}
    | External of {| Id: int |}

open Faqt

// Assertions in test or domain code
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
But was: None
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
2. If you use path mapping, deterministic source paths, or want to execute assertions where source files are not
   available (e.g. in production), set `DebugType` to `embedded` and `EmbedAllSources` to `true`. For more details, see
   the [documentation](DOCUMENTATION.md).

## Faqt in a nutshell

As expected by the discerning F# developer, Faqt is:

- **Readable:** Assertions read like natural language and clearly reveal their intention.
- **Concise:** Assertion syntax verbosity is kept to an absolute minimum.
- **Usable:** Faqt comes with batteries included, and contains many useful assertions, including aliases
  (like `BeTrue()` for `Be(true)` on booleans, and `BeSome` for `BeOfCase(Some)` on `option` values).
- **Safe:** Assertions are as type-safe as F# allows.
- **Extensible:** No assertion? No problem! Writing your own assertions is very simple (details in
  the [documentation](DOCUMENTATION.md)).
- **Informative:** The assertion failure messages are designed to give you all the information you need in a consistent
  and easy-to-read format.
- **Discoverable:** The fluent syntax means you can just type a dot to discover all possible assertions and actions on
  the current value.
- **Composable:** As far as possible, assertions are orthogonal (they check one thing only). For example,
  predicate-based collection assertions pass for empty collections, just like F#'s `Seq.forall` and similar. You can
  chain assertions with `And`, `Whose`, `WhoseValue`, `That`, `Derived`, and `Subject`, assert on derived values with
  assertions like `BeSome`, and compose assertions with higher-order assertions like `Satisfy` and `SatisfyAll`.
- **Configurable:** You can configure, either globally or for a specific scope (such as a test), how assertion failure
  messages are rendered, as well as other configuration. You can easily tweak the default rendering or completely
  replace the formatter.
- **Production-ready:** Faqt is very well tested and is highly unlikely to break your code, whether test or production.

## Documentation

See the [documentation](DOCUMENTATION.md) for additional details, such as the list of assertions, how to use the
optional `%` operator (alias for `ignore`), instructions on writing your own assertions, customizing the output format,
security considerations, and a FAQ with, among other things, a brief comparison with other assertion frameworks.

## Contributing

Contributions are welcome! Please see the [contribution guidelines](CONTRIBUTING.md) before opening an issue or pull
request.
