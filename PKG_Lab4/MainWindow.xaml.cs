using System.Diagnostics;
using System.Text;
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
using Xceed.Wpf.Toolkit;

namespace PKG_Lab4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///
    public partial class MainWindow : Window
    {
        private double scale = 20; // Начальный масштаб
        private const int CanvasWidth = 96 * 15;
        private const int CanvasHeight = 1080;
        private const int OriginX = CanvasWidth / 2; // Центр по X
        private const int OriginY = CanvasHeight / 2; // Центр по Y
        private DispatcherTimer drawTimer;

        private Color selectedColor = Colors.Black; // Цвет по умолчанию
        private readonly Color gridColor = Colors.Gray; // Цвет сетки

        private void ColorPicker_SelectedColorChanged(object sender, RoutedEventArgs e)
        {
            if (colorPicker.SelectedColor.HasValue)
            {
                selectedColor = colorPicker.SelectedColor.Value;
            }
        }

        public MainWindow()
        {
            this.MinHeight = 650;
            this.MinWidth = 1150;
            InitializeComponent();
            DrawGrid(); // Рисуем сетку при инициализации
        }

        private void DrawGrid()
        {
            DrawingCanvas.Children.Clear(); // Очищаем канвас перед рисованием
            var gridPen = new Pen(new SolidColorBrush(gridColor), 1); // Используем фиксированный цвет для сетки
            var axisPen = new Pen(Brushes.Black, 4);
            var textBrush = Brushes.Black;

            // Вертикальные линии
            for (int i = -CanvasWidth / (2 * (int)scale); i <= CanvasWidth / (2 * (int)scale); i++)
            {
                DrawLine(OriginX + i * scale, 0, OriginX + i * scale, CanvasHeight, gridPen);

                // Отображаем значения через каждые 5 клеток
                if (i % 5 == 0)
                {
                    var textBlock = new TextBlock
                    {
                        Text = i.ToString(),
                        Foreground = textBrush,
                        FontSize = 12
                    };
                    Canvas.SetLeft(textBlock, OriginX + i * scale + 3);
                    Canvas.SetTop(textBlock, OriginY + 5);
                    DrawingCanvas.Children.Add(textBlock);
                }
            }

            // Горизонтальные линии
            for (int i = -CanvasHeight / (2 * (int)scale); i <= CanvasHeight / (2 * (int)scale); i++)
            {
                DrawLine(0, OriginY - i * scale, CanvasWidth, OriginY - i * scale, gridPen);

                // Отображаем значения через каждые 5 клеток
                if (i % 5 == 0)
                {
                    var textBlock = new TextBlock
                    {
                        Text = (-i).ToString(),
                        Foreground = textBrush,
                        FontSize = 12
                    };
                    Canvas.SetLeft(textBlock, OriginX + 5);
                    Canvas.SetTop(textBlock, OriginY - i * scale - 15);
                    DrawingCanvas.Children.Add(textBlock);
                }
            }

            // Оси координат
            DrawLine(0, OriginY, CanvasWidth, OriginY, axisPen);
            DrawLine(OriginX, 0, OriginX, CanvasHeight, axisPen);
        }

        private void DrawLine(double x1, double y1, double x2, double y2, Pen pen)
        {
            DrawingCanvas.Children.Add(new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = new SolidColorBrush(gridColor), // Используем выбранный цвет для линий растеризации
                StrokeThickness = pen.Thickness
            });
        }

        private void DrawCell(double x, double y)
        {
            Rectangle cell = new Rectangle
            {
                Fill = new SolidColorBrush(selectedColor), // Используем выбранный цвет
                Width = scale,
                Height = scale
            };
            Canvas.SetLeft(cell, OriginX + x * scale);
            Canvas.SetTop(cell, OriginY - y * scale);
            DrawingCanvas.Children.Add(cell);
        }

        private bool TryGetCoordinates(out double x0, out double y0, out double x1, out double y1)
        {
            if (double.TryParse(txtX0.Text, out x0) &&
                double.TryParse(txtY0.Text, out y0) &&
                double.TryParse(txtX1.Text, out x1) &&
                double.TryParse(txtY1.Text, out y1))
            {
                return true;
            }
            System.Windows.MessageBox.Show("Пожалуйста, введите корректные координаты.");
            x0 = y0 = x1 = y1 = 0;
            return false;
        }

        private bool TryGetCircleParameters(out double centerX, out double centerY, out double radius)
        {
            if (double.TryParse(txtX0.Text, out centerX) &&
                double.TryParse(txtY0.Text, out centerY) &&
                double.TryParse(txtX1.Text, out radius))
            {
                return true;
            }
            System.Windows.MessageBox.Show("Пожалуйста, введите корректные параметры окружности.");
            centerX = centerY = radius = 0;
            return false;
        }

        private void Bresenham_Click(object sender, RoutedEventArgs e)
        {
            if (TryGetCoordinates(out double x0, out double y0, out double x1, out double y1))
            {
                var stopwatch = Stopwatch.StartNew();
                Bresenham(x0, y0, x1, y1);
                stopwatch.Stop();
                System.Windows.MessageBox.Show($"Bresenham's Line Time: {stopwatch.ElapsedMilliseconds} ms");
            }
        }

        private void Bresenham(double x0, double y0, double x1, double y1)
        {
            int dx = (int)Math.Abs(x1 - x0);
            int dy = (int)Math.Abs(y1 - y0);
            int sx = (x0 < x1) ? 1 : -1;
            int sy = (y0 < y1) ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                DrawCell(x0, y0);
                if (x0 == x1 && y0 == y1) break;
                int err2 = err * 2;
                if (err2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (err2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        private void BresenhamCircle_Click(object sender, RoutedEventArgs e)
        {
            if (TryGetCircleParameters(out double centerX, out double centerY, out double radius))
            {
                var stopwatch = Stopwatch.StartNew();
                BresenhamCircle(centerX, centerY, radius);
                stopwatch.Stop();
                System.Windows.MessageBox.Show($"Bresenham's Circle Time: {stopwatch.ElapsedMilliseconds} ms");
            }
        }

        private void BresenhamCircle(double centerX, double centerY, double radius)
        {
            int x = 0;
            int y = (int)radius;
            int d = 3 - 2 * (int)radius;

            while (x <= y)
            {
                DrawCirclePixels(centerX, centerY, x, y);
                if (d < 0)
                    d = d + 4 * x + 6;
                else
                {
                    d = d + 4 * (x - y) + 10;
                    y--;
                }
                x++;
            }
        }

        private void DrawCirclePixels(double centerX, double centerY, int x, int y)
        {
            DrawCell(centerX + x, centerY + y);
            DrawCell(centerX - x, centerY + y);
            DrawCell(centerX + x, centerY - y);
            DrawCell(centerX - x, centerY - y);
            DrawCell(centerX + y, centerY + x);
            DrawCell(centerX - y, centerY + x);
            DrawCell(centerX + y, centerY - x);
            DrawCell(centerX - y, centerY - x);
        }

        private void CDA_Click(object sender, RoutedEventArgs e)
        {
            if (TryGetCoordinates(out double x0, out double y0, out double x1, out double y1))
            {
                var stopwatch = Stopwatch.StartNew();
                DDA(x0, y0, x1, y1);
                stopwatch.Stop();
                System.Windows.MessageBox.Show($"DDA Line Time: {stopwatch.ElapsedMilliseconds} ms");
            }
        }

        private void DDA(double x0, double y0, double x1, double y1)
        {
            double dx = x1 - x0;
            double dy = y1 - y0;
            double steps = Math.Max(Math.Abs(dx), Math.Abs(dy));
            dx /= steps;
            dy /= steps;

            double x = x0;
            double y = y0;
            for (int i = 0; i <= steps; i++)
            {
                DrawCell(x, y);
                x += dx;
                y += dy;
            }
        }

        private void SmoothLines_Click(object sender, RoutedEventArgs e)
        {
            if (TryGetCoordinates(out double x0, out double y0, out double x1, out double y1))
            {
                var stopwatch = Stopwatch.StartNew();
                SmoothLine(x0, y0, x1, y1);
                stopwatch.Stop();
                System.Windows.MessageBox.Show($"Smooth Line Time: {stopwatch.ElapsedMilliseconds} ms");
            }
        }

        private void SmoothLine(double x0, double y0, double x1, double y1)
        {
            // Рассчитываем параметры для линий
            int dx = (int)Math.Abs(x1 - x0);
            int dy = (int)Math.Abs(y1 - y0);
            int sx = (x0 < x1) ? 1 : -1;
            int sy = (y0 < y1) ? 1 : -1;
            int err = dx - dy;

            // Основной цикл
            while (true)
            {
                DrawCell(x0, y0); // Рисуем текущую точку

                // Если достигли конца, выходим из цикла
                if (x0 == x1 && y0 == y1) break;

                // Алгоритм сглаживания
                int err2 = err * 2;
                if (err2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (err2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }

                // Добавляем дополнительные точки для сглаживания
                if (sx > 0 && dy > 0)
                {
                    DrawCell(x0, y0 + sy);
                }
                else if (sx < 0 && dy < 0)
                {
                    DrawCell(x0, y0 - sy);
                }
            }
        }

        private void Castle_Click(object sender, RoutedEventArgs e)
        {
            if (TryGetCoordinates(out double x0, out double y0, out double x1, out double y1))
            {
                var stopwatch = Stopwatch.StartNew();
                CastlePitway(x0, y0, x1, y1);
                stopwatch.Stop();
                System.Windows.MessageBox.Show($"Castle Pitway Line Time: {stopwatch.ElapsedMilliseconds} ms");
            }
        }

        private void CastlePitway(double x0, double y0, double x1, double y1)
        {
            int dx = (int)Math.Abs(x1 - x0);
            int dy = (int)Math.Abs(y1 - y0);
            int sx = (x0 < x1) ? 1 : -1;
            int sy = (y0 < y1) ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                DrawCell(x0, y0); // Рисуем текущую точку

                if (x0 == x1 && y0 == y1) break; // Если достигли конечной точки

                int err2 = err * 2;

                // Используем более сложную логику для сглаживания
                if (err2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }

                if (err2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }

                // Добавляем дополнительный эффект сглаживания
                if ((sx > 0 && err2 > -dy && err2 < 0) || (sx < 0 && err2 < -dy && err2 > 0))
                {
                    DrawCell(x0, y0 + sy); // Рисуем дополнительную точку для сглаживания
                }
            }
        }

        private void MidpointCircle_Click(object sender, RoutedEventArgs e)
        {
            if (TryGetCircleParameters(out double centerX, out double centerY, out double radius))
            {
                var stopwatch = Stopwatch.StartNew();
                MidpointCircle(centerX, centerY, radius);
                stopwatch.Stop();
                System.Windows.MessageBox.Show($"Midpoint Circle Time: {stopwatch.ElapsedMilliseconds} ms");
            }
        }

        private void MidpointCircle(double centerX, double centerY, double radius)
        {
            int x = 0;
            int y = (int)radius;
            int d = 1 - (int)radius;

            while (x <= y)
            {
                DrawCirclePixels(centerX, centerY, x, y);
                if (d < 0)
                    d = d + 2 * x + 3;
                else
                {
                    d = d + 2 * (x - y) + 5;
                    y--;
                }
                x++;
            }
        }

        private void StepwiseRasterization(double x0, double y0, double x1, double y1)
        {
            int dx = (int)Math.Abs(x1 - x0);
            int dy = (int)Math.Abs(y1 - y0);
            int sx = (x0 < x1) ? 1 : -1;
            int sy = (y0 < y1) ? 1 : -1;
            int err = dx - dy;

            List<(double x, double y)> pointsToDraw = new List<(double, double)>();

            while (true)
            {
                pointsToDraw.Add((x0, y0)); // Сохраняем текущую точку
                if (x0 == x1 && y0 == y1) break;
                int err2 = err * 2;
                if (err2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (err2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }

            // Запускаем отрисовку с задержкой
            StartStepwiseDrawing(pointsToDraw);
        }

        private void StartStepwiseDrawing(List<(double x, double y)> points)
        {
            drawTimer = new DispatcherTimer();
            drawTimer.Interval = TimeSpan.FromMilliseconds(500); // Настройте задержку по необходимости
            drawTimer.Tick += (sender, e) =>
            {
                if (points.Count > 0)
                {
                    var (x, y) = points[0];
                    DrawCell(x, y); // Рисуем текущую клетку
                    points.RemoveAt(0); // Убираем отрисованную точку
                }
                else
                {
                    drawTimer.Stop(); // Останавливаем таймер, если все точки отрисованы
                }
            };

            drawTimer.Start(); // Запускаем таймер
        }

        private void StepwiseButton_Click(object sender, RoutedEventArgs e)
        {
            if (TryGetCoordinates(out double x0, out double y0, out double x1, out double y1))
            {
                //var stopwatch = Stopwatch.StartNew();
                StepwiseRasterization(x0, y0, x1, y1);
                //stopwatch.Stop();
                //MessageBox.Show($"Stepwise Rasterization Time: {stopwatch.ElapsedMilliseconds} ms");
            }
        }

        private void ScaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            scale = e.NewValue; // Обновляем масштаб
            if (scaleValueText != null)
            scaleValueText.Text = $"Scale: {scale}"; // Обновляем текстовое поле масштаба
            DrawGrid(); // Перерисовываем сетку
        }

        private void ClearCanvas_Click(object sender, RoutedEventArgs e)
        {
            DrawingCanvas.Children.Clear(); // Очищаем канвас
            DrawGrid(); // Перерисовываем сетку
        }
    }
}