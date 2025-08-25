using Timer = System.Windows.Forms.Timer;

namespace TetrisGame
{
    public partial class MainForm : Form
    {
        private const int BoardWidth = 10;
        private const int BoardHeight = 20;
        private const int BlockSize = 25;

        // 預覽區域設定
        private const int PreviewSize = 20; // 預覽方塊大小
        private const int PreviewAreaWidth = 6 * PreviewSize; // 預覽區域寬度
        private const int PreviewAreaHeight = 6 * PreviewSize; // 預覽區域高度

        private Board gameBoard = null!;
        private Timer gameTimer = null!;
        private bool isGameOver;
        private bool isPaused = false; // 用於跟踪遊戲是否被暫停
        private int score;

        public MainForm()
        {
            InitializeComponent();
            InitializeGame();
            SetupTimer();
            this.DoubleBuffered = true;
            this.Paint += MainForm_Paint;
            this.KeyDown += MainForm_KeyDown;
        }

        private void InitializeGame()
        {
            gameBoard = new Board(BoardWidth, BoardHeight);
            isGameOver = false;
            score = 0;
            gameBoard.SpawnNewPiece();
        }

        private void SetupTimer()
        {
            gameTimer = new Timer
            {
                Interval = 1000 // 初始速度
            };
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            if (isGameOver) return;

            if (!gameBoard.MovePieceDown())
            {
                gameBoard.MergePiece();
                int linesCleared = gameBoard.ClearLines();
                score += linesCleared * 100;

                if (gameBoard.IsGameOver())
                {
                    isGameOver = true;
                    isPaused = false;
                    gameTimer.Stop();
                }
                else
                {
                    gameBoard.SpawnNewPiece();
                }
            }
            Invalidate();
        }

        private void MainForm_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // 先繪製背景網格（只繪製一次）
            DrawGrid(g);

            DrawPause(g);

            // 繪製已放置的方塊
            DrawBoard(g);

            // 繪製當前下落的方塊
            DrawCurrentPiece(g);

            // 繪製預覽區域
            DrawNextPiecePreview(g);

            // 繪製分數
            DrawScore(g);

            if (isGameOver)
            {
                DrawGameOver(g);
            }
        }

        private void DrawNextPiecePreview(Graphics g)
        {
            // 預覽區域位置（遊戲區域右側）
            int previewX = (BoardWidth + 2) * BlockSize + 20;
            int previewY = 80;

            // 繪製預覽區域背景
            using (SolidBrush backgroundBrush = new(Color.FromArgb(30, 30, 30)))
            {
                g.FillRectangle(backgroundBrush, previewX, previewY, PreviewAreaWidth, PreviewAreaHeight);
            }

            // 繪製預覽區域邊框
            using (Pen borderPen = new(Color.Gray, 2))
            {
                g.DrawRectangle(borderPen, previewX, previewY, PreviewAreaWidth, PreviewAreaHeight);
            }

            // 繪製 "NEXT" 標題
            using (Font titleFont = new("Arial", 14, FontStyle.Bold))
            {
                g.DrawString("NEXT", titleFont, Brushes.White,
                    new PointF(previewX + 10, previewY - 25));
            }

            // 繪製下一個方塊
            if (gameBoard.NextPiece != null)
            {
                DrawPreviewPiece(g, gameBoard.NextPiece, previewX, previewY);
            }
        }

        private static void DrawPreviewPiece(Graphics g, Tetromino piece, int offsetX, int offsetY)
        {
            // 計算方塊在預覽區域的居中位置
            int pieceWidth = piece.Shape.GetLength(1) * PreviewSize;
            int pieceHeight = piece.Shape.GetLength(0) * PreviewSize;

            int startX = offsetX + (PreviewAreaWidth - pieceWidth) / 2;
            int startY = offsetY + (PreviewAreaHeight - pieceHeight) / 2;

            // 繪製方塊的每個部分
            for (int y = 0; y < piece.Shape.GetLength(0); y++)
            {
                for (int x = 0; x < piece.Shape.GetLength(1); x++)
                {
                    if (piece.Shape[y, x] != 0)
                    {
                        DrawPreviewBlock(g,
                            startX + (x * PreviewSize),
                            startY + (y * PreviewSize),
                            piece.Color);
                    }
                }
            }
        }

