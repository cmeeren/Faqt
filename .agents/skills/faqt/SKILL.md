---
name: faqt
description: Write and review F# tests and assertions using Faqt.
---

# Faqt

This skill describes Faqt 6.x. Use guidance matching the project's package version and follow its existing test framework and assertion conventions. The links below track development; for released versions, read the same files at Git tag `v/<version>` instead of `main`.

## Fluent assertions

Start each assertion chain with `Should()`. Use `And` for another assertion on the same subject. Assertions accept an optional `because` argument for explaining the requirement.

```fsharp
open Faqt
open Faqt.Operators

let count = 3
%count.Should().BePositive().And.BeLessThan(10, "There are fewer than ten seats")
```

The optional `%` operator discards the return value; `|> ignore` works too.

Assertions such as `BeSome`, `BeOk`, and `ContainExactlyOneItem` expose a derived value through `Whose`. Use `Should(())` for subsequent assertions on derived values or their properties within the same chain; the double parentheses preserve subject names in diagnostics. A separately extracted value starts a new chain with `Should()`.

```fsharp
let result = Some "Ada"
%result.Should().BeSome().Whose.Length.Should(()).Be(3)

let name = result.Should().BeSome().Whose
%name.Should().Be("Ada")
```

`WhoseValue`, `That`, and `Derived` are aliases for `Whose`. `Subject` returns the original value directly.

## Choose assertions that explain failures

Prefer assertions on actual values: `actual.Should().Be(expected)` preserves both values in the failure message.

Before reducing a check to a boolean, composing several assertions to express one requirement, or writing a custom assertion, check the relevant subject-type section of the [assertion list](https://github.com/cmeeren/Faqt/blob/main/DOCUMENTATION.md#assertion-list) for a built-in assertion that expresses the requirement directly. Consult the installed package's XML documentation or [source](https://github.com/cmeeren/Faqt/tree/main/src/Faqt) when signatures or behavior are unclear.

For collections, use `SequenceEqual` when order matters and `HaveSameItemsAs` when it does not; both account for repeated items. Use `ContainExactlyOneItem` to assert cardinality and retrieve the item. `AllSatisfy` checks every item and collects failures with item indexes; it also passes for an empty collection, so add `NotBeEmpty` when non-emptiness is part of the requirement.

A chain stops at its first failure. Use `SatisfyAll` for independent checks that should report failures together. Each callback starts its own chain with `Should()` and still stops at its first failure.

```fsharp
let customer = {| Name = "Ada"; Age = 37 |}

%customer.Should().SatisfyAll(
    [
        fun c -> %c.Name.Should().NotBeEmpty()
        fun c -> %c.Age.Should().BeGreaterThanOrEqualTo(18)
    ]
)
```

## Exception assertions and higher-order behavior

Higher-order assertions distinguish `AssertionFailedException` from unexpected errors. `NotSatisfy` negates assertion failures; `SatisfyAny` tries alternatives until one succeeds. Unexpected errors stop evaluation. Assertions explicitly testing exception outcomes or operation success follow their own contracts; see [exception behavior](https://github.com/cmeeren/Faqt/blob/main/DOCUMENTATION.md#assertion-failures-and-unexpected-exceptions).

To assert a particular exception, put the operation in a `unit -> _` function and use `Throw<ExpectedException, _>()` (accepts subtypes) or `ThrowExactly<ExpectedException, _>()`. Negating an operation-success assertion only proves that it failed, not why.

## Custom assertions and diagnostics

For custom assertions, follow the [extension guide](https://github.com/cmeeren/Faqt/blob/main/DOCUMENTATION.md#writing-your-own-assertions): use `t.Assert()`, report assertion failures with `Fail`, and return `And` or `AndDerived` for chaining. `RaiseError` adds context to unexpected errors; custom higher-order assertions should treat only `AssertionFailedException` as an assertion failure.

When path mapping or unavailable source files affect subject names, check the [source embedding setup](https://github.com/cmeeren/Faqt/blob/main/DOCUMENTATION.md#installation-and-requirements): set `DebugType=embedded` and `EmbedAllSources=true` in projects that call assertions.

For rendering or HTTP diagnostics, see [formatting](https://github.com/cmeeren/Faqt/blob/main/DOCUMENTATION.md#customizing-the-format), [configuration](https://github.com/cmeeren/Faqt/blob/main/DOCUMENTATION.md#configuring-options), and [security considerations](https://github.com/cmeeren/Faqt/blob/main/DOCUMENTATION.md#security-considerations). Failure messages can contain source and sensitive values. Header masking only covers headers, and disabling HTTP body previews does not suppress body values reported by content assertions.
