using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hikvisiondecrypter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form_DragEnter(object sender, DragEventArgs e)
        {
            string[] FileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            byte[] key = { 0x73, 0x8B, 0x55, 0x44 };
            byte[] fileContents = File.ReadAllBytes(FileList[0]);
            byte[] xorOutput = new byte[fileContents.Length];

            byte[] decryptedData = Decrypt(fileContents);

            Console.WriteLine(FileList[0]);

            for (int i = 0; i < decryptedData.Length; i++)
            {
                xorOutput[i] = (byte)(decryptedData[i] ^ key[i % key.Length]);
            }

            string output = Encoding.UTF8.GetString(xorOutput);
            string outputrg;

            if (checkBox1.Checked)
            {
                Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                outputrg = rgx.Replace(output, "");
            }
            else
            {
                outputrg = output;
            }

            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "Open Output File";
                dlg.Filter = "Text Files | *.txt";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string SafeFileName = Path.GetFileName(dlg.FileName);
                    File.WriteAllText(dlg.FileName, outputrg);
                }
            }
        }

        public static byte[] Decrypt(byte[] cipherText)
        {
            byte[] cipherBytes = cipherText;
            using (Aes encryptor = Aes.Create())
            {
                encryptor.Mode = CipherMode.ECB;
                encryptor.Padding = PaddingMode.Zeros;
                encryptor.Key = FromHex("27-99-77-f6-2f-6c-fd-2d-91-cd-75-b8-89-ce-0c-9a");
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = ms.ToArray();
                }
            }
            return cipherText;
        }

        public static byte[] FromHex(string hex)
        {
            hex = hex.Replace("-", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return raw;
        }
    }
}
