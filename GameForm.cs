using System.Drawing;
using Timer = System.Windows.Forms.Timer;

namespace TetrisGame
{
    public partial class GameForm : Form
    {
        private GameMode currentGameMode = GameMode.TwoPlayer;

        private const int BoardWidth = 10;  // 棋盤寬度
        private const int BoardHeight = 20; // 棋盤高度
        private const int BlockSize = 25;   // 方塊大小

        // 預覽區域設定
        private const int PreviewSize = 20; // 預覽方塊大小
        private const int PreviewAreaWidth = (6 * PreviewSize) + 5; // 預覽區域寬度
        private const int PreviewAreaHeight = (6 * PreviewSize) + 5; // 預覽區域高度

        private Board gameBoard1 = null!;
        private Board gameBoard2 = null!;
        private Timer gameTimer = null!;
        private bool isGameOver1;
        private bool isGameOver2;
        private bool isPaused = false; // 共用暫停（修正為不在繪圖中啟停 Timer）
        private int score1;
        private int score2;

        // 版面計算
        private int leftBoardOffsetX => 0;       // 左側留空間放 P1 預覽
        private int leftPreviewAreaX => leftBoardOffsetX + (BlockSize * (BoardWidth + 2)); // P1 預覽區 X 座標
        private int rightPreviewAreaX => leftPreviewAreaX + PreviewAreaWidth + BlockSize; // P2 預覽區 X 座標 中間留 BlockSize
        private int rightBoardOffsetX => rightPreviewAreaX + PreviewAreaWidth; // 中間留 BlockSize
        private int SoloFormWidth => leftPreviewAreaX + PreviewAreaWidth + BlockSize + 15; 
        private int DuelFormWidth => (leftPreviewAreaX + PreviewAreaWidth + BlockSize) * 2 - 9;

        // 添加這個屬性來判斷是否要完全退出應用程式
        public bool ExitApplication { get; private set; } = false;
                
        // 覆寫表單關閉事件
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            // 停止計時器
            if (gameTimer != null)
            {
                gameTimer.Stop();
                gameTimer.Dispose();
            }
        }

        public GameForm(GameMode mode)
        {
            currentGameMode = mode;
            InitializeComponent();
            InitializeGame();
            SetupTimer();
            this.DoubleBuffered = true;
            this.Paint += GameForm_Paint;
            this.KeyDown += GameForm_KeyDown;
        }

        private void InitializeGame()
        {
            gameBoard1 = new Board(BoardWidth, BoardHeight);

            // 只有在雙人模式下才初始化第二個遊戲板
            if (currentGameMode == GameMode.TwoPlayer)
            {
                gameBoard2 = new Board(BoardWidth, BoardHeight);
                isGameOver2 = false;
                score2 = 0;
                gameBoard2.SpawnNewPiece();
            }
            else
            {
                isGameOver2 = true; // 單人模式下標記為遊戲結束
            }

            isGameOver1 = false;
            isPaused = false;
            score1 = 0;
            gameBoard1.SpawnNewPiece();
            Invalidate();
        }

        private void SetupTimer()
        {
            gameTimer = new Timer { Interval = 1000 }; // 沿用你的初始速度
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            if (isPaused) return;

            if (!isGameOver1) UpdateBoard(gameBoard1, ref score1, ref isGameOver1);
            if (currentGameMode == GameMode.TwoPlayer && !isGameOver2)
                UpdateBoard(gameBoard2, ref score2, ref isGameOver2);

            if (isGameOver1 || (currentGameMode == GameMode.TwoPlayer && isGameOver2))
            {
                isPaused = false;
                gameTimer.Stop();
            }

            Invalidate();
        }

        private void UpdateBoard(Board board, ref int score, ref bool isGameOver)
        {
            if (!board.MovePieceDown())
            {
                board.MergePiece();
                int linesCleared = board.ClearLines();
                score += linesCleared * 100; // 保留你的得分規則

                if (board.IsGameOver())
                {
                    isGameOver = true;
                }
                else
                {
                    board.SpawnNewPiece();
                }
            }
        }

        private void GameForm_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // 繪製玩家1的棋盤
            DrawGrid(g, leftBoardOffsetX);
            DrawBoard(g, gameBoard1, leftBoardOffsetX);
            DrawCurrentPiece(g, gameBoard1, leftBoardOffsetX);

            // 只有在雙人模式下才繪製玩家2的棋盤
            if (currentGameMode == GameMode.TwoPlayer)
            {
                DrawGrid(g, rightBoardOffsetX);
                DrawBoard(g, gameBoard2, rightBoardOffsetX);
                DrawCurrentPiece(g, gameBoard2, rightBoardOffsetX);
            }

