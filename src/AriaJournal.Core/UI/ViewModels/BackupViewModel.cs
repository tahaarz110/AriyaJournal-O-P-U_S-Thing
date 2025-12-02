// =============================================================================
// فایل: src/AriaJournal.Core/UI/ViewModels/BackupViewModel.cs
// توضیح: ViewModel پشتیبان‌گیری - اصلاح‌شده
// =============================================================================

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Input;
using AriaJournal.Core.Domain.Interfaces.Engines;

namespace AriaJournal.Core.UI.ViewModels;

/// <summary>
/// ViewModel پشتیبان‌گیری
/// </summary>
public partial class BackupViewModel : BaseViewModel
{
    private readonly IBackupEngine _backupEngine;

    private ObservableCollection<BackupItemViewModel> _backups;
    private BackupItemViewModel? _selectedBackup;
    private string _busyMessage = string.Empty;
    private bool _isPasswordDialogOpen;
    private string _passwordInput = string.Empty;
    private string _passwordDialogTitle = string.Empty;
    private Action<string?>? _passwordCallback;

    public BackupViewModel(IBackupEngine backupEngine)
    {
        _backupEngine = backupEngine ?? throw new ArgumentNullException(nameof(backupEngine));
        _backups = new ObservableCollection<BackupItemViewModel>();

        Title = "پشتیبان‌گیری";
    }

    #region Properties

    public ObservableCollection<BackupItemViewModel> Backups
    {
        get => _backups;
        set => SetProperty(ref _backups, value);
    }

    public BackupItemViewModel? SelectedBackup
    {
        get => _selectedBackup;
        set => SetProperty(ref _selectedBackup, value);
    }

    public string BackupPath => _backupEngine.BackupPath;

    public int BackupsCount => Backups.Count;

    public string LastBackupDate
    {
        get
        {
            if (Backups.Count == 0) return "ندارد";
            return Backups[0].CreatedAt.ToString("yyyy/MM/dd HH:mm");
        }
    }

    public bool HasBackups => Backups.Count > 0;
    public bool IsEmpty => !IsBusy && Backups.Count == 0;

    public string BusyMessage
    {
        get => _busyMessage;
        set => SetProperty(ref _busyMessage, value);
    }

    public bool IsPasswordDialogOpen
    {
        get => _isPasswordDialogOpen;
        set => SetProperty(ref _isPasswordDialogOpen, value);
    }

    public string PasswordInput
    {
        get => _passwordInput;
        set => SetProperty(ref _passwordInput, value);
    }

