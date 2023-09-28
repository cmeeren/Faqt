Release notes
==============

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
