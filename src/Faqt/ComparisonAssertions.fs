namespace Faqt

open System
open System.Runtime.CompilerServices

open Faqt.AssertionHelpers


[<Extension>]
type ComparisonAssertions =


    static member inline internal IsNaN(value: 'a) =
        // Static type checks let the JIT eliminate the box/unbox casts for concrete numeric types.
        // Matching directly on box value retained allocations in benchmarks.
        if typeof<'a> = typeof<double> then
            Double.IsNaN(unbox<double> (box value))
        elif typeof<'a> = typeof<single> then
            Single.IsNaN(unbox<single> (box value))
        elif typeof<'a> = typeof<Half> then
            Half.IsNaN(unbox<Half> (box value))
        elif typeof<'a>.IsValueType && isNull (Nullable.GetUnderlyingType(typeof<'a>)) then
            false
        else
            match box value with
            | :? double as value -> Double.IsNaN value
            | :? single as value -> Single.IsNaN value
            | :? Half as value -> Half.IsNaN value
            | _ -> false


    static member inline private TryWithinBuiltInTolerance(subject: 'a, target: 'b, tolerance: 'c) =
        let inline withinTolerance subject target tolerance =
            let difference =
                if subject >= target then
                    subject - target
                else
                    target - subject

            difference <= tolerance

        let inline withinSignedTolerance zero subject target tolerance =
            // Typed CompareTo also avoids boxing for Int128, unlike F# generic comparison.
            let inline compareTo (left: ^n) (right: ^n) =
                (^n: (member CompareTo: ^n -> int) (left, right))

            let struct (low, high) =
                if compareTo subject target <= 0 then
                    struct (subject, target)
                else
                    struct (target, subject)

            if compareTo low zero >= 0 || compareTo high zero < 0 then
                compareTo (high - low) tolerance <= 0
            else
                // Avoid high - low across zero; checking high first keeps high - tolerance in range.
                compareTo high tolerance <= 0 && compareTo low (high - tolerance) >= 0

        // Dispatch on static types so inlined numeric calls can eliminate the type checks and box/unbox casts.
        if typeof<'a> <> typeof<'b> || typeof<'a> <> typeof<'c> then
            ValueNone
        elif typeof<'a> = typeof<byte> then
            ValueSome(
                withinTolerance (unbox<byte> (box subject)) (unbox<byte> (box target)) (unbox<byte> (box tolerance))
            )
        elif typeof<'a> = typeof<uint16> then
            ValueSome(
                withinTolerance
                    (unbox<uint16> (box subject))
                    (unbox<uint16> (box target))
                    (unbox<uint16> (box tolerance))
            )
        elif typeof<'a> = typeof<uint32> then
            ValueSome(
                withinTolerance
                    (unbox<uint32> (box subject))
                    (unbox<uint32> (box target))
                    (unbox<uint32> (box tolerance))
            )
        elif typeof<'a> = typeof<uint64> then
            ValueSome(
                withinTolerance
                    (unbox<uint64> (box subject))
                    (unbox<uint64> (box target))
                    (unbox<uint64> (box tolerance))
            )
        elif typeof<'a> = typeof<unativeint> then
            ValueSome(
                withinTolerance
                    (unbox<unativeint> (box subject))
                    (unbox<unativeint> (box target))
                    (unbox<unativeint> (box tolerance))
            )
#if NET7_0_OR_GREATER
        elif typeof<'a> = typeof<UInt128> then
            ValueSome(
                withinTolerance
                    (unbox<UInt128> (box subject))
                    (unbox<UInt128> (box target))
                    (unbox<UInt128> (box tolerance))
            )
#endif
        elif typeof<'a> = typeof<decimal> then
            try
                ValueSome(
                    withinTolerance
                        (unbox<decimal> (box subject))
                        (unbox<decimal> (box target))
                        (unbox<decimal> (box tolerance))
                )
            with :? OverflowException ->
                // An overflowing decimal distance exceeds every representable tolerance.
                ValueSome false
        elif typeof<'a> = typeof<TimeSpan> then
            ValueSome(
                withinSignedTolerance
                    TimeSpan.Zero
                    (unbox<TimeSpan> (box subject))
                    (unbox<TimeSpan> (box target))
                    (unbox<TimeSpan> (box tolerance))
            )
        else
            ValueNone


    static member inline private IsNegativeTolerance(tolerance: 'a) =
        if typeof<'a> = typeof<sbyte> then
            let value = unbox<sbyte> (box tolerance)
            value < 0y
        elif typeof<'a> = typeof<int16> then
            let value = unbox<int16> (box tolerance)
            value < 0s
        elif typeof<'a> = typeof<int> then
            let value = unbox<int> (box tolerance)
            value < 0
        elif typeof<'a> = typeof<int64> then
            let value = unbox<int64> (box tolerance)
            value < 0L
        elif typeof<'a> = typeof<nativeint> then
            let value = unbox<nativeint> (box tolerance)
            value < 0n
        elif typeof<'a> = typeof<Half> then
            let value = unbox<Half> (box tolerance)
            single value < 0.0f
        elif typeof<'a> = typeof<single> then
            let value = unbox<single> (box tolerance)
            value < 0.0f
        elif typeof<'a> = typeof<double> then
            let value = unbox<double> (box tolerance)
            value < 0.0
        elif typeof<'a> = typeof<decimal> then
            let value = unbox<decimal> (box tolerance)
            value < 0m
        elif typeof<'a> = typeof<bigint> then
            let value = unbox<bigint> (box tolerance)
            value.Sign < 0
        elif typeof<'a> = typeof<TimeSpan> then
            let value = unbox<TimeSpan> (box tolerance)
            value < TimeSpan.Zero
#if NET7_0_OR_GREATER
        elif typeof<'a> = typeof<Int128> then
            let value = unbox<Int128> (box tolerance)
            value < Int128.Zero
#endif
        elif typeof<'a>.IsValueType && isNull (Nullable.GetUnderlyingType(typeof<'a>)) then
            false
        else
            match box tolerance with
            | :? sbyte as value -> value < 0y
            | :? int16 as value -> value < 0s
            | :? int as value -> value < 0
            | :? int64 as value -> value < 0L
            | :? nativeint as value -> value < 0n
            | :? Half as value -> single value < 0.0f
            | :? single as value -> value < 0.0f
            | :? double as value -> value < 0.0
            | :? decimal as value -> value < 0m
            | :? bigint as value -> value.Sign < 0
            | :? TimeSpan as value -> value < TimeSpan.Zero
