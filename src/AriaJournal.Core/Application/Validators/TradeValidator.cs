// =============================================================================
// فایل: src/AriaJournal.Core/Application/Validators/TradeValidator.cs
// شماره فایل: 68
// توضیح: اعتبارسنج معامله
// =============================================================================

using System;
using FluentValidation;
using AriaJournal.Core.Application.DTOs;
using AriaJournal.Core.Domain.Enums;

namespace AriaJournal.Core.Application.Validators;

/// <summary>
/// اعتبارسنج ایجاد معامله
/// </summary>
public class TradeCreateValidator : AbstractValidator<TradeCreateDto>
{
    public TradeCreateValidator()
    {
        RuleFor(x => x.AccountId)
            .GreaterThan(0).WithMessage("حساب معاملاتی را انتخاب کنید");

        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("نماد الزامی است")
            .MaximumLength(20).WithMessage("نماد نباید بیشتر از ۲۰ کاراکتر باشد");

        RuleFor(x => x.Direction)
            .IsInEnum().WithMessage("جهت معامله نامعتبر است");

        RuleFor(x => x.Volume)
            .GreaterThan(0).WithMessage("حجم باید بیشتر از صفر باشد")
            .LessThanOrEqualTo(100).WithMessage("حجم نباید بیشتر از ۱۰۰ لات باشد");

        RuleFor(x => x.EntryPrice)
            .GreaterThan(0).WithMessage("قیمت ورود باید بیشتر از صفر باشد");

        RuleFor(x => x.EntryTime)
            .NotEmpty().WithMessage("زمان ورود الزامی است")
            .LessThanOrEqualTo(DateTime.Now.AddDays(1)).WithMessage("زمان ورود نمی‌تواند در آینده باشد");

        // اعتبارسنجی حد ضرر برای خرید
        When(x => x.Direction == TradeDirection.Buy && x.StopLoss.HasValue, () =>
        {
            RuleFor(x => x.StopLoss)
                .LessThan(x => x.EntryPrice)
                .WithMessage("حد ضرر خرید باید کمتر از قیمت ورود باشد");
        });

        // اعتبارسنجی حد ضرر برای فروش
        When(x => x.Direction == TradeDirection.Sell && x.StopLoss.HasValue, () =>
        {
            RuleFor(x => x.StopLoss)
                .GreaterThan(x => x.EntryPrice)
                .WithMessage("حد ضرر فروش باید بیشتر از قیمت ورود باشد");
        });

        // اعتبارسنجی حد سود برای خرید
        When(x => x.Direction == TradeDirection.Buy && x.TakeProfit.HasValue, () =>
        {
            RuleFor(x => x.TakeProfit)
                .GreaterThan(x => x.EntryPrice)
                .WithMessage("حد سود خرید باید بیشتر از قیمت ورود باشد");
        });

        // اعتبارسنجی حد سود برای فروش
        When(x => x.Direction == TradeDirection.Sell && x.TakeProfit.HasValue, () =>
        {
            RuleFor(x => x.TakeProfit)
                .LessThan(x => x.EntryPrice)
                .WithMessage("حد سود فروش باید کمتر از قیمت ورود باشد");
        });

        // اعتبارسنجی زمان خروج
        When(x => x.ExitTime.HasValue, () =>
        {
            RuleFor(x => x.ExitTime)
                .GreaterThanOrEqualTo(x => x.EntryTime)
                .WithMessage("زمان خروج باید بعد از زمان ورود باشد");
        });

        // اعتبارسنجی قیمت خروج
        When(x => x.ExitPrice.HasValue, () =>
        {
            RuleFor(x => x.ExitPrice)
                .GreaterThan(0).WithMessage("قیمت خروج باید بیشتر از صفر باشد");
        });

        // اعتبارسنجی امتیاز اجرا
        When(x => x.ExecutionRating.HasValue, () =>
        {
            RuleFor(x => x.ExecutionRating)
                .InclusiveBetween(1, 5).WithMessage("امتیاز اجرا باید بین ۱ تا ۵ باشد");
        });

        RuleFor(x => x.Commission)
            .GreaterThanOrEqualTo(0).WithMessage("کمیسیون نمی‌تواند منفی باشد");

        RuleFor(x => x.PreTradeNotes)
            .MaximumLength(2000).WithMessage("یادداشت قبل از معامله نباید بیشتر از ۲۰۰۰ کاراکتر باشد");

        RuleFor(x => x.PostTradeNotes)
            .MaximumLength(2000).WithMessage("یادداشت بعد از معامله نباید بیشتر از ۲۰۰۰ کاراکتر باشد");

        RuleFor(x => x.EntryReason)
            .MaximumLength(1000).WithMessage("دلیل ورود نباید بیشتر از ۱۰۰۰ کاراکتر باشد");

        RuleFor(x => x.Mistakes)
            .MaximumLength(2000).WithMessage("اشتباهات نباید بیشتر از ۲۰۰۰ کاراکتر باشد");

        RuleFor(x => x.Lessons)
            .MaximumLength(2000).WithMessage("درس‌ها نباید بیشتر از ۲۰۰۰ کاراکتر باشد");
    }
}

/// <summary>
/// اعتبارسنج ویرایش معامله
/// </summary>
public class TradeUpdateValidator : AbstractValidator<TradeUpdateDto>
{
    public TradeUpdateValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("شناسه معامله نامعتبر است");

        RuleFor(x => x.AccountId)
            .GreaterThan(0).WithMessage("حساب معاملاتی را انتخاب کنید");

        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("نماد الزامی است")
            .MaximumLength(20).WithMessage("نماد نباید بیشتر از ۲۰ کاراکتر باشد");

