Release notes
==============

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
