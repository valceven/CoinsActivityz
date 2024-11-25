using ImageProcess2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace CoinsActivityz
{
    public partial class Form1 : Form
    {
        Bitmap originalImage, processedImage;
        int totalCoinValue = 0;
        List<List<Point>> coinClusters;
        bool[,] visitedPixels;
        int fivePesoCount, onePesoCount, twentyFiveCentsCount, tenCentsCount, fiveCentsCount;

        public Form1()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (originalImage == null)
                return;

            processedImage = new Bitmap(originalImage);
            int thresholdValue = 200;

            int imageHeight = processedImage.Height;
            int imageWidth = processedImage.Width;

            BitmapData bitmapData = processedImage.LockBits(
                new Rectangle(0, 0, imageWidth, imageHeight),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb
            );

            unsafe
            {
                int padding = bitmapData.Stride - imageWidth * 3;
                byte* pixelData = (byte*)bitmapData.Scan0;

                for (int i = 0; i < processedImage.Height; i++, pixelData += padding)
                {
                    for (int j = 0; j < processedImage.Width; j++, pixelData += 3)
                    {
                        byte colorValue = pixelData[0] < thresholdValue ? (byte)0 : (byte)255;
                        pixelData[0] = pixelData[1] = pixelData[2] = colorValue;
                    }
                }
            }

            processedImage.UnlockBits(bitmapData);
            pictureBox2.Image = processedImage;
            CountCoins(processedImage);
            label1.Text = "Total Value: " + totalCoinValue.ToString();
        }

        private void CountCoins(Bitmap image)
        {
            coinClusters = new List<List<Point>>();
            visitedPixels = new bool[processedImage.Width, processedImage.Height];

            int coinClusterCount = 0;
            totalCoinValue = 0; 

            fivePesoCount = onePesoCount = twentyFiveCentsCount = tenCentsCount = fiveCentsCount = 0;

            for (int x = 0; x < processedImage.Width; x++)
            {
                for (int y = 0; y < processedImage.Height; y++)
                {
                    Color pixelColor = processedImage.GetPixel(x, y);

                    if (pixelColor.R == 0 && !visitedPixels[x, y])
                    {
                        List<Point> coinCluster;
                        int coinSize;

                        (coinCluster, coinSize) = GetCoinCluster(x, y);

                        if (coinSize < 20)
                        {
                            continue;
                        }

                        coinClusters.Add(coinCluster);
                        coinClusterCount++;
                        int coinValue = DetermineCoinValue(coinSize);
                        totalCoinValue += coinValue;
                    }
                }
            }
        }

        private (List<Point>, int) GetCoinCluster(int x, int y)
        {
            List<Point> coinClusterPoints = new List<Point>();
            Bitmap imageClone = (Bitmap)processedImage.Clone();

            int coinClusterSize = 0;
            int imageWidth = imageClone.Width;
            int imageHeight = imageClone.Height;

            Queue<Point> pixelQueue = new Queue<Point>();
            pixelQueue.Enqueue(new Point(x, y));

            while (pixelQueue.Count > 0)
            {
                Point currentPixel = pixelQueue.Dequeue();
                coinClusterPoints.Add(currentPixel);
                int pixelX = currentPixel.X;
                int pixelY = currentPixel.Y;

                if (visitedPixels[pixelX, pixelY]) continue;

                coinClusterSize++;
                visitedPixels[pixelX, pixelY] = true;

                Color pixelColor = imageClone.GetPixel(pixelX, pixelY);

                if (pixelX - 1 >= 0 && pixelColor.R == 0 && !visitedPixels[pixelX - 1, pixelY])
                {
                    pixelQueue.Enqueue(new Point(pixelX - 1, pixelY));
                }

                if (pixelX + 1 < imageWidth && pixelColor.R == 0 && !visitedPixels[pixelX + 1, pixelY])
                {
                    pixelQueue.Enqueue(new Point(pixelX + 1, pixelY));
                }

                if (pixelY - 1 >= 0 && pixelColor.R == 0 && !visitedPixels[pixelX, pixelY - 1])
                {
                    pixelQueue.Enqueue(new Point(pixelX, pixelY - 1));
                }

                if (pixelY + 1 < imageHeight && pixelColor.R == 0 && !visitedPixels[pixelX, pixelY + 1])
                {
                    pixelQueue.Enqueue(new Point(pixelX, pixelY + 1));
                }
            }

            return (coinClusterPoints, coinClusterSize);
        }

        private int DetermineCoinValue(int coinSize)
        {
            if (coinSize > 8000)
            {
                fivePesoCount++;
                return 500; 
            }

            if (coinSize > 6000)
            {
                onePesoCount++;
                return 100;
            }

            if (coinSize > 4000)
            {
                twentyFiveCentsCount++;
                return 25; 
            }

            if (coinSize > 3500)
            {
                tenCentsCount++;
                return 10; 
            }

            fiveCentsCount++;
            return 5; 
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (processedImage == null)
            {
                MessageBox.Show("Please process an image first!");
                return;
            }

            label1.Text = (totalCoinValue / 100) + "." + totalCoinValue % 100;
        }

        private void openFileDialog1_FileOk_1(object sender, CancelEventArgs e)
        {
            originalImage = new Bitmap(openFileDialog1.FileName);
            pictureBox1.Image = originalImage;
        }
    }
}
