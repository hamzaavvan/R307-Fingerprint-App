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
        byte   package_identifier;
        byte[] package = new byte[512];
        byte[] package_len = new byte[2];
        byte[] checksum = new byte[2];
        byte[] package_content = new byte[512];

        public Fingerprint()
        {
            port = new SerialPort("COM4", 9600);
            port.Open();

            this.set_header();
            this.set_adder();
        }

        private void set_header()
        {
            package[0] = 0xEF;
            package[1] = 0x01;
        }

        private void set_adder()
        {
            package[2] = 0xff;
            package[3] = 0xff;
            package[4] = 0xff;
            package[5] = 0xff;
        }

        private void set_package_length(int packet_len, int checksum_len)
        {
            this.package_len = BitConverter.GetBytes(packet_len + checksum_len);

            Buffer.BlockCopy(package_len, 0, package, package.Length-1, package_len.Length);
        }

        private void set_package_content(string data)
        {
            this.package_content = Encoding.ASCII.GetBytes(data);

            Buffer.BlockCopy(package_content, 0, package, package.Length - 1, package_content.Length);
        }

        private void set_checksum()
        {
            Buffer.BlockCopy(this.checksum, 0, package, package.Length - 1, this.checksum.Length);
        }

        private void calculate_checksum()
        {
            this.checksum = BitConverter.GetBytes(this.package_len.Length + this.package_content.Length + this.package_identifier);
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
        private void set_package_identifier(byte code = 0x1)
        {
            this.package_identifier = code;
            package[6] = code;
        }

        private void init(byte identifier, string _string)
        {
            this.set_package_identifier(identifier);
            this.calculate_checksum();
            this.set_package_length(_string.Length, this.checksum.Length);
            this.set_package_content(_string);
            this.set_checksum();
        }

        public void send_command(string cmd)
        {
            this.init(0x1, cmd);
        }

        public void send_data(string data)
        {
            this.init(0x2, data);
        }

        public void send_acknowledgement(string acknowledgement)
        {
            this.init(0x7, acknowledgement);
        }

        public void end(string data = "")
        {
            this.init(0x8, data);
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
