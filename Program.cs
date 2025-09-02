using System;
using System.Windows.Forms;

namespace TetrisGame
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 持續顯示選單直到使用者選擇退出
            while (true)
            {
                using (StartMenuForm startMenu = new StartMenuForm())
                {
                    if (startMenu.ShowDialog() == DialogResult.OK)
                    {
                        // 根據選擇的模式啟動遊戲
                        using (GameForm gameForm = new GameForm(startMenu.SelectedMode))
                        {
                            gameForm.ShowDialog();

                            // 檢查是否要完全退出程式
                            if (gameForm.ExitApplication)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        // 使用者點擊了退出按鈕
                        break;
                    }
                }
            }
        }
    }
}