            // 繪製預覽區
            DrawNextPiecePreview(g, gameBoard1, leftPreviewAreaX, BlockSize * 3, "P1 NEXT");

            // 只有在雙人模式下才繪製玩家2的預覽區
            if (currentGameMode == GameMode.TwoPlayer)
            {
                DrawNextPiecePreview(g, gameBoard2, rightPreviewAreaX, BlockSize * 3, "P2 NEXT");
            }

            // 繪製分數
            DrawScore(g, score1, leftBoardOffsetX + BlockSize, 30, "P1");

            // 只有在雙人模式下才繪製玩家2的分數
            if (currentGameMode == GameMode.TwoPlayer)
            {
                DrawScore(g, score2, rightBoardOffsetX + BlockSize, 30, "P2");
            }

            // 勝負/暫停提示
            if (isGameOver1 || (currentGameMode == GameMode.TwoPlayer && isGameOver2))
                DrawDuelResult(g);

            if (isPaused && !(isGameOver1 || (currentGameMode == GameMode.TwoPlayer && isGameOver2)))
                DrawPause(g);
        }

        // === 視覺繪製 ===
        private static void DrawGrid(Graphics g, int offsetX)
        {
            using Pen dashPen = new(Color.Gray, 1) { DashPattern = [4, 2] };
            for (int x = 1; x <= BoardWidth + 1; x++)
            {
                g.DrawLine(dashPen, offsetX + x * BlockSize, BlockSize, offsetX + x * BlockSize, (BoardHeight + 1) * BlockSize);
            }
            for (int y = 1; y <= BoardHeight + 1; y++)
            {
                g.DrawLine(dashPen, offsetX + BlockSize, y * BlockSize, offsetX + (BoardWidth + 1) * BlockSize, y * BlockSize);
            }
        }

        private static void DrawBoard(Graphics g, Board board, int offsetX)
        {
            for (int y = 0; y < BoardHeight; y++)
            {
                for (int x = 0; x < BoardWidth; x++)
                {
                    if (board.Grid[x, y] > 0)
                    {
                        DrawBlock(g, x, y, Tetromino.GetColor(board.Grid[x, y]), offsetX);
                    }
                }
            }
        }

        private static void DrawCurrentPiece(Graphics g, Board board, int offsetX)
        {
            Tetromino current = board.CurrentPiece;
            for (int y = 0; y < current.Shape.GetLength(0); y++)
            {
                for (int x = 0; x < current.Shape.GetLength(1); x++)
                {
                    if (current.Shape[y, x] != 0)
                    {
                        DrawBlock(g, current.Position.X + x, current.Position.Y + y, current.Color, offsetX);
                    }
                }
            }
        }
                
        private static void DrawBlock(Graphics g, int x, int y, Color color, int offsetX)
        {
            int pixelX = offsetX + (x * BlockSize) + BlockSize;
            int pixelY = (y * BlockSize) + BlockSize;

            using (SolidBrush brush = new(color))
            {
                g.FillRectangle(brush, pixelX, pixelY, BlockSize, BlockSize);
            }
            using Pen borderPen = new(Color.Black, 1);
            g.DrawRectangle(borderPen, pixelX, pixelY, BlockSize, BlockSize);
        }

        private static void DrawScore(Graphics g, int score, int x, int y, string label)
        {
            g.DrawString($"{label} SCORE : {score}", new Font("Arial", 12, FontStyle.Bold), Brushes.White, new PointF(x, y));
        }

        private void DrawDuelResult(Graphics g)
        {
            string msg;

            if (currentGameMode == GameMode.SinglePlayer)
            {
                msg = "Game Over!";
            }
            else
            {
                msg = (isGameOver1 && !isGameOver2) ? "Player 2 Wins!" :
                     (!isGameOver1 && isGameOver2) ? "Player 1 Wins!" :
                     "Draw!";
            }

            using Font font = new("Arial", 20, FontStyle.Bold);
            var size = g.MeasureString(msg, font);
            g.DrawString(msg, font, Brushes.Yellow, (this.ClientSize.Width - size.Width) / 2f, (this.ClientSize.Height - size.Height) / 2f);

            string restartText = "Press F5 to Restart";
            using Font restartFont = new("Arial", 14, FontStyle.Bold);
            float restartX = (this.ClientSize.Width - g.MeasureString(restartText, restartFont).Width) / 2f;
            float restartY = (this.ClientSize.Height) / 2f + 40;
            g.DrawString(restartText, restartFont, Brushes.White, restartX + 1, restartY + 1);
            g.DrawString(restartText, restartFont, Brushes.Yellow, restartX, restartY);
        }

        private static void DrawPause(Graphics g)
        {
            string text = "Paused - Press Pause to Continue";
            using Font font = new("Arial", 14, FontStyle.Bold);
            float x = (g.VisibleClipBounds.Width - g.MeasureString(text, font).Width) / 2f;
            float y = (g.VisibleClipBounds.Height) / 2f + 40;
            g.DrawString(text, font, Brushes.White, x + 1, y + 1);
            g.DrawString(text, font, Brushes.Yellow, x, y);
        }

        private void DrawNextPiecePreview(Graphics g, Board board, int previewX, int previewY, string title)
        {
            using (SolidBrush backgroundBrush = new(Color.FromArgb(30, 30, 30)))
            {
                g.FillRectangle(backgroundBrush, previewX, previewY, PreviewAreaWidth, PreviewAreaHeight);
            }
            using (Pen borderPen = new(Color.Gray, 2))
            {
                g.DrawRectangle(borderPen, previewX + 1, previewY, PreviewAreaWidth, PreviewAreaHeight);
            }
            using (Font titleFont = new("Arial", 14, FontStyle.Bold))
            {
                g.DrawString(title, titleFont, Brushes.White, new PointF(previewX + 10, previewY - 25));
            }

            if (board.NextPiece != null)
            {
                DrawPreviewPiece(g, board.NextPiece, previewX, previewY);
            }
        }

        private static void DrawPreviewPiece(Graphics g, Tetromino piece, int offsetX, int offsetY)
        {
            int pieceWidth = piece.Shape.GetLength(1) * PreviewSize;
            int pieceHeight = piece.Shape.GetLength(0) * PreviewSize;
            int startX = offsetX + (PreviewAreaWidth - pieceWidth) / 2;
            int startY = offsetY + (PreviewAreaHeight - pieceHeight) / 2;

            for (int y = 0; y < piece.Shape.GetLength(0); y++)
            {
                for (int x = 0; x < piece.Shape.GetLength(1); x++)
                {
                    if (piece.Shape[y, x] != 0)
                    {
                        DrawPreviewBlock(g, startX + (x * PreviewSize), startY + (y * PreviewSize), piece.Color);
                    }
                }
            }
        }

        private static void DrawPreviewBlock(Graphics g, int x, int y, Color color)
        {
            using (SolidBrush brush = new(color))
            {
                g.FillRectangle(brush, x, y, PreviewSize, PreviewSize);
            }
            using Pen borderPen = new(Color.Black, 1);
            g.DrawRectangle(borderPen, x, y, PreviewSize, PreviewSize);
        }

        private void GameForm_KeyDown(object? sender, KeyEventArgs e)
        {
            // 添加退出到選單的按鍵
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.OK; // 表示返回選單
                this.Close();
                return;
            }

            // 現有的按鍵處理程式碼保持不變...
            // 若已有人 Game Over，僅允許 F5 重開
            if (isGameOver1 || (currentGameMode == GameMode.TwoPlayer && isGameOver2))
            {
                if (e.KeyCode == Keys.F5)
                {
                    InitializeGame();
                    gameTimer.Start();
                }
                else if (e.KeyCode == Keys.Escape) // 遊戲結束時也可以按 ESC 返回選單
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                return;
            }            

            switch (e.KeyCode)
            {
                // --- Player 1
                case Keys.Left : gameBoard1.MovePieceLeft(); break;
                case Keys.Right: gameBoard1.MovePieceRight(); break;
                case Keys.Down: gameBoard1.MovePieceDown(); break;
                case Keys.Q: gameBoard1.LeftRotatePiece(); break;
                case Keys.E: gameBoard1.RightRotatePiece(); break;
                case Keys.Space: gameBoard1.DropPiece(); break;

                //---Player 2(僅在雙人模式下有效)
                //case Keys.Left:
                //    if (currentGameMode == GameMode.TwoPlayer)
                //        gameBoard2.MovePieceLeft();
                //    break;
                //case Keys.Right:
                //    if (currentGameMode == GameMode.TwoPlayer)
                //        gameBoard2.MovePieceRight();
                //    break;
                //case Keys.Down:
                //    if (currentGameMode == GameMode.TwoPlayer)
                //        gameBoard2.MovePieceDown();
                //    break;
                //case Keys.Insert:
                //    if (currentGameMode == GameMode.TwoPlayer)
                //        gameBoard2.LeftRotatePiece();
                //    break;
                //case Keys.PageUp:
                //    if (currentGameMode == GameMode.TwoPlayer)
                //        gameBoard2.RightRotatePiece();
                //    break;
                //case Keys.Home:
                //    if (currentGameMode == GameMode.TwoPlayer)
                //        gameBoard2.DropPiece();
                //    break;

                // 共用
                case Keys.F5:
                    InitializeGame();
                    gameTimer.Start();
                    break;
                case Keys.Pause:
                    isPaused = !isPaused;
                    break;
            }

            Invalidate();
        }

        private void GameForm_Load(object sender, EventArgs e)
        {
            this.Text = currentGameMode == GameMode.SinglePlayer ? "Tetris Single" : "Tetris Duel";

            if (currentGameMode == GameMode.SinglePlayer)
            {
                this.Width = SoloFormWidth;
                // 隱藏第二個玩家的相關內容
                gameBoard2 = null;
                isGameOver2 = true; // 標記為遊戲結束狀態，避免更新
            }
            else
            {
                this.Width = DuelFormWidth;
            }

            this.Height = (BoardHeight + 2) * BlockSize + 40;
            this.MaximizeBox = false;
            this.BackColor = Color.Black;
        }

    }

    // ===== 以下保留你原始的 Board / Tetromino（未改動邏輯）=====
    public class Board(int width, int height)
    {
        public int[,] Grid { get; private set; } = new int[width, height];
        public Tetromino? CurrentPiece { get; private set; }
        public Tetromino NextPiece { get; private set; } = new Tetromino();
        private readonly Random random = new();
        private readonly Queue<int> pieceBag = new();

        private void FillBag()
        {
            int[] pieces = [1, 2, 3, 4, 5, 6, 7];
            for (int i = pieces.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (pieces[i], pieces[j]) = (pieces[j], pieces[i]);
            }
            foreach (int piece in pieces)
            {
                pieceBag.Enqueue(piece);
            }
        }

        private int GetNextPieceType()
        {
            if (pieceBag.Count == 0) FillBag();
            return pieceBag.Dequeue();
        }

        public void SpawnNewPiece()
        {
            CurrentPiece = NextPiece;
            CurrentPiece.Position = new Point(3, 0);
            int nextType = GetNextPieceType();
            NextPiece = new Tetromino(nextType);
        }

        public bool MovePieceDown()
        {
            if (CurrentPiece == null) return false;
            CurrentPiece.Position = new Point(CurrentPiece.Position.X, CurrentPiece.Position.Y + 1);
            if (CheckCollision())
            {
                CurrentPiece.Position = new Point(CurrentPiece.Position.X, CurrentPiece.Position.Y - 1);
                return false;
            }
            return true;
        }

        public void MovePieceLeft()
        {
            if (CurrentPiece == null) return;
            CurrentPiece.Position = new Point(CurrentPiece.Position.X - 1, CurrentPiece.Position.Y);
            if (CheckCollision())
            {
                CurrentPiece.Position = new Point(CurrentPiece.Position.X + 1, CurrentPiece.Position.Y);
            }
        }

        public void MovePieceRight()
        {
            if (CurrentPiece == null) return;
            CurrentPiece.Position = new Point(CurrentPiece.Position.X + 1, CurrentPiece.Position.Y);
            if (CheckCollision())
            {
                CurrentPiece.Position = new Point(CurrentPiece.Position.X - 1, CurrentPiece.Position.Y);
            }
        }

        public void RightRotatePiece()
        {
            int[,] original = CurrentPiece.Shape;
            Point originalPosition = CurrentPiece.Position;
            CurrentPiece.Rotate();
            if (CheckCollision())
            {
                CurrentPiece.Position = new Point(originalPosition.X - 1, originalPosition.Y);
                if (!CheckCollision()) return;
                CurrentPiece.Position = new Point(originalPosition.X + 1, originalPosition.Y);
                if (!CheckCollision()) return;
                CurrentPiece.Position = new Point(originalPosition.X - 2, originalPosition.Y);
                if (!CheckCollision()) return;
                CurrentPiece.Position = new Point(originalPosition.X + 2, originalPosition.Y);
                if (!CheckCollision()) return;
                CurrentPiece.Position = new Point(originalPosition.X, originalPosition.Y - 1);
                if (!CheckCollision()) return;
                CurrentPiece.Shape = original;
                CurrentPiece.Position = originalPosition;
            }
        }

        public void LeftRotatePiece()
        {
            int[,] original = CurrentPiece.Shape;
            Point originalPosition = CurrentPiece.Position;
            CurrentPiece.Rotate();
            CurrentPiece.Rotate();
            CurrentPiece.Rotate();
            if (CheckCollision())
            {
                CurrentPiece.Position = new Point(originalPosition.X - 1, originalPosition.Y);
                if (!CheckCollision()) return;
                CurrentPiece.Position = new Point(originalPosition.X + 1, originalPosition.Y);
                if (!CheckCollision()) return;
                CurrentPiece.Position = new Point(originalPosition.X - 2, originalPosition.Y);
                if (!CheckCollision()) return;
                CurrentPiece.Position = new Point(originalPosition.X + 2, originalPosition.Y);
                if (!CheckCollision()) return;
                CurrentPiece.Position = new Point(originalPosition.X, originalPosition.Y - 1);
                if (!CheckCollision()) return;
                CurrentPiece.Shape = original;
                CurrentPiece.Position = originalPosition;
            }
        }

        public void DropPiece()
        {
            while (MovePieceDown()) { }
        }

        public void MergePiece()
        {
            if (CurrentPiece == null) return;
            for (int y = 0; y < CurrentPiece.Shape.GetLength(0); y++)
            {
                for (int x = 0; x < CurrentPiece.Shape.GetLength(1); x++)
                {
                    if (CurrentPiece.Shape[y, x] != 0)
                    {
                        Grid[CurrentPiece.Position.X + x, CurrentPiece.Position.Y + y] = CurrentPiece.Shape[y, x];
                    }
                }
            }
        }

        public int ClearLines()
        {
            int linesCleared = 0;
            for (int y = Grid.GetLength(1) - 1; y >= 0; y--)
            {
                bool isLineComplete = true;
                for (int x = 0; x < Grid.GetLength(0); x++)
                {
                    if (Grid[x, y] == 0)
                    {
                        isLineComplete = false;
                        break;
                    }
                }

                if (isLineComplete)
                {
                    linesCleared++;
                    for (int y2 = y; y2 > 0; y2--)
                    {
                        for (int x = 0; x < Grid.GetLength(0); x++)
                        {
                            Grid[x, y2] = Grid[x, y2 - 1];
                        }
                    }
                    y++;
                }
            }
            return linesCleared;
        }

        public bool IsGameOver()
        {
            for (int x = 0; x < Grid.GetLength(0); x++)
            {
                if (Grid[x, 0] != 0) return true;
            }
            return false;
        }

        private bool CheckCollision()
        {
            if (CurrentPiece == null) return true;
            for (int y = 0; y < CurrentPiece.Shape.GetLength(0); y++)
            {
                for (int x = 0; x < CurrentPiece.Shape.GetLength(1); x++)
                {
                    if (CurrentPiece.Shape[y, x] != 0)
                    {
                        int boardX = CurrentPiece.Position.X + x;
                        int boardY = CurrentPiece.Position.Y + y;

                        if (boardX < 0 || boardX >= Grid.GetLength(0) || boardY >= Grid.GetLength(1) || (boardY >= 0 && Grid[boardX, boardY] != 0))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }

    public class Tetromino
    {
        public int[,] Shape { get; set; } = null!;
        public Point Position { get; set; } = new Point();
        public Color Color { get; private set; }

        public Tetromino()
        {
            var random = new Random();
            int type = random.Next(1, 8);
            InitializeShape(type);
        }

        public Tetromino(int type)
        {
            InitializeShape(type);
        }

        private void InitializeShape(int type)
        {
            int[][,] shapes =
            [
                new int[,] { {1,1,1,1} },           // I - type 1
                new int[,] { {2,0,0}, {2,2,2} },    // J - type 2
                new int[,] { {0,0,3}, {3,3,3} },    // L - type 3
                new int[,] { {4,4}, {4,4} },        // O - type 4
                new int[,] { {0,5,5}, {5,5,0} },    // S - type 5
                new int[,] { {0,6,0}, {6,6,6} },    // T - type 6
                new int[,] { {7,7,0}, {0,7,7} }     // Z - type 7
            ];

            Shape = shapes[type - 1];
            Color = GetColor(type);
        }

        public static Color GetColor(int value)
        {
            return value switch
            {
                1 => Color.Cyan,
                2 => Color.Blue,
                3 => Color.Orange,
                4 => Color.Yellow,
                5 => Color.Green,
                6 => Color.Purple,
                7 => Color.Red,
                _ => Color.Black,
            };
        }

        public void Rotate()
        {
            int rows = Shape.GetLength(0);
            int cols = Shape.GetLength(1);
            int[,] rotated = new int[cols, rows];

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    rotated[x, rows - 1 - y] = Shape[y, x];
                }
            }
            Shape = rotated;
        }
    }
}
