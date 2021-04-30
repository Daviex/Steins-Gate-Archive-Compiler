// Copyright (c) 2015 Davide Iuffrida
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
    private enum Choice
    {
      NPA = 1,
      MPK = 2
    }

    static void Main(string[] args)
    {
      Console.WriteLine(
          @"
                      
                      ###################################
                      #      SG Archives Compiler       #
                      ###################################
                      #         Made by Daviex          #
                      ###################################
                      #           Version 2.0           #
                      ###################################
                      #            Codename:            #
                      ###################################
                      #         El Psy Congroo          #
                      ###################################
                      
                           Press any key to start...    
                                                         ");
      Console.ReadLine();

      if (args.Length == 0)
      {
        Console.WriteLine("You should move the folder on me to works!");
        Console.WriteLine("Press a button to close the program.");
        Console.ReadLine();
        Environment.Exit(0);
      }

      int choice = 0;
      do
      {
        Console.WriteLine("In what format you want to compile the archive?");
        Console.WriteLine("[1] NPA");
        Console.WriteLine("[2] MPK");
      }
      while(!int.TryParse(Console.ReadLine(), out choice) || (choice != (int)Choice.NPA && choice != (int)Choice.MPK));

      string originalFolder = args[0];

      Console.WriteLine("I'm reading your folder...");
      Console.WriteLine();

      if (choice == (int)Choice.NPA)
      {
        Console.WriteLine("I'm encrypting the header...");
        Console.WriteLine();

        NPA.ScrambleKey(NPA.keyLen);

        NPA.ReadDirectory(originalFolder);

        BinaryWriter bw = new BinaryWriter(File.Create(originalFolder.Substring(originalFolder.LastIndexOf('\\') + 1) + ".npa.new"));

        NPA.WriteHeader(ref bw);
        NPA.WriteData(ref bw);

        bw.Flush();
        bw.Close();
      }

      if (choice == (int)Choice.MPK)
      {
        MPK.ReadDirectory(originalFolder);

        BinaryWriter bw = new BinaryWriter(File.Create(originalFolder.Substring(originalFolder.LastIndexOf('\\') + 1).Replace("dir_", "") + ".new"));

        MPK.WriteHeader(ref bw);
        MPK.WriteData(ref bw);
      }

      Console.WriteLine();
      Console.WriteLine("I ended, but remember, the Agency still watches you!");
      Console.WriteLine("Press a button to close the program.");
      Console.ReadLine();
    }

    public static class NPA
    {
      public struct NPA2ENTRY
      {
        public int nameLen;
        public string name;
        public int fileSize;
        public int offset;
        public int unk;
      }

      public static NPA2ENTRY[] entries;
      public static byte[] key = Encoding.ASCII.GetBytes("BUCKTICK");
      public static int keyLen = 8;
      public static int fileCount = 0;
      public static int headerLen = 4;

      public static void ScrambleKey(int keylen)
      {
        for (int i = 0; i < keylen; i++)
          key[i] = (byte)~key[i];
      }

      public static void CryptBuffer(int keylen, ref byte[] header, int headerlen)
      {
        for (int i = 0; i < headerlen; i++)
          header[i] ^= key[i % keylen];
      }

      public static void ReadDirectory(string path)
      {
        int slashPos = path.LastIndexOf('\\');

        string file = String.Empty;

        int curOffset = 0;

        //New in C# 6.0: Now it require the Searchoption for Subdirectories,
        //Or it will only looks in top directories
        fileCount = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).Count();

        entries = new NPA2ENTRY[fileCount + 1];

        int index = 0;

        //New in C# 6.0: Now it require the Searchoption for Subdirectories,
        //Or it will only looks in top directories
        foreach (string files in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
        {
          file = files.Substring(slashPos + 1);
          BinaryReader br = new BinaryReader(File.OpenRead(file));

          file = file.Replace('\\', '/');

          //!!! This part is imortant for the JUST USA Version!!!
          entries[index].nameLen = file.Length * 2;
          entries[index].name = file;
          entries[index].offset = curOffset;
          entries[index].fileSize = (int)br.BaseStream.Length;
          entries[index].unk = 0;

          Console.WriteLine("[+]{0}\tOffset[{1}]\tSize[{2}]", entries[index].name, entries[index].offset.ToString("X"), entries[index].fileSize.ToString("X"));

          br.Close();

          curOffset += entries[index].fileSize;
          headerLen += entries[index].nameLen + 16;

          index++;
        }
      }

      public static void WriteHeader(ref BinaryWriter bw)
      {
        byte[] header = new byte[headerLen];
        byte[] headerBuffer;
        int fileNameLen = 0, headerArrayLen = 0;

        headerBuffer = BitConverter.GetBytes(headerLen);

        bw.Write(headerBuffer);

        //Number of Files
        headerBuffer = BitConverter.GetBytes(fileCount);
        WriteHeaderBuffer(ref header, headerBuffer, ref headerArrayLen);

        for (int i = 0; i < fileCount; i++)
        {
          //File Name Length
          fileNameLen = entries[i].nameLen;

          //Convert the file name length into bytes
          headerBuffer = BitConverter.GetBytes(fileNameLen);
          WriteHeaderBuffer(ref header, headerBuffer, ref headerArrayLen);

          //File Name
          headerBuffer = Encoding.Unicode.GetBytes(entries[i].name);
          WriteHeaderBuffer(ref header, headerBuffer, ref headerArrayLen);

          //File Size
          headerBuffer = BitConverter.GetBytes(entries[i].fileSize);
          WriteHeaderBuffer(ref header, headerBuffer, ref headerArrayLen);

          //Pointer
          headerBuffer = BitConverter.GetBytes(entries[i].offset + headerLen + 4);
          WriteHeaderBuffer(ref header, headerBuffer, ref headerArrayLen);

          //Unknown!
          headerBuffer = BitConverter.GetBytes(entries[i].unk);
          WriteHeaderBuffer(ref header, headerBuffer, ref headerArrayLen);
        }

        //Crypt all the first part of the header
        CryptBuffer(keyLen, ref header, headerLen);

        //Write it into the file
        bw.Write(header);
      }

      /// <summary>
      /// This will avoid to write 5 for, just use function!
      /// </summary>
      /// <param name="header">Data</param>
      /// <param name="headerBuffer">Temporary Data</param>
      /// <param name="headerArrayLen">Length Temp Data</param>
      public static void WriteHeaderBuffer(ref byte[] header, byte[] headerBuffer, ref int headerArrayLen)
      {
        //Adding it to the header
        for (int j = headerArrayLen, k = 0; k < headerBuffer.Length; k++, j++)
          header[j] = headerBuffer[k];

        //Sum of the current header length + new informations
        headerArrayLen += headerBuffer.Length;
      }

      public static void WriteData(ref BinaryWriter bw)
      {
        byte[] buffer;

        for (int i = 0; i < fileCount; i++)
        {
          //Read the file
          BinaryReader br = new BinaryReader(File.OpenRead(entries[i].name));

          //Read the entire file
          buffer = br.ReadBytes(entries[i].fileSize);

          //Crypt It!
          CryptBuffer(keyLen, ref buffer, entries[i].fileSize);

          bw.Write(buffer);
        }
      }
    }

    public static class MPK
    {
      const short headerVersion = 0x02;
      public static int fileCount = 0;
      public static int headerSize = 0;

      public struct MPK2ENTRY
      {
        public int fileNum;
        public long offset;
        public long length;
        public string filename;
        public string originalPath;
      }

      public static MPK2ENTRY[] entries;

      public static void ReadDirectory(string path)
      {
        int slashPos = path.LastIndexOf('\\');
        var fileEntries = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).ToList();
        fileCount = fileEntries.Count();

        string file = String.Empty;

        headerSize = 0x44 + (fileEntries.Count * 0x100) + 0x1BC;
        long curOffset = headerSize;

        int index = 0;

        entries = new MPK2ENTRY[fileCount];
        foreach (string files in fileEntries.OrderBy(x => x))
        {
          //file = files.Substring(slashPos + 1);
          file = files.Replace($"{path}\\", "");
          
          using (BinaryReader br = new BinaryReader(File.OpenRead(files)))
          {
            entries[index].fileNum = index;
            entries[index].offset = curOffset;
            entries[index].length = (int)br.BaseStream.Length;
            entries[index].filename = file;
            entries[index].originalPath = files;

            Console.WriteLine("[+]{0}\tOffset[{1}]\tSize[{2}]", entries[index].filename, entries[index].offset.ToString("X"), entries[index].length.ToString("X"));
          }

          curOffset += entries[index].length;

          index++;
        }
      }

      public static void WriteHeader(ref BinaryWriter bw)
      {
        byte[] header = new byte[headerVersion];

        //Magic
        bw.Write(Encoding.ASCII.GetBytes("MPK"));
        bw.Write(new byte []{ 0x00, 0x00, 0x00 });
        //Header Size is constant
        bw.Write(BitConverter.GetBytes(headerVersion));
        //Number of Files
        bw.Write(BitConverter.GetBytes(fileCount));
        //Padding
        bw.Write(new byte[0x38]);

        for (int i = 0; i < fileCount; i++)
        {
          bw.Write(BitConverter.GetBytes(entries[i].fileNum));
          bw.Write(BitConverter.GetBytes(entries[i].offset));
          bw.Write(BitConverter.GetBytes(entries[i].length));
          bw.Write(BitConverter.GetBytes(entries[i].length));

          byte[] fn = new byte[0xE4];
          var fileBytes = Encoding.UTF8.GetBytes(entries[i].filename);
          for (int j = 0; j < fileBytes.Length; j++) {
            fn[j] = fileBytes[j];
          }
          bw.Write(fn);
        }

        bw.Write(new byte[0x1BC]);
        bw.Flush();
      }

      public static void WriteData(ref BinaryWriter bw)
      {
        byte[] buffer;

        for (int i = 0; i < fileCount; i++)
        {
          //Read the file
          BinaryReader br = new BinaryReader(File.OpenRead(entries[i].originalPath));

          //Read the entire file
          buffer = br.ReadBytes((int)entries[i].length);

          bw.Write(buffer);
        }
      }
    }
  }
}
