// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Common/Result.cs
// شماره فایل: 1
// توضیح: کلاس Result برای مدیریت خطاها - اصلاح شده
// =============================================================================

namespace AriaJournal.Core.Domain.Common;

/// <summary>
/// کلاس نتیجه برای مدیریت خطاها بدون Exception
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        // اعتبارسنجی: اگر موفق است نباید خطا داشته باشد
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("نتیجه موفق نمی‌تواند خطا داشته باشد");
        
        // اعتبارسنجی: اگر ناموفق است باید خطا داشته باشد
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("نتیجه ناموفق باید خطا داشته باشد");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value, true, Error.None);
    public static Result<T> Failure<T>(Error error) => new(default, false, error);
}

/// <summary>
/// کلاس نتیجه جنریک با مقدار برگشتی
/// </summary>
public class Result<T> : Result
{
    private readonly T? _value;

    public T Value
    {
        get
        {
            if (IsFailure)
                throw new InvalidOperationException("نمی‌توان به مقدار نتیجه ناموفق دسترسی داشت");
            return _value!;
        }
    }

    internal Result(T? value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        _value = value;
    }

    public static implicit operator Result<T>(T value) => Success(value);
    
    /// <summary>
    /// دریافت مقدار یا مقدار پیش‌فرض در صورت خطا
    /// </summary>
    public T GetValueOrDefault(T defaultValue = default!)
    {
        return IsSuccess ? _value! : defaultValue;
    }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Common/Result.cs
// =============================================================================