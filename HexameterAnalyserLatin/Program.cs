//Console.Write("Latin sentence: ");
//string sentence = Console.ReadLine();

using System.Diagnostics.SymbolStore;

string text = "Contigerat nostras infamia temporis aures\n";
text += "quam cupiens falsam summo delabor olympo\n";
text += "et deus humana iustro sub imagine terras\n";
text += "longa mora est quantum noxae sit ubique repertum\n";
text += "enumerare minor fuit ipsa infamia vero\n";
text += "maenala transieram latebris horrenda ferarum\n";
text += "et cum cyllene gelidi pineta lycaei\n";
text += "arcadis hinc sedes et inhospita tecta tyranni\n";
text += "ingredior traherent cum sera crepuscula noctem";
//ProcessSentence("longa mora est quantum noxae sit ubique repertum");
int valid = 0;
int total = 0;
foreach (string vers in text.Split('\n'))
{
    total++;
    valid += ProcessSentence(vers) ? 1 : 0;
}
Console.WriteLine("\n\n" + valid + " / " + total + " = " + (valid / (float)total * 100) + "% valid");

//OutputVerse(GetSyllables("aspirate meis primaque ab origine mundi"));
//OutputVerse(GetSyllables("corpora di coeptis nam vos mutastis et illas"));

static List<Syllable> GetSyllables(string sentence)
{
    Console.WriteLine("Latin sentence: " + sentence);
    List<string> words = sentence.Split(' ').ToList();
    
    // Hiatvermeidung: Wenn ein Wort mit einem Vokal endet und das nächste Wort mit einem Vokal beginnt, wird der Vokal des letzeren Wortes entfernt:
    List<int> wordsWithHiat = new List<int>();
    for (int i = 0; i < words.Count - 1; i++)
    {
        if (words[i].Length > 0 && words[i + 1].Length > 0 &&
            IsLatinVowelSound(words[i][^1]) && IsLatinVowelSound(words[i + 1][0]) &&
            !(words[i + 1][0] == 'i' && IsLatinVowelSound(words[i + 1][1]) && words[i + 1][1] != 'i'))
        {
            // Remove the last letter of the current word
            words[i] = words[i].Substring(0, words[i].Length - 1) + words[i + 1];
            words[i + 1] = "";
        }
        if (words[i].Length > 0 && words[i + 1].Length > 1 &&
            IsLatinVowelSound(words[i][^1]) && IsLatinVowelSound(words[i + 1][1]) && words[i + 1][0] == 'h')
        {
            // Remove the last letter of the current word
            words[i] = words[i].Substring(0, words[i].Length - 1) + words[i + 1].Substring(1);
            words[i + 1] = "";
        }
    }

    for (int i = 0; i < words.Count; i++)
    {
        if (words[i] == "")
        {
            words.RemoveAt(i);
            i--;
        }
    }

    // Silbentrennung
    List<Syllable> syllables = new();
    foreach (string word in words)
    {
        List<string> wordSyllables = SplitLatinIntoSyllables(word);
        wordSyllables.RemoveAll(x => x == "");
        for (int i = 0; i < wordSyllables.Count; i++)
        {
            //Console.WriteLine(wordSyllables[i]);
            syllables.Add(new Syllable(wordSyllables[i], i == 0, i == wordSyllables.Count - 1));
        }
    }

    // Handle edge cases
    for(int i = 0; i < syllables.Count - 1; i++)
    {
        // -qu-a- => -qua-
        if(syllables[i].syllable.EndsWith("qu") && syllables[i + 1].syllable.StartsWith("a"))
        {
            syllables[i].syllable += "a";
            syllables[i + 1].syllable = syllables[i + 1].syllable.Substring(1, syllables[i + 1].syllable.Length - 1);
        }
        // -o-e-, -a-e- => -oe-, -ae-
        if((syllables[i].syllable.EndsWith("o") || syllables[i].syllable.EndsWith("a")) && syllables[i + 1].syllable.StartsWith("e"))
        {
            syllables[i].syllable += "e";
            syllables[i + 1].syllable = syllables[i + 1].syllable.Substring(1, syllables[i + 1].syllable.Length - 1);
        }
        // -i-u- => -iu-, -ae-
        if(syllables[i].syllable.EndsWith("i") && syllables[i + 1].syllable.StartsWith("u"))
        {
            syllables[i].syllable += "u";
            syllables[i + 1].syllable = syllables[i + 1].syllable.Substring(1, syllables[i + 1].syllable.Length - 1);
        }
        // o-ly- => oly-
        if(syllables[i].syllable == "o" && syllables[i].isWordBeginning && syllables[i + 1].syllable.StartsWith("ly"))
        {
            syllables[i].syllable += "ly";
            syllables[i + 1].syllable = syllables[i + 1].syllable.Substring(2, syllables[i + 1].syllable.Length - 2);
        }
    }
    // Merge syllables without vowel with previous syllable
    for (int i = 0; i < syllables.Count - 1; i++)
    {
        bool hasVowel = false;
        foreach (char c in syllables[i].syllable)
        {
            if (IsLatinVowelSound(c)) hasVowel = true;
        }
        if(!hasVowel)
        {
            syllables[i - 1].syllable += syllables[i].syllable;
            syllables[i].syllable = "";
            if (syllables[i].isWordEnding) syllables[i - 1].isWordEnding = true;
        }
    }
    // Remove empty syllables
    for (int i = 0; i < syllables.Count; i++)
    {
        if (syllables[i].syllable == "")
        {
            syllables.RemoveAt(i);
            i--;
        }
    }
    return syllables;
}

