// =============================================================================
// فایل: src/AriaJournal.Core/Application/Services/AuthService.cs
// شماره فایل: 69
// توضیح: سرویس احراز هویت
// =============================================================================

using FluentValidation;
using AriaJournal.Core.Application.DTOs;
using AriaJournal.Core.Application.Validators;
using AriaJournal.Core.Domain.Common;
using AriaJournal.Core.Domain.Entities;
using AriaJournal.Core.Domain.Interfaces.Engines;

namespace AriaJournal.Core.Application.Services;

/// <summary>
/// سرویس احراز هویت
/// </summary>
public class AuthService
{
    private readonly IAuthEngine _authEngine;
    private readonly LoginValidator _loginValidator;
    private readonly RegisterValidator _registerValidator;
    private readonly ChangePasswordValidator _changePasswordValidator;
    private readonly RecoverPasswordValidator _recoverPasswordValidator;

    public AuthService(IAuthEngine authEngine)
    {
        _authEngine = authEngine ?? throw new ArgumentNullException(nameof(authEngine));
        _loginValidator = new LoginValidator();
        _registerValidator = new RegisterValidator();
        _changePasswordValidator = new ChangePasswordValidator();
        _recoverPasswordValidator = new RecoverPasswordValidator();
    }

    /// <summary>
    /// کاربر فعلی
    /// </summary>
    public User? CurrentUser => _authEngine.CurrentUser;

    /// <summary>
    /// آیا کاربر وارد شده
    /// </summary>
    public bool IsAuthenticated => _authEngine.IsAuthenticated;

    /// <summary>
    /// ورود کاربر
    /// </summary>
    public async Task<Result<LoginResponseDto>> LoginAsync(LoginDto dto)
    {
        // اعتبارسنجی
        var validationResult = await _loginValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("\n", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<LoginResponseDto>(Error.Validation(errors));
        }

        // ورود
        var result = await _authEngine.LoginAsync(dto.Username, dto.Password);

        if (result.IsFailure)
        {
            return Result.Failure<LoginResponseDto>(result.Error);
        }

        return Result.Success(new LoginResponseDto
        {
            Success = true,
            UserId = result.Value.User.Id,
            Username = result.Value.User.Username,
            Token = result.Value.Token,
            Message = "ورود موفقیت‌آمیز"
        });
    }

    /// <summary>
    /// ثبت‌نام کاربر جدید
    /// </summary>
    public async Task<Result<RegisterResponseDto>> RegisterAsync(RegisterDto dto)
    {
        // اعتبارسنجی
        var validationResult = await _registerValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("\n", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<RegisterResponseDto>(Error.Validation(errors));
        }

        // ثبت‌نام
        var result = await _authEngine.RegisterAsync(dto.Username, dto.Password);

        if (result.IsFailure)
        {
            return Result.Failure<RegisterResponseDto>(result.Error);
        }

        return Result.Success(new RegisterResponseDto
        {
            Success = true,
            UserId = result.Value.User.Id,
            RecoveryKey = result.Value.RecoveryKey,
            Message = "ثبت‌نام موفقیت‌آمیز. کلید بازیابی را در جای امنی ذخیره کنید."
        });
    }

    /// <summary>
    /// تغییر رمز عبور
    /// </summary>
    public async Task<Result<bool>> ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        // اعتبارسنجی
        var validationResult = await _changePasswordValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("\n", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<bool>(Error.Validation(errors));
        }

        return await _authEngine.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);
    }

    /// <summary>
    /// بازیابی رمز عبور
    /// </summary>
    public async Task<Result<bool>> RecoverPasswordAsync(RecoverPasswordDto dto)
    {
        // اعتبارسنجی
        var validationResult = await _recoverPasswordValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("\n", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<bool>(Error.Validation(errors));
        }

        return await _authEngine.RecoverAsync(dto.Username, dto.RecoveryKey, dto.NewPassword);
    }

    /// <summary>
    /// خروج
    /// </summary>
    public async Task LogoutAsync()
    {
        await _authEngine.LogoutAsync();
    }

    /// <summary>
    /// بررسی قفل بودن حساب
    /// </summary>
    public bool IsLockedOut(int userId)
    {
        return _authEngine.IsLockedOut(userId);
    }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Application/Services/AuthService.cs
// =============================================================================