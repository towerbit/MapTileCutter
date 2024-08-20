using System;
using System.Drawing;
using System.IO;

namespace GaodeMapTileCutter
{
    /*
     * Created by wupeng 2019.02.26
     * 参考资料：http://wiki.openstreetmap.org/wiki/Slippy_map_tilenames
     * 适用地图：高德，Google Map，OSM
     */
    public class TransformClassSlippy
    {
        
        public TransformClassSlippy()
        {

        }

        private double _Math_sinh(double x)
        {
            return (Math.Exp(x) - Math.Exp(-x)) / 2;
        }

        /*
        * 某一瓦片等级下瓦片地图X轴(Y轴)上的瓦片数目
        */
        private static double _getMapSize(int level)
        {
            return Math.Pow(2, level);
        }

        /*
        * 分辨率，表示水平方向上一个像素点代表的真实距离(m)
        */
        private double getResolution(double latitude, int level)
        {
            double resolution = 6378137.0 * 2 * Math.PI * Math.Cos(latitude) / 256 / _getMapSize(level);
            return resolution;
        }

        private static double _lngToTileX(double longitude, int level)
        {
            double x = (longitude + 180) / 360;
            double tileX = Math.Floor(x * _getMapSize(level));
            return tileX;
        }

        private static double _latToTileY(double latitude, int level)
        {
            double lat_rad = latitude * Math.PI / 180;
            double y = (1 - Math.Log(Math.Tan(lat_rad) + 1 / Math.Cos(lat_rad)) / Math.PI) / 2;
            double tileY = Math.Floor(y * _getMapSize(level));

            // 代替性算法,使用了一些三角变化，其实完全等价
            //let sinLatitude = Math.sin(latitude * Math.PI / 180);
            //let y = 0.5 - Math.log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI);
            //let tileY = Math.floor(y * this._getMapSize(level));

            return tileY;
        }

        /*
        * 从经纬度获取某一级别瓦片坐标编号
        */
        public string lnglatToTile(double longitude, double latitude, int level)
        {
            double tileX = _lngToTileX(longitude, level);
            double tileY = _latToTileY(latitude, level);

            return tileX + "," + tileY;
        }

        /// <summary>
        /// 从经纬度转换到平面坐标
        /// </summary>
        /// <param name="latLng">经纬度</param>
        /// <returns>平面坐标</returns>
        public static Point fromLatLngToPoint(LatLng latLng, int level)
        {
            //double[] latlng = GpsUtil.Gps84ToGcj02(latLng.Lat, latLng.Lng);
            double tileX = _lngToTileX(latLng.Lng, level);
            double tileY = _latToTileY(latLng.Lat, level);

            //double pixelX = _lngToPixelX(latLng.Lng, level);
            //double pixelY = _latToPixelY(latLng.Lat, level);

            return new Point(tileX, tileY);
        }

        /// <summary>
        /// 从经纬度获取点在某一级别瓦片中的像素坐标
        /// </summary>
        /// <param name="latLng">经纬度</param>
        /// <returns>像素坐标</returns>
        public static Point fromLatLngToPixel(LatLng latLng, int level)
        {

            double pixelX = _lngToPixelX(latLng.Lng, level);
            double pixelY = _latToPixelY(latLng.Lat, level);

            return new Point(pixelX, pixelY);
        }

        private static double _lngToPixelX(double longitude, int level)
        {
            double x = (longitude + 180) / 360;
            double pixelX = Math.Floor(x * _getMapSize(level) * 256 % 256);

            return pixelX;
        }

        private static double _latToPixelY(double latitude, int level)
        {
            double sinLatitude = Math.Sin(latitude * Math.PI / 180);
            double y = 0.5 - Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI);
            double pixelY = Math.Floor(y * _getMapSize(level) * 256 % 256);

            return pixelY;
        }

        /*
        * 从经纬度获取点在某一级别瓦片中的像素坐标
        */
        private void lnglatToPixel(double longitude, double latitude, int level)
        {
            double pixelX = _lngToPixelX(longitude, level);
            double pixelY = _latToPixelY(latitude, level);
        }

        private double _pixelXTolng(double pixelX, double tileX, int level)
        {
            double pixelXToTileAddition = pixelX / 256.0;
            double lngitude = (tileX + pixelXToTileAddition) / _getMapSize(level) * 360 - 180;

            return lngitude;
        }

        private double _pixelYToLat(double pixelY, double tileY, int level)
        {
            double pixelYToTileAddition = pixelY / 256.0;
            double latitude = Math.Atan(this._Math_sinh(Math.PI * (1 - 2 * (tileY + pixelYToTileAddition) / _getMapSize(level)))) * 180.0 / Math.PI;

            return latitude;
        }

        /*
        * 从某一瓦片的某一像素点到经纬度
        */
        private void pixelToLnglat(double pixelX, double pixelY, double tileX, double tileY, int level)
        {
            double lng = this._pixelXTolng(pixelX, tileX, level);
            double lat = this._pixelYToLat(pixelY, tileY, level);
            
        }
    }
}
