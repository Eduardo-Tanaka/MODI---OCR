using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OcrMODI
{
    // Classe com métodos auxiliares para aplicar o OCR
    class Utils
    {
        // método que remove os acentos da string
        public static string RemoveAccents(string text)
        {
            try
            {
                StringBuilder sbReturn = new StringBuilder();
                var arrayText = text.Normalize(NormalizationForm.FormD).ToCharArray();
                foreach (char letter in arrayText)
                {
                    if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
                        sbReturn.Append(letter);
                }
                return sbReturn.ToString();
            }
            catch (Exception ex)
            {
                return text;
            }
        }

        // método que recebe uma string e retorna apenas os números
        public static string RetornaApenasNumero(string s)
        {
            string numero = "";
            try
            {
                numero = String.Join("", Regex.Split(s, @"[^\d]"));
                return numero;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
    }
}
