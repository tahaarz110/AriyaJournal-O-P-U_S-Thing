// =============================================================================
// فایل: src/AriaJournal.Core/UI/Views/FieldEditorView.xaml.cs
// توضیح: کد پشت ویوی مدیریت فیلدها
// =============================================================================

using System.Windows.Controls;
using AriaJournal.Core.UI.ViewModels;

namespace AriaJournal.Core.UI.Views;

/// <summary>
/// کد پشت ویوی مدیریت فیلدها
/// </summary>
public partial class FieldEditorView : UserControl
{
    public FieldEditorView()
    {
        InitializeComponent();
    }

    public FieldEditorView(FieldEditorViewModel viewModel) : this()
    {
        DataContext = viewModel;
        Loaded += async (s, e) => await viewModel.InitializeAsync();
    }
}

// =============================================================================
// پایان فایل
// =============================================================================