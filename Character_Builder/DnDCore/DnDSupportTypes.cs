using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;

namespace DnDSupportTypes
{
    public static class DnDCore
    {
        public static bool loaded = false;
        public static List<DnDRace> Races = new List<DnDRace>();
        public static List<DnDClass> Classes = new List<DnDClass>();
        public static List<DnDFeature> Features = new List<DnDFeature>();
        public static List<string> Attributes = new List<string>();
        public static List<int> StartingScores = new List<int>();

        public static List<string> Names_Male = new List<string>();
        public static  List<string> Names_Female = new List<string>();
        public static  List<string> Names_Last = new List<string>();

        static DnDCore()
        {
            initFeatures();
            initRaces();
            initClasses();
            initNames();

            Attributes = new List<string>() { "Str", "Dex", "Con", "Int", "Wis", "Cha" };
            StartingScores = new List<int>() { 15, 14, 13, 12, 10, 8 };


            // this has to be the last line!
            loaded = true;
        }

        private static void initClasses()
        {
            try
            {
                using (StreamReader sr = new StreamReader("DnDCore\\Classes.json"))
                {
                    var classJSON = JsonConvert.DeserializeObject<List<DnDClass>>(sr.ReadToEnd());

                    foreach (var r in classJSON)
                    {
                        Classes.Add(r);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private static void initRaces()
        {
            try
            {
                using (StreamReader sr = new StreamReader("DnDCore\\Races.json"))
                {
                    var raceJSON = JsonConvert.DeserializeObject<List<DnDRace>>(sr.ReadToEnd());

                    foreach (var r in raceJSON.Where(z => !z.Name.Contains("base")))
                    {
                        Races.Add(r);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private static void initFeatures()
        {
            try
            {
                using (StreamReader sr = new StreamReader("DnDCore\\Features.json"))
                {
                    var featureJSON = JsonConvert.DeserializeObject<List<DnDFeature>>(sr.ReadToEnd());

                    foreach (var f in featureJSON)
                    {
                        Features.Add(f);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private static void initNames()
        {
            try
            {
                using (StreamReader sr = new StreamReader("DnDCore\\Names.json"))
                {
                    var NamesJSON = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(sr.ReadToEnd());

                    foreach (var n in NamesJSON["Male"])
                    {
                        Names_Male.Add(n);
                    }
                    foreach (var n in NamesJSON["Female"])
                    {
                        Names_Female.Add(n);
                    }
                    foreach (var n in NamesJSON["Last"])
                    {
                        Names_Last.Add(n);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }

    public class DnDRace
    {
        public string Name;
        public string Size;
        public int Speed;
        public List<string> Languages = new List<string>();
        public List<DnDFeature> RacialFeatures = new List<DnDFeature>();
        public Dictionary<string, int> AbilityChanges = new Dictionary<string, int>();

        public DnDRace(string name, string size, int speed, List<string> languages, List<string> racialFeatures)
        {
            Name = name;
            Size = size;
            Speed = speed;
            Languages = languages.ToList();

            foreach (string thing in racialFeatures)
            {
                RacialFeatures.Add(DnDCore.Features.Find(z => z.Name.Equals(thing)));
            }
        }

        public override string ToString()
        {
            string race = $"Race: {Name}\nSize: {Size}\nSpeed: {Speed}\nLanguages: ";
            foreach (var l in Languages)
            {
                race += $"{l}, ";
            }
            race = race.Remove(race.Length - 2, 1) + "\nRacial Features: ";
            foreach (var rf in RacialFeatures)
            {
                race += $"{rf.Name}, ";
            }
            race = race.Remove(race.Length - 2, 1) + "\nAbility Changes: ";
            foreach (var ac in AbilityChanges)
            {
                race += $"{ac.Key}" + (ac.Value >= 0 ? $"+{ac.Value}" : $"-{ac.Value}")+", ";
            }
            race = race.Remove(race.Length - 2, 1) + "\n";

            return race;
        }
    }

    public class DnDClass
    {
        public string Name;
        public string Description;
        public int HitDieType;
        public List<string> PrimaryAbilities = new List<string>();
        public List<string> SaveProficiencies = new List<string>();
        public List<string> GearProficiencies = new List<string>();
        public Dictionary<string, int> Progression = new Dictionary<string, int>();
        public int numSkills;
        public List<string> ClassSkills = new List<string>();

        public override string ToString()
        {
            string outclass = "";

            return outclass;
        }
    }

    public class DnDFeature
    {
        public string Name;
        public string Description;

        public DnDFeature(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public override string ToString()
        {
            string feature=$"Feature: {Name}\n{Description}\n";
            return feature;
        }
    }

    public class DnDSkill
    {
        public string Name;
        public string Description;
        public string AbilityMod;
    }

    public class DnDCharacter
    {
        private static Random rand = new Random();

        public string Name = "";

        public int XP = 0;
        public bool isMale;
        public string Race = "";
        public string Background = "";
        public DnDClass Class = null;
        public int Level = 0;
        public int ProficiencyBonus = 0;
        public int AC = 0;
        public int Initiative = 0;
        public int Speed = 0;
        public string GearProficiencies = "";
        public List<DnDFeature> ClassFeatures = new List<DnDFeature>();
        public List<DnDFeature> RacialFeatures = new List<DnDFeature>();
        public List<DnDSkill> Skills = new List<DnDSkill>();
        public int HitDice = 0;

        public Dictionary<string, int> Attributes = new Dictionary<string, int>();
        public Dictionary<string, int> AttributeMods = new Dictionary<string, int>();
        public Dictionary<string, int> Saves = new Dictionary<string, int>();

        public DnDCharacter()
        {
            isMale = pickSex();
            Name = createName(isMale);

            var tempAttr = DnDCore.StartingScores.ToList();
            foreach (var attr in DnDCore.Attributes)
            {
                int t = rand.Next(0, tempAttr.Count);
                Attributes[attr] = tempAttr[t];
                tempAttr.RemoveAt(t);
                AttributeMods[attr] = AttrMod(Attributes[attr]);
            }
        }

        private bool pickSex()
        {
            return rand.NextDouble() < .5;
        }

        private int AttrMod(int attributeScore)
        {
            return (int)Math.Floor((double)(attributeScore - 10) / 2);
        }

        private string createName(bool isMale)
        {
            string name = "";
            if (isMale)
            {
                name = DnDCore.Names_Male[rand.Next(DnDCore.Names_Male.Count)];
            } else
            {
                name = DnDCore.Names_Female[rand.Next(DnDCore.Names_Female.Count)];
            }

            name += " " + DnDCore.Names_Last[rand.Next(DnDCore.Names_Last.Count)];
            return name;
        }

        public override string ToString()
        {
            string output = $"{Name}\nCharacter Attributes:\n";
            foreach (var kvp in Attributes)
            {
                output += $"{kvp.Key}  {kvp.Value},  mod {AttributeMods[kvp.Key]}\n";
            }
            return output;
        }
    }

}