static bool ProcessSentence(string sentence)
{
    List<Syllable> syllables = GetSyllables(sentence);

    // VersmaßAnalyse
    syllables[0].length = Length.Long;
    syllables[^1].length = Length.Anceps;
    syllables[^2].length = Length.Long;
    syllables[^3].length = Length.Short;
    syllables[^4].length = Length.Short;
    syllables[^5].length = Length.Long;

    syllables[0].isVerseBeginning = true;
    syllables[0].isCertain = true;
    syllables[^1].isCertain = true;
    syllables[^2].isCertain = true;
    syllables[^2].isVerseBeginning = true;
    syllables[^3].isCertain = true;
    syllables[^4].isCertain = true;
    syllables[^5].isVerseBeginning = true;
    syllables[^5].isCertain = true;

    

    syllables = Positionslänge(syllables);
    
    // Sicherheit geht vor
    syllables = FaustRegeln(syllables, FaustRegelCertainty.Sicher);
    syllables = VersmaßAnalyse(syllables);
    syllables = VersmaßAnalyse(syllables);
    if (!IsValid(FillRest(syllables)))
    {
        // Außer das Ding hat keine Ahnung mehr
        syllables = FaustRegeln(syllables, FaustRegelCertainty.HilfeIchVerzweifel);
        syllables = VersmaßAnalyse(syllables);
        syllables = VersmaßAnalyse(syllables);
    }

    // Fill rest
    syllables = FillRest(syllables);
    Console.WriteLine();
    Console.WriteLine("Versmaß: ");
    OutputVerse(syllables);
    Console.WriteLine("Is valid: " + IsValid(syllables) + "\n\n");
    return IsValid(syllables);
}

static List<Syllable> FillRest(List<Syllable> syllables)
{
    int followingShort = 0;
    for (int i = 0; i < syllables.Count; i++)
    {
        if (followingShort == 0 && syllables[i].length == Length.Long)
        {
            followingShort = 2;
        }
        else if(followingShort > 0 && syllables[i].length == Length.Long)
        {
            followingShort = 0;
        }
        if(followingShort <= 0 && syllables[i].length != Length.Long && !syllables[i].isCertain)
        {
            syllables[i].length = Length.Long;
            syllables[i].isCertain = true;
            syllables[i].isVerseBeginning = true;
            followingShort = 2;
        }
        if(syllables[i].length == Length.Uncertain)
        {
            syllables[i].length = Length.Short;
        }
        if (syllables[i].length != Length.Long)
        {
            followingShort--;
        }
    }

    return syllables;
}

