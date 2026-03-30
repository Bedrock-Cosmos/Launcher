using System.Windows.Forms;

namespace BedrockCosmos.App
{
    internal static class LocalizedMessageBox
    {
        internal static DialogResult ShowInfoWithTitle(string message, string title)
        {
            return MessageBox.Show(
                message,
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        internal static DialogResult ShowInfo(string message, string titleKey = "Common.Dialog.InfoTitle")
        {
            return MessageBox.Show(
                message,
                LanguageHandler.Get(titleKey),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        internal static DialogResult ShowWarning(string message, string titleKey = "Common.Dialog.WarningTitle")
        {
            return MessageBox.Show(
                message,
                LanguageHandler.Get(titleKey),
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        internal static DialogResult ShowError(string message, string titleKey = "Common.Dialog.ErrorTitle")
        {
            return MessageBox.Show(
                message,
                LanguageHandler.Get(titleKey),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