        private static void DrawPreviewBlock(Graphics g, int x, int y, Color color)
        {
            // 填充方塊顏色
            using (SolidBrush brush = new(color))
            {
                g.FillRectangle(brush, x, y, PreviewSize, PreviewSize);
            }

            // 繪製方塊邊框讓方塊更明顯
            using Pen borderPen = new(Color.Black, 1);
            g.DrawRectangle(borderPen, x, y, PreviewSize, PreviewSize);
        }

        private static void DrawGrid(Graphics g)
        {
            using Pen dashPen = new(Color.Gray, 1) { DashPattern = [4, 2] };
            // 繪製垂直線
            for (int x = 1; x <= BoardWidth + 1; x++)
            {
                g.DrawLine(dashPen,
                    x * BlockSize,
                    BlockSize,
                    x * BlockSize,
                    (BoardHeight + 1) * BlockSize);
            }

            // 繪製水平線
            for (int y = 1; y <= BoardHeight + 1; y++)
            {
                g.DrawLine(dashPen,
                    BlockSize,
                    y * BlockSize,
                    (BoardWidth + 1) * BlockSize,
                    y * BlockSize);
            }
        }

        private void DrawBoard(Graphics g)
        {
            for (int y = 0; y < BoardHeight; y++)
            {
                for (int x = 0; x < BoardWidth; x++)
                {
                    if (gameBoard.Grid[x, y] > 0)
                    {
                        DrawBlock(g, x, y, Tetromino.GetColor(gameBoard.Grid[x, y]));
                    }
                }
            }
        }

        private void DrawCurrentPiece(Graphics g)
        {
            Tetromino current = gameBoard.CurrentPiece;
            for (int y = 0; y < current.Shape.GetLength(0); y++)
            {
                for (int x = 0; x < current.Shape.GetLength(1); x++)
                {
                    if (current.Shape[y, x] != 0)
                    {
                        DrawBlock(g,
                            current.Position.X + x,
                            current.Position.Y + y,
                            current.Color);
                    }
                }
            }
        }

        // 只繪製單個方塊
        private static void DrawBlock(Graphics g, int x, int y, Color color)
        {
            int pixelX = (x * BlockSize) + BlockSize;
            int pixelY = (y * BlockSize) + BlockSize;

            // 填充方塊顏色
            using (SolidBrush brush = new(color))
            {
                g.FillRectangle(brush, pixelX, pixelY, BlockSize, BlockSize);
            }

            // 繪製方塊邊框讓方塊更明顯
            using Pen borderPen = new(Color.Black, 1);
            g.DrawRectangle(borderPen, pixelX, pixelY, BlockSize, BlockSize);
        }

        private void DrawScore(Graphics g)
        {
            g.DrawString($"SCORE : {score}",
                new Font("Arial", 12, FontStyle.Bold),
                Brushes.White,
                new PointF(BoardWidth * BlockSize + 60, 30));
        }

        private static void DrawGameOver(Graphics g)
        {
            g.DrawString("GAME OVER !",
                new Font("Arial", 20, FontStyle.Bold),
                Brushes.White,
                new PointF((BoardWidth * BlockSize / 2) - 70 - 1, (BoardHeight * BlockSize / 2) - 1));
            g.DrawString("GAME OVER !",
                new Font("Arial", 20, FontStyle.Bold),
                Brushes.White,
                new PointF((BoardWidth * BlockSize / 2) - 70 + 1, (BoardHeight * BlockSize / 2) + 1));
            g.DrawString("GAME OVER !",
                new Font("Arial", 20, FontStyle.Bold),
                Brushes.Red,
                new PointF(BoardWidth * BlockSize / 2 - 70, BoardHeight * BlockSize / 2));

            // 重新開始提示
            string restartText = "Press F5 to Restart";
            using Font restartFont = new Font("Arial", 14, FontStyle.Bold);
            // 計算提示文字位置（居中）
            float restartX = (BoardWidth * BlockSize / 2) - (g.MeasureString(restartText, restartFont).Width / 2) + 25;
            float restartY = (BoardHeight * BlockSize / 2) + 40;

            // 繪製文字陰影
            g.DrawString(restartText, restartFont, Brushes.White, restartX + 1, restartY + 1);
            // 繪製主文字
            g.DrawString(restartText, restartFont, Brushes.Yellow, restartX, restartY);
        }

