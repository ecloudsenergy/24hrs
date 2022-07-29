using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace sample2.helpers
{
    public class routines
    {
        public static void Denominations(List<int> denominations)
        {
            denominations.Add(2000);
            denominations.Add(500);
            denominations.Add(200);
            denominations.Add(100);
            denominations.Add(50);
            denominations.Add(20);
            denominations.Add(10);
        }

        public static void DenominationsCA(List<int> denominations)
        {
            denominations.Add(200);
            denominations.Add(100);
            denominations.Add(50);
            denominations.Add(20);
            denominations.Add(10);
        }

        public static string split_amount(string amount)
        {
            string[] split_amount = amount.Split(' ');
            return split_amount[1];

        }



        public static int row_col_convertion(int row, int col)
        {
            int coil = ((row - 1) * 8) + col;
            return coil;
        }


        public static List<byte> ModRTU_CRC(List<byte> data)
        {
            List<byte> checksum = new List<byte>();
            ushort crc = 0xFFFF;

            for (int i = 0; i < data.Count; i++)
            {
                crc ^= data[i];          // XOR byte into least sig. byte of crc

                for (int j = 0; j < 8; j++)
                {    // Loop over each bit
                    if ((crc & 0x01) == 1)
                        crc = (ushort)((crc >> 1) ^ 0xA001);                    // Shift right and XOR 0xA001
                    else                            // Else LSB is not set
                        crc = (ushort)(crc >> 1);                    // Just shift right
                }
            }

            checksum.Add((byte)((crc >> 8) & 0xFF));
            checksum.Add((byte)(crc & 0xFF));
            return checksum;
        }


        public static string ConvertToHexstring(List<byte> data_content)
        {
            string output_val = "";
            foreach (byte hexval in data_content)
            {
                byte[] _hexval = new byte[] { hexval };
                output_val += hexval.ToString("X2") + " ";
            }

            return output_val;
        }

        public static byte Checksum(List<byte> hexstring)
        {
            byte checksum = new byte();
            foreach (byte item in hexstring) checksum += item;
            return checksum;
        }

        public static T GetChildOfType<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}
