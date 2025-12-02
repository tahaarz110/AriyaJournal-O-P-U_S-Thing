// =============================================================================
// فایل: src/AriaJournal.Core/Infrastructure/Engines/SchemaEngine.cs
// شماره فایل: 53
// =============================================================================

using System.IO;  // ← این خط را اضافه کنید
using System.Collections.Concurrent;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using AriaJournal.Core.Domain.Common;
using AriaJournal.Core.Domain.Interfaces.Engines;
using AriaJournal.Core.Domain.Schemas;

namespace AriaJournal.Core.Infrastructure.Engines;

/// <summary>
/// پیاده‌سازی موتور Schema
/// </summary>
public class SchemaEngine : ISchemaEngine
{
    private readonly ConcurrentDictionary<string, SchemaDefinition> _schemas;
    private readonly ConcurrentDictionary<string, FormSchema> _forms;
    private readonly ICacheEngine _cacheEngine;
    private readonly IDeserializer _yamlDeserializer;
    private readonly string _schemasPath;

    public SchemaEngine(ICacheEngine cacheEngine)
    {
        _cacheEngine = cacheEngine;
        _schemas = new ConcurrentDictionary<string, SchemaDefinition>();
        _forms = new ConcurrentDictionary<string, FormSchema>();

        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        // مسیر پوشه schemas
        var appPath = AppDomain.CurrentDomain.BaseDirectory;
        _schemasPath = Path.Combine(appPath, "schemas");
    }

    public async Task InitializeAsync()
    {
        // ایجاد پوشه schemas اگر وجود ندارد
        if (!Directory.Exists(_schemasPath))
        {
            Directory.CreateDirectory(_schemasPath);
            await CreateDefaultSchemasAsync();
        }

        // بارگذاری همه فایل‌های YAML
        var yamlFiles = Directory.GetFiles(_schemasPath, "*.yaml");
        foreach (var file in yamlFiles)
        {
            await LoadAsync(file);
        }
    }

    public async Task<Result<SchemaDefinition>> LoadAsync(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return Result.Failure<SchemaDefinition>(Error.FileNotFound);
            }

            var yamlContent = await File.ReadAllTextAsync(path);
            var schema = _yamlDeserializer.Deserialize<SchemaDefinition>(yamlContent);

            if (schema == null || string.IsNullOrWhiteSpace(schema.Module))
            {
                return Result.Failure<SchemaDefinition>(Error.InvalidSchema);
            }

            // ذخیره در دیکشنری
            _schemas.AddOrUpdate(schema.Module, schema, (_, _) => schema);

            // ایندکس کردن فرم‌ها
            foreach (var form in schema.Forms)
            {
                _forms.AddOrUpdate(form.Id, form, (_, _) => form);
            }

            // کش کردن
            _cacheEngine.Set($"schema:{schema.Module}", schema, TimeSpan.FromHours(1));

