﻿using System;
using System.Drawing;
using System.IO;

namespace GaodeMapTileCutter
{
    public class TileCutter
    {
        private string imgPath;
        private string outputPath;
        private OutputFileTypes outputFileType;
        private LatLng center;
        private ZoomInfo zoomInfo;
        private OutputLayerTypes outputLayerType;
        private string mapTypeName;

        private Bitmap tileImage;
        private int imgWidth;
        private int imgHeight;

        private int totalZoom = 0;
        private int totalCount = 0;
        private int finishCount = 0;

        public TileCutter()
        {

        }

        public int GetFinishCount()
        {
            return finishCount;
        }

        public int GetTotalCount()
        {
            return totalCount;
        }

        public void SetInfo(string imgPath, string outputPath, OutputFileTypes outputFileType, LatLng center,
            ZoomInfo zoomInfo, OutputLayerTypes outputLayerType, string mapTypeName)
        {
            this.imgPath = imgPath;
            this.outputPath = outputPath;
            this.outputFileType = outputFileType;
            this.center = center;
            this.zoomInfo = zoomInfo;
            this.outputLayerType = outputLayerType;
            this.mapTypeName = mapTypeName;

            tileImage = new Bitmap(imgPath);
            imgWidth = tileImage.Width;
            imgHeight = tileImage.Height;
        }

        public void StartCut()
        {
            totalZoom = zoomInfo.MaxZoom - zoomInfo.MinZoom + 1;
            totalCount = 0;
            for (int i = zoomInfo.MinZoom; i <= zoomInfo.MaxZoom; i++)
            {
                totalCount += calcTotalTilesByZoom(i);
            }
            beginCut();
            if (outputFileType == OutputFileTypes.TileAndCode)
            {
                generateHTMLFile();
            }
            tileImage.Dispose();
        }