        RuleFor(x => x.Direction)
            .IsInEnum().WithMessage("جهت معامله نامعتبر است");

        RuleFor(x => x.Volume)
            .GreaterThan(0).WithMessage("حجم باید بیشتر از صفر باشد")
            .LessThanOrEqualTo(100).WithMessage("حجم نباید بیشتر از ۱۰۰ لات باشد");

        RuleFor(x => x.EntryPrice)
            .GreaterThan(0).WithMessage("قیمت ورود باید بیشتر از صفر باشد");

        RuleFor(x => x.EntryTime)
            .NotEmpty().WithMessage("زمان ورود الزامی است");

        // اعتبارسنجی حد ضرر برای خرید
        When(x => x.Direction == TradeDirection.Buy && x.StopLoss.HasValue, () =>
        {
            RuleFor(x => x.StopLoss)
                .LessThan(x => x.EntryPrice)
                .WithMessage("حد ضرر خرید باید کمتر از قیمت ورود باشد");
        });

        // اعتبارسنجی حد ضرر برای فروش
        When(x => x.Direction == TradeDirection.Sell && x.StopLoss.HasValue, () =>
        {
            RuleFor(x => x.StopLoss)
                .GreaterThan(x => x.EntryPrice)
                .WithMessage("حد ضرر فروش باید بیشتر از قیمت ورود باشد");
        });

        // اعتبارسنجی حد سود برای خرید
        When(x => x.Direction == TradeDirection.Buy && x.TakeProfit.HasValue, () =>
        {
            RuleFor(x => x.TakeProfit)
                .GreaterThan(x => x.EntryPrice)
                .WithMessage("حد سود خرید باید بیشتر از قیمت ورود باشد");
        });

        // اعتبارسنجی حد سود برای فروش
        When(x => x.Direction == TradeDirection.Sell && x.TakeProfit.HasValue, () =>
        {
            RuleFor(x => x.TakeProfit)
                .LessThan(x => x.EntryPrice)
                .WithMessage("حد سود فروش باید کمتر از قیمت ورود باشد");
        });

        // اعتبارسنجی زمان خروج
        When(x => x.ExitTime.HasValue, () =>
        {
            RuleFor(x => x.ExitTime)
                .GreaterThanOrEqualTo(x => x.EntryTime)
                .WithMessage("زمان خروج باید بعد از زمان ورود باشد");
        });

        // اعتبارسنجی قیمت خروج
        When(x => x.ExitPrice.HasValue, () =>
        {
            RuleFor(x => x.ExitPrice)
                .GreaterThan(0).WithMessage("قیمت خروج باید بیشتر از صفر باشد");
        });

        // اعتبارسنجی امتیاز اجرا
        When(x => x.ExecutionRating.HasValue, () =>
        {
            RuleFor(x => x.ExecutionRating)
                .InclusiveBetween(1, 5).WithMessage("امتیاز اجرا باید بین ۱ تا ۵ باشد");
        });

        RuleFor(x => x.Commission)
            .GreaterThanOrEqualTo(0).WithMessage("کمیسیون نمی‌تواند منفی باشد");

        RuleFor(x => x.PreTradeNotes)
            .MaximumLength(2000).WithMessage("یادداشت قبل از معامله نباید بیشتر از ۲۰۰۰ کاراکتر باشد");

        RuleFor(x => x.PostTradeNotes)
            .MaximumLength(2000).WithMessage("یادداشت بعد از معامله نباید بیشتر از ۲۰۰۰ کاراکتر باشد");

        RuleFor(x => x.EntryReason)
            .MaximumLength(1000).WithMessage("دلیل ورود نباید بیشتر از ۱۰۰۰ کاراکتر باشد");

        RuleFor(x => x.Mistakes)
            .MaximumLength(2000).WithMessage("اشتباهات نباید بیشتر از ۲۰۰۰ کاراکتر باشد");

        RuleFor(x => x.Lessons)
            .MaximumLength(2000).WithMessage("درس‌ها نباید بیشتر از ۲۰۰۰ کاراکتر باشد");
    }
}

/// <summary>
/// اعتبارسنج بستن معامله
/// </summary>
public class TradeCloseValidator : AbstractValidator<TradeCloseDto>
{
    public TradeCloseValidator()
    {
        RuleFor(x => x.TradeId)
            .GreaterThan(0).WithMessage("شناسه معامله نامعتبر است");

        RuleFor(x => x.ExitPrice)
            .GreaterThan(0).WithMessage("قیمت خروج باید بیشتر از صفر باشد");

        RuleFor(x => x.ExitTime)
            .NotEmpty().WithMessage("زمان خروج الزامی است");
    }
}

/// <summary>
/// اعتبارسنج حساب
/// </summary>
public class AccountCreateValidator : AbstractValidator<AccountCreateDto>
{
    public AccountCreateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("نام حساب الزامی است")
            .MaximumLength(100).WithMessage("نام حساب نباید بیشتر از ۱۰۰ کاراکتر باشد");

        RuleFor(x => x.BrokerName)
            .NotEmpty().WithMessage("نام بروکر الزامی است")
            .MaximumLength(100).WithMessage("نام بروکر نباید بیشتر از ۱۰۰ کاراکتر باشد");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("نوع حساب نامعتبر است");

        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0).WithMessage("موجودی اولیه نمی‌تواند منفی باشد");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("واحد ارز الزامی است")
            .MaximumLength(10).WithMessage("واحد ارز نباید بیشتر از ۱۰ کاراکتر باشد");

        RuleFor(x => x.Leverage)
            .InclusiveBetween(1, 3000).WithMessage("لوریج باید بین ۱ تا ۳۰۰۰ باشد");
    }
}

// =============================================================================
// پایان فایل
// =============================================================================