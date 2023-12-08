#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server;

#endregion

namespace Nelderim
{
	public static class Translate
	{
		public readonly record struct TranslationResult(Language lang, string[] originalWords, string[] translatedWords);
		
		public static TranslationResult Apply(string original, Language lang)
		{
			var originalWords = original.Split(' ');
			var translatedWords = lang switch
			{
				Language.Krasnoludzki => TranslateUsingDict(originalWords, LanguagesDictionary.Krasnoludzki),
				Language.Elficki => TranslateUsingDict(originalWords, LanguagesDictionary.Elficki),
				Language.Drowi => TranslateUsingDict(originalWords, LanguagesDictionary.Drowi),
				Language.Jarlowy => TranslateUsingDict(originalWords, LanguagesDictionary.Jarlowy),
				Language.Demoniczny => TranslateUsingList(originalWords, LanguagesDictionary.Demoniczny),
				Language.Orkowy => TranslateUsingDict(originalWords, LanguagesDictionary.Orkowy),
				Language.Nieumarlych => TranslateUsingList(originalWords, LanguagesDictionary.Nieumarlych),
				Language.Powszechny => RandomWord(originalWords),
				Language.Belkot => RandomWord(originalWords),
				_ => null
			};

			return new TranslationResult(lang, originalWords, translatedWords);
		}

		public static string Combine(TranslationResult tr, Mobile from, Mobile to)
		{
			if (to.IsStaff() || from == to)
				return $"[{from.LanguageSpeaking}] {string.Join(" ", tr.originalWords)}";
			
			var fromLangValue = from.LanguagesKnown[tr.lang];
			var toLangValue = to.LanguagesKnown[tr.lang];
			if (tr.originalWords.Length != tr.translatedWords.Length)
			{
				Console.WriteLine("Translated words doesn't match original words!");
				Console.WriteLine($"original {tr.originalWords}");
				Console.WriteLine($"translated {tr.translatedWords}");
			}
			StringBuilder sb = new StringBuilder();
			if (toLangValue >= 500 && fromLangValue >= 500)
			{
				sb.Append($"[{from.LanguageSpeaking}]");
			}
			for (var index = 0; index < tr.originalWords.Length; index++)
			{
				sb.Append(" ");
				var originalWord = tr.originalWords[index];
				var translatedWord = tr.translatedWords[index];
				if (Math.Abs(originalWord.GetHashCode() % 1000) < fromLangValue && Math.Abs(translatedWord.GetHashCode() % 1000) < toLangValue)
				{
					sb.Append(originalWord);
				}
				else
				{
					sb.Append(translatedWord);
				}
			}
			return sb.ToString();
		}

		private static char[] _Alphabet = "abcdefghijklmnopqrstuvwxyz".ToArray();

		private static string[] RandomWord(string[] originalWords)
		{
			var result = new string[originalWords.Length];
			for (var index = 0; index < originalWords.Length; index++)
			{
				var word = originalWords[index];
				var newWord = new StringBuilder();
				foreach (var c in word)
				{
					var newChar = Utility.RandomList(_Alphabet);
					newWord.Append(Char.IsUpper(c) ? Char.ToUpper(newChar) : newChar);
				}

				result[index] = newWord.ToString();
			}
			return result;
		}

		private static string[] TranslateUsingDict(string[] originalWords, Dictionary<string, string> dict)
		{
			var translatedWords = new string[originalWords.Length];
			for (var index = 0; index < originalWords.Length; index++)
			{
				var word = originalWords[index].ToLower();
				translatedWords[index] = dict.TryGetValue(word, out var value)
					? value
					: dict.ElementAt(Math.Abs(word.GetHashCode()) % dict.Count).Value;

				if (Char.IsUpper(originalWords[index][0]))
				{
					translatedWords[index] = CapitalizeFirstLetter(translatedWords[index]);
				}
			}
			return translatedWords;
		}

		private static string[] TranslateUsingList(string[] originalWords, List<string> list)
		{
			var translatedWords = new string[originalWords.Length];
			for (var index = 0; index < originalWords.Length; index++)
			{
				translatedWords[index] = list[(Math.Abs(originalWords[index].ToLower().GetHashCode()) % list.Count)];
				if (Char.IsUpper(originalWords[index][0]))
				{
					translatedWords[index] = CapitalizeFirstLetter(translatedWords[index]);
				}
			}
			return translatedWords;
		}

		private static string CapitalizeFirstLetter(string text)
		{
			return text.Remove(0, 1).Insert(0, Char.ToUpper(text[0]).ToString());
		}
	}
}
