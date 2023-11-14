// -------------------------------------------------------------
// Copyright (c) 2023 Innovian Corporation. All rights reserved.
// -------------------------------------------------------------

using Innovian.Telnyx.Storage.Validation;

namespace Innovian.Telnyx.Storage.Tests.Validation;

[TestClass]
public class ValidatorsTests
{
    [TestMethod]
    public void CannotBeFormattedAsAnIpAddress()
    {
        const string name = "255.255.255.1";
        var result = Validators.IsBucketNameValid(name);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void CannotBeTwoCharactersLong()
    {
        const string name = "ab";
        var result = Validators.IsBucketNameValid(name);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void CannotExceed63CharactersLong()
    {
        const string name = "1234567890123456789012345678901234567890123456789012345678901234567890";
        var result = Validators.IsBucketNameValid(name);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void MustNotContainUppercaseCharacters()
    {
        const string name = "ThisIsATest";
        var result = Validators.IsBucketNameValid(name);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void MustNotContainUnderscores()
    {
        const string name = "This_Is_A_Test";
        var result = Validators.IsBucketNameValid(name);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void MustNotContainADash()
    {
        const string name = "This-Is-A-Test";
        var result = Validators.IsBucketNameValid(name);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void LabelsMustStartWithLowerCaseLetterOrNumber1()
    {
        const string name = "this.is.a.Test";
        var result = Validators.IsBucketNameValid(name);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void LabelMustStartWithLowerCaseLetterOrNumber2()
    {
        const string name = "th-i0.is.a.test";
        var result = Validators.IsBucketNameValid(name);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void LabelMustStartWithLowerCaseLetterOrNumber3()
    {
        const string name = "this.is.a.2est";
        var result = Validators.IsBucketNameValid(name);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ShouldBeValid()
    {
        var result = Validators.IsBucketNameValid("this-is-a.t3st");
        Assert.IsTrue(result);
    }
}