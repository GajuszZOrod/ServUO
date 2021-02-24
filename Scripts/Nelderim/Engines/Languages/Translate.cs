using System;
using Server.Network;
using Server.Mobiles;
using Server;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Nelderim
{
	public class Translate
	{
		public static void Initialize()
		{
			// Register our speech handler
			EventSink.Speech += new SpeechEventHandler( EventSink_Speech );
		}
		
		private static void EventSink_Speech( SpeechEventArgs args )
		{
			PlayerMobile from = args.Mobile as PlayerMobile;
			string mySpeech = args.Speech;
 			if (from == null || mySpeech == null || args.Type == MessageType.Emote || from.LanguageSpeaking == Language.Powszechny) {
				return;
			}
			
			args.Blocked = true;
			int tileLength = 15;
			
			switch ( args.Type )
			{
				case MessageType.Yell:
					tileLength = 18;
					break;
				case MessageType.Whisper:
					tileLength = 1;
					break;
			}

			foreach (Mobile m in from.Map.GetMobilesInRange(from.Location, tileLength))
			{
				String mySpeechTranslated = "";
				if (m.Player) {
					PlayerMobile pm = m as PlayerMobile;

					if (pm.LanguagesKnown.Get(from.LanguageSpeaking) || from == m) {
						from.SayTo(pm, String.Format("[{0}] ", from.LanguageSpeaking.ToString()) + mySpeech);
					} else {
						
						switch (from.LanguageSpeaking) {
							case Language.Krasnoludzki: mySpeechTranslated = TranslateUsingDict(mySpeech,LanguagesDictionary.Krasnoludzki); break;
							case Language.Elficki: mySpeechTranslated = TranslateUsingDict(mySpeech, LanguagesDictionary.Elficki); break;
							case Language.Drowi: mySpeechTranslated = TranslateUsingDict(mySpeech, LanguagesDictionary.Drowi); break;
							case Language.Jarlowy: mySpeechTranslated = TranslateUsingDict(mySpeech, LanguagesDictionary.Jarlowy); break;
							case Language.Demoniczny: mySpeechTranslated = TranslateUsingWordsList(mySpeech, LanguagesDictionary.Demoniczny); break;
							case Language.Orkowy: mySpeechTranslated = TranslateUsingDict(mySpeech, LanguagesDictionary.Orkowy); break;
							case Language.Nieumarlych: mySpeechTranslated = TranslateUsingSentencesList(LanguagesDictionary.Nieumarlych); break;
							case Language.Belkot: mySpeechTranslated = TranslateUsingWordsList(mySpeech, LanguagesDictionary.Belkot); break;
						}
						from.SayTo(pm, mySpeechTranslated);
					}
				} else {
					m.OnSpeech(args);
				}			
			}			
		}

		private static Random random = new Random();
		public static String RandomWord(int length) {
			const string chars = "abcdefghijklmnopqrstuvwxyz";
			return new string(Enumerable.Repeat(chars, length)
			  .Select(s => s[random.Next(s.Length)]).ToArray());
		}

		public static String TranslateUsingDict(String speech, Dictionary<String, String> dict) {
			string translatedWord;
			StringBuilder sb = new StringBuilder(speech.Length);
			foreach (string word in speech.Split(' ')) {
				if (word.StartsWith("*")) {
					translatedWord = word.Substring(1);
				} else if (dict.ContainsKey(word.ToLower())) {
					translatedWord = dict[word.ToLower()];
				} else {
					translatedWord = dict.ElementAt(random.Next(dict.Count)).Value;
				}
				if (translatedWord.Length > 0 && Char.IsUpper(word[0])) {
					char upperChar = Char.ToUpper(translatedWord[0]);
					sb.Append(upperChar);
					sb.Append(translatedWord.Substring(1));
				} else {
					sb.Append(translatedWord);
				}
				sb.Append(" ");
			}
			return sb.ToString(); ;
		}

		public static String TranslateUsingWordsList(String speech, List<String> list) {
			string translatedWord;
			StringBuilder sb = new StringBuilder(speech.Length);
			foreach (string word in speech.Split(' ')) {
				if (word.StartsWith("*")) {
					sb.Append(word);
				} else {
					translatedWord = list[random.Next(list.Count)];
					if (translatedWord.Length > 0 && Char.IsUpper(word[0])) {
						char upperChar = Char.ToUpper(translatedWord[0]);
						sb.Append(upperChar);
						sb.Append(translatedWord.Substring(1));
					} else {
						sb.Append(translatedWord);
					}	
				}
				sb.Append(" ");
			}
			return sb.ToString(); ;
		}

		public static String TranslateUsingSentencesList(List<String> list) {
			return list[random.Next(list.Count)];

		}
	}
}
