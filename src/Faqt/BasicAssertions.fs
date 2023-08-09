namespace Faqt

open System.Runtime.CompilerServices
open AssertionHelpers
open Formatting


[<Extension>]
type BasicAssertions =


    /// Asserts that the subject is the specified value, using the specified equality comparison.
    [<Extension>]
    static member Be(t: Testable<'a>, expected: 'b, isEqual: 'a -> 'b -> bool, ?because) : AndDerived<'a, 'b> =
        use _ = t.Assert()

        if not (isEqual t.Subject expected) then
            t.Fail(
                "{subject}\n\tshould be\n{0}\n\t{because}but the specified equality comparer returned false when comparing it to\n{actual}",
                because,
                format expected
            )

        AndDerived(t, expected)


    /// Asserts that the subject is the specified value, using the default equality comparison (=).
    [<Extension>]
    static member Be(t: Testable<'a>, expected: 'a, ?because) : And<'a> =
        use _ = t.Assert()

        if t.Subject <> expected then
            t.Fail("{subject}\n\tshould be\n{0}\n\t{because}but was\n{actual}", because, format expected)

        And(t)


    /// Asserts that the subject is not the specified value, using default equality comparison (=).
    [<Extension>]
    static member NotBe(t: Testable<'a>, expected: 'b, isEqual: 'a -> 'b -> bool, ?because) : And<'a> =
        use _ = t.Assert()

        if isEqual t.Subject expected then
            t.Fail(
                "{subject}\n\tshould not be\n{0}\n\t{because}but the specified equality comparer returned true when comparing it to\n{actual}",
                because,
                format expected
            )

        And(t)


    /// Asserts that the subject is not the specified value, using default equality comparison (=).
    [<Extension>]
    static member NotBe(t: Testable<'a>, expected: 'a, ?because) : And<'a> =
        use _ = t.Assert()

        if t.Subject = expected then
            t.Fail("{subject}\n\tshould not be\n{0}\n\t{because}but the values were equal.", because, format expected)

        And(t)


    /// Asserts that the subject is reference equal to the specified value (which must not be null).
    [<Extension>]
    static member BeSameAs(t: Testable<'a>, expected: 'a, ?because) : And<'a> =
        use _ = t.Assert()

        if not (LanguagePrimitives.PhysicalEquality t.Subject expected) then
            let expectedStr =
                if isNull (box expected) then
                    "null"
                else
                    $"%i{LanguagePrimitives.PhysicalHash expected} %s{expected.GetType().AssertionName}\n%s{format expected}"

            let actualStr =
                if isNull (box t.Subject) then
                    "null"
                else
                    $"%i{LanguagePrimitives.PhysicalHash t.Subject} %s{t.Subject.GetType().AssertionName}\n%s{format t.Subject}"

            t.Fail(
                "{subject}\n\tshould be reference equal to\n{0}\n\t{because}but was\n{1}",
                because,
                expectedStr,
                actualStr
            )

        And(t)


    /// Asserts that the subject is not reference equal to the specified value (which must not be null).
    [<Extension>]
    static member NotBeSameAs(t: Testable<'a>, expected: 'a, ?because) : And<'a> =
        use _ = t.Assert()

        if LanguagePrimitives.PhysicalEquality t.Subject expected then
            let expectedStr =
                if isNull (box expected) then
                    "null"
                else
                    $"%i{LanguagePrimitives.PhysicalHash expected} %s{expected.GetType().AssertionName}\n%s{format expected}"

            t.Fail(
                "{subject}\n\tshould not be reference equal to\n{0}\n\t{because}but was the same reference.",
                because,
                expectedStr
            )

        And(t)


    /// Asserts that the subject is null.
    [<Extension>]
    static member BeNull(t: Testable<'a>, ?because) : And<'a> =
        use _ = t.Assert()

        if not (isNull t.Subject) then
            t.Fail("{subject}\n\tshould be null{because}, but was\n{actual}", because)

        And(t)


    /// Asserts that the subject is not null.
    [<Extension>]
    static member NotBeNull(t: Testable<'a>, ?because) : And<'a> =
        use _ = t.Assert()

        if isNull t.Subject then
            t.Fail("{subject}\n\tshould not be null{because}, but was null.", because)

        And(t)


    /// Asserts that the subject can be transformed using the specified function (i.e., that the function does not
    /// throw).
    [<Extension>]
    static member Transform(t: Testable<'a>, f: 'a -> 'b, ?because) : AndDerived<'a, 'b> =
        use _ = t.Assert()

        try
            AndDerived(t, f t.Subject)
        with ex ->
            t.Fail(
                "{subject}\n\tshould be successfully transformed by the specified function{because}, but failed with the following exception:\n{0}: {1}",
                because,
                ex.GetType().FullName,
                ex.Message
            )


    /// Asserts that the subject can be transformed using the specified function (fails if the function returns None or
    /// throws).
    [<Extension>]
    static member TryTransform(t: Testable<'a>, f: 'a -> 'b option, ?because) : AndDerived<'a, 'b> =
        use _ = t.Assert()

        let result =
            try
                f t.Subject
            with ex ->
                t.Fail(
                    "{subject}\n\tshould be successfully transformed by the specified function{because}, but failed with the following exception:\n{0}: {1}",
                    because,
                    ex.GetType().FullName,
                    ex.Message
                )

        match result with
        | Some x -> AndDerived(t, x)
        | None ->
            t.Fail(
                "{subject}\n\tshould be successfully transformed by the specified function{because}, but the function returned\n{0}",
                because,
                format None
            )


    /// Asserts that the subject can be transformed using the specified function (fails if the function returns
    /// ValueNone or throws).
    [<Extension>]
    static member TryTransform(t: Testable<'a>, f: 'a -> 'b voption, ?because) : AndDerived<'a, 'b> =
        use _ = t.Assert()

        let result =
            try
                f t.Subject
            with ex ->
                t.Fail(
                    "{subject}\n\tshould be successfully transformed by the specified function{because}, but failed with the following exception:\n{0}: {1}",
                    because,
                    ex.GetType().FullName,
                    ex.Message
                )

        match result with
        | ValueSome x -> AndDerived(t, x)
        | ValueNone ->
            t.Fail(
                "{subject}\n\tshould be successfully transformed by the specified function{because}, but the function returned\n{0}",
                because,
                format ValueNone
            )


    /// Asserts that the subject can be transformed using the specified function (fails if the function returns Error or
    /// throws).
    [<Extension>]
    static member TryTransform(t: Testable<'a>, f: 'a -> Result<'b, 'c>, ?because) : AndDerived<'a, 'b> =
        use _ = t.Assert()

        let result =
            try
                f t.Subject
            with ex ->
                t.Fail(
                    "{subject}\n\tshould be successfully transformed by the specified function{because}, but failed with the following exception:\n{0}: {1}",
                    because,
                    ex.GetType().FullName,
                    ex.Message
                )

        match result with
        | Ok x -> AndDerived(t, x)
        | Error err ->
            t.Fail(
                "{subject}\n\tshould be successfully transformed by the specified function{because}, but the function returned\n{0}",
                because,
                format (Error err)
            )


    /// Asserts that the subject can be transformed using the specified function (fails if the function returns false or
    /// throws). This overload is suitable for functions like Int32.TryParse.
    [<Extension>]
    static member TryTransform(t: Testable<'a>, f: 'a -> bool * 'b, ?because) : AndDerived<'a, 'b> =
        use _ = t.Assert()

        let result =
            try
                f t.Subject
            with ex ->
                t.Fail(
                    "{subject}\n\tshould be successfully transformed by the specified function{because}, but failed with the following exception:\n{0}: {1}",
                    because,
                    ex.GetType().FullName,
                    ex.Message
                )

        match result with
        | true, x -> AndDerived(t, x)
        | false, _ ->
            t.Fail(
                "{subject}\n\tshould be successfully transformed by the specified function{because}, but the function returned\n{0}",
                because,
                format false
            )
