Release notes
==============

### Unreleased

#### Breaking changes

* HTTP failure diagnostics now limit captured body bytes before decoding, as well as rendered characters, using
  `HttpContentMaxLength`. Stream previews start at the current position, except byte-array and string content.
  Seekable positions are restored; nonseekable bodies may be consumed, as indicated in the diagnostics.
  Zero omits the body without reading it.

* Unexpected callback, comparer, and HTTP content-read exceptions retain Faqt's diagnostic context but
  now use ordinary `Exception` wrappers instead of `AssertionFailedException`, preserving the original exception in
  the `InnerException` chain. Aggregation and evaluation of further alternatives stop on an unexpected error.
  Ordinary assertion failures and explicit exception-testing contracts retain their behavior.
  `OperationCanceledException` and its subtypes propagate without wrapping. Custom assertions can use
  `t.With(...).RaiseError(ex, because)` to report unexpected errors with context.

* `Throw`, `ThrowExactly`, `ThrowInner`, `NotThrow`, and all `Roundtrip` overloads now reject null function
  subjects with `ArgumentNullException` (`ParamName = "subject"`) before invocation. A null function can no longer
  satisfy a `Throw` assertion through the `NullReferenceException` caused by attempting to invoke it. Exceptions
  thrown by an actual function retain their existing behavior.

* `BeCloseTo` and `NotBeCloseTo` now reject negative tolerances for supported built-in numeric types and
  `TimeSpan` with `ArgumentException`. Existing NaN handling takes precedence over this validation. Custom tolerance
  types retain their existing operator-based behavior.

* The `Guid` overloads of `Be` and `NotBe` now reject invalid string arguments with `ArgumentException`.
  Previously, malformed strings threw `FormatException`, and null strings threw `ArgumentNullException`.

* The `Type` overloads of `BeOfType` and `BeAssignableTo` now reject a null `expectedType` with
  `ArgumentNullException`, before checking the subject.

* Configuration and formatter APIs now reject null inputs with `ArgumentNullException` when supplied,
  rather than accepting them and potentially failing later. This applies to `Config.Set`/`With`,
  `Formatter.Set`/`With`, `FaqtConfig.SetMapHttpHeaderValues`, and `YamlFormatterBuilder` methods accepting callbacks
  or converters. `FaqtConfig.SetHttpContentMaxLength` now rejects negative lengths with `ArgumentException`.
  `YamlFormatterBuilder.SerializeAs` now rejects projected types assignable to the input type with
  `ArgumentException`, preventing the converter from recursively selecting itself. `SerializeExactAs` still allows
  subtype outputs because its converter matches only the exact input type.

#### Fixes and improvements

* Detect cycles and bound recursive serialization across nested `TryFormat` wrappers, including dictionary keys,
  so diagnostic formatting uses its fallback instead of overflowing the stack.

* Stop formatter projections that re-enter the same converter at the same JSON depth, preventing stack overflows
  from boxed self-projections. Custom object converters and projections of nested values remain supported.

* Preserve dictionary comparer and set comparison semantics when returning stored containment matches.
  Collection assertions avoid unnecessary re-enumeration; `ContainItemsMatching` retains its lazy derived sequence.
  Multiset equality and subset/superset assertions now handle single-pass sequences correctly. Dictionary equality
  checks item counts and reports mismatched values as a list so structurally equal distinct keys cannot collide.

* Fix decimal and `TimeSpan` overflow and unsigned underflow (including `UInt128`) in close-to
  comparisons. Reject NaN operands (including `Half.NaN`) in scalar comparisons and adjacent sequence-ordering
  comparisons, including projected ordering keys. Equality-based assertions continue to follow F# equality
  semantics, including NaN not being equal to itself.

* Match response and content headers consistently, including exact values and quoted comma-separated members.
  `HaveHeader` returns all matching header values for further assertions.

* Read bounded HTTP body previews, preserve content headers, and honor quoted charset parameters when decoding
  diagnostics. Restore seekable stream positions even when reading fails. Obtaining a stream may still buffer
  generated content, such as `JsonContent`, in full.

* Improve diagnostic formatting for `TryFormat` dictionary keys, serialization failures, throwing `ToString`
  implementations, and special floating-point values.

