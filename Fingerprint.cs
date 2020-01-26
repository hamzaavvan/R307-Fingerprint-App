using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Drawing;
using System.IO;

namespace WindowsFormsApp1
{
    class Fingerprint
    {
        SerialPort port;
        byte[] cmd = new byte[512];

        public Fingerprint()
        {
            port = new SerialPort("COM4", 9600);
            port.Open();
        }

        private void set_header()
        {
            cmd[0] = 0xEF;
            cmd[1] = 0x01;
        }

        private void set_adder()
        {
            cmd[2] = 0xff;
            cmd[3] = 0xff;
            cmd[4] = 0xff;
            cmd[5] = 0xff;
        }

        private void set_package_length(int data_packet_len, int cmd_packet_len, int checksum_len)
        {
            byte[] package_len = BitConverter.GetBytes(data_packet_len + cmd_packet_len + checksum_len);

            Buffer.BlockCopy(package_len, 0, cmd, 6, package_len.Length);
        }

        private void set_package_content()
        {

        }

        private void checksum()
        {

        }

        /**
         * Package Identifier
         * 
         * @param byte code = 01h,02h,07h,08h
         * 01h for command packet
         * 02h for data packet
         * 07h for acknowledge package
         * 08h for end of data packet
         */
        private void package_identifier(byte code = 0x1)
        {
            cmd[6] = code;
        }

        public Bitmap write(string text = "")
        {
            String s = "ReadSysPara";
            byte[] buff = new byte[32];

            port.Write(s);

            port.Read(buff, 0, 32);

            string result = Encoding.UTF8.GetString(buff, 0, buff.Length);

            return ByteToImage(buff);
        }

        public static Bitmap ByteToImage(byte[] blob)
        {
            MemoryStream mStream = new MemoryStream();
            byte[] pData = blob;
            mStream.Write(pData, 0, Convert.ToInt32(pData.Length));
            Bitmap bm = new Bitmap(mStream, false);
            mStream.Dispose();
            return bm;
        }

        ~Fingerprint()
        {
            port.Close();
        }
    }
}
