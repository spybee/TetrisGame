using System.Windows.Forms;

namespace TetrisGame
{
    public partial class StartMenuForm : Form
    {
        public GameMode SelectedMode { get; private set; } = GameMode.None;

        public StartMenuForm()
        {
            InitializeComponent();
        }

        private void btnSinglePlayer_Click(object sender, System.EventArgs e)
        {
            SelectedMode = GameMode.SinglePlayer;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnTwoPlayer_Click(object sender, System.EventArgs e)
        {
            SelectedMode = GameMode.TwoPlayer;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnExit_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }

    public enum GameMode
    {
        None,
        SinglePlayer,
        TwoPlayer
    }
}