* Normalize JSON object-key order recursively for equivalence checks while preserving case-sensitive key names.

* Fix wildcard matching when literal text collides with the previous internal placeholder strings.

#### Clarifications

* Clarified zero enum flag semantics: `HaveFlag` passes and `NotHaveFlag` fails for a zero mask, consistently with
  `Enum.HasFlag`. Use equality with the enum's zero value to assert that no flags are set.

* Clarified that `SatisfyAny` passes when its assertion list is empty. This behavior is unchanged.

### 5.1.0 (2025-09-18)

* The `AssertionFailedException` message now begins with `Assertion failed.` on its own first line, with the structured
  failure data on the following lines. This change improves readability because many tools render the exception message
  on the same line as other text (e.g., the assertion type name), and the structured data (the YAML starting with
  `Subject: ...` when using the default formatter) should start on a new line for optimal readability. I considered
  prefixing the message with a newline, but most tooling trims leading newlines. This could be a breaking change if you
  programmatically parse exception messages, but that is assumed to not be a common use-case, so this release bumps the
  minor version only.

### 5.0.0 (2025-06-02)

**Breaking:** Faqt has been updated for nullable reference types, and now works best with them. This has caused some
necessary changes to Faqt's policy on null handling.

#### Background

To understand the reason for the changes, first a few points of background with limitations on F#'s implementation of
nullable reference types, to be referenced further below:

1. **Input vs. output nullability:** Many of Faqt's assertions used to pass for `null` subject values. However, for
   assertions that are not completely generic, such as `string` assertions, accepting null input requires adding a
   `| null` annotation to the subject type: `Testable<string | null>`. Since the assertion should pass for `null`, this
   means that the chainable value will also be nullable, i.e., `And<string | null>`. What we _want_ is for the return
   value to have the same nullability as the input (so it is known to be non-nullable if the input is non-nullable), but
   since it must be typed as e.g. `And<string | null>`, it will always be nullable, even if it is known that the subject
   is non-null at the callsite where the assertion is used. I expect that in the vast majority of cases, the input
   subject is known to be non-null, and it would be unnecessary boilerplate to have to deal with a nullable return value
   that will actually never be `null`.
2. **Generics and value types:** Unlike C#'s `?` annotation, F#'s `| null` annotation only works for reference types. If
   used with a generic type, it will constrain the type to be a reference type. This means that parameters that should
   accept both reference types and value types cannot have `| null`. This applies not only to fully generic parameters,
   but also to interfaces like `IDictionary<_, _>` or `seq<_>`: Adding `| null` here would prevent value-type
   implementations of these interfaces.

For the reasons above, and because one can now use `NotBeNull` to get from a nullable type to a non-nullable type to
continue asserting on, I have decided that most assertions no longer accept `null` values.

#### Specific changes

(Remarks about nullness warnings are only relevant if you have enabled nullable reference types.)

* Assertions that fail for `null` subjects or argument values now cause a nullness warning and, if actually called with
  `null` (due to ignoring nullness warnings or not using nullable reference types), will throw a generic
  `NullReferenceException` or similar instead of an `AssertionFailedException`. The following are exceptions:
  * `NotBeNull`, for obvious reasons: The whole point of this assertion is to check for `null` subjects. The runtime
    behavior is unchanged. At compile-time, it has been improved to guarantee that the return value is non-nullable.
  * `BeOfType`, `BeOfType<_>`, `BeAssignableTo` and `BeAssignableTo<_>`: These work exactly as before. Ideally they
    should statically reject `null` values, but that is not possible: They are "special" assertions implemented as
    intrinsic extension methods on `Testable<_>` to avoid callers having to specify an additional type parameter, and
    therefore can't introduce a non-null constraint on the subject. Adding a constraint here would require making them
    normal extension members, and callers would have to write `BeOfType<MyType, _>` instead of `BeOfType<MyType>`.
