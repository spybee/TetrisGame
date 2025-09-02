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

            // ������ܿ�檽��ϥΪ̿�ܰh�X
            while (true)
            {
                using (StartMenuForm startMenu = new StartMenuForm())
                {
                    if (startMenu.ShowDialog() == DialogResult.OK)
                    {
                        // �ھڿ�ܪ��Ҧ��ҰʹC��
                        using (GameForm gameForm = new GameForm(startMenu.SelectedMode))
                        {
                            gameForm.ShowDialog();

                            // �ˬd�O�_�n�����h�X�{��
                            if (gameForm.ExitApplication)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        // �ϥΪ��I���F�h�X���s
                        break;
                    }
                }
            }
        }
    }
}