static List<Syllable> FaustRegeln(List<Syllable> syllables, FaustRegelCertainty certainty)
{
    // Faustregeln anwenden
    for (int i = 1; i < syllables.Count - 5; i++)
    {
        if (syllables[i].isCertain) continue;
        if (certainty <= FaustRegelCertainty.Sicher)
        {
            // Falls Silbe auf -i/-o/-u endet oder auf -s aber nicht auf -ibus, lang
            if (syllables[i].isWordEnding &&
                (syllables[i].syllable.EndsWith("s") && !syllables[i].syllable.EndsWith("ibus")
                 || syllables[i].syllable.EndsWith("i") || syllables[i].syllable.EndsWith("o") ||
                 syllables[i].syllable.EndsWith("u")))
            {
                syllables[i].length = Length.Long;
                syllables[i].isCertain = true;
            }
        }
        
        // Falls Silbe auf -a/-e endet, kurz
        if (certainty <= FaustRegelCertainty.HilfeIchVerzweifel)
        {
            if (syllables[i].isWordEnding &&
                (syllables[i].syllable.EndsWith("a") || syllables[i].syllable.EndsWith("e")))
            {
                syllables[i].length = Length.Short;
                syllables[i].isCertain = true;
            }
        }

        if (certainty <= FaustRegelCertainty.SehrSicher)
        {
            // Silben mit zwei Vokalen am Ende lang
            if(syllables[i].syllable.Length > 1 && isVowel(syllables[i].syllable[^1]) && isVowel(syllables[i].syllable[^2]))
            {
                syllables[i].length = Length.Long;
                syllables[i].isCertain = true;
            }
            else if(certainty <= FaustRegelCertainty.Sicher)
            {
                // Silben mit zwei Vokalen nicht am Ende kurz
                int vowels = 0;
                foreach (char c in syllables[i].syllable)
                {
                    if(isVowel(c)) vowels++;
                }

                if (vowels > 1)
                {
                    syllables[i].length = Length.Short;
                    syllables[i].isCertain = true;
                }
            }
        }
    }

    return syllables;
}

static List<Syllable> Positionslänge(List<Syllable> syllables)
{
    for (int i = 0; i < syllables.Count; i++)
    {
        // Positionslänge, Vokal wenn gefolgt von 2 Konsonanten, die nicht h sind, silbe lang
        if (i + 1 < syllables.Count)
        {
            string toAnalyse = syllables[i].syllable + syllables[i + 1].syllable;
            int consonantCount = 0;
            bool gotVowel = false;
            for (int k = 0; k < toAnalyse.Length; k++)
            {
                if (!IsLatinVowelSound(toAnalyse[k]) && gotVowel && toAnalyse[k] != 'h')
                {
                    consonantCount++;
                }
                if (IsLatinVowelSound(toAnalyse[k]) && (k == toAnalyse.Length - 1 || !IsLatinVowelSound(toAnalyse[k + 1])))
                {
                    gotVowel = !gotVowel;
                }
            }

            if (consonantCount >= 2)
            {
                syllables[i].length = Length.Long;
                syllables[i].isCertain = true;
            }
        }
    }

    return syllables;
}