    public string PasswordDialogTitle
    {
        get => _passwordDialogTitle;
        set => SetProperty(ref _passwordDialogTitle, value);
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task CreateBackupAsync()
    {
        var result = MessageBox.Show(
            "آیا می‌خواهید نسخه پشتیبان رمزنگاری شود؟",
            "ایجاد پشتیبان",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Cancel)
            return;

        string? password = null;
        if (result == MessageBoxResult.Yes)
        {
            password = await ShowPasswordDialogAsync("رمز عبور برای رمزنگاری پشتیبان:");
            if (string.IsNullOrEmpty(password)) return;
        }

        BusyMessage = "در حال ایجاد پشتیبان...";
        await ExecuteAsync(async () =>
        {
            var backupResult = await _backupEngine.CreateBackupAsync(password);

            if (backupResult.IsSuccess)
            {
                ShowSuccess($"پشتیبان با موفقیت ایجاد شد:\n{backupResult.Value.FileName}");
                await LoadBackupsAsync();
            }
            else
            {
                ShowError(backupResult.Error.Message);
            }
        }, "خطا در ایجاد پشتیبان");
    }

    [RelayCommand]
    private async Task RestoreAsync(BackupItemViewModel? backup)
    {
        if (backup == null) return;

        var confirm = MessageBox.Show(
            "⚠️ هشدار: بازیابی پشتیبان، تمام اطلاعات فعلی را جایگزین می‌کند.\n\nآیا مطمئن هستید؟",
            "تأیید بازیابی",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes)
            return;

        string? password = null;
        if (backup.IsEncrypted)
        {
            password = await ShowPasswordDialogAsync("رمز عبور فایل پشتیبان:");
            if (string.IsNullOrEmpty(password)) return;
        }

        BusyMessage = "در حال بازیابی...";
        await ExecuteAsync(async () =>
        {
            var restoreResult = await _backupEngine.RestoreAsync(backup.FilePath, password);

            if (restoreResult.IsSuccess)
            {
                MessageBox.Show(
                    "بازیابی با موفقیت انجام شد.\nبرنامه باید مجدداً راه‌اندازی شود.",
                    "موفقیت",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // راه‌اندازی مجدد برنامه
                var processPath = Environment.ProcessPath;
                if (!string.IsNullOrEmpty(processPath))
                {
                    System.Diagnostics.Process.Start(processPath);
                    System.Windows.Application.Current.Shutdown();
                }
            }
        }, "خطا در بازیابی");
    }

    [RelayCommand]
    private async Task DeleteBackupAsync(BackupItemViewModel? backup)
    {
        if (backup == null) return;

        var confirm = MessageBox.Show(
            $"آیا از حذف پشتیبان «{backup.FileName}» مطمئن هستید؟",
            "تأیید حذف",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes)
            return;

        await ExecuteAsync(async () =>
        {
            var deleteResult = await _backupEngine.DeleteBackupAsync(backup.FilePath);

            if (deleteResult.IsSuccess)
            {
                ShowSuccess("پشتیبان حذف شد");
                await LoadBackupsAsync();
            }
            else
            {
                ShowError(deleteResult.Error.Message);
            }
        }, "خطا در حذف پشتیبان");
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadBackupsAsync();
    }

    [RelayCommand]
    private void ConfirmPassword()
    {
        IsPasswordDialogOpen = false;
        _passwordCallback?.Invoke(PasswordInput);
        PasswordInput = string.Empty;
    }

    [RelayCommand]
    private void CancelPassword()
    {
        IsPasswordDialogOpen = false;
        _passwordCallback?.Invoke(null);
        PasswordInput = string.Empty;
    }

    #endregion

    #region Private Methods

    private async Task LoadBackupsAsync()
    {
        await ExecuteAsync(async () =>
        {
            await Task.Run(() =>
            {
                var backups = _backupEngine.GetBackups();

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Backups.Clear();
                    foreach (var backup in backups)
                    {
                        Backups.Add(new BackupItemViewModel
                        {
                            FileName = backup.FileName,
                            FilePath = backup.FilePath,
                            FileSize = backup.FileSize,
                            CreatedAt = backup.CreatedAt,
                            IsEncrypted = backup.IsEncrypted
                        });
                    }

                    OnPropertyChanged(nameof(BackupsCount));
                    OnPropertyChanged(nameof(LastBackupDate));
                    OnPropertyChanged(nameof(HasBackups));
                    OnPropertyChanged(nameof(IsEmpty));
                });
            });
        });
    }

    private Task<string?> ShowPasswordDialogAsync(string title)
    {
        var tcs = new TaskCompletionSource<string?>();

        PasswordDialogTitle = title;
        PasswordInput = string.Empty;
        _passwordCallback = result => tcs.TrySetResult(result);
        IsPasswordDialogOpen = true;

        return tcs.Task;
    }

    #endregion

    public override async Task InitializeAsync()
    {
        await LoadBackupsAsync();
    }
}

/// <summary>
/// ViewModel آیتم پشتیبان
/// </summary>
public class BackupItemViewModel
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsEncrypted { get; set; }

    public string FileSizeDisplay
    {
        get
        {
            if (FileSize < 1024)
                return $"{FileSize} B";
            if (FileSize < 1024 * 1024)
                return $"{FileSize / 1024.0:N1} KB";
            return $"{FileSize / (1024.0 * 1024):N1} MB";
        }
    }

    public string EncryptedDisplay => IsEncrypted ? "بله" : "خیر";
}

// =============================================================================
// پایان فایل
// =============================================================================