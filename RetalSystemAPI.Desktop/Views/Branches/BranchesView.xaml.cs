using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.Desktop.Controls;
using RetalSystemAPI.Desktop.Models.Branch;
using RetalSystemAPI.Desktop.ViewModels.Branches;
using RetalSystemAPI.Desktop.Views.Branches.Dialogs;

namespace RetalSystemAPI.Desktop.Views.Branches;

public partial class BranchesView : UserControl
{
    public BranchesView(BranchesViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.OpenDialogHandler = OpenBranchDialog;
        viewModel.ConfirmDeleteHandler = (title, message) =>
        {
            var owner = Window.GetWindow(this);
            bool confirmed = ModernConfirmDialog.ShowConfirm(owner, title, message, "تأكيد الحذف", "إلغاء", ConfirmDialogType.Danger);
            return Task.FromResult(confirmed);
        };
    }

    private Task OpenBranchDialog(BranchDto? branch)
    {
        var app = (App)Application.Current;
        var dialogVM = app.Services.GetRequiredService<BranchFormViewModel>();
        dialogVM.Initialize(branch);

        var dialog = new BranchFormDialog(dialogVM)
        {
            Owner = Window.GetWindow(this)
        };

        dialogVM.CloseWindowHandler = () => dialog.DialogResult = true;
        dialog.ShowDialog();

        return Task.CompletedTask;
    }
}