static List<Syllable> VersmaßAnalyse(List<Syllable> syllables)
{
    // VersmaßAnalyse
    //for(int i = 0; i < syllables.Count; i++)
    for(int i = syllables.Count - 1; i >= 0; i--)
    {
        if (i == 0)
        {
            // First syllable only
            
            // On _ * u
            // Do _ u u
            if (syllables[i].length == Length.Long && syllables[i].isCertain
                                                   && !syllables[i + 1].isCertain
                                                   && syllables[i + 2].length == Length.Short &&
                                                   syllables[i + 2].isCertain)
            {
                syllables[i + 1].length = Length.Short;
                syllables[i + 1].isCertain = true;
                syllables[i].isVerseBeginning = true;
            }
        }
        if (i + 3 < syllables.Count)
        {
            // On _ u * *
            // Do _ u u _
            if(syllables[i].length == Length.Long && syllables[i].isCertain 
                                                  && syllables[i + 1].length == Length.Short && syllables[i + 1].isCertain
                                                  && !syllables[i + 2].isCertain
                                                  && !syllables[i + 3].isCertain)
            {
                syllables[i].isVerseBeginning = true;
                syllables[i + 2].length = Length.Short;
                syllables[i + 2].isCertain = true;
                syllables[i + 3].length = Length.Long;
                syllables[i + 3].isCertain = true;
                syllables[i + 3].isVerseBeginning = true;
            }
            /*
            // On _ * * u
            // Do _ _ _ u
            if(syllables[i].length == Length.Long && syllables[i].isCertain 
                                                  && !syllables[i + 1].isCertain
                                                  && !syllables[i + 2].isCertain
                                                  && syllables[i + 3].length == Length.Short && syllables[i + 3].isCertain)
            {
                syllables[i].isVerseBeginning = true;
                syllables[i + 1].length = Length.Long;
                syllables[i + 1].isCertain = true;
                syllables[i + 2].length = Length.Long;
                syllables[i + 2].isCertain = true;
                syllables[i + 2].isVerseBeginning = true;
            }
            */
            // On _ u u *
            // Do _ u u _
            if(syllables[i].length == Length.Long && syllables[i].isCertain 
                                                  && syllables[i + 1].length == Length.Short && syllables[i + 1].isCertain
                                                  && syllables[i + 2].length == Length.Short && syllables[i + 2].isCertain
                                                  && !syllables[i + 3].isCertain)
            {
                syllables[i].isVerseBeginning = true;
                syllables[i + 3].length = Length.Long;
                syllables[i + 3].isCertain = true;
                syllables[i + 3].isVerseBeginning = true;
            }
            // On * _ _ u
            // Do _ _ _ u
            if(!syllables[i].isCertain
               && syllables[i + 1].length == Length.Long && syllables[i + 1].isCertain
               && syllables[i + 2].length == Length.Long && syllables[i + 2].isCertain
               && syllables[i + 3].length == Length.Short && syllables[i + 3].isCertain)
            {
                syllables[i].length = Length.Long;
                syllables[i].isCertain = true;
                syllables[i].isVerseBeginning = true;
                syllables[i + 2].isVerseBeginning = true;
            }
        }
        if (i + 2 < syllables.Count)
        {
            // On _ * _
            // Do _ _ _
            if(syllables[i].length == Length.Long && syllables[i].isCertain 
                                                  && !syllables[i + 1].isCertain
                                                  && syllables[i + 2].length == Length.Long && syllables[i + 2].isCertain)
            {
                syllables[i + 1].length = Length.Long;
                syllables[i + 1].isCertain = true;
            }
            // On _ u *
            // Do _ u u
            if(syllables[i].length == Length.Long && syllables[i].isCertain 
                                                  && syllables[i + 1].length == Length.Short && syllables[i + 1].isCertain
                                                  && !syllables[i + 2].isCertain)
            {
                syllables[i].isVerseBeginning = true;
                syllables[i + 2].length = Length.Short;
                syllables[i + 2].isCertain = true;
            }
            // On * u u
            // Do _ u u
            if(!syllables[i].isCertain 
                && syllables[i + 1].length == Length.Short && syllables[i + 1].isCertain
                && syllables[i + 2].length == Length.Short && syllables[i + 2].isCertain)
            {
                syllables[i].isVerseBeginning = true;
                syllables[i].length = Length.Long;
                syllables[i].isCertain = true;
            }
            // On _ _ _
            //        ^
            // Do _ _ _
            //    ^   ^
            if(syllables[i].length == Length.Long && syllables[i].isCertain 
                                                  && syllables[i + 1].length == Length.Long && syllables[i + 1].isCertain
                                                  && syllables[i + 2].length == Length.Long && syllables[i + 2].isCertain && syllables[i + 2].isVerseBeginning)
            {
                syllables[i].isVerseBeginning = true;
            }
            // On * u _
            // Do u u _
            if(!syllables[i].isCertain 
                && syllables[i + 1].length == Length.Short && syllables[i + 1].isCertain
                && syllables[i + 2].length == Length.Long && syllables[i + 2].isCertain)
            {
                syllables[i].length = Length.Short;
                syllables[i].isCertain = true;
                syllables[i + 2].isVerseBeginning = true;
            }
        }
    }
    return syllables;
}


static bool IsValid(List<Syllable> syllables)
{
    int followingShort = 0;
    for (int i = 0; i < syllables.Count; i++)
    {
        if (followingShort == 0 && syllables[i].length == Length.Long)
        {
            followingShort = 2;
        }
        else if(followingShort > 0 && syllables[i].length == Length.Long)
        {
            followingShort = 0;
        }

        if (i > 1 && syllables[i].length == Length.Long)
        {
            if (syllables[i - 1].length == Length.Short && syllables[i - 2].length != Length.Short)
            {
                
                Console.WriteLine("Issue at " + i);
                return false;
            }
        }
        if(followingShort <= 0 && syllables[i].length != Length.Long)
        {
            Console.WriteLine("Issue at " + i);
            return false;
        }
        if (syllables[i].length != Length.Long)
        {
            followingShort--;
        }
    }

    return true;
}


