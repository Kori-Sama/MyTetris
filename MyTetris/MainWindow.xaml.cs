using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Tetris {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        //Image resource input
        private readonly ImageSource[] tileImages = new ImageSource[]
        {
            new BitmapImage(new Uri("Assets/TileEmpty.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileCyan.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileBlue.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileOrange.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileYellow.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileGreen.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TilePurple.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileRed.png", UriKind.Relative))
        };

        private readonly ImageSource[] blockImages = new ImageSource[]
        {
            new BitmapImage(new Uri("Assets/Block-Empty.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-I.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-J.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-L.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-O.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-S.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-T.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-Z.png", UriKind.Relative))
        };

        private readonly Image[,] imageControls;
        private readonly int minDelay = 75;
        private readonly int maxDelay = 1000;
        private readonly int delayDecrease = 25;
        public int bestScore = 0;

        //Make a instance of gameState
        private GameState gameState = new GameState();

        //Initialize game by constructor
        public MainWindow() {
            InitializeComponent();
            imageControls = SetupGameCanvas(gameState.GameGrid);
        }

        private Image[,] SetupGameCanvas(GameGrid grid) {
            Image[,] imageControls = new Image[grid.Rows, grid.Columns];
            int cellSize = 25;

            for (int r = 0; r < grid.Rows; r++) {
                for (int c = 0; c < grid.Columns; c++) {
                    Image imageControl = new Image {
                        Width = cellSize,
                        Height = cellSize
                    };

                    Canvas.SetTop(imageControl, (r - 2) * cellSize + 10);
                    Canvas.SetLeft(imageControl, c * cellSize);
                    GameCanvas.Children.Add(imageControl);
                    imageControls[r, c] = imageControl;
                }
            }

            return imageControls;
        }

        //Painting game grids method
        private void DrawGrid(GameGrid grid) {
            for (int r = 0; r < grid.Rows; r++) {
                for (int c = 0; c < grid.Columns; c++) {
                    int id = grid[r, c];
                    imageControls[r, c].Opacity = 1;
                    imageControls[r, c].Source = tileImages[id];
                }
            }
        }

        //Painting Blocks method
        private void DrawBlock(Block block) {
            foreach (Position p in block.TilePositions()) {
                imageControls[p.Row, p.Column].Opacity = 1;
                imageControls[p.Row, p.Column].Source = tileImages[block.Id];
            }
        }

        //Painting next block in the right
        private void DrawNextBlock(BlockQueue blockQueue) {
            Block next = blockQueue.NextBlock;
            NextImage.Source = blockImages[next.Id];
        }

        private void DrawHeldBlock(Block heldBlock) {
            if (heldBlock == null) {
                HoldImage.Source = blockImages[0];
            } else {
                HoldImage.Source = blockImages[heldBlock.Id];
            }
        }

        private void DrawGhostBlock(Block block) {
            int distance = gameState.BlockDropDistance();
            foreach (Position p in block.TilePositions()) {
                imageControls[p.Row + distance, p.Column].Opacity = 0.25;
                imageControls[p.Row + distance, p.Column].Source = tileImages[block.Id];
            }
        }


        //Method to call grid and block draw methods
        private void Draw(GameState gameState) {
            DrawGrid(gameState.GameGrid);

            DrawBlock(gameState.CurrentBlock);

            DrawGhostBlock(gameState.CurrentBlock);

            DrawHeldBlock(gameState.CurrentBlock);

            DrawNextBlock(gameState.BlockQueue);

            ScoreText.Text = $"Score: {gameState.Score}";
        }

        //Make game loop to keep game run constantly
        private async Task GameLoop() {
            Draw(gameState);

            while (!gameState.GameOver) {
                int delay = Math.Max(minDelay, maxDelay - (gameState.Score * delayDecrease));
                await Task.Delay(delay);
                gameState.MoveBlockDown();
                Draw(gameState);
            }

            gameState.ReadScore();

            if (gameState.Score > gameState.bestScore) {
                gameState.WriteScore(gameState.Score);
                gameState.bestScore = gameState.Score;
            }

            FinalScoreText.Text = $"Score: {gameState.Score}";
            HistoryBestText.Text = $"HistoryBest: {gameState.bestScore}";

            GameOverMenu.Visibility = Visibility.Visible;

        }

        //Transfer keyboard inputs to Block move
        private void Window_KeyDown(object sender, KeyEventArgs e) {
            if (gameState.GameOver) {
                return;
            }

            switch (e.Key) {
                case Key.Left:
                    gameState.MoveBlockLeft();
                    break;
                case Key.Right:
                    gameState.MoveBlockRight();
                    break;
                case Key.LeftShift:
                    gameState.MoveBlockDown();
                    break;
                case Key.Up:
                    gameState.RotateBlockCW();
                    break;
                case Key.Down:
                    gameState.RotateBlockCCW();
                    break;
                case Key.Space:
                    gameState.DropBlock();
                    break;
                default:
                    return;
            }

            Draw(gameState);
        }

        //Load Game
        private async void GameCanvas_Loaded(object sender, RoutedEventArgs e) {
            await GameLoop();
        }

        //Restart game
        private async void PlayAgain_Click(object sender, RoutedEventArgs e) {
            gameState = new GameState();
            GameOverMenu.Visibility = Visibility.Hidden;
            await GameLoop();
        }
    }
}
