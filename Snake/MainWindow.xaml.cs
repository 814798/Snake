using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Snake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dictionary<GridValue, ImageSource>
            gridValToImage = new()
            {
                {GridValue.Empty, Images.Empty },
                {GridValue.Snake, Images.Body },
                {GridValue.Food, Images.Food },
                {GridValue.Poison, Images.Poison },
            };

        private readonly Dictionary<Direction, int> dirtoRotation = new()
        {
            {Direction.Up, 0 },
            {Direction.Right, 90 },
            {Direction.Down, 180 },
            {Direction.Left, 270 }
        };

        private int rows = 14;
        private int cols = 14;
        private int speed = 100;
        private readonly Image[,] gridImages;
        private GameState gameState;
        private bool gameRunning;
        private Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            gridImages = SetupGrid();
            gameState = new GameState(rows, cols);
        }

        private async Task GameLoop()
        {
            while (!gameState.GameOver)
            {
                await Task.Delay(speed);
                gameState.Move();
                Draw();
            }
        }


        private Image[,] SetupGrid()
        {
            Image[,] images = new Image[rows, cols];
            GameGrid.Rows = rows;
            GameGrid.Columns = cols;
            GameGrid.Width = GameGrid.Height * (cols / (double)rows);
            for(int r=0; r <rows; r++) { 
                for(int c=0; c<cols; c++)
                {
                    Image image = new Image
                    {
                        Source = Images.Empty,
                        RenderTransformOrigin = new Point(.5, .5)
                    };

                    images[r, c] = image;
                    GameGrid.Children.Add(image);
                }
            }
            return images;
        }

        private async Task RunGame()
        {
            Draw();
            await ShowCountDown();
            Overlay.Visibility = Visibility.Hidden;
            await GameLoop();
            await ShowGameOver();
            gameState = new GameState(rows, cols);
        }

        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e) { 
            if(e.Key != Key.PageDown && e.Key != Key.PageUp)
            {
                if (Overlay.Visibility == Visibility.Visible)
                {
                    e.Handled = true;
                }
                if (!gameRunning)
                {
                    gameRunning = true;
                    await RunGame();
                    gameRunning = false;
                }
            }
        }




        public static int difficulty = 1;
        private void writeDifficulty()
        {
            switch (difficulty)
            {
                case -1:
                    ScoreText.Text = "Difficulty: Too Easy";
                    break;
                case 0:
                    ScoreText.Text = "Difficulty: Easy";
                    break;
                case 1:
                    ScoreText.Text = "Difficulty: Normal";
                    break;
                case 2:
                    ScoreText.Text = "Difficulty: Hard";
                    break;
                case 3:
                    ScoreText.Text = "Difficulty: Impossible";
                    break;

            }
        }
        private void Windows_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.PageUp:
                    if (difficulty <= 3)
                    {
                        difficulty++;
                        writeDifficulty();
                        break;
                    }
                    break;

                case Key.PageDown:
                    if (difficulty >= -1)
                    {
                        difficulty--;
                        writeDifficulty();
                        break;
                    }
                    break;

            }

            if (gameState.GameOver)
            {
                return;
            }

            switch(e.Key)
            {
                case Key.Left:
                case Key.A:
                    gameState.ChangeDirection(Direction.Left); break;
                case Key.Right:
                case Key.D:
                    gameState.ChangeDirection(Direction.Right); break;
                case Key.Up:
                case Key.W:
                    gameState.ChangeDirection(Direction.Up); break;
                case Key.Down:
                case Key.S:
                    gameState.ChangeDirection(Direction.Down); break;
                case Key.E:
                case Key.B:
                case Key.D1:
                case Key.Space:
                    if(speed > 70)
                    {
                        speed = 70; break;
                    } else
                    {
                        speed = 100; break;
                    }
                //case Key.PageUp:
                    //rows++;
                    //cols++;
                    //break;
               // case Key.PageDown:  // Wanted to add a way to increase/decrease the size of grid, but doesn't seem to work very well.
                   // rows--;
                  //  cols--;
                  //  DrawGrid();
                 //   break;
            }
        }

        private void Draw()
        {
            DrawGrid();
            DrawSnakeHead();
            ScoreText.Text = $"Score {gameState.Score}";
        }
        private void DrawGrid()
        {
            for(int r=0; r < rows; r++)
            {
                for(int c=0; c< cols; c++)
                {
                    GridValue gridVal = gameState.Grid[r, c];
                    gridImages[r, c].Source = gridValToImage[gridVal];
                    gridImages[r, c].RenderTransform = Transform.Identity;
                }
            }
        }
        private async Task ShowCountDown()
        {
            for(int i = 3; i>=1; i--)
            {
                OverlayText.Text = i.ToString();
                await Task.Delay(500);
            }
        }

        private async Task ShowGameOver()
        {
            Audio.GameOver.Play();
            await DrawDeadSnake();
            await Task.Delay(1000);
            Overlay.Visibility = Visibility.Visible;
            OverlayText.Text = "Press Any Key to Start";
        }

        private void DrawSnakeHead()
        {
            Position headPos = gameState.HeadPosition();
            Image image = gridImages[headPos.Row, headPos.Col];
            image.Source = Images.Head;

            int rotation = dirtoRotation[gameState.Dir];
            image.RenderTransform = new RotateTransform(rotation);
        }

        private async Task DrawDeadSnake()
        {
            List<Position> positions = new List<Position>(gameState.SnakePositions());
            for(int i = 0; i < positions.Count; i++)
            {
                Position pos = positions[i];
                ImageSource source = (i == 0) ? Images.DeadHead : Images.DeadBody;
                gridImages[pos.Row, pos.Col].Source = source;
                await Task.Delay(50);

            }
        }

        private async Task ShakeWindow(int durationMS)
        {
            var oLeft = this.Left;
            var oTop = this.Top;

            var shakeTimer = new DispatcherTimer();
            shakeTimer.Tick += (sender, args) =>
            {
                this.Left = oLeft + random.Next(-10, 11);
                this.Top = oTop + random.Next(-10, 11);
            };
            shakeTimer.Interval = TimeSpan.FromMilliseconds(200);
            shakeTimer.Start();

            await Task.Delay(durationMS);
        }

    }
}
