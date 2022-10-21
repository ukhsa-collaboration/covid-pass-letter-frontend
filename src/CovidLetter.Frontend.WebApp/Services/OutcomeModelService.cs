using System;
using System.Collections.Generic;
using CovidLetter.Frontend.Search;
using CovidLetter.Frontend.WebApp.Extensions;
using CovidLetter.Frontend.WebApp.Models;

namespace CovidLetter.Frontend.WebApp.Services;

public class OutcomeModelService
{
    private readonly IObfuscationService _obfuscationService;

    public OutcomeModelService(IObfuscationService obfuscationService)
    {
        _obfuscationService = obfuscationService;
    }

    public void PopulateViewModelWithObfuscatedContactDetails(
        ContactPreferenceViewModel viewModel,
        SearchResultData contactDetails,
        bool isAccessibleFormatOrOtherLanguageRequested,
        DateOfBirthViewModel userSessionDateOfBirth)
    {
        viewModel.AccessibleFormatOrOtherLanguageRequested = isAccessibleFormatOrOtherLanguageRequested;

        if (RequestIsForOver12(userSessionDateOfBirth))
        {
            AddEmails(contactDetails.Emails, viewModel);
            AddPhoneNumbers(contactDetails.Mobiles, viewModel);
        }

    }

    private void AddEmails(IDictionary<Guid, string> emails, ContactPreferenceViewModel viewModel)
    {
        foreach (var (key, value) in emails)
        {
            if (_obfuscationService.TryObfuscateEmail(
                    value,
                    out var obfuscatedEmail))
            {
                viewModel.ObfuscatedContactDetails.Add(
                    key,
                    obfuscatedEmail
                );
            }
        }
    }

    private void AddPhoneNumbers(IDictionary<Guid, string> phoneNumbers, ContactPreferenceViewModel viewModel)
    {
        foreach (var (key, value) in phoneNumbers)
        {
            var obfuscatedPhoneNumber = _obfuscationService.ObfuscatePhone(value);

            if (!string.IsNullOrEmpty(obfuscatedPhoneNumber))
            {
                viewModel.ObfuscatedContactDetails.Add(
                    key,
                    obfuscatedPhoneNumber);
            }
        }
    }

    private static bool RequestIsForOver12(DateOfBirthViewModel dateOfBirthViewModel)
    {
        var dateOfBirth = dateOfBirthViewModel.ToDateTime();

        return DateTime.UtcNow.AddYears(-12).Date >= dateOfBirth.Date;
    }
}