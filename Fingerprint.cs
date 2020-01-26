using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;


namespace WindowsFormsApp1
{
    class Fingerprint
    {
        SerialPort port;

        byte   package_identifier;
        byte[] header       = new byte[] { 0xEF, 0x01 };
        byte[] adder        = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
        byte[] package      = new byte[720]; // array to hold entire package to be sent to module
        byte[] package_len  = new byte[2];
        byte[] checksum     = new byte[2];
        byte[] package_content = new byte[512];

        const byte FINGERPRINT_SEND_CMD = 0x1;
        const byte FINGERPRINT_SEND_DATA = 0x2;
        const byte FINGERPRINT_SEND_ACKNOWLEDGMENT = 0x7;
        const byte FINGERPRINT_END_DATA = 0x8;

        public Fingerprint(string portName, int baudRate)
        {
            port = new SerialPort(portName, baudRate);
            port.Open();

            this.set_header();
            this.set_adder();
        }

        private void set_header()
        {
            Buffer.BlockCopy(this.header, 0, this.package, 0, this.header.Length);
        }

        private void set_adder()
        {
            Buffer.BlockCopy(this.adder, 0, this.package, this.package.Length-1, adder.Length);
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
            this.init(FINGERPRINT_SEND_CMD, cmd);
        }

        public void send_data(string data)
        {
            this.init(FINGERPRINT_SEND_DATA, data);
        }

        public void send_acknowledgement(string acknowledgement)
        {
            this.init(FINGERPRINT_SEND_ACKNOWLEDGMENT, acknowledgement);
        }

        public void end_data(string data = "")
        {
            this.init(FINGERPRINT_END_DATA, data);
        }

        public byte[] read()
        {
            byte[] buff = new byte[32];

            port.Read(buff, 0, 32);

            return buff;
        }

        public string readString()
        {
            byte[] buff = this.read();
            string result = Encoding.UTF8.GetString(buff, 0, buff.Length);

            return result;
        }

        ~Fingerprint()
        {
            port.Close();
            Array.Clear(package, 0, package.Length);
        }
    }
}
