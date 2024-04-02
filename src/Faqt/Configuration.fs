namespace Faqt.Configuration

open System
open System.Threading


type FaqtConfig = private {
    httpContentMaxLength: int
    formatHttpContent: bool
} with


    /// Returns the default configuration. Note that changes to this configuration is not considered a breaking change.
    /// The configuration is immutable; all instance methods return a new instance.
    static member Default = {
        httpContentMaxLength = 1024 * 1024
        formatHttpContent = true
    }


    /// Gets the maximum length of rendered HttpContent in assertion failure output.
    member this.HttpContentMaxLength = this.httpContentMaxLength


    /// Sets the maximum length of rendered HttpContent in assertion failure output.
    member this.SetHttpContentMaxLength(length) = {
        this with
            httpContentMaxLength = length
    }


    /// Gets whether to attempt formatting HttpContent in assertion failure output.
    member this.FormatHttpContent = this.formatHttpContent


    /// Sets whether to attempt formatting HttpContent in assertion failure output.
    member this.SetFormatHttpContent(shouldFormat) = {
        this with
            formatHttpContent = shouldFormat
    }


/// Allows changing the current formatter, either temporarily (for the current thread) or globally.
type Config private () =


    static let mutable globalConfig: FaqtConfig = FaqtConfig.Default


    static let localConfig: AsyncLocal<FaqtConfig> = AsyncLocal()


    static member Current =
        if isNull (box localConfig.Value) then
            globalConfig
        else
            localConfig.Value


    /// Sets the specified config as the default global config.
    static member Set(config) = globalConfig <- config


    /// Sets the specified config as the config for the current thread. When the returned value is disposed, the old
    /// config is restored.
    static member With(config) =
        let oldLocalConfig = localConfig.Value
        localConfig.Value <- config

        { new IDisposable with
            member _.Dispose() = localConfig.Value <- oldLocalConfig
        }
