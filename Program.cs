using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DtxCS;
using DtxCS.DataTypes;

namespace SongDTAInjector
{
    class Program
    {

        static void DisplayUsage()
        {
            Console.WriteLine();
            Console.WriteLine("Arguments:");
            Console.WriteLine("SongDTAInjector <script.dta> <target_milo.milo_ps3/xbox/wii> <out.milo_xbox/ps3/wii>");
            Console.WriteLine("  Injects a DTA script into a Milo scene.");
        }

        static int FindBytePattern(byte[] array, byte[] pattern)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
                {
                    return i;
                }
            }

            return -1;
        }

        static void Main(string[] args)
        {

            Console.WriteLine("Milo Scene DTA Injector - 2022 ihatecompvir");
            Console.WriteLine("--------------------------------------------");

            if (args.Length == 0)
            {
                DisplayUsage();
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine($"DTA file {args[1]} does not exist!");
                return;
            }


            if (!File.Exists(args[1]))
            {
                Console.WriteLine($"Milo file {args[1]} does not exist!");
                return;
            }


            Console.WriteLine($"Attempting to inject {args[0]} into {args[1]}...");

            // find the DEADDEAD pattern indicating the end of the first ObjectDir

            byte[] pattern = new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE };
            var dtbTreePos = FindBytePattern(File.ReadAllBytes(args[1]), pattern);

            if (dtbTreePos == -1)
            {
                Console.WriteLine("Could not find pattern! Please check that your Milo file is valid.");
                return;
            }

            Console.WriteLine("First asset in file found, injecting there");

            using (FileStream dtaStream = new FileStream(args[0], FileMode.Open))
            using (FileStream miloStream = new FileStream(args[1], FileMode.Open))
            using (FileStream outStream = new FileStream(args[2], FileMode.Create))
            using (MemoryStream dtbStream = new MemoryStream((int)dtaStream.Length * 3))
            {
                var miloReader = new BinaryReader(miloStream);
                var miloWriter = new BinaryWriter(outStream);

                DataArray data = DTX.FromDtaStream(dtaStream);
                dtaStream.Close();

                long dtbSize = DTX.ToDtb(data, dtbStream, 1, false);
                var dtbBytes = dtbStream.ToArray();

                // fix block sizes

                var magic = miloReader.ReadUInt32();
                var blockOffset = miloReader.ReadUInt32();
                var blockCount = miloReader.ReadUInt32();

                if (magic != 3401506479)
                {
                    Console.WriteLine("Milo magic value is the compressed type. This tool will only work on decompressed milos!");
                    return;
                }

                // adjust the block sizes so the entire scene is one contiguous block

                miloWriter.Write(magic);
                miloWriter.Write(blockOffset);

                uint newBlockSize = (uint)miloStream.Length - blockOffset;

                miloWriter.Write(1);
                miloWriter.Write((int)(newBlockSize + dtbBytes.Length));
                miloWriter.Write((int)(newBlockSize + dtbBytes.Length));

                for (int i = 1; i < blockCount; i++)
                {
                    miloWriter.Write(0);
                }

                // jump to the first block

                miloWriter.BaseStream.Position = blockOffset;
                miloReader.BaseStream.Position = blockOffset;

                miloWriter.Write(miloReader.ReadBytes((dtbTreePos - 5) - (int)blockOffset));
                miloWriter.Write(dtbBytes);

                miloReader.ReadByte();

                int rest = (int)(miloReader.BaseStream.Length - miloReader.BaseStream.Position);
                miloWriter.Write(miloReader.ReadBytes(rest));

                Console.WriteLine("DTA injected! If you get a crash or the game doesn't work properly, please ensure that your DTA is valid.");




            }
        }
    }
}
