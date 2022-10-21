using CovidLetter.Frontend.WebApp.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace CovidLetter.Frontend.WebApp.Configuration;

internal class ConfigureModelBindingLocalization : IConfigureOptions<MvcOptions>
{
    private readonly IStringLocalizer<ModelBinding> _localizer;

    public ConfigureModelBindingLocalization(IStringLocalizer<ModelBinding> localizer)
    {
        _localizer = localizer;
    }
    
    public void Configure(MvcOptions options)
    {
        options.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(
            arg0 => GetLocalizedString("missingBindRequiredMember", arg0));
        options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(
            () => GetLocalizedString("bothKeyAndValueMustBePresent"));
        options.ModelBindingMessageProvider.SetMissingRequestBodyRequiredValueAccessor(
            () => GetLocalizedString("missingRequestBodyRequiredMember"));
        options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
            arg0 => GetLocalizedString("nullValueNotValid", arg0));
        options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
            (arg0, arg1) => GetLocalizedString("attemptedValueIsInvalid", arg0, arg1));
        options.ModelBindingMessageProvider.SetNonPropertyAttemptedValueIsInvalidAccessor(
            arg0 => GetLocalizedString("nonPropertyAttemptedValueIsInvalid", arg0));
        options.ModelBindingMessageProvider.SetUnknownValueIsInvalidAccessor(
            arg0 => GetLocalizedString("unknownValueIsInvalid", arg0));
        options.ModelBindingMessageProvider.SetNonPropertyUnknownValueIsInvalidAccessor(
            () => GetLocalizedString("nonPropertyUnknownValueIsInvalid"));
        options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
            arg0 => GetLocalizedString("valueIsInvalid", arg0));
        options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
            arg0 => GetLocalizedString("valueMustBeNumber", arg0));
        options.ModelBindingMessageProvider.SetNonPropertyValueMustBeANumberAccessor(
            () => GetLocalizedString("nonPropertyValueMustBeNumber"));
    }

    private string GetLocalizedString(string key, params object[] args)
    {
        return string.Format(_localizer[key].Value, args);
    }
}