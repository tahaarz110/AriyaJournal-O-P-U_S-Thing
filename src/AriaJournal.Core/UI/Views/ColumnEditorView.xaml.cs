// =============================================================================
// فایل: src/AriaJournal.Core/UI/Views/ColumnEditorView.xaml.cs
// توضیح: کد پشت ویوی مدیریت ستون‌ها
// =============================================================================

using System.Windows.Controls;
using AriaJournal.Core.UI.ViewModels;

namespace AriaJournal.Core.UI.Views;

/// <summary>
/// کد پشت ویوی مدیریت ستون‌ها
/// </summary>
public partial class ColumnEditorView : UserControl
{
    public ColumnEditorView()
    {
        InitializeComponent();
    }

    public ColumnEditorView(ColumnEditorViewModel viewModel) : this()
    {
        DataContext = viewModel;
        Loaded += async (s, e) => await viewModel.InitializeAsync();
    }
}

// =============================================================================
// پایان فایل
// =============================================================================