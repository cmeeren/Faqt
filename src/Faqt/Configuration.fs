namespace Faqt.Configuration

open System
open System.Threading


type FaqtConfig = private {
    // TODO
    dummy: unit
} with


    /// Returns the default config. Note that changes to this config is not considered a breaking change. The config is
    /// immutable; all instance methods return a new instance.
    static member Default = { dummy = () }


/// Allows changing the current formatter, either temporarily (for the current thread) or globally.
type Config private () =


    static let mutable globalConfig: FaqtConfig = FaqtConfig.Default


    static let currentConfig: AsyncLocal<FaqtConfig> = AsyncLocal()


    static member internal Config =
        if isNull (box currentConfig.Value) then
            globalConfig
        else
            currentConfig.Value


    /// Sets the specified config as the default global config.
    static member Set(config) = globalConfig <- config


    /// Sets the specified config as the config for the current thread. When the returned value is disposed, the old
    /// config is restored.
    static member With(config) =
        let oldFormatter = currentConfig.Value
        currentConfig.Value <- config

        { new IDisposable with
            member _.Dispose() = currentConfig.Value <- oldFormatter
        }