static void OutputVerse(List<Syllable> syllables)
{
    for (int i = 0; i < syllables.Count; i++)
    {
        Console.Write(syllables[i].syllable);
        if (i < syllables.Count - 1)
        {
            if (syllables[i].isWordEnding && syllables[i + 1].isWordBeginning) Console.Write(" ");
            else Console.Write("-");
        }
    }
    Console.Write("\n");
    for (int i = 0; i < syllables.Count; i++)
    {
        Console.Write((char)syllables[i].length + new String(' ', syllables[i].syllable.Length - 1));
        if (i < syllables.Count - 1)
        {
            if (syllables[i].isWordEnding && syllables[i + 1].isWordBeginning) Console.Write(" ");
            else Console.Write(" ");
        }
    }
    
    // Is verse beginning
    Console.Write("\n");
    for (int i = 0; i < syllables.Count; i++)
    {
        Console.Write((syllables[i].isVerseBeginning ? "^" : " ") + new String(' ', syllables[i].syllable.Length - 1));
        if (i < syllables.Count - 1)
        {
            if (syllables[i].isWordEnding && syllables[i + 1].isWordBeginning) Console.Write(" ");
            else Console.Write(" ");
        }
    }
    
    // Is certain
    Console.Write("\n");
    for (int i = 0; i < syllables.Count; i++)
    {
        Console.Write((syllables[i].isCertain ? "." : " ") + new String(' ', syllables[i].syllable.Length - 1));
        if (i < syllables.Count - 1)
        {
            if (syllables[i].isWordEnding && syllables[i + 1].isWordBeginning) Console.Write(" ");
            else Console.Write(" ");
        }
    }
    Console.WriteLine();
}

static List<string> SplitLatinIntoSyllables(string word)
{
    List<string> syllables = new List<string>();
    int index = 0;
    string syllable = "";

    while (index < word.Length)
    {
        // Add the current letter to the current syllable
        syllable += word[index];

        // If the current letter is a vowel sound and the previous letter is not, start a new syllable
        if (IsLatinVowelSound(word[index]) && (index == 0 || !IsLatinVowelSound(word[index - 1])))
        {
            syllables.Add(syllable);
            syllable = "";
        }

        /*
        if (syllables.Count > 0 && index > 0&& IsLatinVowelSound(word[index]) && IsLatinVowelSound(word[index - 1]))
        {
            syllable = syllable.Substring(0, syllable.Length - 1);
            syllables[^1] += word[index];
        }
*/
        index++;
    }

    // Add any remaining letters to the last syllable
    if (syllable != "")
    {
        syllables.Add(syllable);
    }

    bool hasVowel = false;
    foreach (char c in syllables[^1])
    {
        if (IsLatinVowelSound(c)) hasVowel = true;
    }
    if (!hasVowel)
    {
        syllables[^2] += syllables[^1];
        syllables.RemoveAt(syllables.Count - 1);
    }

    return syllables;
}

static bool isVowel(char letter)
{
    string vowels = "aeiouAEIOU";
    return vowels.Contains(letter);
}

static bool IsLatinVowelSound(char letter)
{
    string vowels = "aeiouyAEIOUY";
    return vowels.Contains(letter);
}

public class Syllable
{
    public string syllable = "";
    public bool isWordEnding = false;
    public bool isWordBeginning = false;
    public Length length = Length.Uncertain;
    public bool isCertain = false;
    
    public bool isVerseBeginning = false;
    
    
    public Syllable(string syllable, bool isWordBeginning, bool isWordEnding)
    {
        this.syllable = syllable;
        this.isWordBeginning = isWordBeginning;
        this.isWordEnding = isWordEnding;
    }
}
public enum FaustRegelCertainty
{
    SehrSicher = 2,
    Sicher = 1,
    HilfeIchVerzweifel = 0
}
public enum Length
{
    Long = '_',
    Short = 'u',
    Anceps = 'x',
    Uncertain = '*'
}