* Most assertions that used to pass for `null` subject values now no longer accept nullable types. They will produce
  nullness warnings if called with nullable types, and, if called with `null` (due to ignoring nullness warnings or not
  using nullable reference types), will now fail with an exception (typically a `NullReferenceException`). This applies
  to:
  * For reason #1: All assertions with subject types that are not fully generic:
    * `string` assertions:
      * `NotBe`
      * `NotContain`
      * `NotStartWith`
      * `NotEndWith`
      * `NotMatchRegex`
      * `NotMatchWildcard`
    * `Set<_>` assertions:
      * `NotContain`
  * For reason #1: The inner `string` in `seq<string>` assertions reject nulls at compile-time (i.e., reject
    `seq<string | null>`). The runtime behavior for any such `null` elements is unchanged. This applies to the following
    `seq<string>` assertions:
    * `BeAscending`
    * `BeDescending`
    * `BeStrictlyAscending`
    * `BeStrictlyDescending`
  * For reason #1 and #2: All assertions with interface subject types. This applies to:
    * `IDictionary<_, _>` assertions:
      * `NotContain`
      * `NotContainKey`
      * `NotContainValue`
      * `HaveSameItemsAs` (no longer passes if both dictionaries are `null`)
    * `seq<_>` assertions:
      * `NotContain`
      * `SequenceEqual` (no longer passes if both sequences are `null`)
      * `HaveSameItemsAs` (no longer passes if both sequences are `null`)
      * `NotContainItemsMatching`
      * `NotIntersectWith`
  * Due to being fully generic, the following assertions have actually been _loosened_; they now accept `null` for the
    subject and all arguments, and let then relevant operators handle nulls:
    * `BeCloseTo`
    * `NotBeCloseTo` (note that this no longer automatically passes for `null` subjects; instead, `null` values will be
      compared like any other value)
    * `BeGreaterThan`
    * `BeGreaterThanOrEqualTo`
    * `BeLessThan`
    * `BeLessThanOrEqualTo`
    * `BePositive`
    * `BeNegative`
    * `BeNonNegative`
    * `BeNonPositive`
    * `BeInRange`
* For reason #1: The `Be` and `NotBe` overloads for strings produce nullness warnings for `string | null` arguments.
  However, the runtime behavior is unchanged.
* The `BeNull` and `NotBeNull` assertions now emit a nullness warning if called with a non-nullable type, since it makes
  no sense to call them with non-nullable types. The runtime behavior is unchanged.
* `BeNullOrEmpty` now statically returns a nullable type. If you know statically that you have a non-`null` input, you
  can use `BeEmpty` instead to avoid having to deal with the nullable output. The runtime behavior is unchanged.
* `DeserializeTo`: This now fails if deserializing to `null` (i.e., if the subject is `"null"` – a string containing
  only the JSON null token), except if the target type is `Option<_>` or another type using
  `CompilationRepresentationFlags.UseNullAsTrueValue`). To allow deserializing to `null`, use the new
  `DeserializeToNullable`.

### 4.5.0 (2025-01-16)

* Removed static `null` constraint from `BeNullOrEmpty`
* Added `| null` annotation to `TryFormat` to allow nulls

### 4.4.0 (2024-12-18)

* Improved rendering of anonymous types in assertion messages, from `<>f__AnonymousType508954136<System.Int32>` to
  `{| A: System.Int32 |}`

### 4.3.0 (2024-12-05)

* The higher-order assertions `Satisfy`, `NotSatisfy`, `SatisfyAny`, and `SatisfyAll` now output the entire subject
  value, similar to other higher-order assertions.

### 4.2.1 (2024-10-14)

* Updated FracturedJson from 4.0.2 to 4.0.3
* Updated YamlDotNet from 16.0.0 to 16.1.3

### 4.2.0 (2024-09-06)

* Added `seq<string>` assertions `BeAscendingBy` and `BeDescendingBy` with `StringComparison`, `CultureInfo` and
  `CompareOptions` parameters

### 4.1.0 (2024-09-06)

* Added `seq<string>` assertions `BeAscending` and `BeDescending` with `StringComparison`, `CultureInfo` and
  `CompareOptions` parameters

### 4.0.1 (2024-08-24)

* Reduced the required FSharp.Core version from 7.0.400 to 5.0.2

### 4.0.0 (2024-08-13)

* **Breaking:** All higher-order assertions except `NotSatisfy` now fail with `AssertionFailedException` if the
  assertion throws any exception. Previously, these assertions let exceptions other than `AssertionFailedException`
  bubble up. The following assertions are affected:
  * `Satisfy`
  * `SatisfyAny`
  * `SatisfyAll`
  * `AllSatisfy`
  * `SatisfyRespectively`
  * `HaveStringContentSatisfying`
