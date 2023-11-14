using System;
using System.IO;
using Meisui.Random;
using System.Threading.Tasks;
using System.Text;
using Stego;
using System.Collections.Generic;
using System.Linq;
using System.IO.Compression;
using System.Diagnostics;

namespace stego
{
    class Program
    {
        public const int N = 44;
        public const int M = 9 * N - 12;
        static char[,] key = new char[10, M];
        static char[,] etalons;
        static readonly List<int>[] kl = new List<int>[10];
        static void Keygen(string @keyfile)
        {
            Stegomask s = new Stegomask(N);
            s.GenerateKey();
            s.GetKey(out key);
            BinaryWriter w = new BinaryWriter
                (new FileStream(@keyfile, FileMode.Create));
            for (int i = 0; i < key.GetLength(0); i++)
                for (int j = 0; j < key.GetLength(1); j++)
                    if (key[i, j] == '1') w.Write(i * M + j);
            w.Close();
        }
        static string Hide(byte c)
        {
            string str = Convert.ToUInt32(c).ToString().PadLeft(3, '0');
            char[] stego = new char[3 * M];
            MersenneTwister r = new MersenneTwister();
            for (int i = 0; i < 3; i++)
            {
                int h = str[i] - 48;
                for (int k = 0; k < M; k++)
                    if (key[h, k] == '0')
                        stego[i * M + k] = r.genrand_Int32() % 2 == 0 ? '0' : '1';
                    else
                        stego[i * M + k] = etalons[h, k];
            }
            return new string(stego);
        }
        static void Hidefiles(string @keyfile, string @sourcefile, string @destination)
        {
            Stegomask s = new Stegomask(N);
            s.GetEtalons(out etalons);
            FileStream kf = new FileStream(@keyfile, FileMode.Open);
            if (kf.Length > 400) throw new Exception("Invalid key detected.");
            BinaryReader br = new BinaryReader(kf);
            int ki = br.ReadInt32();
            int kic = 0;
            for (int i = 0; i < 10; i++)
                for (int j = 0; j < M; j++)
                    if (kic < kf.Length / 4 && ki == (i * M + j))
                    {
                        key[i, j] = '1';
                        if (++kic < kf.Length / 4)
                            ki = br.ReadInt32();
                    }
                    else
                        key[i, j] = '0';
            kf.Close();
            br.Close();
            if (!sourcefile.Contains("\\"))
                sourcefile = Directory.GetCurrentDirectory() + "\\" + sourcefile;
            if (@destination[@destination.Length - 1] != '\\')
                @destination = @destination.Trim('"') + "\\";
            string @location = System.Reflection.Assembly.GetEntryAssembly().Location;
            string locerr = @location.Remove(@location.LastIndexOf('\\') + 1) + "log.txt";
            StreamWriter sw = new StreamWriter(new FileStream(locerr, FileMode.Append));
            Parallel.ForEach(Directory.GetFiles(@sourcefile.Remove(@sourcefile.LastIndexOf('\\') + 1),
            @sourcefile.Substring(@sourcefile.LastIndexOf('\\') + 1)), (file) =>
            {
                Stopwatch t = new Stopwatch();
                MemoryStream ms = new MemoryStream();
                FileStream fs = new FileStream(@file, FileMode.Open);
                byte[] buf = new byte[fs.Length];
                fs.Read(buf, 0, buf.Length);
                t.Start();
                using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Compress))
                {
                    ds.Write(buf, 0, buf.Length);
                }
                byte[] f = ms.ToArray();
                t.Stop();
                lock ("log")
                {
                    sw.WriteLine(@"{4}: File '{0}' size of {1:0.00} KB is compressed {2:0.000} times. Compression time - {3:0.000} sec.",
                    file.Substring(file.LastIndexOf('\\') + 1), (double)fs.Length/1024, (double) fs.Length / (double)f.Length, t.Elapsed.TotalSeconds, DateTime.Now);
                }
                t.Reset();
                t.Start();
                FileStream hw = new FileStream(@destination + @file.Substring(@sourcefile.LastIndexOf('\\') + 1) + ".stego", FileMode.Create);
                long state_cur = 0;
                long state_old = 0;
                Console.Write("Completed 0%");
                for (long i = 0; i < f.Length; i++)
                {
                    string stego = Hide(f[i]);
                    for (int j = 0; j < stego.Length; j += 8)
                        hw.WriteByte(Convert.ToByte(stego.Substring(j, 8), 2));
                    state_cur = (long)(i * 100 / f.Length);
                    if (state_cur > state_old)
                    {
                        Console.CursorLeft = 10;
                        Console.Write($"{state_cur}%");
                        state_old = state_cur;
                    }
                }
                hw.Close();
                Console.CursorLeft = 10;
                Console.Write("100%");
                t.Stop();
                lock ("log")
                {
                    sw.WriteLine(@"{3}: File '{0}' is successfully hidden. The size of hidden file is {1:0.00} times larger than the original one. Concealment time - {2:0.000} sec.",
                    file.Substring(file.LastIndexOf('\\') + 1), (double) new FileInfo(@destination + @file.Substring(@sourcefile.LastIndexOf('\\') + 1) + ".stego").Length / (double)fs.Length, t.Elapsed.TotalSeconds, DateTime.Now);
                }
            });
            sw.Close();
        }
        static byte Disclose(string stego)
        {
            MersenneTwister r = new MersenneTwister();
            char[] code = ((byte)r.genrand_Int32()).ToString().PadLeft(3, '0').ToCharArray();
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 10; j++)
                    if (kl[j].All(ki => stego[i * M + ki] == etalons[j, ki]))
                    {
                        code[i] = (char)(j + 48);
                        break;
                    }
            return Convert.ToByte(new string(code));
        }
        static void Disclosefiles(string @keyfile, string @sourcefile, string @destination)
        {
            Stegomask s = new Stegomask(N);
            s.GetEtalons(out etalons);
            FileStream kf = new FileStream(@keyfile, FileMode.Open);
            if (kf.Length > 400) throw new Exception("Invalid key detected.");
            BinaryReader br = new BinaryReader(kf);
            int kiold = 0;
            int ki = br.ReadInt32();
            if (ki > M * 10 - 1) throw new Exception("Invalid key detected.");
            int kic = 0;
            for (int i = 0; i < 10; i++)
            {
                kl[i] = new List<int>();
                for (int j = 0; j < M; j++)
                    if (kic < kf.Length / 4 && ki == (i * M + j))
                    {
                        kiold = ki;
                        kl[i].Add(j);
                        key[i, j] = '1';
                        if (++kic < kf.Length / 4)
                            ki = br.ReadInt32();
                        if (ki > M * 10 - 1 || ki < kiold) throw new Exception("Invalid key detected.");
                    }
                    else
                        key[i, j] = '0';
            }
            kf.Close();
            br.Close();
            if (!sourcefile.Contains("\\"))
                sourcefile = Directory.GetCurrentDirectory() + "\\" + sourcefile;
            if (@destination[@destination.Length - 1] != '\\')
                @destination = @destination.Trim('"') + "\\";
            string @location = System.Reflection.Assembly.GetEntryAssembly().Location;
            string locerr = @location.Remove(@location.LastIndexOf('\\') + 1) + "log.txt";
            StreamWriter sw = new StreamWriter(new FileStream(locerr, FileMode.Append));
            Parallel.ForEach(Directory.GetFiles(@sourcefile.Remove(@sourcefile.LastIndexOf('\\') + 1),
            @sourcefile.Substring(@sourcefile.LastIndexOf('\\') + 1)), (file) =>
            {
                Stopwatch t = new Stopwatch();
                t.Start();
                FileStream f;
                string dir = @destination + @file.Substring(@sourcefile.LastIndexOf('\\') + 1);
                if (file.Contains(".stego"))
                    f = new FileStream(@dir.Remove(@dir.Length - 5), FileMode.Create);
                else
                    f = new FileStream(@dir, FileMode.Create);
                MemoryStream ms = new MemoryStream();
                FileStream fs = new FileStream(@file, FileMode.Open);
                byte[] mb = new byte[(int)fs.Length];
                fs.Read(mb, 0, (int)fs.Length);
                long state_cur = 0;
                long state_old = 0;
                Console.Write("Completed 0%");
                StringBuilder sb = new StringBuilder(M * 3 / 8);
                for (long i = 0; i < mb.Length; i += M * 3 / 8)
                {
                    for (int k = 0; k < M * 3 / 8; k++)
                        sb.Append(Convert.ToString(mb[i + k], 2).PadLeft(8, '0'));
                    ms.WriteByte(Disclose(sb.ToString()));
                    state_cur = (long)(i * 100 / mb.Length);
                    if (state_cur > state_old)
                    {
                        Console.CursorLeft = 10;
                        Console.Write($"{state_cur}%");
                        state_old = state_cur;
                    }
                    sb.Clear();
                }
                int fLength;
                ms.Position = 0;
                byte[] buf = new byte[(int)fs.Length];
                using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Decompress))
                {
                    fLength = ds.Read(buf, 0, (int)fs.Length);
                }
                f.Write(buf, 0, fLength);
                Console.CursorLeft = 10;
                Console.Write("100%");
                f.Close();
                t.Stop();
                lock ("log")
                {
                    sw.WriteLine(@"{2}: File '{0}' is disclosed. Disclosing time - {1:0.000} sec.",
                    file.Substring(file.LastIndexOf('\\') + 1), t.Elapsed.TotalSeconds, DateTime.Now);
                }
            });
            sw.Close();
        }
        static void Main(string[] args)
        {
            try
            {
                if (args.Length < 2)
                    throw new Exception("No arguments specified.");
                switch (args[0])
                {
                    case "-keygen": // Key generation
                        Keygen(args[1]); // Key file destination. For example, d:\dir\key.stego 
                        break;
                    case "-hide": // File(s) hiding
                        Hidefiles(args[1],  // Key file. For example, d:\dir\key.stego  
                                  args[2],  // Source file for hiding. For example, d:\dir\file.txt | d:\dir\*.txt
                                  args[3]); // Hiding file destination. For example, d:\dir\ 
                        break;
                    case "-disclose": // File(s) disclosing
                        Disclosefiles(args[1],  // Key file. For example, d:\dir\key.stego  
                                      args[2],  // Source file for disclosing. For example, d:\dir\file.txt.stego | d:\dir\*.stego
                                      args[3]); // Disclosing file destination. For example, d:\dir\
                        break;
                    default:
                        throw new Exception("Invalid argument specified.");
                }
            }
            catch (Exception exc)
            {
                Console.Write("Error has occurred.");
                string @location = System.Reflection.Assembly.GetEntryAssembly().Location;
                string locerr = @location.Remove(@location.LastIndexOf('\\') + 1) + "errors.txt";
                StreamWriter sw = new StreamWriter(new FileStream(locerr, FileMode.Create));
                sw.WriteLine(exc.ToString());
                sw.Close();
            }
        }
    }
}