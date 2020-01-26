using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    class Imager
    {
        public static Bitmap ByteToImage(byte[] blob)
        {
            MemoryStream mStream = new MemoryStream();
            byte[] pData = blob;
            mStream.Write(pData, 0, Convert.ToInt32(pData.Length));
            Bitmap bm = new Bitmap(mStream, false);
            mStream.Dispose();
            return bm;
        }
    }
}
