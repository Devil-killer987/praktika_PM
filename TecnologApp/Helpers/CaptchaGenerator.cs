using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace TecnologApp.Helpers
{
    public class CaptchaGenerator
    {
        private static readonly Random _random = new Random();

        public static CaptchaResult GenerateCaptcha(int width = 250, int height = 80)
        {
            // Генерация случайного текста (6 символов)
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ0123456789";
            string captchaText = "";
            for (int i = 0; i < 6; i++)
            {
                captchaText += chars[_random.Next(chars.Length)];
            }

            // Создание изображения
            using (Bitmap bitmap = new Bitmap(width, height))
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.WhiteSmoke);

                // Рисуем шум (случайные линии)
                for (int i = 0; i < 15; i++)
                {
                    using (Pen pen = new Pen(Color.FromArgb(_random.Next(150, 200),
                        Color.Gray), 1))
                    {
                        int x1 = _random.Next(width);
                        int y1 = _random.Next(height);
                        int x2 = _random.Next(width);
                        int y2 = _random.Next(height);
                        g.DrawLine(pen, x1, y1, x2, y2);
                    }
                }

                // Рисуем шум (случайные точки)
                for (int i = 0; i < 200; i++)
                {
                    int x = _random.Next(width);
                    int y = _random.Next(height);
                    bitmap.SetPixel(x, y, Color.FromArgb(_random.Next(100, 200),
                        Color.DarkGray));
                }

                // Рисуем текст
                using (Font font = new Font("Arial", 24, FontStyle.Bold | FontStyle.Italic))
                {
                    // Искривление текста
                    for (int i = 0; i < captchaText.Length; i++)
                    {
                        float angle = _random.Next(-15, 15);
                        using (Matrix matrix = new Matrix())
                        {
                            matrix.RotateAt(angle, new PointF(35 + i * 30, height / 2));
                            g.Transform = matrix;
                            using (Brush brush = new SolidBrush(Color.FromArgb(
                                _random.Next(50, 150),
                                _random.Next(50, 150),
                                _random.Next(50, 150))))
                            {
                                g.DrawString(captchaText[i].ToString(), font, brush,
                                    30 + i * 30, height / 2 - 15);
                            }
                            g.ResetTransform();
                        }
                    }
                }

                // Конвертация в массив байт
                using (MemoryStream ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Png);
                    byte[] imageBytes = ms.ToArray();
                    string base64String = Convert.ToBase64String(imageBytes);

                    return new CaptchaResult
                    {
                        CaptchaText = captchaText,
                        CaptchaImageBase64 = base64String
                    };
                }
            }
        }
    }

    public class CaptchaResult
    {
        public string CaptchaText { get; set; }
        public string CaptchaImageBase64 { get; set; }
    }
}
