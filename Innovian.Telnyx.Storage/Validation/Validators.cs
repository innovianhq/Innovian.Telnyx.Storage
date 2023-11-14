// -------------------------------------------------------------
// Copyright (c) 2023 Innovian Corporation. All rights reserved.
// -------------------------------------------------------------

using System.Text.RegularExpressions;

namespace Innovian.Telnyx.Storage.Validation;

/// <summary>
/// Used to validate values passed into the API.
/// </summary>
public static class Validators
{
    public const string BucketNameValidityMessage =
        "The bucket name must be 3-65 characters long and can consist only of lowercase letters, numbers, dots and hyphens";

    /// <summary>
    /// Validates a given bucket name.
    /// </summary>
    /// <param name="bucketName">The name of the bucket to validate.</param>
    /// <returns>True if the bucket is valid; false if not.</returns>
    public static bool IsBucketNameValid(string bucketName)
    {
        //Documentation at https://developers.telnyx.com/docs/v2/cloud_storage/endpoints/bucket_operations/createbucket/

        //The name must be 3 to 65 characters long.
        if (bucketName.Length is < 3 or > 65)
            return false;

        //Must start with a lowercase letter or number
        var firstCharacter = bucketName[0];
        if (!char.IsLower(firstCharacter) && !char.IsNumber(firstCharacter))
        {
            return false;
        }

        //The name must consist only of lowercase letters, numbers, dots (periods) or hyphens.
        foreach (var character in bucketName)
        {
            if (char.IsLower(character) || char.IsNumber(character) || character == '.' || character == '-')
                continue;

            return false;
        }

        if (bucketName.Contains('.'))
        {
            //Cannot be formatted as an IP address
            var ipRegex = new Regex(@"(\d{1,3}\.){3}\d{1,3}");
            if (ipRegex.IsMatch(bucketName))
                return false;

            var labels = bucketName.Split('.');
            foreach (var label in labels)
            {
                if (!ValidateLabel(label))
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// If the bucket name contains periods, this validates the label between each period.
    /// </summary>
    /// <param name="label">The label to validate.</param>
    /// <returns></returns>
    private static bool ValidateLabel(string label)
    {
        //Must start and end with a lowercase letter or a number - meaning it must be populated
        if (label.Length == 0)
            return false;

        //Must start with a lowercase letter or number
        var firstCharacter = label.First();
        if (!char.IsNumber(firstCharacter) && !char.IsLower(firstCharacter))
            return false;

        //Must end with a lowercase letter or number
        var lastCharacter = label.Last();
        if (!char.IsNumber(lastCharacter) && !char.IsLower(lastCharacter))
            return false;

        
        foreach (var character in label)
        {
            //Must not contain uppercase characters
            if (char.IsUpper(character))
                return false;

            //Must not contain underscores
            if (character == '_')
                return false;
        }

        return true;
    }
}