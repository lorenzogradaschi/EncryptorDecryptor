using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;

namespace AESnRSA
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        int AESKeySize, RSAKeySize;
        string inputFile, outputFile;
        string encryptedFile;
        string decryptedFile;
        string aeskeyFile, aesivFile, rsakeyFile; 
        bool sourceFile, EncFile, aesK, aes4, rsaK; 

        public static void EncryptFile(string inputFile, string outputFile, byte[] aesKey, byte[] aesIV, RSA rsa)
        {
            // Create the output file stream and the crypto stream
            using (FileStream fsOutput = new FileStream(outputFile, FileMode.Create))
            using (Aes aesAlg = Aes.Create())
            using (CryptoStream csEncrypt = new CryptoStream(fsOutput, aesAlg.CreateEncryptor(aesKey, aesIV), CryptoStreamMode.Write))
            {
                // Write the AES key and IV to the output file

                fsOutput.Write(aesKey, 0, aesKey.Length);
                fsOutput.Write(aesIV, 0, aesIV.Length);

                // Encrypt the file in blocks
                int bufferSize = 1048576; // 1MB
                byte[] buffer = new byte[bufferSize];
                using (FileStream fsInput = new FileStream(inputFile, FileMode.Open))
                {
                    int bytesRead;
                    while ((bytesRead = fsInput.Read(buffer, 0, bufferSize)) > 0)
                    {
                        csEncrypt.Write(buffer, 0, bytesRead);
                    }
                }
            }

            // Encrypt the AES key and IV with RSA and save to file
            byte[] encryptedAesKey = rsa.Encrypt(aesKey, RSAEncryptionPadding.OaepSHA256);
            byte[] encryptedAesIV = rsa.Encrypt(aesIV, RSAEncryptionPadding.OaepSHA256);
            using (FileStream fs = new FileStream(outputFile + ".key", FileMode.Create))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(encryptedAesKey.Length);
                bw.Write(encryptedAesKey);
                bw.Write(encryptedAesIV.Length);
                bw.Write(encryptedAesIV);
            }
        }

        public static void DecryptFile(string inputFile, string outputFile, byte[] aesKeyEncrypted, byte[] aesIVEncrypted, RSA rsa)
        {
            using (Aes aes = Aes.Create())
            {
                // Decrypt the AES key and IV with RSA
                byte[] aesKey = rsa.Decrypt(aesKeyEncrypted, RSAEncryptionPadding.OaepSHA256);
                byte[] aesIV = rsa.Decrypt(aesIVEncrypted, RSAEncryptionPadding.OaepSHA256);
                aes.KeySize = aesKey.Length * 8;
                
                // Create the streams used for decryption
                using (FileStream inputFileStream = new FileStream(inputFile, FileMode.Open))
                using (FileStream outputFileStream = new FileStream(outputFile, FileMode.Create))
                using (CryptoStream decryptStream = new CryptoStream(inputFileStream, aes.CreateDecryptor(aesKey, aesIV), CryptoStreamMode.Read))
                {
                    int offset = 16 + aes.KeySize / 8;
                    inputFileStream.Seek(offset, SeekOrigin.Begin);
                    // Decrypt the file
                    byte[] buffer = new byte[1048576];
                    int bytesRead;
                    while ((bytesRead = decryptStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        outputFileStream.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }

        static void EncryptFileAES(string inputFile, string outputFile, byte[] aesKey, byte[] aesIV)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = aesKey;
                aesAlg.IV = aesIV;

                // Determine if the input file is larger than 100 MB
                long fileSize = new FileInfo(inputFile).Length;
                bool isLargeFile = fileSize > 100000000;

                if (isLargeFile)
                {
                    // Create the output file stream and the crypto stream
                    using (FileStream fsOutput = new FileStream(outputFile, FileMode.Create))
                    //using (Aes aesAlg = Aes.Create())
                    using (CryptoStream csEncrypt = new CryptoStream(fsOutput, aesAlg.CreateEncryptor(aesKey, aesIV), CryptoStreamMode.Write))
                    {
                        // Write the AES key and IV to the output file
                        
                        fsOutput.Write(aesKey, 0, aesKey.Length);
                        fsOutput.Write(aesIV, 0, aesIV.Length);
                        Console.WriteLine(aesKey.Length);

                        // Encrypt the file in blocks
                        int bufferSize = 1048576; // 1MB
                        byte[] buffer = new byte[bufferSize];
                        using (FileStream fsInput = new FileStream(inputFile, FileMode.Open))
                        {
                            int bytesRead;
                            while ((bytesRead = fsInput.Read(buffer, 0, bufferSize)) > 0)
                            {
                                csEncrypt.Write(buffer, 0, bytesRead);
                            }
                        }
                    }
                }
                else
                {
                    // Encrypt the file in one go
                    using (FileStream inputFileStream = new FileStream(inputFile, FileMode.Open))
                    using (FileStream outputFileStream = new FileStream(outputFile, FileMode.Create))
                    using (CryptoStream encryptor = new CryptoStream(outputFileStream, aesAlg.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        inputFileStream.CopyTo(encryptor);
                    }
                }
            }
        }

        static void DecryptFileAES(string inputFile, string outputFile, byte[] aesKey, byte[] aesIV)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = aesKey;
                aesAlg.IV = aesIV;

                // Determine if the input file is larger than 100 MB
                long fileSize = new FileInfo(inputFile).Length;
                bool isLargeFile = fileSize > 100000000;

                if (isLargeFile)
                {
                    // Decrypt the AES key and IV with RSA
                    int KeySize = aesKey.Length * 8;

                    // Create the streams used for decryption
                    using (FileStream inputFileStream = new FileStream(inputFile, FileMode.Open))
                    using (FileStream outputFileStream = new FileStream(outputFile, FileMode.Create))
                    using (CryptoStream decryptStream = new CryptoStream(inputFileStream, aesAlg.CreateDecryptor(aesKey, aesIV), CryptoStreamMode.Read))
                    {
                        int offset = 16 + KeySize/8;
                        inputFileStream.Seek(offset, SeekOrigin.Begin);
                        // Decrypt the file
                        byte[] buffer = new byte[1048576];
                        int bytesRead;
                        while ((bytesRead = decryptStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            outputFileStream.Write(buffer, 0, bytesRead);
                        }
                    }
                }
                else
                {
                    // Decrypt the entire file in one go
                    using (FileStream inputStream = File.OpenRead(inputFile))
                    using (FileStream outputStream = File.OpenWrite(outputFile))
                    {
                        // Create a decryptor to perform the stream transform
                        using (ICryptoTransform decryptor = aesAlg.CreateDecryptor())
                        {
                            // Create a CryptoStream to perform the decryption
                            using (CryptoStream cryptoStream = new CryptoStream(inputStream, decryptor, CryptoStreamMode.Read))
                            {
                                // Read the decrypted data from the CryptoStream and write it to the output file
                                byte[] buffer = new byte[4096];
                                int bytesRead;
                                while ((bytesRead = cryptoStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    outputStream.Write(buffer, 0, bytesRead);
                                }
                            }
                        }
                    }
                }

            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = "C:\\";
            openFileDialog1.Filter = "All files (*.*)|*.*";
            openFileDialog1.Title = "Select a file to Encrypt";


            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                
                inputFile = openFileDialog1.FileName;
                sourceFile = true;
            }
            this.comboBox1.SelectedIndex = 0;
            this.comboBox2.SelectedIndex = 0;

        }

        private void button3_Click(object sender, EventArgs e)
        {
          
            byte[] aesKey;
            byte[] aesIV;
            byte[] aesKeyEncrypted;
            byte[] aesIVEncrypted;

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Title = "Save Encrypted file";
            saveFileDialog1.Filter = "Encrypted files (*.bin)|*.bin|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Save the file
                outputFile = saveFileDialog1.FileName;
            }

            // Saving the AES Key files and RSA Private key in the same directory of the encrypted file
            string directoryPath = Path.GetDirectoryName(outputFile);
            string aeskeyFilename = "aesKey.bin";
            string aesIVFilename = "aesIV.bin";
            aeskeyFilename = Path.Combine(directoryPath, aeskeyFilename);
            aesIVFilename = Path.Combine(directoryPath, aesIVFilename);

            if (radioButton1.Checked)
            {
                if (comboBox1.SelectedIndex == 0)
                {
                    AESKeySize = 128;
                }
                else if (comboBox1.SelectedIndex == 1)
                {
                    AESKeySize = 192;
                }
                else if (comboBox1.SelectedIndex == 2)
                {
                    AESKeySize = 256;
                }

                using (Aes aesAlg = Aes.Create())
                {
                    //aesAlg.KeySize = keyLength;
                    aesAlg.BlockSize = 128;
                    aesAlg.KeySize = AESKeySize;
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.GenerateKey();
                    aesAlg.GenerateIV();

                    EncryptFileAES(inputFile, outputFile, aesAlg.Key, aesAlg.IV);
                    File.WriteAllBytes(aeskeyFilename, aesAlg.Key);
                    File.WriteAllBytes(aesIVFilename, aesAlg.IV);

                }
            }
            else if (radioButton2.Checked)
            {
                if (comboBox2.SelectedIndex == 0)
                {
                    RSAKeySize = 1024;
                }
                else if (comboBox2.SelectedIndex == 1)
                {
                    RSAKeySize = 2048;
                }

                // Generate RSA public and private keys
                using (RSA rsa = RSA.Create())
                {
                    rsa.KeySize = RSAKeySize;
                    RSAParameters publicKey = rsa.ExportParameters(false);
                    //byte[] privateKey = rsa.ExportParameters(true);
                    //RSAParameters privateKey = rsa.ExportParameters(true);
                    string rsaKeyFileName = "rsaKey.pem";
                    string fileinpath = Path.Combine(directoryPath, rsaKeyFileName);
                    File.WriteAllBytes(fileinpath, rsa.ExportRSAPrivateKey());
                    //byte[] privateKeyBytes = rsa.ExportRSAPrivateKey();

                    // Generate AES key and IV
                    using (Aes aes = Aes.Create())
                    {
                        aes.KeySize = AESKeySize;
                        aes.GenerateKey();
                        aes.GenerateIV();
                        aesKey = aes.Key;
                        aesIV = aes.IV;
                    }
                    // Encrypt AES key and IV with RSA public key
                    aesKeyEncrypted = rsa.Encrypt(aesKey, RSAEncryptionPadding.OaepSHA256);
                    aesIVEncrypted = rsa.Encrypt(aesIV, RSAEncryptionPadding.OaepSHA256);

                    // Encrypt file using AES and save encrypted key and IV
                    EncryptFile(inputFile, outputFile, aesKey, aesIV, rsa);
                    File.WriteAllBytes(aeskeyFilename, aesKeyEncrypted);
                    File.WriteAllBytes(aesIVFilename, aesIVEncrypted);

                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog2 = new OpenFileDialog();
            openFileDialog2.InitialDirectory = "C:\\";
            openFileDialog2.Filter = "Encrypted files (*.bin)|*.bin|All files (*.*)|*.*";
            openFileDialog2.Title = "Select a file";


            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                // The user selected a file, so you can access the file path using openFileDialog1.FileName
                encryptedFile = openFileDialog2.FileName;
                EncFile = true;
            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog4 = new OpenFileDialog();
            openFileDialog2.InitialDirectory = "C:\\";
            openFileDialog2.Filter = "Key files (*.bin)|*.bin|All files (*.*)|*.*";
            openFileDialog2.Title = "Select a file";


            if (openFileDialog4.ShowDialog() == DialogResult.OK)
            {
                // The user selected a file, so you can access the file path using openFileDialog1.FileName
                aesivFile = openFileDialog4.FileName;
                aes4 = true;    
            }

        }

        private void button7_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog5 = new OpenFileDialog();
            openFileDialog2.InitialDirectory = "C:\\";
            openFileDialog2.Filter = "Key files (*.pem)|*.pem|All files (*.*)|*.*";
            openFileDialog2.Title = "Select RSA private key file";


            if (openFileDialog5.ShowDialog() == DialogResult.OK)
            {
                // The user selected a file, so you can access the file path using openFileDialog1.FileName
                rsakeyFile = openFileDialog5.FileName;
                rsaK = true;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (radioButton3.Checked && aesK && aes4 && EncFile)   // AES Mode
            {
                byte[] aesKey = File.ReadAllBytes(aeskeyFile);
                byte[] aesIV = File.ReadAllBytes(aesivFile);
                DecryptFileAES(encryptedFile, decryptedFile, aesKey, aesIV);
            }
            else if(radioButton4.Checked && EncFile && rsaK && aes4 && aesK)   // RSA Mode
            {
                // Decrypt file using RSA private key and saved key and IV
                byte[] aesKeyDecrypted = File.ReadAllBytes(aeskeyFile);
                byte[] aesIVDecrypted = File.ReadAllBytes(aesivFile);
                //rsa.ImportParameters(privateKey);
                // Read RSA private key
                byte[] privateKeyBytes = File.ReadAllBytes(rsakeyFile);
                RSA rsa = RSA.Create();
                rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
                DecryptFile(encryptedFile, decryptedFile, aesKeyDecrypted, aesIVDecrypted, rsa);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog3 = new OpenFileDialog();
            openFileDialog2.InitialDirectory = "C:\\";
            openFileDialog2.Filter = "Key files (*.bin)|*.bin|All files (*.*)|*.*";
            openFileDialog2.Title = "Select a file";


            if (openFileDialog3.ShowDialog() == DialogResult.OK)
            {
                // The user selected a file, so you can access the file path using openFileDialog1.FileName
                aeskeyFile = openFileDialog3.FileName;
                // Do something with the file...
            }

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        

    }
}
