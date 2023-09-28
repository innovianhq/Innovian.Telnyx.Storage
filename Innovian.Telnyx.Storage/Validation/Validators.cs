//  -------------------------------------------------------------
//  Copyright (c) 2023 Innovian Corporation. All rights reserved.
//  -------------------------------------------------------------

namespace Innovian.Telnyx.Storage.Validation;

internal static class Validators
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
        //The name must be 3 to 65 characters long.
        if (bucketName.Length is < 3 or > 65)
            return false;

        //The name must consist only of lowercase letters, numbers, dots (periods) or hyphens.
        foreach (var character in bucketName)
        {
            if (char.IsLower(character) || char.IsNumber(character) || character == '.' || character == '-')
                continue;

            return false;
        }

        return true;
    }
}