        private void DeleteCurrentFiles()
        {
            DirectoryInfo di = new DirectoryInfo(outputPath);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        private int calcTotalTilesByZoom(int zoom)
        {
            // 计算中心点所在的网格编号
            // 中心点的墨卡托平面坐标
            //Point mcPoint = Projection.fromLatLngToPoint(center);
            // 中心点在整个世界中的像素坐标
            //Point pxPoint = mcPoint.Multiply(Math.Pow(2, zoom - 18));
            //pxPoint.Round();
            Point pxPoint = TransformClassSlippy.fromLatLngToPoint(center, zoom);
            Point pxPoint2 = TransformClassSlippy.fromLatLngToPixel(center, zoom);
            pxPoint = pxPoint.Multiply(256);
            pxPoint = new Point(pxPoint.X+ pxPoint2.X,pxPoint.Y+pxPoint2.Y);
            //pxPoint = pxPoint.Multiply(256);
            //Point tileCoord = pxPoint.Divide(256);
            //tileCoord.Floor();

            // 中心网格左下角的像素坐标（相对于整个世界的像素坐标）
            // Point centerTilePixel = tileCoord.Multiply(256);

            int imgWidthForCurrentZoom = (int)Math.Round(imgWidth * Math.Pow(2, zoom - zoomInfo.ImageZoom));
            int imgHeightForCurrentZoom = (int)Math.Round(imgHeight * Math.Pow(2, zoom - zoomInfo.ImageZoom));

            // 图片左下角的像素坐标和网格编号
            Point bottomLeftPixel = new Point(pxPoint.X - imgWidthForCurrentZoom / 2, pxPoint.Y - imgHeightForCurrentZoom / 2);
            bottomLeftPixel.Round();
            Point bottomLeftTileCoords = bottomLeftPixel.Divide(256);
            bottomLeftTileCoords.Floor();
            // 图片右上角的像素坐标和网格编号
            Point upperRightPixel = new Point(pxPoint.X + imgWidthForCurrentZoom / 2, pxPoint.Y + imgHeightForCurrentZoom / 2);
            upperRightPixel.Floor();
            Point upperRightTileCoords = upperRightPixel.Divide(256);
            upperRightTileCoords.Floor();

            int totalCols = (int)(upperRightTileCoords.X - bottomLeftTileCoords.X + 1);
            int totalRows = (int)(upperRightTileCoords.Y - bottomLeftTileCoords.Y + 1);

            return totalCols * totalRows;
        }

        private void generateHTMLFile()
        {
            StreamWriter htmlFile = File.CreateText(outputPath + "/index.html");

            htmlFile.WriteLine("<!doctype html>");
            htmlFile.WriteLine("<html>");
            htmlFile.WriteLine("<head>");
            htmlFile.WriteLine("    <meta charset=\"utf - 8\">");
            htmlFile.WriteLine("    <meta http-equiv=\"X - UA - Compatible\" content=\"IE = edge\">");
            htmlFile.WriteLine("    <meta name=\"viewport\" content=\"initial - scale = 1.0, user - scalable = no, width = device - width\">");
            htmlFile.WriteLine("    <title>图片图层</title>");
            htmlFile.WriteLine("    <script src=\"https://webapi.amap.com/maps?v=1.4.13&key=您申请的key值\"></script>");
            htmlFile.WriteLine("    <style>");
            htmlFile.WriteLine("        html,");
            htmlFile.WriteLine("        body,");
            htmlFile.WriteLine("        #container {");
            htmlFile.WriteLine("            margin: 0;");
            htmlFile.WriteLine("            padding: 0;");
            htmlFile.WriteLine("            width: 100%;");
            htmlFile.WriteLine("            height: 100%;");
            htmlFile.WriteLine("        }");
            htmlFile.WriteLine("    </style>");
            htmlFile.WriteLine("</head>");
            htmlFile.WriteLine("<body>");
            htmlFile.WriteLine("<div id=\"container\"></div>");
            htmlFile.WriteLine("<script>");
            htmlFile.WriteLine("    var googleMapLayer = new AMap.TileLayer({");
            //htmlFile.WriteLine("        getTileUrl: 'tiles/[z]/[x]_[y].png',");
            //url: "http://19.0.0.220/gaode/{z}/{x}/{y}/x={x}&y={y}&z={z}.png", 
            htmlFile.WriteLine("        getTileUrl: 'tiles/[z]/[x]/[y]/x=[x]&y=[y]&z=[z].png',");
            htmlFile.WriteLine("        opacity:1,");
            htmlFile.WriteLine("        zIndex:99,");
            htmlFile.WriteLine("        zooms: [3, 19]");
            htmlFile.WriteLine("    });");
            htmlFile.WriteLine("  ");
            htmlFile.WriteLine("    var map = new AMap.Map('container', {");
            htmlFile.WriteLine("        resizeEnable: true,");
            htmlFile.WriteLine("        center: ["+ center.ToString() + "],");
            htmlFile.WriteLine("        zoom: 15,");
            htmlFile.WriteLine("        layers: [");
            htmlFile.WriteLine("            new AMap.TileLayer(),");
            htmlFile.WriteLine("            googleMapLayer");
            htmlFile.WriteLine("        ]");
            htmlFile.WriteLine("    });");
            htmlFile.WriteLine("</script>");
            htmlFile.WriteLine("</body>");
            htmlFile.WriteLine("</html>");



            
            htmlFile.Close();
        }

        private void beginCut()
        {
            for (int i = zoomInfo.MinZoom; i <= zoomInfo.MaxZoom; i++)
            {
                Bitmap image;
                if (i == zoomInfo.ImageZoom)
                {
                    image = tileImage;
                }
                else
                {
                    // 生成临时图片
                    Size newSize = new Size();
                    double factor = Math.Pow(2, i - zoomInfo.ImageZoom);
                    newSize.Width = (int)Math.Round(imgWidth * factor);
                    newSize.Height = (int)Math.Round(imgHeight * factor);
                    if (newSize.Width < 256 || newSize.Height < 256)
                    {
                        // 图片尺寸过小不再切了
                        Console.WriteLine("尺寸过小，跳过");
                        continue;
                    }
                    Console.WriteLine(tileImage.Height + ", " + tileImage.Width);
                    image = new Bitmap(tileImage, newSize);
                }
                cutImage(image, i);
            }
            if (finishCount != totalCount)
            {
                Console.WriteLine("修正完成的网格数");
                finishCount = totalCount;
            }
        }

        /// <summary>
        /// 切某一级别的图
        /// </summary>
        /// <param name="imgFile">图片对象</param>
        /// <param name="zoom">图片对应的级别</param>
        private void cutImage(Bitmap imgFile, int zoom)
        {
            int halfWidth = (int)Math.Round((double)imgFile.Width / 2);
            int halfHeight = (int)Math.Round((double)imgFile.Height / 2);
            Directory.CreateDirectory(outputPath + "/tiles/" + zoom.ToString());

            // 计算中心点所在的网格编号
            // 中心点的墨卡托平面坐标
            //Point mcPoint = Projection.fromLatLngToPoint(center);
            // 中心点在整个世界中的像素坐标
            //Point pxPoint = mcPoint.Multiply(Math.Pow(2, zoom - 18));
            //pxPoint.Round();
            Point pxPoint = TransformClassSlippy.fromLatLngToPoint(center, zoom);
            Point pxPoint2 = TransformClassSlippy.fromLatLngToPixel(center, zoom);
            pxPoint = pxPoint.Multiply(256);
            pxPoint = new Point(pxPoint.X+ pxPoint2.X,pxPoint.Y+pxPoint2.Y);
            Size pxDiff = new Size(0 - (int)pxPoint.X, 0 - (int)pxPoint.Y);
            //Point tileCoord = pxPoint.Divide(256);
            //tileCoord.Floor();
            

            // 中心网格左下角的像素坐标（相对于整个世界的像素坐标）
            //Point centerTilePixel = tileCoord.Multiply(256);
            // 左下角在图片中的像素位置
            //Point centerTilePixelInImage = new Point(tileCoord.X + pxDiff.Width, tileCoord.Y + pxDiff.Height);


            // 图片左下角的像素坐标和网格编号
            Point bottomLeftPixel = new Point(pxPoint.X - halfWidth, pxPoint.Y - halfHeight);
            bottomLeftPixel.Round();
            Point bottomLeftTileCoords = bottomLeftPixel.Divide(256);
            bottomLeftTileCoords.Floor();
            // 图片右上角的像素坐标和网格编号
            Point upperRightPixel = new Point(pxPoint.X + halfWidth, pxPoint.Y + halfHeight);
            upperRightPixel.Floor();
            Point upperRightTileCoords = upperRightPixel.Divide(256);
            upperRightTileCoords.Floor();

            int totalCols = (int)(upperRightTileCoords.X - bottomLeftTileCoords.X + 1);
            int totalRows = (int)(upperRightTileCoords.Y - bottomLeftTileCoords.Y + 1);
            Console.WriteLine("total col and row: " + totalCols + ", " + totalRows);
            for (int i = 0; i < totalCols; i++)
            {
                for (int j = 0; j < totalRows; j++)
                {
                    Bitmap img = new Bitmap(256, 256);
                    Point eachTileCoords = new Point(bottomLeftTileCoords.X + i, bottomLeftTileCoords.Y + j);
                    int offsetX = (int)eachTileCoords.X * 256 + pxDiff.Width + halfWidth;
                    int offsetY = ((int)eachTileCoords.Y * 256 + pxDiff.Height + halfHeight);
                    copyImagePixel(img, imgFile, offsetX, offsetY);

                    // ===================
                    // 这里定义文件输出的格式
                    // ===================
                    //string imgFileName = outputPath + "/tiles/" + zoom.ToString()
                    //    + "/tile-" + eachTileCoords.X.ToString() + "_" + eachTileCoords.Y.ToString() + ".png";
                    //string imgFileName = Path.Combine(outputPath, 
                    //                                  "tiles",
                    //                                  zoom.ToString(),
                    //                                  $"{eachTileCoords.X}_{eachTileCoords.Y}.png");
                    // --------------------------
                    // 兼容现有的离线高德瓦片命名格式
                    //url: "http://19.0.0.220/gaode/{z}/{x}/{y}/x={x}&y={y}&z={z}.png", 
                    string imgFileName = Path.Combine(outputPath,
                                                      "tiles",
                                                      zoom.ToString(),
                                                      eachTileCoords.X.ToString(),
                                                      eachTileCoords.Y.ToString(),
                                                      $"x={eachTileCoords.X}&y={eachTileCoords.Y}&z={zoom}.png");
                    // 每个目录下又只有一个文件，不知道下载瓦片的工具是什么脑回路
                    // --------------------------
                    var folder = Path.GetDirectoryName(imgFileName);
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);
                    img.Save(imgFileName, System.Drawing.Imaging.ImageFormat.Png);
                    //using (var ms = new MemoryStream())
                    //{
                    //    img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    //    File.WriteAllBytes(imgFileName, ms.ToArray());
                    //}
                    
                    img.Dispose();
                    finishCount++;
                }
            }
            if (imgFile != tileImage)
            {
                imgFile.Dispose();
            }
        }

        /// <summary>
        /// 将图片的部分像素复制到目标图像上
        /// </summary>
        /// <param name="destImage">目标图像</param>
        /// <param name="sourceImage">原始图像</param>
        /// <param name="offsetX">原始图像的像素水平偏移值</param>
        /// <param name="offsetY">原始图像的像素竖直偏移值</param>
        private void copyImagePixel(Bitmap destImage, Bitmap sourceImage, int offsetX, int offsetY)
        {
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    // 默认透明色
                    Color color = Color.FromArgb(0, 0, 0, 0);
                    int pixelX = offsetX + i;
                    int pixelY = offsetY + j;
                    if (pixelX >= 0 && pixelX < sourceImage.Width
                        && pixelY >= 0 && pixelY < sourceImage.Height)
                    {
                        color = sourceImage.GetPixel(pixelX, pixelY);
                    }

                    destImage.SetPixel(i, j, color);
                }
            }
        }
    }
}
