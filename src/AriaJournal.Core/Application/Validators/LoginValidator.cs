// =============================================================================
// فایل: src/AriaJournal.Core/Application/Validators/LoginValidator.cs
// شماره فایل: 67
// توضیح: اعتبارسنج ورود و ثبت‌نام
// =============================================================================

using FluentValidation;
using AriaJournal.Core.Application.DTOs;

namespace AriaJournal.Core.Application.Validators;

/// <summary>
/// اعتبارسنج ورود
/// </summary>
public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("نام کاربری الزامی است")
            .MinimumLength(3).WithMessage("نام کاربری باید حداقل ۳ کاراکتر باشد")
            .MaximumLength(50).WithMessage("نام کاربری نباید بیشتر از ۵۰ کاراکتر باشد");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("رمز عبور الزامی است")
            .MinimumLength(6).WithMessage("رمز عبور باید حداقل ۶ کاراکتر باشد");
    }
}

/// <summary>
/// اعتبارسنج ثبت‌نام
/// </summary>
public class RegisterValidator : AbstractValidator<RegisterDto>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("نام کاربری الزامی است")
            .MinimumLength(3).WithMessage("نام کاربری باید حداقل ۳ کاراکتر باشد")
            .MaximumLength(50).WithMessage("نام کاربری نباید بیشتر از ۵۰ کاراکتر باشد")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("نام کاربری فقط می‌تواند شامل حروف انگلیسی، اعداد و _ باشد");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("رمز عبور الزامی است")
            .MinimumLength(6).WithMessage("رمز عبور باید حداقل ۶ کاراکتر باشد")
            .MaximumLength(100).WithMessage("رمز عبور نباید بیشتر از ۱۰۰ کاراکتر باشد");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("تکرار رمز عبور الزامی است")
            .Equal(x => x.Password).WithMessage("رمز عبور و تکرار آن یکسان نیستند");
    }
}

/// <summary>
/// اعتبارسنج تغییر رمز عبور
/// </summary>
public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("رمز عبور فعلی الزامی است");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("رمز عبور جدید الزامی است")
            .MinimumLength(6).WithMessage("رمز عبور جدید باید حداقل ۶ کاراکتر باشد")
            .NotEqual(x => x.CurrentPassword).WithMessage("رمز عبور جدید باید متفاوت از رمز فعلی باشد");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("تکرار رمز عبور جدید الزامی است")
            .Equal(x => x.NewPassword).WithMessage("رمز عبور جدید و تکرار آن یکسان نیستند");
    }
}

/// <summary>
/// اعتبارسنج بازیابی رمز عبور
/// </summary>
public class RecoverPasswordValidator : AbstractValidator<RecoverPasswordDto>
{
    public RecoverPasswordValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("نام کاربری الزامی است");

        RuleFor(x => x.RecoveryKey)
            .NotEmpty().WithMessage("کلید بازیابی الزامی است")
            .Length(32, 40).WithMessage("کلید بازیابی نامعتبر است");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("رمز عبور جدید الزامی است")
            .MinimumLength(6).WithMessage("رمز عبور جدید باید حداقل ۶ کاراکتر باشد");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("تکرار رمز عبور جدید الزامی است")
            .Equal(x => x.NewPassword).WithMessage("رمز عبور جدید و تکرار آن یکسان نیستند");
    }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Application/Validators/LoginValidator.cs
// =============================================================================