* Added a `BeOneOf` overload that instead of `seq<'a>` accepts `seq<'a * 'b>` and returns the associated second item as
  the derived state.

### 3.0.0 (2024-08-12)

* **Breaking:** The `HttpResponseMessage` assertion `HaveStringContentSatisfying` now returns the inner `Async<_>` value
  instead of `Async<unit>`. This allows using this assertion to return values derived from the HTTP content, while still
  getting the full request/response formatting for inner assertion failures.
* Added `string` assertion `DeserializeTo`
* Fixed `BeJsonEquivalentTo` erroneously failing for long lines

### 2.0.0 (2024-07-29)

* **Breaking:** Updated YamlDotNet from 15.1.2 to 16.0.0.
  See [this page](https://github.com/aaubry/YamlDotNet/releases/tag/v16.0.0) for breaking changes.
* Updated FSharp.SystemTextJson from 1.2.42 to 1.3.13

### 1.3.12 (2024-05-22)

* Updated FracturedJson from 3.1.1 to 4.0.2

### 1.3.11 (2024-05-13)

* Added `string` assertion `BeJsonEquivalentTo`
* Added `HttpResponseMessage` assertion `HaveStringContentSatisfying`. Note that this is async and therefore not
  chainable.
* The `HttpResponseMessage` assertions `HaveHeader` and `HaveHeaderValue` now correctly detects content headers such as
  `Content-Type` (which are set on `HttpContent` and not `HttpResponseMessage`)

### 1.3.9 (2024-04-02)

* Added `FaqtConfig.SetMapHttpHeaderValues` to set a function that can map HTTP header values (e.g. for masking
  `Authorization` headers).

### 1.3.8 (2024-03-22)

* Fixed subject name when using `_.Should()`
* On .NET 8, `Map<_, _>` now serializes as an object (not array) for non-`string` key types (
  using `WithMapFormat(MapFormat.Object)` in FSharp.SystemTextJson)

### 1.3.7 (2024-03-15)

* Fixed rare bug relating to race conditions
* Updated YamlDotNet from 15.1.0 to 15.1.2
* Updated FracturedJson from 3.1.0 to 3.1.1

### 1.3.6 (2024-03-13)

* Added `seq<_>` assertions `ContainAtMostOneItem` and `ContainAtMostOneItemMatching`

### 1.3.5 (2024-02-23)

* Removed `let`/`use`/`do` from the start of the subject name (e.g. when binding `.Subject` or `.Derived`)

### 1.3.4 (2024-02-21)

* Fixed incorrect subject name when `.Should` was placed right after `WhoseValue`

### 1.3.3 (2024-01-29)

* Updated YamlDotNet from 13.3.1 to 15.1.0

### 1.3.2 (2023-12-20)

* Now gives correct subject names for the new F# 8 shorthand lambda syntax

### 1.3.1 (2023-12-14)

* Balanced parentheses are now removed from the subject name. For example, `(Some 1).Should()...` will now give the
  subject name `Some 1` instead of `(Some 1)`.

### 1.3.0 (2023-09-28)

* Added `seq<_>` assertion `NotContainItemsMatching`

### 1.2.1 (2023-09-28)

* Made some accidentally public modules `internal`. This is strictly speaking a breaking change, but hopefully no-one
  was using them.

### 1.2.0 (2023-09-27)

* Added `seq<_>` assertions `AllBeMappedTo`, `AllBeEqual`, and `AllBeEqualBy`
* Added `IDictionary<_, _>` assertion `ContainKeys`

### 1.1.0 (2023-09-22)

* Added `Roundtrip` assertions for function subjects (`'a -> 'a`, `'a -> 'a option`, and `'a -> Result<'a, 'b>`)
* Byte arrays and other byte sequences are now formatted using `Convert.ToHexString`

### 1.0.0 (2023-09-12)

* Initial stable release

### 1.0.0-rc1 (2023-09-12)

* Initial release candidate

### 0.2.0 (2023-07-20)

* New proof-of-concept release with improvements to general features

### 0.1.0 (2023-07-15)

* Initial proof-of-concept release with few assertions but otherwise all features
