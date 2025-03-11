using Essentials.Results;
using FlowGuardMonitoring.BLL.Models;
using FlowGuardMonitoring.BLL.Options;
using FlowGuardMonitoring.BLL.Resources;
using FlowGuardMonitoring.BLL.Services;
using Microsoft.Extensions.Options;
using Moq;
using Resend;

namespace FlowGuardMonitoring.BLL.Tests.Services;

public class EmailSenderServiceTests
{
    [Fact]
    public async Task SendEmailAsync_Returns_SuccessfulResult_When_EmailIsSent()
    {
        var emailOptions = new EmailOptions
        {
            Username = "sender@example.com",
            ApiKey = "test123",
        };
        var mockGuid = new Guid("c5f0a55f-3586-44b9-ad0c-edd240dbadbb");
        var mockOptions = new Mock<IOptions<EmailOptions>>();
        mockOptions.Setup(opt => opt.Value).Returns(emailOptions);

        var mockResend = new Mock<IResend>();
        mockResend
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), CancellationToken.None))
            .Returns(Task.FromResult(new ResendResponse<Guid>(mockGuid)));

        var service = new EmailSenderService(mockOptions.Object, mockResend.Object);

        var emailModel = new EmailModel
        {
            Recipient = "recipient@example.com",
            Subject = "Test Subject",
            Body = "Test Body"
        };

        StandardResult result = await service.SendEmailAsync(emailModel);

        // Assert
        Assert.True(result.Succeeded);
        mockResend.Verify(r => r.EmailSendAsync(It.Is<EmailMessage>(m =>
            m.From.Email == emailOptions.Username &&
            m.To.First() == emailModel.Recipient &&
            m.Subject == emailModel.Subject &&
            m.HtmlBody == emailModel.Body), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_Returns_UnsuccessfulResult_When_ExceptionThrown()
    {
        // Arrange
        var emailOptions = new EmailOptions
        {
            Username = "sender@example.com",
            ApiKey = "test123"
        };
        var mockOptions = new Mock<IOptions<EmailOptions>>();
        mockOptions.Setup(opt => opt.Value).Returns(emailOptions);

        var mockResend = new Mock<IResend>();
        // Setup the resend service to throw an exception.
        mockResend
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), CancellationToken.None))
            .ThrowsAsync(new Exception("Test exception"));

        var service = new EmailSenderService(mockOptions.Object, mockResend.Object);

        var emailModel = new EmailModel
        {
            Recipient = "recipient@example.com",
            Subject = "Test Subject",
            Body = "Test Body"
        };

        // Act
        StandardResult result = await service.SendEmailAsync(emailModel);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task SendResetPasswordEmailAsync_Sends_Email_With_Correct_Content()
    {
        // Arrange
        var emailOptions = new EmailOptions
        {
            Username = "sender@example.com",
            ApiKey = "test123"
        };
        var mockGuid = new Guid("c5f0a55f-3586-44b9-ad0c-edd240dbadbb");
        var mockOptions = new Mock<IOptions<EmailOptions>>();
        mockOptions.Setup(opt => opt.Value).Returns(emailOptions);

        var mockResend = new Mock<IResend>();
        EmailMessage capturedMessage = null;
        mockResend
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), CancellationToken.None))
            .Callback<EmailMessage, CancellationToken>((msg, token) => capturedMessage = msg)
            .Returns(Task.FromResult(new ResendResponse<Guid>(mockGuid)));

        var service = new EmailSenderService(mockOptions.Object, mockResend.Object);

        var recipientEmail = "user@example.com";
        var resetPasswordUrl = "https://example.com/reset?token=123";

        // Act
        StandardResult result = await service.SendResetPasswordEmailAsync(recipientEmail, resetPasswordUrl);

        // Assert
        Assert.True(result.Succeeded);
        mockResend.Verify(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), CancellationToken.None), Times.Once);
        Assert.NotNull(capturedMessage);
        Assert.Equal(emailOptions.Username, capturedMessage.From.Email);
        Assert.Equal(recipientEmail, capturedMessage.To.First());
        Assert.Equal(EmailLocals.ResetPasswordTitle, capturedMessage.Subject);
        Assert.Equal(EmailLocals.ResetPassword.Replace("{0}", resetPasswordUrl), capturedMessage.HtmlBody);
    }

    [Fact]
    public async Task SendEmailConfirmationAsync_Sends_Email_With_Correct_Content()
    {
        // Arrange
        var emailOptions = new EmailOptions
        {
            Username = "sender@example.com",
            ApiKey = "test123"
        };
        var mockOptions = new Mock<IOptions<EmailOptions>>();
        var mockGuid = new Guid("c5f0a55f-3586-44b9-ad0c-edd240dbadbb");
        mockOptions.Setup(opt => opt.Value).Returns(emailOptions);

        var mockResend = new Mock<IResend>();
        EmailMessage capturedMessage = null;
        mockResend
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), CancellationToken.None))
            .Callback<EmailMessage, CancellationToken>((msg, token)=> capturedMessage = msg)
            .Returns(Task.FromResult(new ResendResponse<Guid>(mockGuid)));

        var service = new EmailSenderService(mockOptions.Object, mockResend.Object);

        var recipientEmail = "user@example.com";
        var confirmation = "confirmationCode123";

        // Act
        StandardResult result = await service.SendEmailConfirmationAsync(recipientEmail, confirmation);

        // Assert
        Assert.True(result.Succeeded);
        mockResend.Verify(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), CancellationToken.None), Times.Once);
        Assert.NotNull(capturedMessage);
        Assert.Equal(emailOptions.Username, capturedMessage.From.Email);
        Assert.Equal(recipientEmail, capturedMessage.To.First());
        Assert.Equal(EmailLocals.ConfirmEmailTitle, capturedMessage.Subject);
        Assert.Equal(EmailLocals.ConfirmEmail.Replace("{0}", confirmation), capturedMessage.HtmlBody);
    }
}