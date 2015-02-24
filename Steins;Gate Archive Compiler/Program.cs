﻿// Copyright (c) 2015 Davide Iuffrida
// License: Academic Free License ("AFL") v. 3.0
// AFL License page: http://opensource.org/licenses/AFL-3.0


using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Steins_Gate_Creation_Files
{
    class Program
    {
        public struct NPA2ENTRY
        {
            public int nameLen;
            public string name;
            public int fileSize;
            public int offset;
            public int unk;
        }

        public static NPA2ENTRY[] entry;

        public static byte[] key = Encoding.ASCII.GetBytes("BUCKTICK");
        public static int keyLen = 8;
        public static int fileCount = 0;
        public static int headerLen = 4;

        static void Main(string[] args)
        {
            Console.WriteLine("###################################");
            Console.WriteLine("#      NPA Archives Compiler      #");
            Console.WriteLine("###################################");
            Console.WriteLine("#   Original Tool Made by Nagato  #");
            Console.WriteLine("###################################");
            Console.WriteLine("#     New Tool Made by Daviex     #");
            Console.WriteLine("###################################");
            Console.WriteLine("#   Italian Steins;Gate VN Team   #");
            Console.WriteLine("###################################");
            Console.WriteLine("#        Version 1.2 Alpha        #");
            Console.WriteLine("###################################");
            Console.WriteLine("#            CodeName:            #");
            Console.WriteLine("###################################");
            Console.WriteLine("#         El Psy Congrue          #");
            Console.WriteLine("###################################");
            Console.WriteLine();
            Console.WriteLine("Press any key to start...");
            Console.ReadLine();

            if (args.Length == 0)
            {
                Console.WriteLine("You should move the folder on me to works!");
                Console.WriteLine("Press a button to close the program.");
                Console.ReadLine();
                Environment.Exit(0);
            }

            string originalFolder = args[0];

            #region Prints
            Console.WriteLine("I'm reading your folder...");
            Console.WriteLine("I'm encrypting the header...");
            Console.WriteLine();
            #endregion

            ScrambleKey(keyLen);

            ReadDirectory(originalFolder);

            BinaryWriter bw = new BinaryWriter(File.Create(originalFolder.Substring(originalFolder.LastIndexOf('\\') + 1) + ".npa.new"));

            WriteHeader(ref bw);
            WriteData(ref bw);

            bw.Flush();
            bw.Close();

            #region Prints

            Console.WriteLine();
            Console.WriteLine("I ended, but remember, the Agency still watch you!");
            Console.WriteLine("Press a button to close the program.");
            Console.ReadLine();

            #endregion
        }

        static public void ScrambleKey(int keylen)
        {
            for (int i = 0; i < keylen; i++)
                key[i] = (byte)~key[i];
        }

        static public void CryptBuffer(int keylen, ref byte[] header, int headerlen)
        {
            for (int i = 0; i < headerlen; i++)
                header[i] ^= key[i % keylen];
        }

        static public void ReadDirectory(string path)
        {
            int slashPos = path.LastIndexOf('\\');

            string file = String.Empty;

            int curOffset = 0;

            fileCount = Directory.EnumerateFiles(path).Count();

            entry = new NPA2ENTRY[fileCount+1];

            int index = 0;

            foreach (string files in Directory.EnumerateFiles(path))
            {
                file = files.Substring(slashPos + 1);
                BinaryReader br = new BinaryReader(File.OpenRead(file));

                file = file.Replace('\\', '/');

                //!!! This part is imortant for the JUST USA Version!!!
                entry[index].nameLen = file.Length * 2;
                entry[index].name = file;
                entry[index].offset = curOffset;
                entry[index].fileSize = (int)br.BaseStream.Length;
                entry[index].unk = 0;

                Console.WriteLine("[+]" + entry[index].name + "\tOffset[" + entry[index].offset.ToString("X") + "]\tSize[" + entry[index].fileSize.ToString("X") + "]");

                br.Close();

                curOffset += entry[index].fileSize;
                headerLen += entry[index].nameLen + 16;

                index++;
            }
        }

        static public void WriteHeader(ref BinaryWriter bw)
        {
            byte[] header = new byte[headerLen];
            byte[] headerBuffer;
            int fileNameLen = 0, headerArrayLen = 0;

            headerBuffer = BitConverter.GetBytes(headerLen);

            bw.Write(headerBuffer);
            
            headerBuffer = BitConverter.GetBytes(fileCount);

            for (int j = headerArrayLen, k = 0; k < headerBuffer.Length; k++, j++)
                header[j] = headerBuffer[k];

            headerArrayLen = headerBuffer.Length;

            for(int i = 0; i < fileCount; i++)
            {
                //File Name Length
                fileNameLen = entry[i].nameLen;
                           
                //Convert the file name length into bytes
                headerBuffer = BitConverter.GetBytes(fileNameLen);

                //Adding it to the header
                for (int j = headerArrayLen, k = 0; k < headerBuffer.Length; k++, j++)
                    header[j] = headerBuffer[k];

                //Sum of the current header length + new informations
                headerArrayLen += headerBuffer.Length;

                //File Name
                headerBuffer = Encoding.Unicode.GetBytes(entry[i].name);

                for (int j = headerArrayLen, k = 0; k < headerBuffer.Length; k++, j++)
                    header[j] = headerBuffer[k];

                headerArrayLen += headerBuffer.Length;

                //File Size
                headerBuffer = BitConverter.GetBytes(entry[i].fileSize);

                for (int j = headerArrayLen, k = 0; k < headerBuffer.Length; k++, j++)
                    header[j] = headerBuffer[k];

                headerArrayLen += headerBuffer.Length;

                //Pointer
                headerBuffer = BitConverter.GetBytes(entry[i].offset + headerLen + 4);

                for (int j = headerArrayLen, k = 0; k < headerBuffer.Length; k++, j++)
                    header[j] = headerBuffer[k];

                headerArrayLen += headerBuffer.Length;

                //Unknown!
                headerBuffer = BitConverter.GetBytes(entry[i].unk);

                for (int j = headerArrayLen, k = 0; k < 4; k++, j++)
                    header[j] = headerBuffer[k];

                headerArrayLen += headerBuffer.Length;
            }

            //Crypt all the first part of the header
            CryptBuffer(keyLen, ref header, headerLen);

            //Write it into the file
            bw.Write(header);
        }

        static void WriteData(ref BinaryWriter bw)
        {
            byte[] buffer;

            for(int i = 0; i < fileCount; i++)
            {
                //Read the file
                BinaryReader br = new BinaryReader(File.OpenRead(entry[i].name));

                //Read the entire file
                buffer = br.ReadBytes(entry[i].fileSize);

                //Crypt It!
                CryptBuffer(keyLen, ref buffer, entry[i].fileSize);

                bw.Write(buffer);
            }
        }
    }
}
