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

namespace _9_11
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {



       DispatcherTimer gameTimer = new DispatcherTimer();

        double score;
        int gravity = 8;
        bool gameOver;
        Rect flappyBirdHitBox;
        
        public MainWindow()
        {
            InitializeComponent();

            gameTimer.Tick += MainEventTimer;
            gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            StartGame();
        }

        private void MainEventTimer(object sender, EventArgs e)
        {
            txtScore.Content = "Score: " + score;

            // Создаем хитбокс самолета
            flappyBirdHitBox = new Rect(Canvas.GetLeft(flappyBird) +5 , Canvas.GetTop(flappyBird) +5, 
                flappyBird.ActualWidth -10, flappyBird.ActualHeight -10);

            // Гравитация
            Canvas.SetTop(flappyBird, Canvas.GetTop(flappyBird) + gravity);

            // --- ЛОГИКА ХИТБОКСА-ЛИНИИ ---

            // Определяем длину линии (например, 70% от ширины самолета)
            double lineLength = flappyBird.ActualWidth * 0.7;

            // Находим центр самолета, чтобы линия была ровно посередине
            double offsetX = (flappyBird.ActualWidth - lineLength) / 2;
            double offsetY = flappyBird.ActualHeight / 2;

            // Создаем Rect высотой в 1 пиксель (это и есть наша линия)
            // Координаты (X, Y, Width, Height)
            Rect lineRectRaw = new Rect(offsetX, offsetY, lineLength, 1);

            // Генерируем трансформацию, которая учитывает поворот и позицию
            GeneralTransform birdTransform = flappyBird.TransformToAncestor(MyCanvas);

            // Получаем итоговый хитбокс, который наклонится вместе с самолетом
            flappyBirdHitBox = birdTransform.TransformBounds(lineRectRaw);

            // Проверка на вылет за границы экрана
            if (Canvas.GetTop(flappyBird) < -50 || Canvas.GetTop(flappyBird) > MyCanvas.ActualHeight)
            {
                EndGame();
            }


            foreach (var x in MyCanvas.Children.OfType<Image>())
            {
                string tag = (string)x.Tag;
                if (tag != null && tag.StartsWith("obs"))
                {
                    // Движение труб
                    Canvas.SetLeft(x, Canvas.GetLeft(x) - 5);

                    // Перенос трубы в начало при выходе за экран
                    if (Canvas.GetLeft(x) < -100)
                    {
                        Canvas.SetLeft(x, 850);
                        // Начисляем очки (только для одной из пары труб, например нижней)
                        if (tag.EndsWith("d")) score++;
                    }

                    // Хитбокс трубы
                    Rect pipeHitBox = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.ActualWidth, x.ActualHeight);

                    // Проверка столкновения
                    if (flappyBirdHitBox.IntersectsWith(pipeHitBox))
                    {
                        EndGame();
                        
                    }
                }
                if (tag == "cloud")
                {
                    Canvas.SetLeft(x, Canvas.GetLeft(x) - 1);
                    if (Canvas.GetLeft(x) < -250)
                    {
                        Canvas.SetLeft(x, 550);
                    }
                }
            }
        }
        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                // Поворот вверх при прыжке
                flappyBird.RenderTransform = new RotateTransform(-20, flappyBird.ActualWidth / 2, flappyBird.ActualHeight / 2);
                gravity = -8;
            }
            if (e.Key == Key.Enter && gameOver)
            {
                StartGame();
            }
        }

        private void KeyIsUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                // Поворот вниз при падении
                flappyBird.RenderTransform = new RotateTransform(5, flappyBird.ActualWidth / 2, flappyBird.ActualHeight / 2);
                gravity = 8;
            }
        }

        private void StartGame()
        {
            MyCanvas.Focus();
            int temp = 300;
            score = 0;
            gameOver = false;
            Canvas.SetTop(flappyBird, 150);
            Canvas.SetLeft(flappyBird, 50);

            foreach (var x in MyCanvas.Children.OfType<Image>())
            {
                string tag = x.Tag as string;
                if (tag == null) continue;

                // Расставляем пары колонн равномерно
                if (tag.Contains("obs1")) Canvas.SetLeft(x, 500);  
                if (tag.Contains("obs2")) Canvas.SetLeft(x, 800); 
                if (tag.Contains("obs3")) Canvas.SetLeft(x, 1100); 

                if (tag == "cloud")
                {
                    Canvas.SetLeft(x, 300); // Начальная позиция облаков
                }
            }
            gameTimer.Start();
        }

        private void EndGame()
        {
            gameOver = true;
            gameTimer.Stop();
            txtScore.Content += " Game Over!!! Press Enter";
        }
    }
}
