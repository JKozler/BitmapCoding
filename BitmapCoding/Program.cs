using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;

namespace BitmapCoding
{
    class Program
    {
        enum State
        {
            HIDING,
            FILL_WITH_ZEROS
        };
        static void Main(string[] args)
        {
            if (args.Contains("--hide"))
            {
                Bitmap bitmap = new Bitmap("guitar.png");
                Bitmap bm = CreateNonIndexedImage(bitmap);
                bm.Save("file.png");
                Console.WriteLine("Úspěšně provedeno! (uloženo jako file1.png)");
                Console.WriteLine(ExtractText(bitmap));
                Bitmap bm1 = MergeText(args[2], bm);
                bm1.Save("file1.png");
                Console.ReadKey();
            }
            else if (args.Contains("--show"))
            {
                if (File.Exists("file1.png"))
                {
                    Bitmap bm1 = new Bitmap("file1.png");
                    Console.WriteLine("Úspěšně provedeno! text z obrázku:");
                    Console.WriteLine(ExtractText(bm1));
                    Console.ReadKey();
                }
                else
                    Console.WriteLine("Soubor nebyl zašifrován do obrázku.");
            }
        }
        public static Bitmap CreateNonIndexedImage(Bitmap src)
        {
            Bitmap newBmp = new Bitmap(src.Width, src.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics gfx = Graphics.FromImage(newBmp))
            {
                gfx.DrawImage(src, 0, 0);
            }
            return newBmp;
        }
        #region Zandání textu do Bitmapy
        public static Bitmap MergeText(string text, Bitmap bmp)
        {
            State s = State.HIDING;
            int charIndex = 0;
            int charValue = 0;
            long colorUnitIndex = 0;
            int zeros = 0;
            int R = 0, G = 0, B = 0;
            //Získám výšku obrázku a s ní pracuji (na úrovni pixelů)
            for (int i = 0; i < bmp.Height; i++)
            {
                //Získám šířku obrázku a s ní pracuji (na úrovni pixelů)
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);
                    pixel = Color.FromArgb(pixel.R - pixel.R % 2, pixel.G - pixel.G % 2, pixel.B - pixel.B % 2);
                    //Nastavení RGB
                    R = pixel.R; G = pixel.G; B = pixel.B;
                    //Projedu a nastavím nově PGB
                    for (int n = 0; n < 3; n++)
                    {
                        if (colorUnitIndex % 8 == 0)
                        {
                            if (zeros == 8)
                            {
                                if ((colorUnitIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }
                                return bmp;
                            }
                            if (charIndex >= text.Length)
                            {
                                s = State.FILL_WITH_ZEROS;
                            }
                            else
                            {
                                charValue = text[charIndex++];
                            }
                        }

                        switch (colorUnitIndex % 3)
                        {
                            case 0:
                                {
                                    if (s == State.HIDING)
                                    {
                                        R += charValue % 2;

                                        charValue /= 2;
                                    }
                                }
                                break;
                            case 1:
                                {
                                    if (s == State.HIDING)
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                }
                                break;
                            case 2:
                                {
                                    if (s == State.HIDING)
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }
                                break;
                        }

                        colorUnitIndex++;

                        if (s == State.FILL_WITH_ZEROS)
                        {
                            zeros++;
                        }
                    }
                }
            }
            return bmp;
        }
        #endregion
        #region Získání textu
        public static string ExtractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;
            string extractedText = String.Empty;
            //To samé jako nahoře, akorát opačně
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);

                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2;
                                }
                                break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                }
                                break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                }
                                break;
                        }
                        colorUnitIndex++;
                        if (colorUnitIndex % 8 == 0)
                        {
                            charValue = reverseBits(charValue);
                            if (charValue == 0)
                            {
                                return extractedText;
                            }
                            char c = (char)charValue;
                            extractedText += c.ToString();
                        }
                    }
                }
            }

            return extractedText;
        }
        #endregion
        public static int reverseBits(int n)
        {
            int result = 0;
            for (int i = 0; i < 8; i++)
            {
                result = result * 2 + n % 2;
                n /= 2;
            }
            return result;
        }
    }
}
