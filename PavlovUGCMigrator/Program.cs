using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace PavlovUGCMigrator
{
    class Program
    {
        public static IEnumerable<string> GetFileList(string rootFolderPath)
        {
            Queue<string> pending = new Queue<string>();
            pending.Enqueue(rootFolderPath);
            string[] tmp;
            while (pending.Count > 0)
            {
                rootFolderPath = pending.Dequeue();
                try
                {
                    tmp = Directory.GetFiles(rootFolderPath);
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }
                for (int i = 0; i < tmp.Length; i++)
                {
                    yield return tmp[i];
                }
                tmp = Directory.GetDirectories(rootFolderPath);
                for (int i = 0; i < tmp.Length; i++)
                {
                    pending.Enqueue(tmp[i]);
                }
            }
        }
        static public List<int> SearchBytePattern(byte[] pattern, byte[] bytes)
        {
            List<int> positions = new List<int>();
            int patternLength = pattern.Length;
            int totalLength = bytes.Length;
            byte firstMatchByte = pattern[0];
            for (int i = 0; i < totalLength; i++)
            {
                if (firstMatchByte == bytes[i] && totalLength - i >= patternLength)
                {
                    byte[] match = new byte[patternLength];
                    Array.Copy(bytes, i, match, 0, patternLength);
                    if (match.SequenceEqual<byte>(pattern))
                    {
                        positions.Add(i);
                        i += patternLength - 1;
                    }
                }
            }
            return positions;
        }

        static void Main(string[] args)
        {
            Console.Write("\nPavlovUGCMigrator written by Nan.\nPlease report bugs to superdirtlover99@gmail.com (yes, really).");

            string migrateUGC = "f";
            string destinationUGC = "F";

            //Populate MigrateUGC if given arg, otherwise ask
            if (args == null || args.Length == 0)
            {
                Console.Write("\n\nPlease enter the filepath to your Migrated UGC folder:\n");
                migrateUGC = Console.ReadLine().Replace("\"", ""); ;

            }
            else
            {
                migrateUGC = args[0];
            }
            Console.Write("\nPlease enter the filepath to your Destination UGC folder:\n");
            destinationUGC = Console.ReadLine().Replace("\"", ""); ;
            //Console.Write("\n" + migrateUGC + "\n" + migrateUGC + "\n");

            //Get UGCs from filepaths
            string migrateID = migrateUGC.Substring(migrateUGC.Length - 13);
            string destinationID = destinationUGC.Substring(migrateUGC.Length - 13);

            Console.Write("Looks like your IDs are "+migrateID+" and "+destinationID+".\n");
            Console.Write("Starting to process files. This may take a few minutes.\n");

            //Create list of files
            IEnumerable<string> query = GetFileList(migrateUGC);
            string[] filepaths = query.Cast<string>().ToArray();

            //Iterate through every file, replacing instances of the migrated UGC ID with the new one,
            //then save that file to the destination UGC folder
            byte[] migrateIdBytes = Encoding.ASCII.GetBytes(migrateID);
            byte[] destinationIdBytes = Encoding.ASCII.GetBytes(destinationID);

            for (int i = 0; i < filepaths.Length; i++)
            {
                //Create byte array of current file
                FileStream stream = File.OpenRead(filepaths[i]);
                byte[] fileBytes = new byte[stream.Length];
                stream.Read(fileBytes, 0, fileBytes.Length);
                stream.Close();

                //Find instances of old ID and replace with new
                List<int> positions = SearchBytePattern(migrateIdBytes, fileBytes);
                for (int j=0; j < positions.Count; j++)
                {
                    for (int k = 0; k < destinationIdBytes.Length; k++)
                    {
                        fileBytes[positions[j]+k] = destinationIdBytes[k];
                    }
                }

                //Save byte array file to destination folder
                string savePath = filepaths[i].Replace(migrateID,destinationID);
                string saveFolder = savePath.Substring(0, savePath.LastIndexOf('\\'));
                DirectoryInfo di = Directory.CreateDirectory(saveFolder);
                File.WriteAllBytes(savePath, fileBytes);
            }
            Console.Write("\nProcessing complete.");
            Console.ReadLine();

        }
    }
}
