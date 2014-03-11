using System;
using System.IO;

// ReSharper disable StringIndexOfIsCultureSpecific.1
// ReSharper disable StringLastIndexOfIsCultureSpecific.1
// ReSharper disable StringIndexOfIsCultureSpecific.2
// ReSharper disable ReplaceWithStringIsNullOrEmpty
namespace Skewworks.NETMF
{
   public class Strings
   {

      /// <summary>
      /// Return absolute path from relative path
      /// </summary>
      /// <param name="baseDirectory"></param>
      /// <param name="target"></param>
      /// <returns></returns>
      public static string AbsolutePath(string baseDirectory, string target)
      {
         // Remove last slash
         if (baseDirectory.Substring(baseDirectory.Length - 1) == "\\")
            baseDirectory = baseDirectory.Substring(0, baseDirectory.Length - 1);

         // Check roots
         if (target.Substring(0, 1) == "\\")
            return target;

         // Remove upline
         while (target.IndexOf("..\\") >= 0)
         {
            int i = baseDirectory.LastIndexOf("\\");

            // Invalid Path
            if (i <= 0)
               return target;

            baseDirectory = baseDirectory.Substring(0, i);
            target = target.Substring(3);
         }

         return NormalizeDirectory(baseDirectory) + target;
      }

      /// <summary>
      /// Ensures the directory ends with a '\' character
      /// </summary>
      /// <param name="path"></param>
      /// <returns></returns>
      public static string NormalizeDirectory(string path)
      {
         if (path.Substring(path.Length - 1) != "\\") return path + "\\";
         return path;
      }

      /// <summary>
      /// Get a relative path for a file or folder
      /// </summary>
      /// <param name="baseDirectory"></param>
      /// <param name="target"></param>
      /// <returns></returns>
      public static string RelativePath(string baseDirectory, string target)
      {
         string sRes = string.Empty;
         int i;

         // Check for file only
         if (target.IndexOf('\\') < 0)
            return target;

         // Split strings
         string[] rel1 = NormalizeDirectory(baseDirectory).Split('\\');
         string[] rel2 = target.Split('\\');

         // Check for different drive
         if (rel1[1].ToLower() != rel2[1].ToLower())
            return target;

         // Find last match
         int s = 0;
         int e = (rel1.Length < rel2.Length) ? rel1.Length : rel2.Length;
         for (i = 2; i < e; i++)
         {
            if (rel1[i].ToLower() != rel2[i].ToLower())
            {
               s = i;
               break;
            }
         }

         // Build upline
         if (s < rel1.Length - 1)
         {
            for (i = s; i < rel1.Length; i++)
            {
               if (rel1[i] != string.Empty)
                  sRes += "..\\";
            }
         }

         // Build downline
         if (s < rel2.Length)
         {
            for (i = s; i < rel2.Length; i++)
               sRes += rel2[i] + "\\";
         }

         // Check for file
         if (Path.GetExtension(target) != string.Empty)
            sRes = sRes.Substring(0, sRes.Length - 1);

         // Return outcome
         return sRes;
      }

      /// <summary>
      /// Finds and replaces occurances within a string
      /// </summary>
      /// <param name="source"></param>
      /// <param name="toFind"></param>
      /// <param name="replaceWith"></param>
      /// <returns></returns>
      public static string Replace(string source, string toFind, string replaceWith)
      {
         int iStart = 0;

         if (source == string.Empty || source == null || toFind == string.Empty || toFind == null)
            return source;

         while (true)
         {
            int i = source.IndexOf(toFind, iStart);
            if (i < 0) break;

            if (i > 0)
               source = source.Substring(0, i) + replaceWith + source.Substring(i + toFind.Length);
            else
               source = replaceWith + source.Substring(i + toFind.Length);

            iStart = i + replaceWith.Length;
         }
         return source;
      }

      public static bool InQuotes(string value, int position)
      {
         int qcount = 0;
         int iStart = 0;

         while (true)
         {
            // Find next instance of a quote
            int i = value.IndexOf('"', iStart);

            // If not return our value
            if (i < 0 || i >= position)
               return qcount % 2 != 0;

            // Check if it's a qualified quote
            if (i > 0 && value.Substring(i, 1) != "\\" || i == 0)
               qcount++;

            iStart = i + 1;
         }
      }

      public static string[] SplitComponents(string value, char deliminator)
      {
         int iStart = 0;
         string[] ret = null;

         while (true)
         {
            // Find deliminator
            int i = value.IndexOf(deliminator, iStart);

            if (InQuotes(value, i))
               iStart = i + 1;
            else
            {
               // Separate value
               string s;
               if (i < 0)
                  s = value;
               else
               {
                  s = value.Substring(0, i).Trim();
                  value = value.Substring(i + 1);
               }

               // Add value
               if (ret == null)
                  ret = new[] { s };
               else
               {
                  var tmp = new string[ret.Length + 1];
                  Array.Copy(ret, tmp, ret.Length);
                  tmp[tmp.Length - 1] = s;
                  ret = tmp;
               }

               iStart = 0;
            }

            // Break on last value
            if (i < 0 || value == string.Empty)
               break;
         }

         return ret;
      }

      public static string[] Tokenize(string command)
      {
         string[] res = SplitComponents(command, ' ');
         for (int i = 0; i < res.Length; i++)
         {
            res[i] = res[i].Trim();
            if (res[i].Substring(0, 1) == "\"" && res[i].Substring(res[i].Length - 1) == "\"")
               res[i] = res[i].Substring(1, res[i].Length - 2);
         }
         return res;
      }

   }
}
// ReSharper restore StringIndexOfIsCultureSpecific.1
// ReSharper restore StringLastIndexOfIsCultureSpecific.1
// ReSharper restore StringIndexOfIsCultureSpecific.2
// ReSharper restore ReplaceWithStringIsNullOrEmpty