#if NET7_0_OR_GREATER
            | :? Int128 as value -> value < Int128.Zero
#endif
            | _ -> false


    /// Asserts that the subject is at most the specified tolerance greater or smaller than the specified value.
    [<Extension>]
    static member inline BeCloseTo(t: Testable<'a>, target: 'b, tolerance: 'c, ?because) : And<'a> =
        use _ = t.Assert()

        let hasNaN =
            ComparisonAssertions.IsNaN(t.Subject)
            || ComparisonAssertions.IsNaN(target)
            || ComparisonAssertions.IsNaN(tolerance)

        if not hasNaN && ComparisonAssertions.IsNegativeTolerance(tolerance) then
            invalidArg (nameof tolerance) "The tolerance must be non-negative"

        let isClose =
            if hasNaN then
                false
            else
                match ComparisonAssertions.TryWithinBuiltInTolerance(t.Subject, target, tolerance) with
                | ValueSome isClose -> isClose
                // Preserve the existing generic subtraction-based path for mixed custom types.
                | ValueNone -> not (t.Subject - target > tolerance || target - t.Subject > tolerance)

        if hasNaN || not isClose then
            t.With("Target", target).With("With tolerance", tolerance).With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is not within the specified tolerance greater or smaller than the specified value.
    /// Passes if the subject is null.
    [<Extension>]
    static member inline NotBeCloseTo(t: Testable<'a>, target: 'b, tolerance: 'c, ?because) : And<'a> =
        use _ = t.Assert()

        let hasNaN =
            ComparisonAssertions.IsNaN(t.Subject)
            || ComparisonAssertions.IsNaN(target)
            || ComparisonAssertions.IsNaN(tolerance)

        if not hasNaN && ComparisonAssertions.IsNegativeTolerance(tolerance) then
            invalidArg (nameof tolerance) "The tolerance must be non-negative"

        let isClose =
            if hasNaN then
                false
            else
                match ComparisonAssertions.TryWithinBuiltInTolerance(t.Subject, target, tolerance) with
                | ValueSome isClose -> isClose
                // Preserve the existing generic subtraction-based path for mixed custom types.
                | ValueNone -> not (t.Subject - target > tolerance || target - t.Subject > tolerance)

        if hasNaN || isClose then
            t.With("Target", target).With("With tolerance", tolerance).With("But was", t.Subject).Fail(because)

        And(t)


    [<Extension>]
    static member inline private Compare(t: Testable<'a>, op, other, because) =
        if
            ComparisonAssertions.IsNaN(t.Subject)
            || ComparisonAssertions.IsNaN(other)
            || not (op t.Subject other)
        then
            t.With("Other", other).With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is greater than the specified value.
    [<Extension>]
    static member inline BeGreaterThan(t: Testable<'a>, other: 'a, ?because) : And<'a> =
        use _ = t.Assert()
        t.Compare((>), other, because)


    /// Asserts that the subject is greater than or equal to the specified value.
    [<Extension>]
    static member inline BeGreaterThanOrEqualTo(t: Testable<'a>, other: 'a, ?because) : And<'a> =
        use _ = t.Assert()
        t.Compare((>=), other, because)


    /// Asserts that the subject is less than the specified value.
    [<Extension>]
    static member inline BeLessThan(t: Testable<'a>, other: 'a, ?because) : And<'a> =
        use _ = t.Assert()
        t.Compare((<), other, because)


    /// Asserts that the subject is less than or equal to the specified value.
    [<Extension>]
    static member inline BeLessThanOrEqualTo(t: Testable<'a>, other: 'a, ?because) : And<'a> =
        use _ = t.Assert()
        t.Compare((<=), other, because)


    /// Asserts that the subject is greater than zero.
    [<Extension>]
    static member inline BePositive(t: Testable<'a>, ?because) : And<'a> =
        use _ = t.Assert()

        if
            ComparisonAssertions.IsNaN(t.Subject)
            || t.Subject <= LanguagePrimitives.GenericZero
        then
            t.With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is less than zero.
    [<Extension>]
    static member inline BeNegative(t: Testable<'a>, ?because) : And<'a> =
        use _ = t.Assert()

        if
            ComparisonAssertions.IsNaN(t.Subject)
            || t.Subject >= LanguagePrimitives.GenericZero
        then
            t.With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is greater than or equal to zero.
    [<Extension>]
    static member inline BeNonNegative(t: Testable<'a>, ?because) : And<'a> =
        use _ = t.Assert()

        if
            ComparisonAssertions.IsNaN(t.Subject)
            || t.Subject < LanguagePrimitives.GenericZero
        then
            t.With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is less than or equal to zero.
    [<Extension>]
    static member inline BeNonPositive(t: Testable<'a>, ?because) : And<'a> =
        use _ = t.Assert()

        if
            ComparisonAssertions.IsNaN(t.Subject)
            || t.Subject > LanguagePrimitives.GenericZero
        then
            t.With("But was", t.Subject).Fail(because)

        And(t)


    /// Asserts that the subject is in the specified inclusive range.
    [<Extension>]
    static member inline BeInRange(t: Testable<'a>, lower: 'a, upper: 'a, ?because) : And<'a> =
        use _ = t.Assert()

        let hasNaN =
            ComparisonAssertions.IsNaN(t.Subject)
            || ComparisonAssertions.IsNaN(lower)
            || ComparisonAssertions.IsNaN(upper)

        if hasNaN || t.Subject < lower || t.Subject > upper then
            t.With("Lower", lower).With("Upper", upper).With("But was", t.Subject).Fail(because)

        And(t)