            return Result.Success(schema);
        }
        catch (Exception ex)
        {
            return Result.Failure<SchemaDefinition>(
                Error.Validation($"خطا در خواندن Schema: {ex.Message}"));
        }
    }

    public SchemaDefinition? GetSchema(string module)
    {
        if (string.IsNullOrWhiteSpace(module))
            return null;

        // اول از کش بخوان
        var cached = _cacheEngine.Get<SchemaDefinition>($"schema:{module}");
        if (cached != null)
            return cached;

        // از دیکشنری بخوان
        _schemas.TryGetValue(module, out var schema);
        return schema;
    }

    public FormSchema? GetForm(string formId)
    {
        if (string.IsNullOrWhiteSpace(formId))
            return null;

        _forms.TryGetValue(formId, out var form);
        return form;
    }

    public List<FieldSchema> GetFields(string formId)
    {
        var form = GetForm(formId);
        if (form == null)
            return new List<FieldSchema>();

        return form.Sections
            .SelectMany(s => s.Fields)
            .ToList();
    }

    public List<RuleSchema> GetRules(string formId)
    {
        var form = GetForm(formId);
        return form?.Rules ?? new List<RuleSchema>();
    }

    public async Task<Result<bool>> RegisterSchemaAsync(SchemaDefinition schema)
    {
        if (schema == null || string.IsNullOrWhiteSpace(schema.Module))
        {
            return Result.Failure<bool>(Error.Validation("Schema نامعتبر است"));
        }

        _schemas.AddOrUpdate(schema.Module, schema, (_, _) => schema);

        foreach (var form in schema.Forms)
        {
            _forms.AddOrUpdate(form.Id, form, (_, _) => form);
        }

        _cacheEngine.Set($"schema:{schema.Module}", schema, TimeSpan.FromHours(1));

        return await Task.FromResult(Result.Success(true));
    }

    public void UnregisterSchema(string module)
    {
        if (string.IsNullOrWhiteSpace(module))
            return;

        if (_schemas.TryRemove(module, out var schema))
        {
            foreach (var form in schema.Forms)
            {
                _forms.TryRemove(form.Id, out _);
            }

            _cacheEngine.Remove($"schema:{module}");
        }
    }

    public async Task ReloadAllAsync()
    {
        _schemas.Clear();
        _forms.Clear();
        _cacheEngine.RemoveByPattern("schema:*");

        await InitializeAsync();
    }

    public IEnumerable<string> GetRegisteredModules()
    {
        return _schemas.Keys.ToList();
    }

    /// <summary>
    /// ایجاد Schema های پیش‌فرض
    /// </summary>
    private async Task CreateDefaultSchemasAsync()
    {
        // Schema حساب
        var accountSchema = @"module: Account
version: '1.0'
forms:
  - id: accountEntry
    titleFa: ثبت حساب جدید
    direction: rtl
    sections:
      - id: basic
        titleFa: اطلاعات پایه
        fields:
          - id: name
            labelFa: نام حساب
            type: text
            required: true
            placeholder: مثال: حساب اصلی
          - id: type
            labelFa: نوع حساب
            type: select
            required: true
            options:
              - { value: '1', labelFa: 'واقعی (Real)' }
              - { value: '2', labelFa: 'دمو (Demo)' }
              - { value: '3', labelFa: 'Prop Trading' }
              - { value: '4', labelFa: 'چالش' }
          - id: brokerName
            labelFa: نام بروکر
            type: text
            required: true
            placeholder: مثال: ICMarkets
          - id: accountNumber
            labelFa: شماره حساب
            type: text
            placeholder: اختیاری
          - id: initialBalance
            labelFa: موجودی اولیه
            type: decimal
            required: true
            validation:
              min: 0
          - id: currency
            labelFa: واحد ارز
            type: select
            required: true
            defaultValue: 'USD'
            options:
              - { value: 'USD', labelFa: 'دلار (USD)' }
              - { value: 'EUR', labelFa: 'یورو (EUR)' }
              - { value: 'GBP', labelFa: 'پوند (GBP)' }
          - id: leverage
            labelFa: لوریج
            type: integer
            defaultValue: '100'
            validation:
              min: 1
              max: 3000
    actions:
      - { id: save, labelFa: ذخیره, style: primary }
      - { id: cancel, labelFa: انصراف }
";

        // Schema معامله
        var tradeSchema = @"module: Trade
version: '1.0'
forms:
  - id: tradeEntry
    titleFa: ثبت معامله جدید
    direction: rtl
    sections:
      - id: basic
        titleFa: اطلاعات پایه
        fields:
          - id: symbol
            labelFa: نماد
            type: text
            required: true
            placeholder: مثال: XAUUSD
          - id: direction
            labelFa: جهت
            type: select
            required: true
            options:
              - { value: '1', labelFa: 'خرید (Buy)' }
              - { value: '2', labelFa: 'فروش (Sell)' }
          - id: volume
            labelFa: حجم (لات)
            type: decimal
            required: true
            validation:
              min: 0.01
              max: 100
          - id: entryPrice
            labelFa: قیمت ورود
            type: decimal
            required: true
          - id: entryTime
            labelFa: زمان ورود
            type: datetime
            required: true
      - id: levels
        titleFa: سطوح قیمتی
        fields:
          - id: stopLoss
            labelFa: حد ضرر (SL)
            type: decimal
          - id: takeProfit
            labelFa: حد سود (TP)
            type: decimal
          - id: exitPrice
            labelFa: قیمت خروج
            type: decimal
          - id: exitTime
            labelFa: زمان خروج
            type: datetime
      - id: result
        titleFa: نتیجه
        fields:
          - id: profitLoss
            labelFa: سود/زیان
            type: decimal
            readOnly: true
          - id: commission
            labelFa: کمیسیون
            type: decimal
            defaultValue: '0'
          - id: swap
            labelFa: سواپ
            type: decimal
            defaultValue: '0'
      - id: notes
        titleFa: یادداشت‌ها
        fields:
          - id: entryReason
            labelFa: دلیل ورود
            type: textarea
          - id: preTradeNotes
            labelFa: یادداشت قبل از معامله
            type: textarea
          - id: postTradeNotes
            labelFa: یادداشت بعد از معامله
            type: textarea
          - id: mistakes
            labelFa: اشتباهات
            type: textarea
          - id: lessons
            labelFa: درس‌های آموخته شده
            type: textarea
      - id: evaluation
        titleFa: ارزیابی
        fields:
          - id: executionRating
            labelFa: امتیاز اجرا
            type: rating
          - id: followedPlan
            labelFa: طبق پلن بود؟
            type: boolean
          - id: isImpulsive
            labelFa: ورود هیجانی بود؟
            type: boolean
    actions:
      - { id: save, labelFa: ذخیره, style: primary }
      - { id: saveAndNew, labelFa: ذخیره و جدید }
      - { id: cancel, labelFa: انصراف }
    rules:
      - id: validateBuySL
        trigger: OnSave
        condition: 'direction == 1 AND stopLoss != null AND stopLoss >= entryPrice'
        action: Validate
        target: stopLoss
        value: حد ضرر خرید باید کمتر از قیمت ورود باشد
      - id: validateSellSL
        trigger: OnSave
        condition: 'direction == 2 AND stopLoss != null AND stopLoss <= entryPrice'
        action: Validate
        target: stopLoss
        value: حد ضرر فروش باید بیشتر از قیمت ورود باشد
      - id: calculatePL
        trigger: OnChange
        dependsOn: exitPrice
        condition: 'exitPrice != null AND entryPrice != null'
        action: Calculate
        target: profitLoss
        value: '(exitPrice - entryPrice) * volume * 100000'
";

        // Schema تنظیمات
        var settingsSchema = @"module: Settings
version: '1.0'
forms:
  - id: settingsForm
    titleFa: تنظیمات
    direction: rtl
    sections:
      - id: appearance
        titleFa: ظاهر برنامه
        fields:
          - id: theme
            labelFa: تم
            type: select
            defaultValue: 'Dark'
            options:
              - { value: 'Dark', labelFa: 'تیره' }
              - { value: 'Light', labelFa: 'روشن' }
          - id: language
            labelFa: زبان
            type: select
            defaultValue: 'fa'
            options:
              - { value: 'fa', labelFa: 'فارسی' }
              - { value: 'en', labelFa: 'English' }
      - id: display
        titleFa: نمایش
        fields:
          - id: dateFormat
            labelFa: فرمت تاریخ
            type: select
            defaultValue: 'yyyy/MM/dd'
            options:
              - { value: 'yyyy/MM/dd', labelFa: '1403/01/15' }
              - { value: 'dd/MM/yyyy', labelFa: '15/01/1403' }
          - id: priceDecimals
            labelFa: تعداد اعشار قیمت
            type: integer
            defaultValue: '5'
            validation:
              min: 0
              max: 8
          - id: itemsPerPage
            labelFa: تعداد آیتم در هر صفحه
            type: integer
            defaultValue: '50'
            validation:
              min: 10
              max: 200
      - id: backup
        titleFa: پشتیبان‌گیری
        fields:
          - id: autoBackup
            labelFa: پشتیبان‌گیری خودکار
            type: boolean
            defaultValue: 'true'
          - id: autoBackupInterval
            labelFa: بازه پشتیبان‌گیری (روز)
            type: integer
            defaultValue: '7'
            validation:
              min: 1
              max: 30
          - id: backupPath
            labelFa: مسیر پشتیبان‌گیری
            type: text
    actions:
      - { id: save, labelFa: ذخیره, style: primary }
      - { id: reset, labelFa: بازنشانی }
";

        // ذخیره فایل‌ها
        await File.WriteAllTextAsync(Path.Combine(_schemasPath, "schema.account.yaml"), accountSchema);
        await File.WriteAllTextAsync(Path.Combine(_schemasPath, "schema.trade.yaml"), tradeSchema);
        await File.WriteAllTextAsync(Path.Combine(_schemasPath, "schema.settings.yaml"), settingsSchema);
    }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Infrastructure/Engines/SchemaEngine.cs
// =============================================================================