        private void DrawPause(Graphics g)
        {
            if (isPaused)
            {
                gameTimer.Start(); // 恢復 Timer
            }
            else
            {
                gameTimer.Stop(); // 暫停 Timer
                // 暫停提示
                string restartText = "Press Pause to Continue";
                using Font restartFont = new Font("Arial", 14, FontStyle.Bold);
                // 計算提示文字位置（居中）
                float restartX = (BoardWidth * BlockSize / 2) - (g.MeasureString(restartText, restartFont).Width / 2) + 25;
                float restartY = (BoardHeight * BlockSize / 2) + 40;

                // 繪製文字陰影
                g.DrawString(restartText, restartFont, Brushes.White, restartX + 1, restartY + 1);
                // 繪製主文字
                g.DrawString(restartText, restartFont, Brushes.Yellow, restartX, restartY);
            }
        }
        private void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (isGameOver)
            {
                switch (e.KeyCode)
                {
                    case Keys.F5:
                        InitializeGame();
                        gameTimer.Start();
                        isGameOver = false;
                        score = 0;
                        break;
                    default:
                        return;
                }
            }
            switch (e.KeyCode)
            {
                case Keys.F5:
                    InitializeGame();
                    gameTimer.Start();
                    isGameOver = false;
                    score = 0;
                    break;
                case Keys.A:
                case Keys.Left:
                    gameBoard.MovePieceLeft();
                    break;
                case Keys.D: 
                case Keys.Right:
                    gameBoard.MovePieceRight();
                    break;
                case Keys.S: 
                case Keys.Down:
                    gameBoard.MovePieceDown();
                    break;
                case Keys.E:
                case Keys.W:
                case Keys.Up:
                    gameBoard.LeftRotatePiece();
                    break;
                case Keys.Q:
                    gameBoard.RightRotatePiece();
                    break;
                case Keys.Space:
                    gameBoard.DropPiece();
                    break;
                case Keys.Pause:
                    isPaused = !isPaused;
                    break;
            }
            Invalidate();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Text = "Tetris Game";
            this.Width = (BoardWidth + 2) * BlockSize + 200;
            this.Height = (BoardHeight + 2) * BlockSize + 40;
            this.MaximizeBox = false;
            this.BackColor = Color.Black;
        }
    }

    public class Board(int width, int height)
    {
        public int[,] Grid { get; private set; } = new int[width, height];
        public Tetromino? CurrentPiece { get; private set; }
        public Tetromino NextPiece { get; private set; } = new Tetromino();
        private readonly Random random = new();
        private readonly Queue<int> pieceBag = new();

        private void FillBag()
        {
            // 創建包含 1-7 的數組（代表7種不同方塊）
            int[] pieces = [1, 2, 3, 4, 5, 6, 7];

            // 使用 Fisher-Yates 洗牌算法
            for (int i = pieces.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (pieces[i], pieces[j]) = (pieces[j], pieces[i]);
            }

            // 將洗牌後的方塊加入隊列
            foreach (int piece in pieces)
            {
                pieceBag.Enqueue(piece);
            }
        }

        private int GetNextPieceType()
        {
            // 如果袋子空了，重新填充
            if (pieceBag.Count == 0)
            {
                FillBag();
            }

            return pieceBag.Dequeue();
        }

        public void SpawnNewPiece()
        {
            // 將下一個方塊設為當前方塊
            CurrentPiece = NextPiece;
            CurrentPiece.Position = new Point(3, 0);

            // 產生新的下一個方塊，使用 7-bag 系統
            int nextType = GetNextPieceType();
            NextPiece = new Tetromino(nextType);
        }

        public bool MovePieceDown()
        {
            if (CurrentPiece == null) return false;

            CurrentPiece.Position = new Point(
                CurrentPiece.Position.X,
                CurrentPiece.Position.Y + 1);

            if (CheckCollision())
            {
                CurrentPiece.Position = new Point(
                    CurrentPiece.Position.X,
                    CurrentPiece.Position.Y - 1);
                return false;
            }
            return true;
        }

        public void MovePieceLeft()
        {
            if (CurrentPiece == null) return;

            CurrentPiece.Position = new Point(
                CurrentPiece.Position.X - 1,
                CurrentPiece.Position.Y);

            if (CheckCollision())
            {
                CurrentPiece.Position = new Point(
                    CurrentPiece.Position.X + 1,
                    CurrentPiece.Position.Y);
            }
        }

        public void MovePieceRight()
        {
            if (CurrentPiece == null) return;

            CurrentPiece.Position = new Point(
                CurrentPiece.Position.X + 1,
                CurrentPiece.Position.Y);

            if (CheckCollision())
            {
                CurrentPiece.Position = new Point(
                    CurrentPiece.Position.X - 1,
                    CurrentPiece.Position.Y);
            }
        }

        public void RightRotatePiece()
        {
            int[,] original = CurrentPiece.Shape;
            Point originalPosition = CurrentPiece.Position;

            CurrentPiece.Rotate();

            // 如果旋轉後有碰撞，嘗試踢牆
            if (CheckCollision())
            {
                // 嘗試向左移動
                CurrentPiece.Position = new Point(originalPosition.X - 1, originalPosition.Y);
                if (!CheckCollision()) return; // 成功踢牆

                // 嘗試向右移動
                CurrentPiece.Position = new Point(originalPosition.X + 1, originalPosition.Y);
                if (!CheckCollision()) return; // 成功踢牆

                // 嘗試向左移動兩格（適用於 I 型方塊）
                CurrentPiece.Position = new Point(originalPosition.X - 2, originalPosition.Y);
                if (!CheckCollision()) return; // 成功踢牆

                // 嘗試向右移動兩格（適用於 I 型方塊）
                CurrentPiece.Position = new Point(originalPosition.X + 2, originalPosition.Y);
                if (!CheckCollision()) return; // 成功踢牆

                // 嘗試向上移動
                CurrentPiece.Position = new Point(originalPosition.X, originalPosition.Y - 1);
                if (!CheckCollision()) return; // 成功踢牆

                // 所有踢牆嘗試都失敗，恢復原狀
                CurrentPiece.Shape = original;
                CurrentPiece.Position = originalPosition;
            }
        }

        public void LeftRotatePiece()
        {
            int[,] original = CurrentPiece.Shape;
            Point originalPosition = CurrentPiece.Position;

            // 左旋轉 = 右旋轉三次
            CurrentPiece.Rotate();
            CurrentPiece.Rotate();
            CurrentPiece.Rotate();

            // 如果旋轉後有碰撞，嘗試踢牆
            if (CheckCollision())
            {
                // 嘗試向左移動
                CurrentPiece.Position = new Point(originalPosition.X - 1, originalPosition.Y);
                if (!CheckCollision()) return; // 成功踢牆

                // 嘗試向右移動
                CurrentPiece.Position = new Point(originalPosition.X + 1, originalPosition.Y);
                if (!CheckCollision()) return; // 成功踢牆

                // 嘗試向左移動兩格（適用於 I 型方塊）
                CurrentPiece.Position = new Point(originalPosition.X - 2, originalPosition.Y);
                if (!CheckCollision()) return; // 成功踢牆

                // 嘗試向右移動兩格（適用於 I 型方塊）
                CurrentPiece.Position = new Point(originalPosition.X + 2, originalPosition.Y);
                if (!CheckCollision()) return; // 成功踢牆

                // 嘗試向上移動
                CurrentPiece.Position = new Point(originalPosition.X, originalPosition.Y - 1);
                if (!CheckCollision()) return; // 成功踢牆

                // 所有踢牆嘗試都失敗，恢復原狀
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
                        Grid[
                            CurrentPiece.Position.X + x,
                            CurrentPiece.Position.Y + y] = CurrentPiece.Shape[y, x];
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
                    y++; // 重新檢查當前位置
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

                        if (boardX < 0 ||
                            boardX >= Grid.GetLength(0) ||
                            boardY >= Grid.GetLength(1) ||
                            (boardY >= 0 && Grid[boardX, boardY] != 0))
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

        // 原有的隨機建構函數（保持向後兼容）
        public Tetromino()
        {
            var random = new Random();
            int type = random.Next(1, 8);
            InitializeShape(type);
        }

        // 新的指定類型建構函數
        public Tetromino(int type)
        {
            InitializeShape(type);
        }

        private void InitializeShape(int type)
        {
            // 七種方塊形狀
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
                1 => Color.Cyan,    // I
                2 => Color.Blue,    // J
                3 => Color.Orange,  // L
                4 => Color.Yellow,  // O
                5 => Color.Green,   // S
                6 => Color.Purple,  // T
                7 => Color.Red,     // Z
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